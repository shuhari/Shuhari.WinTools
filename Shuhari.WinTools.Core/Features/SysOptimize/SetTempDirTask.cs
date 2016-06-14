using System;
using System.IO;

namespace Shuhari.WinTools.Core.Features.SysOptimize
{
    public class SetTempDirTask : OptimizeTask
    {
        public SetTempDirTask(string dirName)
        {
            _dirName = dirName;
        }

        private string _dirName;

        public override string DisplayName
        {
            get { return string.Format("设置临时目录为 {0}", _dirName); }
        }

        public override bool Execute()
        {
            if (!Directory.Exists(_dirName))
                Directory.CreateDirectory(_dirName);

            Environment.SetEnvironmentVariable("TEMP", _dirName, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("TMP", _dirName, EnvironmentVariableTarget.User);

            return true;
        }
    }
}
