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
 * User: khannan
 * Date: 2017-1-5
 */

using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Core.Data;
using MARC.HI.EHRS.SVC.Core.Services;
using OpenIZ.Core.Model.RISI;
using OpenIZ.Core.Security;
using OpenIZ.Reporting.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Security.Authentication;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OpenIZ.Core.Diagnostics;
using OpenIZ.Core.Security.Attribute;
using OpenIZ.Reporting.Core.Auth;
using OpenIZ.Reporting.Core.Configuration;
using OpenIZ.Reporting.Core.Event;
using OpenIZ.Reporting.Jasper.Model;

namespace OpenIZ.Reporting.Jasper
{
	/// <summary>
	/// Represents a Jasper server report executor.
	/// </summary>
	[Service(ServiceInstantiationType.Instance)]
	public class JasperReportExecutor : IReportExecutor, ISupportBasicAuthentication
	{
		/// <summary>
		/// The internal reference to the <see cref="HttpClient"/> instance.
		/// </summary>
		private readonly HttpClient client;

		/// <summary>
		/// The cookie container.
		/// </summary>
		private readonly CookieContainer cookieContainer;

		/// <summary>
		/// The configuration.
		/// </summary>
		private static readonly ReportingConfiguration configuration = ApplicationContext.Current.GetService<IConfigurationManager>().GetSection("openiz.reporting.core") as ReportingConfiguration;

		/// <summary>
		/// The jasper authentication path.
		/// </summary>
		private const string JasperAuthPath = "/rest/login";

		/// <summary>
		/// The jasper report path.
		/// </summary>
		private static readonly string JasperReportPath = "/rest_v2/reports";

		/// <summary>
		/// The jasper resources path.
		/// </summary>
		private const string JasperResourcesPath = "/rest_v2/resources";

		/// <summary>
		/// The jasper cookie key.
		/// </summary>
		private const string JasperCookieKey = "JSESSIONID";

		/// <summary>
		/// The internal reference to the trace source.
		/// </summary>
		private readonly TraceSource tracer = new TraceSource("OpenIZ.Reporting.Jasper");

		/// <summary>
		/// The username.
		/// </summary>
		private readonly string username;

		/// <summary>
		/// The password.
		/// </summary>
		private readonly string password;

		/// <summary>
		/// Occurs when a service is authenticated.
		/// </summary>
		public event EventHandler<AuthenticatedEventArgs> Authenticated;
		/// <summary>
		/// Occurs when a service is authenticating.
		/// </summary>
		public event EventHandler<AuthenticatingEventArgs> Authenticating;
		/// <summary>
		/// Occurs when a service fails authentication.
		/// </summary>
		public event EventHandler<AuthenticationErrorEventArgs> OnAuthenticationError;

		/// <summary>
		/// Initializes a new instance of the <see cref="JasperReportExecutor" /> class.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">Non username and password authentication methods are not supported for Jasper Reports</exception>
		public JasperReportExecutor()
		{
			this.cookieContainer = new CookieContainer();

			var handler = new HttpClientHandler
			{
				CookieContainer = this.cookieContainer
			};

			this.client = new HttpClient(handler);

			var usernamePasswordCredential = this.Configuration.Credentials.Credential as UsernamePasswordCredential;

			if (usernamePasswordCredential == null)
			{
				throw new InvalidOperationException("Only the username and password authentication mechanism is currently supported for Jasper Reports");
			}

			this.username = usernamePasswordCredential.Username;
			this.password = usernamePasswordCredential.Password;

			this.Authenticated += OnAuthenticated;
			this.ReportUri = new Uri(this.Configuration.Address);
		}

		/// <summary>
		/// Gets or sets the authentication result of the authentication handler.
		/// </summary>
		public AuthenticationResult AuthenticationResult { get; set; }

		/// <summary>
		/// Gets the configuration.
		/// </summary>
		/// <value>The configuration.</value>
		public ReportingConfiguration Configuration => configuration;

		/// <summary>
		/// Gets or sets the report URI.
		/// </summary>
		public Uri ReportUri { get; }

