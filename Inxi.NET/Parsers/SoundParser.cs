
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

using System.Collections.Generic;
using System.Management;
using Claunia.PropertyList;
using Extensification.DictionaryExts;
using Extensification.External.Newtonsoft.Json.JPropertyExts;
using Newtonsoft.Json.Linq;

namespace InxiFrontend
{

    class SoundParser : HardwareParserBase, IHardwareParser
    {

        /// <summary>
        /// Parses sound cards
        /// </summary>
        /// <param name="InxiToken">Inxi JSON token. Ignored in Windows.</param>
        /// <param name="SystemProfilerToken">system_profiler token</param>
        public override Dictionary<string, HardwareBase> ParseAll(JToken InxiToken, NSArray SystemProfilerToken)
        {
            Dictionary<string, HardwareBase> SPUParsed;

            if (InxiInternalUtils.IsUnix())
            {
                if (InxiInternalUtils.IsMacOS())
                    SPUParsed = ParseAllMacOS(SystemProfilerToken);
                else
                    SPUParsed = ParseAllLinux(InxiToken);
            }
            else
            {
                InxiTrace.Debug("Selecting entries from Win32_SoundDevice...");
                var SoundDevice = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice");
                SPUParsed = ParseAllWindows(SoundDevice);
            }

            return SPUParsed;
        }

        public override Dictionary<string, HardwareBase> ParseAllLinux(JToken InxiToken)
        {
            var SPUParsed = new Dictionary<string, HardwareBase>();
            Sound SPU;

            // SPU information fields
            string SPUName;
            string SPUVendor;
            string SPUDriver;
            string SPUBusID;
            string SPUChipID;

            InxiTrace.Debug("Selecting the Audio token...");
            foreach (var InxiSPU in InxiToken.SelectTokenKeyEndingWith("Audio"))
            {
                if (InxiSPU.SelectTokenKeyEndingWith("Device") is not null)
                {
                    // Get information of a sound card
                    SPUName = (string)InxiSPU.SelectTokenKeyEndingWith("Device");
                    SPUVendor = (string)InxiSPU.SelectTokenKeyEndingWith("vendor");
                    SPUDriver = (string)InxiSPU.SelectTokenKeyEndingWith("driver");
                    SPUBusID = (string)InxiSPU.SelectTokenKeyEndingWith("bus ID");
                    SPUChipID = (string)InxiSPU.SelectTokenKeyEndingWith("chip ID");
                    InxiTrace.Debug("Got information. SPUName: {0}, SPUDriver: {1}, SPUVendor: {2}, SPUBusID: {3}, SPUChipID: {4}", SPUName, SPUDriver, SPUVendor, SPUBusID, SPUChipID);

                    // Create an instance of sound class
                    SPU = new Sound(SPUName, SPUVendor, SPUDriver, SPUChipID, SPUBusID);
                    SPUParsed.AddIfNotFound(SPUName, SPU);
                    InxiTrace.Debug("Added {0} to the list of parsed SPUs.", SPUName);
                }
            }

            return SPUParsed;
        }

        public override Dictionary<string, HardwareBase> ParseAllMacOS(NSArray SystemProfilerToken)
        {
            var SPUParsed = new Dictionary<string, HardwareBase>();
            Sound SPU;

            // TODO: Currently, Inxi.NET adds a dumb device to parsed device. We need actual data. Use "system_profiler SPAudioDataType -xml >> audio.plist" and attach it to Issues
            // Create an instance of sound class
            InxiTrace.Debug("TODO: Currently, Inxi.NET adds a dumb device to parsed device. We need actual data. Use \"system_profiler SPAudioDataType -xml >> audio.plist\" and attach it to Issues.");
            SPU = new Sound("Placeholder", "Aptivi", "SoundParser", "", "");
            SPUParsed.AddIfNotFound("Placeholder", SPU);
            InxiTrace.Debug("Added Placeholder to the list of parsed SPUs.");
            return SPUParsed;
        }

        public override Dictionary<string, HardwareBase> ParseAllWindows(ManagementObjectSearcher WMISearcher)
        {
            var SoundDevice = WMISearcher;
            var SPUParsed = new Dictionary<string, HardwareBase>();
            Sound SPU;

            // SPU information fields
            string SPUName;
            string SPUVendor;
            string SPUDriver;
            string SPUBusID;
            string SPUChipID;

            // TODO: Driver not implemented in Windows
            // Get information of sound cards
            InxiTrace.Debug("Getting the base objects...");
            InxiTrace.Debug("TODO: Driver not implemented in Windows.");
            foreach (ManagementBaseObject Device in SoundDevice.Get())
            {
                // Get information of a sound card
                SPUName = (string)Device["ProductName"];
                SPUVendor = (string)Device["Manufacturer"];
                SPUDriver = "";
                SPUChipID = (string)Device["DeviceID"];
                SPUBusID = "";
                InxiTrace.Debug("Got information. SPUName: {0}, SPUDriver: {1}, SPUVendor: {2}, SPUBusID: {3}, SPUChipID: {4}", SPUName, SPUDriver, SPUVendor, SPUBusID, SPUChipID);

                // Create an instance of sound class
                SPU = new Sound(SPUName, SPUVendor, SPUDriver, SPUChipID, SPUBusID);
                SPUParsed.AddIfNotFound(SPUName, SPU);
                InxiTrace.Debug("Added {0} to the list of parsed SPUs.", SPUName);
            }

            return SPUParsed;
        }

    }
}