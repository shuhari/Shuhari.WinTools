using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuhari.WinTools.Core.Features.SysOptimize
{
    public class OptimizeTaskStatusEventArgs : EventArgs
    {
        public OptimizeTaskStatusEventArgs(OptimizeTask task, OptimizeTaskStatus status)
        {
            this.Task = task;
            this.Status = status;
        }

        public OptimizeTask Task { get; private set; }

        public OptimizeTaskStatus Status { get; private set; }
    }
}
