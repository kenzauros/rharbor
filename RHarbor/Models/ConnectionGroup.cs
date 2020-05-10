namespace kenzauros.RHarbor.Models
{
    /// <summary>
    /// Group which categorize connection informations.
    /// </summary>
    internal class ConnectionGroup
    {
        /// <summary>
        /// Group name that specify in <see cref="IConnectionInfo"/>.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Display name for this group.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Instantiates with the specified identity name and display name.
        /// The identity name is also used as the display name when not specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        public ConnectionGroup(string name, string displayName = null)
        {
            Name = name;
            DisplayName = displayName ?? name;
        }
    }
}
