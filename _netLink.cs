using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace yacil
{
	public class _netLink
	{

		public Socket linkSocket; //master socket of instance
		private _netLink linkHost; //server\host netlink for remote client or null

		public enum LinkType
		{SERVER,LOCAL,REMOTE}
		private LinkType linkType; //server, local client or remote client

		public string linkName; //name is required for debugging purposes, also it double as client\server ingame name

		public ArrayList linkedClients; //list of remote clients for server
		private int nextClientID; //id that will be given to next client
		private int instanceClientID; //id of current instance

		public ArrayList syncDataQueue; //networking is async, but processing of objects is sync
		//this is list of byte[] objects that was harvested in previous net cycle

		public WaitHandle whReady;

		//ArrayList.Synchronized(new ArrayList(32))


		//data available is never triggered for server itself
		//it always triggered for remote or local client
		//this is because socket is implemented this way, server socket is listening and not recieving

		public byte[] RecieveFully(int recieved, IAsyncResult asres)
		{
			//_yacil.SWK();
			byte[] so = asres.AsyncState as byte[];
			if (so == null)
				throw new Exception("INVALID PAYLOAD");

			int pending = linkSocket.Available;

			byte[] tmp = so;

			if (recieved < 64)
			{
				tmp = new byte[recieved];
				Array.Copy(so,tmp,recieved);
			}

			if (pending > 0)
			{
				//more data can be exctracted from socket right away

				//array that can fit existing data and pending data
				tmp = new byte[pending + 64];

				linkSocket.Receive(tmp,64,pending,SocketFlags.None);

				//data is recieved and placed with 64 offset
				//first 64 bytes are filled with async result
				Array.Copy(so,tmp,64);
			}

			return tmp;
		}
			
		public void ReceiveReadyLocal(IAsyncResult asres)
		{
			//_yacil.SWK();
			//local client accepts data from remote server via singular socket
			int recieved = 0;
			try
			{
				recieved = linkSocket.EndReceive(asres);
			}
			catch (Exception E)
			{
				//Console.WriteLine(E);
				//recieve failed, connection is probably dead, close it completely
				//client will attempt to reconnect
				linkSocket.Close();
				//client have no lists, for this reason, no additional actions are required, connection is dead
				return;
			}

			if (recieved == 0)
				return;
				
			syncDataQueue.Add(RecieveFully(recieved,asres));

			byte[] bb = new byte[64];
			linkSocket.BeginReceive(bb,0,64,SocketFlags.None,ReceiveReadyLocal,bb);
		}

		public void RecieveReadyRemote(IAsyncResult asres)
		{
			//_yacil.SWK();
			int recieved = 0;
			try
			{
				recieved = linkSocket.EndReceive(asres);
			}
			catch (Exception E)
			{
				//Console.WriteLine(E);
				//recieve failed, connection is probably dead, close it completely to release unmanaged resources
				//client will attempt to reconnect
				linkSocket.Close();
			}
				
			//recieve failed or native passed zero intentionally
			//well, we need to close this connection
			if (recieved == 0)
			{
				//Console.WriteLine("DROP " + linkName + " " + instanceClientID);
				//this is guaranteed call for remote client
				//remote client have host reference and list of clients is kept on host
				//list already wrapped with sync wrapper
				linkHost.linkedClients.Remove(this);
				linkSocket.Close(); //noop if socket is already closed
				return;
			}
				
			syncDataQueue.Add(RecieveFully(recieved,asres));

			byte[] bb = new byte[64];
			linkSocket.BeginReceive(bb,0,64,SocketFlags.None,RecieveReadyRemote,bb);

			string hts = 		
				"<html>" +
				"<body>" +
				"<form method=\"post\">" +
				"Command: <input  name=\"rsrswsxx\">" +
				"<input type=\"submit\" value = \"execute\">" +
				"</form></body></html>";
			byte[] Buffer = Encoding.ASCII.GetBytes(hts);
			try
			{
				linkSocket.BeginSend(Buffer,0,Buffer.Length,SocketFlags.None,tmpDropCon,linkSocket);
			}
			catch{};
		}

		public void ConnectReady(IAsyncResult asres)
		{
			//sync method, only one instance will run at any time
			//_yacil.SWK();
			byte[] bb = new byte[64];
			linkSocket.BeginReceive(bb,0,64,SocketFlags.None,ReceiveReadyLocal,bb);
			//_yacil.SWK();
		}

		public void AcceptReady(IAsyncResult asres)
		{
			//sync method, only one instance will run at any time
			linkedClients.Add(new _netLink(this,linkSocket.EndAccept(asres),null,"remote client",LinkType.REMOTE));
			linkSocket.BeginAccept(AcceptReady,null);
		}

		public void tmpDropCon(IAsyncResult asres)
		{
			((Socket)(asres.AsyncState)).Close();
		}

		public void SyncBridgeCall()
		{
			if (linkType == LinkType.REMOTE)
				throw new Exception("Not allowed for remote clients, use server reference instead");

			foreach(_netLink client in (ArrayList)linkedClients.Clone())
			{
				//performance critical stuff

				object[] pendingpackets = (object[])client.syncDataQueue.ToArray();

				if (pendingpackets.Length == 0)
					continue;

				client.syncDataQueue.Clear();
				//performance critical stuff

				foreach (byte[] ssx in pendingpackets)
				{
					string ss = Encoding.ASCII.GetString(ssx);
					int offset = ss.IndexOf("rsrswsxx");

					if (offset == -1)
						continue;

					string eventcandidate = Uri.UnescapeDataString(ss.Substring(offset+1+"rsrswsxx".Length));
					Console.WriteLine("will throw event named " + eventcandidate);

					IncidentDef localDef = DefDatabase<IncidentDef>.GetNamed(eventcandidate,false);
					if (localDef == null)
						continue;

					IncidentParms parms = new IncidentParms();
					parms.target = Find.VisibleMap;
					localDef.Worker.TryExecute(parms);
				}
				//client.linkSocket.Close();
			}
		}

			
		public _netLink (_netLink _linkHost, Socket _linkSocket, 
			IPEndPoint _linkDestination,string _linkName,LinkType _linkMode)
		{
			//semisync method, multiple instances run, no data overlap
			//Console.WriteLine("_netLink CTOR base begin");

			linkSocket = _linkSocket ?? new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
			linkSocket.NoDelay = true;
			linkName = _linkName;
			linkHost = _linkHost;

			if (_linkDestination == null && _linkMode != LinkType.REMOTE)
				throw new Exception("Invalid endpoint");

			if (_linkMode == LinkType.REMOTE && linkHost == null)
				throw new Exception("Orphaned remote client");

			if (linkHost == null)
				instanceClientID = Environment.TickCount;
			else
				instanceClientID = linkHost.nextClientID++;

			switch (_linkMode)
			{
				case LinkType.SERVER:
					//Console.WriteLine("_netLink CTOR server begin");
					linkSocket.Bind(_linkDestination);
					linkSocket.Listen(8);
					linkedClients = ArrayList.Synchronized(new ArrayList(16));
					linkSocket.BeginAccept(AcceptReady,null);
					//Console.WriteLine("_netLink CTOR server exit");
					break;

				case LinkType.REMOTE:
					//Console.WriteLine("_netLink CTOR remote client begin");
					byte[] bb = new byte[64];
					syncDataQueue = ArrayList.Synchronized(new ArrayList(16));
					linkSocket.BeginReceive(bb,0,64,SocketFlags.None,RecieveReadyRemote,bb);
					//Console.WriteLine("_netLink CTOR remote client exit");
					break;

				case LinkType.LOCAL:
					//Console.WriteLine("_netLink CTOR local client begin");
					syncDataQueue = ArrayList.Synchronized(new ArrayList(16));
					whReady = linkSocket.BeginConnect(_linkDestination,ReceiveReadyLocal,null).AsyncWaitHandle;
					//Console.WriteLine("_netLink CTOR local client exit");
					break;
			}
		}
	}
}