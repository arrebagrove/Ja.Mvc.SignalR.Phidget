using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ja.SignalR.Phidget.Models;
using System.Threading.Tasks;

namespace Ja.Mvc.SignalR.Phidget.Hubs
{
    #region Phidget

    /// <summary>
    /// [10001] ADD: Hubs/PhidgetHubs.cs
    /// </summary>
    public class PhidgetMiddlewareServerHub : Hub
    {

        #region Data Members
        /// <summary>
        /// SignalR clients list connected of type PhidgetsClients.
        /// </summary>
        private static List<SignalRClientDetails> ConnectedPhidgetClients = new List<SignalRClientDetails>();
        /// <summary>
        /// SignalR clients list connected of type PhidgetsClients.
        /// This list will be send to PhidgetClient when he connect.
        /// </summary>
        private static List<SignalRClientDetails> ConnectedPhidgetServers = new List<SignalRClientDetails>();
        #endregion

        #region Server side Hub Events
        public override Task OnConnected()
        {
            MyTrace(string.Format("OnConnected({0})", Context.ConnectionId));

            return base.OnConnected();

        }
        public override Task OnReconnected()
        {
            MyTrace(string.Format("OnReconnected({0})", Context.ConnectionId));

            return base.OnReconnected();

        }
        /// <summary>
        /// The caller can be a PhidgetClient or PhidgetServer.
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            MyTrace(string.Format("OnDisconnected({0})", Context.ConnectionId));

            // Searching if caller is PhidgetClient or PhidgetServer.
            var phidgetClient = ConnectedPhidgetClients.Find(cpc => cpc.ConnectionId == Context.ConnectionId);
            var phidgetServer = ConnectedPhidgetServers.Find(cps => cps.ConnectionId == Context.ConnectionId);
            // If caller is PhidgetClient
            if (phidgetClient != null)
            {
                // remove it from ConnectedPhidgetClients list
                lock (ConnectedPhidgetClients)
                {
                    ConnectedPhidgetClients.Remove(phidgetClient);
                }
                // remove it from PhidgetClient group
                LeavePhidgetClientGroup();
                // remove it from PhidgetServer group
                LeavePhidgetServerGroup(phidgetClient.PhidgetServerGroupAttachement);
            }
            // If caller is PhidgetServer
            if (phidgetServer != null)
            {
                // remove it from ConnectedPhidgetServers list
                lock (ConnectedPhidgetServers)
                {
                    ConnectedPhidgetServers.Remove(phidgetServer);
                }
                // signal all PhidgetClient that phidgetServer is disconnected.
                Clients.Group(SignalRClientType.PhidgetClient).onPhidgetServerDisconnect(phidgetServer);
            }

            return base.OnDisconnected(true);

        }

        #endregion

