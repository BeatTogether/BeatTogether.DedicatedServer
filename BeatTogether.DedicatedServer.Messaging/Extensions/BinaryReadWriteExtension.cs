using System.IO;
using BeatTogether.DedicatedServer.Messaging.Models;

namespace BeatTogether.Extensions
{
    public static class BinaryReadWriteExtension
    {
        // Token: 0x06000069 RID: 105 RVA: 0x00002ED8 File Offset: 0x000010D8
        public static void Write(this BinaryWriter binaryWriter, Color color)
        {
            binaryWriter.Write(color.r);
            binaryWriter.Write(color.g);
            binaryWriter.Write(color.b);
            binaryWriter.Write(color.a);
        }

        // Token: 0x0600006A RID: 106 RVA: 0x00002F0A File Offset: 0x0000110A
        public static Color ReadColor(this BinaryReader binaryReader)
        {
            return new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
        }
    }
}
