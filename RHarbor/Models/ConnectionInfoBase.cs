using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace kenzauros.RHarbor.Models
{
    [Serializable]
    [DataContract]
    internal class ConnectionInfoBase : RewriteableBase, IConnectionInfo, IPassword
    {
        #region ToString

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? $"{Host}:{Port}" : $"{Name} ({Host}:{Port})";

        #endregion

        #region ConnectionType

        [NotMapped]
        [Browsable(false)]
        public virtual ConnectionType ConnectionType { get; }

        #endregion

        [Key]
        [Browsable(false)]
        public long Id { get => _Id; set => SetProp(ref _Id, value); }
        private long _Id;

        [DataMember]
        [Required]
        [LocalizedCategory("ConnectionInfo_Category_General"), PropertyOrder(1)]
        [LocalizedDisplayName(nameof(ConnectionInfoBase) + "_" + nameof(Name))]
        public string Name { get => _Name; set => SetProp(ref _Name, value); }
        private string _Name;

        [DataMember]
        [Required]
        [LocalizedCategory("ConnectionInfo_Category_General"), PropertyOrder(2)]
        [LocalizedDisplayName(nameof(ConnectionInfoBase) + "_" + nameof(Host))]
        public string Host { get => _Host; set => SetProp(ref _Host, value); }
        private string _Host;

        [DataMember]
        [Required]
        [LocalizedCategory("ConnectionInfo_Category_General"), PropertyOrder(3), Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
        [LocalizedDisplayName(nameof(ConnectionInfoBase) + "_" + nameof(Port))]
        public int Port { get => _Port; set => SetProp(ref _Port, value); }
        private int _Port = 3389;

        [DataMember]
        [LocalizedCategory("ConnectionInfo_Category_General"), PropertyOrder(4)]
        [LocalizedDisplayName(nameof(ConnectionInfoBase) + "_" + nameof(GroupName))]
        public string GroupName { get => _GroupName; set => SetProp(ref _GroupName, value); }
        private string _GroupName;

        [DataMember]
        [LocalizedCategory("ConnectionInfo_Category_Authentication"), PropertyOrder(1)]
        [LocalizedDisplayName(nameof(ConnectionInfoBase) + "_" + nameof(Username))]
        public string Username { get => _Username; set => SetProp(ref _Username, value); }
        private string _Username;

        [DataMember]
        [Required]
        [Browsable(false)]
        public bool SaveUsername { get => _SaveUsername; set => SetProp(ref _SaveUsername, value); }
        private bool _SaveUsername = true;

        [Browsable(false)]
        public string Password { get => _Password; set => SetProp(ref _Password, value); }
        private string _Password;

        [DataMember]
        [Required]
        [Browsable(false)]
        public bool SavePassword
        {
            get => _SavePassword;
            set => SetProp(ref _SavePassword, value);
        }
        private bool _SavePassword = false;

        #region IPassword

        [DataMember]
        [NotMapped]
        [LocalizedCategory("ConnectionInfo_Category_Authentication"), PropertyOrder(2)]
        [LocalizedDisplayName(nameof(ConnectionInfoBase) + "_" + nameof(Password))]
        public virtual string RawPassword
        {
            get => _RawPassword;
            set
            {
                if (SetProp(ref _RawPassword, value))
                {
                    this.WritebackPassword();
                }
            }
        }
        private string _RawPassword;

        #endregion

        [DataMember]
        [DefaultValue(false)]
        [LocalizedCategory("ConnectionInfo_Category_Other"), PropertyOrder(10)]
        [LocalizedDisplayName(nameof(ConnectionInfoBase) + "_" + nameof(ShowInJumpList))]
        public bool? ShowInJumpList { get => _ShowInJumpList == true; set => SetProp(ref _ShowInJumpList, value); }
        private bool? _ShowInJumpList = false;

        #region IRewriteable

        public override IRewriteable Clone()
        {
            return this.CloneDeep();
        }

        #endregion

        #region Serialize and deserialize (with encryption)

        /// <summary>
        /// Returns the serialized data of this object.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public byte[] Serialize()
        {
            var serializer = new DataContractJsonSerializer(GetType());
            using var sourceStream = new MemoryStream();
            serializer.WriteObject(sourceStream, this);
            return sourceStream.ToArray();
        }

        /// <summary>
        /// Creates an object from the serialized data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] data) where T : IConnectionInfo
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            var serializer = new DataContractJsonSerializer(typeof(T));
            using var stream = new MemoryStream(data);
            return (T)serializer.ReadObject(stream);
        }

        #endregion
    }
}