        #region Server RPC for clients (must be public)
        /// <summary>
        /// After connection, PhidgetServer and PhidgetClient must call AfterConnect.
        /// Put them in cache
        /// If of type PhidgetServer
        /// Signal the new PhidgetServer connection to all PhidgetClient.
        /// If of type PhidgetClient
        /// Join it the PhidgetClient group.
        /// </summary>
        /// <param name="signalRClient">PhidgetClient or PhidgetServer</param>
        public void AfterConnect(SignalRClientDetails signalRClient)
        {
            MyTrace(string.Format("AfterConnect({0})", signalRClient.MachineName));

            // Proceed SignalRClient initilization began on client side and cache it in a list.
            var caller = new SignalRClientDetails
            {
                ConnectionId = Context.ConnectionId,
                IPAddress = signalRClient.IPAddress,
                MachineName = signalRClient.MachineName,
            };
            switch (signalRClient.SignalRClientTypeConnected)
            {
                case SignalRClientType.PhidgetServer:
                    // A PhidgetServer had PhidgetModule attached to USB port.
                    caller.PhidgetModuleTypeAttached = signalRClient.PhidgetModuleTypeAttached;
                    // Add signalRClient to ConnectedPhidgetServers list.
                    lock (ConnectedPhidgetServers)
                    {
                        ConnectedPhidgetServers.Add(caller);
                    }
                    // Signal the new PhidgetServer connection to all PhidgetClient.
                    MyTrace(string.Format("onNewPhidgetServerConnect: {0} / {1}", SignalRClientType.PhidgetClient, caller.MachineName));
                    Clients.Group(SignalRClientType.PhidgetClient).onNewPhidgetServerConnect(caller);
                    break;
                case SignalRClientType.PhidgetClient:
                    // Add signalRClient to ConnectedPhidgetClients list.
                    lock (ConnectedPhidgetClients)
                    {
                        ConnectedPhidgetClients.Add(caller);
                    }
                    // Join signalRClient to "PhidgetClient" group.
                    JoinPhidgetClientGroup();
                    // Send the ConnectedPhidgetServers list to this new PhidgetClient.
                    if (ConnectedPhidgetServers.Count != 0)
                    {
                        MyTrace(string.Format("showPhidgetServerList: {0}", ConnectedPhidgetServers[0].MachineName));
                        Clients.Caller.showPhidgetServerList(ConnectedPhidgetServers);
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// TODO: If convenient, remove the PhidgetClient caller from phidgetServerSelectedOld group.
        /// Join the caller to the phidgetServerSelected Group.
        /// Ask to phidgetServerSelected to push data to the caller.
        /// </summary>
        /// <param name="phidgetServerSelected"></param>
        public void OnPhidgetServerSelected(SignalRClientDetails phidgetServerSelected)
        {
            MyTrace(string.Format("OnPhidgetServerSelected({0})", phidgetServerSelected.MachineName));

            // Find caller PhidgetClient object.
            var phidgetClientIndex = ConnectedPhidgetClients.FindIndex(cpc => cpc.ConnectionId == Context.ConnectionId);
            // If caller is yet in a PhidgetServer group, remove it from that group.
            if (ConnectedPhidgetClients[phidgetClientIndex].PhidgetServerGroupAttachement != null)
            {
                // We remove the PhidgetClient caller from it's old phidgetServer group.
                LeavePhidgetServerGroup(ConnectedPhidgetClients[phidgetClientIndex].PhidgetServerGroupAttachement);
                // Remove PhidgetServer group for the caller.
                ConnectedPhidgetClients[phidgetClientIndex].PhidgetServerGroupAttachement = null;
                MyTrace(string.Format("LeavePhidgetServerGroup: {0} / {1}", phidgetServerSelected.MachineName, Context.ConnectionId));

            }
            // Store PhidgetServer group for the caller.
            ConnectedPhidgetClients[phidgetClientIndex].PhidgetServerGroupAttachement = phidgetServerSelected.MachineName;
            // TODO: We join the caller to phidgetServerSelected group.
            JoinPhidgetServerGroup(phidgetServerSelected.MachineName);
            // Ask to phidgetServerSelected to push data to the caller.
            // We call phidgetServerSelected get method to retreive data.
            // get must then call SendToPhidgetClient to push data to this PhidgetClient.
            MyTrace(string.Format("get: {0} / {1}", phidgetServerSelected.ConnectionId, Context.ConnectionId));
            Clients.Client(phidgetServerSelected.ConnectionId).get(Context.ConnectionId);
        }

        /// <summary>
        /// Called by a phidgetServerSelected to push data to one PhidgetClient.
        /// </summary>
        /// <param name="phidgetClientID"></param>
        /// <param name="data"></param>
        public void SendToPhidgetClient(string phidgetClientID, dynamic data)
        {
            MyTrace(string.Format("SendToPhidgetClient({0},{1})", phidgetClientID, data.ToString()));
            Clients.Client(phidgetClientID).show(data);
        }

        /// <summary>
        /// PhidgetClient call SetByPhidgetClient to set phidgetServer outputs.
        /// PhidgetServer must signal changes to all PhidgetClient attached to him.
        /// </summary>
        public void SetByPhidgetClient(SignalRClientDetails phidgetServer, dynamic data)
        {
            MyTrace(string.Format("SetByPhidgetClient({0},{1})", phidgetServer, data.ToString()));

            // PhidgetMiddlewareServer call phidgetServer set method to set phidgetServer outputs.
            // PhidgetServer must then call SendToPhidgetClients to signal changes to all PhidgetClient attached to him.
            MyTrace(string.Format("set: {0} / {1}", phidgetServer.ConnectionId, data.ToString()));
            Clients.Client(phidgetServer.ConnectionId).set(data);
        }
        /// <summary>
        /// Called by PhidgetServer to signal changes to all PhidgetClient attached to him.
        /// </summary>
        /// <param name="phidgetServer"></param>
        /// <param name="data"></param>
        public void SendToPhidgetClients(SignalRClientDetails phidgetServer, dynamic data)
        {
            MyTrace(string.Format("SendToPhidgetClients({0},{1})", phidgetServer.MachineName, data.ToString()));

            // Retrieve PhidgetServer object.
            //var fromPhidgetServer = ConnectedPhidgetServers.Find(pc => pc.ConnectionId == phidgetServerID);
            // Send data to the group.
            MyTrace(string.Format("show: {0} / {1}", phidgetServer.MachineName, data.ToString()));
            Clients.Group(phidgetServer.MachineName).show(data);
        }

        #endregion

        #region Private methods
        /// <summary>
        /// We have to send PhidgetServer data to all PhidgetClient selecting selecting this PhidgetServer.
        /// So we join PhidgetClient to a PhidgetServer Group.
        /// PhidgetServer Group name = PhidgetServer host name
        /// http://www.asp.net/signalr/overview/guide-to-the-api/working-with-groups
        /// </summary>
        /// <param name="phidgetServerGroupName"></param>
        /// <returns></returns>
        private Task JoinPhidgetServerGroup(string phidgetServerGroupName)
        {

            MyTrace(string.Format("JoinPhidgetServerGroup({0})", phidgetServerGroupName));

            return Groups.Add(Context.ConnectionId, phidgetServerGroupName);
        }
    //private async Task JoinPhidgetServerGroup(string phidgetServerGroupName)
    //{

    //    MyTrace(string.Format("JoinPhidgetServerGroup({0})", phidgetServerGroupName));

    //    await Groups.Add(Context.ConnectionId, phidgetServerGroupName).ContinueWith(task =>
    //    {
    //        if (task.IsFaulted)
    //        {
    //            MyTrace(string.Format("IsFaulted: JoinPhidgetServerGroup({0}) : ", task.Exception.Message));
    //        }
    //    });
    //}
        /// <summary>
        /// To remove PhidgetClient from PhidgetServer Group.
        /// </summary>
        /// <param name="phidgetServerGroup"></param>
        /// <returns></returns>
        private Task LeavePhidgetServerGroup(string phidgetServerGroupName)
        {
            MyTrace(string.Format("LeavePhidgetServerGroup({0})", phidgetServerGroupName));

            return Groups.Remove(Context.ConnectionId, phidgetServerGroupName);
        }
        /// <summary>
        /// We have to push data to all SignalRClient of type PhidgetClient.
        /// So we create a group named "PhidgetClient" and store all PhidgetClient ConnectionId in that group.
        /// </summary>
        /// <returns></returns>
        private Task JoinPhidgetClientGroup()
        {
            MyTrace("JoinPhidgetClientGroup");

            return Groups.Add(Context.ConnectionId, SignalRClientType.PhidgetClient);
        }
        /// <summary>
        /// To remove PhidgetClient from "PhidgetClient" Group.
        /// </summary>
        /// <returns></returns>
        private Task LeavePhidgetClientGroup()
        {
            MyTrace("LeavePhidgetClientGroup");

            return Groups.Remove(Context.ConnectionId, SignalRClientType.PhidgetClient);
        }
        /// <summary>
        /// For server tracing in server and client side.
        /// </summary>
        /// <param name="message"></param>
        private void MyTrace(string message)
        {
            //if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["IsTraceEnable"]))
            //{
            System.Diagnostics.Trace.TraceInformation(message);
            //Clients.All.showServerTrace(message);
            //}
        }

        #endregion

    }
    #endregion
}