using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuhari.WinTools.Core.Features.SysOptimize
{
    public abstract class OptimizeTask
    {
        public abstract string DisplayName { get; }

        public abstract bool Execute();
    }
}
