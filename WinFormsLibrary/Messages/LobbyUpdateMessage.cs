using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsLibrary.Messages
{
    public class LobbyUpdateMessage
    {
        private float _countdownEndTime;

        public LobbyUpdateMessage(float countdownEndTime)
        {
            _countdownEndTime = countdownEndTime;
        }

        public float GetCountdownEndTime()
        {
            return _countdownEndTime;
        }
    }
}
