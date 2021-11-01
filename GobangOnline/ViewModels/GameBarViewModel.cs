using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GobangOnline.Client;
using GobangOnline.Models;
using GobangOnline.Views;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace GobangOnline.ViewModels
{
    internal class GameBarViewModel: ObservableObject
    {

        private bool localStarted = false;
        private bool netStarted = false;
        public Checkerboard checkerboard { get; set; }
        private GobangViewModel gobangViewModel;
        public ICommand LocalStartCommand { get; set; }
        public ICommand LocalRestartCommand { get; set; }
        public ICommand NetJoinCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }

        public bool CanLocalStart
        {
            get => _canLocalStart;
            set => SetProperty(ref _canLocalStart, value);
        }

        public bool CanNetStart
        {
            get => _canNetStart;
            set => SetProperty(ref _canNetStart, value);
        }

        public bool CanLocalRestart
        {
            get => _canLocalRestart;
            set => SetProperty(ref _canLocalRestart, value);
        }

        public bool ChatEnabled
        {
            get => _chatEnabled;
            set => SetProperty(ref _chatEnabled, value);
        }

        public ObservableCollection<string> ChatMessages { get; set; }
        private string _tips;

        public string Tips
        {
            get => _tips;
            set => SetProperty(ref _tips, value);
        }

        public GameBarViewModel(Checkerboard c)
        {
            CanLocalStart = true;
            CanNetStart = true;
            checkerboard = c;
            gobangViewModel = checkerboard.DataContext as GobangViewModel;
            ChatMessages = new ObservableCollection<string>();
            LocalStartCommand = new RelayCommand(() =>
            {
                checkerboard.Board.PreviewMouseDown += (o, eventArgs) =>
                {
                    var currPiece = checkerboard.CurrPieceType == PieceType.Black ? "黑" : "白";
                    Tips = $"当前棋子：{currPiece}";
                };
                if (!CanLocalStart) return;
                gobangViewModel.Enabled = true;
                CanLocalRestart = true;
                CanLocalStart = false;
                CanNetStart = false;
            });
            LocalRestartCommand = new RelayCommand(() =>
            {
                checkerboard.ClearCheckerBoard();
                gobangViewModel.Enabled = true;
            });
            NetJoinCommand = new RelayCommand(NetJoin);
            SendMessageCommand = new RelayCommand(SendTextMessage);

        }
        private JoinGame joinGameWindow;
        private bool _canLocalStart;
        private bool _canNetStart;
        private bool _canLocalRestart;
        private string _editMessageText;
        private JoinGameViewModel joinGameVm;
        private bool _chatEnabled;

        private void NetJoin()
        {

            joinGameWindow = new JoinGame();
            //{
            //    Owner = Window.GetWindow(this)
            //};
            joinGameVm = new JoinGameViewModel(ReceiveMessage)
            {
                CloseAction = () =>
                {
                    joinGameWindow.Close();
                    CanLocalStart = false;
                    CanLocalRestart = false;
                    CanNetStart = false;
                    ChatEnabled = true;
                    NetStart();
                }
            };
            joinGameWindow.DataContext = joinGameVm;
            joinGameWindow.ShowDialog();
        }
        private PieceType pieceType = PieceType.None;

        private void NetStart()
        {
            checkerboard.gameWay = Checkerboard.GameWay.Net;
            //if (joinGameVm.SeatNum == 1)
            //{
            //    pieceType = PieceType.Black;
            //    //gobangViewModel.Enabled = true;
            //}
            //else if (joinGameVm.SeatNum == 2)
            //{
            //    pieceType = PieceType.White;
            //}
            //else
            //{
            //    pieceType = PieceType.None;
            //}
            checkerboard.NetAddNewPiece += point =>
            {
                SendPieceMessage(new PieceMessage()
                {
                    Message = "", Piece = new ChessPiece() { PieceType = pieceType, X = point.X, Y = point.Y },
                    Roomid = joinGameVm.Roomid, Username = ""
                });
                gobangViewModel.Enabled = false;
            };
        }

        private void SendPieceMessage(PieceMessage msg)
        {
            joinGameVm.OnlineClient.SendMessage(msg);
        }

        private bool newPlayerFlag = true;

        private void ReceiveMessage(PieceMessage msg)
        {
            if (msg.Username != "System")
            {
                if (!string.IsNullOrWhiteSpace(msg.Message))
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        SynchronizationContext.SetSynchronizationContext(new
                            DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                        SynchronizationContext.Current.Post(pl =>
                        {
                            ChatMessages.Add(msg.Message);
                        }, null);
                    });
                }
                if (msg.Piece != null)
                {
                    gobangViewModel.CheckerboardData[msg.Piece.X, msg.Piece.Y].PieceType = msg.Piece.PieceType;
                    if (msg.Piece.PieceType != pieceType && msg.Piece.PieceType != PieceType.None && !gobangViewModel.GameFinished)
                    {
                        gobangViewModel.Enabled = true;
                    }
                }
            }
            else
            {
                var memberNum = int.Parse(msg.Message);
                var playerNum = memberNum > 2 ? 2 : memberNum;
                var audienceNum = memberNum > 2 ? memberNum - 2 : 0;
                if (newPlayerFlag)
                {
                    pieceType = memberNum switch
                    {
                        1 => PieceType.Black,
                        2 => PieceType.White,
                        _ => PieceType.None
                    };
                    newPlayerFlag = false;
                }
                string color = pieceType switch
                {
                    PieceType.Black => "黑",
                    PieceType.White => "白",
                    PieceType.None => "无",
                };
                Tips = $"房间号: {joinGameVm.Roomid}\n当前选手数: {playerNum}\n您的棋子颜色: {color}\n观战人数: {audienceNum}";
                if (memberNum == 2 && pieceType == PieceType.Black)
                {
                    gobangViewModel.Enabled = true;
                }
            }
        }

        public string EditMessageText
        {
            get => _editMessageText;
            set => SetProperty(ref _editMessageText, value);
        }

        private void SendTextMessage()
        {
            if (!string.IsNullOrWhiteSpace(EditMessageText))
                joinGameVm.OnlineClient.SendMessage(new PieceMessage()
                {
                    Message = $"{pieceType}: {EditMessageText}",
                    Piece = null,
                    Username = "",
                    Roomid = joinGameVm.OnlineClient.Roomid
                });
            EditMessageText = "";
        }

    }
}
