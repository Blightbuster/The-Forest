using System;
using UnityEngine;


public class tempDummyFixer : MonoBehaviour
{
	
	private void Awake()
	{
		if (!BoltNetwork.isRunning)
		{
			foreach (Component component in this.DestroyIfNotBolt)
			{
				if (component)
				{
					UnityEngine.Object.Destroy(component);
				}
			}
		}
	}

	
	public Component[] DestroyIfNotBolt;
}
