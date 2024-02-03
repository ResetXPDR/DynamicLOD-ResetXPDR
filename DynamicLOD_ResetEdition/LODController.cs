﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
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
        private float tlod = 0;
        private float tlod_dec = 0;
        private float olod = 0;
        private float olod_dec = 0;
        private int cloudQ = 0;
        private int cloudQ_VR = 0;
        public bool FirstStart { get; set; } = true;
        private int fpsModeTicks = 0;
        private int fpsModeDelayTicks = 0;

        public LODController(ServiceModel model)
        {
            Model = model;

            SimConnect = IPCManager.SimConnect;
            SimConnect.SubscribeSimVar("VERTICAL SPEED", "feet per second");
            SimConnect.SubscribeSimVar("PLANE ALT ABOVE GROUND", "feet");
            SimConnect.SubscribeSimVar("PLANE ALT ABOVE GROUND MINUS CG", "feet");
            SimConnect.SubscribeSimVar("SIM ON GROUND", "Bool");
            tlod = Model.MemoryAccess.GetTLOD_PC();
            olod = Model.MemoryAccess.GetOLOD_PC();
            cloudQ = Model.MemoryAccess.GetCloudQ();
            cloudQ_VR = Model.MemoryAccess.GetCloudQ_VR();
            if (cloudQ > Model.DefaultCloudQ) Model.DefaultCloudQ = cloudQ;
            if (cloudQ_VR > Model.DefaultCloudQ_VR) Model.DefaultCloudQ_VR = cloudQ_VR;
            Model.CurrentPairTLOD = 0;
            Model.CurrentPairOLOD = 0;
            Model.fpsMode = false;
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

            tlod = Model.MemoryAccess.GetTLOD_PC();
            olod = Model.MemoryAccess.GetOLOD_PC();
            cloudQ = Model.MemoryAccess.GetCloudQ();
            cloudQ_VR = Model.MemoryAccess.GetCloudQ_VR();
        }


        // Bug fixes since 0.3.2
        // Rounding of LOD memory reads to fix precision error when comparing to desired LOD
        // Forced calling of FindPairs on settings changes to ensure correct LOD pair is selected
        // Disallow zero as an input value on most integer settings
        // Stop FPS Adaption minumum LOD acting when FPS Adaption wasn't enabled
        // Use ResetFPSMode at the end of FindPairs in place of essentially duplicate code

        // New in 0.3.6
        // Separate LOD minimums in FPS Adaption or at the very least disabled for OLOD so that system wide setting is used instead
        // Remove Reduce on pairs/indices setting for FPS adaption because no one uses it?
        // Decrease cloud quality option for FPS Adaption with user definable null zone for FPS Adaption cancellation for cloud change

        // To fix/improve
        // Reduce/Remove LOD step log entries
        // Save starting TLOD and OLOD values so that they can automatically be restored upon app exit and remove setting from UI
        // Auto clean of no longer used entries in config file following update, using version stored in config file?
        // Averaging of FPS values over 5 seconds so that false triggering of FPS adapation doesn't occur


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

            if (Model.UseTargetFPS)
            {
                if (SimConnect.GetAverageFPS() < Model.TargetFPS)
                {
                    if (!Model.fpsMode)
                    {
                        if (fpsModeDelayTicks >= Model.ConstraintDelayTicks)
                        {
                            Logger.Log(LogLevel.Information, "LODController:RunTick", $"FPS Constraint active");
                            Model.fpsMode = true;
                            if (Model.DecCloudQ && Model.DefaultCloudQ >= 1)
                            {
                                Model.MemoryAccess.SetCloudQ(Model.DefaultCloudQ - 1);
                            }
                            if (Model.DecCloudQ && Model.DefaultCloudQ_VR >= 1)
                            {
                                Model.MemoryAccess.SetCloudQ_VR(Model.DefaultCloudQ_VR - 1);
                            }
                            tlod_dec = Model.DecreaseTLOD;
                            olod_dec = Model.DecreaseOLOD;
                            fpsModeDelayTicks = 0;
                        }
                        else fpsModeDelayTicks++;                    }
                }
                else if (SimConnect.GetAverageFPS() > Model.TargetFPS + (Model.DecCloudQ ? Model.CloudRecoveryFPS : 0) && Model.fpsMode) 
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
            if (tlod != newlod)
            {
                if (evalResult > 0) Logger.Log(LogLevel.Information, "LODController:RunTick", $"Setting TLOD {newlod}" + (Model.LodStepMax ? " in steps" : ""));
                if (!Model.ForceEvaluation)
                {
                    if (tlod > newlod && tlod - Model.LodStepMaxDec > newlod) newlod = tlod - Model.LodStepMaxDec;
                    else if (tlod + Model.LodStepMaxInc < newlod) newlod = tlod + Model.LodStepMaxInc;
                }
                Model.MemoryAccess.SetTLOD(newlod);
            }

            evalResult = EvaluateLodPairByHeight(ref Model.CurrentPairOLOD, Model.PairsOLOD[Model.SelectedProfile]);
            newlod = EvaluateLodValue(Model.PairsOLOD[Model.SelectedProfile], Model.CurrentPairOLOD, olod_dec, false);
            if (olod != newlod)
            {
                if (evalResult > 0) Logger.Log(LogLevel.Information, "LODController:RunTick", $"Setting OLOD {newlod}" + (Model.LodStepMax ? " in steps" : ""));
                if (!Model.ForceEvaluation)
                {
                    if (olod > newlod && olod - Model.LodStepMaxDec > newlod) newlod = olod - Model.LodStepMaxDec;
                    else if (olod + Model.LodStepMaxInc < newlod) newlod = olod + Model.LodStepMaxInc;
                }
                Model.MemoryAccess.SetOLOD(newlod);
            }
            
            Model.ForceEvaluation = false;
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
                Model.MemoryAccess.SetCloudQ(Model.DefaultCloudQ);
                Model.MemoryAccess.SetCloudQ_VR(Model.DefaultCloudQ_VR);
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
            Logger.Log(LogLevel.Information, "LODController:FindPairs", $"Finding Pairs (onGround: {Model.OnGround} | tlod: {tlod} | olod: {olod})");

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
                if (Model.ForceEvaluation || tlod != Model.PairsTLOD[Model.SelectedProfile][result].Item2)
                {
                    Logger.Log(LogLevel.Information, "LODController:FindPairs", $"Setting TLOD {Model.PairsTLOD[Model.SelectedProfile][result].Item2}");
                    Model.MemoryAccess.SetTLOD(Model.PairsTLOD[Model.SelectedProfile][result].Item2);
                }

                result = 0;
                for (int i = 0; i < Model.PairsOLOD[Model.SelectedProfile].Count; i++)
                {
                    if (altAboveGnd > Model.PairsOLOD[Model.SelectedProfile][i].Item1)
                        result = i;
                }
                Model.CurrentPairOLOD = result;
                Logger.Log(LogLevel.Information, "LODController:FindPairs", $"OLOD Index {result}");
                if (Model.ForceEvaluation || olod != Model.PairsOLOD[Model.SelectedProfile][result].Item2)
                {
                    Logger.Log(LogLevel.Information, "LODController:FindPairs", $"Setting OLOD {Model.PairsOLOD[Model.SelectedProfile][result].Item2}");
                    Model.MemoryAccess.SetOLOD(Model.PairsOLOD[Model.SelectedProfile][result].Item2);
                }
            }
            else
            {
                int result = 0;
                Model.CurrentPairTLOD = result;
                if (Model.ForceEvaluation || tlod != Model.PairsTLOD[Model.SelectedProfile][result].Item2)
                {
                    Logger.Log(LogLevel.Information, "LODController:FindPairs", $"Setting TLOD {Model.PairsTLOD[Model.SelectedProfile][result].Item2}");
                    Model.MemoryAccess.SetTLOD(Model.PairsTLOD[Model.SelectedProfile][result].Item2);
                }
                Model.CurrentPairOLOD = result;
                if (Model.ForceEvaluation || olod != Model.PairsOLOD[Model.SelectedProfile][result].Item2)
                {
                    Logger.Log(LogLevel.Information, "LODController:FindPairs", $"Setting OLOD {Model.PairsOLOD[Model.SelectedProfile][result].Item2}");
                    Model.MemoryAccess.SetOLOD(Model.PairsOLOD[Model.SelectedProfile][result].Item2);
                }
            }

            ResetFPSMode(false);
            FirstStart = false;
        }
    }
}