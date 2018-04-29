using System;
using TheForest.Utils;
using UnityEngine;


public class artifactBallEffectSetup : MonoBehaviour
{
	
	private void Start()
	{
		this.sqrtRepelRange = this.repelRange * this.repelRange;
	}

	
	public void sendEnemyState(bool attract)
	{
		if (Scene.MutantControler.activeCannibals.Count > 0)
		{
			for (int i = 0; i < Scene.MutantControler.activeCannibals.Count; i++)
			{
				if (Scene.MutantControler.activeCannibals[i] != null)
				{
					if (attract)
					{
						if ((Scene.MutantControler.activeCannibals[i].transform.position - base.transform.position).sqrMagnitude > 900f)
						{
							Scene.MutantControler.activeCannibals[i].SendMessage("onArtifactAttract", base.transform.position, SendMessageOptions.DontRequireReceiver);
						}
					}
					else if ((Scene.MutantControler.activeCannibals[i].transform.position - base.transform.position).sqrMagnitude < this.sqrtRepelRange)
					{
						Scene.MutantControler.activeCannibals[i].SendMessage("onArtifactRepel", base.transform, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
		if (Scene.MutantControler.activeInstantSpawnedCannibals.Count > 0)
		{
			for (int j = 0; j < Scene.MutantControler.activeInstantSpawnedCannibals.Count; j++)
			{
				if (Scene.MutantControler.activeInstantSpawnedCannibals[j] != null)
				{
					if (attract)
					{
						if ((Scene.MutantControler.activeCannibals[j].transform.position - base.transform.position).sqrMagnitude > 900f)
						{
							Scene.MutantControler.activeInstantSpawnedCannibals[j].SendMessage("onArtifactAttract", base.transform.position, SendMessageOptions.DontRequireReceiver);
						}
					}
					else if ((Scene.MutantControler.activeInstantSpawnedCannibals[j].transform.position - base.transform.position).sqrMagnitude < this.sqrtRepelRange)
					{
						Scene.MutantControler.activeInstantSpawnedCannibals[j].SendMessage("onArtifactRepel", base.transform, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
		if (Scene.MutantControler.activeBabies.Count > 0)
		{
			for (int k = 0; k < Scene.MutantControler.activeBabies.Count; k++)
			{
				if (Scene.MutantControler.activeBabies[k] != null)
				{
					if (attract)
					{
						if ((Scene.MutantControler.activeCannibals[k].transform.position - base.transform.position).sqrMagnitude > 900f)
						{
							Scene.MutantControler.activeBabies[k].SendMessage("onArtifactAttract", base.transform.position, SendMessageOptions.DontRequireReceiver);
						}
					}
					else if ((Scene.MutantControler.activeBabies[k].transform.position - base.transform.position).sqrMagnitude < this.sqrtRepelRange)
					{
						Scene.MutantControler.activeBabies[k].SendMessage("onArtifactRepel", base.transform, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
	}

	
	public GameObject effigyEffectGo;

	
	public float repelRange;

	
	private float sqrtRepelRange;
}
