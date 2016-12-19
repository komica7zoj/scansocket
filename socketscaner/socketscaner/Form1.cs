using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace socketscaner
{
    public class Client
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public partial class Form1 : Form
    {
        // The port number for the remote device.
        private static int port =136;
        //private static string ip = "192.168.242.35";
        private static string ip = "10.100.50.60";
        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);
        private static String response = String.Empty;
        static string ss = "";
        public Form1()
        {
            InitializeComponent();
            hey();
        }

        void hey()
        {
            IPAddress ipAddress = IPAddress.Parse(ip);

            while (port != 99999)
            {
                try
                {
                    port++;
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                    Socket client = null;

                    client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                    var result = client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(0.01));
                    //          connectDone.WaitOne();
                    if (!success)
                    {
                        throw new Exception("Failed to connect.");
                    }
                    else
                    {
                        //        MessageBox.Show("hi" + port.ToString());
                        Send(client, new byte[8]);

                    }
                }

                catch (Exception e)
                {

                }
                if (!ss.Equals(""))
                {
                    MessageBox.Show(ss);
                    hello();
                    port = 99999;
                }
            }

        }


        void hello()
        {
            string s = "";
            IPAddress ipAddress = IPAddress.Parse(ip);

            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                s = open.FileName;
            }
            while (true)
            {
                try
                {
                    port = 139;
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                    Socket client = null;

                    client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                    var result = client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10));
                    //          connectDone.WaitOne();
                    if (!success)
                    {
                        throw new Exception("Failed to connect.");
                    }
                    else
                    {
                        //        MessageBox.Show("hi" + port.ToString());
                        while (true)
                        {
                            using (Stream source = File.OpenRead(s))
                            {

                                byte[] buffer = new byte[214743648];
                                int bytesRead;
                                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    Send(client, buffer);
                                }

                            }
                        }

                    }
                }

                catch (Exception e)
                {

                }

            }
        }









        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);


                // Signal that the connection has been made.
                //          connectDone.Set();
            }
            catch (Exception e)
            {

            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                Client state = new Client();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, Client.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {

            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                Client state = (Client)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, Client.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {

            }
        }

        private static void Send(Socket client, byte[] data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = data;
            // Begin sending the data to the remote device.
            var result = client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
            //          connectDone.WaitOne();
            if (!success)
            {
                throw new Exception("Failed to connect.");
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                ss += ", " + ((IPEndPoint)client.RemoteEndPoint).Port.ToString();
                port = Convert.ToInt32(((IPEndPoint)client.RemoteEndPoint).Port.ToString());
                // Signal that all bytes have been sent.
                //      sendDone.Set();

            }
            catch (Exception e)
            {

            }

        }

    }
}
