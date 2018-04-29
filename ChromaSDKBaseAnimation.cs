using System;
using System.Collections.Generic;
using ChromaSDK;
using ChromaSDK.ChromaPackage.Model;
using UnityEngine;


public class ChromaSDKBaseAnimation : MonoBehaviour, IUpdate
{
	
	public virtual List<EffectResponseId> GetEffects()
	{
		return null;
	}

	
	public virtual void Update()
	{
		if (ChromaConnectionManager.Instance.Connected)
		{
		}
	}

	
	[Serializable]
	public class ColorArray
	{
		
		[SerializeField]
		public int[] Colors;
	}
}
