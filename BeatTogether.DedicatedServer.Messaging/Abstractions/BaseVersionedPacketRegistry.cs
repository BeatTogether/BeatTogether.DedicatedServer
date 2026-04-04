using BeatTogether.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public abstract class BaseVersionedPacketRegistry : IVersionedPacketRegistry
    {
        public delegate INetSerializable PacketFactory();

        private int GetVersionIndex(byte ID, int version_in)
        {
            int Low_Ver = 0;
            if (versionRanges.TryGetValue(ID, out var version_candidates))
            {
                for (var i = 0; i < version_candidates.Length; i++)
                {
                    if (version_in >= version_candidates[i].Item1 && version_in <= version_candidates[i].Item2)
                    {
                        Low_Ver = version_candidates[i].Item1;
                        break;
                    }
                }
            }
            return Low_Ver;
        }

        private Type? GetPacketType(byte ID, int version_in)
        {
            if (!_types[GetVersionIndex(ID, version_in)].TryGetValue(ID, out var ret_type))
            {
                return null;
            }
            return ret_type;
        }

        private readonly Dictionary<byte, (int, int)[]> versionRanges = new();
        private readonly Dictionary<int, Dictionary<byte, Type>> _types = new();
        private readonly Dictionary<Type, IEnumerable<byte>> _packetIds = new();

        private readonly Dictionary<int, Dictionary<byte, IVersionedPacketRegistry>> _subPacketRegistries = new();
        private readonly Dictionary<int, Dictionary<byte, PacketFactory>> _factories = new();


        private readonly HashSet<VersionRange> _versionRanges = new();
        private readonly HashSet<Version> _versions = new();
        private Version[] _versions_in_order;
        private readonly Dictionary<Version, int> _version_order_dict = new();

        //For registering packets and sub registries
        private readonly Dictionary<Type, (byte, VersionRange, PacketFactory)> _packetVersionRanges = new();
        private readonly Dictionary<Type, (byte, VersionRange, IVersionedPacketRegistry)> _subRegistryVersionRanges = new();

        //Collects the version ranges and puts them in order for fast comparrisons.
        private void SetupVersioning()
        {
            //Add min and max versions.
            _versions.Add(new Version("0.0.0")); //will be index 0
            _versions.Add(new Version($"{int.MaxValue}.{int.MaxValue}.{int.MaxValue}")); //will be index max
            //Add versions between
            foreach (var version_range in _versionRanges)
            {
                _versions.Add(new Version(version_range.MinVersion));
                _versions.Add(new Version(version_range.MaxVersion));
            }
            //Versions hash set now contains a list of all versions.

            _versions_in_order = _versions.ToArray();
            Array.Sort(_versions_in_order);

            for (int i = 0; i < _versions_in_order.Length - 1; i++)
            {
                _version_order_dict.Add(_versions_in_order[i], i);
            }

            Dictionary<byte, List<(int, int)>> versionRangesConstructor = new();

            foreach (var pair in _packetVersionRanges)
            {

                VersionRangeToOrderRange(pair.Value.Item2, out int min, out int max);

                if(!versionRangesConstructor.TryGetValue(pair.Value.Item1, out List<(int, int)>? ranges))
                {
                    ranges = new();
                }
                ranges.Add((min, max));
                versionRangesConstructor[pair.Value.Item1] = ranges;

                if(!_types.TryGetValue(min, out var ty))
                {
                    ty = new();
                    _types[min] = ty;
                }

                if(!_factories.TryGetValue(min, out var fact))
                {
                    fact = new();
                    _factories[min] = fact;
                }

                _types[min][(byte)pair.Value.Item1] = pair.Key;
                _factories[min][(byte)pair.Value.Item1] = pair.Value.Item3;
            }

            foreach(var pair in _subRegistryVersionRanges)
            {
                VersionRangeToOrderRange(pair.Value.Item2, out int min, out int max);

                if (!versionRangesConstructor.TryGetValue(pair.Value.Item1, out List<(int, int)>? ranges))
                {
                    ranges = new();
                    versionRangesConstructor[pair.Value.Item1] = ranges;
                }
                versionRangesConstructor[pair.Value.Item1].Add((min, max));

                if (!_subPacketRegistries.TryGetValue(min, out var sub))
                {
                    sub = new();
                    _subPacketRegistries[min] = sub;
                }

                _subPacketRegistries[min][(byte)pair.Value.Item1] = pair.Value.Item3;

            }
        }

        private void VersionRangeToOrderRange(VersionRange? range, out int min, out int max)
        {
            //If no range, then return entire range.
            if(range == null)
            {
                min = 0;
                max = _versions_in_order.Length - 1;
                return;
            }
            if(!_version_order_dict.TryGetValue(new Version(range.MinVersion), out min))
            {
                min = 0;
            }
            if(!_version_order_dict.TryGetValue(new Version(range.MaxVersion), out max))
            {
                max = _versions_in_order.Length -1;
            }
        }

        #region Public Methods

        public BaseVersionedPacketRegistry()
        {
            Register();
            SetupVersioning();
        }

        //Now, when storing packets, we should store them in a dictionary, starting with the lowest update number they exist for.

        

        public abstract void Register();

        /// <inheritdoc cref="IVersionedPacketRegistry.GetAllPacketIds"/>
        public IReadOnlyDictionary<Type, IEnumerable<byte>> GetAllPacketIds() =>
            _packetIds;

        //Allows versions that have not been registered to be found.
        public int GetVersionNumber(Version? gameVersion)
        {
            //Version is unknown, return lowest value.
            if (gameVersion == null)
                return 0; //Smallest game version for an unknown.
            for (int i = 0; i < _versions_in_order.Length - 1; i++)
            {
                //If the version in order is bigger than the game version, return the last version below.
                if (_versions_in_order[i] > gameVersion)
                {
                    return i - 1;
                }
            }
            //Version is above all mentioned versions
            return _versions_in_order.Length - 1;
        }

        ///// <inheritdoc cref="IVersionedPacketRegistry.GetPacketIds"/>
        ///// broke but too lazy to fix
        //public IEnumerable<byte> GetPacketIds(Type type) =>
        //    _packetIds[type];

        ///// <inheritdoc cref="IVersionedPacketRegistry.GetPacketIds{T}"/>
        ///// broke but too lazy to fix
        //public IEnumerable<byte> GetPacketIds<T>()
        //    where T : class, INetSerializable =>
        //    GetPacketIds(typeof(T));

        /// <inheritdoc cref="IVersionedPacketRegistry.GetPacketType"/>
        public Type GetPacketType(object packetId, int version_number)
        {
            int ver = GetVersionIndex((byte)packetId, version_number);
            return _types[ver][(byte)packetId];
        }

        /// <inheritdoc cref="IVersionedPacketRegistry.GetSubPacketRegistry"/>
        public IVersionedPacketRegistry GetSubPacketRegistry(object packetRegistryId, int version_number)
        {
            int ver = GetVersionIndex((byte)packetRegistryId, version_number);
            return _subPacketRegistries[ver][(byte)packetRegistryId];
        }

        /// <inheritdoc cref="IVersionedPacketRegistry.CreatePacket"/>
        public INetSerializable CreatePacket(object packetId, int version_number)
        {
            int ver = GetVersionIndex((byte)packetId, version_number);
            return _factories[ver][(byte)packetId]();
        }

        /// <inheritdoc cref="IVersionedPacketRegistry.TryGetPacketIds"/>
        public bool TryGetPacketIds(Type type, int version_number, [MaybeNullWhen(false)] out IEnumerable<byte> packetIds)
        {
            if (_packetIds.TryGetValue(type, out packetIds))
                return true;

            //int ver = GetVersionIndex((byte)packetId, version_number);

            //For all the version range sub registries (if there are any)
            foreach (var item in _subRegistryVersionRanges)
            {
                if (item.Value.Item3.TryGetPacketIds(type, version_number, out IEnumerable<byte>? subPacketIds))
                {
                    packetIds = Enumerable.Empty<byte>().Append((byte)item.Value.Item1).Concat(subPacketIds);
                    return true;
                }
            }
            //For all the default(all version) sub packet registries
            foreach (var (id, subRegistry) in _subPacketRegistries[0])
            {
                if (subRegistry.TryGetPacketIds(type, version_number, out IEnumerable<byte>? subPacketIds))
                {
                    packetIds = Enumerable.Empty<byte>().Append((byte)id).Concat(subPacketIds);
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc cref="IVersionedPacketRegistry.TryGetPacketId{T}"/>
        public bool TryGetPacketIds<T>(int version_number, [MaybeNullWhen(false)] out IEnumerable<byte> packetIds)
            where T : class, INetSerializable =>
            TryGetPacketIds(typeof(T), version_number, out packetIds);

        /// <inheritdoc cref="IVersionedPacketRegistry.TryGetPacketType"/>
        public bool TryGetPacketType(object packetId, int version_number, [MaybeNullWhen(false)] out Type type)
        {
            int ver = GetVersionIndex((byte)packetId, version_number);
            return _types[ver].TryGetValue((byte)packetId, out type);
        }

        /// <inheritdoc cref="IVersionedPacketRegistry.TryGetSubPacketRegistry"/>
        public bool TryGetSubPacketRegistry(object packetRegistryId, int version_number, [MaybeNullWhen(false)] out IVersionedPacketRegistry packetRegistry)
        {
            int ver = GetVersionIndex((byte)packetRegistryId, version_number);
            return _subPacketRegistries[ver].TryGetValue((byte)packetRegistryId, out packetRegistry);
        }


        /// <inheritdoc cref="IVersionedPacketRegistry.TryCreatePacket"/>
        public bool TryCreatePacket(object packetId, int version_number, [MaybeNullWhen(false)] out INetSerializable packet)
        {
            int ver = GetVersionIndex((byte)packetId, version_number);
            if (_factories[ver].TryGetValue((byte)packetId, out var factory))
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

            _types[0][(byte)packetId] = type;
            _packetIds[type] = Enumerable.Empty<byte>()
                .Append((byte)packetId);
            _factories[0][(byte)packetId] = () => new T();

        }

        protected void AddPacket<T>(object packetId, VersionRange versionRange)
            where T : class, INetSerializable, new()
        {
            var type = typeof(T);
            if (_types.ContainsKey((byte)packetId) || _packetIds.ContainsKey(type))
                throw new Exception(
                    $"Duplicate registration for packet of type '{type.Name}' " +
                    $"(PacketId={packetId})."
                );

            _packetIds[type] = Enumerable.Empty<byte>().Append((byte)packetId);
            _versionRanges.Add(versionRange);
            _packetVersionRanges[type] = ((byte)packetId, versionRange, () => new T());
        }

        protected void AddSubPacketRegistry<T>(object packetRegistryId)
            where T : class, IVersionedPacketRegistry, new()
        {
            var subPacketRegistry = new T();
            _subPacketRegistries[0][(byte)packetRegistryId] = subPacketRegistry;

        }

        protected void AddSubPacketRegistry<T>(object packetRegistryId, VersionRange versionRange)
            where T : class, IVersionedPacketRegistry, new()
        {
            var subPacketRegistry = new T();
            _subRegistryVersionRanges[typeof(T)] = ((byte)packetRegistryId, versionRange, subPacketRegistry);
        }

        #endregion
    }
}
