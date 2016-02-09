using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ja.SignalR.Phidget.Models
{
    public class SignalRClientDetails
    {
        public string ConnectionId { get; set; }
        public string MachineName { get; set; }
        public string IPAddress { get; set; }
        public string ConnectionDateTime { get;}

        #region For PhidgetClient only
        /// <summary>
        /// PhidgetServer group to which PhidgetClient belong.
        /// </summary>
        public string PhidgetServerGroupAttachement { get; set; }
        #endregion

        #region For PhidgetServer only
        /// <summary>
        /// PhidgetServer must tell the PhidgetModule type is attached to his USB port.
        /// </summary>
        public string PhidgetModuleTypeAttached { get; set; }
        #endregion

        /// <summary>
        /// SignalRClient must tell if It's PhidgetServer or PhidgetClient type.
        /// </summary>
        public string SignalRClientTypeConnected { get; set; }
        public SignalRClientDetails()
        {
            ConnectionDateTime = DateTime.UtcNow.ToString();
        }

    }
}