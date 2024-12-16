using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;

namespace DynamicLOD_ResetEdition
{
    public class ServiceModel
    {
        public static readonly int maxProfile = 6;
        private static readonly int BuildConfigVersion = 1;
        public int ConfigVersion { get; set; }
        public bool ServiceExited { get; set; } = false;
        public bool CancellationRequested { get; set; } = false;
        public bool AppEnabled { get; set; } = true;

        public bool IsSimRunning { get; set; } = false;
        public bool IsSessionRunning { get; set; } = false;

        public MemoryManager MemoryAccess { get; set; } = null;
        public int VerticalTrend { get; set; }
        public float AltLead { get; set; }
        public bool OnGround { get; set; } = true;
        public bool ForceEvaluation { get; set; } = false;
        public bool ReloadAppWindowSettings { get; set; } = false;

        public int SelectedProfile { get; set; } = 0;
        public string ProfileName1 { get; set; }
        public string ProfileName2 { get; set; }
        public string ProfileName3 { get; set; }
        public string ProfileName4 { get; set; }
        public string ProfileName5 { get; set; }
        public string ProfileName6 { get; set; }
        public List<List<(float, float)>> PairsTLOD { get; set; }
        public int CurrentPairTLOD;
        public List<List<(float, float)>> PairsOLOD { get; set; }
        public int CurrentPairOLOD;
        public bool fpsMode { get; set; }
        public bool UseTargetFPS { get; set; }
        public int TargetFPS { get; set; }
        public int TargetFPS_PC { get; set; }
        public int TargetFPS_VR { get; set; }
        public int TargetFPS_LS { get; set; }
        public int TargetFPS_FG { get; set; }
        public int CloudRecoveryFPS { get; set; }
        public int ConstraintTicks { get; set; }
        public int ConstraintDelayTicks { get; set; }
        public float DecreaseTLOD { get; set; }
        public float DecreaseOLOD { get; set; }
        public float MinTLOD { get; set; }
        public float MinOLOD { get; set; }
        public float SimMinLOD { get; set; }
        public float DefaultTLOD { get; set; } = 100;
        public float DefaultTLOD_VR { get; set; } = 100;
        public float DefaultOLOD { get; set; } = 100;
        public float DefaultOLOD_VR { get; set; } = 100;
        public int DefaultCloudQ { get; set; } = 2;
        public int DefaultCloudQ_VR { get; set; } = 2;
        public bool DefaultDynSet { get; set; } = false;
        public bool DefaultDynSetVR { get; set; } = false;
        public bool DefaultSettingsRead { get; set; } = false;
        public int LodStepMaxInc { get; set; }
        public int LodStepMaxDec { get; set; }
        public bool tlod_step { get; set; } = false;
        public bool olod_step { get; set; } = false;


        public string LogLevel { get; set; }
        public static int MfLvarsPerFrame { get; set; }
        public bool WaitForConnect { get; set; }
        public bool OpenWindow { get; set; }
        public int windowTop { get; set; }
        public int windowLeft { get; set; }
        public bool windowIsVisible { get; set; }
        public bool RememberWindowPos { get; set; }

        private bool resetWindowPosition;
        public bool OnTop { get; set; }

        public bool PauseMSFSFocusLost { get; set; } = false;
        public bool VrModeActive { get; set; }
        public bool FgModeActive { get; set; }
        public bool LsModeActive { get; set; }
        public int LsModeMultiplier { get; set; }

        public bool ActiveWindowMSFS { get; set; }
        public string ActiveGraphicsMode { get; set; } = "PC";
        public bool ActiveGraphicsModeChanged { get; set; } = false;

