using System;
using Bolt;
using UnityEngine;

public class CoopAnimalServer : CoopBase<IAnimalState>
{
	private void Start()
	{
		if (BoltNetwork.isRunning && !base.GetComponent<BoltEntity>())
		{
			SpawnBunny spawnBunny = SpawnBunny.Create(GlobalTargets.OnlyServer);
			spawnBunny.Pos = base.transform.position;
			spawnBunny.Rot = base.transform.rotation;
			spawnBunny.Send();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public override void Attached()
	{
		CoopAnimal coopAnimal = base.GetComponentInChildren<CoopAnimal>();
		if (!coopAnimal)
		{
			coopAnimal = base.GetComponent<CoopAnimal>();
		}
		if (coopAnimal.rotationTransform)
		{
			base.state.TransformPosition.SetTransforms(base.transform);
			base.state.TransformRotation.SetTransforms(coopAnimal.rotationTransform);
		}
		else
		{
			base.state.TransformFull.SetTransforms(base.transform);
		}
	}

	public GameObject NetworkContainerPrefab;

	public bool NonPooled;
}
