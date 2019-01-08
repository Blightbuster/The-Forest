using System;
using Bolt;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;

public class torchLightDetector : MonoBehaviour
{
	private void Start()
	{
		this.spread = 0.065f;
		this.mask = 103966720;
	}

	private void FixedUpdate()
	{
		if (((Scene.Atmosphere.TimeOfDay > 85f && Scene.Atmosphere.TimeOfDay < 272f) || LocalPlayer.IsInCaves) && this.updateCount++ > 8)
		{
			this.doTorchRay();
			this.updateCount = 0;
		}
	}

	private void doTorchRay()
	{
		Vector3 vector = base.transform.forward;
		vector = vector + base.transform.up * UnityEngine.Random.Range(-this.spread, this.spread) + base.transform.right * UnityEngine.Random.Range(-this.spread, this.spread);
		if (Physics.Raycast(base.transform.position, vector, out this.hit, 85f, this.mask) && !this.hit.collider.isTrigger && this.hit.distance > 10f)
		{
			bool flag = false;
			for (int i = 0; i < Scene.SceneTracker.allVisTargets.Count; i++)
			{
				if (Scene.SceneTracker.allVisTargets[i] != null && (Scene.SceneTracker.allVisTargets[i].transform.position - this.hit.point).sqrMagnitude < 64f)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				if (BoltNetwork.isClient)
				{
					syncTorchLight syncTorchLight = syncTorchLight.Create(GlobalTargets.OnlyServer, ReliabilityModes.Unreliable);
					syncTorchLight.position = this.hit.point;
					syncTorchLight.sourcePosition = base.transform.position;
					syncTorchLight.distanceToPlayer = this.hit.distance;
					syncTorchLight.Send();
				}
				Transform transform = PoolManager.Pools["misc"].Spawn(this.visDummyGo.transform, this.hit.point, Quaternion.identity);
				torchLightSetup component = transform.GetComponent<torchLightSetup>();
				component.sourcePos = base.transform.position;
				component.distanceToPlayer = this.hit.distance;
			}
		}
	}

	private LayerMask mask;

	private RaycastHit hit;

	private int updateCount;

	public GameObject visDummyGo;

	public float spread = 0.1f;
}
