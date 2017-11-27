using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketIOClient;
using SocketIOClient.Messages;

namespace HappyFeet
{
    class NodeClient
    {        
        private Client socket;
        private const string server = "http://csc.cpsc.ucalgary.ca:8000";

        private string clientName;
        public bool connectionMade{ get; set; }
        public bool handshook{ get; set; }

        public NodeClient()
        {
            connectionMade = false;
            handshook = false;
        }

        public void startRec()
        {
            socket.Emit("record", null);
        }

        public void finishRec()
        {
            socket.Emit("finishRecord", null);
        }

        public void startPlayback(string playbackNumber)
        {
            Data oData = new Data() { name = clientName, data = playbackNumber };
            socket.Emit("startPlayback", oData);
        }

        public void finishPlayback(string playbackNumber)
        {
            Data oData = new Data() { name = clientName, data = playbackNumber };
            socket.Emit("finishPlayback", oData);
        }
        

        public void handshake(string clientName, string message)
        {
            Data data = new Data() { name = clientName, data = message };
            this.clientName = clientName;
            socket.Emit("handshake", data);
        }

        public void sendData(string message)
        {
            Data oData = new Data() { name = clientName, data = message };
             socket.Emit("data", oData);
        }

        public bool connectToServer()
        {            
            Console.WriteLine("Starting TestSocketIOClient Example...");

            socket = new Client(server); // url to nodejs 
            socket.Opened += SocketOpened;
            socket.Message += SocketMessage;
            socket.SocketConnectionClosed += SocketConnectionClosed;
            socket.Error += SocketError;

            // register for 'connect' event with io server
            socket.On("connect", (fn) =>
            {
                Console.WriteLine("\r\nConnected event...\r\n");                
            });

            // make the socket.io connection            
            socket.Connect();

            return !(socket.ReadyState == WebSocket4Net.WebSocketState.None);          

        }

        private ManualResetEvent testHold = new ManualResetEvent(false);

        //			testHold.WaitOne(5000);

        void SocketError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("socket client error:");
            Console.WriteLine(e.Message);
        }

        void SocketConnectionClosed(object sender, EventArgs e)
        {
            Console.WriteLine("WebSocketConnection was terminated!");
        }

        void SocketMessage(object sender, MessageEventArgs e)
        {
            
            // uncomment to show any non-registered messages
            if (string.IsNullOrEmpty(e.Message.Event))
            {
                if (e.Message.MessageText.Contains("recordingFinished"))
                    MainWindow.recordingFinishedAck = true;
                else if (e.Message.MessageText.Contains("fileLoaded"))
                    MainWindow.fileLoaded = true;
                else
                    Console.WriteLine("Generic SocketMessage: {0}", e.Message.MessageText);
            }
            else
            {
                //   Console.WriteLine("Generic SocketMessage: {0} : {1}", e.Message.Event, e.Message.Json.ToJsonString());
                if (e.Message.Event.Equals("handshook"))
                {
                    handshook = true;
                    Data tempData = e.Message.Json.GetFirstArgAs<Data>();
                    if (tempData.data == null)
                        return;                    
                    if(tempData.data.Equals("playback"))
                        MainWindow.receivingOtherSidesData = false;
                    else
                        MainWindow.receivingOtherSidesData = true;

                }
                else if (e.Message.Event.Equals("stream"))
                {                    
                    Data tempData = e.Message.Json.GetFirstArgAs<Data>();
                    if (tempData.data == null)
                        return;                    
                    string[] COORDs = tempData.data.Split(',');
                    int objectIndex = Convert.ToInt32(COORDs[0]);
                    MainWindow.StreamsData.networkData[objectIndex] = tempData.data;
                }
            }
        }

        void SocketOpened(object sender, EventArgs e)
        {
            connectionMade = true;
            Console.WriteLine("Connectecd");
        }

        public void Close()
        {
            if (socket != null)
            {
                //socket.Dispose();// Emit("exit", null);
                socket.Opened -= SocketOpened;
                socket.Message -= SocketMessage;
                socket.SocketConnectionClosed -= SocketConnectionClosed;
                socket.Error -= SocketError;                
                socket.Dispose(); // close & dispose of socket client
            }
        }
    }
}
