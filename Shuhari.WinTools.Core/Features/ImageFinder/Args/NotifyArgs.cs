using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuhari.WinTools.Core.Features.ImageFinder.Args
{
    public abstract class NotifyArgs : EventArgs
    {
        public abstract void Apply(IImageFinderUI win);
    }

}
