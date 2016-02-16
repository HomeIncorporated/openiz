﻿using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Core.Services;
using MARC.HI.EHRS.SVC.Core.Services.Policy;
using MARC.HI.EHRS.SVC.Core.Services.Security;
using OpenIZ.Core.Configuration;
using OpenIZ.Core.Security.Claims;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using OpenIZ.Core;

namespace OpenIZ.Core.Security.Wcf
{
    /// <summary>
    /// Basic authorization policy
    /// </summary>
    public class BasicAuthorizationAccessPolicy : IAuthorizationPolicy
    {

        // Configuration from main OpenIZ
        private OpenIzConfiguration m_configuration = ApplicationContext.Current.GetService<IConfigurationManager>().GetSection(OpenIzConstants.OpenIZConfigurationName) as OpenIzConfiguration;

        // Trace source
        private TraceSource m_traceSource = new TraceSource(OpenIzConstants.SecurityTraceSourceName);
        
        // Guid
        private Guid m_id = Guid.NewGuid();

        /// <summary>
        /// Gets the id of the authoziation policy
        /// </summary>
        public string Id
        {
            get
            {
                return this.m_id.ToString();
            }
        }

        /// <summary>
        /// Issuer
        /// </summary>
        public ClaimSet Issuer
        {
            get
            {
                return ClaimSet.System;
            }
        }

        /// <summary>
        /// Evaluate the context 
        /// </summary>
        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            try
            {
                this.m_traceSource.TraceInformation("Entering BasicAuthorizationAccessPolicy");

                object obj;
                if (!evaluationContext.Properties.TryGetValue("Identities", out obj))
                    throw new Exception("No Identity found");
                IList<IIdentity> identities = obj as IList<IIdentity>;
                if (identities == null || identities.Count <= 0)
                    throw new Exception("No Identity found");

                // Role service
                var roleService = ApplicationContext.Current.GetService<IRoleProviderService>();
                var pipService = ApplicationContext.Current.GetService<IPolicyInformationService>();

                // Claims
                var roles = roleService.GetAllRoles(identities[0].Name);
                List<System.Security.Claims.Claim> claims = new List<System.Security.Claims.Claim>(
                    roles.Select(o => new System.Security.Claims.Claim(ClaimsIdentity.DefaultRoleClaimType, o))
                    );


                // Add claims made by the client
                HttpRequestMessageProperty httpRequest = (HttpRequestMessageProperty)evaluationContext.Properties[HttpRequestMessageProperty.Name];
                if (httpRequest != null)
                {
                    var clientClaims = OpenIzClaimTypes.ExtractClaims(httpRequest.Headers);
                    foreach(var claim in clientClaims)
                    {
                        if (this.m_configuration.Security?.BasicAuth?.AllowedClientClaims?.Contains(claim.Type) == false)
                            throw new SecurityException(ApplicationContext.Current.GetLocaleString("SECE001"));
                        else
                        {
                            var handler = OpenIzClaimTypes.GetHandler(claim.Type);
                            if (handler == null ||
                                handler.Validate(new GenericPrincipal(identities[0], roles), claim.Value))
                                claims.Add(claim);
                            else
                                throw new SecurityException(ApplicationContext.Current.GetLocaleString("SECE002"));
                        }
                    }
                }

                // Claim headers built in
                if (pipService != null)
                    claims.AddRange(pipService.GetActivePolicies(identities[0]).Select(o => new System.Security.Claims.Claim(OpenIzClaimTypes.OpenIzGrantedPolicyClaim, o.Policy.Oid)));

                var principal = new ClaimsPrincipal(new ClaimsIdentity(identities[0], claims));
                
                evaluationContext.Properties["Principal"] = principal;

                AuthenticationContext.Current = new AuthenticationContext(principal); // Set Authentication context

                return true;
            }
            catch (Exception e)
            {
                this.m_traceSource.TraceEvent(TraceEventType.Error, e.HResult, e.ToString());
                throw;
            }
        }
    }
}