		/// <summary>
		/// Authenticates against a remote system using a username and password.
		/// </summary>
		/// <param name="username">The username of the user.</param>
		/// <param name="password">The password of the user.</param>
		/// <returns>Returns an authentication result.</returns>
		/// <exception cref="System.Security.Authentication.AuthenticationException">Unable to authenticate against the Jasper Reports Service.</exception>
		public AuthenticationResult Authenticate(string username, string password)
		{
			var content = new StringContent($"j_username={username}&j_password={password}");

			content.Headers.Remove("Content-Type");
			content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

			this.Authenticating?.Invoke(this, new AuthenticatingEventArgs());

			tracer.TraceEvent(TraceEventType.Information, 0, "Authenticating against jasper server");

			var response = this.client.PostAsync($"{this.ReportUri}{JasperAuthPath}", content).Result;

			if (response.IsSuccessStatusCode)
			{
				var values = response.Headers.GetValues("Set-Cookie");

				string token = null;

				foreach (var value in values.SelectMany(v => v.Split(';')))
				{
					if (!value.StartsWith(JasperCookieKey + "="))
					{
						continue;
					}

					token = value.Split('=')[1];
					this.tracer.TraceEvent(TraceEventType.Information, 0, "Successfully authenticated against jasper server");
					this.tracer.TraceEvent(TraceEventType.Verbose, 0, token);
					break;
				}

				this.Authenticated?.Invoke(this, new AuthenticatedEventArgs(new AuthenticationResult(token)));
			}
			else
			{
				var message = $"Unable to authenticate against the Jasper Service, using username: {username}";

				this.OnAuthenticationError?.Invoke(this, new AuthenticationErrorEventArgs(message));

				this.tracer.TraceEvent(TraceEventType.Error, 0, message);

				throw new AuthenticationException(message);
			}

			return this.AuthenticationResult;
		}

		/// <summary>
		/// Creates a new report parameter type.
		/// </summary>
		/// <param name="parameterType">The report parameter type to create.</param>
		/// <returns>Returns the created report parameter type.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.UnrestrictedMetadata)]
		public ParameterType CreateParameterType(ParameterType parameterType)
		{
			var persistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<ParameterType>>();

			if (persistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate persistence service: {nameof(IDataPersistenceService<ParameterType>)}");
			}

			return persistenceService.Insert(parameterType, AuthenticationContext.Current.Principal, TransactionMode.Commit);
		}

		/// <summary>
		/// Creates a new report definition.
		/// </summary>
		/// <param name="reportDefinition">The report definition to create.</param>
		/// <returns>Returns the created report definition.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.UnrestrictedMetadata)]
		public ReportDefinition CreateReportDefinition(ReportDefinition reportDefinition)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates a report format.
		/// </summary>
		/// <param name="reportFormat">The report format to create.</param>
		/// <returns>Returns the created report format.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.UnrestrictedMetadata)]
		public ReportFormat CreateReportFormat(ReportFormat reportFormat)
		{
			var persistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<ReportFormat>>();

			if (persistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate persistence service: {nameof(IDataPersistenceService<ReportFormat>)}");
			}

			return persistenceService.Insert(reportFormat, AuthenticationContext.Current.Principal, TransactionMode.Commit);
		}

