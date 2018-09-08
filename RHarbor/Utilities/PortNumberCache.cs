using kenzauros.RHarbor.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace kenzauros.RHarbor
{
    /// <summary>
    /// Cache system which stores the port numbers of forwarded local port and helps port number recycling.
    /// </summary>
    internal static class PortNumberCache
    {
        private const int RECOMMENDED_PORT_FROM = 10000;
        private const int RECOMMENDED_PORT_TO = 65535;

        #region File I/O

        private const string FILENAME = nameof(PortNumberCache) + ".json";

        private static string _CacheFilePath;
        /// <summary>
        /// Gets cache file path
        /// </summary>
        public static string CacheFilePath
            => _CacheFilePath ??
            (_CacheFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FILENAME));

        /// <summary>
        /// Loads cached port numbers from the file placed at <see cref="CacheFilePath"/>.
        /// </summary>
        /// <returns></returns>
        public static async Task Load()
        {
            if (File.Exists(CacheFilePath))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        using (var sr = new StreamReader(CacheFilePath, Encoding.UTF8))
                        {
                            RecordDictionary =
                                new JsonSerializer()
                                .Deserialize(sr, typeof(Dictionary<string, List<int>>))
                                as Dictionary<string, List<int>>;
                        }
                    }
                    catch (Exception ex)
                    {
                        MyLogger.Log($"Failed to load {nameof(PortNumberCache)}.", ex);
                    }
                }).ConfigureAwait(false);
            }
            // Initialize empty dictionary
            RecordDictionary = RecordDictionary ?? new Dictionary<string, List<int>>();
        }

        /// <summary>
        /// Saves cached port numbers to the file placed at <see cref="CacheFilePath"/>.
        /// </summary>
        /// <returns></returns>
        public static async Task Save()
        {
            await Task.Run(() =>
            {
                try
                {
                    var dir = Path.GetDirectoryName(CacheFilePath);
                    if (!Directory.Exists(dir)) // Prepare folder
                    {
                        Directory.CreateDirectory(dir);
                    }
                    using (var sr = new StreamWriter(CacheFilePath, false, Encoding.UTF8))
                    {
                        new JsonSerializer() { Formatting = Formatting.Indented }
                            .Serialize(sr, RecordDictionary);
                    }
                }
                catch (Exception ex)
                {
                    MyLogger.Log($"Failed to save {nameof(PortNumberCache)}.", ex);
                }
            }).ConfigureAwait(false);
        }

        #endregion

        #region Cache feature

        /// <summary>
        /// Internal dictionary as store
        /// </summary>
        private static Dictionary<string, List<int>> RecordDictionary { get; set; }

        /// <summary>
        /// Creates a dictionary key for the connection.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static string CreateKey(IConnectionInfo info) => $"{info.GetType().Name}__{info.Id}";

        /// <summary>
        /// Stores a port number as a cache for the connection.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="port"></param>
        public static void AddPortCache(this IConnectionInfo info, int port)
        {
            var key = CreateKey(info);
            if (!RecordDictionary.ContainsKey(key))
            {
                RecordDictionary[key] = new List<int>();
            }
            RecordDictionary[key].Remove(port);
            RecordDictionary[key].Insert(0, port);
            if (RecordDictionary[key].Count > 3)
            {
                RecordDictionary[key].RemoveAt(RecordDictionary[key].Count - 1);
            }
        }

        /// <summary>
        /// Gets available recently-used port for the connection.
        /// Returns 0 if no port is cached or available.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static int GetAvailableCachedPort(this IConnectionInfo info)
        {
            var key = CreateKey(info);
            if (RecordDictionary.ContainsKey(key))
            {
                return RecordDictionary[key].Except(GetUnavailableLocalPorts()).FirstOrDefault();
            }
            return 0;
        }

        #endregion

        #region Utitlity methods

        /// <summary>
        /// Enumerates port numbers which are not used or listened on either TCP or UDP.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<int> GetUnavailableLocalPorts()
        {
            var ipgp = IPGlobalProperties.GetIPGlobalProperties();
            return ipgp.GetActiveTcpConnections().Select(x => x.LocalEndPoint.Port)
                .Union(ipgp.GetActiveTcpListeners().Select(x => x.Port))
                .Union(ipgp.GetActiveUdpListeners().Select(x => x.Port))
                .Where(x => x >= RECOMMENDED_PORT_FROM && x <= RECOMMENDED_PORT_TO);
        }

        /// <summary>
        /// Gets whether the specified port number is currently available or not.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsAvailablePort(int port)
            => GetUnavailableLocalPorts().All(x => x != port);

        #endregion
    }
}
