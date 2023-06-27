
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
using System.Diagnostics;
using System.Management;
using System.Text;
using Claunia.PropertyList;
using Extensification.StringExts;
using Newtonsoft.Json.Linq;

namespace InxiFrontend
{
    /// <summary>
    /// Hardware information class
    /// </summary>
    public class HardwareInfo
    {

        /// <summary>
        /// List of hard drives detected
        /// </summary>
        public readonly Dictionary<string, HardDrive> HDD = new();
        /// <summary>
        /// List of logical hard drive partitions detected
        /// </summary>
        public readonly Dictionary<string, WindowsLogicalPartition> LogicalParts = new();
        /// <summary>
        /// List of processors detected
        /// </summary>
        public readonly Dictionary<string, Processor> CPU = new();
        /// <summary>
        /// List of graphics cards detected
        /// </summary>
        public readonly Dictionary<string, Graphics> GPU = new();
        /// <summary>
        /// List of sound cards detected
        /// </summary>
        public readonly Dictionary<string, Sound> Sound = new();
        /// <summary>
        /// List of network cards detected
        /// </summary>
        public readonly Dictionary<string, Network> Network = new();
        /// <summary>
        /// List of batteries detected
        /// </summary>
        public readonly List<Battery> Battery = new();
        /// <summary>
        /// System information
        /// </summary>
        public readonly SystemInfo System;
        /// <summary>
        /// Machine information
        /// </summary>
        public readonly MachineInfo Machine;
        /// <summary>
        /// BIOS information
        /// </summary>
        public readonly BIOS BIOS;
        /// <summary>
        /// RAM information
        /// </summary>
        public readonly PCMemory RAM;

        /// <summary>
        /// Inxi token used for hardware probe
        /// </summary>
        internal JToken InxiToken;
        /// <summary>
        /// SystemProfiler token used for hardware probe
        /// </summary>
        internal NSArray SystemProfilerToken;

