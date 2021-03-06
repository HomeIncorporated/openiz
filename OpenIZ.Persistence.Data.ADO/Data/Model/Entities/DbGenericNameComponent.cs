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
 * Date: 2017-1-14
 */
using OpenIZ.OrmLite.Attributes;
using OpenIZ.Persistence.Data.ADO.Data.Model.Concepts;
using System;

namespace OpenIZ.Persistence.Data.ADO.Data.Model.Entities
{
    /// <summary>
    /// Generic name component
    /// </summary>
    public abstract class DbGenericNameComponent : DbAssociation, IDbIdentified
    {

        /// <summary>
        /// Gets or sets the key of the component
        /// </summary>
        [Column("cmp_id"), PrimaryKey, AutoGenerated]
        public override Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the type of the component
        /// </summary>
        [Column("typ_cd_id"), ForeignKey(typeof(DbConcept), nameof(DbConcept.Key))]
        public Guid? ComponentTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the value key
        /// </summary>
        [Column("val_seq_id")]
        public virtual Decimal ValueSequenceId { get; set; }
    }
}