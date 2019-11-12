using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.Utilities;
using System;

namespace kenzauros.RHarbor.IPC
{
    [System.ServiceModel.ServiceBehavior(InstanceContextMode = System.ServiceModel.InstanceContextMode.Single)]
    internal class ProcessCommander : IProcessCommander
    {
        public IConnectionRequest ConnectionRequest { get; set; }

        /// <summary>
        /// Activates the main window.
        /// </summary>
        public void Activate()
        {
            App.Current?.MainWindow?.Activate();
            MyLogger.Log($"Requested to activate the primary window.");
        }

        /// <summary>
        /// Invokes to connect the specified host.
        /// </summary>
        /// <param name="connectionType"></param>
        /// <param name="id"></param>
        public void Invoke(ConnectionType connectionType, int id)
        {
            if (ConnectionRequest == null)
                throw new InvalidOperationException($"{nameof(ConnectionRequest)} property has not been set.");
            MyLogger.Log($"Connection (Type: {connectionType}, Id: {id}) invocation request received.");
            ConnectionRequest.Queue.Enqueue((connectionType, id));
        }
    }
}