		/// <summary>
		/// Deletes a report parameter type.
		/// </summary>
		/// <param name="id">The id of the report parameter type to delete.</param>
		/// <returns>Returns the deleted report parameter type.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.UnrestrictedMetadata)]
		public ParameterType DeleteParameterType(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Deletes a report definition.
		/// </summary>
		/// <param name="id">The id of the report definition to delete.</param>
		/// <returns>Returns the deleted report definition.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.UnrestrictedMetadata)]
		public ReportDefinition DeleteReportDefinition(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Deletes a report format.
		/// </summary>
		/// <param name="id">The id of the report format.</param>
		/// <returns>Returns the report deleted report format.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.UnrestrictedMetadata)]
		public ReportFormat DeleteReportFormat(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.client?.Dispose();
		}

		/// <summary>
		/// Gets a list of all report parameter types.
		/// </summary>
		/// <returns>Returns a list of report parameter types.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.ReadMetadata)]
		public RisiCollection<ReportParameter> GetAllReportParameterTypes()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a parameter type by id.
		/// </summary>
		/// <param name="id">The id of the parameter type to retrieve.</param>
		/// <returns>Returns a parameter type.</returns>
		/// <exception cref="System.InvalidOperationException">If the persistence service is not found.</exception>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.ReadMetadata)]
		public ParameterType GetParameterType(Guid id)
		{
			var persistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<ParameterType>>();

			if (persistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate persistence service: {nameof(IDataPersistenceService<ParameterType>)}");
			}

			return persistenceService.Get(new Identifier<Guid>(id), AuthenticationContext.Current.Principal, true);
		}

		/// <summary>
		/// Gets a report definition by id.
		/// </summary>
		/// <param name="id">The id of the report definition to retrieve.</param>
		/// <returns>Returns a report definition.</returns>
		/// <exception cref="System.InvalidOperationException">If the persistence service is not found.</exception>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.ReadMetadata)]
		public ReportDefinition GetReportDefinition(Guid id)
		{
			var persistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<ReportDefinition>>();

			if (persistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate persistence service: {nameof(IDataPersistenceService<ReportDefinition>)}");
			}

			var reportDefinition = persistenceService.Get(new Identifier<Guid>(id), AuthenticationContext.Current.Principal, false);

			if (reportDefinition == null)
			{
				return null;
			}

			this.Authenticate(this.username, this.password);

			var response = client.GetAsync($"{this.ReportUri}{JasperResourcesPath}{reportDefinition.CorrelationId}").Result;


			if (!response.IsSuccessStatusCode)
			{
				return reportDefinition;
			}

			using (var stream = response.Content.ReadAsStreamAsync().Result)
			{
				var serializer = new XmlSerializer(typeof(Model.Core.ReportUnit));

				var reportUnit = (Model.Core.ReportUnit)serializer.Deserialize(stream);

				this.tracer.TraceEvent(TraceEventType.Information, 0, reportUnit.ToString());
			}

			return reportDefinition;
		}

		/// <summary>
		/// Gets a list of report definitions based on a specific query.
		/// </summary>
		/// <returns>Returns a list of report definitions.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.ReadMetadata)]
		public RisiCollection<ReportDefinition> GetReportDefinitions()
		{
			this.Authenticate(this.username, this.password);

			var response = client.GetAsync($"{this.ReportUri}{JasperResourcesPath}").Result;

			tracer.TraceEvent(TraceEventType.Information, 0, $"Jasper report server response: {response.Content}");

			Resources resources;

			using (var stream = response.Content.ReadAsStreamAsync().Result)
			{
				var serializer = new XmlSerializer(typeof(Resources));

				resources = (Resources)serializer.Deserialize(stream);
			}

			var reportDefinitionPersistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<ReportDefinition>>();

			if (reportDefinitionPersistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate persistence service: {nameof(IDataPersistenceService<ReportDefinition>)}");
			}

			var reports = new List<ReportDefinition>();

			foreach (var resourceLookup in resources.ResourceLookups)
			{
				switch (resourceLookup.ResourceType)
				{
					case "reportUnit":
						//var reportResponse = client.GetAsync($"{this.ReportUri}/{JasperResourcesPath}/{resourceLookup.Uri}").Result;

						var reportDefinition = new ReportDefinition(resourceLookup.Label)
						{
							CorrelationId = resourceLookup.Uri,
							Description = resourceLookup.Description
						};

						reports.Add(reportDefinition);
						break;
				}
			}

			foreach (var report in reports)
			{
				var existingReport = reportDefinitionPersistenceService.Query(r => r.CorrelationId == report.CorrelationId, AuthenticationContext.Current.Principal).FirstOrDefault();

				if (existingReport == null)
				{
					reportDefinitionPersistenceService.Insert(report, AuthenticationContext.Current.Principal, TransactionMode.Commit);
				}
				else
				{
					existingReport.Description = report.Description;
					existingReport.Name = report.Name;

					reportDefinitionPersistenceService.Update(existingReport, AuthenticationContext.Current.Principal, TransactionMode.Commit);
				}
			}

			// load the reports from the database.
			var dbReports = reportDefinitionPersistenceService.Query(r => r.Key != null, AuthenticationContext.Current.Principal);

			return new RisiCollection<ReportDefinition>(dbReports);
		}

		/// <summary>
		/// Gets a report format by id.
		/// </summary>
		/// <param name="id">The id of the report format to retrieve.</param>
		/// <returns>Returns a report format.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.ReadMetadata)]
		public ReportFormat GetReportFormat(Guid id)
		{
			var persistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<ReportFormat>>();

			if (persistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate persistence service: {nameof(IDataPersistenceService<ReportFormat>)}");
			}

			return persistenceService.Get(new Identifier<Guid>(id), AuthenticationContext.Current.Principal, true);
		}

		/// <summary>
		/// Gets a report parameter by id.
		/// </summary>
		/// <param name="id">The id of the report parameter to retrieve.</param>
		/// <returns>Returns a report parameter.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.ReadMetadata)]
		public ReportParameter GetReportParameter(Guid id)
		{
			var persistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<ReportParameter>>();

			if (persistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate persistence service: {nameof(IDataPersistenceService<ReportParameter>)}");
			}

			return persistenceService.Get(new Identifier<Guid>(id), AuthenticationContext.Current.Principal, true);
		}

		/// <summary>
		/// Gets a list of report parameters.
		/// </summary>
		/// <param name="id">The id of the report for which to retrieve parameters.</param>
		/// <returns>Returns a list of parameters.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.ReadMetadata)]
		public RisiCollection<ReportParameter> GetReportParameters(Guid id)
		{
			var persistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<ReportDefinition>>();

			if (persistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate persistence service: {nameof(IDataPersistenceService<ReportDefinition>)}");
			}

			var reportDefinition = persistenceService.Get(new Identifier<Guid>(id), AuthenticationContext.Current.Principal, true);

			return new RisiCollection<ReportParameter>(reportDefinition.Parameters);
		}

		/// <summary>
		/// Gets a list of auto-complete parameters which are applicable for the specified parameter.
		/// </summary>
		/// <param name="id">The id of the report.</param>
		/// <param name="parameterId">The id of the parameter for which to retrieve detailed information.</param>
		/// <returns>Returns an auto complete source definition of valid parameters values for a given parameter.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.ReadMetadata)]
		public AutoCompleteSourceDefinition GetReportParameterValues(Guid id, Guid parameterId)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the report source.
		/// </summary>
		/// <param name="id">The id of the report for which to retrieve the source.</param>
		/// <returns>Returns the report source.</returns>
		/// <exception cref="System.InvalidOperationException">Unable to locate the persistence service or Unable to contact the Jasper Report Service.</exception>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.ReadMetadata)]
		public byte[] GetReportSource(Guid id)
		{
			var persistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<ReportDefinition>>();

			if (persistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate persistence service: {nameof(IDataPersistenceService<ReportDefinition>)}");
			}

			var reportDefinition = persistenceService.Get(new Identifier<Guid>(id), AuthenticationContext.Current.Principal, true);

			if (reportDefinition == null)
			{
				return null;
			}

			this.Authenticate(this.username, this.password);

			var response = client.GetAsync($"{this.ReportUri}{JasperResourcesPath}{reportDefinition.CorrelationId}").Result;

			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"Unable to contact the Jasper Report Service: { response.Content.ReadAsStringAsync().Result }");
			}

			using (var stream = response.Content.ReadAsStreamAsync().Result)
			{
				var serializer = new XmlSerializer(typeof(Model.Core.ReportUnit));

				var reportUnit = (Model.Core.ReportUnit)serializer.Deserialize(stream);

				this.tracer.TraceEvent(TraceEventType.Information, 0, reportUnit.ToString());

				response = client.GetAsync($"{this.ReportUri}{JasperResourcesPath}{reportUnit.JrXmlFileReference.Uri}").Result;

				return response.Content.ReadAsByteArrayAsync().Result;
			}
		}

