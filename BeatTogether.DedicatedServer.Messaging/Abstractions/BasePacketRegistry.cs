using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public abstract class BasePacketRegistry : IPacketRegistry
    {
        public delegate INetSerializable PacketFactory();

        private readonly Dictionary<byte, Type> _types = new();
        private readonly Dictionary<Type, IEnumerable<byte>> _packetIds = new();
        private readonly Dictionary<byte, IPacketRegistry> _subPacketRegistries = new();
        private readonly Dictionary<byte, PacketFactory> _factories = new();

        #region Public Methods

        public BasePacketRegistry() => Register();

        public abstract void Register();

        /// <inheritdoc cref="IPacketRegistry.GetAllPacketIds"/>
        public IReadOnlyDictionary<Type, IEnumerable<byte>> GetAllPacketIds() =>
            _packetIds;

        /// <inheritdoc cref="IPacketRegistry.GetPacketIds"/>
        /// broke but too lazy to fix
        public IEnumerable<byte> GetPacketIds(Type type) =>
            _packetIds[type];

        /// <inheritdoc cref="IPacketRegistry.GetPacketIds{T}"/>
        /// broke but too lazy to fix
        public IEnumerable<byte> GetPacketIds<T>()
            where T : class, INetSerializable =>
            GetPacketIds(typeof(T));

        /// <inheritdoc cref="IPacketRegistry.GetPacketType"/>
        public Type GetPacketType(object packetId) =>
            _types[(byte)packetId];

        /// <inheritdoc cref="IPacketRegistry.GetSubPacketRegistry"/>
        public IPacketRegistry GetSubPacketRegistry(object packetRegistryId) =>
            _subPacketRegistries[(byte)packetRegistryId];

        /// <inheritdoc cref="IPacketRegistry.CreatePacket"/>
        public INetSerializable CreatePacket(object packetId) =>
            _factories[(byte)packetId]();

        /// <inheritdoc cref="IPacketRegistry.TryGetPacketIds"/>
        public bool TryGetPacketIds(Type type, [MaybeNullWhen(false)] out IEnumerable<byte> packetIds)
        {
            if (_packetIds.TryGetValue(type, out packetIds))
                return true;
            foreach (var (id, subRegistry) in _subPacketRegistries)
            {
                if (subRegistry.TryGetPacketIds(type, out IEnumerable<byte>? subPacketIds))
                {
                    packetIds = Enumerable.Empty<byte>().Append((byte)id).Concat(subPacketIds);
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc cref="IPacketRegistry.TryGetPacketId{T}"/>
        public bool TryGetPacketIds<T>([MaybeNullWhen(false)] out IEnumerable<byte> packetIds)
            where T : class, INetSerializable =>
            TryGetPacketIds(typeof(T), out packetIds);

        /// <inheritdoc cref="IPacketRegistry.TryGetPacketType"/>
        public bool TryGetPacketType(object packetId, [MaybeNullWhen(false)] out Type type) =>
            _types.TryGetValue((byte)packetId, out type);

        /// <inheritdoc cref="IPacketRegistry.TryGetSubPacketRegistry"/>
        public bool TryGetSubPacketRegistry(object packetRegistryId, [MaybeNullWhen(false)] out IPacketRegistry packetRegistry) =>
            _subPacketRegistries.TryGetValue((byte)packetRegistryId, out packetRegistry);

        /// <inheritdoc cref="IPacketRegistry.TryCreatePacket"/>
        public bool TryCreatePacket(object packetId, [MaybeNullWhen(false)] out INetSerializable packet)
        {
            if (_factories.TryGetValue((byte)packetId, out var factory))
            {
                packet = factory();
                return true;
            }

            packet = null;
            return false;
        }

        #endregion

        #region Private Methods

        protected void AddPacket<T>(object packetId)
            where T : class, INetSerializable, new()
        {
            var type = typeof(T);
            if (_types.ContainsKey((byte)packetId) || _packetIds.ContainsKey(type))
                throw new Exception(
                    $"Duplicate registration for packet of type '{type.Name}' " +
                    $"(PacketId={packetId})."
                );

            _types[(byte)packetId] = type;
            _packetIds[type] = Enumerable.Empty<byte>()
                .Append((byte)packetId);
            _factories[(byte)packetId] = () => new T();
        }

        protected void AddSubPacketRegistry<T>(object packetRegistryId)
            where T : class, IPacketRegistry, new()
        {
            var subPacketRegistry = new T();
            _subPacketRegistries[(byte)packetRegistryId] = subPacketRegistry;
            //foreach (var (packetType, packetIds) in subPacketRegistry.GetAllPacketIds())
            //    _packetIds[packetType] = packetIds;
        }

        #endregion
    }
}
