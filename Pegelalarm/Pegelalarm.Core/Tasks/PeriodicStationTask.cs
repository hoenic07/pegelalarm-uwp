using Pegelalarm.Core.Data;
using Pegelalarm.Core.Network.Data;
using Pegelalarm.Core.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Pegelalarm.Core.Services
{
    public class PeriodicStationTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var def = taskInstance.GetDeferral();
            var st = await GlobalSettings.Instance.GetMonitoredStations();

            //load stations


            var stationData = new List<Station>();

            foreach (var station in stationData)
            {
                var uist = new UIStation { Data = station };
                if(station.situation != 100 && station.situation >= 30)
                {
                    ShowToast(uist.SituationString, string.Format("{0}, Pegel: {1} {2}, Trend: {3}", station.name, station.data?[0]?.value, station.data?[0]?.type, uist.TrendString));
                }
            }


            def.Complete();
        }

        public void ShowToast(string header, string text)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(header));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(text));

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "long");

            ToastNotification toast = new ToastNotification(toastXml);

            toast.Group = "pegelalarm";
            ToastNotificationManager.History.RemoveGroup("pegelalarm");

            toast.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(3600);

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
