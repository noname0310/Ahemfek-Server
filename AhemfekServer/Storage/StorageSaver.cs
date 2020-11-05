using System;
using System.Collections.Concurrent;
using System.Timers;

namespace AhemfekServer.Storage
{
    class StorageSaver
    {
        public ConcurrentQueue<Action> TimerQueue { get; set; }
        public Timer Timer { get; set; }

        private AhemStorage AhemStorage;

        public StorageSaver(AhemStorage ahemStorage, int minuteSaveinterval)
        {
            TimerQueue = new ConcurrentQueue<Action>();
            AhemStorage = ahemStorage;
            Timer = new Timer();
            Timer.Interval = 1000 * 60 * minuteSaveinterval;
            Timer.Elapsed += Timer_Elapsed;
        }

        public void Start() => Timer.Start();

        public void Stop() => Timer.Stop();

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimerQueue.Enqueue(() => {
                AhemStorage.Save();
            });
        }
    }
}
