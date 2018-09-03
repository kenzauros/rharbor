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

        [Key]
        [Browsable(false)]
        public long Id { get { return _Id; } set { SetProp(ref _Id, value); } }
        private long _Id;

        [Required]
        [Category("General"), PropertyOrder(1), DisplayName("Connection Name")]
        public string Name { get { return _Name; } set { SetProp(ref _Name, value); } }
        private string _Name;

        [Required]
        [Category("Remote"), PropertyOrder(1)]
        public string Host { get { return _Host; } set { SetProp(ref _Host, value); } }
        private string _Host;

        [Required]
        [Category("Remote"), PropertyOrder(2), Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
        public int Port { get { return _Port; } set { SetProp(ref _Port, value); } }
        private int _Port = 3389;

        [Category("Authentication"), PropertyOrder(1)]
        public string Username { get { return _Username; } set { SetProp(ref _Username, value); } }
        private string _Username;

        [Required]
        [Browsable(false)]
        public bool SaveUsername { get { return _SaveUsername; } set { SetProp(ref _SaveUsername, value); } }
        private bool _SaveUsername = true;

        [Browsable(false)]
        public string Password { get { return _Password; } set { SetProp(ref _Password, value); } }
        private string _Password;

        [Required]
        [Browsable(false)]
        public bool SavePassword
        {
            get { return _SavePassword; }
            set { if (SetProp(ref _SavePassword, value)) { RaisePropertyChanged(nameof(DisplayPassword)); } }
        }
        private bool _SavePassword = false;

        #region IPassword

        [IgnoreDataMember]
        [NotMapped]
        [Category("Authentication"), PropertyOrder(2), DisplayName("Password")]
        public SecureString SecurePassword
        {
            get { return _SecurePassword; }
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
