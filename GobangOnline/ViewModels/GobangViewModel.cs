using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GobangOnline.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace GobangOnline.ViewModels
{
    internal class GobangViewModel: ObservableObject
    {
        public static readonly short CheckerboardLength = 15;


        private bool _enabled;

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty(ref _enabled, value);
        }

        public bool GameFinished { get; set; }

        public ChessPiece[,] CheckerboardData { get; set; }
        public Action<ChessPiece>? AddPieceAction { set; get; }

        public delegate void Victory(PieceType type);
        public event Victory VictoryEvent;

        public void ResetGame()
        {
            initPieceData();
        }

        private void JudgeOutcome()
        {
            for (int i = 1; i < CheckerboardLength; i++)
            {
                for (int j = 1; j < CheckerboardLength; j++)
                {
                    var currPieceType = CheckerboardData[i, j].PieceType;
                    var offset = 0;
                    var count = 0;
                    if (currPieceType == PieceType.None) continue;
                    if (i >= CheckerboardLength - 4 || j >= CheckerboardLength - 4) continue;
                    if (i >= 4 && j >= 4)
                    {
                        // 左下
                        while (CheckerboardData[i - offset, j + offset].PieceType == currPieceType)
                        {
                            count++;
                            offset++;
                            if (count < 5) continue;
                            VictoryEvent?.Invoke(currPieceType);
                            break;
                        }
                    }

                    offset = 0;
                    count = 0;
                    // 横
                    while (CheckerboardData[i + offset, j].PieceType == currPieceType)
                    {
                        count++;
                        offset++;
                        if (count < 5) continue;
                        VictoryEvent?.Invoke(currPieceType);
                        break;
                    }
                    offset = 0;
                    count = 0;
                    // 竖
                    while (CheckerboardData[i, j + offset].PieceType == currPieceType)
                    {
                        count++;
                        offset++;
                        if (count < 5) continue;
                        VictoryEvent?.Invoke(currPieceType);
                        break;
                    }
                    offset = 0;
                    count = 0;
                    // 右下
                    while (CheckerboardData[i + offset, j + offset].PieceType == currPieceType)
                    {
                        count++;
                        offset++;
                        if (count < 5) continue;
                        VictoryEvent?.Invoke(currPieceType);
                        break;
                    }
                }
            }
        }

        private void initPieceData()
        {
            CheckerboardData = new ChessPiece[CheckerboardLength, CheckerboardLength];
            for (var i = 0; i < CheckerboardLength; i++)
            {
                for (var j = 0; j < CheckerboardLength; j++)
                {
                    var temp = new ChessPiece(AddPieceAction)
                    {
                        PieceType = PieceType.None, X = i, Y = j,
                    };
                    CheckerboardData[i, j] = temp;
                }
            }
        }
        
        public GobangViewModel(Action<ChessPiece> addPieceAction)
        {
            AddPieceAction = piece =>
            {
                addPieceAction?.Invoke(piece);
				JudgeOutcome();
            };
            VictoryEvent += type =>
            {
                var curr = type == PieceType.Black ? "黑" : "白";
                MessageBox.Show($"{curr}色棋子胜利!");
                ResetGame();
            };
            initPieceData();
        }
    }
}
