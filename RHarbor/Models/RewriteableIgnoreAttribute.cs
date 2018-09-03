using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.Models
{
    /// <summary>
    /// Indicates this member will be ignored in <see cref="RewriteableBase.RewriteWith(IRewriteable)"/> method.
    /// </summary>
    internal class RewriteableIgnoreAttribute : Attribute
    {
    }
}
