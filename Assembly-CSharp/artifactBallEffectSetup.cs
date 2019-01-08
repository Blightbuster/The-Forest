using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

public class artifactBallEffectSetup : MonoBehaviour
{
	private void Start()
	{
		this.sqrRepelRange = this.repelRange * this.repelRange;
	}

	public void sendEnemyState(bool attract)
	{
		artifactBallEffectSetup.SendEnemyState(Scene.MutantControler.activeCannibals, base.transform, attract, this.sqrRepelRange);
		artifactBallEffectSetup.SendEnemyState(Scene.MutantControler.activeInstantSpawnedCannibals, base.transform, attract, this.sqrRepelRange);
		artifactBallEffectSetup.SendEnemyState(Scene.MutantControler.activeBabies, base.transform, attract, this.sqrRepelRange);
		artifactBallEffectSetup.SendEnemyState(Scene.MutantControler.activeWorms, base.transform, attract, this.sqrRepelRange);
	}

	private static void SendEnemyState(List<GameObject> targets, Transform sourceTransform, bool attract, float sqrRepelRange)
	{
		if (targets == null || targets.Count == 0)
		{
			return;
		}
		foreach (GameObject eachTarget in targets)
		{
			artifactBallEffectSetup.SendEnemyState(eachTarget, sourceTransform, attract, sqrRepelRange);
		}
	}

	private static void SendEnemyState(GameObject eachTarget, Transform sourceTransform, bool attract, float sqrRepelRange)
	{
		if (eachTarget == null)
		{
			return;
		}
		Vector3 position = eachTarget.transform.position;
		Vector3 vector = position - sourceTransform.position;
		if (attract)
		{
			if (vector.sqrMagnitude > 900f)
			{
				eachTarget.SendMessage("onArtifactAttract", sourceTransform, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (vector.sqrMagnitude < sqrRepelRange)
		{
			eachTarget.SendMessage("onArtifactRepel", sourceTransform, SendMessageOptions.DontRequireReceiver);
		}
	}

	private const string ArtifactAttractMessage = "onArtifactAttract";

	private const string ArtifactRepelMessage = "onArtifactRepel";

	private const int MaxAffectDistance = 900;

	public GameObject effigyEffectGo;

	public float repelRange;

	private float sqrRepelRange;
}
