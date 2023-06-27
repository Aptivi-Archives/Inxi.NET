
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
using System.Management;
using Claunia.PropertyList;
using Extensification.External.Newtonsoft.Json.JPropertyExts;
using Newtonsoft.Json.Linq;

namespace InxiFrontend
{

    class PCMemoryParser : HardwareParserBase, IHardwareParser
    {

        /// <summary>
        /// Parses processors
        /// </summary>
        /// <param name="InxiToken">Inxi JSON token. Ignored in Windows.</param>
        /// <param name="SystemProfilerToken">system_profiler token</param>
        public override HardwareBase Parse(JToken InxiToken, NSArray SystemProfilerToken)
        {
            PCMemory Mem;
            if (InxiInternalUtils.IsUnix())
            {
                if (InxiInternalUtils.IsMacOS())
                    Mem = (PCMemory)ParseMacOS(SystemProfilerToken);
                else
                    Mem = (PCMemory)ParseLinux(InxiToken);
            }
            else
            {
                InxiTrace.Debug("Selecting entries from Win32_OperatingSystem...");
                var System = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                Mem = (PCMemory)ParseWindows(System);
            }

            return Mem;
        }

        public override HardwareBase ParseLinux(JToken InxiToken)
        {
            var Mem = default(PCMemory);

            // TODO: Free memory is not implemented in Inxi.
            InxiTrace.Debug("TODO: Free memory is not implemented in Inxi.");
            InxiTrace.Debug("Selecting the Info token...");
            foreach (var InxiMem in InxiToken.SelectTokenKeyEndingWith("Info"))
            {
                // Get information of memory
                string TotalMem = (string)InxiMem.SelectTokenKeyEndingWith("Memory");
                string UsedMem = (string)InxiMem.SelectTokenKeyEndingWith("used");
                InxiTrace.Debug("Got information. TotalMem: {0}, UsedMem: {1}", TotalMem, UsedMem);

                // Create an instance of memory class
                Mem = new PCMemory(TotalMem, UsedMem, "");
            }

            return Mem;
        }

        public override HardwareBase ParseMacOS(NSArray SystemProfilerToken)
        {
            var Mem = default(PCMemory);

            // TODO: Used memory and free memory not implemented in macOS.
            // Check for data type
            InxiTrace.Debug("Checking for data type...");
            InxiTrace.Debug("TODO: Used memory and free memory not implemented in macOS.");
            foreach (NSDictionary DataType in SystemProfilerToken)
            {
                if ((string)DataType["_dataType"].ToObject() == "SPHardwareDataType")
                {
                    InxiTrace.Debug("DataType found: SPHardwareDataType...");

                    // Get information of a memory
                    NSArray HardwareEnum = (NSArray)DataType["_items"];
                    InxiTrace.Debug("Enumerating memory information...");
                    foreach (NSDictionary HardwareDict in HardwareEnum)
                    {
                        // Get information of memory
                        string TotalMem = (string)HardwareDict["physical_memory"].ToObject();
                        InxiTrace.Debug("Got information. TotalMem: {0}", TotalMem);

                        // Create an instance of memory class
                        Mem = new PCMemory(TotalMem, "", "");
                    }
                }
            }

            return Mem;
        }

        public override HardwareBase ParseWindows(ManagementObjectSearcher WMISearcher)
        {
            PCMemory Mem;
            var System = WMISearcher;
            var TotalMem = default(long);
            var UsedMem = default(long);
            var FreeMem = default(long);

            // Get memory
            InxiTrace.Debug("Getting the base objects...");
            foreach (ManagementBaseObject OS in System.Get())
            {
                TotalMem = Convert.ToInt64(OS["TotalVisibleMemorySize"]);
                UsedMem = TotalMem - Convert.ToInt64(OS["FreePhysicalMemory"]);
                FreeMem = Convert.ToInt64(OS["FreePhysicalMemory"]);
                InxiTrace.Debug("Got information. TotalMem: {0}, UsedMem: {1}, FreeMem: {2}", TotalMem, UsedMem, FreeMem);
            }

            // Create an instance of memory class
            Mem = new PCMemory(TotalMem.ToString(), UsedMem.ToString(), FreeMem.ToString());
            return Mem;
        }

    }
}