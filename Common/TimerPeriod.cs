using System;
using System.Threading.Tasks;
using Common.Log;

namespace Common
{
    // Таймер, который исполняет метод Execute через определенный интервал после окончания исполнения метода Execute
    public abstract class TimerPeriod : IStarter
    {
        private readonly string _componentName;
        private readonly int _periodMs;
        private readonly ILog _log;

        protected TimerPeriod(string componentName, int periodMs, ILog log)
        {
            _componentName = componentName;

            _periodMs = periodMs;
            _log = log;
        }

        public bool Working { get; private set; }

        private void LogFatalError(Exception exception)
        {
            try
            {
                _log.WriteFatalError(_componentName, "Loop", "", exception).Wait();
            }
            catch (Exception)
            {
            }
        }


        private async Task ThreadMethod()
        {
            while (Working)
            {
                try
                {
                    await Execute();
                }
                catch (Exception exception)
                {
                    LogFatalError(exception);
                }
                await Task.Delay(_periodMs);
            }
        }

        protected abstract Task Execute();

        public virtual void Start()
        {

            if (Working)
                return;

            Working = true;
            Task.Run(async () => { await ThreadMethod(); });

        }

        public void Stop()
        {
            Working = false;
        }

    }
}
