using Gallio.Framework;
using Gallio.Runtime;
using Gallio.Runtime.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apache.NMS.ZMQ
{
    public class TestTracer : ITrace
    {
        public void Debug(string message)
        {
            RuntimeAccessor.Logger.Log(LogSeverity.Debug, message);
        }

        public void Error(string message)
        {
            RuntimeAccessor.Logger.Log(LogSeverity.Error, message);
        }

        public void Fatal(string message)
        {
            RuntimeAccessor.Logger.Log(LogSeverity.Error, message);
        }

        public void Info(string message)
        {
            RuntimeAccessor.Logger.Log(LogSeverity.Info, message);
        }

        public bool IsDebugEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsFatalEnabled
        {
            get { return true; }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsWarnEnabled
        {
            get { return true; }
        }

        public void Warn(string message)
        {
            RuntimeAccessor.Logger.Log(LogSeverity.Warning, message);
        }
    }
}
