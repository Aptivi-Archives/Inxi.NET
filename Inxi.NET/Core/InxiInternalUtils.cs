
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

using Newtonsoft.Json.Linq;
using System;
using UnameNET;

namespace InxiFrontend
{

    static class InxiInternalUtils
    {

        /// <summary>
        /// Is the platform Unix?
        /// </summary>
        internal static bool IsUnix()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix;
        }

        /// <summary>
        /// Is the Unix platform macOS?
        /// </summary>
        internal static bool IsMacOS()
        {
            if (IsUnix())
            {
                string System = UnameManager.GetUname(UnameTypes.KernelName);
                InxiTrace.Debug("Searching {0} for \"Darwin\"...", System.Replace(Environment.NewLine, ""));
                return System.Contains("Darwin");
            }
            else
            {
                return false;
            }
        }

        internal static JToken GetTokenFromInxiToken(string name, JToken InxiToken)
        {
            foreach (var token in InxiToken)
                foreach (var token1 in token)
                    if (token1.Path.Contains(name))
                        return token1;
            return null;
        }

    }
}