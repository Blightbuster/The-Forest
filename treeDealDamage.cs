using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.World;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class treeDealDamage : MonoBehaviour
{
	
	private void Start()
	{
		this._rb = base.transform.GetComponent<Rigidbody>();
		this._col = base.transform.GetComponent<CapsuleCollider>();
		this.initBool = false;
		base.Invoke("doInit", 2f);
	}

	
	private void doInit()
	{
		this.initBool = true;
	}

	
	private void OnDestroy()
	{
		this._rb = null;
		this._col = null;
		this.treeDust = null;
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (!this.initBool)
		{
			return;
		}
		if (other.gameObject.CompareTag("playerHitDetect") || other.gameObject.CompareTag("enemyCollide") || other.gameObject.CompareTag("animalCollide"))
		{
			base.StartCoroutine("calculateDamage", other.gameObject);
		}
		if (other.gameObject.CompareTag("Tree"))
		{
			return;
		}
		if (other.gameObject.CompareTag("TerrainMain"))
		{
			float x = (this._col.bounds.center.x - Terrain.activeTerrain.transform.position.x) / Terrain.activeTerrain.terrainData.size.x;
			float y = (this._col.bounds.center.z - Terrain.activeTerrain.transform.position.z) / Terrain.activeTerrain.terrainData.size.z;
			Vector3 interpolatedNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(x, y);
			Quaternion rotation = Quaternion.LookRotation(Vector3.Cross(base.transform.forward, interpolatedNormal), interpolatedNormal);
			UnityEngine.Object.Instantiate(this.treeDust, this._col.bounds.center, rotation);
		}
		base.StartCoroutine("calculateDamage", other.gameObject);
	}

	
	private void OnCollisionEnter(Collision other)
	{
		if (!this.initBool)
		{
			return;
		}
		if (other.gameObject.CompareTag("playerHitDetect") || other.gameObject.CompareTag("enemyCollide") || other.gameObject.CompareTag("animalCollide"))
		{
			base.StartCoroutine("calculateDamage", other.gameObject);
		}
		else
		{
			string tag = other.gameObject.tag;
			if (tag != null)
			{
				if (treeDealDamage.<>f__switch$map6 == null)
				{
					treeDealDamage.<>f__switch$map6 = new Dictionary<string, int>(4)
					{
						{
							"structure",
							0
						},
						{
							"SLTier1",
							0
						},
						{
							"SLTier2",
							0
						},
						{
							"SLTier3",
							0
						}
					};
				}
				int num;
				if (treeDealDamage.<>f__switch$map6.TryGetValue(tag, out num))
				{
					if (num == 0)
					{
						base.StartCoroutine("calculateDamage", other.gameObject);
					}
				}
			}
		}
	}

	
	private IEnumerator calculateDamage(GameObject target)
	{
		if (this._col)
		{
			this.currPos = this._col.bounds.center;
			yield return YieldPresets.WaitForFixedUpdate;
			yield return YieldPresets.WaitForFixedUpdate;
			if (target && this._col)
			{
				if (BoltNetwork.isClient)
				{
					this.damage = 25f;
				}
				else
				{
					this.damage = (this._col.bounds.center - this.currPos).magnitude * this._rb.mass * 70f;
				}
				int sendDamage = (int)this.damage;
				if (this.damage > 15f)
				{
					if (target.CompareTag("playerHitDetect"))
					{
						target.SendMessageUpwards("hitFromEnemy", sendDamage, SendMessageOptions.DontRequireReceiver);
					}
					else
					{
						string tag = target.tag;
						if (tag != null)
						{
							if (treeDealDamage.<>f__switch$map7 == null)
							{
								treeDealDamage.<>f__switch$map7 = new Dictionary<string, int>(4)
								{
									{
										"structure",
										0
									},
									{
										"SLTier1",
										0
									},
									{
										"SLTier2",
										0
									},
									{
										"SLTier3",
										0
									}
								};
							}
							int num;
							if (treeDealDamage.<>f__switch$map7.TryGetValue(tag, out num))
							{
								if (num == 0)
								{
									target.SendMessage("LocalizedHit", new LocalizedHitData(this._col.bounds.center, 100000f), SendMessageOptions.DontRequireReceiver);
									goto IL_277;
								}
							}
						}
						if (!target.gameObject.CompareTag("Player"))
						{
							target.SendMessage("Hit", sendDamage, SendMessageOptions.DontRequireReceiver);
						}
					}
				}
			}
		}
		IL_277:
		yield break;
	}

	
	private Rigidbody _rb;

	
	private float damage;

	
	private CapsuleCollider _col;

	
	private Vector3 currPos;

	
	private Vector3 nextPos;

	
	public GameObject treeDust;

	
	private bool initBool;
}
