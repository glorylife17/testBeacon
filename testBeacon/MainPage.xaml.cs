using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using testBeacon.Interfaces;
using testBeacon.ViewModels;
using Xamarin.Forms;

namespace testBeacon
{
    public partial class MainPage : ContentPage
    {

        readonly string _uuid = "FF524CBC-BF93-4BD7-9147-FEAE039C820A";
        private readonly MainViewModel _vm;
        IBeaconService _serivce;

        public MainPage()
        {
            InitializeComponent();

            _vm = new MainViewModel();
            BindingContext = _vm;

            _serivce = DependencyService.Get<IBeaconService>();

            _serivce.SetUuid(_uuid);
            _serivce.BeaconsReceived += EventBeaconsReceived;
        }

        void btnSearch_Clicked(System.Object sender, System.EventArgs e)
        {
            if (!_serivce.IsRanging)
            {
                _serivce.Start();
            }
            else
            {
                _serivce.Stop();
            }

        }

        private void EventBeaconsReceived(object sender, List<BeaconModel> beacons)
        {
            _vm.CreateAndUpdateBeacons(beacons);
        }
    }

    internal class MainViewModel : BaseViewModel
    {
        public ObservableCollection<BeaconModel> Beacons { get; set; }

        public MainViewModel()
        {
            Beacons = new ObservableCollection<BeaconModel>();
        }

        public void CreateAndUpdateBeacons(List<BeaconModel> beacons)
        {

            foreach (var beacon in beacons)
            {
                if (Beacons.Where(x => x.Major == beacon.Major && x.Minor == beacon.Minor).Any())
                {
                    var item = Beacons.Where(x => x.Major == beacon.Major && x.Minor == beacon.Minor).First();
                    item.Rssi = beacon.Rssi;
                    item.Distinct = beacon.Distinct;
                }
                else
                {
                    Beacons.Add(beacon);
                }
            }
        }
    }

    public class BeaconModel:BaseViewModel
    {
        private string uuid;
        public string Uuid {
            get { return uuid; }
            set { SetProperty(ref uuid, value); }
        }

        private short rssi;
        public short Rssi
        {
            get { return rssi; }
            set { SetProperty(ref rssi, value); }
        }

        private ushort major;
        public ushort Major
        {
            get { return major; }
            set { SetProperty(ref major, value); }
        }

        private ushort minor;
        public ushort Minor
        {
            get { return minor; }
            set { SetProperty(ref minor, value); }
        }

        private double distinct;
        public double Distinct
        {
            get { return distinct; }
            set { SetProperty(ref distinct, value); }
        }
    }
}
