﻿/*
 * Copyright 2016-2016 Mohawk College of Applied Arts and Technology
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
 * User: fyfej
 * Date: 2016-1-19
 */



using OpenIZ.Core.Model.Attributes;
using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace OpenIZ.Core.Model.DataTypes
{
    /// <summary>
    /// Represents a relationship between two concepts
    /// </summary>
    
    [XmlType("ConceptRelationship", Namespace = "http://openiz.org/model")]
    public class ConceptRelationship : VersionedAssociation<Concept>
    {

        // Target concept id
        private Guid m_targetConceptId;
        // Target concept
        
        private Concept m_targetConcept;
        // Relaltionship type id
        private Guid m_relationshipTypeId;
        // Relationship type
        
        private ConceptRelationshipType m_relationshipType;

        /// <summary>
        /// Gets or sets the target concept identifier
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        
        [XmlElement("targetConcept")]
        public Guid  TargetConceptKey
        {
            get { return this.m_targetConceptId; }
            set
            {
                this.m_targetConceptId = value;
                this.m_targetConcept = null;
            }
        }

        /// <summary>
        /// Gets or sets the target concept
        /// </summary>
        [DelayLoad(nameof(TargetConceptKey))]
        [XmlIgnore]
        public Concept TargetConcept
        {
            get
            {
                this.m_targetConcept = base.DelayLoad(this.m_targetConceptId, this.m_targetConcept);
                return this.m_targetConcept;
            }
            set
            {
                this.m_targetConcept = value;
                if (value != null)
                    this.m_targetConceptId = value.Key;
                else
                    this.m_targetConceptId = Guid.Empty;
            }
        }

        /// <summary>
        /// Relationship type
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        
        [XmlIgnore]
        public Guid  RelationshipTypeKey {
            get { return this.m_relationshipTypeId; }
            set
            {
                this.m_relationshipTypeId = value;
                this.m_relationshipType = null;
            }
        }

        /// <summary>
        /// Gets or sets the relationship type
        /// </summary>
        [DelayLoad(nameof(RelationshipTypeKey))]
        [XmlIgnore]
        [XmlElement("relationshipType")]
        public ConceptRelationshipType RelationshipType
        {
            get
            {
                this.m_relationshipType = base.DelayLoad(this.m_relationshipTypeId, this.m_relationshipType);
                return this.m_relationshipType;
            }
            set
            {
                this.m_relationshipType = value;
                if (value == null)
                    this.m_relationshipTypeId = Guid.Empty;
                else
                    this.m_relationshipTypeId = value.Key;
            }
        }

        /// <summary>
        /// Force reloading of delay load properties
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            this.m_relationshipType = null;
            this.m_targetConcept = null;
        }
    }
}