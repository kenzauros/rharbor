using kenzauros.RHarbor.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace kenzauros.RHarbor.Models
{
    [Serializable]
    [LocalizedCategoryOrder("ConnectionInfo_Category_General", 1)]
    [LocalizedCategoryOrder("ConnectionInfo_Category_Authentication", 2)]
    [LocalizedCategoryOrder("ConnectionInfo_Category_Screen", 3)]
    [LocalizedCategoryOrder("ConnectionInfo_Category_Other", 4)]
    [Table("rdp_connection_infos")]
    internal class RDPConnectionInfo : ConnectionInfoBase
    {
        #region Static

        private static readonly RDPConnectionInfo Empty = new RDPConnectionInfo();

        #endregion

        public override string ToString() => this == Empty ? $" " : base.ToString();

        [NotMapped]
        [Browsable(false)]
        public override ConnectionType ConnectionType => ConnectionType.RDP;

        [Required]
        [LocalizedCategory("ConnectionInfo_Category_Screen"), PropertyOrder(1)]
        [LocalizedDisplayName(nameof(RDPConnectionInfo) + "_" + nameof(FullScreen))]
        public bool FullScreen { get => _FullScreen; set => SetProp(ref _FullScreen, value); }
        private bool _FullScreen = false;

        [LocalizedCategory("ConnectionInfo_Category_Screen"), PropertyOrder(3), Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
        [LocalizedDisplayName(nameof(RDPConnectionInfo) + "_" + nameof(DesktopWidth))]
        public int? DesktopWidth { get => _DesktopWidth; set { SetProp(ref _DesktopWidth, value); RaisePropertyChanged(nameof(DesktopResulution)); } }
        private int? _DesktopWidth;

        [LocalizedCategory("ConnectionInfo_Category_Screen"), PropertyOrder(4), Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
        [LocalizedDisplayName(nameof(RDPConnectionInfo) + "_" + nameof(DesktopHeight))]
        public int? DesktopHeight { get => _DesktopHeight; set { SetProp(ref _DesktopHeight, value); RaisePropertyChanged(nameof(DesktopResulution)); } }
        private int? _DesktopHeight;

        [Required]
        [LocalizedCategory("ConnectionInfo_Category_Other"), PropertyOrder(1)]
        [LocalizedDisplayName(nameof(RDPConnectionInfo) + "_" + nameof(Admin))]
        public bool Admin { get => _Admin; set => SetProp(ref _Admin, value); }
        private bool _Admin = false;

        [Browsable(false)]
        public string Settings { get => _Settings; set => SetProp(ref _Settings, value); }
        private string _Settings = "";

        [ForeignKey("RequiredConnection")]
        [LocalizedCategory("ConnectionInfo_Category_Other"), PropertyOrder(2)]
        [LocalizedDisplayName(nameof(RDPConnectionInfo) + "_" + nameof(RequiredConnection))]
        public long? RequiredConnectionId
        {
            get => _RequiredConnectionId;
            set => SetProp(ref _RequiredConnectionId, SSHConnectionInfo.Empty.Id.Equals(value) ? null : value);
        }
        private long? _RequiredConnectionId;

        #region Relation Ships

        [RewriteableIgnore]
        [Browsable(false)]
        public virtual SSHConnectionInfo RequiredConnection { get => _RequiredConnection; set { SetProp(ref _RequiredConnection, SSHConnectionInfo.Empty.Equals(value) ? null : value); } }
        [NonSerialized]
        private SSHConnectionInfo _RequiredConnection;

        #endregion

        #region Save as file

        /// <summary>
        /// Saves this connection info as a .rdp file.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void SaveAs(string filepath, string host = null, int? port = null)
        {
            var winposstr = string.Empty;
            if (DesktopWidth.HasValue && DesktopHeight.HasValue)
            {
                var workArea = System.Windows.SystemParameters.WorkArea;
                var (w, h) = (DesktopWidth.Value + 50, DesktopHeight.Value + 100);
                var (l, t) = (Math.Floor((workArea.Width - w) / 2), Math.Floor((workArea.Height - h) / 2));
                var (r, b) = (l + w, t + h);
                winposstr = $"winposstr:s:0,1,{l},{t},{r},{b}";
            }
            var password = string.Empty;
            if (!string.IsNullOrEmpty(RawPassword))
            {
                var bytes = ProtectedData
                    .Protect(Encoding.Unicode.GetBytes(RawPassword), null, DataProtectionScope.CurrentUser)
                    .Select(x => $"{x:X2}");
                password = $"password 51:b:{string.Join("", bytes)}";
                System.Diagnostics.Debug.WriteLine(password);
            }
            File.WriteAllText(filepath,
                $@"
screen mode id:i:{(FullScreen ? 2 : 1)}
{(DesktopWidth.HasValue ? $"desktopwidth:i:{DesktopWidth.Value}" : "")}
{(DesktopHeight.HasValue ? $"desktopheight:i:{DesktopHeight.Value}" : "")}
username:s:{Username}
{password}
use multimon:i:0
session bpp:i:32
{winposstr}
compression:i:1
keyboardhook:i:2
audiocapturemode:i:0
videoplaybackmode:i:1
connection type:i:7
networkautodetect:i:1
bandwidthautodetect:i:1
displayconnectionbar:i:1
enableworkspacereconnect:i:0
disable wallpaper:i:0
allow font smoothing:i:0
allow desktop composition:i:0
disable full window drag:i:1
disable menu anims:i:1
disable themes:i:0
disable cursor setting:i:0
bitmapcachepersistenable:i:1
full address:s:{host ?? Host}:{port ?? (Port > 0 ? Port : 3389)}
audiomode:i:1
redirectprinters:i:1
redirectcomports:i:0
redirectsmartcards:i:1
redirectclipboard:i:1
redirectposdevices:i:0
drivestoredirect:s:
autoreconnection enabled:i:1
authentication level:i:2
prompt for credentials:i:0
negotiate security layer:i:1
remoteapplicationmode:i:0
alternate shell:s:
shell working directory:s:
gatewayhostname:s:
gatewayusagemethod:i:4
gatewaycredentialssource:i:4
gatewayprofileusagemethod:i:0
promptcredentialonce:i:0
gatewaybrokeringtype:i:0
use redirection server name:i:0
rdgiskdcproxy:i:0
kdcproxyname:s:
");
        }

        #endregion

        #region AvailableRequiredConnections

        /// <summary>
        /// Enumerates connection infos that can be assigned to this <see cref="RequiredConnection"/>.
        /// </summary>
        [IgnoreDataMember]
        [RewriteableIgnore]
        [Browsable(false)]
        [NotMapped]
        public virtual IEnumerable<SSHConnectionInfo> AvailableRequiredConnections
            => SSHConnectionInfo.EnumerateAvailableRequiredConnections(null);

        #endregion

        #region Desktop resolution suggestion

        [NotMapped]
        [IgnoreDataMember]
        [RewriteableIgnore]
        [LocalizedCategory("ConnectionInfo_Category_Screen"), PropertyOrder(2)]
        [LocalizedDisplayName(nameof(RDPConnectionInfo) + "_" + nameof(DesktopResulution))]
        public DesktopResolution DesktopResulution
        {
            get => DesktopResolution.Find(DesktopWidth ?? 0, DesktopHeight ?? 0);
            set
            {
                if (value != null)
                {
                    DesktopWidth = value.Width;
                    DesktopHeight = value.Height;
                }
            }
        }

        #endregion
    }
}
