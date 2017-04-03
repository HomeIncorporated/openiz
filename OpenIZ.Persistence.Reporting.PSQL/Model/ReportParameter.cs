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
 * Date: 2017-4-1
 */

using System;

namespace OpenIZ.Persistence.Reporting.PSQL.Model
{
	/// <summary>
	/// Represents a report parameter.
	/// </summary>
	public class ReportParameter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ReportParameter"/> class.
		/// </summary>
		public ReportParameter()
		{
			this.CreationTime = DateTimeOffset.UtcNow;
			this.Id = Guid.NewGuid();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ReportParameter"/> class
		/// with a specific <see cref="Core.Model.RISI.ReportParameter"/> instance.
		/// </summary>
		/// <param name="reportParameter">The report parameter instance.</param>
		public ReportParameter(Core.Model.RISI.ReportParameter reportParameter) : this()
		{
			this.Id = reportParameter.Key.Value;
			this.IsNullable = reportParameter.IsNullable;
			this.Name = reportParameter.Name;
			this.ParameterTypeId = reportParameter.ParameterType.Key.Value;
			this.Value = reportParameter.Value;
		}

		/// <summary>
		/// Gets or sets the creation time of the parameter.
		/// </summary>
		public DateTimeOffset CreationTime { get; set; }

		/// <summary>
		/// Gets or sets the description of the report parameter.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the id of the parameter.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Gets or sets whether the report parameter is nullable.
		/// </summary>
		public bool IsNullable { get; set; }

		/// <summary>
		/// Gets or sets the name of the report parameter.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the order of the report parameter.
		/// </summary>
		public int Order { get; set; }

		/// <summary>
		/// Gets or sets the id of the parameter type associated with the report parameter.
		/// </summary>
		public Guid ParameterTypeId { get; set; }

		/// <summary>
		/// Gets or sets the report id associated with the report parameter.
		/// </summary>
		public Guid ReportId { get; set; }

		/// <summary>
		/// Get or set the value of the report parameter.
		/// </summary>
		public byte[] Value { get; set; }
	}
}