using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace gfxTabletClient
{
    internal class UdpSenderDeviceUpdater : IDeviceUpdater
    {
        private string mIP;
        private string mPort;
        private Socket socket;
        private IPEndPoint endPoint;
        private Queue<gfxPacket> packetQueue;
        private readonly LinkedList<gfxPacket> packetPool;
        private bool running = true;
        private Thread thread;

        private const int PoolCapacity = 4;

        struct state
        {
            public bool clicked;
            public bool barrel;
            public int x, y;
        }

        private state State = new state();

        public UdpSenderDeviceUpdater(string mIP, string mPort)
        {
            this.mIP = mIP;
            this.mPort = mPort;
            packetQueue = new Queue<gfxPacket>(PoolCapacity);
            packetPool = new LinkedList<gfxPacket>();
            for (int i = 0; i < PoolCapacity; i++)
                packetPool.AddLast(new gfxPacket()
                {
                    signature = Encoding.ASCII.GetBytes("GfxTablet"),
                    vnum = SwapBytes(3)
                });
            InitSocket();
            thread = new Thread(this.update);
            thread.Start();
        }

        public static ushort SwapBytes(ushort x)
        {
            return ((ushort)(((uint)IPAddress.HostToNetworkOrder((int)x)) >> 16));
        }
        private void InitSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress serverAddress = IPAddress.Parse(mIP);
            endPoint = new IPEndPoint(serverAddress, int.Parse(mPort));
        }

        public void ProcessAndEnqueueUpdate(double x, double y, double xmax, double ymax, float pressure, bool inair, bool inverted, bool barrel)
        {
            gfxPacket packet = packetFromPool();

            packet.pressure = SwapBytes((ushort)(pressure * 32768));
            packet.x = SwapBytes((ushort)(x * 65536 / xmax));
            packet.y = SwapBytes((ushort)(y * 65536 / ymax));
            packet.eventType = 0; // position change

            if (barrel ^ State.barrel)
            {
                State.barrel = !State.barrel;
                EmitClick(packet, 1, !State.barrel);
            }

            if (inair)
            {
                // movement event
                if (State.clicked)
                {
                    // it was clicked previously, emit an up event
                    EmitClick(packet, -1, true);
                    State.clicked = false;
                }
            }
            else
            {
                if (!State.clicked)
                {
                    // it was not clicked before, emit a down event
                    EmitClick(packet, -1, false);
                    State.clicked = true;
                }
            }

            Enqueue(packet);
        }
        private gfxPacket packetFromPool()
        {
            if (packetPool.Count == 0)
                for (int i = 0; i < PoolCapacity / 2; i++)
                    packetPool.AddLast(packetQueue.Dequeue()); // remove half of the last messages, it's probably too old anyway if we're this ingested

            gfxPacket packet = packetPool.Last.Value;
            packetPool.RemoveLast();
            return packet;
        }
        private void Enqueue(gfxPacket packet)
        {
            packetQueue.Enqueue(packet);
        }

        private void EmitClick(gfxPacket packet, short button, bool up)
        {
            gfxPacket npacket = packetFromPool();

            npacket.x = packet.x;
            npacket.y = packet.y;
            npacket.pressure = packet.pressure;
            npacket.eventType = (byte)SwapBytes(1); // button
            npacket.button = button;
            npacket.down = (byte) SwapBytes((ushort)(up ? 0 : 1));

            Enqueue(npacket);
        }

        public void EmitRawClick(short button, bool up)
        {
            gfxPacket npacket = packetFromPool();

            npacket.eventType = (byte)SwapBytes(1); // button
            npacket.button = button;
            npacket.down = (byte)SwapBytes((ushort)(up ? 0 : 1));

            Enqueue(npacket);
        }

        private void update()
        {
            while (running)
            {
                if (packetQueue.Count > 0)
                {
                    gfxPacket packet = packetQueue.Dequeue();
                    socket.SendTo(RawSerialize(packet), endPoint);
                    packetPool.AddLast(packet);
                    // Console.WriteLine($"sent {packet}");
                    Thread.Sleep(0);
                }
            }
        }

        public byte[] RawSerialize(gfxPacket item)
        {
            int rawSize = Marshal.SizeOf(typeof(gfxPacket));
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(item, buffer, false);
            byte[] rawData = new byte[rawSize];
            Marshal.Copy(buffer, rawData, 0, rawSize);
            Marshal.FreeHGlobal(buffer);
            return rawData;
        }

        public void Stop()
        {
            running = false;
            thread.Join();
        }
    }
}