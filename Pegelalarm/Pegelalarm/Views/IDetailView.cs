using Pegelalarm.Core.Data;
using Pegelalarm.Core.Network.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegelalarm.Views
{
    public interface IDetailView
    {
        void ConfigChart(double warnValue, double alarmValue, List<Sample> samples);
        void ShowAlarmSlider();
    }
}
