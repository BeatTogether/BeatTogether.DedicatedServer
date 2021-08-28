using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class NodePoseSyncState : INetSerializable
    {
        public Pose Head { get; set; }
        public Pose LeftController { get; set; }
        public Pose RightController { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Head.Deserialize(reader);
            LeftController.Deserialize(reader);
            RightController.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            Head.Serialize(writer);
            LeftController.Serialize(writer);
            RightController.Serialize(writer);
        }
    }
}
