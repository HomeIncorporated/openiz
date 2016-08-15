﻿using System;
using OpenIZ.Core.Model.Acts;
using OpenIZ.Persistence.Data.MSSQL.Data;
using System.Security.Principal;
using System.Linq;

namespace OpenIZ.Persistence.Data.MSSQL.Services.Persistence
{
    /// <summary>
    /// Persistence class for observations
    /// </summary>
    public abstract class ObservationPersistenceService<TObservation, TDbObservation> : ActDerivedPersistenceService<TObservation, TDbObservation>
        where TObservation : Core.Model.Acts.Observation, new()
        where TDbObservation : class, IDbIdentified, new()
    {

        /// <summary>
        /// Convert a data act and observation instance to an observation
        /// </summary>
        public virtual TObservation ToModelInstance(TDbObservation dataInstance, Data.ActVersion actInstance, Data.Observation obsInstance, ModelDataContext context, IPrincipal principal)
        {
            var retVal = m_actPersister.ToModelInstance<TObservation>(actInstance, context, principal);

            if(obsInstance.InterpretationConceptId != null)
                retVal.InterpretationConceptKey = obsInstance.InterpretationConceptId;

            return retVal;
        }

        /// <summary>
        /// Insert the specified observation into the database
        /// </summary>
        public override TObservation Insert(ModelDataContext context, TObservation data, IPrincipal principal)
        {
            data.InterpretationConcept?.EnsureExists(context, principal);
            data.InterpretationConceptKey = data.InterpretationConcept?.Key ?? data.InterpretationConceptKey;

            return base.Insert(context, data, principal);
        }

        /// <summary>
        /// Updates the specified observation
        /// </summary>
        public override TObservation Update(ModelDataContext context, TObservation data, IPrincipal principal)
        {
            data.InterpretationConcept?.EnsureExists(context, principal);
            data.InterpretationConceptKey = data.InterpretationConcept?.Key ?? data.InterpretationConceptKey;

            return base.Update(context, data, principal);
        }
    }

    /// <summary>
    /// Text observation service
    /// </summary>
    public class TextObservationPersistenceService : ObservationPersistenceService<Core.Model.Acts.TextObservation, Data.TextObservation>
    {
        /// <summary>
        /// Convert the specified object to a model instance
        /// </summary>
        public override Core.Model.Acts.TextObservation ToModelInstance(Data.TextObservation dataInstance, Data.ActVersion actInstance, Data.Observation obsInstance, ModelDataContext context, IPrincipal principal)
        {
            var retVal = base.ToModelInstance(dataInstance, actInstance, obsInstance, context, principal);
            retVal.Value = dataInstance.Value;
            return retVal;
        }

        /// <summary>
        /// Convert to model instance
        /// </summary>
        public override Core.Model.Acts.TextObservation ToModelInstance(object dataInstance, ModelDataContext context, IPrincipal principal)
        {
            var iddat = dataInstance as IDbVersionedData;
            var textObs = dataInstance as Data.TextObservation ?? context.GetTable<Data.TextObservation>().Where(o => o.ActVersionId == iddat.VersionId).First();
            var dba = dataInstance as Data.ActVersion ?? context.GetTable<Data.ActVersion>().Where(o => o.ActVersionId == iddat.VersionId).First();
            var dbo = context.GetTable<Data.Observation>().Where(o => o.ActVersionId == textObs.ActVersionId).First();
            return this.ToModelInstance(textObs, dba, dbo, context, principal);
        }
    }

    /// <summary>
    /// Coded observation service
    /// </summary>
    public class CodedObservationPersistenceService : ObservationPersistenceService<Core.Model.Acts.CodedObservation, Data.CodedObservation>
    {
        /// <summary>
        /// Convert the specified object to a model instance
        /// </summary>
        public override Core.Model.Acts.CodedObservation ToModelInstance(Data.CodedObservation dataInstance, Data.ActVersion actInstance, Data.Observation obsInstance, ModelDataContext context, IPrincipal principal)
        {
            var retVal = base.ToModelInstance(dataInstance, actInstance, obsInstance, context, principal);
            if(dataInstance.ValueConcept != null)
                retVal.ValueKey = dataInstance.ValueConceptId;
            return retVal;
        }

