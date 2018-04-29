using System;
using TheForest.Buildings.World;
using TheForest.World;
using UnityEngine;


public class quickBuild : MonoBehaviour
{
	
	
	
	public float newBuildingDamage { get; set; }

	
	private void Start()
	{
		base.Invoke("doFastBuild", 0f);
	}

	
	private void doFastBuild()
	{
		GameObject gameObject;
		if (BoltNetwork.isRunning)
		{
			gameObject = BoltNetwork.Instantiate(this.buildThis, base.transform.position, base.transform.rotation).gameObject;
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate<GameObject>(this.buildThis, base.transform.position, base.transform.rotation);
		}
		if (gameObject && this.newBuildingDamage > 0f)
		{
			gameObject.GetComponent<BuildingHealth>().DamageOnly(new LocalizedHitData
			{
				_damage = this.newBuildingDamage
			}, 0);
		}
		if (gameObject && this.TreeId > -1)
		{
			gameObject.GetComponent<TreeStructure>().TreeId = this.TreeId;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	public GameObject buildThis;

	
	public int TreeId = -1;
}
