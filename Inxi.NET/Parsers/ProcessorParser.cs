
// Inxi.NET  Copyright (C) 2020-2021  Aptivi
// 
// This file is part of Inxi.NET
// 
// Inxi.NET is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Inxi.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using Claunia.PropertyList;
using Extensification.ArrayExts;
using Extensification.DictionaryExts;
using Extensification.External.Newtonsoft.Json.JPropertyExts;
using Newtonsoft.Json.Linq;

namespace InxiFrontend
{

    class ProcessorParser : HardwareParserBase, IHardwareParser
    {

        /// <summary>
        /// Parses processors
        /// </summary>
        /// <param name="InxiToken">Inxi JSON token. Ignored in Windows.</param>
        /// <param name="SystemProfilerToken">system_profiler token</param>
        public override Dictionary<string, HardwareBase> ParseAll(JToken InxiToken, NSArray SystemProfilerToken)
        {
            Dictionary<string, HardwareBase> CPUParsed;

            if (InxiInternalUtils.IsUnix())
            {
                if (InxiInternalUtils.IsMacOS())
                    CPUParsed = ParseAllMacOS(SystemProfilerToken);
                else
                    CPUParsed = ParseAllLinux(InxiToken);
            }
            else
            {
                InxiTrace.Debug("Selecting entries from Win32_Processor...");
                var CPUClass = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                CPUParsed = ParseAllWindows(CPUClass);
            }

            return CPUParsed;
        }

        public override Dictionary<string, HardwareBase> ParseAllLinux(JToken InxiToken)
        {
            var CPUParsed = new Dictionary<string, HardwareBase>();
            Processor CPU;
            var CPUSpeedReady = default(bool);

            // CPU information fields
            string CPUName = "";
            string CPUTopology = "";
            string CPUType = "";
            var CPUBits = default(int);
            string CPUMilestone = "";
            var CPUFlags = Array.Empty<string>();
            string CPUL2Size = "";
            int CPUL3Size = 0;
            string CPUSpeed = "";
            string CPURev = "";
            int CPUBogoMips = 0;

            // TODO: L3 cache is not implemented in Linux
            InxiTrace.Debug("TODO: L3 cache is not implemented in Linux.");
            InxiTrace.Debug("Selecting the CPU token...");
            foreach (var InxiCPU in InxiToken.SelectTokenKeyEndingWith("CPU"))
            {
                if (!CPUSpeedReady)
                {
                    // Get information of a processor
                    CPUName = (string)InxiCPU.SelectTokenKeyEndingWith("model");
                    CPUTopology = (string)InxiCPU.SelectTokenKeyEndingWith("Topology");
                    if (string.IsNullOrEmpty(CPUTopology))
                        CPUTopology = (string)InxiCPU.SelectTokenKeyEndingWith("Info");
                    CPUType = (string)InxiCPU.SelectTokenKeyEndingWith("type");
                    CPUBits = (int)InxiCPU.SelectTokenKeyEndingWith("bits");
                    CPUMilestone = (string)InxiCPU.SelectTokenKeyEndingWith("arch");
                    CPUL2Size = (string)InxiCPU.SelectTokenKeyContaining("L2");
                    CPURev = (string)InxiCPU.SelectTokenKeyEndingWith("rev");
                    CPUSpeedReady = true;
                }
                else if (InxiCPU.SelectTokenKeyEndingWith("flags") is not null)
                {
                    CPUFlags = ((string)InxiCPU.SelectTokenKeyEndingWith("flags")).Split(' ');
                    CPUBogoMips = (int)InxiCPU.SelectTokenKeyEndingWith("bogomips");
                }
                else
                    CPUSpeed = (string)InxiCPU.SelectTokenKeyEndingWith("Speed");
                InxiTrace.Debug("Got information. CPUName: {0}, CPUTopology: {1}, CPUType: {2}, CPUBits: {3}, CPUMilestone: {4}, CPUL2Size: {5}, CPURev: {6}, CPUFlags: {7}, CPUBogoMips: {8}, CPUSpeed: {9}", CPUName, CPUTopology, CPUType, CPUBits, CPUMilestone, CPUL2Size, CPURev, CPUFlags.Length, CPUBogoMips, CPUSpeed);
            }

            // Create an instance of processor class
            CPU = new Processor(CPUName, CPUTopology, CPUType, CPUBits, CPUMilestone, CPUFlags, CPUL2Size, CPUL3Size, CPURev, CPUBogoMips, CPUSpeed);
            CPUParsed.AddIfNotFound(CPUName, CPU);
            InxiTrace.Debug("Added {0} to the list of parsed processors.", CPUName);
            return CPUParsed;
        }

