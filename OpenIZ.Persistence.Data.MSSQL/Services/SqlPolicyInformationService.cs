﻿using MARC.HI.EHRS.SVC.Core.Services.Policy;
using OpenIZ.Core.Model;
using OpenIZ.Core.Model.Acts;
using OpenIZ.Core.Model.Entities;
using OpenIZ.Core.Model.Security;
using OpenIZ.Core.Security;
using OpenIZ.Persistence.Data.MSSQL.Configuration;
using OpenIZ.Persistence.Data.MSSQL.Data;
using OpenIZ.Persistence.Data.MSSQL.Security;
using OpenIZ.Persistence.Data.MSSQL.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Persistence.Data.MSSQL.Services
{
    /// <summary>
    /// Represents a PIP fed from SQL Server tables
    /// </summary>
    public class SqlPolicyInformationService : IPolicyInformationService
    {
        // Get the SQL configuration
        private SqlConfiguration m_configuration = ConfigurationManager.GetSection("openiz.persistence.data.mssql") as SqlConfiguration;
        
        /// <summary>
        /// Get active policies for the specified securable type
        /// </summary>
        public IEnumerable<IPolicyInstance> GetActivePolicies(object securable)
        {
            using (ModelDataContext context = new ModelDataContext(this.m_configuration.ReadonlyConnectionString))
                // Security device
                if (securable is Core.Model.Security.SecurityDevice)
                    return context.SecurityDevicePolicies.Where(o => o.DeviceId == (securable as IdentifiedData).Key && o.Policy.ObsoletionTime == null).Select(o => new SqlSecurityPolicyInstance(o));
                else if (securable is Core.Model.Security.SecurityRole)
                    return context.SecurityRolePolicies.Where(o => o.RoleId == (securable as IdentifiedData).Key && o.Policy.ObsoletionTime == null).Select(o => new SqlSecurityPolicyInstance(o));
                else if (securable is IPrincipal || securable is IIdentity)
                {
                    var identity = (securable as IPrincipal).Identity ?? securable as IIdentity;
                    var user = context.SecurityUsers.SingleOrDefault(u => u.UserName == identity.Name);
                    if (user == null)
                        throw new KeyNotFoundException("Identity not found");

                    List<IPolicyInstance> retVal = new List<IPolicyInstance>();

                    // Role policies
                    retVal.AddRange(context.SecurityRolePolicies.Where(o => user.SecurityUserRoles.Any(r => r.RoleId == o.RoleId)).Select(o=>new SqlSecurityPolicyInstance(o)));

                    // Claims principal, then we want device and app SID
                    if (securable is ClaimsPrincipal)
                    {
                        var cp = securable as ClaimsPrincipal;
                        var appClaim = cp.FindAll(OpenIzClaimTypes.OpenIzApplicationIdentifierClaim).SingleOrDefault();
                        var devClaim = cp.FindAll(OpenIzClaimTypes.OpenIzDeviceIdentifierClaim).SingleOrDefault();

                        // There is an application claim so we want to add the application policies - most restrictive
                        if (appClaim != null)
                            retVal.AddRange(context.SecurityApplicationPolicies.Where(o => o.SecurityApplication.ApplicationSecret == appClaim.Value).Select(o => new SqlSecurityPolicyInstance(o)));
                        // There is an device claim so we want to add the device policies - most restrictive
                        if (devClaim != null)
                            retVal.AddRange(context.SecurityDevicePolicies.Where(o => o.SecurityDevice.DeviceSecret == devClaim.Value).Select(o => new SqlSecurityPolicyInstance(o)));
                    }
                    return retVal;
                }
                else if (securable is Core.Model.Security.SecurityApplication)
                    return context.SecurityApplicationPolicies.Where(o => o.ApplicationId == (securable as IdentifiedData).Key && o.Policy.ObsoletionTime == null).Select(o => new SqlSecurityPolicyInstance(o));
                else if (securable is Core.Model.Acts.Act)
                {
                    var pAct = securable as Core.Model.Acts.Act;
                    return context.ActPolicies.Where(o => o.ActId == (securable as IdentifiedData).Key && o.Policy.ObsoletionTime == null && pAct.VersionSequence >= o.EffectiveVersionSequenceId && (pAct.VersionSequence < o.ObsoleteVersionSequenceId || o.ObsoleteVersionSequenceId == null)).Select(o => new SqlSecurityPolicyInstance(o));
                }
                else if (securable is Core.Model.Entities.Entity)
                {
                    throw new NotImplementedException();
                }
                else
                    return new List<IPolicyInstance>();
        }

        /// <summary>
        /// Get all policies on the system
        /// </summary>
        public IEnumerable<IPolicy> GetPolicies()
        {
            using (var dataContext = new ModelDataContext(this.m_configuration.ReadonlyConnectionString))
                return dataContext.Policies.Where(o => o.ObsoletionTime == null).Select(o => new SqlSecurityPolicy(o));
        }

        /// <summary>
        /// Get a specific policy
        /// </summary>
        public IPolicy GetPolicy(string policyOid)
        {
            using (var dataContext = new ModelDataContext(this.m_configuration.ReadonlyConnectionString))
            {
                var policy = dataContext.Policies.SingleOrDefault(o => o.PolicyOid == policyOid);
                if(policy != null)
                    return new SqlSecurityPolicy(policy);
                return null;
            }
        }
    }
}