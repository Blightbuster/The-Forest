using System;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;


public class snowDustController : MonoBehaviour
{
	
	private void Start()
	{
		this.updateRate = 1f;
		this.nextUpdateDelay = Time.time + this.updateRate;
		this.fluffNextUpdateDelay = Time.time + this.fluffUpdateRate;
		this.lightningBugsNextUpdateDelay = Time.time + this.lightningBugsUpdateRate;
		this.wormsNextUpdateDelay = Time.time + this.wormsUpdateRate;
		this.snowStartHeight = Terrain.activeTerrain.materialTemplate.GetFloat("_SnowStartHeight");
		this.vis = LocalPlayer.GameObject.GetComponent<visRangeSetup>();
	}

	
	private void Update()
	{
		if (Time.time > this.nextUpdateDelay && LocalPlayer.Stats.IsInNorthColdArea() && !LocalPlayer.IsInCaves)
		{
			this.spawnParticleAroundPlayer();
			this.nextUpdateDelay = Time.time + this.updateRate;
		}
	}

	
	private void spawnWorms()
	{
		Vector3 position = LocalPlayer.Transform.position;
		position.y = Terrain.activeTerrain.SampleHeight(position) + Terrain.activeTerrain.transform.position.y;
		if (this.checkKillMask(position))
		{
			float x = (position.x - Terrain.activeTerrain.transform.position.x) / Terrain.activeTerrain.terrainData.size.x;
			float y = (position.z - Terrain.activeTerrain.transform.position.z) / Terrain.activeTerrain.terrainData.size.z;
			Vector3 interpolatedNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(x, y);
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Cross(this.wormsParticle.transform.forward, interpolatedNormal), interpolatedNormal);
			quaternion *= Quaternion.AngleAxis(90f, this.wormsParticle.transform.right);
			PoolManager.Pools["Particles"].Spawn(this.wormsParticle.transform, position, quaternion);
			this.wormsNextUpdateDelay = Time.time + 45f;
		}
	}

	
	private void spawnForestFluff()
	{
		if (this.vis.treeCount > 5f)
		{
			PoolManager.Pools["Particles"].Spawn(this.forestFluffParticle.transform, base.transform.position, this.forestFluffParticle.transform.rotation);
		}
	}

	
	private void spawnLightningBugs()
	{
		if (Scene.Atmosphere.TimeOfDay > 74f && Scene.Atmosphere.TimeOfDay < 220f)
		{
			float num = float.PositiveInfinity;
			for (int i = 0; i < Scene.SceneTracker.fireFliesMarkers.Count; i++)
			{
				float sqrMagnitude = (Scene.SceneTracker.fireFliesMarkers[i].transform.position - base.transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
				}
			}
			if (num < 3600f)
			{
				PoolManager.Pools["Particles"].Spawn(this.lightningBugsParticle.transform, base.transform.position, this.lightningBugsParticle.transform.rotation);
			}
		}
	}

	
	private void spawnParticleAroundPlayer()
	{
		Vector2 vector = this.Circle2(UnityEngine.Random.Range(this.minRange, this.maxRange));
		Vector3 vector2 = new Vector3(LocalPlayer.Transform.position.x + vector.x, LocalPlayer.Transform.position.x, LocalPlayer.Transform.position.z + vector.y);
		vector2.y = Terrain.activeTerrain.SampleHeight(vector2) + Terrain.activeTerrain.transform.position.y;
		if (vector2.y > this.snowStartHeight && this.checkKillMask(vector2))
		{
			float x = (vector2.x - Terrain.activeTerrain.transform.position.x) / Terrain.activeTerrain.terrainData.size.x;
			float y = (vector2.z - Terrain.activeTerrain.transform.position.z) / Terrain.activeTerrain.terrainData.size.z;
			Vector3 interpolatedNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(x, y);
			Quaternion rot = Quaternion.LookRotation(Vector3.Cross(this.snowParticle.transform.right, interpolatedNormal), interpolatedNormal);
			if (Scene.Atmosphere.afs.Wind.w > 1.2f)
			{
				PoolManager.Pools["Particles"].Spawn(this.snowParticle.transform, vector2 + this.snowParticle.transform.position, rot);
			}
			else
			{
				PoolManager.Pools["Particles"].Spawn(this.snowParticleSlow.transform, vector2 + this.snowParticleSlow.transform.position, rot);
			}
		}
	}

	
	private Vector2 Circle2(float radius)
	{
		Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
		return normalized * radius;
	}

	
	private bool checkKillMask(Vector3 pos)
	{
		Terrain activeTerrain = Terrain.activeTerrain;
		if (activeTerrain)
		{
			Vector3 position = activeTerrain.GetPosition();
			TerrainData terrainData = activeTerrain.terrainData;
			int alphamapResolution = terrainData.alphamapResolution;
			int num = (int)((pos.x - position.x) / terrainData.size.x * (float)alphamapResolution);
			int num2 = (int)((pos.z - position.z) / terrainData.size.z * (float)alphamapResolution);
			bool result = false;
			if (num >= 0 && num2 >= 0 && num < alphamapResolution && num2 < alphamapResolution)
			{
				float[,,] alphamaps = activeTerrain.terrainData.GetAlphamaps(num, num2, 1, 1);
				float num3 = 0f;
				int num4 = -1;
				for (int i = terrainData.alphamapLayers - 1; i >= 0; i--)
				{
					if (alphamaps[0, 0, i] > num3)
					{
						num3 = alphamaps[0, 0, i];
						num4 = i;
					}
				}
				if (num4 >= 0)
				{
					for (int j = 0; j < this.TerrainTextureMask.Length; j++)
					{
						if (this.TerrainTextureMask[j] == num4)
						{
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}
		return false;
	}

	
	public GameObject snowParticle;

	
	public GameObject snowParticleSlow;

	
	public GameObject forestFluffParticle;

	
	public GameObject lightningBugsParticle;

	
	public GameObject wormsParticle;

	
	private visRangeSetup vis;

	
	public float minRange = 30f;

	
	public float maxRange = 60f;

	
	public float updateRate = 1f;

	
	public float fluffUpdateRate = 10f;

	
	public float lightningBugsUpdateRate = 5f;

	
	public float wormsUpdateRate = 5f;

	
	public int[] TerrainTextureMask;

	
	public int[] wormsTerrainTextureMask;

	
	private float nextUpdateDelay;

	
	private float fluffNextUpdateDelay;

	
	private float lightningBugsNextUpdateDelay;

	
	private float wormsNextUpdateDelay;

	
	private float snowStartHeight;
}
