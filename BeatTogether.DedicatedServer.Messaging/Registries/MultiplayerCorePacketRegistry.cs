using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;
using BeatTogether.LiteNetLib.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BeatTogether.DedicatedServer.Messaging.Registries
{
    public class MultiplayerCorePacketRegistry : IPacketRegistry<string>
    {
        public delegate INetSerializable PacketFactory();

        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

        private readonly Dictionary<Type, IEnumerable<object>> _packetIds = new Dictionary<Type, IEnumerable<object>>();

        private readonly Dictionary<string, IPacketRegistry<object>> _subPacketRegistries = new Dictionary<string, IPacketRegistry<object>>();

        private readonly Dictionary<string, PacketFactory> _factories = new Dictionary<string, PacketFactory>();

        public MultiplayerCorePacketRegistry()
        {
            Register();
        }

        public void Register()
        {
            //TODO register MPCorePacketsHere
            AddPacket<MpBeatmapPacket>();
        }

        public IReadOnlyDictionary<Type, IEnumerable<string>> GetAllPacketIds()
        {
            return (IReadOnlyDictionary<Type, IEnumerable<string>>)_packetIds;
        }

        public IEnumerable<string> GetPacketIds(Type type)
        {
            return (IEnumerable<string>)_packetIds[type];
        }

        public IEnumerable<string> GetPacketIds<T>() where T : class, INetSerializable
        {
            return GetPacketIds(typeof(T));
        }

        public Type GetPacketType(object packetId)
        {
            return _types[(string)packetId];
        }

        public IPacketRegistry<object> GetSubPacketRegistry(object packetRegistryId)
        {
            return _subPacketRegistries[(string)packetRegistryId];
        }

        public INetSerializable CreatePacket(object packetId)
        {
            return _factories[(string)packetId]();
        }

        public bool TryGetPacketIds(Type type, [MaybeNullWhen(false)] out IEnumerable<object> packetIds)
        {
            if (_packetIds.TryGetValue(type, out packetIds))
                return true;
            foreach (var (id, subRegistry) in _subPacketRegistries)
            {
                if (subRegistry.TryGetPacketIds(type, out IEnumerable<object>? subPacketIds))
                {
                    packetIds = Enumerable.Empty<object>().Append(id).Concat(subPacketIds);
                    return true;
                }
            }
            return false;
        }

        public bool TryGetPacketIds<T>([MaybeNullWhen(false)] out IEnumerable<object> packetIds) where T : class, INetSerializable
        {
            return TryGetPacketIds(typeof(T), out packetIds);
        }

        public bool TryGetPacketType(object packetId, [MaybeNullWhen(false)] out Type type)
        {
            return _types.TryGetValue((string)packetId, out type);
        }

        public bool TryGetSubPacketRegistry(object packetRegistryId, [MaybeNullWhen(false)] out IPacketRegistry<object> packetRegistry)
        {
            packetRegistry = null;
            if (_subPacketRegistries.TryGetValue((string)packetRegistryId, out var packetrRegistry))
                packetRegistry = packetrRegistry;
            return packetRegistry != null;
        }

        public bool TryCreatePacket(object packetId, [MaybeNullWhen(false)] out INetSerializable packet)
        {
            if (_factories.TryGetValue((string)packetId, out PacketFactory? value) && value != null)
            {
                packet = value();
                return true;
            }

            packet = null;
            return false;
        }

        protected void AddPacket<T>() where T : class, INetSerializable, new()
        {
            Type typeFromHandle = typeof(T);
            if (_types.ContainsKey(typeFromHandle.Name) || _packetIds.ContainsKey(typeFromHandle))
            {
                throw new Exception("Duplicate registration for packet of type '" + typeFromHandle.Name + "' " + $"(PacketId={typeFromHandle.Name}).");
            }

            _types[typeFromHandle.Name] = typeFromHandle;
            _packetIds[typeFromHandle] = Enumerable.Empty<string>().Append(typeFromHandle.Name);
            _factories[typeFromHandle.Name] = (() => new T());
        }

        protected void AddSubPacketRegistry<T>(object packetRegistryId) where T : class, IPacketRegistry<object>, new()
        {
            T value = new T();
            _subPacketRegistries[(string)packetRegistryId] = value;
        }
    }
}
