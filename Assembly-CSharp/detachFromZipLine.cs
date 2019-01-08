using System;
using UnityEngine;

public class detachFromZipLine : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			playerZipLineAction componentInChildren = other.GetComponentInChildren<playerZipLineAction>();
			if (componentInChildren && componentInChildren._onZipLine && componentInChildren._currentZipLine)
			{
				activateZipLine componentInChildren2 = componentInChildren._currentZipLine.parent.GetComponentInChildren<activateZipLine>();
				activateZipLine componentInChildren3 = base.transform.parent.GetComponentInChildren<activateZipLine>();
				if (componentInChildren2 && componentInChildren3 && componentInChildren2.transform == componentInChildren3.transform)
				{
					componentInChildren.ExitZipLine();
				}
			}
		}
	}
}
