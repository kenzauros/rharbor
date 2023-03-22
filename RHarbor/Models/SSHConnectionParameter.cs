using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kenzauros.RHarbor.Models
{
    [Serializable]
    [Table("ssh_connection_parameters")]
    internal class SSHConnectionParameter : RewriteableBase
    {
        [Key]
        [Browsable(false)]
        public long Id { get => _Id; set => SetProp(ref _Id, value); }
        private long _Id;

        [ForeignKey("Connection")]
        public long ConnectionId { get => _ConnectionId; set => SetProp(ref _ConnectionId, value); }
        private long _ConnectionId;

        [Required]
        public string Key { get => _Key; set => SetProp(ref _Key, value); }
        private string _Key;

        public string Value { get => _Value; set => SetProp(ref _Value, value); }
        private string _Value;

        [RewriteableIgnore]
        [Browsable(false)]
        public virtual SSHConnectionInfo Connection { get => _Connection; set => SetProp(ref _Connection, value); }
        [NonSerialized]
        private SSHConnectionInfo _Connection;

        public override IEnumerable<string> RewritingPropertyNames => new[] {
            nameof(Key),
            nameof(Value)
        };
    }
}
