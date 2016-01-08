﻿using OpenIZ.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MARC.HI.EHRS.SVC.Core.Authorization;
using MARC.HI.EHRS.SVC.Core.Data;
using OpenIZ.Persistence.Data.MSSQL.Data;
using System.Linq.Expressions;
using OpenIZ.Persistence.Data.MSSQL.Exceptions;
using System.Reflection;
using OpenIZ.Core.Model;

namespace OpenIZ.Persistence.Data.MSSQL.Services.Persistence
{
    /// <summary>
    /// A persistence service which can persist and query security user objects
    /// </summary>
    public class SecurityUserPersistenceService : BaseDataPersistenceService<Core.Model.Security.SecurityUser>
    {


        /// <summary>
        /// Perform a get operation
        /// </summary>
        /// <param name="containerId">The identifier of the container to retrieve</param>
        /// <param name="authContext">The authorization context</param>
        /// <param name="loadFast">True if the history and historical data should be loaded</param>
        /// <param name="dataContext">The data context</param>
        /// <returns>The security user as part of the get</returns>
        protected override Core.Model.Security.SecurityUser DoGet(Identifier<Guid> containerId, AuthorizationContext authContext, bool loadFast, ModelDataContext dataContext)
        {
            var dataUser = dataContext.SecurityUsers.FirstOrDefault(o => o.UserId == containerId.Id);

            if (dataUser != null)
                return this.ConvertToModel(dataUser);
            else
                return null;
        }

        /// <summary>
        /// Perform an insert
        /// </summary>
        /// <param name="storageData">The model class to be stored</param>
        /// <param name="authContext">The authorization context</param>
        /// <param name="dataContext">The data context</param>
        /// <returns>The security user which was inserted</returns>
        protected override Core.Model.Security.SecurityUser DoInsert(Core.Model.Security.SecurityUser storageData, AuthorizationContext authContext, ModelDataContext dataContext)
        {
            if (storageData.Key != default(Guid)) // Trying to insert an already inserted user?
                throw new SqlFormalConstraintException("Insert must be for an unidentified object");

            var dataUser = this.ConvertFromModel(storageData) as Data.SecurityUser;
            dataUser.CreatedBy = authContext == null ? null : (Guid?)base.GetUserFromAuthContext(authContext, dataContext);
            dataContext.SecurityUsers.InsertOnSubmit(dataUser);

            // Persist data to the db
            dataContext.SubmitChanges();

            return this.ConvertToModel(dataUser);
        }

        /// <summary>
        /// Perform an obsolete
        /// </summary>
        /// <param name="storageData">The data object to be obsoleted</param>
        /// <param name="authContext">The authorization context under which the security user is obsolete</param>
        /// <param name="dataContext">The current data context</param>
        /// <returns>The obsoleted user</returns>
        protected override Core.Model.Security.SecurityUser DoObsolete(Core.Model.Security.SecurityUser storageData, AuthorizationContext authContext, ModelDataContext dataContext)
        {
            if (storageData.Key == default(Guid))
                throw new SqlFormalConstraintException("Obsolete must be for an identified object");

            var dataUser = this.ConvertFromModel(storageData) as Data.SecurityUser;
            dataUser.ObsoletedBy = base.GetUserFromAuthContext(authContext, dataContext);
            dataUser.ObsoletionTime = DateTimeOffset.Now;

            // Persist data to the db
            dataContext.SubmitChanges();

            return this.ConvertToModel(dataUser);
        }

        /// <summary>
        /// Perform a query 
        /// </summary>
        protected override IQueryable<Core.Model.Security.SecurityUser> DoQuery(Expression<Func<Core.Model.Security.SecurityUser, bool>> query, AuthorizationContext authContext, ModelDataContext dataContext)
        {
            Expression queryExpression = s_mapper.MapModelExpression<Core.Model.Security.SecurityUser, Data.SecurityUser>(query);
            return null;
        }

        protected override Core.Model.Security.SecurityUser DoUpdate(Core.Model.Security.SecurityUser storageData, AuthorizationContext authContext, ModelDataContext dataContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert a model class into a data class
        /// </summary>
        internal override object ConvertFromModel(Core.Model.Security.SecurityUser model)
        {
            return s_mapper.MapModelInstance<Core.Model.Security.SecurityUser, Data.SecurityUser>(model);
        }

        /// <summary>
        /// Convert data to model
        /// </summary>
        internal override Core.Model.Security.SecurityUser ConvertToModel(object data)
        {
            return s_mapper.MapDomainInstance<Data.SecurityUser, Core.Model.Security.SecurityUser>(data as Data.SecurityUser);
        }
    }
}