        public const int FPSSettleSeconds = 6;
        public int FPSSettleCounter { get; set; } = FPSSettleSeconds;
        public float tlod { get; set; } = 0;
        public float olod { get; set; } = 0;
        public int cloudQ { get; set; }
        public int cloudQ_VR { get; set; }
        public bool CruiseLODUpdates { get; set; }
        public bool DecCloudQ { get; set; }
        public bool LodStepMax { get; set; }
        public string SimBinary { get; set; }
        public string SimModule { get; set; }
        public long OffsetModuleBase { get; set; }
        public long OffsetPointerMain { get; set; }
        public long OffsetPointerTlod { get; set; }
        public long OffsetPointerTlodVr { get; set; }
        public long OffsetPointerOlod { get; set; }
        public long OffsetPointerOlodVr { get; set; }
        public long OffsetPointerCloudQ { get; set; }
        public long OffsetPointerCloudQVr { get; set; }
        public long OffsetPointerVrMode { get; set; }
        public long OffsetPointerFgMode { get; set; }
        public long OffsetPointerAnsio { get; set; }
        public long OffsetPointerDynSet { get; set; }
        public long OffsetPointerDynSetVr { get; set; }
        public long OffsetPointerCubeMap { get; set; }
        public long OffsetPointerWaterWaves { get; set; }
        public bool OffsetSearchingActive { get; set; }

        public const bool TestVersion = false;
        public const string TestVariant = "-test1";
        public bool isMSFS2024 = false;
        public bool isMSFS2024_last = false;


        public ConfigurationFile ConfigurationFile = new();

        public ServiceModel()
        {
            if (!TestVersion && File.GetLastWriteTime(App.ConfigFile) > DateTime.Now.AddSeconds(-10)) resetWindowPosition = true;
            CurrentPairTLOD = 0;
            CurrentPairOLOD = 0;
            if ((File.Exists(App.msConfigStore) || File.Exists(App.msConfigSteam) && !(File.Exists(App.msConfigStore2024) || File.Exists(App.msConfigSteam2024))))
            {
                isMSFS2024 = isMSFS2024_last = false;
            }
            else if (!(File.Exists(App.msConfigStore) || File.Exists(App.msConfigSteam) && (File.Exists(App.msConfigStore2024) || File.Exists(App.msConfigSteam2024))))
            {
                isMSFS2024 = isMSFS2024_last = true;
            }
            else if (new FileInfo(App.ConfigFile2024).LastWriteTime > new FileInfo(App.ConfigFile).LastWriteTime)
            {
                isMSFS2024 = isMSFS2024_last = true;
            }
            LoadConfiguration();
        }

