using System;
using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using Android.Content;
using testBeacon.Droid.Services;
using testBeacon.Interfaces;
using UniversalBeacon.Library;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interop;
using Xamarin.Forms;

[assembly: Dependency(typeof(BeaconService))]
namespace testBeacon.Droid.Services
{
    public class BeaconService : IBeaconService
    {
        private string _uuid;
        private bool _isRanging = false;
        private static BluetoothAdapter _adapter;
        private static BLEScanCallback _scanCallback;
        private static BluetoothManager _manager;
        private List<BeaconModel> _beacons= new List<BeaconModel>();

        public bool IsRanging { get { return _isRanging; } }
        public event EventHandler<List<BeaconModel>> BeaconsReceived;

        public BeaconService()
        {
            _adapter = _manager.Adapter;
            _scanCallback = new BLEScanCallback();
        }

        public BeaconService(Context context)
        {
            createManager(context);
        }

        private void createManager(Context context)
        {
            if (_manager == null)
            {
                _manager = (BluetoothManager)context.GetSystemService("bluetooth");
            }
        }

        public void SetUuid(string uuid)
        {
            _uuid = uuid;
        }

        public void Start()
        {
            _isRanging = true;
            _scanCallback.OnAdvertisementPacketReceived += LocationManagerRangBeacons;
            _adapter.BluetoothLeScanner.StartScan(_scanCallback);
        }

        private void LocationManagerRangBeacons(object sender, BLEAdvertisementPacketArgs e)
        {
            if (e.Data.Advertisement.ManufacturerData.Any())
            {
                foreach (var manufacturerData in e.Data.Advertisement.ManufacturerData)
                {
                    // Print the company ID + the raw data in hex format
                    //var manufacturerDataString = $"0x{manufacturerData.CompanyId.ToString("X")}: {BitConverter.ToString(manufacturerData.Data.ToArray())}";
                    //Debug.WriteLine("Manufacturer data: " + manufacturerDataString);

                    var manufacturerDataArry = manufacturerData.Data.ToArray();
                    if (BeaconFrameHelper.IsProximityBeaconPayload(manufacturerData.CompanyId, manufacturerDataArry))
                    {
                        var beaconFrame = new ProximityBeaconFrame(manufacturerDataArry);

                        if (beaconFrame.Uuid == new Guid(_uuid))
                        {
                            var rssi = e.Data.RawSignalStrengthInDBm;
                            var dist = getDistance1(rssi, beaconFrame.TxPower);

                            BeaconsReceived?.Invoke(this, new List<BeaconModel>()
                            {
                                new BeaconModel
                                {
                                     Uuid = beaconFrame.Uuid.ToString(),
                                     Major= beaconFrame.Major,
                                     Minor = beaconFrame.Minor,
                                     Rssi = e.Data.RawSignalStrengthInDBm,
                                     Distinct = dist
                                }
                            });
                        }
                    }
                }
            }
        }

        private double getDistance1(short rssi, sbyte txPower)
        {
            if (rssi == 0) return -1.0;

            var radio = rssi * 1.0 / txPower;
            var dist = 0.0;

            if (radio < 1.0)
            {
                dist = Math.Pow(radio, 10);
            }
            else
            {
                dist = (0.89976) * Math.Pow(radio, 7.7095) + 0.111;
            }

            return dist;
        }

        private double getDistance2(short rssi, sbyte txPower)
        {
            var dist = Math.Pow(10, (double)Math.Abs(rssi - txPower) / (double)(10 * 4));

            return dist;
        }

        public void Stop()
        {
            _isRanging = false;
            _adapter.BluetoothLeScanner.StopScan(_scanCallback);
        }

        private class subBeaconModel:BeaconModel
        {
            public string mac { get; set; }
        }
    }
}
