using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using Verse;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace yacil
{
	[StaticConstructorOnStartup]
	public unsafe class _yacil
	{
		static _yacil()
		{
			//no op outside of running game instance
			Verse.LongEventHandler.QueueLongEvent (gate_Main, "gate_Main_indirect_call", false, null);
		}

		public class Console2UnityLogWritter : System.IO.TextWriter
		{

			public override System.Text.Encoding Encoding
			{
				get { return null;}
			}

			public override void Write (string value)
			{
				Log.Warning (value);
			}

			public override void Write (object value)
			{
				Log.Warning (value.ToString ());
			}

			public override void WriteLine ()
			{
				return;
			}
		}

		static public void gate_Main()
		{
			Console.SetOut (new Console2UnityLogWritter ());
			Main (null);
		}
			
		static public void fgds()
		{
			while(true)
			{
				srvx.SyncBridgeCall();
				Thread.Sleep(1000);
			}

		}

		static _netLink srvx;

		static public void Main(string[] ignored)
		{
			Console.WriteLine("_yacil entry");

			//srvx = new _netLink(null,null,new IPEndPoint(IPAddress.Loopback,80),"local server",_netLink.LinkType.SERVER);

			//var refx1 = new _netLink(null,null,new IPEndPoint(IPAddress.Loopback,80),"local client 1",_netLink.LinkType.LOCAL);
			//var refx2 = new _netLink(null,null,new IPEndPoint(IPAddress.Loopback,80),"local client 2",_netLink.LinkType.LOCAL);
			//var refx3 = new _netLink(null,null,new IPEndPoint(IPAddress.Loopback,80),"local client 3",_netLink.LinkType.LOCAL);
			//var refx4 = new _netLink(null,null,new IPEndPoint(IPAddress.Loopback,80),"local client 4",_netLink.LinkType.LOCAL);
			//var refx5 = new _netLink(null,null,new IPEndPoint(IPAddress.Loopback,80),"local client 5",_netLink.LinkType.LOCAL);

			//new Thread(fgds).Start();

			//Thread.Sleep(-1);

			Console.WriteLine("_yacil end");
		}
	}
}