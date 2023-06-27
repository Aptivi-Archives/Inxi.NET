
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
using Extensification.DictionaryExts;
using Extensification.External.Newtonsoft.Json.JPropertyExts;
using Newtonsoft.Json.Linq;

namespace InxiFrontend
{

    class NetworkParser : HardwareParserBase, IHardwareParser
    {

        /// <summary>
        /// Parses network cards
        /// </summary>
        /// <param name="InxiToken">Inxi JSON token. Ignored in Windows.</param>
        /// <param name="SystemProfilerToken">system_profiler token</param>
        public override Dictionary<string, HardwareBase> ParseAll(JToken InxiToken, NSArray SystemProfilerToken)
        {
            Dictionary<string, HardwareBase> NetworkParsed;

            if (InxiInternalUtils.IsUnix())
            {
                if (InxiInternalUtils.IsMacOS())
                    NetworkParsed = ParseAllMacOS(SystemProfilerToken);
                else
                    NetworkParsed = ParseAllLinux(InxiToken);
            }
            else
            {
                InxiTrace.Debug("Selecting entries from Win32_NetworkAdapter...");
                var Networks = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter");
                NetworkParsed = ParseAllWindows(Networks);
            }

            // Return list of network devices
            return NetworkParsed;
        }

        public override Dictionary<string, HardwareBase> ParseAllLinux(JToken InxiToken)
        {
            var NetworkParsed = new Dictionary<string, HardwareBase>();
            Network Network;
            var NetworkCycled = default(bool);

            // Network information fields
            string NetName = "";
            string NetDriver = "";
            string NetDriverVersion = "";
            string NetDuplex = "";
            string NetSpeed = "";
            string NetState = "";
            string NetMacAddress = "";
            string NetDeviceID = "";
            string NetBusID = "";
            string NetChipID = "";

            InxiTrace.Debug("Selecting the Network token...");
            foreach (var InxiNetwork in InxiToken.SelectTokenKeyEndingWith("Network"))
            {
                // Get information of a network card
                if (InxiNetwork.SelectTokenKeyEndingWith("Device") is not null)
                {
                    NetName = (string)InxiNetwork.SelectTokenKeyEndingWith("Device");
                    if (InxiNetwork.SelectTokenKeyEndingWith("type") is not null & (string)InxiNetwork.SelectTokenKeyEndingWith("type") == "network bridge")
                    {
                        NetDriver = (string)InxiNetwork.SelectTokenKeyEndingWith("driver");
                        NetDriverVersion = (string)InxiNetwork.SelectTokenKeyEndingWith("v");
                        NetworkCycled = true;
                    }
                    else
                    {
                        NetDriver = (string)InxiNetwork.SelectTokenKeyEndingWith("driver");
                        NetDriverVersion = (string)InxiNetwork.SelectTokenKeyEndingWith("v");
                    }
                }
                else if (InxiNetwork.SelectTokenKeyEndingWith("IF") is not null)
                {
                    NetDuplex = (string)InxiNetwork.SelectTokenKeyEndingWith("duplex");
                    NetSpeed = (string)InxiNetwork.SelectTokenKeyEndingWith("speed");
                    NetState = (string)InxiNetwork.SelectTokenKeyEndingWith("state");
                    NetMacAddress = (string)InxiNetwork.SelectTokenKeyEndingWith("mac");
                    NetDeviceID = (string)InxiNetwork.SelectTokenKeyEndingWith("IF");
                    NetBusID = (string)InxiNetwork.SelectTokenKeyEndingWith("bus ID");
                    NetChipID = (string)InxiNetwork.SelectTokenKeyEndingWith("chip ID");
                    NetworkCycled = true; // Ensures that all info is filled.
                }

                // Create instance of network class
                if (NetworkCycled)
                {
                    InxiTrace.Debug("Got information. NetName: {0}, NetDriver: {1}, NetDriverVersion: {2}, NetDuplex: {3}, NetSpeed: {4}, NetState: {5}, NetDeviceID: {6}, NetChipID: {7}, NetBusID: {8}", NetName, NetDriver, NetDriverVersion, NetDuplex, NetSpeed, NetState, NetDeviceID, NetChipID, NetBusID);
                    Network = new Network(NetName, NetDriver, NetDriverVersion, NetDuplex, NetSpeed, NetState, NetMacAddress, NetDeviceID, NetChipID, NetBusID);
                    NetworkParsed.Add(NetName, Network);
                    InxiTrace.Debug("Added {0} to the list of parsed network cards.", NetName);
                    NetName = "";
                    NetDriver = "";
                    NetDriverVersion = "";
                    NetDuplex = "";
                    NetSpeed = "";
                    NetState = "";
                    NetMacAddress = "";
                    NetDeviceID = "";
                    NetChipID = "";
                    NetBusID = "";
                    NetworkCycled = false;
                }
            }

            // Return list of network devices
            return NetworkParsed;
        }

