
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
using System.Linq;
using UnameNET;

namespace InxiFrontend
{

    static class InxiInternalUtils
    {

        /// <summary>
        /// Is the platform Unix?
        /// </summary>
        internal static bool IsUnix() =>
            Environment.OSVersion.Platform == PlatformID.Unix;

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
                return false;
        }

        internal static JToken GetTokenFromInxiToken(string name, JToken InxiToken)
        {
            foreach (var token in InxiToken)
                foreach (var token1 in token)
                    if (token1.Path.Contains(name))
                        return token1;
            return null;
        }

        internal static string ReplaceAllRange(this string Str, string[] ToBeReplaced, string[] ToReplace)
        {
            if (Str is null)
                throw new ArgumentNullException(nameof(Str));
            if (ToBeReplaced is null)
                throw new ArgumentNullException(nameof(ToBeReplaced));
            if (ToBeReplaced.Length == 0)
                throw new ArgumentNullException(nameof(ToBeReplaced));
            if (ToReplace is null)
                throw new ArgumentNullException(nameof(ToReplace));
            if (ToReplace.Length == 0)
                throw new ArgumentNullException(nameof(ToReplace));
            if (ToBeReplaced.Length != ToBeReplaced.Length)
                throw new ArgumentException("Array length of which strings to be replaced doesn't equal the array length of which strings to replace.");
            for (int i = 0, loopTo = ToBeReplaced.Length - 1; i <= loopTo; i++)
                Str = Str.Replace(ToBeReplaced[i], ToReplace[i]);
            return Str;
        }

        internal static string[] SplitNewLines(this string Str) =>
            Str.Replace(Convert.ToChar(13).ToString(), "").Split(Convert.ToChar(10));

        internal static string GetPropertyNameEndingWith(this JToken Token, string Containing)
        {
            foreach (JProperty TokenProperty in Token.Cast<JProperty>())
                if (TokenProperty.Name.EndsWith(Containing))
                    return TokenProperty.Name;
            return "";
        }

        internal static string GetPropertyNameContaining(this JToken Token, string Containing)
        {
            foreach (JProperty TokenProperty in Token.Cast<JProperty>())
                if (TokenProperty.Name.Contains(Containing))
                    return TokenProperty.Name;
            return "";
        }

        internal static JToken SelectTokenKeyEndingWith(this JToken Token, string Containing)
        {
            string PropertyName = Token.GetPropertyNameEndingWith(Containing);
            if (!string.IsNullOrEmpty(PropertyName))
                return Token.SelectToken("['" + PropertyName.ReplaceAllRange(new[] { @"\", "/", "'", "\"" }, new[] { @"\\", @"\/", @"\'", @"\""" }) + "']");
            else
                return null;
        }

        internal static JToken SelectTokenKeyContaining(this JToken Token, string Containing)
        {
            string PropertyName = Token.GetPropertyNameContaining(Containing);
            if (!string.IsNullOrEmpty(PropertyName))
                return Token.SelectToken("['" + PropertyName.ReplaceAllRange(new[] { @"\", "/", "'", "\"" }, new[] { @"\\", @"\/", @"\'", @"\""" }) + "']");
            else
                return null;
        }

    }
}