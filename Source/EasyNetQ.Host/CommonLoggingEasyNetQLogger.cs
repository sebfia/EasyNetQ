using System;
using System.Diagnostics;
using Common.Logging;

namespace EasyNetQ.Host
{
    public class CommonLoggingEasyNetQLogger : IEasyNetQLogger
    {
        private readonly ILog _log;

        public CommonLoggingEasyNetQLogger()
        {
            _log = LogManager.GetCurrentClassLogger();
        }

        [DebuggerStepThrough]
        public void DebugWrite(string format, params object[] args)
        {
            _log.Debug(m => m(format, args));
        }

        [DebuggerStepThrough]
        public void InfoWrite(string format, params object[] args)
        {
            _log.Info(m => m(format, args));
        }

        [DebuggerStepThrough]
        public void ErrorWrite(string format, params object[] args)
        {
            try
            {
                _log.Error(m => m(format, args));
            }
            catch (FormatException) { }
        }

        [DebuggerStepThrough]
        public void ErrorWrite(Exception exception)
        {
            _log.Error(m => m("An {0} was thrown!{1}{2}", exception.GetType().Name, Environment.NewLine, exception));
        }
    }
}