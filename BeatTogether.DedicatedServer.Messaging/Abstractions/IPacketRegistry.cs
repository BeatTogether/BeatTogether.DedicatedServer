using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public interface IPacketRegistry
    {
        /// <summary>
        /// Retrieves the identifiers of all registered packets.
        /// </summary>
        /// <returns>The identifiers of all registered packets.</returns>
        IReadOnlyDictionary<Type, IEnumerable<byte>> GetAllPacketIds();

        /// <summary>
        /// Retrieves the identifiers associated with the packet
        /// of the given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of packet.</param>
        /// <returns>The identifiers associated with the packet.</returns>
        IEnumerable<byte> GetPacketIds(Type type);

        /// <summary>
        /// Retrieves the identifiers associated with the packet of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of packet.</typeparam>
        /// <returns>The identifiers associated with the packet.</returns>
        IEnumerable<byte> GetPacketIds<T>()
            where T : class, INetSerializable;

        /// <summary>
        /// Retrieves the <see cref="Type"/> of the packet
        /// associated with the given <paramref name="packetId"/>.
        /// </summary>
        /// <param name="packetId">
        /// The identifier associated with the packet.
        /// This must be castable to a <see cref="byte"/>.
        /// </param>
        /// <returns>The <see cref="Type"/> object of the packet.</returns>
        Type GetPacketType(object packetId);

        /// <summary>
        /// Retrieves the <see cref="IPacketRegistry"/> instance
        /// associated with the given <paramref name="packetRegistryId"/>.
        /// </summary>
        /// <param name="packetRegistryId">
        /// The identifier associated with the sub packet registry.
        /// This must be castable to a <see cref="byte"/>.
        /// </param>
        /// <returns>The <see cref="IPacketRegistry"/> instance associated with the given identifier.</returns>
        IPacketRegistry GetSubPacketRegistry(object packetRegistryId);

        /// <summary>
        /// Creates a new instance of the packet associated with the given <paramref name="packetId"/>.
        /// </summary>
        /// <param name="packetId">
        /// The identifier associated with the packet.
        /// This must be castable to a <see cref="byte"/>.
        /// </param>
        /// <returns>The packet instance.</returns>
        INetSerializable CreatePacket(object packetId);

        /// <summary>
        /// Retrieves the identifiers associated with the packet
        /// of the given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of packet.</param>
        /// <param name="packetIds">The identifiers associated with the packet.</param>
        /// <returns>
        /// <see langword="true"/> when the <paramref name="packetIds"/> were retrieved successfully;
        /// <see langword="false"/> otherwise.
        /// </returns>
        bool TryGetPacketIds(Type type, [MaybeNullWhen(false)] out IEnumerable<byte> packetIds);

        /// <summary>
        /// Retrieves the identifiers associated with the packet of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of packet.</typeparam>
        /// <param name="packetIds">The identifiers associated with the packet.</param>
        /// <returns>
        /// <see langword="true"/> when the <paramref name="packetIds"/> were retrieved successfully;
        /// <see langword="false"/> otherwise.
        /// </returns>
        bool TryGetPacketIds<T>([MaybeNullWhen(false)] out IEnumerable<byte> packetIds)
            where T : class, INetSerializable;

        /// <summary>
        /// Retrieves the <see cref="Type"/> of the packet
        /// associated with the given <paramref name="packetId"/>.
        /// </summary>
        /// <param name="packetId">
        /// The identifier associated with the packet.
        /// This must be castable to a <see cref="byte"/>.
        /// </param>
        /// <param name="type">The <see cref="Type"/> object of the packet.</param>
        /// <returns>
        /// <see langword="true"/> when the <paramref name="type"/> was retrieved successfully;
        /// <see langword="false"/> otherwise.</returns>
        bool TryGetPacketType(object packetId, [MaybeNullWhen(false)] out Type type);

        /// <summary>
        /// Retrieves the <see cref="IPacketRegistry"/> instance
        /// associated with the given <paramref name="packetRegistryId"/>.
        /// </summary>
        /// <param name="packetRegistryId">
        /// The identifier associated with the sub packet registry.
        /// This must be castable to a <see cref="byte"/>.
        /// </param>
        /// <param name="packetRegistry">
        /// The <see cref="IPacketRegistry"/> instance
        /// associated with the given identifier.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when the <paramref name="packetRegistry"/> was retrieved successfully;
        /// <see langword="false"/> otherwise.</returns>
        bool TryGetSubPacketRegistry(object packetRegistryId, [MaybeNullWhen(false)] out IPacketRegistry packetRegistry);

        /// <summary>
        /// Creates a new instance of the packet associated with the given <paramref name="packetId"/>.
        /// </summary>
        /// <param name="packetId">
        /// The identifier associated with the packet.
        /// This must be castable to a <see cref="uint"/>.
        /// </param>
        /// <param name="packet">The packet instance.</param>
        /// <returns>
        /// <see langword="true"/> when the <paramref name="packet"/> was created successfully;
        /// <see langword="false"/> otherwise.
        /// </returns>
        bool TryCreatePacket(object packetId, [MaybeNullWhen(false)] out INetSerializable packet);
    }
}
