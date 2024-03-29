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

namespace InxiFrontend
{
    /// <summary>
    /// Sound class
    /// </summary>
    public class Sound : HardwareBase
    {

        /// <summary>
        /// Name of sound card
        /// </summary>
        public override string Name { get; }
        /// <summary>
        /// The maker of sound card
        /// </summary>
        public string Vendor { get; private set; }
        /// <summary>
        /// Driver software used for sound card
        /// </summary>
        public string Driver { get; private set; }
        /// <summary>
        /// Device chip ID
        /// </summary>
        public string ChipID { get; private set; }
        /// <summary>
        /// Device bus ID
        /// </summary>
        public string BusID { get; private set; }

        /// <summary>
        /// Installs specified values parsed by Inxi to the class
        /// </summary>
        internal Sound(string Name, string Vendor, string Driver, string ChipID, string BusID)
        {
            this.Name = Name;
            this.Vendor = Vendor;
            this.Driver = Driver;
            this.ChipID = ChipID;
            this.BusID = BusID;
        }

    }
}