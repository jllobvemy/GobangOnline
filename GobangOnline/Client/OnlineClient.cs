using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using GobangOnline.Models;
using Websocket.Client;

namespace GobangOnline.Client
{
    public class OnlineClient 
    {
        //private static string host = "127.0.0.1:8080";
        private static string host = "ws.jllobvemy.com";
        public string Roomid { get; set; }
        private WebsocketClient client;

        public OnlineClient(string roomid, Action<PieceMessage>? action = null)
        {
            this.Roomid = roomid;
            var url = new Uri($"ws://{host}/ws?roomid={roomid}");
            client = new WebsocketClient(url);
            client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            client.ReconnectionHappened.Subscribe(info =>
                Console.WriteLine($"Reconnection happened, type: {info.Type}"));
            client.MessageReceived.Subscribe(msg =>
            {
                action?.Invoke(JsonSerializer.Deserialize<PieceMessage>(msg.Text) ?? new PieceMessage()
                {
                    Piece = null,
                    Roomid = "",
                    Message = "",
                    Username = ""
                });
            });
            //Task.Run(() => client.Send("{ message }"));
            //Task.Run(exitEvent.WaitOne);
        }

        public void Start()
        {
            client.Start();
        }
        public static async Task<int> GetMemberNum(string roomid)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync($"http://{host}/num?roomid={roomid}");
            response.EnsureSuccessStatusCode();
            var num = await response.Content.ReadAsStringAsync();
            return int.Parse(num);
        }

        public void SendMessage(PieceMessage message)
        {
            var ret = JsonSerializer.Serialize(message);
            Task.Run(() => client.Send(ret));
        }
    }
}
