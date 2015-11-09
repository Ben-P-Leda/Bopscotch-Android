using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

// EXPERIMENT - does open and close in this manner save comms?
// If not - or it stops working - revert to commented line in constructor and remove all _clientOpen condition checks

namespace Bopscotch.Communication
{
    public class CommunicationHandler
    {
        public delegate void CommsEventHandler(Dictionary<string, string> decodedData);

        private UdpClient _client;
        private bool _clientOpen;

        public CommsEventHandler CommsCallback { get; set; }
        public string MyID { private get; set; }

        public CommunicationHandler()
        {
            _client = new UdpClient(3007);
            _client.EnableBroadcast = true;
            _client.JoinMulticastGroup(IPAddress.Parse("224.109.108.107"));
            //_client.BeginReceive(HandleClientPacketReceived, _client);

            _clientOpen = false;

            CommsCallback = null;
        }

        private void HandleClientPacketReceived(IAsyncResult result)
        {
            if (CommsCallback != null)
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 3007);
                byte[] dataBytes = _client.EndReceive(result, ref endPoint);

                string data = Encoding.ASCII.GetString(dataBytes, 0, dataBytes.Length).Trim('\0');
                Android.Util.Log.Debug("Leda", "Received: " + data);

                if ((data.IndexOf(Game_Identifier) > -1) && (data.IndexOf(string.Concat("&id=", MyID)) < 0))
                {
                    Android.Util.Log.Debug("Leda", "Sent");
                    DecodeAndSendData(data);
                }
            }

            if (_clientOpen) { _client.BeginReceive(HandleClientPacketReceived, _client); }
        }

        private void DecodeAndSendData(string encodedData)
        {
            Dictionary<string, string> decodedData = new Dictionary<string, string>();
            bool dataIsValid = true;

            if (string.IsNullOrEmpty(encodedData))
            {
                dataIsValid = false;
            }
            else
            {
                foreach (string s in encodedData.Split('&'))
                {
                    if (s.IndexOf("=") > -1)
                    {
                        string key = s.Split('=')[0];
                        string value = s.Split('=')[1].Trim();

                        if ((!decodedData.ContainsKey(key)) && (!string.IsNullOrEmpty(value))) { decodedData.Add(key, value); }
                        else { dataIsValid = false; break; }
                    }
                    else
                    {
                        dataIsValid = false;
                        break;
                    }
                }
            }

            if (dataIsValid) { CommsCallback(decodedData); }
        }

        public void SendData(string message)
        {
            message = string.Concat(Game_Identifier, "&id=", MyID, "&", message);
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            if (messageBytes.Length > 0)
            {
                _client.Send(messageBytes, messageBytes.Length, new IPEndPoint(IPAddress.Parse("224.109.108.107"), 3007));
            }
        }

        public void Open()
        {
            _clientOpen = true;
            _client.BeginReceive(HandleClientPacketReceived, _client);
        }

        public void Close()
        {
            _clientOpen = false;
        }

        private const string Game_Identifier = "game=bopscotch";
    }
}
