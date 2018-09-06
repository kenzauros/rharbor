using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace kenzauros.RHarbor.Models
{
    [Serializable]
    [CategoryOrder("General", 1)]
    [CategoryOrder("Remote", 2)]
    [CategoryOrder("Authentication", 3)]
    [CategoryOrder("KeepAlive", 4)]
    [CategoryOrder("Optional Settings", 5)]
    [CategoryOrder("Other", 7)]
    [Table("ssh_connection_infos")]
    internal class SSHConnectionInfo : ConnectionInfoBase
    {
        #region Static

        public static IEnumerable<SSHConnectionInfo> All { get; set; }

        public static async Task RefreshAll(AppDbContext db)
        {
            All = await db.SSHConnectionInfos
                .Include("RequiredConnection")
                .ToListAsync().ConfigureAwait(false);
        }

        public static readonly SSHConnectionInfo Empty = new SSHConnectionInfo();

        #endregion

        public override string ToString() => this == Empty ? $" " : $"{Name} ({Host}:{Port})";

        public SSHConnectionInfo()
        {
            Port = 22;
        }

        [Category("Authentication"), PropertyOrder(4), DisplayName("Private Key File")]
        public string PrivateKeyFilePath { get { return _PrivateKeyFilePath; } set { SetProp(ref _PrivateKeyFilePath, value); } }
        private string _PrivateKeyFilePath;

        [Browsable(false)]
        public string ExpectedFingerPrint { get { return _ExpectedFingerPrint; } set { SetProp(ref _ExpectedFingerPrint, value); } }
        private string _ExpectedFingerPrint;

        [Required]
        [Category("KeepAlive"), PropertyOrder(1), DisplayName("Enabled")]
        public bool KeepAliveEnabled { get { return _KeepAliveEnabled; } set { SetProp(ref _KeepAliveEnabled, value); } }
        private bool _KeepAliveEnabled = false;

        [Required]
        [Category("KeepAlive"), PropertyOrder(2), DisplayName("Interval")]
        public int KeepAliveInterval { get { return _KeepAliveInterval; } set { SetProp(ref _KeepAliveInterval, value); } }
        private int _KeepAliveInterval = 10000;

        [ForeignKey("RequiredConnection")]
        [Category("Optional Settings"), PropertyOrder(1), DisplayName("Required Connection")]
        public long? RequiredConnectionId { get { return _RequiredConnectionId; } set { SetProp(ref _RequiredConnectionId, value); } }
        private long? _RequiredConnectionId;

        [RewriteableIgnore]
        [Browsable(false)]
        public string PortForwardings
        {
            get { return _PortForwardings; }
            set
            {
                SetProp(ref _PortForwardings, value);
                PortForwardingCollection = (value != null)
                    ? new ObservableCollection<PortForwarding>(JsonConvert.DeserializeObject<PortForwarding[]>(value))
                    : new ObservableCollection<PortForwarding>();
            }
        }
        private string _PortForwardings;

        [IgnoreDataMember]
        [RewriteableIgnore]
        [Browsable(false)]
        [NotMapped]
        public ObservableCollection<PortForwarding> PortForwardingCollection { get { return _PortForwardingCollection; } set { SetProp(ref _PortForwardingCollection, value); } }
        private ObservableCollection<PortForwarding> _PortForwardingCollection;

        #region Relation Ships

        [RewriteableIgnore]
        [Browsable(false)]
        public virtual SSHConnectionInfo RequiredConnection { get { return _RequiredConnection; } set { SetProp(ref _RequiredConnection, (value == Empty) ? null : value); } }
        [NonSerialized]
        private SSHConnectionInfo _RequiredConnection;

        [RewriteableIgnore]
        [Browsable(false)]
        [NotMapped]
        public virtual ICollection<RDPConnectionInfo> RDPConnections { get; set; }

        #endregion

        #region IRewriteable

        public override void RewriteWith(IRewriteable item)
        {
            base.RewriteWith(item);
            var portForwardings = (item as SSHConnectionInfo).PortForwardingCollection?.ToArray();
            PortForwardings = portForwardings != null ? JsonConvert.SerializeObject(portForwardings) : null;
        }

        #endregion

        #region AvailableRequiredConnections

        /// <summary>
        /// Enumerates connection infos that can be assigned to this <see cref="RequiredConnection"/>.
        /// This is used for binding in XAML.
        /// </summary>
        [IgnoreDataMember]
        [RewriteableIgnore]
        [Browsable(false)]
        [NotMapped]
        public virtual IEnumerable<SSHConnectionInfo> AvailableRequiredConnections
        {
            get
            {
                var related = new List<long>() {
                    Id // self
                };
                void func(long sourceId)
                {
                    var relatedInfoIds = All
                        .Where(x => x.RequiredConnectionId == sourceId)
                        .Select(x => x.Id);
                    if (relatedInfoIds.Any())
                    {
                        related.AddRange(relatedInfoIds);
                        foreach (var id in relatedInfoIds)
                        {
                            func(id);
                        }
                    }
                }
                func(Id);
                var list = new List<SSHConnectionInfo>()
                {
                    Empty // to set null to RequiredConnection column. See the setter of RequiredConnection.
                };
                var remains = All.Where(x => !related.Contains(x.Id));
                list.AddRange(remains);
                return list;
            }
        }

        #endregion

    }
}
