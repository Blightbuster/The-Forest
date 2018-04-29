using System;
using UnityEngine;


public class getWalkableSurface : MonoBehaviour
{
	
	public getWalkableSurface.walkableType _type;

	
	public enum walkableType
	{
		
		slippery,
		
		normal
	}
}
