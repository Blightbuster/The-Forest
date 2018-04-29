using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Buildings.World;
using TheForest.Items;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[DoNotSerializePublic]
	[AddComponentMenu("Buildings/Creation/Wall Defensive Chunk Reinforcement")]
	public class WallDefensiveChunkReinforcement : EntityBehaviour, IEntityReplicationFilter
	{
		
		bool IEntityReplicationFilter.AllowReplicationTo(BoltConnection connection)
		{
			BoltEntity parentHack = this.GetParentHack();
			return parentHack && connection.ExistsOnRemote(parentHack) == ExistsResult.Yes;
		}

		
		private void Awake()
		{
			base.StartCoroutine(this.DelayedAwake(LevelSerializer.IsDeserializing));
			base.enabled = false;
		}

		
		private IEnumerator DelayedAwake(bool isDeserializing)
		{
			yield return null;
			if ((this._wasBuilt || this._wasPlaced) && ((BoltNetwork.isServer && isDeserializing) || BoltNetwork.isClient))
			{
				while (!this.entity || !this.entity.isAttached)
				{
					yield return null;
				}
				if (BoltNetwork.isClient)
				{
					while (!this.GetParentHack())
					{
						yield return null;
					}
					base.transform.parent = this.GetParentHack().transform;
					this.OnDeserialized();
					yield break;
				}
				this.OnDeserialized();
			}
			if (this._wasBuilt && !isDeserializing)
			{
				this.OnDeserialized();
				this._wallRoot.transform.parent = base.transform;
				if (this._wasBuilt && LocalPlayer.Sfx)
				{
					LocalPlayer.Sfx.PlayBuildingComplete(base.gameObject, false);
				}
			}
			else if (this._craftStructure)
			{
				this._craftStructure.OnBuilt = new Action<GameObject>(this.OnBuilt);
				this._craftStructure._playTwinkle = false;
				if (LocalPlayer.Create)
				{
					LocalPlayer.Create.Grabber.ClosePlace();
				}
			}
			if (isDeserializing || this._wasBuilt || this._wasPlaced)
			{
				base.enabled = false;
			}
			else
			{
				base.enabled = true;
			}
			yield break;
		}

		
		private void Update()
		{
			if (this._chunk && !this._wallRoot)
			{
				LocalPlayer.Create.BuildingPlacer.ForcedParent = this._chunk.gameObject;
				this.CreateStructure(false);
			}
			if (this._wallRoot)
			{
				LocalPlayer.Create.BuildingPlacer.SetClear();
			}
			else
			{
				LocalPlayer.Create.BuildingPlacer.SetNotclear();
			}
			if (TheForest.Utils.Input.GetButtonDown("Build"))
			{
				this._wasPlaced = true;
				base.enabled = false;
			}
			else
			{
				this._caster.CastForAnchors<WallDefensiveChunkArchitect>(new Action<WallDefensiveChunkArchitect>(this.CheckTargetingSupport));
			}
			Scene.HudGui.PlaceIcon.SetActive(this._wallRoot && !this._wasPlaced);
			Scene.HudGui.RotateIcon.SetActive(!this._chunk && !this._wasPlaced);
		}

		
		protected void OnDeserialized()
		{
			if (!this._initialized && (!BoltNetwork.isRunning || (this.entity && this.entity.isAttached)))
			{
				if (!this._chunk)
				{
					this._chunk = base.GetComponentInParent<WallDefensiveChunkArchitect>();
					if (!this._chunk)
					{
						if (!BoltNetwork.isRunning)
						{
							UnityEngine.Object.Destroy(base.gameObject);
						}
						return;
					}
				}
				if (!this._chunk.Reinforcement)
				{
					this._chunk.Reinforcement = this;
				}
				if (!this._chunk.Reinforcement.Equals(this) && !BoltNetwork.isClient)
				{
					if (BoltNetwork.isRunning)
					{
						BoltNetwork.Destroy(base.gameObject);
					}
					else
					{
						UnityEngine.Object.Destroy(base.gameObject);
					}
					return;
				}
				if (BoltNetwork.isServer)
				{
					base.StartCoroutine(this.SetParentHack(this._chunk.GetComponent<BoltEntity>()));
				}
				this._initialized = true;
				if (this._wasBuilt)
				{
					BuildingHealth component = this._chunk.GetComponent<BuildingHealth>();
					component._maxHP *= this._hpBonus;
					this.CreateStructure(false);
					this._wallRoot.transform.parent = base.transform;
				}
				else
				{
					base.transform.position = Vector3.Lerp(this._chunk.P2, this._chunk.P1, 0.5f);
					this.CreateStructure(false);
					base.StartCoroutine(this.OnPlaced());
				}
			}
		}

		
		private void OnDestroy()
		{
			this.Clear();
		}

		
		private void CheckTargetingSupport(WallDefensiveChunkArchitect WDCA)
		{
			if (WDCA)
			{
				if (WDCA.WasBuilt && !WDCA.Reinforcement && !(WDCA is WallDefensiveGateArchitect))
				{
					Quaternion rotation;
					if (Vector3.Dot((WDCA.transform.position - this._caster.LastHit.point).normalized, WDCA.transform.right) < 0f)
					{
						rotation = Quaternion.LookRotation(WDCA.P2 - WDCA.P1);
					}
					else
					{
						rotation = Quaternion.LookRotation(WDCA.P1 - WDCA.P2);
					}
					if (WDCA != this._chunk || Mathf.Abs((rotation.eulerAngles.y + 360f) % 360f - (base.transform.eulerAngles.y + 360f) % 360f) > 3f)
					{
						base.transform.rotation = rotation;
						this._chunk = WDCA;
						this.Clear();
						base.GetComponent<Renderer>().enabled = false;
					}
				}
			}
			else if (this._chunk)
			{
				this._chunk = null;
				this.Clear();
				base.GetComponent<Renderer>().enabled = true;
			}
		}

		
		private IEnumerator OnPlaced()
		{
			this._wasPlaced = true;
			UnityEngine.Object.Destroy(base.GetComponent<Rigidbody>());
			UnityEngine.Object.Destroy(base.GetComponent<Collider>());
			base.enabled = false;
			if (!this._craftStructure)
			{
				this._craftStructure = base.GetComponentInChildren<Craft_Structure>();
			}
			try
			{
				this._craftStructure.GetComponent<Collider>().enabled = false;
			}
			catch (Exception ex)
			{
				Exception exn = ex;
				Debug.LogError(exn);
			}
			yield return null;
			if (this._craftStructure)
			{
				base.transform.parent = this._chunk.transform;
				if (!BoltNetwork.isRunning)
				{
					base.transform.position = Vector3.Lerp(this._chunk.P2, this._chunk.P1, 0.5f);
				}
				Transform ghostRoot = this._wallRoot;
				Transform transform = ghostRoot;
				transform.name += "Ghost";
				Transform perLogGhostPrefab = this._perLogPrefab;
				this._perLogPrefab = Prefabs.Instance.PerLogWDReinforcementBuiltPrefab;
				Transform wallBuiltTr = this.SpawnEdge();
				wallBuiltTr.parent = base.transform;
				this._wallRoot.parent = base.transform;
				this._perLogPrefab = perLogGhostPrefab;
				Craft_Structure.BuildIngredients sri = this._craftStructure._requiredIngredients.FirstOrDefault((Craft_Structure.BuildIngredients i) => i._itemID == this.<>f__this._stickItemId);
				if (sri == null)
				{
					sri = new Craft_Structure.BuildIngredients();
					sri._itemID = this._stickItemId;
					sri._amount = 0;
					sri._renderers = new GameObject[0];
					this._craftStructure._requiredIngredients.Insert(0, sri);
				}
				sri._amount += this.GetStickCost();
				Craft_Structure.BuildIngredients rri = this._craftStructure._requiredIngredients.FirstOrDefault((Craft_Structure.BuildIngredients i) => i._itemID == this.<>f__this._rockItemId);
				if (rri == null)
				{
					rri = new Craft_Structure.BuildIngredients();
					rri._itemID = this._stickItemId;
					rri._amount = 0;
					rri._renderers = new GameObject[0];
					this._craftStructure._requiredIngredients.Insert(0, rri);
				}
				rri._amount += this.GetRockCost();
				List<GameObject> builtStickRenderers = new List<GameObject>(20);
				List<GameObject> builtRockRenderers = new List<GameObject>(20);
				foreach (Renderer r in wallBuiltTr.GetComponentsInChildren<Renderer>())
				{
					if (r.name.Equals("s"))
					{
						builtStickRenderers.Add(r.gameObject);
					}
					else
					{
						builtRockRenderers.Add(r.gameObject);
					}
					r.gameObject.SetActive(false);
				}
				sri._renderers = builtStickRenderers.ToArray();
				rri._renderers = builtRockRenderers.ToArray();
				this._craftStructure.GetComponent<Collider>().enabled = true;
				Collider c = this._craftStructure.GetComponent<Collider>();
				yield return null;
				this._craftStructure.manualLoading = true;
				while (LevelSerializer.IsDeserializing && !this._craftStructure.WasLoaded)
				{
					yield return null;
				}
				this._craftStructure.Initialize();
				this._craftStructure.gameObject.SetActive(true);
				base.GetComponent<Renderer>().enabled = false;
				base.enabled = false;
				yield return null;
				c.enabled = false;
				c.enabled = true;
			}
			yield break;
		}

		
		private void OnBuilt(GameObject built)
		{
			WallDefensiveChunkReinforcement component = built.GetComponent<WallDefensiveChunkReinforcement>();
			component._chunk = this._chunk;
			component._chunk.Reinforcement = component;
			this._chunk.GetComponent<BuildingHealth>().Invoke("Repair", 0.2f);
		}

		
		public void Clear()
		{
			if (this._wallRoot)
			{
				UnityEngine.Object.Destroy(this._wallRoot.gameObject);
				this._wallRoot = null;
			}
		}

		
		protected void CreateStructure(bool isRepair = false)
		{
			if (isRepair)
			{
				this.Clear();
			}
			int num = LayerMask.NameToLayer("Prop");
			this._wallRoot = this.SpawnEdge();
			if (this._wasPlaced || this._wasBuilt)
			{
				this._wallRoot.parent = base.transform;
			}
			if (this._wasBuilt)
			{
				foreach (Renderer renderer in this._wallRoot.gameObject.GetComponentsInChildren<Renderer>())
				{
					if (renderer.name.Equals("s"))
					{
						renderer.transform.rotation *= Quaternion.Euler(UnityEngine.Random.Range(-this._stickRandomFactor, this._stickRandomFactor), UnityEngine.Random.Range(-this._stickRandomFactor, this._stickRandomFactor), UnityEngine.Random.Range(-this._stickRandomFactor, this._stickRandomFactor));
					}
					else
					{
						renderer.transform.rotation *= Quaternion.Euler(UnityEngine.Random.Range(-this._rockRandomFactor, this._rockRandomFactor), UnityEngine.Random.Range(-this._rockRandomFactor, this._rockRandomFactor), UnityEngine.Random.Range(-this._rockRandomFactor, this._rockRandomFactor));
					}
				}
				if (!this._collision)
				{
					Vector3 rhs = this._chunk.P2 - this._chunk.P1;
					this._collision = new GameObject("Collision").transform;
					this._collision.parent = this._wallRoot;
					this._collision.localPosition = Vector3.zero;
					this._collision.localRotation = Quaternion.Euler(0f, 0f, (Vector3.Dot(base.transform.forward, rhs) <= 0f) ? 146f : 34f);
					BoxCollider boxCollider = this._collision.gameObject.AddComponent<BoxCollider>();
					boxCollider.size = new Vector3(3f, 0.5f, Vector3.Distance(this._chunk.P1, this._chunk.P2));
					boxCollider.center = new Vector3(2f, 0f, boxCollider.size.z / 2f - 0.5f);
					this._collision.parent = base.transform;
				}
			}
		}

		
		protected Transform SpawnEdge()
		{
			Transform transform = new GameObject("WallChunk").transform;
			transform.transform.position = this._chunk.P1;
			Vector3 vector = this._chunk.P2 - this._chunk.P1;
			Vector3 a = Vector3.Scale(vector, new Vector3(1f, 0f, 1f));
			Vector3 normalized = a.normalized;
			float y = Mathf.Tan(Vector3.Angle(vector, normalized) * 0.0174532924f) * this._chunk.LogWidth;
			Quaternion rotation = Quaternion.LookRotation(a * (float)((Vector3.Dot(base.transform.forward, vector) <= 0f) ? -1 : 1));
			float num = Vector3.Distance(this._chunk.P1, this._chunk.P2);
			this._logCount = Mathf.RoundToInt(num / this._chunk.LogWidth);
			Vector3 b = normalized * num / (float)this._logCount;
			b.y = y;
			if (vector.y < 0f)
			{
				b.y *= -1f;
			}
			Vector3 vector2 = this._chunk.P1;
			transform.position = this._chunk.P1;
			transform.LookAt(this._chunk.P2);
			for (int i = 0; i < this._logCount; i++)
			{
				Transform transform2 = this.NewLog(vector2, rotation);
				transform2.parent = transform;
				vector2 += b;
			}
			return transform;
		}

		
		private Transform NewLog(Vector3 position, Quaternion rotation)
		{
			return (Transform)UnityEngine.Object.Instantiate(this._perLogPrefab, position, (!this._wasBuilt) ? rotation : this.RandomizeLogRotation(rotation));
		}

		
		private Quaternion RandomizeLogRotation(Quaternion logRot)
		{
			return logRot * Quaternion.Euler(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1.5f, 1.5f), UnityEngine.Random.Range(-1.5f, 1.5f));
		}

		
		private int GetStickCost()
		{
			return this._logCount * this._stickCostPerLog;
		}

		
		private int GetRockCost()
		{
			return this._logCount * this._rockCostPerLog;
		}

		
		
		public Transform BuiltLogPrefab
		{
			get
			{
				return Prefabs.Instance.LogWallDefensiveExBuiltPrefab;
			}
		}

		
		public override void Attached()
		{
			if (BoltNetwork.isServer && !this.GetParentHack() && this._chunk)
			{
				base.StartCoroutine(this.SetParentHack(this._chunk.GetComponent<BoltEntity>()));
			}
		}

		
		private BoltEntity GetParentHack()
		{
			if (!this.entity || !this.entity.isAttached)
			{
				return null;
			}
			if (this.entity.StateIs<IBuildingState>())
			{
				return this.entity.GetState<IBuildingState>().ParentHack;
			}
			if (this.entity.StateIs<IConstructionState>())
			{
				return this.entity.GetState<IConstructionState>().ParentHack;
			}
			throw new Exception("invalid state type");
		}

		
		private IEnumerator SetParentHack(BoltEntity parent)
		{
			while (!this.entity.isAttached || !parent.isAttached)
			{
				yield return null;
			}
			if (this.entity.StateIs<IBuildingState>())
			{
				this.entity.GetState<IBuildingState>().ParentHack = parent;
			}
			else
			{
				if (!this.entity.StateIs<IConstructionState>())
				{
					throw new Exception("invalid state type");
				}
				this.entity.GetState<IConstructionState>().ParentHack = parent;
			}
			yield break;
		}

		
		public SpherecastAnchoring _caster;

		
		[SerializeThis]
		public WallDefensiveChunkArchitect _chunk;

		
		public bool _wasPlaced;

		
		public bool _wasBuilt;

		
		public Craft_Structure _craftStructure;

		
		[ItemIdPicker(Item.Types.Equipment)]
		public int _stickItemId;

		
		public int _stickCostPerLog = 2;

		
		[ItemIdPicker(Item.Types.Equipment)]
		public int _rockItemId;

		
		public int _rockCostPerLog = 3;

		
		public Transform _perLogPrefab;

		
		public float _hpBonus = 1.45f;

		
		public float _stickRandomFactor = 4f;

		
		public float _rockRandomFactor = 20f;

		
		private bool _initialized;

		
		private int _logCount;

		
		private Transform _wallRoot;

		
		private Transform _collision;
	}
}