        public override Dictionary<string, HardwareBase> ParseAllMacOS(NSArray SystemProfilerToken)
        {
            var CPUParsed = new Dictionary<string, HardwareBase>();
            Processor CPU;

            // CPU information fields
            string CPUName = "";
            string CPUTopology = "";
            string CPUType = "";
            var CPUBits = default(int);
            string CPUMilestone = "";
            var CPUFlags = Array.Empty<string>();
            string CPUL2Size = "";
            int CPUL3Size = 0;
            string CPUSpeed = "";
            string CPURev = "";
            int CPUBogoMips = 0;

            // TODO: L2, L3, and speed only done in macOS
            // Check for data type
            InxiTrace.Debug("Checking for data type...");
            InxiTrace.Debug("TODO: L2, L3, and speed only done in macOS.");
            foreach (NSDictionary DataType in SystemProfilerToken)
            {
                if ((string)DataType["_dataType"].ToObject() == "SPHardwareDataType")
                {
                    InxiTrace.Debug("DataType found: SPHardwareDataType...");

                    // Get information of a drive
                    NSArray HardwareEnum = (NSArray)DataType["_items"];
                    foreach (NSDictionary HardwareDict in HardwareEnum)
                    {
                        CPUL2Size = (string)HardwareDict["l2_cache"].ToObject();
                        CPUL3Size = Convert.ToInt32(HardwareDict["l3_cache"].ToObject().ToString().Replace(" MB", ""));
                        CPUSpeed = (string)HardwareDict["current_processor_speed"].ToObject();
                        InxiTrace.Debug("Got information. CPUL2Size: {0}, CPUL3Size: {1}, CPUSpeed: {2}", CPUL2Size, CPUL3Size, CPUSpeed);
                    }
                }
            }

            // Create an instance of processor class
            CPU = new Processor(CPUName, CPUTopology, CPUType, CPUBits, CPUMilestone, CPUFlags, CPUL2Size, CPUL3Size, CPURev, CPUBogoMips, CPUSpeed);
            CPUParsed.AddIfNotFound(CPUName, CPU);
            InxiTrace.Debug("Added {0} to the list of parsed processors.", CPUName);
            return CPUParsed;
        }

        public override Dictionary<string, HardwareBase> ParseAllWindows(ManagementObjectSearcher WMISearcher)
        {
            var CPUParsed = new Dictionary<string, HardwareBase>();
            InxiTrace.Debug("Selecting entries from Win32_OperatingSystem...");
            var CPUClass = WMISearcher;
            Processor CPU;

            // CPU information fields
            string CPUName = "";
            string CPUTopology = "";
            string CPUType = "";
            var CPUBits = default(int);
            string CPUMilestone = "";
            var CPUFlags = Array.Empty<string>();
            string CPUL2Size = "";
            int CPUL3Size = 0;
            string CPUSpeed = "";
            string CPURev = "";
            int CPUBogoMips = 0;

            // TODO: Topology, Rev, BogoMips, and Milestone not implemented in Windows
            // Get information of processors
            InxiTrace.Debug("Getting the base objects...");
            InxiTrace.Debug("TODO: Topology, Rev, BogoMips, and Milestone not implemented in Windows.");
            foreach (ManagementBaseObject CPUManagement in CPUClass.Get())
            {
                CPUName = (string)CPUManagement["Name"];
                CPUType = Convert.ToString(CPUManagement["ProcessorType"]);
                CPUBits = Convert.ToInt32(CPUManagement["DataWidth"]);
                CPUL2Size = Convert.ToString(CPUManagement["L2CacheSize"]);
                CPUL3Size = Convert.ToInt32(CPUManagement["L3CacheSize"]);
                CPUSpeed = Convert.ToString(CPUManagement["CurrentClockSpeed"]);
                foreach (CPUFeatures.SSEnum CPUFeature in Enum.GetValues(typeof(CPUFeatures.SSEnum)))
                {
                    if (CPUFeatures.IsProcessorFeaturePresent(CPUFeature))
                        CPUFlags = CPUFlags.Add(CPUFeature.ToString().ToLower());
                }
                InxiTrace.Debug("Got information. CPUName: {0}, CPUType: {1}, CPUBits: {2}, CPUL2Size: {3}, CPUFlags: {4}, CPUL3Size: {5}, CPUSpeed: {6}", CPUName, CPUType, CPUBits, CPUL2Size, CPUFlags.Length, CPUL3Size, CPUSpeed);
            }

            // Create an instance of processor class
            CPU = new Processor(CPUName, CPUTopology, CPUType, CPUBits, CPUMilestone, CPUFlags, CPUL2Size, CPUL3Size, CPURev, CPUBogoMips, CPUSpeed);
            CPUParsed.AddIfNotFound(CPUName, CPU);
            InxiTrace.Debug("Added {0} to the list of parsed processors.", CPUName);
            return CPUParsed;
        }

    }

    static class CPUFeatures
    {

        /// <summary>
        /// [Windows] Check for specific processor feature. More info: https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-isprocessorfeaturepresent
        /// </summary>
        /// <param name="processorFeature">An SSE version</param>
        /// <returns>True if supported, false if not supported</returns>
        [DllImport("kernel32.dll")]
        internal static extern bool IsProcessorFeaturePresent(SSEnum processorFeature);

        /// <summary>
        /// [Windows] Collection of SSE versions
        /// </summary>
        internal enum SSEnum : uint
        {
            /// <summary>
            /// [Windows] The SSE instruction set is available.
            /// </summary>
            SSE = 6U,
            /// <summary>
            /// [Windows] The SSE2 instruction set is available. (This is used in most apps nowadays, since recent processors have this capability.)
            /// </summary>
            SSE2 = 10U,
            /// <summary>
            /// [Windows] The SSE3 instruction set is available.
            /// </summary>
            SSE3 = 13U
        }

    }
}