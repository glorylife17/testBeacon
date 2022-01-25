using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNETCF.IoC;
using Plugin.Permissions;
using testBeacon.Models;
using UniversalBeacon.Library.Core.Entities;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace testBeacon
{
    public partial class MainPage : ContentPage
    {

        private readonly MainViewModel _vm;

        public MainPage()
        {
            InitializeComponent();

            _vm = new MainViewModel();
            BindingContext = _vm;

            Device.BeginInvokeOnMainThread(async () =>
            {
                await _vm.RequestPermissions();
            });

        }

        void btnSearch_Clicked(System.Object sender, System.EventArgs e)
        {
            var dd = _vm.Beacons;
        }
    }

    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BeaconService _service;
        public ObservableCollection<BeaconModel> Beacons { get; set; }
        private Beacon _selectedBeacon;

        public MainViewModel()
        {
            Beacons = new ObservableCollection<BeaconModel>();
        }

        public async Task RequestPermissions()
        {
            await RequestLocationPermission();
        }

        private async Task RequestLocationPermission()
        {
            Debug.WriteLine("Starting beacon service...");
            StartBeaconService();
        }

        private void StartBeaconService()
        {
            _service = RootWorkItem.Services.Get<BeaconService>();
            if (_service == null)
            {
                _service = RootWorkItem.Services.AddNew<BeaconService>();
                if (_service.Beacons != null) _service.Beacons.CollectionChanged += Beacons_CollectionChanged;
            }
        }

        private void Beacons_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine($"Beacons_CollectionChanged {sender} e {e}");

            Beacons.Clear();
            var items = ((IList<Beacon>)sender); //.Where(x=>x.BeaconType== Beacon.BeaconTypeEnum.iBeacon).ToArray();
            foreach (var item in items)
            {
                var obj = new BeaconModel
                {
                    Mac = item.BluetoothAddressAsString,
                    Type = item.BeaconType,
                    Rssi = item.Rssi
                };
                
                if(item.BeaconFrames.Any())
                {
                    var frame = (ProximityBeaconFrame)item.BeaconFrames.First();
                    obj.TxPower = frame.TxPower;
                    obj.Major = frame.Major;
                    obj.Minor = frame.Minor;

                    var dist = obj.Distance;
                }
              
                Beacons.Add(obj);

            }

        }


        public Beacon SelectedBeacon
        {
            get => _selectedBeacon;
            set
            {
                _selectedBeacon = value;
                PropertyChanged.Fire(this, "SelectedBeacon");
            }
        }
    }

    public class BeaconModel
    {
        public string Mac { get; set; }
        public short Rssi { get; set; }
        public Beacon.BeaconTypeEnum Type { get; set; }

        public ushort Major { get; set; }
        public ushort Minor { get; set; }
        public short TxPower { get; set; }

        public double Distance
        {
            get {
                var res = 0.0;

                if (TxPower == 0)
                    return res;

                res = Math.Pow(10, ((TxPower - Rssi) / 100.0));

                return res;
            }
        }

    }
}
