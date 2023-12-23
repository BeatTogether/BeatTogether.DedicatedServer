using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Messaging.Packets.Legacy
{
    public static class ClientVersions
    {
        public static Version DefaultVersion = new Version(1, 28, 0);
        public static Version NewPacketVersion = new Version(1, 34, 0);

        public static Version ParseGameVersion(string versionText)
        {
            var idxUnderscore = versionText.IndexOf('_');

            if (idxUnderscore >= 0)
                versionText = versionText.Substring(0, idxUnderscore);

            return Version.TryParse(versionText, out var version) ? version : DefaultVersion;
        }

    }
}
