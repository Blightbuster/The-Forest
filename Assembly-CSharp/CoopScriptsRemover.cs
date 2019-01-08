using System;
using Bolt;
using UnityEngine;

public class CoopScriptsRemover : EntityBehaviour
{
	public override void Attached()
	{
		foreach (Component obj in this.RemoveInCoop)
		{
			UnityEngine.Object.Destroy(obj);
		}
	}

	public Component[] RemoveInCoop;
}