        /// <summary>
        /// Convert to model instance
        /// </summary>
        public override Core.Model.Acts.CodedObservation ToModelInstance(object dataInstance, ModelDataContext context, IPrincipal principal)
        {
            var iddat = dataInstance as IDbVersionedData;
            var codeObs = dataInstance as Data.CodedObservation ?? context.GetTable<Data.CodedObservation>().Where(o => o.ActVersionId == iddat.VersionId).First();
            var dba = dataInstance as Data.ActVersion ?? context.GetTable<Data.ActVersion>().Where(o => o.ActVersionId == codeObs.ActVersionId).First();
            var dbo = context.GetTable<Data.Observation>().Where(o => o.ActVersionId == codeObs.ActVersionId).First();
            return this.ToModelInstance(codeObs, dba, dbo, context, principal);
        }

        /// <summary>
        /// Insert the observation
        /// </summary>
        public override Core.Model.Acts.CodedObservation Insert(ModelDataContext context, Core.Model.Acts.CodedObservation data, IPrincipal principal)
        {
            data.Value?.EnsureExists(context, principal);
            data.ValueKey = data.Value?.Key ?? data.ValueKey;
            return base.Insert(context, data, principal);
        }

        /// <summary>
        /// Update the specified observation
        /// </summary>
        public override Core.Model.Acts.CodedObservation Update(ModelDataContext context, Core.Model.Acts.CodedObservation data, IPrincipal principal)
        {
            data.Value?.EnsureExists(context, principal);
            data.ValueKey = data.Value?.Key ?? data.ValueKey;
            return base.Update(context, data, principal);
        }
    }

    /// <summary>
    /// Quantity observation persistence service
    /// </summary>
    public class QuantityObservationPersistenceService : ObservationPersistenceService<Core.Model.Acts.QuantityObservation, Data.QuantityObservation>
    {
        /// <summary>
        /// Convert the specified object to a model instance
        /// </summary>
        public override Core.Model.Acts.QuantityObservation ToModelInstance(Data.QuantityObservation dataInstance, Data.ActVersion actInstance, Data.Observation obsInstance, ModelDataContext context, IPrincipal principal)
        {
            var retVal = base.ToModelInstance(dataInstance, actInstance, obsInstance, context, principal);
            if (dataInstance.UnitOfMeasureConceptId != null)
                retVal.UnitOfMeasureKey = dataInstance.UnitOfMeasureConceptId;
            retVal.Value = dataInstance.Quantity;
            
            return retVal;
        }

        /// <summary>
        /// Convert to model instance
        /// </summary>
        public override Core.Model.Acts.QuantityObservation ToModelInstance(object dataInstance, ModelDataContext context, IPrincipal principal)
        {
            var iddat = dataInstance as IDbVersionedData;
            var qObs = dataInstance as Data.QuantityObservation ?? context.GetTable<Data.QuantityObservation>().Where(o => o.ActVersionId == iddat.VersionId).First();
            var dba = dataInstance as Data.ActVersion ?? context.GetTable<Data.ActVersion>().Where(o => o.ActVersionId == qObs.ActVersionId).First();
            var dbo = context.GetTable<Data.Observation>().Where(o => o.ActVersionId == qObs.ActVersionId).First();
            return this.ToModelInstance(qObs, dba, dbo, context, principal);
        }

        /// <summary>
        /// Insert the observation
        /// </summary>
        public override Core.Model.Acts.QuantityObservation Insert(ModelDataContext context, Core.Model.Acts.QuantityObservation data, IPrincipal principal)
        {
            data.UnitOfMeasure?.EnsureExists(context, principal);
            data.UnitOfMeasureKey = data.UnitOfMeasure?.Key ?? data.UnitOfMeasureKey;
            return base.Insert(context, data, principal);
        }

        /// <summary>
        /// Update the specified observation
        /// </summary>
        public override Core.Model.Acts.QuantityObservation Update(ModelDataContext context, Core.Model.Acts.QuantityObservation data, IPrincipal principal)
        {
            data.UnitOfMeasure?.EnsureExists(context, principal);
            data.UnitOfMeasureKey = data.UnitOfMeasure?.Key ?? data.UnitOfMeasureKey;
            return base.Update(context, data, principal);
        }
    }
}