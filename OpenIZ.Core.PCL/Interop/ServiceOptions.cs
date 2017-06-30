﻿using Newtonsoft.Json;
using OpenIZ.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenIZ.Core.Interop
{
    /// <summary>
    /// Service options
    /// </summary>
    [XmlType(nameof(ServiceOptions), Namespace = "http://openiz.org/model"), JsonObject(nameof(ServiceOptions))]
    public class ServiceOptions : IdentifiedData
    {
        /// <summary>
        /// Services offered
        /// </summary>
        public ServiceOptions()
        {
            this.Services = new List<ServiceResourceOptions>();
            this.Endpoints = new List<ServiceEndpointOptions>();
        }

        /// <summary>
        /// Gets or sets the version of the service interface
        /// </summary>
        [XmlAttribute("version"), JsonProperty("version")]
        public String InterfaceVersion { get; set; }

        /// <summary>
        /// Gets the service resource options
        /// </summary>
        [XmlElement("resource"), JsonProperty("resource")]
        public List<ServiceResourceOptions> Services { get; set; }

        /// <summary>
        /// Gets or sets the endpoint options
        /// </summary>
        [XmlElement("endpoint"), JsonProperty("endpoint")]
        public List<ServiceEndpointOptions> Endpoints { get; set; }

        /// <summary>
		/// Gets or sets the modified on date time of the service options.
		/// </summary>
        public override DateTimeOffset ModifiedOn => DateTimeOffset.Now;
    }
}
