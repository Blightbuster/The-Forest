using System;
using UnityEngine;


public class getWalkableSurface : MonoBehaviour
{
	
	public float CustomSlopeLimit;

	
	public getWalkableSurface.walkableType _type;

	
	public enum walkableType
	{
		
		slippery,
		
		normal
	}
}
