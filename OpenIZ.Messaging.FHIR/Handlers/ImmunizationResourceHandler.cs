﻿using MARC.HI.EHRS.SVC.Messaging.FHIR.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MARC.HI.EHRS.SVC.Core.Services;
using MARC.HI.EHRS.SVC.Messaging.FHIR;
using MARC.HI.EHRS.SVC.Messaging.FHIR.Resources;
using System.Collections.Specialized;
using MARC.Everest.Connectors;
using MARC.HI.EHRS.SVC.Core.Data;
using OpenIZ.Core.Model.Acts;
using OpenIZ.Core.Model.Query;
using System.Linq.Expressions;
using OpenIZ.Core.Services;
using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Messaging.FHIR.DataTypes;
using OpenIZ.Messaging.FHIR.Util;
using OpenIZ.Core.Model.Constants;
using OpenIZ.Core.Model.Entities;
using MARC.HI.EHRS.SVC.Messaging.FHIR.Backbone;
using System.ServiceModel.Web;

namespace OpenIZ.Messaging.FHIR.Handlers
{
    /// <summary>
    /// Resource handler for immunization classes
    /// </summary>
    public class ImmunizationResourceHandler : ResourceHandlerBase<MARC.HI.EHRS.SVC.Messaging.FHIR.Resources.Immunization, Core.Model.Acts.SubstanceAdministration>
    {
        // Repository
        private IActRepositoryService m_repository;

        /// <summary>
        /// Place resource handler subscription
        /// </summary>
        public ImmunizationResourceHandler()
        {
            ApplicationContext.Current.Started += (o, e) => this.m_repository = ApplicationContext.Current.GetService<IActRepositoryService>();
        }
        
        protected override SubstanceAdministration Create(SubstanceAdministration modelInstance, List<IResultDetail> issues, MARC.HI.EHRS.SVC.Core.Services.TransactionMode mode)
        {
            throw new NotImplementedException();
        }

        protected override SubstanceAdministration Delete(Guid modelId, List<IResultDetail> details)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps the substance administration to FHIR
        /// </summary>
        protected override Immunization MapToFhir(SubstanceAdministration model)
        {
            
            var retVal = DatatypeConverter.CreateResource<Immunization>(model);
            retVal.DoseQuantity = new FhirQuantity()
            {
                Units = model.DoseUnit.Mnemonic,
                Value = new FhirDecimal(model.DoseQuantity)
            };
            retVal.Date = (FhirDate)model.ActTime.DateTime;
            retVal.Route = DatatypeConverter.Convert(model.Route);
            retVal.Site = DatatypeConverter.Convert(model.Site);
            retVal.Status = "completed";
            //retVal.SelfReported = model.Tags.Any(o => o.TagKey == "selfReported" && Convert.ToBoolean(o.Value));
            retVal.WasNotGiven = model.IsNegated;
            
            // Material
            var matl = model.Participations.FirstOrDefault(o => o.ParticipationRoleKey == ActParticipationKey.Consumable)?.PlayerEntity as Material;
            if(matl != null)
            {
                retVal.VaccineCode = DatatypeConverter.Convert(matl.TypeConcept);
                retVal.ExpirationDate = (FhirDate)matl.ExpiryDate;
                retVal.LotNumber = (matl as ManufacturedMaterial)?.LotNumber;
            }

            // RCT
            var rct = model.Participations.FirstOrDefault(o => o.ParticipationRoleKey == ActParticipationKey.RecordTarget);
            if (rct != null)
            { 
                retVal.Patient = Reference.CreateLocalResourceReference(new Patient() { Id = rct.PlayerEntityKey.ToString() });
            }

            // Performer 
            var prf = model.Participations.FirstOrDefault(o => o.ParticipationRoleKey == ActParticipationKey.Performer);
            if(prf != null)
                retVal.Performer = Reference.CreateResourceReference(new Practictioner() { Id = rct.PlayerEntityKey.ToString() }, WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri);

            // Protocol
            foreach (var itm in model.Protocols)
            {
                ImmunizationProtocol protocol = new ImmunizationProtocol();
                protocol.DoseSequence = new FhirInt((int)model.SequenceId);
                protocol.Series = itm.Protocol.Name;
                retVal.VaccinationProtocol.Add(protocol);
            }
            if (retVal.VaccinationProtocol.Count == 0)
                retVal.VaccinationProtocol.Add(new ImmunizationProtocol() { DoseSequence = (int)model.SequenceId });

            return retVal;
        }

        protected override SubstanceAdministration MapToModel(Immunization resource)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Query for substance administrations
        /// </summary>
        protected override IEnumerable<SubstanceAdministration> Query(Expression<Func<SubstanceAdministration, bool>> query, List<IResultDetail> issues, int offset, int count, out int totalResults)
        {
            Expression<Func<SubstanceAdministration, bool>> filter = o => o.ClassConceptKey == ActClassKeys.SubstanceAdministration && o.ObsoletionTime == null && o.MoodConceptKey == ActMoodKeys.Eventoccurrence;
            var parm = Expression.Parameter(typeof(SubstanceAdministration));
            query = Expression.Lambda<Func<SubstanceAdministration, bool>>(Expression.AndAlso(Expression.Invoke(filter, parm), Expression.Invoke(query, parm)), parm);
            return this.m_repository.FindSubstanceAdministrations(query, offset, count, out totalResults);
        }

        protected override SubstanceAdministration Read(Identifier<Guid> id, List<IResultDetail> details)
        {
            throw new NotImplementedException();
        }

        protected override SubstanceAdministration Update(SubstanceAdministration model, List<IResultDetail> details, MARC.HI.EHRS.SVC.Core.Services.TransactionMode mode)
        {
            throw new NotImplementedException();
        }
    }
}