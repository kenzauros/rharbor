using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace kenzauros.RHarbor
{
    internal class LocalizedCategoryAttribute : CategoryAttribute
    {
        public LocalizedCategoryAttribute(string resourceKey)
            : base(LocalizedResources.GetString(resourceKey)) { }
    }

    internal class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        public LocalizedDisplayNameAttribute(string resourceKey)
            : base(LocalizedResources.GetString(resourceKey)) { }
    }

    internal class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        public LocalizedDescriptionAttribute(string resourceKey)
            : base(LocalizedResources.GetString(resourceKey)) { }
    }

    internal class LocalizedCategoryOrderAttribute : CategoryOrderAttribute
    {
        public LocalizedCategoryOrderAttribute(string resourceKey, int order)
            : base(LocalizedResources.GetString(resourceKey), order) { }
    }
}
