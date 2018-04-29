using System;
using UnityEngine;


public class ForestVR : MonoBehaviour
{
	
	
	
	public static bool Enabled { get; set; }

	
	private void Awake()
	{
		ForestVR.Prototype = this.MakeThisScenePrototype;
	}

	
	public static bool Prototype;

	
	[Tooltip("Is VR prototype behaviour enabled on this scene (stubs out game code so can run in testbed)")]
	public bool MakeThisScenePrototype = true;
}
