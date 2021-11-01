using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GobangOnline.Client;
using GobangOnline.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace GobangOnline.ViewModels
{
    internal class JoinGameViewModel: ObservableObject
    {
        private OnlineClient _onlineClient;

        public OnlineClient OnlineClient
        {
            get => _onlineClient;
            set => _onlineClient = value;
        }
        private Action<PieceMessage>? _pieceReceiveAction;
        public Action CloseAction { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand JoinCommand { get; set; }

        public bool JoinEnabled
        {
            get => _joinEnabled;
            set => SetProperty(ref _joinEnabled, value);
        }

        public int SeatNum { get; set; }
        public JoinGameViewModel(Action<PieceMessage>? receiveAction)
        {
            _pieceReceiveAction = receiveAction;
            SearchCommand = new AsyncRelayCommand(SearchRoom);
            JoinCommand = new RelayCommand(() => JoinToGame(receiveAction));
        }

        public void JoinToGame(Action<PieceMessage>? receiveAction)
        {
            _onlineClient = new OnlineClient(Roomid, receiveAction);
            _onlineClient.Start();
            CloseAction?.Invoke();
        }

        private async Task SearchRoom()
        {
            //_onlineClient = new OnlineClient(_roomid, _pieceReceiveAction);
            //MemberNum = await _onlineClient.GetMemberNum();
            if (String.IsNullOrWhiteSpace(Roomid)) return;
            MemberNum = await OnlineClient.GetMemberNum(Roomid);
            SeatNum = MemberNum + 1;
            JoinEnabled = true;
        }

        private string _roomid;
        private int _memberNum;
        private bool _joinEnabled;
        public ObservableCollection<string> Messages { get; set; }

        public string Roomid
        {
            get => _roomid;
            set => SetProperty(ref _roomid, value);
        }

        public int MemberNum
        {
            get => _memberNum;
            set => SetProperty(ref _memberNum, value);
        }
    }
}
