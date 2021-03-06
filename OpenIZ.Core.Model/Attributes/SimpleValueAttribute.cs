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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Core.Model.Attributes
{
    /// <summary>
    /// Identifies the simple value
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SimpleValueAttribute : Attribute
    {

        /// <summary>
        /// Classifier attribute property
        /// </summary>
        /// <param name="valueProperty"></param>
        public SimpleValueAttribute(String valueProperty)
        {
            this.ValueProperty = valueProperty;
        }

        /// <summary>
        /// Gets or sets the classifier property
        /// </summary>
        public String ValueProperty { get; set; }
    }
}
