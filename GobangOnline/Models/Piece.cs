using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace GobangOnline.Models
{
    public enum PieceType
    {
        Black = -1,
        None = 0,
        White = 1
    }
    public class ChessPiece: ObservableObject
    {
        private PieceType _pieceType;

        public PieceType PieceType
        {
            get => _pieceType;
            set => SetProperty(ref _pieceType, value);
        }

        public int X { get; set; }
        public int Y { get; set; }

        //public Action<ChessPiece>? AddPieceAction { set; get; }
        public ChessPiece(Action<ChessPiece> addPieceAction)
        {
            PropertyChanged += (sender, args) =>
            {
                addPieceAction?.Invoke(sender as ChessPiece ?? throw new NullReferenceException());
            };
        }

    }

}
