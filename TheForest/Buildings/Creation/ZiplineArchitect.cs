using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Items;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[DoNotSerializePublic]
	public class ZiplineArchitect : EntityBehaviour, ICoopTokenConstruction
	{
		
		private void Awake()
		{
			base.StartCoroutine(this.DelayedAwake(LevelSerializer.IsDeserializing));
			base.enabled = false;
		}

		
		private IEnumerator DelayedAwake(bool isDeserializing)
		{
			this._ropePool = new Stack<Transform>();
			this._ropeLength = ((!this._ropePrefab.GetComponent<Renderer>()) ? this._ropeRenderer.bounds.size.z : this._ropePrefab.GetComponent<Renderer>().bounds.size.z);
			yield return null;
			if (this._wasBuilt)
			{
				if (!isDeserializing)
				{
					this.OnDeserialized();
					if (!BoltNetwork.isClient && LocalPlayer.Sfx)
					{
						LocalPlayer.Sfx.PlayBuildingComplete(base.gameObject, true);
					}
				}
			}
			else
			{
				this._wasPlaced = (!LocalPlayer.Create || base.gameObject != LocalPlayer.Create.CurrentGhost);
				this._ropeMat = ((!this._wasPlaced) ? new Material(this._ropeRenderer.sharedMaterial) : this._ropeRenderer.sharedMaterial);
				this._gate1.GetComponent<Renderer>().sharedMaterial = this._ropeMat;
				this._gate2.GetComponent<Renderer>().sharedMaterial = this._ropeMat;
				this._craftStructure.OnBuilt = new Action<GameObject>(this.OnBuilt);
				this._craftStructure._playTwinkle = false;
				if (!this._wasPlaced)
				{
					if (!CoopPeerStarter.DedicatedHost)
					{
						while (!LocalPlayer.Create)
						{
							yield return null;
						}
						if (LocalPlayer.Create.BuildingPlacer)
						{
							LocalPlayer.Create.Grabber.ClosePlace();
						}
					}
				}
				else if (this._craftStructure)
				{
					this.OnDeserialized();
				}
			}
			if (isDeserializing || this._wasPlaced || this._wasBuilt)
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
			bool flag = !this._gate1.transform.parent;
			bool flag2 = !this._gate2.transform.parent;
			if (flag)
			{
				if (!flag2)
				{
					Vector3 gate2RopePosition = this.Gate2RopePosition;
					if (Vector3.Distance(this.Gate1RopePosition, gate2RopePosition) > 1f)
					{
						Transform ziplineRoot = this.CreateZipline(this.Gate1RopePosition, gate2RopePosition);
						if (this._ziplineRoot)
						{
							UnityEngine.Object.Destroy(this._ziplineRoot.gameObject);
						}
						this._ziplineRoot = ziplineRoot;
						if (!this._gate2.activeSelf)
						{
							this._gate2.SetActive(true);
						}
						this._gate1.transform.LookAt(new Vector3(this._gate2.transform.position.x, this._gate1.transform.position.y, this._gate2.transform.position.z));
						this._gate2.transform.LookAt(this.Gate2LookAtTarget);
					}
					else if (this._gate2.activeSelf)
					{
						this._gate2.SetActive(false);
					}
				}
			}
			else if (this._ziplineRoot)
			{
				UnityEngine.Object.Destroy(this._ziplineRoot.gameObject);
				this._ziplineRoot = null;
				this._ropePool.Clear();
			}
			bool flag3 = this.CheckLockGate();
			this.CheckUnlockGate();
			bool flag4 = flag2;
			if (LocalPlayer.Create.BuildingPlacer.Clear != flag4 || Scene.HudGui.RotateIcon.activeSelf == flag)
			{
				Scene.HudGui.RotateIcon.SetActive(!flag);
				LocalPlayer.Create.BuildingPlacer.Clear = flag4;
			}
			this._ropeMat.SetColor("_TintColor", (!flag3 && !flag4) ? LocalPlayer.Create.BuildingPlacer.RedMat.GetColor("_TintColor") : LocalPlayer.Create.BuildingPlacer.ClearMat.GetColor("_TintColor"));
			bool showManualfillLockIcon = !flag && flag3;
			bool canLock = flag && flag3;
			bool canUnlock = flag;
			Scene.HudGui.RoofConstructionIcons.Show(showManualfillLockIcon, false, false, flag4, canLock, canUnlock, false);
		}

		
		private void OnDestroy()
		{
			if (this._ziplineRoot)
			{
				UnityEngine.Object.Destroy(this._ziplineRoot.gameObject);
			}
			if (!this._wasPlaced && LocalPlayer.Create)
			{
				LocalPlayer.Create.Grabber.ClosePlace();
				if (Scene.HudGui)
				{
					Scene.HudGui.RoofConstructionIcons.Shutdown();
				}
				if (LocalPlayer.Create && this._ropeMat == this._ropeRenderer.sharedMaterial)
				{
					this._ropeMat.SetColor("_TintColor", LocalPlayer.Create.BuildingPlacer.ClearMat.GetColor("_TintColor"));
				}
			}
			if (this._gate1 && !this._gate1.transform.parent)
			{
				UnityEngine.Object.Destroy(this._gate1.gameObject);
			}
			if (this._gate2 && !this._gate2.transform.parent)
			{
				UnityEngine.Object.Destroy(this._gate2.gameObject);
			}
		}

		
		private void OnDeserialized()
		{
			if (!this._initialized)
			{
				if (!this._gate1 || !this._gate2)
				{
					this._initialized = true;
					UnityEngine.Object.Destroy(base.gameObject);
				}
				else if (this._wasBuilt)
				{
					this._initialized = true;
					this.CreateStructure(false);
					FoundationArchitect.CreateWindSFX(base.transform);
					this._ropePool = null;
				}
				else if (this._wasPlaced)
				{
					this._initialized = true;
					this._gate2.SetActive(true);
					this.CreateStructure(false);
					this._ropePool = new Stack<Transform>();
					base.StartCoroutine(this.OnPlaced(true));
				}
			}
		}

		
		protected virtual void OnBeginCollapse()
		{
			if (!BoltNetwork.isClient)
			{
				if (this._wasBuilt)
				{
					base.SendMessage("Collapse", base.transform.position, SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					this._craftStructure.CancelBlueprint();
				}
				UnityEngine.Object.DestroyImmediate(this);
			}
		}

		
		private IEnumerator OnPlaced(bool readyMp = false)
		{
			Debug.Log("Zipline - OnPlaced()");
			bool loadedSave = this._wasPlaced;
			this._wasPlaced = true;
			base.enabled = false;
			this._craftStructure.GetComponent<Collider>().enabled = false;
			Scene.HudGui.RoofConstructionIcons.Shutdown();
			this._ziplineRoot.transform.parent = null;
			yield return null;
			if (this._craftStructure)
			{
				UnityEngine.Object.Destroy(base.GetComponent<Rigidbody>());
				UnityEngine.Object.Destroy(base.GetComponent<Collider>());
				UnityEngine.Object.Destroy(base.GetComponent<Renderer>());
				Transform ghostRoot = this._ziplineRoot;
				this._ropePool.Clear();
				Transform logGhostPrefab = this._ropePrefab;
				this._ropePrefab = Prefabs.Instance.ZiplineRopeBuiltPrefab;
				this._ziplineRoot = this.CreateZipline(this.Gate1RopePosition, this.Gate2RopePosition);
				this._ziplineRoot.name = "ZiplineRootBuilt";
				this._ropePool = null;
				int totalLogs = this._ziplineRoot.childCount;
				Craft_Structure.BuildIngredients ri = this._craftStructure._requiredIngredients.FirstOrDefault((Craft_Structure.BuildIngredients i) => i._itemID == this.<>f__this._ropeItemId);
				if (ri == null)
				{
					ri = new Craft_Structure.BuildIngredients();
					ri._itemID = this._ropeItemId;
					ri._amount = 0;
					ri._renderers = new GameObject[0];
					this._craftStructure._requiredIngredients.Insert(0, ri);
				}
				ri._amount += totalLogs;
				List<GameObject> logStacks = new List<GameObject>();
				foreach (object obj in this._ziplineRoot)
				{
					Transform logStack = (Transform)obj;
					logStack.gameObject.SetActive(false);
					logStacks.Add(logStack.gameObject);
				}
				ri._amount += totalLogs;
				ri._renderers = logStacks.AsEnumerable<GameObject>().Reverse<GameObject>().Union(ri._renderers).ToArray<GameObject>();
				foreach (object obj2 in this._ziplineRoot)
				{
					Transform log = (Transform)obj2;
					log.gameObject.SetActive(false);
				}
				this._ropePrefab = logGhostPrefab;
				ghostRoot.transform.parent = null;
				this._gate1.transform.parent = null;
				this._gate2.transform.parent = null;
				base.transform.position = this._gate1.transform.position;
				base.transform.LookAt(this._gate2.transform.position);
				this._gate1.transform.parent = base.transform;
				this._gate2.transform.parent = base.transform;
				this._ziplineRoot.transform.parent = base.transform;
				ghostRoot.transform.parent = base.transform;
				BoxCollider bc;
				if (this._craftStructure.GetComponent<Collider>() is BoxCollider)
				{
					bc = (BoxCollider)this._craftStructure.GetComponent<Collider>();
				}
				else
				{
					bc = this._craftStructure.gameObject.AddComponent<BoxCollider>();
					bc.isTrigger = true;
				}
				bc.size = new Vector3(4.5f, 30f, Vector3.Distance(this._gate1.transform.position, this._gate2.transform.position));
				bc.center = new Vector3(0f, 0f, bc.size.z / 2f);
				bc.enabled = true;
				if (!loadedSave)
				{
					this.EnsureGatesDestroyProxies(false);
				}
				yield return null;
				this._craftStructure.manualLoading = true;
				while (LevelSerializer.IsDeserializing && !this._craftStructure.WasLoaded)
				{
					yield return null;
				}
				this._craftStructure.Initialize();
				this._craftStructure.gameObject.SetActive(true);
				yield return null;
				bc.enabled = false;
				bc.enabled = true;
			}
			yield break;
		}

		
		private Transform FindGateParent(Transform gate)
		{
			RaycastHit raycastHit;
			if (Physics.SphereCast(gate.position + Vector3.down * 4f, 1f, Vector3.down, out raycastHit, 3f, LayerMask.GetMask(new string[]
			{
				"Prop"
			})))
			{
				PrefabIdentifier componentInParent = raycastHit.collider.GetComponentInParent<PrefabIdentifier>();
				if (componentInParent)
				{
					return componentInParent.transform;
				}
			}
			return null;
		}

		
		protected virtual void OnBuilt(GameObject built)
		{
			ZiplineArchitect component = built.GetComponent<ZiplineArchitect>();
			component._gate1.transform.position = this._gate1.transform.position;
			component._gate1.transform.rotation = this._gate1.transform.rotation;
			component._gate2.transform.position = this._gate2.transform.position;
			component._gate2.transform.rotation = this._gate2.transform.rotation;
			component.EnsureGatesDestroyProxies(true);
			component._wasBuilt = true;
		}

		
		protected void EnsureGatesDestroyProxies(bool canRemoveOld)
		{
			Transform transform = this.FindGateParent(this._gate1.transform);
			if (transform)
			{
				if (canRemoveOld)
				{
					foreach (OnDestroyProxyS onDestroyProxyS in transform.GetComponentsInChildren<OnDestroyProxyS>())
					{
						if (!onDestroyProxyS._todo || onDestroyProxyS._todo == base.gameObject)
						{
							UnityEngine.Object.Destroy(onDestroyProxyS.gameObject);
							break;
						}
					}
				}
				OnDestroyProxyS onDestroyProxyS2 = UnityEngine.Object.Instantiate<OnDestroyProxyS>(Prefabs.Instance.DestroyProxy);
				onDestroyProxyS2.transform.parent = transform.transform;
				onDestroyProxyS2.transform.position = this._gate1.transform.position;
				onDestroyProxyS2._todo = base.gameObject;
			}
			if (!base.transform.parent)
			{
				Transform transform2 = this.FindGateParent(this._gate2.transform);
				if (transform2)
				{
					if (canRemoveOld)
					{
						foreach (OnDestroyProxyS onDestroyProxyS3 in transform2.GetComponentsInChildren<OnDestroyProxyS>())
						{
							if (onDestroyProxyS3._todo == base.gameObject)
							{
								UnityEngine.Object.Destroy(onDestroyProxyS3.gameObject);
								break;
							}
						}
					}
					OnDestroyProxyS onDestroyProxyS4 = UnityEngine.Object.Instantiate<OnDestroyProxyS>(Prefabs.Instance.DestroyProxy);
					onDestroyProxyS4.transform.parent = transform2.transform;
					onDestroyProxyS4.transform.position = this._gate2.transform.position;
					onDestroyProxyS4._todo = base.gameObject;
				}
			}
		}

		
		private void RespawningRenderersFrom(GameObject respawned)
		{
			ZiplineArchitect component = respawned.GetComponent<ZiplineArchitect>();
			component._gate1.transform.parent = this._gate1.transform.parent;
			component._gate1.transform.position = this._gate1.transform.position;
			component._gate1.transform.rotation = this._gate1.transform.rotation;
			component._gate2.transform.parent = this._gate2.transform.parent;
			component._gate2.transform.position = this._gate2.transform.position;
			component._gate2.transform.rotation = this._gate2.transform.rotation;
			UnityEngine.Object.Destroy(this._gate1.gameObject);
			UnityEngine.Object.Destroy(this._gate2.gameObject);
			this._gate1 = component._gate1;
			this._gate2 = component._gate2;
		}

		
		public void CreateStructure(bool isRepair = false)
		{
			if (this._wasBuilt && isRepair)
			{
				if (this._ziplineRoot)
				{
					UnityEngine.Object.Destroy(this._ziplineRoot.gameObject);
				}
				this._ziplineRoot = null;
				base.StartCoroutine(this.DelayedAwake(true));
			}
			this._gate1.transform.LookAt(new Vector3(this._gate2.transform.position.x, this._gate1.transform.position.y, this._gate2.transform.position.z));
			this._gate2.transform.LookAt(new Vector3(this._gate1.transform.position.x, this._gate2.transform.position.y, this._gate1.transform.position.z));
			GameObject gameObject = (!this._ziplineRoot) ? null : this._ziplineRoot.gameObject;
			this._ziplineRoot = this.CreateZipline(this.Gate1RopePosition, this.Gate2RopePosition);
			this._ziplineRoot.name = "ZiplineRoot" + ((!this._wasBuilt) ? "Ghost" : "Built");
			this._ziplineRoot.parent = base.transform;
			if (gameObject)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			if (this._wasBuilt)
			{
				if (this.Gate1RopePosition.y > this._gate2.transform.position.y)
				{
					this._enterTrigger.position = this.Gate1RopePosition;
					this._enterTrigger.LookAt(this.Gate2RopePosition);
					this._exitTrigger.position = this.Gate2RopePosition;
					this._exitTrigger.LookAt(this.Gate1RopePosition);
				}
				else
				{
					this._enterTrigger.position = this.Gate2RopePosition;
					this._enterTrigger.LookAt(this.Gate1RopePosition);
					this._exitTrigger.position = this.Gate1RopePosition;
					this._exitTrigger.LookAt(this.Gate2RopePosition);
				}
			}
			if (this._wasBuilt && isRepair)
			{
				this._ziplineRoot.parent = base.transform;
			}
		}

		
		protected virtual bool CheckLockGate()
		{
			bool flag = this._gate2.transform.parent && LocalPlayer.Create.BuildingPlacer.ClearOfCollision;
			if (flag)
			{
				bool flag2 = !this._gate1.transform.parent;
				if (flag2)
				{
					float num = Vector3.Distance(this._gate1.transform.position, this._gate2.transform.position);
					if (num < 8f || num > 470f)
					{
						return false;
					}
					RaycastHit raycastHit;
					if (Physics.SphereCast(this.Gate1RopePosition + Vector3.down, 1.5f, this.Gate2RopePosition - this.Gate1RopePosition, out raycastHit, num, LocalPlayer.Create.BuildingPlacer.FloorLayers | 1 << LayerMask.NameToLayer("treeMid")))
					{
						return false;
					}
				}
				if (TheForest.Utils.Input.GetButtonDown("Fire1"))
				{
					if (!flag2)
					{
						this._gate1.transform.parent = null;
					}
					else
					{
						this._gate2.transform.parent = null;
						if (this._ziplineRoot)
						{
							UnityEngine.Object.Destroy(this._ziplineRoot.gameObject);
						}
						this._ziplineRoot = this.CreateZipline(this.Gate1RopePosition, this.Gate2RopePosition);
					}
				}
			}
			return flag;
		}

		
		protected virtual void CheckUnlockGate()
		{
			if (TheForest.Utils.Input.GetButtonDown("AltFire"))
			{
				if (!this._gate2.transform.parent)
				{
					this._gate2.transform.parent = base.transform;
					this._gate2.transform.localPosition = Vector3.zero;
					this._gate2.transform.localRotation = Quaternion.identity;
				}
				else if (!this._gate1.transform.parent)
				{
					this._gate1.transform.parent = base.transform;
					this._gate1.transform.localPosition = Vector3.zero;
					this._gate1.transform.localRotation = Quaternion.identity;
					if (this._gate2.activeSelf)
					{
						this._gate2.SetActive(false);
					}
				}
			}
		}

		
		protected Transform CreateZipline(Vector3 p1, Vector3 p2)
		{
			float num = 1f;
			Transform parent = null;
			if (p1.y < p2.y)
			{
				Vector3 vector = p1;
				p1 = p2;
				p2 = vector;
			}
			Transform transform = new GameObject("ZiplineRoot").transform;
			transform.position = p1;
			Vector3 b = p2 - p1;
			float magnitude = b.magnitude;
			int num2 = Mathf.CeilToInt(magnitude / this._ropeLength);
			Vector3 localScale = new Vector3(1f, 1f, magnitude / (float)num2 / this._ropeLength);
			b = b.normalized * (this._ropeLength * localScale.z);
			Stack<Transform> stack = new Stack<Transform>();
			Quaternion rotation = Quaternion.LookRotation(p2 - p1);
			for (int i = 0; i < num2; i++)
			{
				Transform transform2;
				if (!this._wasBuilt)
				{
					transform2 = this.NewRope(p1, rotation);
					num += this._ropeCost;
					if (num >= 1f)
					{
						num -= 1f;
						parent = transform2;
						transform2.parent = transform;
						transform2.localScale = localScale;
					}
					else
					{
						transform2.parent = parent;
						transform2.localScale = Vector3.one;
					}
				}
				else
				{
					transform2 = this.NewRope(p1, rotation);
					transform2.parent = transform;
					transform2.localScale = localScale;
				}
				stack.Push(transform2);
				p1 += b;
			}
			if (!this._wasPlaced && !this._wasBuilt)
			{
				this._ropePool = stack;
			}
			return transform;
		}

		
		private Transform NewRope(Vector3 position, Quaternion rotation)
		{
			if (this._ropePool.Count > 0)
			{
				Transform transform = this._ropePool.Pop();
				transform.position = position;
				transform.rotation = rotation;
				return transform;
			}
			Transform transform2 = (Transform)UnityEngine.Object.Instantiate(this._ropePrefab, position, rotation);
			if (!this._wasBuilt && !this._wasPlaced)
			{
				transform2.GetComponentInChildren<Renderer>().sharedMaterial = this._ropeMat;
			}
			return transform2;
		}

		
		
		protected virtual Vector3 Gate1RopePosition
		{
			get
			{
				return this._gate1.transform.position;
			}
		}

		
		
		protected virtual Vector3 Gate2RopePosition
		{
			get
			{
				return this._gate2.transform.position;
			}
		}

		
		
		protected virtual Vector3 Gate2LookAtTarget
		{
			get
			{
				return new Vector3(this._gate1.transform.position.x, this._gate2.transform.position.y, this._gate1.transform.position.z);
			}
		}

		
		public override void Attached()
		{
			this.CustomToken = this.entity.attachToken;
		}

		
		
		
		public virtual IProtocolToken CustomToken
		{
			get
			{
				return new CoopZiplineToken
				{
					p1 = this._gate1.transform.position,
					p2 = this._gate2.transform.position
				};
			}
			set
			{
				CoopZiplineToken coopZiplineToken = (CoopZiplineToken)value;
				this._gate1.transform.position = coopZiplineToken.p1;
				this._gate2.transform.position = coopZiplineToken.p2;
				if (!this._wasBuilt)
				{
					if (this.entity.isOwner)
					{
						this.EnsureGatesDestroyProxies(true);
					}
					this._wasPlaced = true;
				}
			}
		}

		
		[SerializeThis]
		public bool _wasPlaced;

		
		[SerializeThis]
		public bool _wasBuilt;

		
		public GameObject _gate1;

		
		public GameObject _gate2;

		
		public Transform _ropePrefab;

		
		public Renderer _ropeRenderer;

		
		public float _verticalBend = 0.001f;

		
		public Transform _enterTrigger;

		
		public Transform _exitTrigger;

		
		[Range(0f, 1f)]
		public float _ropeCost = 0.5f;

		
		public Craft_Structure _craftStructure;

		
		[ItemIdPicker]
		public int _ropeItemId;

		
		private bool _initialized;

		
		protected Transform _ziplineRoot;

		
		private float _ropeLength;

		
		private Stack<Transform> _ropePool;

		
		private Material _ropeMat;
	}
}
