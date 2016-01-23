﻿

using OpenIZ.Core.Model.Acts;
using OpenIZ.Core.Model.Attributes;
using OpenIZ.Core.Model.DataTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using System.Text;
using System.Threading.Tasks;
using OpenIZ.Core.Model.EntityLoader;

namespace OpenIZ.Core.Model.Entities
{
    /// <summary>
    /// Represents the base of all entities
    /// </summary>
    [XmlType("Entity", Namespace = "http://openiz.org/model")]
    [XmlRoot(Namespace = "http://openiz.org/model", ElementName = "Entity")]
    
    public class Entity : VersionedEntityData<Entity>
    {

        // Classe concept
        private Guid m_classConceptId;
        // Determiner concept id
        private Guid m_determinerConceptId;
        // Status 
        private Guid m_statusConceptId;
        // Control act which created this
        private Guid m_creationActId;
        // Type concept
        private Guid? m_typeConceptId;

        // Class concept
        private Concept m_classConcept;
        // Determiner concept
        
        private Concept m_determinerConcept;
        // TODO: Change this to Act
        private Act m_creationAct;
        // Status concept
        private Concept m_statusConcept;
        // Type concept
        private Concept m_typeConcept;

        // Identifiers 
        private List<EntityIdentifier> m_identifiers;
        // Associations
        private List<EntityRelationship> m_relationships;
        // Telecom addresses
        private List<EntityTelecomAddress> m_telecomAddresses;
        // Extensions
        private List<EntityExtension> m_extensions;
        // Names
        private List<EntityName> m_names;
        // Addresses
        private List<EntityAddress> m_addresses;
        // Notes
        private List<EntityNote> m_notes;
        // Tags
        private List<EntityTag> m_tags;
        // Participations
        private List<ActParticipation> m_participations;

        /// <summary>
        /// Class concept
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        
        [XmlElement("classConcept")]
        public virtual Guid ClassConceptKey
        {
            get { return this.m_classConceptId; }
            set
            {
                this.m_classConceptId = value;
                this.m_classConcept = null;
            }
        }

        /// <summary>
        /// Determiner concept
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        
        [XmlElement("determinerConcept")]
        public virtual Guid DeterminerConceptKey
        {
            get { return this.m_determinerConceptId; }
            set
            {
                this.m_determinerConceptId = value;
                this.m_determinerConcept = null;
            }
        }

        /// <summary>
        /// Status concept id
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        
        [XmlElement("statusConcept")]
        public Guid  StatusConceptKey
        {
            get { return this.m_statusConceptId; }
            set
            {
                this.m_statusConceptId = value;
                this.m_statusConcept = null;
            }
        }
        
        /// <summary>
        /// Creation act reference
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        
        [XmlElement("creationAct")]
        public Guid  CreationActKey
        {
            get { return this.m_creationActId; }
            set
            {
                this.m_creationActId = value;
                this.m_creationAct = null;
            }
        }

        /// <summary>
        /// Type concept identifier
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        
        [XmlElement("typeConcept")]
        public Guid?  TypeConceptKey
        {
            get { return this.m_typeConceptId; }
            set
            {
                this.m_typeConceptId = value;
                this.m_typeConcept = null;
            }
        }

        /// <summary>
        /// Class concept datal load property
        /// </summary>
        [XmlIgnore]
        [DelayLoad(nameof(ClassConceptKey))]
        public Concept ClassConcept
        {
            get {
                this.m_classConcept = base.DelayLoad(this.m_classConceptId, this.m_classConcept);
                return this.m_classConcept;
            }
        }

        /// <summary>
        /// Determiner concept
        /// </summary>
        [DelayLoad(nameof(DeterminerConceptKey))]
        [XmlIgnore]
        public virtual Concept DeterminerConcept
        {
            get
            {
                this.m_determinerConcept = base.DelayLoad(this.m_determinerConceptId, this.m_determinerConcept);
                return this.m_determinerConcept;
            }
        }

        /// <summary>
        /// Status concept id
        /// </summary>
        [DelayLoad(nameof(StatusConceptKey))]
        [XmlIgnore]
        public Concept StatusConcept
        {
            get
            {
                this.m_statusConcept = base.DelayLoad(this.m_statusConceptId, this.m_statusConcept);
                return this.m_statusConcept;
            }
            set
            {
                this.m_statusConcept = value;
                if (value == null)
                    this.m_statusConceptId = Guid.Empty;
                else
                    this.m_statusConceptId = value.Key;
            }
        }

        /// <summary>
        /// Creation act reference
        /// </summary>
        [DelayLoad(nameof(CreationActKey))]
        [XmlIgnore]
        public Act CreationAct
        {
            get {
                this.m_creationAct = base.DelayLoad(this.m_creationActId, this.m_creationAct);
                return this.m_creationAct;
            }
            set
            {
                this.m_creationAct = value;
                if (value != null)
                    this.m_creationActId = value.Key;
                else
                    this.m_creationActId = Guid.Empty;
            }
        }

