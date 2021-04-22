using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace kenzauros.RHarbor.Models
{
    [Serializable]
    [DataContract]
    internal class RewriteableBase : BindableBase, IRewriteable
    {
        [IgnoreDataMember]
        [Browsable(false)]
        public virtual IEnumerable<string> RewritingPropertyNames { get; }

        [NonSerialized]
        List<PropertyInfo> _RewritingPropertyInfos;
        List<PropertyInfo> RewritingPropertyInfos
        {
            get
            {
                if (_RewritingPropertyInfos == null)
                {
                    if (RewritingPropertyNames == null)
                    {
                        _RewritingPropertyInfos = GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .Where(x => x.Name != nameof(RewritingPropertyNames))
                            .Where(x => x.CanWrite && x.GetSetMethod(nonPublic: true).IsPublic)
                            .Where(x => x.GetCustomAttribute<KeyAttribute>() == null && x.GetCustomAttribute<RewriteableIgnoreAttribute>() == null) // ignore key properties
                            .ToList();
                    }
                    else
                    {
                        _RewritingPropertyInfos = RewritingPropertyNames
                            .Select(x => GetType().GetProperty(x, BindingFlags.Instance | BindingFlags.Public))
                            .Where(x => x != null)
                            .ToList();
                    }
                }
                return _RewritingPropertyInfos;
            }
        }

        public virtual IRewriteable Clone()
        {
            return (IRewriteable)MemberwiseClone();
        }

        public virtual void RewriteWith(IRewriteable item)
        {
            foreach (var propertyInfo in RewritingPropertyInfos)
            {
                var val1 = propertyInfo.GetValue(item);
                var val2 = propertyInfo.GetValue(this);
                if (val1 != val2)
                {
                    propertyInfo.SetValue(this, val1);
                }
            }
        }
    }
}
