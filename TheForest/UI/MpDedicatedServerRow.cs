using System;
using Steamworks;
using UnityEngine;

namespace TheForest.UI
{
	
	public class MpDedicatedServerRow : MpGameRow
	{
		
		public UILabel _ip;

		
		public GameObject _VACProtected;

		
		public GameObject _passwordProtected;

		
		public GameObject _prefabDbVersionMissmatch;

		
		public gameserveritem_t Server;
	}
}
