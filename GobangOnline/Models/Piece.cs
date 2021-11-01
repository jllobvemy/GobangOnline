using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace GobangOnline.Models
{
    public enum PieceType
    {
        //[EnumMember(Value = "Black")]
        Black = -1,
        //[EnumMember(Value = "None")]
        None = 0,
        //[EnumMember(Value = "White")]
        White = 1
    }
    public class ChessPiece: ObservableObject
    {
        private PieceType _pieceType;
        //[JsonConverter(typeof(StringEnumConverter))]
        public PieceType PieceType
        {
            get => _pieceType;
            set => SetProperty(ref _pieceType, value);
        }

        public int X { get; set; }
        public int Y { get; set; }

        //public Action<ChessPiece>? AddPieceAction { set; get; }
        public ChessPiece()
        {
            
        }
        public ChessPiece(Action<ChessPiece>? addPieceAction = null)
        {
            PropertyChanged += (sender, args) =>
            {
                addPieceAction?.Invoke(sender as ChessPiece ?? throw new NullReferenceException());
            };
        }

    }

}
