using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegelalarm.Views
{
    public interface IMainView
    {

        void ShowMapAt(double lat, double lon, int zoom = 10);
        void MapItemsChanged(object sender, NotifyCollectionChangedEventArgs e);
    }
}
