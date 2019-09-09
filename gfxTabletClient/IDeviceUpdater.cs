namespace gfxTabletClient
{
    internal interface IDeviceUpdater
    {
        void ProcessAndEnqueueUpdate(double x, double y, double xmax, double ymax, float pressure, bool inair, bool inverted, bool barrel);
        void Stop();
        void EmitRawClick(byte button, bool down);
    }
}