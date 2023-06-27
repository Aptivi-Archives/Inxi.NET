
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
using Newtonsoft.Json.Linq;

namespace InxiFrontend
{
    /// <summary>
    /// Base for hardware parser
    /// </summary>
    public abstract class HardwareParserBase : IHardwareParser
    {
        /// <summary>
        /// Parses a hardware
        /// </summary>
        /// <param name="InxiToken">Inxi token</param>
        /// <param name="SystemProfilerToken">system_profiler token</param>
        /// <returns><see cref="HardwareBase"/> containing all the information</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual HardwareBase Parse(JToken InxiToken, NSArray SystemProfilerToken)
        {
            throw new NotImplementedException("This hardware parser is not implemented yet!");
        }

        /// <summary>
        /// Parses a list of hardware
        /// </summary>
        /// <param name="InxiToken">Inxi token</param>
        /// <param name="SystemProfilerToken">system_profiler token</param>
        /// <returns>A dictionary containing list of hardware</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Dictionary<string, HardwareBase> ParseAll(JToken InxiToken, NSArray SystemProfilerToken)
        {
            throw new NotImplementedException("This hardware parser is not implemented yet!");
        }

        /// <summary>
        /// Parses a list of hardware
        /// </summary>
        /// <param name="InxiToken">Inxi token</param>
        /// <param name="SystemProfilerToken">system_profiler token</param>
        /// <returns>A list containing list of hardware</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual List<HardwareBase> ParseAllToList(JToken InxiToken, NSArray SystemProfilerToken)
        {
            throw new NotImplementedException("This hardware parser is not implemented yet!");
        }

        /// <summary>
        /// The base Linux hardware parser
        /// </summary>
        public virtual HardwareBase ParseLinux(JToken InxiToken)
        {
            throw new NotImplementedException("This hardware parser is not implemented yet on Linux!");
        }

        /// <summary>
        /// The base Windows hardware parser
        /// </summary>
        public virtual HardwareBase ParseWindows(ManagementObjectSearcher WMISearcher)
        {
            throw new NotImplementedException("This hardware parser is not implemented yet on Windows!");
        }

        /// <summary>
        /// The base Linux hardware parser
        /// </summary>
        public virtual Dictionary<string, HardwareBase> ParseAllLinux(JToken InxiToken)
        {
            throw new NotImplementedException("This hardware parser is not implemented yet on Linux!");
        }

        /// <summary>
        /// The base Windows hardware parser
        /// </summary>
        public virtual Dictionary<string, HardwareBase> ParseAllWindows(ManagementObjectSearcher WMISearcher)
        {
            throw new NotImplementedException("This hardware parser is not implemented yet on Windows!");
        }

        /// <summary>
        /// The base Linux hardware parser
        /// </summary>
        public virtual List<HardwareBase> ParseAllToListLinux(JToken InxiToken)
        {
            throw new NotImplementedException("This hardware parser is not implemented yet on Linux!");
        }

        /// <summary>
        /// The base Windows hardware parser
        /// </summary>
        public virtual List<HardwareBase> ParseAllToListWindows(ManagementObjectSearcher WMISearcher)
        {
            throw new NotImplementedException("This hardware parser is not implemented yet on Windows!");
        }

    }
}