        /// <summary>
        /// Type concept identifier
        /// </summary>
        [DelayLoad(nameof(TypeConceptKey))]
        [XmlIgnore]
        public Concept TypeConcept
        {
            get {
                this.m_typeConcept = base.DelayLoad(this.m_typeConceptId, this.m_typeConcept);
                return this.m_typeConcept;
            }
            set
            {
                this.m_typeConcept = value;
                this.m_typeConceptId = value?.Key;
            }
        }

        /// <summary>
        /// Gets the identifiers associated with this entity
        /// </summary>
        [DelayLoad(null)]
        [XmlElement("identifier")]
        public List<EntityIdentifier> Identifiers
        {
            get
            {
                if (this.IsDelayLoadEnabled)
                    this.m_identifiers = EntitySource.Current.GetRelations(this.Key, this.VersionSequence, this.m_identifiers);
                return this.m_identifiers;
            }
        }

        /// <summary>
        /// Gets a list of all associated entities for this entity
        /// </summary>
        [DelayLoad(null)]
        [XmlElement("relationship")]
        public List<EntityRelationship> Relationships
        {
            get
            {
                if (this.IsDelayLoadEnabled)
                    this.m_relationships = EntitySource.Current.GetRelations(this.Key, this.VersionSequence, this.m_relationships);

                return this.m_relationships;
            }
        }

        /// <summary>
        /// Gets a list of all telecommunications addresses associated with the entity
        /// </summary>
        [DelayLoad(null)]
        [XmlElement("telecom")]
        public List<EntityTelecomAddress> Telecom
        {
            get
            {
                if (this.IsDelayLoadEnabled)
                    this.m_telecomAddresses = EntitySource.Current.GetRelations(this.Key, this.VersionSequence, this.m_telecomAddresses);

                return this.m_telecomAddresses;
            }
        }

        /// <summary>
        /// Gets a list of all extensions associated with the entity
        /// </summary>
        [DelayLoad(null)]
        [XmlElement("extension")]
        public List<EntityExtension> Extensions
        {
            get
            {
                if (this.IsDelayLoadEnabled)
                    this.m_extensions = EntitySource.Current.GetRelations(this.Key, this.VersionSequence, this.m_extensions);

                return this.m_extensions;
            }
        }

        /// <summary>
        /// Gets a list of all names associated with the entity
        /// </summary>
        [DelayLoad(null)]
        [XmlElement("name")]
        public List<EntityName> Names
        {
            get
            {
                if (this.IsDelayLoadEnabled)
                    this.m_names = EntitySource.Current.GetRelations(this.Key, this.VersionSequence, this.m_names);

                return this.m_names;
            }
        }

        /// <summary>
        /// Gets a list of all addresses associated with the entity
        /// </summary>
        [DelayLoad(null)]
        [XmlElement("address")]
        public List<EntityAddress> Addresses
        {
            get
            {
                if (this.IsDelayLoadEnabled)
                    this.m_addresses = EntitySource.Current.GetRelations(this.Key, this.VersionSequence, this.m_addresses);

                return this.m_addresses;
            }
        }

        /// <summary>
        /// Gets a list of all notes associated with the entity
        /// </summary>
        [DelayLoad(null)]
        [XmlElement("note")]
        public List<EntityNote> Notes
        {
            get
            {
                if (this.IsDelayLoadEnabled)
                    this.m_notes = EntitySource.Current.GetRelations(this.Key, this.VersionSequence, this.m_notes);

                return this.m_notes;
            }
        }

        /// <summary>
        /// Gets a list of all tags associated with the entity
        /// </summary>
        [DelayLoad(null)]
        [XmlElement("tag")]
        public List<EntityTag> Tags
        {
            get
            {
                if (this.IsDelayLoadEnabled)
                    this.m_tags = EntitySource.Current.GetRelations(this.Key, this.m_tags);

                return this.m_tags;
            }
        }

        /// <summary>
        /// Gets the acts in which this entity participates
        /// </summary>
        [DelayLoad(null)]
        [XmlElement("participation")]
        public List<ActParticipation> Participations
        {
            get
            {
                if (this.IsDelayLoadEnabled)
                    this.m_participations = EntitySource.Current.GetRelations(this.Key, this.VersionSequence, this.m_participations);

                return this.m_participations;
            }
        }

        /// <summary>
        /// Forces the delay load properties in this type to reload
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            this.m_classConcept = this.m_determinerConcept = this.m_statusConcept = this.m_typeConcept = null;
            this.m_addresses = null;
            this.m_relationships = null;
            this.m_creationAct = null;
            this.m_extensions = null;
            this.m_identifiers = null;
            this.m_names = null;
            this.m_notes = null;
            this.m_tags = null;
            this.m_telecomAddresses = null;
        }

    }
}