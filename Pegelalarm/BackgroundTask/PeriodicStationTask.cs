using Pegelalarm.Core.Data;
using Pegelalarm.Core.Network;
using Pegelalarm.Core.Network.Data;
using Pegelalarm.Core.Persistance;
using Pegelalarm.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Devices.Geolocation;
using Windows.UI.Notifications;

namespace BackgroundTask
{
    public sealed class PeriodicStationTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background task!");
            var loc = GlobalSettings.Instance.LocationRange;
            if (loc.Latitude == 0 && loc.Longitude == 0)
            {
                return;
            }

            var def = taskInstance.GetDeferral();

            var gp = new Geopoint(new BasicGeoposition { Latitude = loc.Latitude, Longitude = loc.Longitude });
            var bb = GeoUtils.CalculateBoundingBoxAroundPosition(gp, loc.DisplayRadius * 1000);

            var monitoredStations = await GlobalSettings.Instance.GetMonitoredStations();
            var alarmNotify = GlobalSettings.Instance.AlarmRangeNotificationsOn;

            if (monitoredStations.Count == 0 && !alarmNotify)
            {
                def.Complete();
                return;
            }

            var allStations = await (new WebService().GetStationsBy(bb));

            if (!allStations.IsSuccessful)
            {
                def.Complete();
                return;
            }

            

            foreach (var station in allStations.Payload)
            {
                var uist = new UIStation { Data = station };
                var stGp = new Geopoint(new BasicGeoposition { Latitude = station.latitude, Longitude = station.longitude });

                //check if station is in alarm radius
                var isInAlarmRadius = GeoUtils.GetDistanceInMetersBetween(gp, stGp) < loc.AlarmRadius * 1000;

                var monFlow = monitoredStations.FirstOrDefault(s => s.StationId == station.commonid && s.MetricKind==MetricKind.Flow);
                var monHeight = monitoredStations.FirstOrDefault(s => s.StationId == station.commonid && s.MetricKind==MetricKind.Height);

                var flow = station.data.FirstOrDefault(d => d.Metric == MetricKind.Flow);
                var height = station.data.FirstOrDefault(d => d.Metric == MetricKind.Height);
                
                if (monFlow != null && flow!=null)
                {
                    if(monFlow.WaterKind==WaterKind.Highwater && flow.value >= monFlow.AlarmValue ||
                       monFlow.WaterKind == WaterKind.Lowwater && flow.value <= monFlow.AlarmValue)
                    {
                        ShowToast($"{monFlow.WaterKindString}-Alarm!", $"{station.name}, {flow.value} {monFlow.MetricKindString}");
                        continue;
                    }
                }

                if (monHeight != null && height != null)
                {
                    if (monHeight.WaterKind == WaterKind.Highwater && height.value >= monHeight.AlarmValue ||
                       monHeight.WaterKind == WaterKind.Lowwater && height.value <= monHeight.AlarmValue)
                    {
                        ShowToast($"{monFlow.WaterKindString}-Alarm!", $"{station.name}, {flow.value} {monFlow.MetricKindString}");
                        continue;
                    }
                }

                if (isInAlarmRadius && station.situation != 100 && station.situation >= 20 && alarmNotify)
                {
                    ShowToast("Alarm!", station.name + " - " + uist.SituationString);
                    continue;
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
