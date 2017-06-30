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
 * User: justi
 * Date: 2016-8-2
 */
using OpenIZ.OrmLite;
using OpenIZ.OrmLite.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Persistence.Data.ADO.Data.Model
{
    /// <summary>
    /// Represents identified data
    /// </summary>

    public interface IDbIdentified
    {
        /// <summary>
        /// Gets or sets the key of the object
        /// </summary>
        Guid Key { get; set; }
    }

    /// <summary>
    /// Gets or sets the identified data
    /// </summary>
    public abstract class DbIdentified : IAdoLoadedData, IDbIdentified
    {
        /// <summary>
        /// Create database identified
        /// </summary>
        public DataContext Context { get; set; }

        /// <summary>
        /// Gets or sets the key of the object
        /// </summary>
        [AutoGenerated]
        public abstract Guid Key { get; set; }
    }

    
}
