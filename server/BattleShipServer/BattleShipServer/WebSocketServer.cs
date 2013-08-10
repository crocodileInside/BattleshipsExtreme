using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System.Security.Cryptography;

namespace BattleShipServer
{
	public class WebSocketServer
	{
		private TcpListener tcpListener;
		private Thread listenThread;
		private WebSocketProtocol protocol;

		public WebSocketServer()
		{
			this.protocol = new WebSocketProtocol ();
			this.tcpListener = new TcpListener(IPAddress.Any, 54321);
			this.listenThread = new Thread(new ThreadStart(ListenForClients));
			this.listenThread.Start();
		}

		private void ListenForClients()
		{
			this.tcpListener.Start();

			while (true)
			{
				//blocks until a client has connected to the server
				TcpClient client = this.tcpListener.AcceptTcpClient();

				//create a thread to handle communication 
				//with connected client
				Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
				clientThread.Start(client);
			}
		}

		private void HandleClient(object par)
		{
			TcpClient client = (TcpClient)par;
			NetworkStream stream = client.GetStream();

			if (! AcceptWebSocketConnection(client, stream))
				return;
			while (client.Connected)
			{
				string msg = nextMessage(stream);
				if (msg == null)
					break;
				Console.WriteLine ("\nFrom '" + ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString () + 
				                   "'\nReceived: \n----------------------------\n" +
				                   msg + "\n-----------------------------");
				//Processing here
				sendMessage (stream, "1Hello");
			}

			stream.Close ();
			client.Close ();
		}

		private string nextMessage(NetworkStream stream)
		{
			protocol.reset();
			while (protocol.needsToBeFinished)
			{
				byte[] received;
				if ((received = ReceiveBuffer (stream)).Length == 0)
					return null;
				protocol.next (received);
				if (protocol.status == WebSocketStatus.WS_CONNECTION_CLOSE)
					return null;
			}
			return protocol.message;

		}

		private void sendMessage(NetworkStream stream, string msg)
		{
			WebSocketPackage p = new WebSocketPackage ();
			p.payload = (new ASCIIEncoding ()).GetBytes (msg);
			byte[] buffer = protocol.make (p);
			SendBuffer (stream, buffer);
		}

		private bool AcceptWebSocketConnection(TcpClient client, NetworkStream stream)
		{
			string package = ReceiveString(stream);

			if (package.IndexOf ("Update: websocket") >= 0 || package.IndexOf("Sec-WebSocket-Key") >= 0)
			{
				string r_key = package.Substring (package.IndexOf("Sec-WebSocket-Key"));
				r_key = r_key.Substring (0, r_key.IndexOf("\n"));
				r_key = r_key.Split (':') [1];
				if (r_key.StartsWith (" "))
					r_key = r_key.Substring (1);
				r_key = r_key.Substring (0, r_key.IndexOf ("==") + 2);

				Console.WriteLine("Received key: {0}", r_key);

				ASCIIEncoding enc = new ASCIIEncoding ();

				byte[] full_key_buffer = enc.GetBytes (r_key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");

				SHA1 sha1 = new SHA1CryptoServiceProvider();
				byte[] result = sha1.ComputeHash(full_key_buffer);

				string key = Convert.ToBase64String (result);

				Console.WriteLine("Converted into key: {0}", key);

				string reply = 
					"HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\n" +
					"Sec-WebSocket-Accept: ";
				reply += key + "\r\n\r\n";
				SendString (stream, reply);

				/*if (nextMessage(stream).Equals ("success"))
					return true;*/
				return true;
			};

			return false;
		}


		private string ReceiveString(NetworkStream stream)
		{
			byte[] buffer = new byte[4048];
			try
			{
				stream.Read(buffer, 0, 4048);
			}
			catch(System.Exception)
			{
				return "";
			}
			ASCIIEncoding enc = new ASCIIEncoding();
			string str = enc.GetString(buffer);

			return str;
		}

		private byte[] ReceiveBuffer(NetworkStream stream)
		{
			byte[] buffer = new byte[4048];
			try
			{
				stream.Read(buffer, 0, 4048);
			}
			catch(System.Exception)
			{
				return new byte[0];
			}
			for (int i = 0; i < buffer.Length; i++)
				if (buffer [i] == 0)
					buffer = SubByteArray (buffer, 0, i);
			return buffer;
		}

		private void SendString(NetworkStream stream, string msg)
		{
			ASCIIEncoding enc = new ASCIIEncoding();

			byte[] buffer = enc.GetBytes(msg);

			stream.Write (buffer, 0, buffer.Length);
		}

		private void SendBuffer(NetworkStream stream, byte[] buffer)
		{
			stream.Write (buffer, 0, buffer.Length);
		}

		private byte[] SubByteArray(byte[] data, int index, long length)
		{
			byte[] result = new byte[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}
	
	}
}

