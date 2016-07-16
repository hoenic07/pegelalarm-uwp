using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Pegelalarm.Core.Utils
{
    public class Delayer
    {
        private DispatcherTimer _timer;
        public Delayer(TimeSpan timeSpan)
        {
            _timer = new DispatcherTimer() { Interval = timeSpan };
            _timer.Tick += Timer_Tick;
        }

        public event RoutedEventHandler Action;

        private void Timer_Tick(object sender, object e)
        {
            _timer.Stop();
            if (Action != null)
                Action(this, new RoutedEventArgs());
        }

        public void ResetAndTick()
        {
            _timer.Stop();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
