using kenzauros.RHarbor.Models;
using System;
using System.ServiceModel;

namespace kenzauros.RHarbor.IPC
{
    [ServiceContract]
    internal interface IProcessCommander
    {
        [OperationContract]
        void Activate();
        [OperationContract]
        void Invoke(ConnectionType connectionType, int id);
    }
}
