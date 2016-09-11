using Pegelalarm.Core.Data;
using Pegelalarm.Core.Network;
using Pegelalarm.Core.Network.Data;
using Pegelalarm.Core.Persistance;
using Pegelalarm.Core.Utils;
using System;
using System.Diagnostics;
using System.Linq;
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
                        ShowToast($"{monFlow.WaterKindStringPlain}-Alarm!", $"{station.water} / {station.stationName}, {flow.value} {monFlow.MetricKindString}", station.commonid);
                        continue;
                    }
                }

                if (monHeight != null && height != null)
                {
                    if (monHeight.WaterKind == WaterKind.Highwater && height.value >= monHeight.AlarmValue ||
                       monHeight.WaterKind == WaterKind.Lowwater && height.value <= monHeight.AlarmValue)
                    {
                        ShowToast($"{monHeight.WaterKindStringPlain}-Alarm!", $"{station.water} / {station.stationName}, {height.value} {monHeight.MetricKindString}", station.commonid);
                        continue;
                    }
                }

                if (isInAlarmRadius && station.situation != 100 && station.situation >= 20 && alarmNotify)
                {
                    ShowToast("Alarm!", $"{station.water} / {station.stationName} - {uist.SituationString}",station.commonid);
                    continue;
                }
            }

            def.Complete();
        }

        public void ShowToast(string header, string text, string group)
        {
            var xml = $"<toast scenario='alarm'>  <visual>    <binding template='ToastGeneric'>      <text>{header}</text>      <text>{text}</text>    </binding>  </visual>  <actions>    <action      activationType='background'      arguments='dismiss'      content='Schließen'/>  </actions></toast>";

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                var t = new ToastNotification(doc);
                t.Group = group;
                ToastNotificationManager.CreateToastNotifier().Show(t);
            }
            catch(Exception ex)
            {

            }
        }
    }
}
