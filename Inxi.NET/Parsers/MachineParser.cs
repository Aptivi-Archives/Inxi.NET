
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

    class MachineParser : HardwareParserBase, IHardwareParser
    {

        /// <summary>
        /// Parses machine info
        /// </summary>
        /// <param name="InxiToken">Inxi JSON token. Ignored in Windows.</param>
        /// <param name="SystemProfilerToken">system_profiler token</param>
        public override HardwareBase Parse(JToken InxiToken, NSArray SystemProfilerToken)
        {
            HardwareBase MachInfo;

            if (InxiInternalUtils.IsUnix())
            {
                if (InxiInternalUtils.IsMacOS())
                    MachInfo = ParseMacOS(SystemProfilerToken);
                else
                    MachInfo = ParseLinux(InxiToken);
            }
            else
            {
                InxiTrace.Debug("Selecting entries from Win32_ComputerSystem...");
                var WMIMachine = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                MachInfo = ParseWindows(WMIMachine);
            }

            return MachInfo;
        }

        public override HardwareBase ParseLinux(JToken InxiToken)
        {
            var MachInfo = default(HardwareBase);

            InxiTrace.Debug("Selecting the Machine token...");
            foreach (var InxiSys in InxiToken.SelectTokenKeyEndingWith("Machine"))
            {
                // Get information of system
                string Type = (string)InxiSys.SelectTokenKeyEndingWith("Type");
                string Product = (string)InxiSys.SelectTokenKeyEndingWith("product");
                string System = (string)InxiSys.SelectTokenKeyEndingWith("System");
                string Chassis = (string)InxiSys.SelectTokenKeyEndingWith("Chassis");
                string MoboManufacturer = (string)InxiSys.SelectTokenKeyEndingWith("Mobo");
                string MoboModel = (string)InxiSys.SelectTokenKeyEndingWith("model");
                InxiTrace.Debug("Got information. Type: {0}, Product: {1}, System: {2}, Chassis: {3}, MoboManufacturer: {4}, MoboModel: {5}", Type, Product, System, Chassis, MoboManufacturer, MoboModel);

                // Create an instance of system class
                MachInfo = new MachineInfo(Type, Product, System, Chassis, MoboManufacturer, MoboModel);
            }

            return MachInfo;
        }

        public override HardwareBase ParseMacOS(NSArray SystemProfilerToken)
        {
            var MachInfo = default(HardwareBase);

            // Check for data type
            InxiTrace.Debug("Checking for data type...");
            foreach (NSDictionary DataType in SystemProfilerToken)
            {
                if ((string)DataType["_dataType"].ToObject() == "SPHardwareDataType")
                {
                    InxiTrace.Debug("DataType found: SPHardwareDataType...");

                    // Get information of a machine
                    NSArray SoftwareEnum = (NSArray)DataType["_items"];
                    InxiTrace.Debug("Enumerating machines...");
                    foreach (NSDictionary SoftwareDict in SoftwareEnum)
                    {
                        // Get information of machine
                        string Type = SoftwareDict["machine_name"].ToObject().ToString().Contains("MacBook") ? "Laptop" : "Desktop";
                        string Product = (string)SoftwareDict["machine_name"].ToObject();
                        string System = "macOS";
                        string Chassis = "Apple";
                        string MoboManufacturer = "Apple";
                        string MoboModel = (string)SoftwareDict["machine_model"].ToObject();
                        InxiTrace.Debug("Got information. Type: {0}, Product: {1}, System: {2}, Chassis: {3}, MoboManufacturer: {4}, MoboModel: {5}", Type, Product, System, Chassis, MoboManufacturer, MoboModel);

                        // Create an instance of machine class
                        MachInfo = new MachineInfo(Type, Product, System, Chassis, MoboManufacturer, MoboModel);
                    }
                }
            }

            return MachInfo;
        }

        public override HardwareBase ParseWindows(ManagementObjectSearcher WMISearcher)
        {
            HardwareBase MachInfo;
            var WMIMachine = WMISearcher;

            InxiTrace.Debug("Selecting entries from Win32_BaseBoard...");
            var WMIBoard = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            InxiTrace.Debug("Selecting entries from Win32_OperatingSystem...");
            var WMISystem = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");

            // Get information of system and motherboard
            string Type = "";
            string Product = "";
            string System = "";
            string Chassis = "";
            string MoboModel = "";
            string MoboManufacturer = "";

            // Get information for ChassisSKUNumber
            InxiTrace.Debug("Getting the base objects...");
            foreach (ManagementBaseObject WMISystemBase in WMISystem.Get())
            {
                if (Convert.ToString(WMISystemBase["Version"]).StartsWith("10") && Environment.OSVersion.Platform == PlatformID.Win32NT) // If running on Windows 10
                {
                    InxiTrace.Debug("Target is running Windows 10/11.");
                    foreach (ManagementBaseObject MachineBase in WMIMachine.Get())
                        Type = (string)MachineBase["ChassisSKUNumber"];
                }
            }

            // TODO: Chassis not implemented in Windows
            // Get informaiton for machine model and family
            InxiTrace.Debug("Getting the base objects...");
            InxiTrace.Debug("TODO: Chassis not implemented in Windows");
            foreach (ManagementBaseObject MachineBase in WMIMachine.Get())
            {
                Product = (string)MachineBase["Model"];
                System = (string)MachineBase["SystemFamily"];
            }

            // Get information for model and manufacturer
            InxiTrace.Debug("Getting the base objects...");
            foreach (ManagementBaseObject MoboBase in WMIBoard.Get())
            {
                MoboModel = (string)MoboBase["Model"];
                MoboManufacturer = (string)MoboBase["Manufacturer"];
            }
            InxiTrace.Debug("Got information. Type: {0}, Product: {1}, System: {2}, Chassis: {3}, MoboManufacturer: {4}, MoboModel: {5}", Type, Product, System, Chassis, MoboManufacturer, MoboModel);

            // Create an instance of system class
            MachInfo = new MachineInfo(Type, Product, System, Chassis, MoboManufacturer, MoboModel);
            return MachInfo;
        }

    }
}