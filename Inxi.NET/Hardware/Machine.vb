﻿
'    Inxi.NET  Copyright (C) 2020-2021  EoflaOE
'
'    This file is part of Inxi.NET
'
'    Inxi.NET is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    Inxi.NET is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with this program.  If not, see <https://www.gnu.org/licenses/>.

Public Class MachineInfo
    Inherits HardwareBase

    ''' <summary>
    ''' Machine name
    ''' </summary>
    Public Overrides ReadOnly Property Name As String
    ''' <summary>
    ''' Machine type
    ''' </summary>
    Public ReadOnly Property Type As String
    ''' <summary>
    ''' Machine product name
    ''' </summary>
    Public ReadOnly Property Product As String
    ''' <summary>
    ''' Machine system name
    ''' </summary>
    Public ReadOnly Property System As String
    ''' <summary>
    ''' Machine chassis
    ''' </summary>
    Public ReadOnly Property Chassis As String
    ''' <summary>
    ''' Motherboard manufacturer
    ''' </summary>
    Public ReadOnly Property MoboManufacturer As String
    ''' <summary>
    ''' Motherboard model
    ''' </summary>
    Public ReadOnly Property MoboModel As String

    ''' <summary>
    ''' Installs specified values parsed by Inxi to the class
    ''' </summary>
    Friend Sub New(Type As String, Product As String, System As String, Chassis As String, MoboManufacturer As String, MoboModel As String)
        Me.Type = Type
        Me.Product = Product
        Name = Product
        Me.System = System
        Me.Chassis = Chassis
        Me.MoboManufacturer = MoboManufacturer
        Me.MoboModel = MoboModel
    End Sub

End Class