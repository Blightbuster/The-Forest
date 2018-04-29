using System;
using Bolt;
using TheForest.Utils;
using TheForest.World;
using UnityEngine;


public class ExplodeTreeStump : MonoBehaviour
{
	
	private void Awake()
	{
		if (!BoltNetwork.isRunning)
		{
			this._tree = base.GetComponentInParent<TreeHealth>();
			if (this._tree)
			{
				this._coolDown = 0f;
			}
			else
			{
				this._coolDown = -1f;
			}
		}
	}

	
	private void Update()
	{
		if (this._coolDown > 0f)
		{
			this._coolDown -= Time.deltaTime;
			if (this._coolDown <= 0f)
			{
				this._coolDown = 0f;
			}
		}
		else if (this._coolDown < 0f)
		{
			this._tree = base.GetComponentInParent<TreeHealth>();
			if (this._tree)
			{
				this._coolDown = 0f;
			}
			else
			{
				this._coolDown = 1f;
			}
		}
	}

	
	public void CoolDown()
	{
		this._coolDown = 1f;
	}

	
	private void OnSpawned()
	{
		this._hp = 250f;
	}

	
	private void lookAtExplosion(Vector3 position)
	{
		Vector3 b = new Vector3(1f, 0f, 1f);
		if (!this._idleIfPresent && Vector3.Distance(Vector3.Scale(position, b), Vector3.Scale(base.transform.position, b)) < 3.5f)
		{
			if (LocalPlayer.Sfx)
			{
				LocalPlayer.Sfx.PlayBreakWood(base.gameObject);
			}
			LOD_Stump componentInParent = base.GetComponentInParent<LOD_Stump>();
			LOD_Trees lod_Trees = (!componentInParent) ? base.GetComponentInParent<LOD_Trees>() : componentInParent.transform.parent.GetComponent<LOD_Trees>();
			if (lod_Trees)
			{
				if (componentInParent && componentInParent.Pool.IsSpawned(base.transform))
				{
					base.transform.parent = componentInParent.Pool.transform;
					componentInParent.Pool.Despawn(base.transform);
				}
				if (!BoltNetwork.isRunning)
				{
					this.Finalize(lod_Trees, (!componentInParent) ? base.gameObject : componentInParent.gameObject);
				}
				else
				{
					RemoveStump removeStump = RemoveStump.Create(GlobalTargets.OnlyServer);
					removeStump.TargetTree = lod_Trees.GetComponent<BoltEntity>();
					if (this._blownUpStump)
					{
						removeStump.Position = base.transform.position;
						removeStump.Rotation = base.transform.rotation;
						removeStump.CutUpStumpPrefabId = this._blownUpStump.GetComponent<BoltEntity>().prefabId;
					}
					removeStump.Send();
					this.Finalize((!BoltNetwork.isServer) ? null : lod_Trees, (!componentInParent) ? base.gameObject : componentInParent.gameObject);
				}
			}
			else
			{
				TreeHealth componentInParent2 = base.GetComponentInParent<TreeHealth>();
				if (componentInParent2 && componentInParent2.LodTree)
				{
					if (BoltNetwork.isRunning)
					{
						CoopTreeId component = componentInParent2.LodTree.GetComponent<CoopTreeId>();
						if (component)
						{
							component.Goto_Removed();
						}
					}
					this.Finalize(componentInParent2.LodTree, componentInParent2.gameObject);
				}
			}
		}
	}

	
	private void LocalizedHit(LocalizedHitData data)
	{
		if (!this._idleIfPresent)
		{
			if (BoltNetwork.isRunning)
			{
				CoopTreeId coopTreeId = base.GetComponentInParent<CoopTreeId>();
				if (!coopTreeId)
				{
					TreeHealth componentInParent = base.GetComponentInParent<TreeHealth>();
					if (componentInParent && componentInParent.LodTree)
					{
						coopTreeId = componentInParent.LodTree.GetComponent<CoopTreeId>();
					}
				}
				if (coopTreeId && coopTreeId.state.State != 3)
				{
					return;
				}
			}
			else if (this._tree)
			{
				if (this._tree.Health > 0)
				{
					return;
				}
			}
			else if (this._coolDown != 0f)
			{
				return;
			}
			Prefabs.Instance.SpawnHitPS(HitParticles.Wood, data._position, Quaternion.LookRotation(base.transform.position - data._position));
			if (this._hp > 0f)
			{
				this._hp -= data._damage;
				if (this._hp <= 0f)
				{
					this.lookAtExplosion(base.transform.position);
				}
			}
		}
	}

	
	private void Finalize(LOD_Trees lt, GameObject go)
	{
		if (this._blownUpStump && !BoltNetwork.isRunning)
		{
			this.NavCheck();
			UnityEngine.Object.Instantiate(this._blownUpStump, base.transform.position, base.transform.rotation);
		}
		if (lt)
		{
			UnityEngine.Object.Destroy(lt);
		}
		if (go)
		{
			UnityEngine.Object.Destroy(go);
		}
	}

	
	private void NavCheck()
	{
		Collider component = base.transform.GetComponent<Collider>();
		if (component)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate((GameObject)Resources.Load("dummyNavRemove"), base.transform.position, base.transform.rotation);
			gameObject.SendMessage("doDummyNavRemove", component.bounds, SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public GameObject _idleIfPresent;

	
	public GameObject _blownUpStump;

	
	private float _hp = 250f;

	
	private float _coolDown = 1f;

	
	private TreeHealth _tree;
}
