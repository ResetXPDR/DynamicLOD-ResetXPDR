using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicLOD_ResetEdition
{
    public class LODController
    {
        private MobiSimConnect SimConnect;
        private ServiceModel Model;

        private int[] verticalStats = new int[5];
        private float[] verticalStatsVS = new float[5];
        private int verticalIndex = 0;
        private int altAboveGnd = 0;
        private float tlod_dec = 0;
        private float olod_dec = 0;
        public bool FirstStart { get; set; } = true;
        private int fpsModeTicks = 0;
        private int fpsModeDelayTicks = 0;
        private int VRStateCounter = 5;

        public LODController(ServiceModel model)
        {
            Model = model;

            SimConnect = IPCManager.SimConnect;
            SimConnect.SubscribeSimVar("VERTICAL SPEED", "feet per second");
            SimConnect.SubscribeSimVar("PLANE ALT ABOVE GROUND", "feet");
            SimConnect.SubscribeSimVar("PLANE ALT ABOVE GROUND MINUS CG", "feet");
            SimConnect.SubscribeSimVar("SIM ON GROUND", "Bool");
            GetMSFSState();
            Model.CurrentPairTLOD = 0;
            Model.CurrentPairOLOD = 0;
            Model.fpsMode = false;
            Model.tlod_step = false;
            Model.olod_step = false;
        }

          private void UpdateVariables()
        {
            float vs = SimConnect.ReadSimVar("VERTICAL SPEED", "feet per second");
            Model.OnGround = SimConnect.ReadSimVar("SIM ON GROUND", "Bool") == 1.0f;
            verticalStatsVS[verticalIndex] = vs;
            if (vs >= 8.0f)
                verticalStats[verticalIndex] = 1;
            else if (vs <= -8.0f)
                verticalStats[verticalIndex] = -1;
            else
                verticalStats[verticalIndex] = 0;

            verticalIndex++;
            if (verticalIndex >= verticalStats.Length || verticalIndex >= verticalStatsVS.Length)
                verticalIndex = 0;

            Model.VerticalTrend = VerticalAverage();

            altAboveGnd = (int)SimConnect.ReadSimVar("PLANE ALT ABOVE GROUND", "feet");
            if (altAboveGnd == 0 && !Model.OnGround)
                altAboveGnd = (int)SimConnect.ReadSimVar("PLANE ALT ABOVE GROUND MINUS CG", "feet");

            GetMSFSState();
        }

        public void RunTick()
        {
            UpdateVariables();
            if (FirstStart)
            {
                fpsModeTicks++;
                if (fpsModeTicks > 2)
                    FindPairs();
                    Model.ForceEvaluation = false;
                return;
            }

            if (Model.ForceEvaluation) FindPairs();

            if ((Model.ActiveWindowMSFS || !Model.PauseMSFSFocusLost) && Model.FPSSettleCounter == 0)
            {
                if (Model.UseTargetFPS)
                {
                    if (GetAverageFPS() < Model.TargetFPS)
                    {
                        if (!Model.fpsMode)
                        {
                            if (fpsModeDelayTicks >= Model.ConstraintDelayTicks)
                            {
                                Logger.Log(LogLevel.Information, "LODController:RunTick", $"FPS Constraint active");
                                Model.fpsMode = true;
                                if (Model.DecCloudQ && Model.DefaultCloudQ >= 1)
                                {
                                    Model.MemoryAccess.SetCloudQ(Model.cloudQ = (Model.DefaultCloudQ - 1));
                                }
                                if (Model.DecCloudQ && Model.DefaultCloudQ_VR >= 1)
                                {
                                    Model.MemoryAccess.SetCloudQ_VR(Model.cloudQ_VR = (Model.DefaultCloudQ_VR - 1));
                                }
                                tlod_dec = Model.DecreaseTLOD;
                                olod_dec = Model.DecreaseOLOD;
                                fpsModeDelayTicks = 0;
                            }
                            else fpsModeDelayTicks++;
                        }
                    }
                    else if (GetAverageFPS() > Model.TargetFPS + (Model.DecCloudQ ? Model.CloudRecoveryFPS : 0) && Model.fpsMode)
                    {
                        fpsModeTicks++;
                        if (fpsModeTicks > Model.ConstraintTicks || Model.ForceEvaluation)
                            ResetFPSMode();
                    }
                    else fpsModeDelayTicks = 0;
                }
                else if (!Model.UseTargetFPS && Model.fpsMode)
                    ResetFPSMode();

                float evalResult = EvaluateLodPairByHeight(ref Model.CurrentPairTLOD, Model.PairsTLOD[Model.SelectedProfile]);
                if (VerticalAverage() > 0 && Model.CurrentPairTLOD + 1 < Model.PairsTLOD[Model.SelectedProfile].Count)
                    Model.AltLead = Math.Abs(Model.PairsTLOD[Model.SelectedProfile][Model.CurrentPairTLOD + 1].Item2 - Model.PairsTLOD[Model.SelectedProfile][Model.CurrentPairTLOD].Item2) / Model.LodStepMaxInc * VSAverage();
                else if (VerticalAverage() < 0 && Model.CurrentPairTLOD - 1 >= 0)
                    Model.AltLead = Math.Abs(Model.PairsTLOD[Model.SelectedProfile][Model.CurrentPairTLOD].Item2 - Model.PairsTLOD[Model.SelectedProfile][Model.CurrentPairTLOD - 1].Item2) / Model.LodStepMaxDec * VSAverage();
                else Model.AltLead = 0;
                float newlod = EvaluateLodValue(Model.PairsTLOD[Model.SelectedProfile], Model.CurrentPairTLOD, tlod_dec, true);
                if (Model.tlod != newlod)
                {
                    if (evalResult > 0) Logger.Log(LogLevel.Information, "LODController:RunTick", $"Setting TLOD {newlod}" + (Model.LodStepMax ? " in steps" : ""));
                    if (!Model.ForceEvaluation)
                    {
                        Model.tlod_step = true;
                        if (Model.tlod > newlod)
                        {
                            if (Model.tlod - Model.LodStepMaxDec > newlod) newlod = Model.tlod - Model.LodStepMaxDec;
                            else Model.tlod_step = false;
                        }
                        else
                        {
                            if (Model.tlod + Model.LodStepMaxInc < newlod) newlod = Model.tlod + Model.LodStepMaxInc;
                            else Model.tlod_step = false;
                        }
                    }
                    Model.MemoryAccess.SetTLOD(Model.tlod = newlod);
                }
                else Model.tlod_step = false;

                evalResult = EvaluateLodPairByHeight(ref Model.CurrentPairOLOD, Model.PairsOLOD[Model.SelectedProfile]);
                newlod = EvaluateLodValue(Model.PairsOLOD[Model.SelectedProfile], Model.CurrentPairOLOD, olod_dec, false);
                if (Model.olod != newlod)
                {
                    if (evalResult > 0) Logger.Log(LogLevel.Information, "LODController:RunTick", $"Setting OLOD {newlod}" + (Model.LodStepMax ? " in steps" : ""));
                    if (!Model.ForceEvaluation)
                    {
                        Model.olod_step = true;
                        if (Model.olod > newlod)
                        {
                            if (Model.olod - Model.LodStepMaxDec > newlod) newlod = Model.olod - Model.LodStepMaxDec;
                            else Model.olod_step = false;
                        }
                        else
                        {
                            if (Model.olod + Model.LodStepMaxInc < newlod) newlod = Model.olod + Model.LodStepMaxInc;
                            else Model.olod_step = false;
                        }
                    }
                    Model.MemoryAccess.SetOLOD(Model.olod = newlod);
                }
                else Model.olod_step = false;

                Model.ForceEvaluation = false;
            }
            else if (--Model.FPSSettleCounter < 0) Model.FPSSettleCounter = 0;

        }

        private float EvaluateLodValue(List<(float, float)> pairs, int currentPair, float decrement, bool tlod)
        {
            if (Model.fpsMode)
                return Math.Max(pairs[currentPair].Item2 - decrement, Math.Max((tlod ? Model.MinTLOD : Model.MinOLOD), Model.SimMinLOD));
            else
                return Math.Max(pairs[currentPair].Item2, Model.SimMinLOD);
        }

        private void ResetFPSMode(bool logEntry = true)
        {
            if (logEntry) Logger.Log(LogLevel.Information, "LODController:RunTick", $"FPS Constraint lifted");
            Model.fpsMode = false;
            fpsModeTicks = 0;
            fpsModeDelayTicks = 0;
            tlod_dec = 0;
            olod_dec = 0;
            if (Model.DecCloudQ || Model.ForceEvaluation)
            {
                Model.MemoryAccess.SetCloudQ(Model.cloudQ = Model.DefaultCloudQ);
                Model.MemoryAccess.SetCloudQ_VR(Model.cloudQ_VR = Model.DefaultCloudQ_VR);
            }
        }

        private float EvaluateLodPairByHeight(ref int index, List<(float, float)> lodPairs)
        {
            float result = -1.0f;
            Logger.Log(LogLevel.Verbose, "LODController:EvaluateLodByHeight", $"VerticalAverage {VerticalAverage()}");
            if ((VerticalAverage() > 0 || Model.ForceEvaluation) && index + 1 < lodPairs.Count && altAboveGnd + Math.Abs(lodPairs[index + 1].Item2 - lodPairs[index].Item2) / Model.LodStepMaxInc * VSAverage() > lodPairs[index + 1].Item1)
            {
                index++;
                Logger.Log(LogLevel.Information, "LODController:EvaluateLodByHeight", $"Higher Pair found (altAboveGnd: {altAboveGnd} | index: {index} | lod: {lodPairs[index].Item2})");
                return lodPairs[index].Item2;
            }
            else if ((VerticalAverage() < 0 || Model.ForceEvaluation) && index - 1 >= 0 && altAboveGnd + Math.Abs(lodPairs[index].Item2 - lodPairs[index - 1].Item2) / Model.LodStepMaxDec * VSAverage() < lodPairs[index].Item1)
            {
                index--;
                Logger.Log(LogLevel.Information, "LODController:EvaluateLodByHeight", $"Lower Pair found (altAboveGnd: {altAboveGnd} | index: {index} | lod: {lodPairs[index].Item2})");
                return lodPairs[index].Item2;
            }
            else if (Model.CruiseLODUpdates && VerticalAverage() == 0 && index + 1 < lodPairs.Count && altAboveGnd * 0.95 > lodPairs[index + 1].Item1)
            {
                index++;
                Logger.Log(LogLevel.Information, "LODController:EvaluateLodByHeight", $"Higher Pair found (altAboveGnd: {altAboveGnd} | index: {index} | lod: {lodPairs[index].Item2})");
                return lodPairs[index].Item2;
            }
            else if (Model.CruiseLODUpdates && VerticalAverage() == 0 && altAboveGnd * 1.05 < lodPairs[index].Item1 && index - 1 >= 0)
            {
                index--;
                Logger.Log(LogLevel.Information, "LODController:EvaluateLodByHeight", $"Lower Pair found (altAboveGnd: {altAboveGnd} | index: {index} | lod: {lodPairs[index].Item2})");
                return lodPairs[index].Item2;
            }

            return result;
        }

        public int VerticalAverage()
        {
            return verticalStats.Sum();
        }
        public float VSAverage()
        {
            return verticalStatsVS.Average();
        }

        private void FindPairs()
        {
            Logger.Log(LogLevel.Information, "LODController:FindPairs", $"Finding Pairs (onGround: {Model.OnGround} | tlod: {Model.tlod} | olod: {Model.olod})");

            if (!Model.OnGround)
            {
                int result = 0;
                for (int i = 0; i < Model.PairsTLOD[Model.SelectedProfile].Count; i++)
                {
                    if (altAboveGnd > Model.PairsTLOD[Model.SelectedProfile][i].Item1)
                        result = i;
                }
                Model.CurrentPairTLOD = result;
                Logger.Log(LogLevel.Information, "LODController:FindPairs", $"TLOD Index {result}");
                if (Model.ForceEvaluation || Model.tlod != Model.PairsTLOD[Model.SelectedProfile][result].Item2)
                {
                    Logger.Log(LogLevel.Information, "LODController:FindPairs", $"Setting TLOD {Model.PairsTLOD[Model.SelectedProfile][result].Item2}");
                    Model.MemoryAccess.SetTLOD(Model.tlod = Model.PairsTLOD[Model.SelectedProfile][result].Item2);
                }

                result = 0;
                for (int i = 0; i < Model.PairsOLOD[Model.SelectedProfile].Count; i++)
                {
                    if (altAboveGnd > Model.PairsOLOD[Model.SelectedProfile][i].Item1)
                        result = i;
                }
                Model.CurrentPairOLOD = result;
                Logger.Log(LogLevel.Information, "LODController:FindPairs", $"OLOD Index {result}");
                if (Model.ForceEvaluation || Model.olod != Model.PairsOLOD[Model.SelectedProfile][result].Item2)
                {
                    Logger.Log(LogLevel.Information, "LODController:FindPairs", $"Setting OLOD {Model.PairsOLOD[Model.SelectedProfile][result].Item2}");
                    Model.MemoryAccess.SetOLOD(Model.olod = Model.PairsOLOD[Model.SelectedProfile][result].Item2);
                }
            }
            else
            {
                int result = 0;
                Model.CurrentPairTLOD = result;
                if (Model.ForceEvaluation || Model.tlod != Model.PairsTLOD[Model.SelectedProfile][result].Item2)
                {
                    Logger.Log(LogLevel.Information, "LODController:FindPairs", $"Setting TLOD {Model.PairsTLOD[Model.SelectedProfile][result].Item2}");
                    Model.MemoryAccess.SetTLOD(Model.tlod = Model.PairsTLOD[Model.SelectedProfile][result].Item2);
                }
                Model.CurrentPairOLOD = result;
                if (Model.ForceEvaluation || Model.olod != Model.PairsOLOD[Model.SelectedProfile][result].Item2)
                {
                    Logger.Log(LogLevel.Information, "LODController:FindPairs", $"Setting OLOD {Model.PairsOLOD[Model.SelectedProfile][result].Item2}");
                    Model.MemoryAccess.SetOLOD(Model.olod = Model.PairsOLOD[Model.SelectedProfile][result].Item2);
                }
            }

            ResetFPSMode(false);
            FirstStart = false;
        }
        public float GetAverageFPS()
        {
            if (Model.VrModeActive) return (float)Math.Round(IPCManager.SimConnect.GetAverageFPS());
            else if (Model.LsModeActive) return (float)Math.Round(IPCManager.SimConnect.GetAverageFPS() * Model.LsModeMultiplier);
            else if (Model.FgModeActive) return (float)Math.Round(IPCManager.SimConnect.GetAverageFPS() * 2.0f);
            else return (float)Math.Round(IPCManager.SimConnect.GetAverageFPS());
        }
        private void GetMSFSState()
        {
            if (--VRStateCounter <= 0)
            {
                Model.VrModeActive = Model.MemoryAccess.IsVrModeActive();
                VRStateCounter = 5;
            }
            if (Model.ActiveWindowMSFS != Model.MemoryAccess.IsActiveWindowMSFS() && Model.PauseMSFSFocusLost) Model.FPSSettleCounter = ServiceModel.FPSSettleSeconds;
            Model.ActiveWindowMSFS = Model.MemoryAccess.IsActiveWindowMSFS();
            string ActiveGraphicsMode = Model.ActiveGraphicsMode;
            if (Model.VrModeActive) Model.ActiveGraphicsMode = "VR";
            else if (Model.LsModeActive) Model.ActiveGraphicsMode = "LSFG";
            else if (Model.FgModeActive) Model.ActiveGraphicsMode = "FG";
            else Model.ActiveGraphicsMode = "PC";
            if (Model.ActiveGraphicsMode != ActiveGraphicsMode)
            {
                Model.FPSSettleCounter = ServiceModel.FPSSettleSeconds;
                Model.ActiveGraphicsModeChanged = true;
            }
        }

    }
}
