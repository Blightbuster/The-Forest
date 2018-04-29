using System;
using UnityEngine;


public class CoopWaterColliderRoot : MonoBehaviour
{
	
	private void Awake()
	{
		CoopWaterColliderRoot.All = this._all;
	}

	
	[SerializeField]
	private CoopWaterCollider[] _all;

	
	public static CoopWaterCollider[] All;
}