        public bool LoadConfiguration()
        {
            bool ConfigFileChanged = false;
            if (IsSimRunning) isMSFS2024 = isMSFS2024_last = IPCManager.IsSim2024();
            ConfigFileChanged = ConfigurationFile.LoadConfiguration(isMSFS2024);

            LogLevel = Convert.ToString(ConfigurationFile.GetSetting("logLevel", "Debug"));
            MfLvarsPerFrame = Convert.ToInt32(ConfigurationFile.GetSetting("mfLvarPerFrame", "15"));
            ConfigVersion = Convert.ToInt32(ConfigurationFile.GetSetting("ConfigVersion", "1"));
            WaitForConnect = Convert.ToBoolean(ConfigurationFile.GetSetting("waitForConnect", "true"));
            OpenWindow = Convert.ToBoolean(ConfigurationFile.GetSetting("openWindow", "true"));
            RememberWindowPos = Convert.ToBoolean(ConfigurationFile.GetSetting("RememberWindowPos", "true"));
            if (resetWindowPosition)
            {
                SetSetting("windowTop", "50", true);
                SetSetting("windowLeft", "50", true);
                resetWindowPosition = false;
            }
            windowTop = Convert.ToInt32(ConfigurationFile.GetSetting("windowTop", "50"));
            windowLeft = Convert.ToInt32(ConfigurationFile.GetSetting("windowLeft", "50"));
            windowIsVisible = Convert.ToBoolean(ConfigurationFile.GetSetting("windowIsVisible", "true"));
            OnTop = Convert.ToBoolean(ConfigurationFile.GetSetting("OnTop", "false"));
            PauseMSFSFocusLost = Convert.ToBoolean(ConfigurationFile.GetSetting("PauseMSFSFocusLost", "false"));
            CruiseLODUpdates = Convert.ToBoolean(ConfigurationFile.GetSetting("CruiseLODUpdates", "false"));
            DecCloudQ = Convert.ToBoolean(ConfigurationFile.GetSetting("DecCloudQ", "false"));
            SimBinary = Convert.ToString(ConfigurationFile.GetSetting("simBinary", isMSFS2024 ? "FlightSimulator2024" : "FlightSimulator"));
            SimModule = Convert.ToString(ConfigurationFile.GetSetting("simModule", "WwiseLibPCx64P.dll"));
            UseTargetFPS = Convert.ToBoolean(ConfigurationFile.GetSetting("useTargetFps", "true"));
            TargetFPS_PC = Convert.ToInt32(ConfigurationFile.GetSetting("targetFpsPC", "30"));
            TargetFPS_VR = Convert.ToInt32(ConfigurationFile.GetSetting("targetFpsVR", "30"));
            TargetFPS_LS = Convert.ToInt32(ConfigurationFile.GetSetting("targetFpsLS", "30"));
            TargetFPS_FG = Convert.ToInt32(ConfigurationFile.GetSetting("targetFpsFG", "30"));
            if (ActiveGraphicsMode == "VR") TargetFPS = TargetFPS_VR;
            else if (ActiveGraphicsMode == "LSFG") TargetFPS = TargetFPS_LS;
            else if (ActiveGraphicsMode == "FG") TargetFPS = TargetFPS_FG;
            else TargetFPS = TargetFPS_PC;
            CloudRecoveryFPS = Convert.ToInt32(ConfigurationFile.GetSetting("CloudRecoveryFPS", "0"));
            ConstraintTicks = Convert.ToInt32(ConfigurationFile.GetSetting("constraintTicks", "60"));
            ConstraintDelayTicks = Convert.ToInt32(ConfigurationFile.GetSetting("constraintDelayTicks", "1"));
            DecreaseTLOD = Convert.ToSingle(ConfigurationFile.GetSetting("decreaseTlod", "50"), new RealInvariantFormat(ConfigurationFile.GetSetting("decreaseTlod", "50")));
            DecreaseOLOD = Convert.ToSingle(ConfigurationFile.GetSetting("decreaseOlod", "50"), new RealInvariantFormat(ConfigurationFile.GetSetting("decreaseOlod", "50")));
            MinTLOD = Convert.ToSingle(ConfigurationFile.GetSetting("minTLod", "100"), new RealInvariantFormat(ConfigurationFile.GetSetting("minTLod", "100")));
            MinOLOD = Convert.ToSingle(ConfigurationFile.GetSetting("minOLod", "100"), new RealInvariantFormat(ConfigurationFile.GetSetting("minOLod", "100")));
            LodStepMaxInc = Convert.ToInt32(ConfigurationFile.GetSetting("LodStepMaxInc", "5"));
            LodStepMaxDec = Convert.ToInt32(ConfigurationFile.GetSetting("LodStepMaxDec", "5"));
            OffsetModuleBase = Convert.ToInt64(ConfigurationFile.GetSetting("offsetModuleBase", isMSFS2024 ? (File.Exists(App.msConfigSteam2024) ? "0x0A241C60" : "0x09F0DC40") : "0x004B2368"), 16);
            OffsetPointerTlod = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerTlod", isMSFS2024 ? "0x358" : "0xC"), 16);
            OffsetPointerTlodVr = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerTlodVr", isMSFS2024 ? "0x480" : "0x114"), 16);
            OffsetPointerOlod = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerOlod", isMSFS2024 ? "0x36C" : "0xC"), 16);
            OffsetPointerCloudQ = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerCloudQ", isMSFS2024 ? "0x3A4" : "0x44"), 16);
            OffsetPointerCloudQVr = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerCloudQVr", isMSFS2024 ? "0x4CC" : "0x108"), 16);
            OffsetPointerVrMode = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerVrMode", isMSFS2024 ? "0x334" : "0x1C"), 16);
            OffsetPointerFgMode = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerFgMode", isMSFS2024 ? "0x304" : "0x4A"), 16);
            OffsetPointerAnsio = -Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerAnsio", (isMSFS2024 ? "0x20" : "0x18")), 16);
            if (isMSFS2024)
            {
                OffsetPointerOlodVr = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerOlodVR", "0x494"), 16);
                OffsetPointerDynSet = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerDynSet", "0x328"), 16);
                OffsetPointerDynSetVr = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerDynSetVR", "0x329"), 16);
                OffsetPointerCubeMap = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerCubeMap", "0x6C"), 16);
            }
            else
            {
                OffsetPointerMain = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerMain", "0x3D0"), 16);
                OffsetPointerWaterWaves = Convert.ToInt64(ConfigurationFile.GetSetting("offsetPointerWaterWaves", "0x3C"), 16);
            }
            SimMinLOD = Convert.ToSingle(ConfigurationFile.GetSetting("simMinLod", "10"), new RealInvariantFormat(ConfigurationFile.GetSetting("simMinLod", "10")));
            if (Boolean.TryParse(ConfigurationFile.GetSetting("LodStepMax", "false"), out bool flag)) LodStepMax = Convert.ToBoolean(ConfigurationFile.GetSetting("LodStepMax", "false"));
            else LodStepMax = false;
            if (!LodStepMax) 
            {
                LodStepMaxInc = 1000;
                LodStepMaxDec = 1000;
            }
 
            SelectedProfile = Convert.ToInt32(ConfigurationFile.GetSetting("selectedProfile", "0"));
            ProfileName1 = Convert.ToString(ConfigurationFile.GetSetting("profileName1", "1"));
            ProfileName2 = Convert.ToString(ConfigurationFile.GetSetting("profileName2", "2"));
            ProfileName3 = Convert.ToString(ConfigurationFile.GetSetting("profileName3", "3"));
            ProfileName4 = Convert.ToString(ConfigurationFile.GetSetting("profileName4", "4"));
            ProfileName5 = Convert.ToString(ConfigurationFile.GetSetting("profileName5", "5"));
            ProfileName6 = Convert.ToString(ConfigurationFile.GetSetting("profileName6", "6"));
            PairsTLOD = new();
            PairsOLOD = new();

            for (int i = 0; i < maxProfile; i++)
            {
                PairsTLOD.Add(LoadPairs(ConfigurationFile.GetSetting($"tlodPairs{i}", "0:100|1500:150|5000:200")));
                PairsOLOD.Add(LoadPairs(ConfigurationFile.GetSetting($"olodPairs{i}", "0:100|2500:50|7500:10")));
            }
            CurrentPairTLOD = 0;
            CurrentPairOLOD = 0;
            ForceEvaluation = true;


            if (ConfigVersion < BuildConfigVersion)
            {
                //CHANGE SETTINGS IF NEEDED, Example:

                SetSetting("ConfigVersion", Convert.ToString(BuildConfigVersion));
            }
            return ConfigFileChanged;
        }

