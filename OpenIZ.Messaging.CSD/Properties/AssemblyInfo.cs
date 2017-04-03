﻿/*
 * Copyright 2015-2017 Mohawk College of Applied Arts and Technology
 *
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: Nityan
 * Date: 2017-3-31
 */
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MARC.HI.EHRS.SVC.Core.Plugins;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("OpenIZ CSD Messaging")]
[assembly: AssemblyDescription("Care Services Discovery (CSD) allows access to OpenIZ CSD services")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Mohawk College of Applied Arts and Technology")]
[assembly: AssemblyProduct("Open Immunize (http://openiz.org)")]
[assembly: AssemblyCopyright("Copyright (C) 2017, Mohawk College of Applied Arts and Technology")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("46111047-55ca-401b-99d0-dd42f8657c1b")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.6.14.*")]
[assembly: AssemblyInformationalVersion("Chippewa")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Assembly plugin info
[assembly: AssemblyPlugin()]

// Depends: OpenIZ.Core v1.0.0.0
[assembly: AssemblyPluginDependency("OpenIZ.Core", "1.0.0.0")]