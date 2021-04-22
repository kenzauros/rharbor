using System.ComponentModel;

namespace kenzauros.RHarbor.Models
{
    internal interface IConnectionInfo : INotifyPropertyChanged
    {
        ConnectionType ConnectionType { get; }

        long Id { get; set; }
        string Name { get; set; }
        string Host { get; set; }
        int Port { get; set; }
        string GroupName { get; set; }

        byte[] Serialize();
    }
}
