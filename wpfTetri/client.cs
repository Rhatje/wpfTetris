using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace client
{
    class Client
    {
        public string ID
        {
            get;
            private set;
        }
        public IPEndPoint EndPoint
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            set;
        }

        public Socket sck
        {
            get;
            set;
        }
        public Client(Socket accepted)
        {
            sck = accepted;
            ID = Guid.NewGuid().ToString();
            EndPoint = (IPEndPoint)sck.RemoteEndPoint;
            //sck.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);
        }
        public void begin_receive()
        {
            sck.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);
        }
        void callback(IAsyncResult ar)
        {
            try
            {
                sck.EndReceive(ar);

                byte[] buf = new byte[8192];

                int rec = sck.Receive(buf, buf.Length, 0);

                if (rec < buf.Length)
                {
                    Array.Resize<byte>(ref buf, rec);
                }
                if (Received != null)
                {
                    Received(this, buf);
                }
                sck.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);
            }
            catch (Exception ex)
            {
                close();
                if (Disconected != null)
                {
                    Disconected(this);
                }
            }
        }
        public void close()
        {
            sck.Close();
            sck.Dispose();
        }

        public delegate void ClientReceivedHandler(Client sender, byte[] data);
        public delegate void ClientDisconnectedHandler(Client sender);

        public event ClientReceivedHandler Received;
        public event ClientDisconnectedHandler Disconected;
    }
}
