
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
using Claunia.PropertyList;
using Newtonsoft.Json.Linq;

namespace InxiFrontend
{
    /// <summary>
    /// Interface for hardware parser
    /// </summary>
    public interface IHardwareParser
    {

        /// <summary>
        /// The base hardware parser
        /// </summary>
        HardwareBase Parse(JToken InxiToken, NSArray SystemProfilerToken);

        /// <summary>
        /// The base hardware parser
        /// </summary>
        Dictionary<string, HardwareBase> ParseAll(JToken InxiToken, NSArray SystemProfilerToken);

        /// <summary>
        /// The base hardware parser
        /// </summary>
        List<HardwareBase> ParseAllToList(JToken InxiToken, NSArray SystemProfilerToken);

    }
}