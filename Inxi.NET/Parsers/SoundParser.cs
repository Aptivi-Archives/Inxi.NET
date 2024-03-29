﻿
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
using System.Linq;
using System.Management;
using Claunia.PropertyList;
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
            JToken spu = InxiInternalUtils.GetTokenFromInxiToken("Audio", InxiToken);
            JToken finalProperty = spu;
            if (spu.Type == JTokenType.Property)
            {
                foreach (var InxiSpu in spu)
                    finalProperty = InxiSpu;
            }
            foreach (var inxiSpu in finalProperty)
            {
                if (inxiSpu.SelectTokenKeyEndingWith("Device") is not null)
                {
                    // Get information of a sound card
                    SPUName = (string)inxiSpu.SelectTokenKeyEndingWith("Device");
                    SPUVendor = (string)inxiSpu.SelectTokenKeyEndingWith("vendor");
                    SPUDriver = (string)inxiSpu.SelectTokenKeyEndingWith("driver");
                    SPUBusID = (string)inxiSpu.SelectTokenKeyEndingWith("bus ID");
                    SPUChipID = (string)inxiSpu.SelectTokenKeyEndingWith("chip ID");
                    InxiTrace.Debug("Got information. SPUName: {0}, SPUDriver: {1}, SPUVendor: {2}, SPUBusID: {3}, SPUChipID: {4}", SPUName, SPUDriver, SPUVendor, SPUBusID, SPUChipID);

                    // Create an instance of sound class
                    SPU = new Sound(SPUName, SPUVendor, SPUDriver, SPUChipID, SPUBusID);
                    if (!SPUParsed.ContainsKey(SPUName))
                        SPUParsed.Add(SPUName, SPU);
                    InxiTrace.Debug("Added {0} to the list of parsed SPUs.", SPUName);
                }
            }

            return SPUParsed;
        }

        public override Dictionary<string, HardwareBase> ParseAllMacOS(NSArray SystemProfilerToken)
        {
            var SPUParsed = new Dictionary<string, HardwareBase>();
            Sound SPU;

            // Check for data type
            // TODO: Bus ID and Chip ID not implemented in macOS.
            InxiTrace.Debug("Checking for data type...");
            InxiTrace.Debug("TODO: Bus ID and Chip ID not implemented in macOS.");
            foreach (NSDictionary DataType in SystemProfilerToken.Cast<NSDictionary>())
            {
                if ((string)DataType["_dataType"].ToObject() == "SPAudioDataType")
                {
                    InxiTrace.Debug("DataType found: SPAudioDataType...");

                    // Get information of a drive
                    NSArray AudioEnum = (NSArray)DataType["_items"];
                    foreach (NSDictionary AudioDict in AudioEnum.Cast<NSDictionary>())
                    {
                        NSArray AudioItemEnum = (NSArray)AudioDict["_items"];
                        foreach (NSDictionary AudioItemDict in AudioItemEnum.Cast<NSDictionary>())
                        {
                            string Name = "";
                            string Vendor = "";
                            string Driver = "";

                            // Populate information
                            Name = (string)AudioItemDict["_name"].ToObject();
                            Vendor = (string)AudioItemDict["coreaudio_device_manufacturer"].ToObject();
                            Driver = (string)AudioItemDict["coreaudio_device_transport"].ToObject();
                            InxiTrace.Debug("Got information. Name: {0}, Vendor: {1}, Driver: {2}", Name, Vendor, Driver);

                            // Create an instance of sound class
                            SPU = new Sound(Name, Vendor, Driver, "", "");
                            if (!SPUParsed.ContainsKey(Name))
                                SPUParsed.Add(Name, SPU);
                            InxiTrace.Debug("Added {0} to the list of parsed SPUs.", Name);
                        }
                    }
                }
            }
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
                if (!SPUParsed.ContainsKey(SPUName))
                    SPUParsed.Add(SPUName, SPU);
                InxiTrace.Debug("Added {0} to the list of parsed SPUs.", SPUName);
            }

            return SPUParsed;
        }

    }
}