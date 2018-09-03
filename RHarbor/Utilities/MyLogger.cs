using kenzauros.RHarbor.ViewModels;
using System;

namespace kenzauros.RHarbor
{
    internal static class MyLogger
    {
        private static NLog.Logger Logger { get; } = NLog.LogManager.GetCurrentClassLogger();

        public static void Log(string content, Exception e = null)
        {
            if (e != null)
            {
                Logger.Error(e, content);
            }
            else
            {
                Logger.Info(content);
            }
        }

        public static void WriteLog(this ConnectionViewModel vm, string content, Exception e = null)
        {
            if (e != null)
            {
                Logger.Error(e, $"{vm.ToString()} : {content}");
            }
            else
            {
                Logger.Info($"{vm.ToString()} : {content}");
            }
            vm.Log.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} : {content}");
        }

    }
}
