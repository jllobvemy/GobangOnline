using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GobangOnline.Models
{
    public class PieceMessage
    {
        public string Username { get; set; }
        public string Roomid { get; set; }
        public string Message { get; set; }
        public ChessPiece? Piece { get; set; }
    }
    internal interface IOnlineOptn
    {
        int GetMemberNum();
        void MessageReceived(Action<PieceMessage> action);
    }
}