        public float GetGPUUsage()
        {
            try
            {
                var gpuCounters = GPUCounters();
                var gpuUsage = GPUUsage(gpuCounters);
                return gpuUsage;
            }
            catch { }
            return 0;

        }

        public static List<PerformanceCounter> GPUCounters()
        {
            var category = new PerformanceCounterCategory("GPU Engine");
            var counterNames = category.GetInstanceNames();

            var gpuCounters = counterNames
                                .Where(counterName => counterName.EndsWith("engtype_3D"))
                                .SelectMany(counterName => category.GetCounters(counterName))
                                .Where(counter => counter.CounterName.Equals("Utilization Percentage"))
                                .ToList();

            return gpuCounters;
        }

        public static float GPUUsage(List<PerformanceCounter> gpuCounters)
        {
            gpuCounters.ForEach(x => x.NextValue());

            var result = gpuCounters.Sum(x => x.NextValue());

            return result;
        }

        public static List<(float, float)> LoadPairs(string settings)
        {
            List<(float, float)> pairsList = new();

            string[] strPairs = settings.Split('|');
            int alt;
            float lod;
            foreach (string pair in strPairs)
            {
                string[] parts = pair.Split(':');
                alt = Convert.ToInt32(parts[0]);
                lod = Convert.ToSingle(parts[1], new RealInvariantFormat(parts[1]));
                pairsList.Add((alt, lod));
            }
            SortTupleList(pairsList);

            return pairsList;
        }

