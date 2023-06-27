
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

    class HardDriveParser : HardwareParserBase, IHardwareParser
    {

        /// <summary>
        /// Parses hard drives
        /// </summary>
        /// <param name="InxiToken">Inxi JSON token. Ignored in Windows.</param>
        /// <param name="SystemProfilerToken">system_profiler token</param>
        public override Dictionary<string, HardwareBase> ParseAll(JToken InxiToken, NSArray SystemProfilerToken)
        {
            // Variables
            Dictionary<string, HardwareBase> HDDParsed;

            // If the system is Unix, use Inxi. If on Windows, use WMI. If on macOS, use system_profiler.
            if (InxiInternalUtils.IsUnix())
            {
                HDDParsed = ParseAllLinux(InxiToken);
            }
            else // on Windows
            {
                var HardDisks = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                HDDParsed = ParseAllWindows(HardDisks);
            }

            return HDDParsed;
        }

        public override Dictionary<string, HardwareBase> ParseAllLinux(JToken InxiToken)
        {
            // Variables
            var HDDParsed = new Dictionary<string, HardwareBase>();
            var DriveParts = new Dictionary<string, Partition>();
            HardDrive Drive;
            Partition DrivePart;
            bool InxiDriveReady = false;

            // Enumerate each drive
            InxiTrace.Debug("Selecting the Drives token...");
            var DrvDevChar1 = default(char);
            var CurrDrvChar1 = default(char);
            foreach (var InxiDrive in InxiToken.SelectTokenKeyEndingWith("Drives"))
            {
                if (InxiDriveReady)
                {
                    // Get information of a drive
                    string DriveID = (string)InxiDrive.SelectTokenKeyEndingWith("ID");
                    string DriveSize = (string)InxiDrive.SelectTokenKeyEndingWith("size");
                    string DriveModel = (string)InxiDrive.SelectTokenKeyEndingWith("model");
                    string DriveVendor = (string)InxiDrive.SelectTokenKeyEndingWith("vendor");
                    string DriveSerial = (string)InxiDrive.SelectTokenKeyEndingWith("serial");
                    string DriveSpeed = (string)InxiDrive.SelectTokenKeyEndingWith("speed");
                    if (string.IsNullOrEmpty(DriveVendor))
                    {
                        DriveSize = (string)InxiDrive.SelectTokenKeyEndingWith("size");
                        DriveModel = (string)InxiDrive.SelectTokenKeyEndingWith("model");
                    }
                    InxiTrace.Debug("Got information. DriveSize: {0}, DriveModel: {1}, DriveVendor: {2}, DriveSerial: {3}, DriveSpeed: {4}, DriveID: {5}", DriveSize, DriveModel, DriveVendor, DriveSerial, DriveSpeed, DriveID);

                    // Get partitions
                    InxiTrace.Debug("Selecting the Partition token...");
                    var DrivePartToken = InxiToken.SelectTokenKeyEndingWith("Partition");
                    if (DrivePartToken is not null)
                    {
                        foreach (var DrivePartition in DrivePartToken)
                        {
                            if (DrivePartition.SelectTokenKeyEndingWith("dev") is not null)
                            {
                                string DrvDevPath = DrivePartition.SelectTokenKeyEndingWith("dev").ToString();
                                string TarDevPath = InxiDrive.SelectTokenKeyEndingWith("ID").ToString();
                                string PartitionFilesystem = (string)DrivePartition.SelectTokenKeyEndingWith("fs");
                                string PartitionSize = (string)DrivePartition.SelectTokenKeyEndingWith("size");
                                string PartitionUsed = (string)DrivePartition.SelectTokenKeyEndingWith("used");

                                if (DrvDevPath.Contains("hd") | DrvDevPath.Contains("sd") | DrvDevPath.Contains("vd")) // /dev/hdX, /dev/sdX, /dev/vdX
                                {
                                    CurrDrvChar1 = DrvDevPath.Replace("/dev/sd", "").Replace("/dev/hd", "").Replace("/dev/vd", "")[0];
                                    DrvDevChar1 = TarDevPath.Replace("/dev/sd", "").Replace("/dev/hd", "").Replace("/dev/vd", "")[0];
                                }
                                else if (DrvDevPath.Contains("mmcblk")) // /dev/mmcblkXpY
                                {
                                    CurrDrvChar1 = DrvDevPath.Replace("/dev/mmcblk", "")[0];
                                    DrvDevChar1 = TarDevPath.Replace("/dev/mmcblk", "")[0];
                                }
                                else if (DrvDevPath.Contains("nvme")) // /dev/nvmeXnY
                                {
                                    CurrDrvChar1 = DrvDevPath.Replace("/dev/nvme", "")[0];
                                    DrvDevChar1 = TarDevPath.Replace("/dev/nvme", "")[0];
                                }
                                InxiTrace.Debug("Got information. DrvDevPath: {0}, TarDevPath: {1}, DrvDevChar: {2}, CurrDrvChar: {3}, PartitionFilesystem: {4}, PartitionSize: {5}, PartitionUsed: {6}", DrvDevPath, TarDevPath, DrvDevChar1, CurrDrvChar1, PartitionFilesystem, PartitionSize, PartitionUsed);

                                if (CurrDrvChar1 == DrvDevChar1)
                                {
                                    DrivePart = new Partition(DrvDevPath, (string)DrivePartition.SelectTokenKeyEndingWith("fs"), (string)DrivePartition.SelectTokenKeyEndingWith("size"), (string)DrivePartition.SelectTokenKeyEndingWith("used"));
                                    DriveParts.Add(DrvDevPath, DrivePart);
                                    InxiTrace.Debug("Added {0} to the list of {1}'s partitions.", DrvDevPath, TarDevPath);
                                }
                            }
                        }
                    }

                    // Create an instance of hard drive class
                    Drive = new HardDrive(DriveID, DriveSize, DriveModel, DriveVendor, DriveSpeed, DriveSerial, DriveParts);
                    HDDParsed.Add(DriveID, Drive);
                    InxiTrace.Debug("Added {0} to the list of parsed drives.", DriveID);
                }
                else
                    InxiDriveReady = true;
            }
            return HDDParsed;
        }

        public override Dictionary<string, HardwareBase> ParseAllWindows(ManagementObjectSearcher WMISearcher)
        {
            // Variables
            var HDDParsed = new Dictionary<string, HardwareBase>();
            var DriveParts = new Dictionary<string, Partition>();
            HardDrive Drive;
            Partition DrivePart;
            InxiTrace.Debug("Selecting entries from Win32_DiskDrive...");
            var HardDisks = WMISearcher;
            InxiTrace.Debug("Selecting entries from Win32_DiskPartition...");
            var DiskPartitions = new ManagementObjectSearcher("SELECT * FROM Win32_DiskPartition");

            // TODO: Used not implemented in Windows
            // HDD Prober
            InxiTrace.Debug("Getting the base objects...");
            InxiTrace.Debug("TODO: Used not implemented in Windows");
            int DriveNo;
            foreach (ManagementBaseObject Hdd in HardDisks.Get())
            {
                try
                {
                    int DiskIndexHdd = Convert.ToInt32(Hdd["Index"]);
                    string DeviceID = (string)Hdd["DeviceID"];
                    string DeviceSize = Convert.ToString(Hdd["Size"]);
                    string DeviceModel = (string)Hdd["Model"];
                    string DeviceManufacturer = (string)Hdd["Manufacturer"];
                    string DeviceSerialNumber = (string)Hdd["SerialNumber"];
                    InxiTrace.Debug("Got information. DiskIndexHdd: {0}, DeviceID: {1}, DeviceSize: {2}, DeviceModel: {3}, DeviceManufacturer: {4}", DiskIndexHdd, DeviceID, DeviceSize, DeviceModel, DeviceManufacturer);
                    InxiTrace.Debug("Getting the partiton base objects...");
                    foreach (ManagementBaseObject Manage in DiskPartitions.Get())
                    {
                        try
                        {
                            string PartitionDeviceID = (string)Manage["DeviceID"];
                            string PartitionFilesystem = (string)Manage["Type"];
                            string PartitionSize = Convert.ToString(Manage["Size"]);
                            string PartitionIndex = Convert.ToString(Manage["Index"]);
                            DriveNo = Convert.ToInt32(Manage["DiskIndex"]);
                            InxiTrace.Debug("Got information. PartitionDeviceID: {0}, PartitionFilesystem: {1}, PartitionSize: {2}, PartitionIndex: {3}, DriveNo: {4}", PartitionDeviceID, PartitionFilesystem, PartitionSize, PartitionIndex, (object)DriveNo);

                            if (DiskIndexHdd == DriveNo)
                            {
                                DrivePart = new Partition(PartitionDeviceID, PartitionFilesystem, PartitionSize, "0");
                                DriveParts.Add($"Physical partition in {Hdd["Model"]} ({DiskIndexHdd}) : {PartitionIndex}", DrivePart);
                                InxiTrace.Debug("Added partition {0} to the list of drive {1}'s partitions.", PartitionIndex, DiskIndexHdd);
                            }
                        }
                        catch (Exception ex)
                        {
                            InxiTrace.Debug("Error: {0}", ex.Message);
                            continue;
                        }
                    }

                    // TODO: Speed not implemented in Windows
                    InxiTrace.Debug("TODO: Speed not implemented in Windows");
                    Drive = new HardDrive(DeviceID, DeviceSize, DeviceModel, DeviceManufacturer, "", DeviceSerialNumber, DriveParts);
                    HDDParsed.Add(DeviceModel + " (" + DiskIndexHdd + ")", Drive);
                    InxiTrace.Debug("Added {0} to the list of parsed drives.", DeviceModel);
                }
                catch (Exception ex)
                {
                    InxiTrace.Debug("Error: {0}", ex.Message);
                    continue;
                }
            }
            return HDDParsed;
        }

    }
}