        /// <summary>
        /// Initializes a new instance of hardware info
        /// </summary>
        public HardwareInfo(string InxiPath, InxiHardwareType ParseFlags)
        {
            if (InxiInternalUtils.IsUnix())
            {
                if (InxiInternalUtils.IsMacOS())
                {
                    // Start the SystemProfiler process
                    var SystemProfilerProcess = new Process();
                    var SystemProfilerProcessInfo = new ProcessStartInfo()
                    {
                        FileName = "/usr/sbin/system_profiler",
                        Arguments = "SPSoftwareDataType SPAudioDataType SPHardwareDataType SPNetworkDataType SPStorageDataType SPDisplaysDataType -xml",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true
                    };
                    SystemProfilerProcess.StartInfo = SystemProfilerProcessInfo;
                    InxiTrace.Debug("Starting system_profiler with \"SPSoftwareDataType SPAudioDataType SPHardwareDataType SPNetworkDataType SPStorageDataType SPDisplaysDataType -xml\"...");
                    SystemProfilerProcess.Start();
                    SystemProfilerProcess.WaitForExit(10000);
                    SystemProfilerToken = (NSArray)PropertyListParser.Parse(Encoding.Default.GetBytes(SystemProfilerProcess.StandardOutput.ReadToEnd()));
                    InxiTrace.Debug("Token parsed.");
                }
                else
                {
                    // Start the Inxi process
                    var InxiProcess = new Process();
                    var InxiProcessInfo = new ProcessStartInfo()
                    {
                        FileName = InxiPath,
                        Arguments = "-Fxx --output json --output-file print",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true
                    };
                    InxiProcess.StartInfo = InxiProcessInfo;
                    InxiTrace.Debug("Starting inxi with \"-Fxx --output json --output-file print\"...");
                    InxiProcess.Start();
                    InxiProcess.WaitForExit();
                    var lines = InxiProcess.StandardOutput.ReadToEnd().SplitNewLines();
                    InxiToken = JToken.Parse(lines[lines.Length - 1]);
                    InxiTrace.Debug("Token parsed.");
                }
            }

            // Ready variables
            var HDDParsed = new Dictionary<string, HardwareBase>();
            var Logicals = new Dictionary<string, WindowsLogicalPartition>();
            var CPUParsed = new Dictionary<string, HardwareBase>();
            var GPUParsed = new Dictionary<string, HardwareBase>();
            var SoundParsed = new Dictionary<string, HardwareBase>();
            var NetParsed = new Dictionary<string, HardwareBase>();
            var BatteryParsed = new List<HardwareBase>();
            var RAMParsed = default(PCMemory);
            var BIOSParsed = default(BIOS);
            var SystemParsed = default(SystemInfo);
            var MachineParsed = default(MachineInfo);

            // Parse hardware starting from HDD
            if (ParseFlags.HasFlag(InxiHardwareType.HardDrive))
            {
                InxiTrace.Debug("Parsing HDD...");
                var BaseParser = new HardDriveParser();
                HDDParsed = BaseParser.ParseAll(InxiToken, SystemProfilerToken);
                InxiTrace.RaiseParsedEvent(InxiHardwareType.HardDrive);
            }

            // Logical partitions
            if (!InxiInternalUtils.IsUnix() && ParseFlags.HasFlag(InxiHardwareType.HardDriveLogical))
            {
                InxiTrace.Debug("Parsing logical partitions...");
                Logicals = WindowsLogicalPartitionParser.ParsePartitions(new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk"));
                InxiTrace.RaiseParsedEvent(InxiHardwareType.HardDriveLogical);
            }

            // Processor
            if (ParseFlags.HasFlag(InxiHardwareType.Processor))
            {
                InxiTrace.Debug("Parsing CPU...");
                var BaseParser = new ProcessorParser();
                CPUParsed = BaseParser.ParseAll(InxiToken, SystemProfilerToken);
                InxiTrace.RaiseParsedEvent(InxiHardwareType.Processor);
            }

            // Graphics
            if (ParseFlags.HasFlag(InxiHardwareType.Graphics))
            {
                InxiTrace.Debug("Parsing GPU...");
                var BaseParser = new GraphicsParser();
                GPUParsed = BaseParser.ParseAll(InxiToken, SystemProfilerToken);
                InxiTrace.RaiseParsedEvent(InxiHardwareType.Graphics);
            }

            // Sound
            if (ParseFlags.HasFlag(InxiHardwareType.Sound))
            {
                InxiTrace.Debug("Parsing sound...");
                var BaseParser = new SoundParser();
                SoundParsed = BaseParser.ParseAll(InxiToken, SystemProfilerToken);
                InxiTrace.RaiseParsedEvent(InxiHardwareType.Sound);
            }

            // Network
            if (ParseFlags.HasFlag(InxiHardwareType.Network))
            {
                InxiTrace.Debug("Parsing network...");
                var BaseParser = new NetworkParser();
                NetParsed = BaseParser.ParseAll(InxiToken, SystemProfilerToken);
                InxiTrace.RaiseParsedEvent(InxiHardwareType.Network);
            }

            // PC Memory
            if (ParseFlags.HasFlag(InxiHardwareType.PCMemory))
            {
                InxiTrace.Debug("Parsing RAM...");
                var BaseParser = new PCMemoryParser();
                RAMParsed = (PCMemory)BaseParser.Parse(InxiToken, SystemProfilerToken);
                InxiTrace.RaiseParsedEvent(InxiHardwareType.PCMemory);
            }

            // BIOS
            if (ParseFlags.HasFlag(InxiHardwareType.BIOS))
            {
                InxiTrace.Debug("Parsing BIOS...");
                var BaseParser = new BIOSParser();
                BIOSParsed = (BIOS)BaseParser.Parse(InxiToken, SystemProfilerToken);
                InxiTrace.RaiseParsedEvent(InxiHardwareType.BIOS);
            }

            // System
            if (ParseFlags.HasFlag(InxiHardwareType.System))
            {
                InxiTrace.Debug("Parsing system...");
                var BaseParser = new SystemParser();
                SystemParsed = (SystemInfo)BaseParser.Parse(InxiToken, SystemProfilerToken);
                InxiTrace.RaiseParsedEvent(InxiHardwareType.System);
            }

            // Machine
            if (ParseFlags.HasFlag(InxiHardwareType.Machine))
            {
                InxiTrace.Debug("Parsing machine...");
                var BaseParser = new MachineParser();
                MachineParsed = (MachineInfo)BaseParser.Parse(InxiToken, SystemProfilerToken);
                InxiTrace.RaiseParsedEvent(InxiHardwareType.Machine);
            }

            // Battery
            if (ParseFlags.HasFlag(InxiHardwareType.Battery))
            {
                InxiTrace.Debug("Parsing battery...");
                var BaseParser = new BatteryParser();
                BatteryParsed = BaseParser.ParseAllToList(InxiToken, SystemProfilerToken);
                InxiTrace.RaiseParsedEvent(InxiHardwareType.Battery);
            }

            // Add the base to the correct type
            var HDDProcessed = new Dictionary<string, HardDrive>();
            var CPUProcessed = new Dictionary<string, Processor>();
            var GPUProcessed = new Dictionary<string, Graphics>();
            var SoundProcessed = new Dictionary<string, Sound>();
            var NetProcessed = new Dictionary<string, Network>();
            var BatteryProcessed = new List<Battery>();

            // Hard drive
            foreach (string Parsed in HDDParsed.Keys)
            {
                HardDrive FinalHardware = (HardDrive)HDDParsed[Parsed];
                HDDProcessed.Add(Parsed, FinalHardware);
            }

            // Processor
            foreach (string Parsed in CPUParsed.Keys)
            {
                Processor FinalHardware = (Processor)CPUParsed[Parsed];
                CPUProcessed.Add(Parsed, FinalHardware);
            }

            // Graphics
            foreach (string Parsed in GPUParsed.Keys)
            {
                Graphics FinalHardware = (Graphics)GPUParsed[Parsed];
                GPUProcessed.Add(Parsed, FinalHardware);
            }

            // Sound
            foreach (string Parsed in SoundParsed.Keys)
            {
                Sound FinalHardware = (Sound)SoundParsed[Parsed];
                SoundProcessed.Add(Parsed, FinalHardware);
            }

            // Network
            foreach (string Parsed in NetParsed.Keys)
            {
                Network FinalHardware = (Network)NetParsed[Parsed];
                NetProcessed.Add(Parsed, FinalHardware);
            }

            // Battery
            foreach (Battery Parsed in BatteryParsed)
                BatteryProcessed.Add(Parsed);

            // Install parsed information to current instance
            HDD = HDDProcessed;
            if (!InxiInternalUtils.IsUnix())
                LogicalParts = Logicals;
            CPU = CPUProcessed;
            GPU = GPUProcessed;
            Sound = SoundProcessed;
            Network = NetProcessed;
            Battery = BatteryProcessed;
            RAM = RAMParsed;
            BIOS = BIOSParsed;
            System = SystemParsed;
            Machine = MachineParsed;
            InxiTrace.Debug("Parsed information installed.");
        }

    }
}