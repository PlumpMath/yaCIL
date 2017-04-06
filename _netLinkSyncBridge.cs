using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using yacil;
using System.Net;

namespace Verse
{
	// Token: 0x0200095A RID: 2394
	public class _netLinkSyncBridge : MapComponent
	{
		private _netLink localeventserver;
		private int ticklimiter;


		// Token: 0x06002FEF RID: 12271 RVA: 0x00109A28 File Offset: 0x00107C28
		public _netLinkSyncBridge(Map map) : base(map)
		{
			localeventserver = new _netLink(null,null,new IPEndPoint(IPAddress.Loopback,80),"local server",_netLink.LinkType.SERVER);
			ticklimiter = Environment.TickCount % 60;
		}

		// Token: 0x06002FF0 RID: 12272 RVA: 0x00109A48 File Offset: 0x00107C48
		public override void ExposeData()
		{
		}

		// Token: 0x06002FF1 RID: 12273 RVA: 0x00109AAC File Offset: 0x00107CAC
		public override void MapComponentTick()
		{
			if (++ticklimiter%60 == 0)
				localeventserver.SyncBridgeCall();
		}
	}
}