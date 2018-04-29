using System;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;


[DoNotSerializePublic]
public class Digger : MonoBehaviour
{
	
	private void Awake()
	{
		this.layer = 26;
		this.layerMask = 1 << this.layer;
	}

	
	private void Start()
	{
		if (ForestVR.Prototype)
		{
			base.enabled = false;
			return;
		}
		this.snowStartHeight = Terrain.activeTerrain.materialTemplate.GetFloat("_SnowStartHeight");
		this.snowFadeLength = Terrain.activeTerrain.materialTemplate.GetFloat("_SnowFadeLength");
		this.snowStartHeight += this.snowFadeLength / 4f;
		this.snowFadeLength /= 2f;
	}

	
	public void doDig()
	{
		if (!LocalPlayer.Inventory.RightHand._heldWeaponInfo)
		{
			return;
		}
		if (Physics.Raycast(LocalPlayer.Inventory.RightHand._heldWeaponInfo.transform.position, LocalPlayer.Inventory.RightHand._heldWeaponInfo.transform.up, out this.hit, 20f, this.layerMask) && this.hit.collider.CompareTag("TerrainMain"))
		{
			this.spawnDigParticle(this.hit.point);
			if (FMOD_StudioSystem.instance)
			{
				FMOD_StudioSystem.instance.PlayOneShot(this.DirtHitEvent, base.transform.position, null);
			}
			this.Dice = UnityEngine.Random.Range(0, 4);
			this.Dice2 = UnityEngine.Random.Range(0, 20);
			if (this.Dice == 1)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.Rock, this.hit.point, base.transform.rotation);
				gameObject.transform.eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			}
			if (this.Dice2 == 1)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.SpecialItem, this.hit.point, base.transform.rotation);
				gameObject2.transform.eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			}
		}
	}

	
	private void OnTriggerExit(Collider other)
	{
	}

	
	private void spawnDigParticle(Vector3 pos)
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		Vector3 pos2 = pos;
		pos2.y = Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.transform.position.y;
		if (this.IsOnSnow())
		{
			PoolManager.Pools["Particles"].Spawn(LocalPlayer.ScriptSetup.events.snowFootParticle.transform, pos2, Quaternion.identity);
			return;
		}
		int prominantTextureIndex = TerrainHelper.GetProminantTextureIndex(base.transform.position);
		if (prominantTextureIndex == 6 || prominantTextureIndex == 1)
		{
			PoolManager.Pools["Particles"].Spawn(LocalPlayer.ScriptSetup.events.leafFootParticle.transform, pos2, Quaternion.identity);
		}
		else if (prominantTextureIndex == 3 || prominantTextureIndex == 0)
		{
			PoolManager.Pools["Particles"].Spawn(LocalPlayer.ScriptSetup.events.dustFootParticle.transform, pos2, Quaternion.identity);
		}
		else if (prominantTextureIndex == 4)
		{
			PoolManager.Pools["Particles"].Spawn(LocalPlayer.ScriptSetup.events.sandFootParticle.transform, pos2, Quaternion.identity);
		}
	}

	
	private void Update()
	{
	}

	
	private bool IsOnSnow()
	{
		if (base.transform.position.z < -300f && base.transform.position.y > this.snowStartHeight)
		{
			Terrain activeTerrain = Terrain.activeTerrain;
			if (!activeTerrain || this.snowFadeLength <= 0f)
			{
				return true;
			}
			Vector3 vector = activeTerrain.transform.InverseTransformPoint(base.transform.position);
			TerrainData terrainData = activeTerrain.terrainData;
			Vector2 vector2 = new Vector2(vector.x / terrainData.size.x, vector.z / terrainData.size.z);
			Vector3 interpolatedNormal = terrainData.GetInterpolatedNormal(vector2.x, vector2.y);
			float num = (base.transform.position.y - this.snowStartHeight) / this.snowFadeLength;
			num -= (1f - interpolatedNormal.y * interpolatedNormal.y) * 2f;
			num += 0.5f;
			if (num >= 1f || (num > 0f && UnityEngine.Random.value < num))
			{
				return true;
			}
		}
		return false;
	}

	
	public GameObject SpecialItem;

	
	public GameObject Rock;

	
	public string DirtHitEvent;

	
	public GameObject Burst;

	
	private int Dice;

	
	private int Dice2;

	
	private float snowStartHeight;

	
	private float snowFadeLength;

	
	private RaycastHit hit;

	
	private int layer;

	
	private int layerMask;
}
