using System;
using System.Text;

namespace BattleShipServer
{
	public enum WebSocketStatus
	{
		WS_CONTINUATION_FRAME, 
		WS_TEXT_FRAME, 
		WS_BINARY_FRAME, 
		WS_CONNECTION_CLOSE, 
		WS_PING, 
		WS_PONG, 
		WS_OTHER
	};

	public class WebSocketProtocol
	{
		public bool needsToBeFinished;
		public WebSocketStatus status;
		public string message;

		public WebSocketProtocol ()
		{
			needsToBeFinished = true;
			status = WebSocketStatus.WS_OTHER;
			message = "";
		}

		public void next(byte[] data)
		{
			WebSocketPackage wsp = new WebSocketPackage(data);
			string msg = decrypt (wsp.payload, wsp.mask, wsp.MASK);
			if (needsToBeFinished)
				message += msg;
			switch(wsp.opcode)
			{
				case 0:
					status = WebSocketStatus.WS_CONTINUATION_FRAME;
					break;
				case 1:
					status = WebSocketStatus.WS_TEXT_FRAME;
					break;
				case 2:
					status = WebSocketStatus.WS_BINARY_FRAME;
					break;
				case 8:
					status = WebSocketStatus.WS_CONNECTION_CLOSE;
					break;
				case 9:
					status = WebSocketStatus.WS_PING;
					break;
				case 10:
					status = WebSocketStatus.WS_PONG;
					break;
				default:
					status = WebSocketStatus.WS_OTHER;
					break;
			}
			if (wsp.FIN)
				needsToBeFinished = !wsp.FIN;
		}
		public void reset()
		{
			needsToBeFinished = true;
			message = "";
		}

		public byte[] make(WebSocketPackage package)
		{
			package.payload = encrypt (package.payload, package.mask, package.MASK);

			return package.make ();
		}

		private byte[] encrypt(byte[] payload, byte[] mask, bool MASK)
		{
			if (MASK)
			{
				for (int i = 0, b; i < payload.Length; i++)
				{
					b = payload [i];
					int j = i % 4;
					payload [i] = (byte)(b ^ (int)mask [j]);
				}
			}
			return payload;
		}

		private string decrypt(byte[] payload, byte[] mask, bool MASK)
		{
			if (MASK)
			{
				for (int i = 0, b; i < payload.Length; i++)
				{
					b = payload [i];
					int j = i % 4;
					payload [i] = (byte)(b ^ (int)mask [j]);
				}
			}
			return (new ASCIIEncoding ()).GetString (payload);
		}
	}

	public class WebSocketPackage
	{
		public bool FIN, RSV1, RSV2, RSV3, MASK;
		public byte opcode;
		public ulong length;
		public byte[] mask;
		public byte[] payload;

		public WebSocketPackage()
		{
			FIN = true;
			RSV1 = false;
			RSV2 = false;
			RSV3 = false;
			MASK = false;
			opcode = 1;
			mask = new byte[0];
		}

		public WebSocketPackage(byte[] data)
		{
			int i = 0;
			FIN  = ((int)data[i] & Convert.ToInt32( "10000000", 2 )) != 0;
			RSV1 = ((int)data[i] & Convert.ToInt32( "01000000", 2 )) != 0;
			RSV2 = ((int)data[i] & Convert.ToInt32( "00100000", 2 )) != 0;
			RSV3 = ((int)data[i] & Convert.ToInt32( "00010000", 2 )) != 0;
			opcode = (byte)((int)data[i] % 16);
			i++;
			MASK = ((int)data[i] & Convert.ToInt32( "10000000", 2 )) != 0;
			length = (byte)((int)data[i] % 128);
			if (length == 127)
			{
				length = makeLongFromByteArray(SubByteArray(data, i, 8));
				i += 7;
			}
			else if (length == 126) 
			{
				length = makeLongFromByteArray(SubByteArray(data, i, 2));
				i += 1;
			}
			i++;
			if(MASK)
			{
				mask = SubByteArray (data, i, 4);
				i += 4;
			}
			if (length + (ulong)i <= (ulong)data.Length)
				payload = SubByteArray(data, i, (long)length);
			else
				payload = SubByteArray(data, i, data.Length - i);
		}

		public byte[] make()
		{
			for (int i2 = 0; i2 < payload.Length; i2++)
				if (payload [i2] == 0)
					payload = SubByteArray(payload, 0, i2);
			if ((ulong)length != (ulong)payload.Length)
				length = (ulong)payload.Length;

			byte[] p;
			if(length > 125)
				p = new byte[4 + (long)mask.Length + (long)length];
			else
				p = new byte[2 + (long)mask.Length + (long)length];

			int i = 0;
			p[i] = 0;
			if (FIN)
				p [i] += 128;
			if (RSV1)
				p [i] += 64;
			if (RSV2)
				p [i] += 32;
			if (RSV3)
				p [i] += 16;
			p[i] += opcode;
			i++;
			p [i] = 0;
			if (MASK)
				p [i] += 128;
			if (length > 125) {
				p [i] += 126;
				i++;
				p [i] = 0;
				ulong ll = length;
				p [i + 1] = (byte)((int)ll % 256);
				ll -= (ulong)((int)ll % 256);	
				p [i] = (byte)(ll / 256);
				i += 2;
			}
			else
			{
				p[i] += (byte)length;
				i++;
			}

			if (MASK)
			{
				for (int i2 = 0; i2 < mask.Length; i2++)
				{
					p [i + i2] = mask [i2];
				}
				i += mask.Length;
			}
			for(ulong i2 = 0; i2 < length; i2++)
			{
				p [(ulong)i + i2] = payload[i2];
			}
			return p;
		}

		private ulong makeLongFromByteArray(byte[] array)
		{
			ulong sum = 0;
			ulong factor = 1;
			for (int i = array.Length - 1; i >= 0; i--)
			{
				sum += factor * array[i];
				factor *= 256;
			}
			return sum;
		}

		private byte[] SubByteArray(byte[] data, int index, long length)
		{
			byte[] result = new byte[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}
	}
}

