using kenzauros.RHarbor.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Shell;

namespace kenzauros.RHarbor.Utilities
{
    internal class JumpListHelper
    {
        public static JumpList JumpListInstance => JumpList.GetJumpList(App.Current);

        public static void RenewJumpList(IEnumerable<ConnectionInfoBase> connections)
        {
            var jumpList = new JumpList();
            jumpList.BeginInit();
            connections.Where(x => x.ShowInJumpList == true)
                .ToList()
                .ForEach(x => AddConnectionJumpTask(jumpList, x));
            jumpList.EndInit();
            jumpList.Apply();
        }

        public static void AddConnectionJumpTask<T>(JumpList jumpList, T connectionInfo) where T : ConnectionInfoBase
        {
            var jumpTask = new JumpTask()
            {
                CustomCategory = connectionInfo.ConnectionType.ToString(),
                Title = connectionInfo.Name,
                Description = connectionInfo.ToString(),
                Arguments = $"/open={connectionInfo.ConnectionType.ToString().ToLower()}:{connectionInfo.Id}",
            };
            jumpList.JumpItems.Add(jumpTask);
        }
    }
}
