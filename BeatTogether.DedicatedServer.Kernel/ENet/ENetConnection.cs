using System.Net;

namespace BeatTogether.DedicatedServer.Kernel.ENet
{
    public class ENetConnection
    {
        public ENetServer ENetServer { get; private set; }
        public uint NativePeerId { get; private set; }
        public IPEndPoint EndPoint { get; private set; }
        public ENetConnectionState State { get; set; }

        public ENetConnection(ENetServer eNetServer, uint nativePeerId, string ip, int port)
        {
            ENetServer = eNetServer;
            NativePeerId = nativePeerId;
            EndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            State = ENetConnectionState.Pending;
        }

        public void Disconnect(bool clientInitiated = false) 
            => ENetServer.KickPeer(NativePeerId, sendKick: !clientInitiated);
    }

    public enum ENetConnectionState
    {
        Pending = 0,
        Accepted = 1,
        Disconnected = 2
    }
}