using System;
using UnityEngine;

namespace TheForest.Audio
{
	
	public abstract class SfxInfo : MonoBehaviour
	{
		
		public SfxInfo.SfxTypes _sfx;

		
		public enum SfxTypes
		{
			
			None = -1,
			
			HitWood,
			
			HitBone,
			
			HitRock,
			
			OpenWoodDoor,
			
			AddItem,
			
			AddLog
		}
	}
}
