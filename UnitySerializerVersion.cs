using System;
using UnityEngine;


public class UnitySerializerVersion : MonoBehaviour
{
	
	static UnitySerializerVersion()
	{
		
		int[] array = new int[3];
		array[0] = 2;
		array[1] = 5;
		UnitySerializerVersion.version = array;
	}

	
	public static int[] version;
}
