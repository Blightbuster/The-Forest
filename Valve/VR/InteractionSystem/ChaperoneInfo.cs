using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	
	public class ChaperoneInfo : MonoBehaviour
	{
		
		
		
		public bool initialized { get; private set; }

		
		
		
		public float playAreaSizeX { get; private set; }

		
		
		
		public float playAreaSizeZ { get; private set; }

		
		
		
		public bool roomscale { get; private set; }

		
		public static SteamVR_Events.Action InitializedAction(UnityAction action)
		{
			return new SteamVR_Events.ActionNoArgs(ChaperoneInfo.Initialized, action);
		}

		
		
		public static ChaperoneInfo instance
		{
			get
			{
				if (ChaperoneInfo._instance == null)
				{
					ChaperoneInfo._instance = new GameObject("[ChaperoneInfo]").AddComponent<ChaperoneInfo>();
					ChaperoneInfo._instance.initialized = false;
					ChaperoneInfo._instance.playAreaSizeX = 1f;
					ChaperoneInfo._instance.playAreaSizeZ = 1f;
					ChaperoneInfo._instance.roomscale = false;
					UnityEngine.Object.DontDestroyOnLoad(ChaperoneInfo._instance.gameObject);
				}
				return ChaperoneInfo._instance;
			}
		}

		
		private IEnumerator Start()
		{
			CVRChaperone chaperone = OpenVR.Chaperone;
			if (chaperone == null)
			{
				Debug.LogWarning("Failed to get IVRChaperone interface.");
				this.initialized = true;
				yield break;
			}
			float px;
			float pz;
			for (;;)
			{
				px = 0f;
				pz = 0f;
				if (chaperone.GetPlayAreaSize(ref px, ref pz))
				{
					break;
				}
				yield return null;
			}
			this.initialized = true;
			this.playAreaSizeX = px;
			this.playAreaSizeZ = pz;
			this.roomscale = (Mathf.Max(px, pz) > 1.01f);
			Debug.LogFormat("ChaperoneInfo initialized. {2} play area {0:0.00}m x {1:0.00}m", new object[]
			{
				px,
				pz,
				(!this.roomscale) ? "Standing" : "Roomscale"
			});
			ChaperoneInfo.Initialized.Send();
			yield break;
			yield break;
		}

		
		public static SteamVR_Events.Event Initialized = new SteamVR_Events.Event();

		
		private static ChaperoneInfo _instance;
	}
}