        public static void SortTupleList(List<(float, float)> pairsList)
        {
            pairsList.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        }

        public static string CreateLodString(List<(float, float)> pairsList)
        {
            string result = "";
            bool first = true;

            foreach (var pair in pairsList)
            {
                if (first)
                    first = false;
                else
                    result += "|";

                result += $"{Convert.ToString((int)pair.Item1)}:{Convert.ToString(pair.Item2, CultureInfo.InvariantCulture)}";
            }

            return result;
        }

        public string GetSetting(string key, string defaultValue = "")
        {
            return ConfigurationFile[key] ?? defaultValue;
        }

        public void SetSetting(string key, string value, bool noLoad = false)
        {
            ConfigurationFile[key] = value;
            if (!noLoad)
                LoadConfiguration();
        }

        public void SavePairs()
        {
            for (int i = 0; i < maxProfile; i++)
            {
                ConfigurationFile[$"tlodPairs{i}"] = CreateLodString(PairsTLOD[i]);
                ConfigurationFile[$"olodPairs{i}"] = CreateLodString(PairsOLOD[i]);
            }
            LoadConfiguration();
        }

        public void DetectGraphics()
        {
            VrModeActive = MemoryAccess.IsVrModeActive();
            FgModeActive = MemoryAccess.IsFgModeEnabled();
            if (Process.GetProcessesByName("LosslessScaling").Length > 0)
            {
                LsModeActive = true;
                LsModeMultiplier = GetLSModeMultiplier();
            }
            else LsModeActive = false;
        }

        public int GetLSModeMultiplier()
        {
            string xmlFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Lossless Scaling\Settings.xml";
            XDocument xmlDoc = XDocument.Load(xmlFilePath);
            var profile = xmlDoc.Descendants("Profile")
                        .FirstOrDefault(p => p.Element("Title")?.Value == (isMSFS2024 ? "MSFS2024" : "MSFS2020"));
            if (profile == null) profile = xmlDoc.Descendants("Profile")
                        .FirstOrDefault(p => p.Element("Title")?.Value == "Default");
            if (profile != null)
            {
                XElement frameGenerationElement = profile.Elements("FrameGeneration").FirstOrDefault();
                if (frameGenerationElement != null)
                {
                    if (frameGenerationElement.Value != "Off")
                    {
                        XElement lsfgModeElement = profile.Elements("LSFGMode").FirstOrDefault();
                        if (lsfgModeElement != null)
                        {
                            int LSFGMode = Convert.ToInt32(lsfgModeElement.Value.Substring(1, 1));
                            if (LSFGMode < 2 || LSFGMode > 4) LSFGMode = 1;
                            return LSFGMode;
                        }
                    }
                }
            }
            return 1;
        }
        public static string CloudQualityText(int CloudQuality)
        {
            if (CloudQuality == 0) return "Low";
            else if (CloudQuality == 1) return "Medium";
            else if (CloudQuality == 2) return "High";
            else if (CloudQuality == 3) return "Ultra";
            else return "n/a";
        }

    }
}