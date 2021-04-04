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

        /// <summary>
        /// Returns true if the object is <see cref="ConnectionGroup"/> and the <see cref="Name"/> equals to this instance's.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is not ConnectionGroup b) return false;
            return Name == b.Name;
        }

        /// <summary>
        /// Gets the hash code for the <see cref="Name"/>.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;
    }
}
