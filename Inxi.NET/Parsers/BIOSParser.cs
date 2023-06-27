
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

    class BIOSParser : HardwareParserBase, IHardwareParser
    {

        /// <summary>
        /// Parses BIOS info
        /// </summary>
        /// <param name="InxiToken">Inxi JSON token. Ignored in Windows.</param>
        /// <param name="SystemProfilerToken">system_profiler token</param>
        public override HardwareBase Parse(JToken InxiToken, NSArray SystemProfilerToken)
        {
            BIOS BIOSInfo;

            if (InxiInternalUtils.IsUnix())
            {
                BIOSInfo = (BIOS)ParseLinux(InxiToken);
            }
            else
            {
                InxiTrace.Debug("Selecting entries from Win32_BIOS...");
                var WMIBIOS = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
                BIOSInfo = (BIOS)ParseWindows(WMIBIOS);
            }

            return BIOSInfo;
        }

        public override HardwareBase ParseLinux(JToken InxiToken)
        {
            var BIOSInfo = default(BIOS);

            InxiTrace.Debug("Selecting the Machine token...");
            foreach (var InxiMachine in InxiToken.SelectTokenKeyEndingWith("Machine"))
            {
                // Get information of system
                string BIOS = (string)InxiMachine.SelectTokenKeyEndingWith("BIOS");
                string Date = (string)InxiMachine.SelectTokenKeyEndingWith("date");
                string Version = (string)InxiMachine.SelectTokenKeyEndingWith("v");
                InxiTrace.Debug("Got information. BIOS: {0}, Date: {1}, Version: {2}", BIOS, Date, Version);

                // Create an instance of system class
                BIOSInfo = new BIOS(BIOS, Date, Version);
            }

            return BIOSInfo;
        }

        public override HardwareBase ParseWindows(ManagementObjectSearcher WMISearcher)
        {
            var BIOSInfo = default(BIOS);
            var WMIBIOS = WMISearcher;

            // Get information of system
            InxiTrace.Debug("Getting the base objects...");
            foreach (ManagementBaseObject BIOSBase in WMIBIOS.Get())
            {
                string BIOS = (string)BIOSBase["Caption"];
                string Date = (string)BIOSBase["ReleaseDate"];
                string Version = (string)BIOSBase["Version"];
                InxiTrace.Debug("Got information. BIOS: {0}, Date: {1}, Version: {2}", BIOS, Date, Version);

                // Create an instance of system class
                BIOSInfo = new BIOS(BIOS, Date, Version);
            }

            return BIOSInfo;
        }

    }
}