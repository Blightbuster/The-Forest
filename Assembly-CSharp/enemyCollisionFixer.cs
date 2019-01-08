using System;
using TheForest.Utils;
using UnityEngine;

public class enemyCollisionFixer : MonoBehaviour
{
	public void OnCollisionEnter(Collision other)
	{
		if (other.collider.gameObject.CompareTag("enemyRoot") && !this.netPrefab)
		{
			LocalPlayer.FpCharacter.clampVelocityTowardEnemy(other.transform.position);
		}
	}

	public void OnCollisionStay(Collision other)
	{
	}

	public void OnCollisionExit(Collision other)
	{
	}

	public bool netPrefab;
}
