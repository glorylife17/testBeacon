using System;
using System.Collections.Generic;

namespace testBeacon.Interfaces
{
    public interface IBeaconService
    {
        event EventHandler<List<BeaconModel>> BeaconsReceived;

        void SetUuid(string uuid);

        bool IsRanging { get; }
        void Start();
        void Stop();
    }
}
