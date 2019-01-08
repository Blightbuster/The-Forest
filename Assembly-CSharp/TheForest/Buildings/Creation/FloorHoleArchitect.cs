using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Bolt;
using TheForest.Buildings.Utils;
using TheForest.Buildings.World;
using TheForest.Utils;
using TheForest.World;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	public class FloorHoleArchitect : MonoBehaviour
	{
		private void Awake()
		{
			if (!this._renderer)
			{
				this._renderer = base.GetComponent<Renderer>();
			}
		}

		private void Update()
		{
			this.CheckNodesforNearbyTargets();
			if (this._previews.Count > 0)
			{
				for (int i = this._previews.Count - 1; i >= 0; i--)
				{
					this._holes[i]._size = this._holeSize;
					this._holes[i]._yRotation = base.transform.rotation.eulerAngles.y;
					this._holes[i]._position = base.transform.position;
					IHoleStructure holeStructure = this._previews[i];
					if (!((MonoBehaviour)holeStructure).enabled)
					{
						holeStructure.CreateStructure(false);
						if (!this._holes[i]._used)
						{
							UnityEngine.Object.Destroy((this._previews[i] as MonoBehaviour).gameObject);
							this._holes[i] = null;
							this._holes.RemoveAt(i);
							this._previews.RemoveAt(i);
							this._targets.RemoveAt(i);
						}
					}
				}
			}
			this.CheckCanPlace();
		}

		private bool IsEnemy(Collider other)
		{
			bool flag = (other.GetType() == typeof(CharacterController) && other.GetComponent<enemyType>() != null) || (other.GetType() == typeof(CapsuleCollider) && other.GetComponent<pushRigidBody>() != null);
			bool flag2 = other.GetType() == typeof(CapsuleCollider) && other.GetComponent<enemyType>() != null && other.GetComponent<Rigidbody>() != null;
			return flag || flag2;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (this.IsEnemy(other))
			{
				this.preventHole = true;
				this.CheckCanPlace();
				return;
			}
			if (other.CompareTag("effigy") || other.GetComponent<FireWarmth>())
			{
				return;
			}
			PrefabIdentifier componentInParent = other.GetComponentInParent<PrefabIdentifier>();
			if (componentInParent)
			{
				BoltEntity component = componentInParent.GetComponent<BoltEntity>();
				if (BoltNetwork.isRunning && component && component.isAttached)
				{
					IRaftState raftState;
					IMultiHolderState multiHolderState;
					if (component.TryFindState<IRaftState>(out raftState))
					{
						foreach (BoltEntity exists in raftState.GrabbedBy)
						{
							if (exists)
							{
								return;
							}
						}
					}
					else if (component.TryFindState<IMultiHolderState>(out multiHolderState) && (multiHolderState.GrabbedBy || multiHolderState.Replaces))
					{
						return;
					}
				}
				this.TryAddTarget(componentInParent, other);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (this.IsEnemy(other))
			{
				this.preventHole = false;
				this.CheckCanPlace();
				return;
			}
			PrefabIdentifier componentInParent = other.GetComponentInParent<PrefabIdentifier>();
			this.TryRemoveTarget(componentInParent, other);
		}

		private void OnDisable()
		{
			if (base.enabled)
			{
				this.OnDestroy();
			}
		}

		private void OnDestroy()
		{
			if (this._targets.Count > 0)
			{
				for (int i = this._targets.Count - 1; i >= 0; i--)
				{
					this._targets[i].RemoveHole(this._holes[i]);
					this._holes[i] = null;
					this._holes.RemoveAt(i);
					if (!((MonoBehaviour)this._targets[i]).enabled)
					{
						this._targets[i].CreateStructure(false);
					}
					this._targets.RemoveAt(i);
					UnityEngine.Object.Destroy((this._previews[i] as MonoBehaviour).gameObject);
					this._previews.RemoveAt(i);
				}
			}
			foreach (KeyValuePair<GameObject, FloorHoleArchitect.DestroyTarget> keyValuePair in this._destroyTargets)
			{
				UnityEngine.Object.Destroy(keyValuePair.Value._ghost);
				keyValuePair.Value.Clear();
			}
		}

		public void OnPlaced()
		{
			if (this._targets.Count > 0 || this._destroyTargets.Count > 0)
			{
				LocalPlayer.Sfx.PlayStructureBreak(base.gameObject, 0.1f);
			}
			if (BoltNetwork.isClient)
			{
				for (int i = 0; i < this._targets.Count; i++)
				{
					try
					{
						CutStructureHole cutStructureHole = CutStructureHole.Create(GlobalTargets.OnlyServer);
						cutStructureHole.TargetStructure = (this._targets[i] as MonoBehaviour).GetComponent<BoltEntity>();
						cutStructureHole.Position = this._holes[i]._position;
						cutStructureHole.YRotation = this._holes[i]._yRotation;
						cutStructureHole.Size = this._holes[i]._size;
						cutStructureHole.Send();
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
					}
				}
				foreach (KeyValuePair<GameObject, FloorHoleArchitect.DestroyTarget> keyValuePair in this._destroyTargets)
				{
					try
					{
						if (keyValuePair.Value._bh)
						{
							keyValuePair.Value._bh.LocalizedHit(new LocalizedHitData
							{
								_damage = 13371337f,
								_distortRatio = 1f,
								_position = base.transform.position
							});
						}
						else
						{
							RemoveBuilding removeBuilding = RemoveBuilding.Create(GlobalTargets.OnlyServer);
							removeBuilding.TargetBuilding = keyValuePair.Value._go.GetComponent<BoltEntity>();
							removeBuilding.Send();
						}
					}
					catch (Exception exception2)
					{
						UnityEngine.Debug.LogException(exception2);
					}
					try
					{
						if (keyValuePair.Value._ghost)
						{
							keyValuePair.Value._ghost.transform.parent = null;
							UnityEngine.Object.Destroy(keyValuePair.Value._ghost);
						}
					}
					catch (Exception exception3)
					{
						UnityEngine.Debug.LogException(exception3);
					}
					keyValuePair.Value.Clear();
				}
				this._destroyTargets.Clear();
				UnityEngine.Object.Destroy(base.gameObject, 0.05f);
			}
			else
			{
				for (int j = 0; j < this._targets.Count; j++)
				{
					this._targets[j].CreateStructure(false);
					(this._targets[j] as MonoBehaviour).SendMessage("OnHolePlaced", SendMessageOptions.DontRequireReceiver);
					UnityEngine.Object.Destroy((this._previews[j] as MonoBehaviour).gameObject);
					this._previews[j] = null;
				}
				if (this._targets.Count > 0)
				{
					Vector3 vector = base.transform.position + new Vector3(this._holeSize.x, 0f, this._holeSize.y).RotateY(this._holes[0]._yRotation);
					Prefabs.Instance.SpawnHitPS(HitParticles.Wood, vector, Quaternion.LookRotation(base.transform.position - vector));
					vector = base.transform.position + new Vector3(this._holeSize.x, 0f, -this._holeSize.y).RotateY(this._holes[0]._yRotation);
					Prefabs.Instance.SpawnHitPS(HitParticles.Wood, vector, Quaternion.LookRotation(base.transform.position - vector));
					vector = base.transform.position + new Vector3(-this._holeSize.x, 0f, -this._holeSize.y).RotateY(this._holes[0]._yRotation);
					Prefabs.Instance.SpawnHitPS(HitParticles.Wood, vector, Quaternion.LookRotation(base.transform.position - vector));
					vector = base.transform.position + new Vector3(-this._holeSize.x, 0f, this._holeSize.y).RotateY(this._holes[0]._yRotation);
					Prefabs.Instance.SpawnHitPS(HitParticles.Wood, vector, Quaternion.LookRotation(base.transform.position - vector));
					this._holes.Clear();
					this._targets.Clear();
					this._previews.Clear();
				}
				foreach (KeyValuePair<GameObject, FloorHoleArchitect.DestroyTarget> keyValuePair2 in this._destroyTargets)
				{
					try
					{
						if (keyValuePair2.Value._bh)
						{
							if (!PlayerPreferences.NoDestruction)
							{
								keyValuePair2.Value._bh.LocalizedHit(new LocalizedHitData
								{
									_damage = keyValuePair2.Value._bh._maxHP * 2f,
									_distortRatio = 1f,
									_position = base.transform.position
								});
							}
							else
							{
								keyValuePair2.Value._bh.Collapse(base.transform.position);
							}
						}
						else if (keyValuePair2.Key)
						{
							keyValuePair2.Key.AddComponent<CollapseStructure>();
						}
					}
					catch (Exception exception4)
					{
						UnityEngine.Debug.LogException(exception4);
					}
					try
					{
						if (keyValuePair2.Value._ghost)
						{
							keyValuePair2.Value._ghost.transform.parent = null;
							UnityEngine.Object.Destroy(keyValuePair2.Value._ghost);
						}
					}
					catch (Exception exception5)
					{
						UnityEngine.Debug.LogException(exception5);
					}
					keyValuePair2.Value.Clear();
				}
				this._destroyTargets.Clear();
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		private bool TryAddTarget(PrefabIdentifier pi, Collider c)
		{
			if (pi)
			{
				IHoleStructure holeStructure = (IHoleStructure)pi.GetComponent(typeof(IHoleStructure));
				if (holeStructure != null)
				{
					if (!this._targets.Contains(holeStructure) && holeStructure.HoleCount < 20)
					{
						Hole item;
						if (holeStructure is FloorArchitect)
						{
							if (!this._previewFloor || !(holeStructure as FloorArchitect).WasBuilt)
							{
								return false;
							}
							FloorArchitect floorArchitect = holeStructure as FloorArchitect;
							FloorArchitect floorArchitect2 = UnityEngine.Object.Instantiate<FloorArchitect>(this._previewFloor, floorArchitect.transform.position, floorArchitect.transform.rotation);
							floorArchitect.OnBuilt(floorArchitect2.gameObject);
							floorArchitect2._wasBuilt = false;
							this._previews.Add(floorArchitect2);
							item = floorArchitect2.AddSquareHole(base.transform.position, base.transform.rotation.y, this._holeSize);
						}
						else if (holeStructure is RoofArchitect)
						{
							if (!this._previewRoof || !(holeStructure as RoofArchitect).WasBuilt)
							{
								return false;
							}
							RoofArchitect roofArchitect = holeStructure as RoofArchitect;
							RoofArchitect roofArchitect2 = UnityEngine.Object.Instantiate<RoofArchitect>(this._previewRoof, roofArchitect.transform.position, roofArchitect.transform.rotation);
							roofArchitect.OnBuilt(roofArchitect2.gameObject);
							roofArchitect2._wasBuilt = false;
							this._previews.Add(roofArchitect2);
							item = roofArchitect2.AddSquareHole(base.transform.position, base.transform.rotation.y, this._holeSize);
						}
						else
						{
							if (holeStructure is CraneArchitect)
							{
								return false;
							}
							if (!(holeStructure is RaftArchitect) || !(holeStructure as RaftArchitect).WasBuilt)
							{
								UnityEngine.Debug.LogError("Trying to cut IHS '" + pi.name + "' which isn't roof, floor or raft. Please report this to guillaume.");
								return false;
							}
							if (!this._previewRaft || !(base.transform.root != pi.transform.root))
							{
								return false;
							}
							RaftArchitect raftArchitect = holeStructure as RaftArchitect;
							RaftArchitect raftArchitect2 = UnityEngine.Object.Instantiate<RaftArchitect>(this._previewRaft, raftArchitect.transform.position, raftArchitect.transform.rotation);
							raftArchitect.OnBuilt(raftArchitect2.gameObject);
							this._previews.Add(raftArchitect2);
							item = raftArchitect2.AddSquareHole(base.transform.position, base.transform.rotation.y, this._holeSize);
						}
						this._targets.Add(holeStructure);
						this._holes.Add(item);
						this.CheckCanPlace();
						return true;
					}
				}
				else
				{
					BuildingBlueprint blueprintByPrefabId = Prefabs.Instance.Constructions.GetBlueprintByPrefabId(pi.ClassId);
					if (blueprintByPrefabId != null && !blueprintByPrefabId._preventHoleCutting)
					{
						FloorHoleArchitect.DestroyTarget destroyTarget;
						if (!this._destroyTargets.TryGetValue(pi.gameObject, out destroyTarget))
						{
							destroyTarget = new FloorHoleArchitect.DestroyTarget();
							destroyTarget._go = pi.gameObject;
							destroyTarget._bh = pi.GetComponent<BuildingHealth>();
							ICoopStructure component = pi.GetComponent<ICoopStructure>();
							if (component != null)
							{
								IProceduralStructure component2 = pi.GetComponent<IProceduralStructure>();
								if (component2 == null)
								{
									return false;
								}
								destroyTarget._ghost = component2.SpawnStructure().gameObject;
							}
							else
							{
								destroyTarget._ghost = UnityEngine.Object.Instantiate<GameObject>(blueprintByPrefabId._ghostPrefab, pi.transform.position, pi.transform.rotation);
							}
							destroyTarget._ghost.transform.parent = pi.transform;
							int layer = LayerMask.NameToLayer("TransparentFX");
							Renderer component3 = destroyTarget._ghost.GetComponent<Renderer>();
							if (component3)
							{
								destroyTarget._ghost.layer = layer;
								component3.sharedMaterial = this._overlayMaterial;
							}
							else
							{
								Craft_Structure componentInChildren = destroyTarget._ghost.GetComponentInChildren<Craft_Structure>();
								if (componentInChildren && componentInChildren._requiredIngredients.Count > 0)
								{
									for (int i = 0; i < componentInChildren._requiredIngredients.Count; i++)
									{
										Craft_Structure.BuildIngredients buildIngredients = componentInChildren._requiredIngredients[i];
										buildIngredients.SetGhostMaterial(this._overlayMaterial);
									}
								}
								else
								{
									Renderer[] componentsInChildren = destroyTarget._ghost.GetComponentsInChildren<Renderer>();
									foreach (Renderer renderer in componentsInChildren)
									{
										renderer.gameObject.layer = layer;
										renderer.sharedMaterial = this._overlayMaterial;
									}
								}
							}
							Transform transform = destroyTarget._ghost.transform.Find("Trigger");
							if (transform)
							{
								UnityEngine.Object.Destroy(transform.gameObject);
							}
							LastBuiltLocation component4 = destroyTarget._ghost.GetComponent<LastBuiltLocation>();
							if (component4)
							{
								UnityEngine.Object.Destroy(component4.gameObject);
							}
							LastBuiltLocation component5 = destroyTarget._ghost.GetComponent<LastBuiltLocation>();
							if (component5)
							{
								UnityEngine.Object.Destroy(component5.gameObject);
							}
							UnityEngine.Object.Destroy(destroyTarget._ghost.GetComponent<PrefabIdentifier>());
							UnityEngine.Object.Destroy(destroyTarget._ghost.GetComponent<SingleAnchorStructure>());
							this._destroyTargets.Add(pi.gameObject, destroyTarget);
						}
						if (c && !destroyTarget._colliders.Contains(c))
						{
							destroyTarget._colliders.Add(c);
						}
						this.CheckCanPlace();
						return true;
					}
				}
			}
			return false;
		}

		private bool TryRemoveTarget(PrefabIdentifier pi, Collider other)
		{
			if (pi)
			{
				IHoleStructure holeStructure = (IHoleStructure)pi.GetComponent(typeof(IHoleStructure));
				if (holeStructure != null)
				{
					Collider component = base.GetComponent<Collider>();
					if (this._targets.Contains(holeStructure) && (other.bounds.max.y < component.bounds.min.y || other.bounds.min.y > component.bounds.max.y))
					{
						int index = this._targets.IndexOf(holeStructure);
						UnityEngine.Object.Destroy((this._previews[index] as MonoBehaviour).gameObject);
						this._targets[index].RemoveHole(this._holes[index]);
						this._holes[index] = null;
						this._holes.RemoveAt(index);
						this._previews.RemoveAt(index);
						this._targets.RemoveAt(index);
						this.CheckCanPlace();
						return true;
					}
				}
				else
				{
					BuildingBlueprint blueprintByPrefabId = Prefabs.Instance.Constructions.GetBlueprintByPrefabId(pi.ClassId);
					FloorHoleArchitect.DestroyTarget destroyTarget;
					if (blueprintByPrefabId != null && this._destroyTargets.TryGetValue(pi.gameObject, out destroyTarget))
					{
						if (destroyTarget._colliders.Contains(other))
						{
							destroyTarget._colliders.Remove(other);
						}
						if (destroyTarget._colliders.Count == 0)
						{
							UnityEngine.Object.Destroy(destroyTarget._ghost);
							this._destroyTargets.Remove(pi.gameObject);
							return true;
						}
						this.CheckCanPlace();
					}
				}
			}
			return false;
		}

		private void CheckNodesforNearbyTargets()
		{
			if (FloorHoleArchitect.Nodes.Count > 0)
			{
				int num = Mathf.Clamp(this._nextNodeId, 0, FloorHoleArchitect.Nodes.Count - 1);
				this._nodeTimer.Reset();
				this._nodeTimer.Start();
				for (int i = 0; i < FloorHoleArchitect.Nodes.Count; i++)
				{
					int num2 = (int)Mathf.Repeat((float)(i + num), (float)FloorHoleArchitect.Nodes.Count);
					Vector3 position = FloorHoleArchitect.Nodes[num2].transform.position;
					if (Vector3.Distance(position, base.transform.position) >= 10f || !this.DetailledNodeCheck(FloorHoleArchitect.Nodes[num2], FloorHoleArchitect.Nodes[num2].transform.parent))
					{
						if (FloorHoleArchitect.Nodes[num2].TargetedBy == this && this.TryRemoveTarget(FloorHoleArchitect.Nodes[num2].transform.parent.GetComponent<PrefabIdentifier>(), null))
						{
							FloorHoleArchitect.Nodes[num2].TargetedBy = null;
						}
						if (this._nodeTimer.ElapsedMilliseconds > 1L)
						{
							this._nodeTimer.Stop();
							this._nextNodeId = (int)Mathf.Repeat((float)(num2 + 1), (float)FloorHoleArchitect.Nodes.Count);
							break;
						}
					}
				}
			}
			else
			{
				this._nextNodeId = 0;
			}
		}

		private bool DetailledNodeCheck(HoleCutterNode node, Transform tr)
		{
			Renderer component = tr.GetComponent<Renderer>();
			if (component && component.bounds.Intersects(base.GetComponent<Collider>().bounds))
			{
				if (node.TargetedBy != this && this.TryAddTarget(node.transform.parent.GetComponent<PrefabIdentifier>(), null))
				{
					node.TargetedBy = this;
				}
				return true;
			}
			IEnumerator enumerator = tr.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform tr2 = (Transform)obj;
					if (this.DetailledNodeCheck(node, tr2))
					{
						return true;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			return false;
		}

		private void CheckCanPlace()
		{
			if (this.preventHole || (this._targets.Count == 0 && this._destroyTargets.Count == 0))
			{
				this._renderer.sharedMaterial = this._offMaterial;
				if (LocalPlayer.Create.BuildingPlacer && !this._isSlave)
				{
					LocalPlayer.Create.BuildingPlacer.Clear = false;
				}
			}
			else
			{
				this._renderer.sharedMaterial = this._onMaterial;
				if (LocalPlayer.Create.BuildingPlacer && !this._isSlave)
				{
					LocalPlayer.Create.BuildingPlacer.Clear = true;
				}
			}
		}

		public Vector2 _holeSize;

		public Material _offMaterial;

		public Material _onMaterial;

		public Material _overlayMaterial;

		public FloorArchitect _previewFloor;

		public RoofArchitect _previewRoof;

		public RaftArchitect _previewRaft;

		public CraneArchitect _previewCrane;

		public Renderer _renderer;

		public bool _isSlave;

		private IList<IHoleStructure> _targets = new List<IHoleStructure>();

		private IList<IHoleStructure> _previews = new List<IHoleStructure>();

		private IList<Hole> _holes = new List<Hole>();

		private IDictionary<GameObject, FloorHoleArchitect.DestroyTarget> _destroyTargets = new Dictionary<GameObject, FloorHoleArchitect.DestroyTarget>();

		private bool preventHole;

		private int _nextNodeId;

		private Stopwatch _nodeTimer = new Stopwatch();

		public static List<HoleCutterNode> Nodes = new List<HoleCutterNode>();

		[Serializable]
		public class DestroyTarget
		{
			public void Clear()
			{
				this._colliders.Clear();
			}

			public GameObject _ghost;

			public GameObject _go;

			public BuildingHealth _bh;

			public List<Collider> _colliders = new List<Collider>();
		}
	}
}
