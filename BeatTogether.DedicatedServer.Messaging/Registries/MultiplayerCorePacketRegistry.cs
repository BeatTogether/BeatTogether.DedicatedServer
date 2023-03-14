using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;
using BeatTogether.LiteNetLib.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BeatTogether.DedicatedServer.Messaging.Registries
{
    public class MultiplayerCorePacketRegistry : BasePacketRegistry
    {

        private readonly ConcurrentDictionary<string, PacketFactory> _factories = new();

        public override void Register()
        {
            AddPacket<MpBeatmapPacket>();
            AddPacket<DediPacketSetNewManagerPacket>();
        }

        public bool TryCreatePacket(string packetId, [MaybeNullWhen(false)] out INetSerializable packet)
        {
            if (_factories.TryGetValue(packetId, out var factory))
            {
                packet = factory();
                return true;
            }

            packet = null;
            return false;
        }

        protected void AddPacket<T>() where T : class, INetSerializable, new()
        {
            Type typeFromHandle = typeof(T);

            _factories[typeFromHandle.Name] = () => new T();
        }
    }
}
