using System.Threading.Tasks;

namespace kenzauros.RHarbor.Models
{
    internal interface IConnectionInfo
    {
        long Id { get; set; }
        string Name { get; set; }
        string Host { get; set; }
        int Port { get; set; }
    }
}
