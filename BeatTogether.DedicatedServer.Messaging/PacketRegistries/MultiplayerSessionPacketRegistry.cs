using BeatTogether.DedicatedServer.Messaging.Enums;

namespace BeatTogether.DedicatedServer.Messaging.PacketRegistries
{
    public sealed class MultiplayerSessionPacketRegistry : BasePacketRegistry
    {
        public override void Register()
        {
            AddSubPacketRegistry<MenuRpcPacketRegistry>(MultiplayerSessionPacketType.MenuRpc);
        }
    }
}
