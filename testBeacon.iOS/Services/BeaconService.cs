using System;
using System.Linq;
using System.Collections.Generic;
using CoreLocation;
using Foundation;
using testBeacon.Interfaces;
using testBeacon.iOS.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(BeaconService))]
namespace testBeacon.iOS.Services
{
    public class BeaconService : IBeaconService
    {
        private string _uuid;
        private CLLocationManager _locationManager;
        private CLBeaconRegion _region;
        private bool _isRanging = false;

        public bool IsRanging { get { return _isRanging; } }
        public event EventHandler<List<BeaconModel>> BeaconsReceived;

        public BeaconService()
        {
            _locationManager = new CLLocationManager();
            _locationManager.RequestWhenInUseAuthorization();
            _locationManager.DidRangeBeacons += LocationManagerRangBeacons;
        }

        public void SetUuid(string uuid)
        {
            _uuid = uuid;
            _region = new CLBeaconRegion(new NSUuid(_uuid), "My region");
        }

        public void Start()
        {
            if (_region == null)
                return;

            _locationManager.StartRangingBeacons(_region);
            _isRanging = true;
        }

        public void Stop()
        {
            _isRanging = false;

            _locationManager.StopRangingBeacons(_region);
        }

        private void LocationManagerRangBeacons(object sender, CLRegionBeaconsRangedEventArgs e)
        {
            if (e.Beacons.Length > 0)
            {
                BeaconsReceived?.Invoke(this, e.Beacons
                    .Select(x => new BeaconModel{
                        Distinct = x.Accuracy,
                        Uuid =x.Uuid.ToString(),
                        Major =(ushort) x.Major,
                        Minor =(ushort) x.Minor,
                        Rssi =(short)  x.Rssi
                }).ToList());
            }
        }

    }
}
