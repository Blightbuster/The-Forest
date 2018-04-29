using System;
using System.Collections;
using Bolt;
using PathologicalGames;
using TheForest.Commons.Delegates;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	
	[AddComponentMenu("Items/World/PickUp")]
	public class PickUp : MonoBehaviour
	{
		
		protected virtual void Awake()
		{
			if (this._mpOnly && !BoltNetwork.isRunning)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			base.enabled = false;
			this.ResetIcons();
			if (BoltNetwork.isRunning)
			{
				this._enableTime = Time.time + this._enableDelayMp;
			}
			if (this._destroyIfAlreadyOwned || this._hideIfAlreadyOwned || this._positionHashSaving)
			{
				this.OwnershipCheckRegistration();
			}
		}

		
		private void OnTriggerEnter(Collider other)
		{
			if (other.transform.root.CompareTag("Player") && ((other.transform.CompareTag("Player") && this._bodyAutoCollect) || this._grabberAutoCollect))
			{
				this.Collect();
			}
		}

		
		private void Update()
		{
			if (this._useSkinAnimalRoutine)
			{
				if ((LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2).tagHash != this.idleHash || (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1).tagHash != this.idleHash && LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1).tagHash != this.heldHash)) && !this.startedSkinning && !LocalPlayer.AnimControl.onRope && !LocalPlayer.AnimControl.onRaft)
				{
					this._myPickUp.SetActive(false);
					return;
				}
				if (!this.startedSkinning)
				{
					this._myPickUp.SetActive(true);
				}
			}
			if (this._enableTime < Time.time && ((this._inputMethod == PickUp.InputMethods.ButtonDown && TheForest.Utils.Input.GetButtonDown("Take")) || (this._inputMethod == PickUp.InputMethods.ButtonUp && TheForest.Utils.Input.GetButtonUp("Take")) || (this._inputMethod == PickUp.InputMethods.ButtonHold && TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, false)) || (this._inputMethod == PickUp.InputMethods.ButtonPress && TheForest.Utils.Input.GetButtonPress("Take"))))
			{
				if (this._useSkinAnimalRoutine)
				{
					base.StartCoroutine(this.pickupSkinAnimalRoutine());
					this.startedSkinning = true;
				}
				else
				{
					this.Collect();
				}
			}
		}

		
		private void OnDisabled()
		{
			if (this._destroyTarget && this._destroyTarget.transform.parent && !this._destroyTarget.transform.parent.gameObject.activeInHierarchy && !this._destroyTarget.activeSelf)
			{
				this._destroyTarget.SetActive(true);
				this.OnSpawned();
			}
		}

		
		private void OnDestroy()
		{
			this.OwnershipCheckUnregister();
		}

		
		private void OnSpawned()
		{
			this.Used = false;
			base.enabled = false;
			if (this._pendingdestroythroughbolt)
			{
				if (this._destroyTarget)
				{
					this.ToggleRenderers(this._destroyTarget.transform, true);
				}
				this._pendingdestroythroughbolt = false;
			}
			this.ResetIcons();
		}

		
		public virtual void GrabEnter()
		{
			this.ToggleIcons(true);
			base.enabled = true;
		}

		
		public virtual void GrabExit()
		{
			this.ToggleIcons(false);
			base.enabled = false;
		}

		
		public void ToggleIcons(bool pickup)
		{
			if (this._sheen && this._sheen.activeSelf != !pickup)
			{
				this._sheen.SetActive(!pickup);
			}
			if (this._myPickUp && this._myPickUp.activeSelf != pickup)
			{
				this._myPickUp.SetActive(pickup);
			}
		}

		
		private void ResetIcons()
		{
			if (this._sheen)
			{
				this._sheen.SetActive(true);
			}
			if (this._myPickUp)
			{
				this._myPickUp.SetActive(false);
			}
		}

		
		private void OwnershipCheckRegistration()
		{
			this._wsToken = WorkScheduler.Register(new WsTask(this.OwnershipCheck), base.transform.position, true);
		}

		
		private void OwnershipCheckUnregister()
		{
			if (this._wsToken >= 0)
			{
				WorkScheduler.Unregister(new WsTask(this.OwnershipCheck), this._wsToken);
				this._wsToken = -1;
			}
		}

		
		protected virtual void OwnershipCheck()
		{
			if (LocalPlayer.Inventory)
			{
				if (this._positionHashSaving)
				{
					if (GlobalDataSaver.GetInt(base.transform.ToGeoHash(), 0) == 1)
					{
						this.ClearOut(false);
					}
					else
					{
						this.OwnershipCheckUnregister();
					}
				}
				else if (LocalPlayer.Inventory.Owns(this._itemId, true))
				{
					if (this._destroyIfAlreadyOwned && !BoltNetwork.isRunning)
					{
						this.ClearOut(false);
					}
					else if (this._destroyTarget.activeSelf)
					{
						this._destroyTarget.SetActive(false);
					}
				}
				else if (!this._destroyTarget.activeSelf)
				{
					this._destroyTarget.SetActive(true);
				}
			}
		}

		
		private bool DestroyThroughBolt(bool fakeDrop)
		{
			if (BoltNetwork.isRunning)
			{
				BoltEntity boltEntity = (!this._destroyTarget) ? base.GetComponentInParent<BoltEntity>() : this._destroyTarget.GetComponent<BoltEntity>();
				if (boltEntity && !this._destroyTarget && !boltEntity.StateIs<IAnimalState>())
				{
					boltEntity = null;
				}
				if (boltEntity && boltEntity.isAttached && !boltEntity.StateIs<IPlayerState>() && !boltEntity.StateIs<IMutantState>())
				{
					if (this._disableInsteadOfDestroy)
					{
						DisablePickup disablePickup;
						if (boltEntity.source == null)
						{
							disablePickup = DisablePickup.Create(GlobalTargets.OnlySelf);
						}
						else
						{
							disablePickup = DisablePickup.Create(boltEntity.source);
						}
						disablePickup.Entity = boltEntity;
						disablePickup.Num = this._destroyTarget.transform.GetSiblingIndex();
						disablePickup.Send();
						this._pendingdestroythroughbolt = true;
						return false;
					}
					DestroyPickUp destroyPickUp;
					if (boltEntity.source == null)
					{
						destroyPickUp = DestroyPickUp.Create(GlobalTargets.OnlySelf);
					}
					else
					{
						destroyPickUp = DestroyPickUp.Create(boltEntity.source);
					}
					destroyPickUp.PickUpPlayer = LocalPlayer.Entity;
					destroyPickUp.PickUpEntity = boltEntity;
					destroyPickUp.ItemId = this._itemId;
					destroyPickUp.SibblingId = ((!this._mpIsFlareFromCrate) ? -1 : this._destroyTarget.transform.GetSiblingIndex());
					destroyPickUp.FakeDrop = (fakeDrop && !this._spawnAfterPickupPrefab);
					destroyPickUp.Send();
					if (this._destroyTarget)
					{
						this.ToggleRenderers(this._destroyTarget.transform, false);
					}
					this._pendingdestroythroughbolt = true;
					return true;
				}
			}
			return false;
		}

		
		private void Collect()
		{
			if (this._pendingdestroythroughbolt)
			{
				return;
			}
			if (this._fakeArrowPickup)
			{
				base.transform.parent.SendMessage("sendRemoveArrow", SendMessageOptions.DontRequireReceiver);
			}
			if (this._unparentArrows)
			{
				CoopWeaponArrowFire componentInChildren = base.transform.parent.GetComponentInChildren<CoopWeaponArrowFire>();
				if (componentInChildren)
				{
					componentInChildren.transform.parent = null;
				}
			}
			if (this._firstTimePickup)
			{
				if (LocalPlayer.Animator)
				{
					LocalPlayer.Animator.SetBool("firstTimePickup", true);
				}
			}
			else if (LocalPlayer.Animator)
			{
				LocalPlayer.Animator.SetBool("firstTimePickup", false);
			}
			if (base.transform.parent)
			{
				artifactBallPlacedController componentInParent = base.transform.GetComponentInParent<artifactBallPlacedController>();
				if (componentInParent)
				{
					componentInParent.setHeldArtifactState();
				}
			}
			if (this._hairSprayFuel && LocalPlayer.Stats.hairSprayFuel.CurrentFuel < LocalPlayer.Stats.hairSprayFuel.MaxFuelCapacity)
			{
				LocalPlayer.Stats.hairSprayFuel.CurrentFuel = LocalPlayer.Stats.hairSprayFuel.MaxFuelCapacity;
			}
			this.CheckTrappedAnimal();
			if (this.MainEffect() || this._destroyIfFull)
			{
				if (this._positionHashSaving)
				{
					GlobalDataSaver.SetInt(base.transform.ToGeoHash(), 1);
				}
				this.CollectBonuses();
				if (this._spiderDice && UnityEngine.Random.Range(0, 4) == 2)
				{
					UnityEngine.Object.Instantiate(Resources.Load("Spider"), base.transform.position, base.transform.rotation);
				}
				if (this._spawnAfterPickupPrefab)
				{
					Vector3 vector = base.transform.position;
					if (this._useSkinAnimalRoutine)
					{
						vector += LocalPlayer.Transform.forward * 1.1f;
					}
					if (!BoltNetwork.isRunning || !this._spawnAfterPickupPrefab.GetComponent<BoltEntity>())
					{
						UnityEngine.Object.Instantiate<GameObject>(this._spawnAfterPickupPrefab, vector, Quaternion.identity);
					}
					else
					{
						BoltNetwork.Instantiate(this._spawnAfterPickupPrefab, vector, Quaternion.identity);
					}
					if (this._spawnAfterPickupPrefab2)
					{
						if (!BoltNetwork.isRunning || !this._spawnAfterPickupPrefab2.GetComponent<BoltEntity>())
						{
							UnityEngine.Object.Instantiate<GameObject>(this._spawnAfterPickupPrefab2, vector, Quaternion.identity);
						}
						else
						{
							BoltNetwork.Instantiate(this._spawnAfterPickupPrefab2, vector, Quaternion.identity);
						}
					}
				}
				if (!this.TryPool() && !this._infinite)
				{
					this.ClearOut(false);
				}
			}
			else if (!this._preventFakeDrop)
			{
				if (this._positionHashSaving)
				{
					GlobalDataSaver.SetInt(base.transform.ToGeoHash(), 1);
				}
				if (this._spawnAfterPickupPrefab)
				{
					Vector3 vector2 = base.transform.position;
					if (this._useSkinAnimalRoutine)
					{
						vector2 += LocalPlayer.Transform.forward * 1.1f;
					}
					if (!BoltNetwork.isRunning || !this._spawnAfterPickupPrefab.GetComponent<BoltEntity>())
					{
						UnityEngine.Object.Instantiate<GameObject>(this._spawnAfterPickupPrefab, vector2, Quaternion.identity);
					}
					else
					{
						BoltNetwork.Instantiate(this._spawnAfterPickupPrefab, vector2, Quaternion.identity);
					}
					if (this._spawnAfterPickupPrefab2)
					{
						if (!BoltNetwork.isRunning || !this._spawnAfterPickupPrefab2.GetComponent<BoltEntity>())
						{
							UnityEngine.Object.Instantiate<GameObject>(this._spawnAfterPickupPrefab2, vector2, Quaternion.identity);
						}
						else
						{
							BoltNetwork.Instantiate(this._spawnAfterPickupPrefab2, vector2, Quaternion.identity);
						}
					}
				}
				this.ClearOut(true);
			}
		}

		
		public void CheckTrappedAnimal()
		{
			animalHealth component = base.transform.root.GetComponent<animalHealth>();
			if (component != null)
			{
				if (component.Trapped && component.Trap != null)
				{
					component.Trap.SendMessageUpwards("setAnimalAsDead", SendMessageOptions.DontRequireReceiver);
					component.Trap = null;
					component.Trapped = false;
				}
			}
			else if (this._destroyTarget && this._destroyTarget.GetComponent<Fish>() && this._destroyTarget.transform.parent)
			{
				this._destroyTarget.transform.parent.SendMessage("removeFishFromTrap", SendMessageOptions.DontRequireReceiver);
			}
		}

		
		protected virtual bool MainEffect()
		{
			bool flag = LocalPlayer.Inventory.HasOwned(this._itemId);
			if (LocalPlayer.Inventory.AddItem(this._itemId, this._amount, this._preventAutoEquip, false, this._properties))
			{
				if (this._forceAutoEquip)
				{
					LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.RightHand);
					LocalPlayer.Inventory.Equip(this._itemId, false);
				}
				else if (!flag)
				{
					LocalPlayer.Inventory.SheenItem(this._itemId, this._properties, true);
				}
				return true;
			}
			return false;
		}

		
		private void CollectBonuses()
		{
			if (this._bonusItems != null && this._bonusItems.Length > 0)
			{
				foreach (PickUp.LootTuple lootTuple in this._bonusItems)
				{
					if (lootTuple._itemId > 0)
					{
						LocalPlayer.Inventory.AddItem(lootTuple._itemId, lootTuple._amount, this._preventAutoEquip, false, null);
					}
				}
			}
		}

		
		public void ClearOut(bool fakeDrop)
		{
			if ((!fakeDrop || !this.DoFakeDrop()) && !this.DestroyThroughBolt(fakeDrop))
			{
				if (!this._disableInsteadOfDestroy)
				{
					this._destroyTarget.transform.parent = null;
					if (!this.TryPool() && !this._infinite)
					{
						UnityEngine.Object.Destroy(this._destroyTarget);
					}
				}
				else
				{
					this.Used = true;
					this.GrabExit();
					this._destroyTarget.SetActive(false);
				}
			}
		}

		
		public bool TryPool()
		{
			if (this._poolManagerDespawnCreature && this._destroyTarget && (!BoltNetwork.isClient || !this._destroyTarget.GetComponent<BoltEntity>()) && PoolManager.Pools[this._poolManagerPool].IsSpawned(this._destroyTarget.transform))
			{
				base.enabled = false;
				this._destroyTarget.transform.parent = null;
				if (BoltNetwork.isServer && this._destroyTarget.GetComponent<BoltEntity>())
				{
					BoltNetwork.Detach(this._destroyTarget.gameObject);
				}
				PoolManager.Pools[this._poolManagerPool].Despawn(this._destroyTarget.transform);
				this.OwnershipCheckUnregister();
				return true;
			}
			return false;
		}

		
		private void ToggleRenderers(Transform t, bool enabled)
		{
			Renderer component = t.gameObject.GetComponent<Renderer>();
			if (component)
			{
				component.enabled = enabled;
			}
			IEnumerator enumerator = t.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform t2 = (Transform)obj;
					this.ToggleRenderers(t2, enabled);
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
		}

		
		protected virtual bool DoFakeDrop()
		{
			bool flag = false;
			if (!this._preventFakeDropReUse)
			{
				Rigidbody rigidbody;
				if (!this._spawnAfterPickupPrefab)
				{
					rigidbody = this._destroyTarget.GetComponentInChildren<Rigidbody>();
				}
				else
				{
					rigidbody = null;
				}
				if (rigidbody)
				{
					Collider component = rigidbody.GetComponent<Collider>();
					if (component.isTrigger)
					{
						rigidbody = null;
					}
					else if (rigidbody.gameObject != this._destroyTarget)
					{
						rigidbody.transform.localPosition = Vector3.zero;
					}
					flag = rigidbody;
				}
				if (!flag)
				{
					if (BoltNetwork.isClient && !this._spawnAfterPickupPrefab)
					{
						BoltEntity component2 = this._destroyTarget.GetComponent<BoltEntity>();
						if (component2)
						{
							flag = component2.isAttached;
						}
					}
					if (BoltNetwork.isRunning && !rigidbody)
					{
						BoltEntity component3 = this._destroyTarget.GetComponent<BoltEntity>();
						if (component3 && component3.isAttached)
						{
							return false;
						}
					}
				}
				else if (BoltNetwork.isRunning)
				{
					BoltEntity component4 = this._destroyTarget.GetComponent<BoltEntity>();
					if (!component4 || !component4.isAttached)
					{
						flag = false;
					}
				}
			}
			LocalPlayer.Inventory.FakeDrop(this._itemId, (!flag) ? null : this._destroyTarget);
			return flag;
		}

		
		private IEnumerator pickupSkinAnimalRoutine()
		{
			LocalPlayer.SpecialActions.SendMessage("setAnimalTransform", base.transform.root, SendMessageOptions.DontRequireReceiver);
			LocalPlayer.SpecialActions.SendMessage("setAnimalType", this._skinnedAnimalType, SendMessageOptions.DontRequireReceiver);
			LocalPlayer.SpecialActions.SendMessage("skinAnimalRoutine", base.transform.position, SendMessageOptions.DontRequireReceiver);
			yield return YieldPresets.WaitOneSecond;
			yield return YieldPresets.WaitOnePointFiveSeconds;
			yield return YieldPresets.WaitPointTwoFiveSeconds;
			this.Collect();
			yield break;
		}

		
		
		
		public bool Used { get; set; }

		
		public RandomRange _amount;

		
		[ItemIdPicker]
		public int _itemId;

		
		public PickUp.LootTuple[] _bonusItems;

		
		public ItemProperties _properties;

		
		public PickUp.InputMethods _inputMethod;

		
		public bool _infinite;

		
		public bool _preventFakeDrop;

		
		public bool _preventAutoEquip;

		
		public bool _forceAutoEquip;

		
		public bool _bodyAutoCollect;

		
		public bool _grabberAutoCollect;

		
		public bool _destroyIfFull;

		
		public bool _disableInsteadOfDestroy;

		
		public bool _spiderDice;

		
		public bool _poolManagerDespawnCreature;

		
		public bool _destroyIfAlreadyOwned;

		
		public bool _hideIfAlreadyOwned;

		
		public bool _positionHashSaving;

		
		public string _poolManagerPool = "creatures";

		
		public float _enableDelayMp;

		
		public GameObject _destroyTarget;

		
		public GameObject _spawnAfterPickupPrefab;

		
		public GameObject _spawnAfterPickupPrefab2;

		
		public GameObject _sheen;

		
		public GameObject _myPickUp;

		
		public bool _mpOnly;

		
		public bool _mpUseRequestReply;

		
		public bool _mpIsFlareFromCrate;

		
		public bool _useSkinAnimalRoutine;

		
		public int _skinnedAnimalType;

		
		public bool _preventFakeDropReUse;

		
		public bool _fakeArrowPickup;

		
		public bool _unparentArrows;

		
		public bool _firstTimePickup;

		
		public bool _hairSprayFuel;

		
		private bool startedSkinning;

		
		private int idleHash = Animator.StringToHash("idling");

		
		private int heldHash = Animator.StringToHash("held");

		
		private int _wsToken = -1;

		
		private float _enableTime;

		
		private bool _pendingdestroythroughbolt;

		
		[Serializable]
		public class LootTuple
		{
			
			public RandomRange _amount;

			
			[ItemIdPicker]
			public int _itemId;
		}

		
		public enum InputMethods
		{
			
			ButtonDown,
			
			ButtonHold,
			
			ButtonUp,
			
			ButtonPress
		}
	}
}
