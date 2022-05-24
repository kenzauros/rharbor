﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace kenzauros.RHarbor.Models
{
    [Serializable]
    [DataContract]
    [LocalizedCategoryOrder("ConnectionInfo_Category_General", 1)]
    [LocalizedCategoryOrder("ConnectionInfo_Category_Authentication", 2)]
    [LocalizedCategoryOrder("ConnectionInfo_Category_KeepAlive", 3)]
    [LocalizedCategoryOrder("ConnectionInfo_Category_Other", 4)]
    [Table("ssh_connection_infos")]
    internal class SSHConnectionInfo : ConnectionInfoBase
    {
        #region Static

        public static List<SSHConnectionInfo> All { get; private set; }

        [NotMapped]
        [Browsable(false)]
        public override ConnectionType ConnectionType => ConnectionType.SSH;

        public static async Task RefreshAll(AppDbContext db)
        {
            All = await db.SSHConnectionInfos
                .Include("RequiredConnection")
                .Include("ConnectionParameters")
                .ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Empty element
        /// </summary>
        public static readonly SSHConnectionInfo Empty = new SSHConnectionInfo();

        /// <summary>
        /// Enumerates SSH connection infos which not related with this connection info.
        /// </summary>
        /// <param name="sourceIdToExlude"></param>
        /// <returns></returns>
        public static IEnumerable<SSHConnectionInfo> EnumerateAvailableRequiredConnections(long? sourceIdToExlude)
        {
            var relatedInfoIdList = new List<long>();
            if (sourceIdToExlude.HasValue)
            {
                relatedInfoIdList.Add(sourceIdToExlude.Value); // self
                void func(long sourceId)
                {
                    var relatedInfoIds = All
                        .Where(x => x.RequiredConnectionId == sourceId)
                        .Select(x => x.Id);
                    if (relatedInfoIds.Any())
                    {
                        relatedInfoIdList.AddRange(relatedInfoIds);
                        foreach (var id in relatedInfoIds)
                        {
                            func(id);
                        }
                    }
                }
                func(sourceIdToExlude.Value);
            }
            yield return Empty; // to set null to RequiredConnection column. See the setter of RequiredConnectionId.
            foreach (var item in All.Where(x => !relatedInfoIdList.Contains(x.Id)))
            {
                yield return item;
            }
        }


        #endregion

        public override string ToString() => this == Empty ? $" " : base.ToString();

        public SSHConnectionInfo()
        {
            Port = 22;
            ConnectionParameters = new ObservableCollection<SSHConnectionParameter>();
        }

        [LocalizedCategory("ConnectionInfo_Category_Authentication"), PropertyOrder(4)]
        [LocalizedDisplayName(nameof(SSHConnectionInfo) + "_" + nameof(PrivateKeyFilePath))]
        public string PrivateKeyFilePath { get => _PrivateKeyFilePath; set => SetProp(ref _PrivateKeyFilePath, value); }
        private string _PrivateKeyFilePath;

        [Browsable(false)]
        public string ExpectedFingerPrint { get => _ExpectedFingerPrint; set => SetProp(ref _ExpectedFingerPrint, value); }
        private string _ExpectedFingerPrint;

        [DataMember]
        [Required]
        [LocalizedCategory("ConnectionInfo_Category_KeepAlive"), PropertyOrder(1)]
        [LocalizedDisplayName(nameof(SSHConnectionInfo) + "_" + nameof(KeepAliveEnabled))]
        public bool KeepAliveEnabled { get => _KeepAliveEnabled; set => SetProp(ref _KeepAliveEnabled, value); }
        private bool _KeepAliveEnabled = false;

        [DataMember]
        [Required]
        [LocalizedCategory("ConnectionInfo_Category_KeepAlive"), PropertyOrder(2)]
        [LocalizedDisplayName(nameof(SSHConnectionInfo) + "_" + nameof(KeepAliveInterval))]
        public int KeepAliveInterval { get => _KeepAliveInterval; set => SetProp(ref _KeepAliveInterval, value); }
        private int _KeepAliveInterval = 10000;

        [ForeignKey("RequiredConnection")]
        [LocalizedCategory("ConnectionInfo_Category_Other"), PropertyOrder(1)]
        [LocalizedDisplayName(nameof(SSHConnectionInfo) + "_" + nameof(RequiredConnection))]
        public long? RequiredConnectionId
        {
            get => _RequiredConnectionId;
            set => SetProp(ref _RequiredConnectionId, Empty.Id.Equals(value) ? null : value);
        }
        private long? _RequiredConnectionId;

        [DataMember]
        [LocalizedCategory("ConnectionInfo_Category_Other"), PropertyOrder(2)]
        [LocalizedDisplayName(nameof(SSHConnectionInfo) + "_" + nameof(ConnectionTimeout))]
        public int? ConnectionTimeout { get => _ConnectionTimeout; set => SetProp(ref _ConnectionTimeout, value); }
        private int? _ConnectionTimeout = null;

        [DataMember]
        [LocalizedCategory("ConnectionInfo_Category_Other"), PropertyOrder(4)]
        [LocalizedDisplayName(nameof(SSHConnectionInfo) + "_" + nameof(AlwaysForwardPorts))]
        public bool? AlwaysForwardPorts { get => _AlwaysForwardPorts ?? false; set => SetProp(ref _AlwaysForwardPorts, value); }
        private bool? _AlwaysForwardPorts = null;

        [RewriteableIgnore]
        [Browsable(false)]
        public string PortForwardings
        {
            get => _PortForwardings;
            set
            {
                SetProp(ref _PortForwardings, value);
                PortForwardingCollection = (value != null)
                    ? new ObservableCollection<PortForwarding>(JsonConvert.DeserializeObject<PortForwarding[]>(value))
                    : new ObservableCollection<PortForwarding>();
            }
        }
        private string _PortForwardings;

        [DataMember]
        [RewriteableIgnore]
        [Browsable(false)]
        [NotMapped]
        public ObservableCollection<PortForwarding> PortForwardingCollection
        {
            get { return _PortForwardingCollection ?? (_PortForwardingCollection = new ObservableCollection<PortForwarding>()); }
            set => SetProp(ref _PortForwardingCollection, value);
        }
        private ObservableCollection<PortForwarding> _PortForwardingCollection;

        #region Relation Ships

        [RewriteableIgnore]
        [Browsable(false)]
        public virtual SSHConnectionInfo RequiredConnection { get => _RequiredConnection; set { SetProp(ref _RequiredConnection, Empty.Equals(value) ? null : value); } }
        [NonSerialized]
        private SSHConnectionInfo _RequiredConnection;

        [RewriteableIgnore]
        [Browsable(false)]
        [NotMapped]
        public virtual ICollection<RDPConnectionInfo> RDPConnections { get; set; }

        #region ConnectionParameters

        [RewriteableIgnore] // Rewrite manually in RewriteWith
        [LocalizedCategory("ConnectionInfo_Category_Other"), PropertyOrder(10)]
        [LocalizedDisplayName(nameof(SSHConnectionInfo) + "_" + nameof(ConnectionParameters))]
        public virtual ICollection<SSHConnectionParameter> ConnectionParameters
        {
            get => _ConnectionParameters;
            set => SetProp(ref _ConnectionParameters, value);
        }
        private ICollection<SSHConnectionParameter> _ConnectionParameters;

        #endregion

        #endregion

        #region IRewriteable

        public override void RewriteWith(IRewriteable item)
        {
            base.RewriteWith(item);
            AlwaysForwardPorts ??= false; // Set default value
            if (item is SSHConnectionInfo org)
            {
                // PortForwardings (serialize)
                PortForwarding[] portForwardings = org.PortForwardingCollection?.ToArray();
                PortForwardings = portForwardings != null ? JsonConvert.SerializeObject(portForwardings) : null;

                // ConnectionParameters (filter empty key records)
                ConnectionParameters = new ObservableCollection<SSHConnectionParameter>(
                    org.ConnectionParameters.Where(x => !string.IsNullOrWhiteSpace(x.Key)));
            }
        }

        #endregion

        #region IPassword

        // NOTE: the override below is just to overwrite DisplayName attribute

        [IgnoreDataMember]
        [NotMapped]
        [LocalizedCategory("ConnectionInfo_Category_Authentication"), PropertyOrder(2)]
        [LocalizedDisplayName(nameof(SSHConnectionInfo) + "_" + nameof(Password))]
        public override string RawPassword
        {
            get => base.RawPassword;
            set => base.RawPassword = value;
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
             => EnumerateAvailableRequiredConnections(Id);

        #endregion

    }
}
