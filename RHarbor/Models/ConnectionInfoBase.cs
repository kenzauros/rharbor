using kenzauros.RHarbor.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace kenzauros.RHarbor.Models
{
    [Serializable]
    internal class ConnectionInfoBase : RewriteableBase, IConnectionInfo, IPassword
    {
        #region ToString

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? $"{Host}:{Port}" : $"{Name} ({Host}:{Port})";

        #endregion

        [Key]
        [Browsable(false)]
        public long Id { get => _Id; set => SetProp(ref _Id, value); }
        private long _Id;

        [Required]
        [LocalizedCategory("ConnectionInfo_Category_General"), PropertyOrder(1)]
        [LocalizedDisplayName(nameof(ConnectionInfoBase) + "_" + nameof(Name))]
        public string Name { get => _Name; set => SetProp(ref _Name, value); }
        private string _Name;

        [Required]
        [LocalizedCategory("ConnectionInfo_Category_General"), PropertyOrder(2)]
        [LocalizedDisplayName(nameof(ConnectionInfoBase) + "_" + nameof(Host))]
        public string Host { get => _Host; set => SetProp(ref _Host, value); }
        private string _Host;

        [Required]
        [LocalizedCategory("ConnectionInfo_Category_General"), PropertyOrder(3), Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
        [LocalizedDisplayName(nameof(ConnectionInfoBase) + "_" + nameof(Port))]
        public int Port { get => _Port; set => SetProp(ref _Port, value); }
        private int _Port = 3389;

        [LocalizedCategory("ConnectionInfo_Category_Authentication"), PropertyOrder(1)]
        [LocalizedDisplayName(nameof(ConnectionInfoBase) + "_" + nameof(Username))]
        public string Username { get => _Username; set => SetProp(ref _Username, value); }
        private string _Username;

        [Required]
        [Browsable(false)]
        public bool SaveUsername { get => _SaveUsername; set => SetProp(ref _SaveUsername, value); }
        private bool _SaveUsername = true;

        [Browsable(false)]
        public string Password { get => _Password; set => SetProp(ref _Password, value); }
        private string _Password;

        [Required]
        [Browsable(false)]
        public bool SavePassword
        {
            get => _SavePassword;
            set { if (SetProp(ref _SavePassword, value)) { RaisePropertyChanged(nameof(DisplayPassword)); } }
        }
        private bool _SavePassword = false;

        #region IPassword

        [IgnoreDataMember]
        [NotMapped]
        [LocalizedCategory("ConnectionInfo_Category_Authentication"), PropertyOrder(2)]
        [LocalizedDisplayName(nameof(ConnectionInfoBase) + "_" + nameof(Password))]
        public virtual SecureString SecurePassword
        {
            get => _SecurePassword;
            set
            {
                if (SetProp(ref _SecurePassword, value))
                {
                    this.WritebackPassword();
                }
            }
        }
        [NonSerialized]
        private SecureString _SecurePassword;

        [IgnoreDataMember]
        [NotMapped]
        [Browsable(false)]
        public string DisplayPassword => SecurePassword?.Length > 0 ? new string('*', 8) : "(Not Set)";

        #endregion

        #region IRewriteable

        public override IRewriteable Clone()
        {
            return this.CloneDeep();
        }

        #endregion

    }
}
