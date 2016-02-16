﻿using MARC.HI.EHRS.SVC.Core.Services;
using OpenIZ.Authentication.OAuth2.Wcf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Authentication.OAuth2
{
    /// <summary>
    /// OAuth2 message handler
    /// </summary>
    public class OAuthMessageHandler : IMessageHandlerService
    {

        // Trace source
        private TraceSource m_traceSource = new TraceSource(OAuthConstants.TraceSourceName);

        // Service host
        private WebServiceHost m_serviceHost;

        /// <summary>
        /// True if is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return this.m_serviceHost != null &&
                    this.m_serviceHost.State == System.ServiceModel.CommunicationState.Opened;
            }
        }

        /// <summary>
        /// Fired when the service is starting
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// Fired when the service is stopping
        /// </summary>
        public event EventHandler Starting;
        /// <summary>
        /// Fired when the service has stopped
        /// </summary>
        public event EventHandler Stopped;
        /// <summary>
        /// Fired when the service is stopping
        /// </summary>
        public event EventHandler Stopping;

        /// <summary>
        /// Start the specified message handler service
        /// </summary>
        public bool Start()
        {
            try
            {
                this.Starting?.Invoke(this, EventArgs.Empty);

                this.m_serviceHost = new WebServiceHost(typeof(OAuthTokenBehavior));

                // Start the webhost
                this.m_serviceHost.Open();

                this.Started?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch(Exception e)
            {
                this.m_traceSource.TraceEvent(TraceEventType.Error, e.HResult, e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Stop the handler
        /// </summary>
        public bool Stop()
        {
            this.Stopping?.Invoke(this, EventArgs.Empty);

            if (this.m_serviceHost != null)
            {
                this.m_serviceHost.Close();
                this.m_serviceHost = null;
            }

            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}