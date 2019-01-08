using System;
using System.Collections;
using Pathfinding;
using TheForest.Buildings.World;
using TheForest.Utils;
using UnityEngine;

public class gridObjectBlocker : MonoBehaviour
{
	private void Awake()
	{
		this.go = base.gameObject;
	}

	private void Start()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (!Scene.FinishGameLoad)
		{
			base.Invoke("findRootTr", 0.1f);
			this.loadGameNavSetup();
			return;
		}
		if (!this.blockNav)
		{
			if (this.doingOnGameStartCheck)
			{
				return;
			}
			base.Invoke("findRootTr", 0.1f);
			this.StartNavUpdate();
		}
		this.updateGraphDelay = 0.8f;
	}

	private void OnEnable()
	{
		if (this.updateOnEnable)
		{
			if (BoltNetwork.isClient)
			{
				return;
			}
			if (this.planeCollision)
			{
				base.StartCoroutine(this.doPlaneNavCut());
			}
			else
			{
				this.doNavCut();
			}
		}
	}

	private void loadGameNavSetup()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (this.disableColliderForUpdate)
		{
			return;
		}
		this.doingOnGameStartCheck = true;
		this.StartNavUpdate();
	}

	public void doNavCut()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (this.blockNav)
		{
			return;
		}
		if (!AstarPath.active || this.coolDown)
		{
			return;
		}
		this.go = base.gameObject;
		if (this.oneTimeOnly)
		{
			this.blockNav = true;
		}
		if (!this.col)
		{
			this.col = this.go.GetComponent<Collider>();
		}
		if (!this.col && !this.GatherChildColliders)
		{
			return;
		}
		if (base.transform.root.GetComponent<raftOnLand>())
		{
			return;
		}
		Bounds bounds = default(Bounds);
		if (this.GatherChildColliders)
		{
			this.allCol = this.go.GetComponentsInChildren<Collider>();
			if (this.allCol.Length == 0)
			{
				return;
			}
			this.col = this.allCol[0];
			bounds = this.allCol[0].bounds;
			for (int i = 1; i < this.allCol.Length; i++)
			{
				bounds.Encapsulate(this.allCol[i].bounds);
			}
		}
		Bounds bounds2 = this.col.bounds;
		if (this.GatherChildColliders)
		{
			bounds2 = bounds;
		}
		this.diff = bounds2.center.y + bounds2.extents.y;
		bounds2.center.y = this.diff;
		float num = Terrain.activeTerrain.SampleHeight(bounds2.center) + Terrain.activeTerrain.transform.position.y;
		this.diff -= num;
		bool flag = false;
		if (this.diff < 0f)
		{
			flag = true;
		}
		this.storeBounds = bounds2;
		if (this.disableColliderForUpdate)
		{
			this.col.enabled = false;
		}
		FoundationChunkTier component = base.gameObject.GetComponent<FoundationChunkTier>();
		if (component)
		{
			this.isStructure = true;
		}
		if (this.diff < 25f && this.diff > 2f && !this.planeCollision && component)
		{
			if (!Scene.SceneTracker.climbableStructures.Contains(base.gameObject))
			{
				Scene.SceneTracker.climbableStructures.Add(base.gameObject);
			}
			if (Scene.SceneTracker.isActiveAndEnabled)
			{
				Scene.SceneTracker.StartCoroutine(Scene.SceneTracker.validateClimbingWalls(false));
			}
		}
		if (flag)
		{
			base.enabled = false;
			return;
		}
		if (component)
		{
		}
		if (base.gameObject.name.Contains("StairsBuilt"))
		{
			this.isStructure = true;
		}
		if (!base.transform.GetComponent<setupNavRemoveRoot>() && !this.planeCollision)
		{
			base.transform.gameObject.AddComponent<setupNavRemoveRoot>();
		}
		getStructureStrength component2 = this.go.GetComponent<getStructureStrength>();
		if (component2)
		{
			if (component2._type == getStructureStrength.structureType.floor)
			{
				this.isFloor = true;
				this.isStructure = true;
			}
			if (component2._type == getStructureStrength.structureType.wall)
			{
				this.isWall = true;
			}
			if (component2._type == getStructureStrength.structureType.foundation)
			{
				this.isFoundation = true;
			}
			if (component2._type == getStructureStrength.structureType.wall || component2._type == getStructureStrength.structureType.foundation || component2._type == getStructureStrength.structureType.floor || this.isStructure)
			{
				if (this.GatherChildColliders)
				{
					foreach (Collider getCol in this.allCol)
					{
						this.recastObjSetup(this.isFloor, getCol);
					}
				}
				else
				{
					this.recastObjSetup(this.isFloor, null);
				}
				if (base.transform.root)
				{
					if (this.doingOnGameStartCheck)
					{
						Scene.SceneTracker.StartCoroutine(Scene.SceneTracker.doGlobalStructureBoundsNavRemove(base.transform.root, this.storeBounds));
						return;
					}
					if (Scene.SceneTracker.gameObject.activeSelf)
					{
						Scene.SceneTracker.StartCoroutine(Scene.SceneTracker.doStructureBoundsNavRemove(base.transform.root, this.storeBounds, this.updateGraphDelay));
						return;
					}
				}
				return;
			}
		}
		if (this.isStructure && base.transform.root)
		{
			if (this.doingOnGameStartCheck)
			{
				Scene.SceneTracker.StartCoroutine(Scene.SceneTracker.doGlobalStructureBoundsNavRemove(base.transform.root, this.storeBounds));
				return;
			}
			if (Scene.SceneTracker.gameObject.activeSelf)
			{
				Scene.SceneTracker.StartCoroutine(Scene.SceneTracker.doStructureBoundsNavRemove(base.transform.root, this.storeBounds, this.updateGraphDelay));
				return;
			}
		}
		if (!this.planeCollision && !this.cutGo && !this.disableColliderForUpdate)
		{
			this.cutGo = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("navCubeCutter"), base.transform.position, base.transform.rotation);
			if (this.col.GetType() == typeof(BoxCollider))
			{
				BoxCollider component3 = this.col.transform.GetComponent<BoxCollider>();
				Vector3 size = component3.size;
				this.cutGo.transform.parent = this.col.transform;
				this.cutGo.transform.localScale = size;
				this.cutGo.transform.localPosition = component3.center;
				this.cutGo.transform.localRotation = Quaternion.identity;
			}
			else if (this.col.GetType() == typeof(SphereCollider))
			{
				SphereCollider component4 = this.col.transform.GetComponent<SphereCollider>();
				Vector3 localScale = this.cutGo.transform.localScale;
				this.cutGo.transform.parent = this.col.transform;
				localScale.x = component4.radius * 1.3f;
				localScale.y = component4.radius * 1.3f;
				localScale.z = component4.radius * 1.3f;
				this.cutGo.transform.localScale = localScale;
			}
			else if (this.col.GetType() == typeof(CapsuleCollider))
			{
				CapsuleCollider component5 = this.col.transform.GetComponent<CapsuleCollider>();
				this.cutGo.transform.parent = this.col.transform;
				Vector3 localScale2 = this.cutGo.transform.localScale;
				localScale2.x = component5.radius * 1.3f;
				localScale2.y = component5.height;
				localScale2.z = component5.radius * 1.3f;
				this.cutGo.transform.localScale = localScale2;
			}
			this.cutGo.GetComponent<RecastMeshObj>().enabled = true;
		}
		if (!base.transform.GetComponent<navCutterSetup>())
		{
			if (this.doingOnGameStartCheck && !this.planeCollision)
			{
				Scene.SceneTracker.StartCoroutine(Scene.SceneTracker.doGlobalStructureBoundsNavRemove(base.transform.root, this.storeBounds));
				return;
			}
			if (Scene.SceneTracker.gameObject.activeSelf)
			{
				Scene.SceneTracker.StartCoroutine(Scene.SceneTracker.doStructureBoundsNavRemove(base.transform, this.storeBounds, this.updateGraphDelay));
			}
		}
		if (!this.updateOnEnable)
		{
			base.enabled = false;
		}
	}

	private void findRootTr()
	{
		this.storeRootTr = base.transform.root;
	}

	private IEnumerator doDelayedNavCut(int delay)
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		if (this.blockNav)
		{
			yield break;
		}
		if (AstarPath.active && !this.delayedCoolDown)
		{
			this.go = base.gameObject;
			if (this.oneTimeOnly)
			{
				this.blockNav = true;
			}
			Collider col = this.go.GetComponent<Collider>();
			if (!col)
			{
				yield break;
			}
			Bounds b = col.bounds;
			this.diff = b.center.y - b.extents.y;
			float height = Terrain.activeTerrain.SampleHeight(b.center) + Terrain.activeTerrain.transform.position.y;
			this.diff -= height;
			if (this.diff > 7f)
			{
				yield break;
			}
			getStructureStrength structure = this.go.GetComponent<getStructureStrength>();
			bool dolayerHack = false;
			if (this.go.layer == 21 && structure)
			{
				if (structure._type == getStructureStrength.structureType.floor)
				{
					this.go.layer = 26;
					dolayerHack = true;
				}
				else if (structure._type == getStructureStrength.structureType.wall)
				{
					this.go.layer = 20;
					dolayerHack = true;
				}
			}
			yield return new WaitForSeconds((float)delay);
			GraphUpdateObject guo = new GraphUpdateObject(b);
			AstarPath.active.UpdateGraphs(guo);
			this.delayedCoolDown = true;
			base.Invoke("resetDelayedCoolDown", 1f);
			if (dolayerHack)
			{
				base.Invoke("resetLayer", 0.2f);
			}
		}
		yield return null;
		yield break;
	}

	private IEnumerator doPlaneNavCut()
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		if (AstarPath.active && !this.delayedCoolDown)
		{
			this.go = base.gameObject;
			Collider component = this.go.GetComponent<Collider>();
			if (!component)
			{
				yield break;
			}
			Bounds bounds = component.bounds;
			this.diff = bounds.center.y - bounds.extents.y;
			float num = Terrain.activeTerrain.SampleHeight(bounds.center) + Terrain.activeTerrain.transform.position.y;
			this.diff -= num;
			GraphUpdateObject ob = new GraphUpdateObject(bounds);
			AstarPath.active.UpdateGraphs(ob);
			this.delayedCoolDown = true;
			base.Invoke("resetDelayedCoolDown", 1f);
		}
		yield return null;
		yield break;
	}

	private void resetLayer()
	{
		if (this.go)
		{
			this.go.layer = 21;
		}
	}

	private void OnDestroy()
	{
		if (!MenuMain.exitingToMenu)
		{
			base.CancelInvoke("findRootTr");
		}
	}

	private void OnDisable()
	{
		if (!this.ignoreOnDisable)
		{
		}
	}

	private void doRemove()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (this.blockNav)
		{
			return;
		}
		getStructureStrength component = base.transform.GetComponent<getStructureStrength>();
		if (component && (component._type == getStructureStrength.structureType.floor || this.isStructure))
		{
			return;
		}
		if (AstarPath.active && !this.coolDown)
		{
			Terrain activeTerrain = Terrain.activeTerrain;
			if (activeTerrain)
			{
				if (!this.go)
				{
					return;
				}
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.go, this.go.transform.position, this.go.transform.rotation);
				Collider component2 = gameObject.GetComponent<Collider>();
				if (!component2)
				{
					UnityEngine.Object.Destroy(gameObject);
					return;
				}
				Bounds bounds = component2.bounds;
				UnityEngine.Object.Destroy(gameObject);
				GraphUpdateObject ob = new GraphUpdateObject(bounds);
				AstarPath.active.UpdateGraphs(ob, 0f);
				this.coolDown = true;
				base.Invoke("resetCoolDown", 1f);
			}
		}
	}

	private void startDummyNavRemove()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (this.blockNav)
		{
			return;
		}
		if (!AstarPath.active)
		{
			return;
		}
		Terrain activeTerrain = Terrain.activeTerrain;
		if (activeTerrain)
		{
			Collider component = base.GetComponent<Collider>();
			if (!component)
			{
				return;
			}
			if (this.isFloor)
			{
				if (this.storeRootTr)
				{
					Scene.SceneTracker.StartCoroutine(Scene.SceneTracker.doStructureBoundsNavRemove(this.storeRootTr, this.storeBounds, 5.5f));
					return;
				}
				Vector3 extents = this.storeBounds.extents;
				extents.y *= 20f;
				this.storeBounds.extents = extents;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("dummyNavRemove"), base.transform.position, base.transform.rotation);
			gameObject.SendMessage("doDummyNavRemove", this.storeBounds);
			this.blockNav = true;
		}
	}

	private void resetCoolDown()
	{
		this.coolDown = false;
	}

	private void resetDelayedCoolDown()
	{
		this.delayedCoolDown = false;
	}

	private void StartNavUpdate()
	{
		gridObjectBlockerManager.Register(this);
	}

	private void recastObjSetup(bool isFloor, Collider getCol = null)
	{
		Collider collider = this.col.transform.GetComponent<Collider>();
		if (getCol)
		{
			collider = getCol;
		}
		if (collider == null)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("navCubeCutter"), base.transform.position, base.transform.rotation);
		if (collider.GetType() == typeof(BoxCollider))
		{
			BoxCollider boxCollider = collider as BoxCollider;
			Vector3 size = boxCollider.size;
			if (isFloor)
			{
				size.x *= 1.5f;
			}
			gameObject.transform.parent = boxCollider.transform;
			gameObject.transform.localScale = size;
			gameObject.transform.localPosition = boxCollider.center;
			gameObject.transform.localRotation = Quaternion.identity;
			RecastMeshObj component = gameObject.GetComponent<RecastMeshObj>();
			if (isFloor)
			{
				component.area = 0;
			}
			component.enabled = true;
		}
		if (collider.GetType() == typeof(CapsuleCollider))
		{
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			getStructureStrength component2 = collider.GetComponent<getStructureStrength>();
			if (component2 && component2.CoasterStilt)
			{
				Transform child = collider.transform.GetChild(0);
				gameObject.transform.parent = child;
				gameObject.transform.localPosition = new Vector3(0f, 0.329f, 0f);
				gameObject.transform.localEulerAngles = Vector3.zero;
				Vector3 localScale = gameObject.transform.localScale;
				localScale.x = capsuleCollider.height;
				localScale.y = capsuleCollider.radius * 2f;
				localScale.z = capsuleCollider.radius * 2f;
				gameObject.transform.localScale = localScale;
			}
			else
			{
				gameObject.transform.parent = collider.transform;
				Vector3 localScale2 = gameObject.transform.localScale;
				localScale2.x = capsuleCollider.radius * 1.3f;
				localScale2.y = capsuleCollider.height;
				localScale2.z = capsuleCollider.radius * 1.3f;
				gameObject.transform.localScale = localScale2;
			}
			RecastMeshObj component3 = gameObject.GetComponent<RecastMeshObj>();
			if (isFloor)
			{
				component3.area = 0;
			}
			component3.enabled = true;
		}
	}

	public GameObject go;

	private GameObject cutGo;

	public Collider col;

	public Collider[] allCol;

	public bool oneTimeOnly;

	public float delay;

	public bool blockNav;

	public bool ignoreOnDisable;

	public bool updateOnEnable;

	public bool disableColliderForUpdate;

	private bool coolDown;

	private bool delayedCoolDown;

	private float diff;

	public Bounds storeBounds;

	public bool isFloor;

	public bool isStructure;

	public Transform storeRootTr;

	public bool inCave;

	public bool doingOnGameStartCheck;

	public bool GatherChildColliders;

	private bool isWall;

	private bool isFoundation;

	private float updateGraphDelay = 0.8f;

	public bool planeCollision;
}
