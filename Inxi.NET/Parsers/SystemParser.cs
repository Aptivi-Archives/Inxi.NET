
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
using System.Diagnostics;
using System.Management;
using Claunia.PropertyList;
using Extensification.External.Newtonsoft.Json.JPropertyExts;
using Newtonsoft.Json.Linq;

namespace InxiFrontend
{

    class SystemParser : HardwareParserBase, IHardwareParser
    {

        /// <summary>
        /// Parses system info
        /// </summary>
        /// <param name="InxiToken">Inxi JSON token. Ignored in Windows.</param>
        /// <param name="SystemProfilerToken">system_profiler token</param>
        public override HardwareBase Parse(JToken InxiToken, NSArray SystemProfilerToken)
        {
            HardwareBase SysInfo;

            if (InxiInternalUtils.IsUnix())
            {
                if (InxiInternalUtils.IsMacOS())
                    SysInfo = ParseMacOS(SystemProfilerToken);
                else
                    SysInfo = ParseLinux(InxiToken);
            }
            else
            {
                InxiTrace.Debug("Selecting entries from Win32_OperatingSystem...");
                var WMISystem = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                SysInfo = ParseWindows(WMISystem);
            }

            return SysInfo;
        }
        public override HardwareBase ParseLinux(JToken InxiToken)
        {
            var SysInfo = default(HardwareBase);

            InxiTrace.Debug("Selecting the System token...");
            foreach (var InxiSys in InxiToken.SelectTokenKeyEndingWith("System"))
            {
                // Get information of system
                string Hostname = (string)InxiSys.SelectTokenKeyEndingWith("Host");
                string Version = (string)InxiSys.SelectTokenKeyEndingWith("Kernel");
                int Bits = (int)InxiSys.SelectTokenKeyEndingWith("bits");
                string Distro = (string)InxiSys.SelectTokenKeyEndingWith("Distro");
                string DesktopMan = (string)InxiSys.SelectTokenKeyEndingWith("Desktop");
                string WindowMan = (string)InxiSys.SelectTokenKeyEndingWith("WM");
                string DisplayMan = (string)InxiSys.SelectTokenKeyEndingWith("dm");
                if (string.IsNullOrEmpty(WindowMan))
                    WindowMan = (string)InxiSys.SelectTokenKeyEndingWith("wm");
                InxiTrace.Debug("Got information. Hostname: {0}, Version: {1}, Distro: {2}, Bits: {3}, DesktopMan: {4}, WindowMan: {5}, DisplayMan: {6}", Hostname, Version, Distro, Bits, DesktopMan, WindowMan, DisplayMan);

                // Create an instance of system class
                SysInfo = new SystemInfo(Hostname, Version, Bits, Distro, DesktopMan, WindowMan, DisplayMan);
            }

            return SysInfo;
        }

        public override HardwareBase ParseMacOS(NSArray SystemProfilerToken)
        {
            var SysInfo = default(HardwareBase);

            // TODO: Bits, DE, WM, and DM not implemented in macOS.
            // Check for data type
            InxiTrace.Debug("Checking for data type...");
            InxiTrace.Debug("TODO: Bits, DE, WM, and DM not implemented in macOS.");
            foreach (NSDictionary DataType in SystemProfilerToken)
            {
                if ((string)DataType["_dataType"].ToObject() == "SPSoftwareDataType")
                {
                    InxiTrace.Debug("DataType found: SPSoftwareDataType...");

                    // Get information of the system
                    NSArray SoftwareEnum = (NSArray)DataType["_items"];
                    InxiTrace.Debug("Enumerating system information...");
                    foreach (NSDictionary SoftwareDict in SoftwareEnum)
                    {
                        // Get information of memory
                        string Hostname = (string)SoftwareDict["local_host_name"].ToObject();
                        string Version = (string)SoftwareDict["kernel_version"].ToObject();
                        string Distro = (string)SoftwareDict["os_version"].ToObject();
                        InxiTrace.Debug("Got information. Hostname: {0}, Version: {1}, Distro: {2}", Hostname, Version, Distro);

                        // Create an instance of system class
                        SysInfo = new SystemInfo(Hostname, Version, 64, Distro, "", "", "");
                    }
                }
            }

            return SysInfo;
        }

        public override HardwareBase ParseWindows(ManagementObjectSearcher WMISearcher)
        {
            var WMISystem = WMISearcher;
            var SysInfo = default(HardwareBase);

            // Get information of system
            // TODO: Desktop Manager and Display Manager are not implemented on Windows.
            InxiTrace.Debug("Getting the base objects...");
            InxiTrace.Debug("TODO: Desktop Manager and Display Manager are not implemented on Windows.");
            foreach (ManagementBaseObject SystemBase in WMISystem.Get())
            {
                string Hostname = System.Net.Dns.GetHostName();
                string Version = (string)SystemBase["Version"];
                int Bits = Convert.ToInt32(SystemBase["OSArchitecture"].ToString().Replace("-bit", ""));
                string Distro = (string)SystemBase["Caption"];
                string WM = Process.GetProcessesByName("dwm").Length > 0 ? "DWM" : "Basic Window Manager";
                InxiTrace.Debug("Got information. Hostname: {0}, Version: {1}, Distro: {2}, Bits: {3}, WM: {4}", Hostname, Version, Distro, Bits, WM);

                // Create an instance of system class
                SysInfo = new SystemInfo(Hostname, Version, Bits, Distro, "", WM, "");
            }

            return SysInfo;
        }

    }
}