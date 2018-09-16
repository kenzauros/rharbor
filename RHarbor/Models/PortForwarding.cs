using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace kenzauros.RHarbor.Models
{
    [NotMapped]
    [Serializable]
    internal class PortForwarding : BindableBase
    {
        [PropertyOrder(1), DisplayName("Type")]
        public string Type { get => _Type; set => SetProp(ref _Type, value); }
        private string _Type = "Local";

        [PropertyOrder(2), DisplayName("Local Host")]
        public string LocalHost { get => _LocalHost; set => SetProp(ref _LocalHost, value); }
        private string _LocalHost;

        [PropertyOrder(3), DisplayName("Local Port"), Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
        public int? LocalPort { get => _LocalPort; set => SetProp(ref _LocalPort, value); }
        private int? _LocalPort;

        [PropertyOrder(4), DisplayName("Remote Host")]
        public string RemoteHost { get => _RemoteHost; set => SetProp(ref _RemoteHost, value); }
        private string _RemoteHost;

        [PropertyOrder(5), DisplayName("Remote Port"), Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
        public int? RemotePort { get => _RemotePort; set => SetProp(ref _RemotePort, value); }
        private int? _RemotePort;

        [PropertyOrder(0), DisplayName("Name")]
        public string Name { get => _Name; set => SetProp(ref _Name, value); }
        private string _Name;
    }
}
