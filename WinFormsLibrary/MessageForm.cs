using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using WinFormsLibrary.Messages;

namespace WinFormsLibrary
{
    public class MessageForm
    {
        public static void Updt()
        {
            Task.Factory.StartNew(() => {
                Messenger.Default.Send<UpdateForm>(null!);
            });
        }

        public static void UpdtFromLobby(float countdownEndTime)
        {
            Task.Factory.StartNew(() => {
                Messenger.Default.Send<LobbyUpdateMessage>(new(countdownEndTime));
            });
        }
    }
}
