using System.Reactive.Linq;
using System.Threading.Tasks;
using kenzauros.RHarbor.Models;
using Reactive.Bindings;
using Renci.SshNet;

namespace kenzauros.RHarbor.ViewModels
{
    internal class ForwardedPortConnectionViewModel : ConnectionViewModel
    {
        public ReactiveProperty<ForwardedPort> ForwardedPort { get; private set; } = new ReactiveProperty<ForwardedPort>();
        public ReactiveProperty<string> TypeInitial { get; private set; } = new ReactiveProperty<string>();

        public ForwardedPortConnectionViewModel(ForwardedPort forwardedPort, string connectionName)
        {
            ForwardedPort.Value = forwardedPort;
            ConnectionName.Value = connectionName;
            DisplayText = ForwardedPort.CombineLatest(ConnectionName, (fp_, name) =>
            {
                var endpoints = "";
                switch (fp_)
                {
                    case ForwardedPortLocal fp:
                        endpoints = $"{fp.BoundHost}:{fp.BoundPort} => {fp.Host}:{fp.Port}";
                        break;
                    case ForwardedPortRemote fp:
                        endpoints = $"{fp.BoundHost}:{fp.BoundPort} => {fp.Host}:{fp.Port}";
                        break;
                    case ForwardedPortDynamic fp:
                        endpoints = $"{fp.BoundHost}:{fp.BoundPort}";
                        break;
                }
                return string.IsNullOrWhiteSpace(name) ? endpoints : $"{name} ({endpoints})";
            }).ToReactiveProperty();

            TypeInitial = ForwardedPort.CombineLatest(ConnectionName,
                (fp_, name) => fp_.GetType().Name.Replace("ForwardedPort", "")[0].ToString()).ToReactiveProperty();
        }

        public override async Task Connect()
        {
            if (!ForwardedPort.Value.IsStarted)
            {
                await Task.Run(() =>
                {
                    ForwardedPort.Value.Start();
                });
                ForwardedPort.ForceNotify();
                IsConnected.Value = true;
            }
        }

        public override async Task Disconnect()
        {
            if (ForwardedPort.Value.IsStarted)
            {
                await Task.Run(() =>
                {
                    ForwardedPort.Value.Stop();
                });
                IsConnected.Value = false;
            }
        }

        public override string ToString()
        {
            // for logging
            return $"[{TypeInitial.Value}] {DisplayText.Value}";
        }
    }
}
