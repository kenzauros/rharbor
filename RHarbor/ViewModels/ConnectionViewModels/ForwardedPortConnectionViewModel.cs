using System.Threading.Tasks;
using Reactive.Bindings;
using Renci.SshNet;

namespace kenzauros.RHarbor.ViewModels
{
    internal class ForwardedPortConnectionViewModel : ConnectionViewModel
    {
        public ForwardedPort ForwardedPort { get; private set; }
        public ReactiveProperty<string> TypeInitial { get; private set; } = new ReactiveProperty<string>();

        public ForwardedPortConnectionViewModel(ForwardedPort fp)
        {
            DisplayText = new ReactiveProperty<string>();
            ForwardedPort = fp;
            SetDisplayText();
        }

        private void SetDisplayText()
        {
            switch (ForwardedPort)
            {
                case ForwardedPortLocal fp:
                    DisplayText.Value = $"{fp.BoundHost}:{fp.BoundPort} → {fp.Host}:{fp.Port}";
                    TypeInitial.Value = "L";
                    break;
                case ForwardedPortRemote fp:
                    DisplayText.Value = $"{fp.BoundHost}:{fp.BoundPort} → {fp.Host}:{fp.Port}";
                    TypeInitial.Value = "R";
                    break;
                case ForwardedPortDynamic fp:
                    DisplayText.Value = $"{fp.BoundHost}:{fp.BoundPort}";
                    TypeInitial.Value = "D";
                    break;
            }
        }

        public override async Task Connect()
        {
            if (!ForwardedPort.IsStarted)
            {
                await Task.Run(() =>
                {
                    ForwardedPort.Start();
                });
                SetDisplayText();
                IsConnected.Value = true;
            }
        }

        public override async Task Disconnect()
        {
            if (ForwardedPort.IsStarted)
            {
                await Task.Run(() =>
                {
                    ForwardedPort.Stop();
                });
                IsConnected.Value = false;
            }
        }

        public override string ToString()
        {
            return $"[{TypeInitial.Value}] {DisplayText.Value}";
        }
    }
}