        public override Dictionary<string, HardwareBase> ParseAllMacOS(NSArray SystemProfilerToken)
        {
            var NetworkParsed = new Dictionary<string, HardwareBase>();
            Network Network;

            // Network information fields
            string NetName = "";
            string NetDriver = "";
            string NetDriverVersion = "";
            string NetDuplex = "";
            string NetSpeed;
            string NetState = "";
            string NetMacAddress;
            string NetDeviceID;
            string NetBusID = "";
            string NetChipID = "";

            // TODO: Name, Driver, DriverVersion, Bus ID, Chip ID, and State not implemented in macOS.
            // Check for data type
            InxiTrace.Debug("Checking for data type...");
            InxiTrace.Debug("TODO: Name, Driver, DriverVersion, Bus ID, Chip ID, and State not implemented in macOS.");
            foreach (NSDictionary DataType in SystemProfilerToken)
            {
                if ((string)DataType["_dataType"].ToObject() == "SPNetworkDataType")
                {
                    InxiTrace.Debug("DataType found: SPNetworkDataType...");

                    // Get information of a network adapter
                    NSArray NetEnum = (NSArray)DataType["_items"];
                    InxiTrace.Debug("Enumerating network cards...");
                    foreach (NSDictionary NetDict in NetEnum)
                    {
                        NSDictionary EthernetDict = (NSDictionary)NetDict["Ethernet"];
                        NSArray EthernetMediaOptions = (NSArray)EthernetDict["MediaOptions"];
                        foreach (NSObject MediaOption in EthernetMediaOptions)
                            NetDuplex += MediaOption.ToObject();
                        NetSpeed = (string)EthernetDict["MediaSubType"].ToObject();
                        NetMacAddress = (string)EthernetDict["MAC Address"].ToObject();
                        NetDeviceID = (string)NetDict["interface"].ToObject();
                        InxiTrace.Debug("Got information. NetName: {0}, NetDriver: {1}, NetDriverVersion: {2}, NetDuplex: {3}, NetSpeed: {4}, NetState: {5}, NetDeviceID: {6}, NetChipID: {7}, NetBusID: {8}", NetName, NetDriver, NetDriverVersion, NetDuplex, NetSpeed, NetState, NetDeviceID, NetChipID, NetBusID);

                        // Create instance of network class
                        Network = new Network(NetName, NetDriver, NetDriverVersion, NetDuplex, NetSpeed, NetState, NetMacAddress, NetDeviceID, NetChipID, NetBusID);
                        NetworkParsed.Add(NetName, Network);
                        InxiTrace.Debug("Added {0} to the list of parsed network cards.", NetName);
                        NetDuplex = "";
                        NetSpeed = "";
                        NetMacAddress = "";
                        NetDeviceID = "";
                        NetChipID = "";
                        NetBusID = "";
                    }
                }
            }

            // Return list of network devices
            return NetworkParsed;
        }

        public override Dictionary<string, HardwareBase> ParseAllWindows(ManagementObjectSearcher WMISearcher)
        {
            var NetworkParsed = new Dictionary<string, HardwareBase>();
            var Networks = WMISearcher;
            Network Network;

            // Network information fields
            string NetName;
            string NetDriver;
            string NetDriverVersion = "";
            string NetDuplex = "";
            string NetSpeed;
            string NetState;
            string NetMacAddress;
            string NetDeviceID;
            string NetBusID = "";
            string NetChipID;

            InxiTrace.Debug("Selecting entries from Win32_PnPSignedDriver with device class of 'NET'...");
            var NetworkDrivers = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceClass='NET'");

            // TODO: Network driver duplex and bus ID not implemented in Windows
            // Get information of network cards
            InxiTrace.Debug("Getting the base objects...");
            InxiTrace.Debug("TODO: Network driver duplex and bus ID not implemented in Windows");
            foreach (ManagementBaseObject Networking in Networks.Get())
            {
                // Get information of a network card
                NetName = (string)Networking["Name"];
                NetDriver = (string)Networking["ServiceName"];
                NetSpeed = Convert.ToString(Networking["Speed"]);
                NetState = Convert.ToString(Networking["NetConnectionStatus"]);
                NetMacAddress = (string)Networking["MACAddress"];
                NetDeviceID = (string)Networking["DeviceID"];
                NetChipID = (string)Networking["PNPDeviceID"];
                foreach (ManagementBaseObject NetworkDriver in NetworkDrivers.Get())
                {
                    if ((string)NetworkDriver["Description"] == NetName)
                    {
                        NetDriverVersion = (string)NetworkDriver["DriverVersion"];
                        break;
                    }
                }
                InxiTrace.Debug("Got information. NetName: {0}, NetDriver: {1}, NetDriverVersion: {2}, NetDuplex: {3}, NetSpeed: {4}, NetState: {5}, NetDeviceID: {6}, NetChipID: {7}, NetBusID: {8}", NetName, NetDriver, NetDriverVersion, NetDuplex, NetSpeed, NetState, NetDeviceID, NetChipID, NetBusID);

                // Create instance of network class
                Network = new Network(NetName, NetDriver, NetDriverVersion, NetDuplex, NetSpeed, NetState, NetMacAddress, NetDeviceID, NetChipID, NetBusID);
                NetworkParsed.AddIfNotFound(NetName, Network);
                InxiTrace.Debug("Added {0} to the list of parsed network cards.", NetName);
            }

            // Return list of network devices
            return NetworkParsed;
        }

    }
}