
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
using Claunia.PropertyList;
using Extensification.External.Newtonsoft.Json.JPropertyExts;
using Newtonsoft.Json.Linq;

namespace InxiFrontend
{

    class BatteryParser : HardwareParserBase, IHardwareParser
    {

        /// <summary>
        /// Parses battery info
        /// </summary>
        /// <param name="InxiToken">Inxi JSON token. Ignored in Windows.</param>
        /// <param name="SystemProfilerToken">system_profiler token</param>
        public override List<HardwareBase> ParseAllToList(JToken InxiToken, NSArray SystemProfilerToken)
        {
            List<HardwareBase> Batteries;

            if (InxiInternalUtils.IsUnix())
            {
                if (InxiInternalUtils.IsMacOS())
                    Batteries = ParseAllToListMacOS(SystemProfilerToken);
                else
                    Batteries = ParseAllToListLinux(InxiToken);
            }
            else
            {
                InxiTrace.Debug("Selecting entries from Win32_Battery...");
                var WMIBattery = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
                Batteries = ParseAllToListWindows(WMIBattery);
            }

            return Batteries;
        }
        public override List<HardwareBase> ParseAllToListLinux(JToken InxiToken)
        {
            var Batteries = new List<HardwareBase>();
            Battery Battery;

            InxiTrace.Debug("Selecting the Battery token...");
            if (InxiToken.SelectTokenKeyEndingWith("Battery") != null)
            {
                foreach (var InxiSys in InxiToken.SelectTokenKeyEndingWith("Battery"))
                {
                    // Get information of battery
                    string Name = (string)InxiSys.SelectTokenKeyEndingWith("ID");
                    int Charge = Convert.ToInt32(InxiSys.SelectTokenKeyEndingWith("charge").ToString().Replace("%", ""));
                    string Condition = (string)InxiSys.SelectTokenKeyEndingWith("condition");
                    string Volts = (string)InxiSys.SelectTokenKeyEndingWith("volts");
                    string Model = (string)InxiSys.SelectTokenKeyEndingWith("model");
                    string Status = (string)InxiSys.SelectTokenKeyEndingWith("status");
                    InxiTrace.Debug("Got information. Name: {0}, Charge: {1}, Condition: {2}, Volts: {3}, Model: {4}, Status: {5}", Name, Charge, Condition, Volts, Model, Status);

                    // Create an instance of battery class
                    Battery = new Battery(Name, Charge, Condition, Volts, Model, Status);
                    Batteries.Add(Battery);
                }
            }

            return Batteries;
        }

        public override List<HardwareBase> ParseAllToListMacOS(NSArray SystemProfilerToken)
        {
            var Batteries = new List<HardwareBase>();

            // TODO: Battery not implemented in macOS.
            InxiTrace.Debug("TODO: Battery not implemented in macOS.");

            // Create an instance of battery class
            var Battery = new Battery("Battery", 100, "", "", "", "Not charging");
            Batteries.Add(Battery);

            return Batteries;
        }

        public override List<HardwareBase> ParseAllToListWindows(ManagementObjectSearcher WMISearcher)
        {
            var Batteries = new List<HardwareBase>();
            var WMIBatt = WMISearcher;

            // Get information of system
            InxiTrace.Debug("Getting the base objects...");
            foreach (ManagementBaseObject BattBase in WMIBatt.Get())
            {
                // Get information of battery
                string Name = (string)BattBase["Caption"];
                int Charge = (int)BattBase["EstimatedChargeRemaining"];
                string Condition = (string)BattBase["BatteryStatus"];
                string Volts = (string)BattBase["DesignVoltage"];
                string Model = (string)BattBase["Name"];
                string Status = (string)BattBase["BatteryStatus"];
                InxiTrace.Debug("Got information. Name: {0}, Charge: {1}, Condition: {2}, Volts: {3}, Model: {4}, Status: {5}", Name, Charge, Condition, Volts, Model, Status);

                // Create an instance of battery class
                var Battery = new Battery(Name, Charge, Condition, Volts, Model, Status);
                Batteries.Add(Battery);
            }

            return Batteries;
        }

    }
}