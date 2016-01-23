﻿using OpenIZ.Core.Model.Attributes;
using OpenIZ.Core.Model.DataTypes;
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml;

namespace OpenIZ.Core.Model.Entities
{
    /// <summary>
    /// Represents a service for a place
    /// </summary>
    
    [XmlType("PlaceService", Namespace = "http://openiz.org/model")]
    public class PlaceService : VersionedAssociation<Entity>
    {

        // Service key
        private Guid m_serviceConceptKey;
        // Service
        
        private Concept m_service;

        /// <summary>
        /// The schedule that the service is offered
        /// </summary>
        [XmlElement("serviceSchedule")]
        public Object ServiceSchedule { get; set; }

        /// <summary>
        /// Gets or sets the service concept key
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement("serviceConceptString", typeof(String))]
        public Guid ServiceConceptKey
        {
            get { return this.m_serviceConceptKey; }
            set
            {
                this.m_serviceConceptKey = value;
                this.m_service = null;
            }
        }

        /// <summary>
        /// Gets or sets the service concept
        /// </summary>
        [DelayLoad(nameof(ServiceConceptKey))]
        [XmlIgnore]
        public Concept ServiceConcept
        {
            get {
                this.m_service = base.DelayLoad(this.m_serviceConceptKey, this.m_service);
                return this.m_service;
            }
            set
            {
                this.m_service = value;
                if (value == null)
                    this.m_serviceConceptKey = Guid.Empty;
                else
                    this.m_serviceConceptKey = value.Key;
            }
        }

        /// <summary>
        /// Refresh the delay load properties
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            this.m_service = null;
        }
    }
}