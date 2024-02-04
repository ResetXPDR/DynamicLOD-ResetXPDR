using System;

namespace DynamicLOD_ResetEdition
{
    public class MemoryManager
    {
        private ServiceModel Model;

        private long addrTLOD;
        private long addrOLOD;
        private long addrTLOD_VR;
        private long addrOLOD_VR;
        private long addrCloudQ;
        private long addrCloudQ_VR;
        private long addrVrMode;

        public MemoryManager(ServiceModel model)
        {
            try
            {
                this.Model = model;

                MemoryInterface.Attach(Model.SimBinary);
                long moduleBase = MemoryInterface.GetModuleAddress(Model.SimModule);

                addrTLOD = MemoryInterface.ReadMemory<long>(moduleBase + Model.OffsetModuleBase) + Model.OffsetPointerMain;
                addrTLOD_VR = MemoryInterface.ReadMemory<long>(addrTLOD) + Model.OffsetPointerTlodVr;
                Logger.Log(LogLevel.Debug, "MemoryManager:MemoryManager", $"Address TLOD VR: 0x{addrTLOD_VR:X} / {addrTLOD_VR}");
                addrTLOD = MemoryInterface.ReadMemory<long>(addrTLOD) + Model.OffsetPointerTlod;
                Logger.Log(LogLevel.Debug, "MemoryManager:MemoryManager", $"Address TLOD: 0x{addrTLOD:X} / {addrTLOD}");
                addrOLOD_VR = addrTLOD_VR + Model.OffsetPointerOlod;
                Logger.Log(LogLevel.Debug, "MemoryManager:MemoryManager", $"Address OLOD VR: 0x{addrOLOD_VR:X} / {addrOLOD_VR}");
                addrOLOD = addrTLOD + Model.OffsetPointerOlod;
                Logger.Log(LogLevel.Debug, "MemoryManager:MemoryManager", $"Address OLOD: 0x{addrOLOD:X} / {addrOLOD}");
                addrCloudQ = addrTLOD + Model.OffsetPointerCloudQ;
                Logger.Log(LogLevel.Debug, "MemoryManager:MemoryManager", $"Address CloudQ: 0x{addrCloudQ:X} / {addrCloudQ}");
                addrCloudQ_VR = addrCloudQ + Model.OffsetPointerCloudQVr; 
                Logger.Log(LogLevel.Debug, "MemoryManager:MemoryManager", $"Address CloudQ VR: 0x{addrCloudQ_VR:X} / {addrCloudQ_VR}");
                addrVrMode = addrTLOD - Model.OffsetPointerVrMode;
                Logger.Log(LogLevel.Debug, "MemoryManager:MemoryManager", $"Address VrMode1: 0x{addrVrMode:X} / {addrVrMode}");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:MemoryManager", $"Exception {ex}: {ex.Message}");
            }
        }

        public bool IsVrModeActive()
        {
            try
            {
                return MemoryInterface.ReadMemory<int>(addrVrMode) == 1; 
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:IsVrModeActive", $"Exception {ex}: {ex.Message}");
            }

            return false;
        }

        public float GetTLOD_PC()
        {
            try
            {
                return (float)Math.Round(MemoryInterface.ReadMemory<float>(addrTLOD) * 100.0f);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:GetTLOD", $"Exception {ex}: {ex.Message}");
            }

            return 0.0f;
        }

        public float GetTLOD_VR()
        {
            try
            {
                return (float)Math.Round(MemoryInterface.ReadMemory<float>(addrTLOD_VR) * 100.0f);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:GetTLOD_VR", $"Exception {ex}: {ex.Message}");
            }

            return 0.0f;
        }

        public float GetOLOD_PC()
        {
            try
            {
                return (float)Math.Round(MemoryInterface.ReadMemory<float>(addrOLOD) * 100.0f);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:GetOLOD", $"Exception {ex}: {ex.Message}");
            }

            return 0.0f;
        }

         public float GetOLOD_VR()
        {
            try
            {
                return (float)Math.Round(MemoryInterface.ReadMemory<float>(addrOLOD_VR) * 100.0f);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:GetOLOD_VR", $"Exception {ex}: {ex.Message}");
            }

            return 0.0f;
        }

        public int GetCloudQ()
        {
            try
            {
                return MemoryInterface.ReadMemory<int>(addrCloudQ);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:GetCloudQ", $"Exception {ex}: {ex.Message}");
            }

            return -1;
        }
        public int GetCloudQ_VR()
        {
            try
            {
                return MemoryInterface.ReadMemory<int>(addrCloudQ_VR);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:GetCloudQ VR", $"Exception {ex}: {ex.Message}");
            }

            return -1;
        }
        public void SetTLOD(float value)
        {
            SetTLOD_PC(value);
            SetTLOD_VR(value);
        }
        public void SetTLOD_PC(float value)
        {
            try
            {
                MemoryInterface.WriteMemory<float>(addrTLOD, value / 100.0f);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:SetTLOD", $"Exception {ex}: {ex.Message}");
            }
        }
        public void SetTLOD_VR(float value)
        {
            try
            {
                MemoryInterface.WriteMemory<float>(addrTLOD_VR, value / 100.0f);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:SetTLOD VR", $"Exception {ex}: {ex.Message}");
            }
        }
        public void SetOLOD(float value)
        {
            SetOLOD_PC(value);
            SetOLOD_VR(value);
        }
        public void SetOLOD_PC(float value)
        {
            try
            {
                MemoryInterface.WriteMemory<float>(addrOLOD, value / 100.0f);
                
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:SetOLOD", $"Exception {ex}: {ex.Message}");
            }
        }
        public void SetOLOD_VR(float value)
        {
            try
            {
                MemoryInterface.WriteMemory<float>(addrOLOD_VR, value / 100.0f);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:SetOLOD VR", $"Exception {ex}: {ex.Message}");
            }
        }
        public void SetCloudQ(int value)
        {
            try
            {
                MemoryInterface.WriteMemory<int>(addrCloudQ, value);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:SetCloudQ", $"Exception {ex}: {ex.Message}");
            }
        }
        public void SetCloudQ_VR(int value)
        {
            try
            {
                MemoryInterface.WriteMemory<int>(addrCloudQ_VR, value);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MemoryManager:SetCloudQ VR", $"Exception {ex}: {ex.Message}");
            }
        }
    }
}
