using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.Models
{
    internal interface IRewriteable
    {
        IRewriteable Clone();
        void RewriteWith(IRewriteable item);
    }
}
