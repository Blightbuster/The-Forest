using System;
using UnityEngine;

public class wormExplosionTrigger : MonoBehaviour
{
	private void Start()
	{
	}

	private void explodeAllPoints()
	{
		for (int i = 0; i < this.explodePoints.Length; i++)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.ExplosionPrefab, this.explodePoints[i].position, Quaternion.identity);
			FMODCommon.PlayOneshotNetworked("event:/mutants/creepies/Worm/worm_fall_impact", this.explodePoints[i], FMODCommon.NetworkRole.Server);
		}
	}

	private void largeExplodeAllPoints()
	{
		for (int i = 0; i < this.explodePoints.Length; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.ExplosionPrefab, this.explodePoints[i].position, Quaternion.identity);
			gameObject.GetComponent<Explode>().radius = 23f;
			FMODCommon.PlayOneshotNetworked("event:/mutants/creepies/Worm/worm_fall_impact", this.explodePoints[i], FMODCommon.NetworkRole.Server);
		}
	}

	private void explodeSinglePoint(int p = 0)
	{
		UnityEngine.Object.Instantiate<GameObject>(this.ExplosionPrefab, this.explodePoints[p].position, Quaternion.identity);
		FMODCommon.PlayOneshotNetworked("event:/mutants/creepies/Worm/worm_fall_impact", this.explodePoints[p], FMODCommon.NetworkRole.Server);
	}

	public Transform[] explodePoints;

	public GameObject ExplosionPrefab;
}
