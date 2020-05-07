using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.Rpc
{
    public class CommanderService : Commander.CommanderBase
    {
        public const int DefaultPort = 50051;

        public Func<Task<bool?>> ActivateFunc { get; set; }

        public async override Task<ActivateReply> Activate(Empty request, ServerCallContext context)
        {
            try
            {
                var result = await ActivateFunc?.Invoke();
                return new ActivateReply() { Success = result == true, Message = null };
            }
            catch (Exception ex)
            {
                return new ActivateReply() { Success = false, Message = $"error: {ex.Message}" };
            }
        }

        public override Task<StartConnectReply> StartConnect(StartConnectRequest request, ServerCallContext context)
        {
            try
            {
                ConnectionRequest.Queue.Enqueue((request.ConnectionType, request.ConnectionId));
                return Task.FromResult(new StartConnectReply() { Success = true });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new StartConnectReply() { Success = false, Message = $"error: {ex.Message}" });
            }
        }
    }
}