		/// <summary>
		/// Handles the <see cref="E:Authenticated" /> event.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="authenticatedEventArgs">The <see cref="AuthenticatedEventArgs"/> instance containing the event data.</param>
		private void OnAuthenticated(object sender, AuthenticatedEventArgs authenticatedEventArgs)
		{
			this.AuthenticationResult = authenticatedEventArgs.AuthenticationResult;
			this.cookieContainer.Add(new Cookie(JasperCookieKey, this.AuthenticationResult.Token) { Domain = JasperCookieKey });
		}

		/// <summary>
		/// Runs a report.
		/// </summary>
		/// <param name="reportId">The id of the report.</param>
		/// <param name="reportFormatId">The format of the report.</param>
		/// <param name="parameters">The parameters of the report.</param>
		/// <returns>Returns the raw report.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.UnrestrictedClinicalData)]
		public byte[] RunReport(Guid reportId, Guid reportFormatId, IEnumerable<ReportParameter> parameters)
		{
			var persistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<ReportDefinition>>();

			if (persistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate persistence service: {nameof(IDataPersistenceService<ReportDefinition>)}");
			}

			var reportFormatPersistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<ReportFormat>>();

			if (reportFormatPersistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate persistence service: {nameof(IDataPersistenceService<ReportDefinition>)}");
			}

			var reportFormat = reportFormatPersistenceService.Get(new Identifier<Guid>(reportFormatId), AuthenticationContext.Current.Principal, true);

			if (reportFormat == null)
			{
				throw new InvalidOperationException($"Unable to locate report format using id: {reportFormatId}");
			}

			var reportDefinition = persistenceService.Get(new Identifier<Guid>(reportId), AuthenticationContext.Current.Principal, true);

			if (reportDefinition == null)
			{
				throw new InvalidOperationException($"Unable to locate report using id: {reportId}");
			}

			var builder = new StringBuilder();

			builder.Append(this.ReportUri);
			builder.Append(JasperReportPath);
			builder.Append(reportDefinition.CorrelationId);
			builder.Append(".");
			builder.Append(reportFormat.Format);
			builder.Append("?");

			var reportParameterPersistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<ReportParameter>>();

			if (reportParameterPersistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate persistence service: {nameof(IDataPersistenceService<ReportParameter>)}");
			}

			var reportParameters = parameters.Select(reportParameter => reportParameterPersistenceService.Get(new Identifier<Guid>(reportParameter.Key.Value), AuthenticationContext.Current.Principal, true)).ToList();

			foreach (var reportParameter in reportParameters.Where(p => reportDefinition.Parameters.Select(r => r.Key).Contains(p.Key)).OrderBy(r => r.Order))
			{
				builder.Append($"&{reportParameter.CorrelationId}={reportParameter.Value}");
			}

			var response = client.GetAsync(builder.ToString()).Result;

			if (!response.IsSuccessStatusCode)
			{
				return null;
			}

			return response.Content.ReadAsByteArrayAsync().Result;
		}

		/// <summary>
		/// Updates a parameter type.
		/// </summary>
		/// <param name="parameterType">The updated parameter type.</param>
		/// <returns>Returns the updated parameter type.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.UnrestrictedMetadata)]
		public ParameterType UpdateParameterType(ParameterType parameterType)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Updates a report definition.
		/// </summary>
		/// <param name="reportDefinition">The updated report definition.</param>
		/// <returns>Returns the updated report definition.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.UnrestrictedMetadata)]
		public ReportDefinition UpdateReportDefinition(ReportDefinition reportDefinition)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Updates a report format.
		/// </summary>
		/// <param name="reportFormat">The updated report format.</param>
		/// <returns>Returns the update report format.</returns>
		[PolicyPermission(SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.UnrestrictedMetadata)]
		public ReportFormat UpdateReportFormat(ReportFormat reportFormat)
		{
			throw new NotImplementedException();
		}
	}
}