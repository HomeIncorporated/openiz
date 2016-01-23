﻿using OpenIZ.Core.Model.Attributes;
using OpenIZ.Core.Model.Constants;
using OpenIZ.Core.Model.DataTypes;
using OpenIZ.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Core.Model.Entities
{
    /// <summary>
    /// An associative entity which links a SecurityApplication to an Entity
    /// </summary>
    
    [XmlType("ApplicationEntity", Namespace = "http://openiz.org/model")]
    [XmlRoot(Namespace = "http://openiz.org/model", ElementName = "ApplicationEntity")]
    public class ApplicationEntity : Entity
    {
        // Security application key
        private Guid m_securityApplicationKey;
        // Security application
        
        private SecurityApplication m_securityApplication;

        /// <summary>
        /// Application entity
        /// </summary>
        public ApplicationEntity()
        {
            base.DeterminerConceptKey = DeterminerKeys.Specific;
            base.ClassConceptKey = EntityClassKeys.Entity;
        }

        /// <summary>
        /// Gets or sets the security application
        /// </summary>
        [XmlElement("securityApplication")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        
        public Guid SecurityApplicationKey
        {
            get { return this.m_securityApplicationKey; }
            set
            {
                this.m_securityApplicationKey = value;
                this.m_securityApplication = null;
            }
        }

        /// <summary>
        /// Gets or sets the security application
        /// </summary>
        [DelayLoad(nameof(SecurityApplicationKey))]
        [XmlIgnore]
        public SecurityApplication SecurityApplication
        {
            get {
                this.m_securityApplication= base.DelayLoad(this.m_securityApplicationKey, this.m_securityApplication);
                return this.m_securityApplication;
            }
            set
            {
                this.m_securityApplication = value;
                if (value == null)
                    this.m_securityApplicationKey = Guid.Empty;
                else
                    this.m_securityApplicationKey = value.Key;
            }
        }

        /// <summary>
        /// Gets or sets the name of the software
        /// </summary>
        [XmlElement("softwareName")]
        public String SoftwareName { get; set; }

        /// <summary>
        /// Gets or sets the version of the software
        /// </summary>
        [XmlElement("versionName")]
        public String VersionName { get; set; }

        /// <summary>
        /// Gets or sets the vendoer name of the software
        /// </summary>
        [XmlElement("vendorName")]
        public String VendorName { get; set; }

        /// <summary>
        /// Force delay loading
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            this.m_securityApplication = null;
        }
    }
}