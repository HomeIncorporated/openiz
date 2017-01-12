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
 * Date: 2017-1-9
 */
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Migrations.Utilities;

namespace OpenIZ.Persistence.Reporting.Migrations
{
	/// <summary>
	/// Represents a SQL migration generator for SQL Server.
	/// </summary>
	internal sealed class MigrationGenerator : SqlServerMigrationSqlGenerator
	{
		protected override void Generate(AddColumnOperation addColumnOperation)
		{
			SetCreatedUtcColumn(addColumnOperation.Column);

			base.Generate(addColumnOperation);
		}

		protected override void Generate(CreateTableOperation createTableOperation)
		{
			SetCreatedUtcColumn(createTableOperation.Columns);

			base.Generate(createTableOperation);
		}

		private static void SetCreatedUtcColumn(IEnumerable<ColumnModel> columns)
		{
			foreach (var columnModel in columns)
			{
				SetCreatedUtcColumn(columnModel);
			}
		}

		private static void SetCreatedUtcColumn(PropertyModel column)
		{
			if (column.Name == "CreationTime")
			{
				column.DefaultValueSql = "GETUTCDATE()";
			}
		}
	}
}