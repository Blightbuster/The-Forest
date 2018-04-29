using System;
using System.Collections;
using Bolt;
using TheForest.Items;
using TheForest.Items.Utils;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[DoNotSerializePublic]
	[AddComponentMenu("Buildings/World/Water Source")]
	public class WaterSource : EntityBehaviour<IWaterSourceState>
	{
		
		private void Awake()
		{
			this._billboardDrink.SetActive(false);
			this._billboardDrinkSheen.SetActive(false);
			if (this._billboardGather)
			{
				this._billboardGather.SetActive(false);
				this._billboardGatherSheen.SetActive(false);
			}
			this._layerMask = 67108880;
			this._entityCache = base.entity;
			base.enabled = false;
		}

		
		private void Update()
		{
			bool canDrink = this.CanDrink;
			if (canDrink)
			{
				if (this._nextUseTime < Time.time)
				{
					bool canGather = this.CanGather;
					if (canGather)
					{
						if (TheForest.Utils.Input.GetButtonAfterDelay("Craft", 0.5f, false))
						{
							if (this.AtLake)
							{
								base.Invoke("doGatherWater", 1f);
							}
							else
							{
								LocalPlayer.Inventory.GatherWater(!this._polluted);
							}
							if (this._maxAmount > 0f)
							{
								this.RemoveWater(1f);
							}
							base.SendMessage("OnGatherWater", SendMessageOptions.DontRequireReceiver);
							if (this.AtLake)
							{
								this._nextUseTime = Time.time + 3f;
								this._canAutoRepeat = false;
								LocalPlayer.SpecialActions.SendMessage("doFillPotRoutine");
								this._billboardGather.SetActive(false);
								this._billboardGatherSheen.SetActive(false);
							}
							else
							{
								LocalPlayer.Sfx.PlayTwinkle();
							}
						}
					}
					else if (TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, this._canAutoRepeat))
					{
						float num;
						if (this._maxAmount == 0f)
						{
							num = LocalPlayer.Stats.Thirst;
						}
						else
						{
							num = Mathf.Min(this.AmountReal, Mathf.Max(LocalPlayer.Stats.Thirst, 0.15f));
							this.RemoveWater(num);
						}
						LocalPlayer.Stats.Thirst -= num * ((!this._polluted) ? 1f : GameSettings.Survival.PolutedWaterThirstRatio);
						ItemUtils.ApplyEffectsToStats(this._usedStatEffect, true, 1);
						if (this.AtLake)
						{
							this.enableDrinkParams();
							base.Invoke("playDrinkSFX", 1f);
							base.Invoke("doPollutedDamage", 2.2f);
							return;
						}
						this.playDrinkSFX();
						this.doPollutedDamage();
						this._nextUseTime = Time.time + this._useDelay;
						this._canAutoRepeat = false;
						base.SendMessage("OnDrinkWater", SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (TheForest.Utils.Input.GetButtonDown("Take"))
				{
					this._canAutoRepeat = true;
				}
			}
			this.ToggleIcons(this._nextUseTime < Time.time);
		}

		
		private void GrabEnter()
		{
			if (LocalPlayer.Transform && LocalPlayer.FpCharacter.drinking)
			{
				this._billboardGather.SetActive(false);
				this._billboardGatherSheen.SetActive(false);
				return;
			}
			base.enabled = true;
			this._terrainBlockDrink = false;
			this.ToggleIcons(this._nextUseTime < Time.time);
		}

		
		private void GrabExit()
		{
			base.enabled = false;
			this.ToggleIcons(false);
		}

		
		private void DeadBodyEnteredArea()
		{
			this._polluted = true;
		}

		
		private void playDrinkSFX()
		{
			if (this._iconMode != WaterSource.IconModes.AutoPosition)
			{
				LocalPlayer.Sfx.PlayDrinkFromWaterSource();
			}
		}

		
		private void doPollutedDamage()
		{
			if (this._polluted)
			{
				LocalPlayer.Stats.HitWaterDelayed(this._pollutedDamage);
			}
		}

		
		private void doGatherWater()
		{
			LocalPlayer.Inventory.GatherWater(!this._polluted);
		}

		
		public void AddWater(float amount)
		{
			this.AmountReal = Mathf.Min(this.AmountReal + amount, this._maxAmount);
			if (base.enabled)
			{
				this.ToggleIcons(base.enabled);
			}
			base.SendMessage("UpdateWater", SendMessageOptions.DontRequireReceiver);
		}

		
		public void RemoveWater(float amount)
		{
			if (BoltNetwork.isClient && this._entityCache && this._entityCache.isAttached)
			{
				RemoveWater removeWater = global::RemoveWater.Create(GlobalTargets.OnlyServer);
				removeWater.Amount = amount;
				removeWater.Entity = this._entityCache;
				removeWater.Send();
			}
			else
			{
				this.AmountReal = Mathf.Max(this.AmountReal - amount, 0f);
			}
			this.ToggleIcons(base.enabled);
			base.SendMessage("UpdateWater", SendMessageOptions.DontRequireReceiver);
		}

		
		private void ToggleIcons(bool sheen)
		{
			if (this.AtLake)
			{
				this.CheckTerrainBlocking();
			}
			if (LocalPlayer.FpCharacter && LocalPlayer.FpCharacter.drinking)
			{
				return;
			}
			bool canGather = this.CanGather;
			bool flag = !canGather && this.CanDrink;
			if (sheen)
			{
				if (this._nextUseTime > Time.time)
				{
					if (this._billboardDrink.activeSelf)
					{
						this._billboardDrink.SetActive(false);
					}
					if (this._billboardDrinkSheen.activeSelf)
					{
						this._billboardDrinkSheen.SetActive(false);
					}
					if (this._billboardGather)
					{
						if (this._billboardGather.activeSelf)
						{
							this._billboardGather.SetActive(false);
						}
						if (this._billboardGatherSheen.activeSelf)
						{
							this._billboardGatherSheen.SetActive(false);
						}
					}
				}
				else
				{
					if (this._billboardDrink.activeSelf)
					{
						this._billboardDrink.SetActive(false);
					}
					if (this._billboardDrinkSheen.activeSelf != flag)
					{
						this._billboardDrinkSheen.SetActive(flag);
					}
					if (flag)
					{
						this.AutoPositionIcon(this._billboardDrinkSheen);
					}
					if (this._billboardGather)
					{
						if (this._billboardGather.activeSelf)
						{
							this._billboardGather.SetActive(false);
						}
						if (this._billboardGatherSheen.activeSelf != canGather)
						{
							this._billboardGatherSheen.SetActive(canGather);
						}
						if (canGather)
						{
							this.AutoPositionIcon(this._billboardGatherSheen);
						}
					}
				}
			}
			else
			{
				if (this._billboardDrink.activeSelf != flag)
				{
					this._billboardDrink.SetActive(flag);
				}
				if (this._billboardDrinkSheen.activeSelf)
				{
					this._billboardDrinkSheen.SetActive(false);
				}
				if (flag)
				{
					this.AutoPositionIcon(this._billboardDrink);
				}
				if (this._billboardGather)
				{
					if (this._billboardGather.activeSelf != canGather)
					{
						this._billboardGather.SetActive(canGather);
					}
					if (this._billboardGatherSheen.activeSelf)
					{
						this._billboardGatherSheen.SetActive(false);
					}
					if (canGather)
					{
						this.AutoPositionIcon(this._billboardGather);
					}
				}
			}
		}

		
		private void CheckTerrainBlocking()
		{
			if (LocalPlayer.MainCamTr == null)
			{
				return;
			}
			Vector3 moveableIconPos = Vector3.zero;
			bool flag = false;
			if (Physics.Raycast(LocalPlayer.MainCamTr.position, LocalPlayer.MainCamTr.forward, out this._hit, 8f, this._layerMask.value))
			{
				if (this._hit.transform.gameObject.CompareTag("Water"))
				{
					flag = true;
					this._terrainBlockDrink = false;
				}
				if (this._hit.transform.gameObject.CompareTag("TerrainMain"))
				{
					this._terrainBlockDrink = true;
					return;
				}
				this._terrainBlockDrink = false;
				moveableIconPos = LocalPlayer.MainCamTr.position + LocalPlayer.MainCamTr.forward * 2.5f;
				moveableIconPos.y -= 1f;
				if (flag)
				{
					float b = this._hit.point.y + 1f;
					moveableIconPos.y = Mathf.Max(moveableIconPos.y, b);
				}
			}
			this._moveableIconPos = moveableIconPos;
		}

		
		private void AutoPositionIcon(GameObject icon)
		{
			if (LocalPlayer.Transform == null)
			{
				return;
			}
			if (this.AtLake)
			{
				icon.transform.position = this._moveableIconPos;
			}
		}

		
		private void enableDrinkParams()
		{
			LocalPlayer.Animator.SetBoolReflected("drinkWater", true);
			LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
			LocalPlayer.Inventory.HideAllEquiped(false, false);
			LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
			LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
			LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
			LocalPlayer.Transform.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
			LocalPlayer.MainRotator.enabled = false;
			LocalPlayer.CamRotator.stopInput = true;
			LocalPlayer.CamRotator.rotationRange = new Vector2(0f, 0f);
			LocalPlayer.FpCharacter.drinking = true;
			LocalPlayer.FpCharacter.enabled = false;
			LocalPlayer.CamFollowHead.smoothLock = true;
			LocalPlayer.CamFollowHead.lockYCam = true;
			LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothDisableSpine");
			base.StartCoroutine("forceStop");
			base.enabled = false;
			this._billboardDrink.SetActive(false);
			this._billboardDrinkSheen.SetActive(false);
		}

		
		private IEnumerator forceStop()
		{
			float t = 0f;
			while (t < 1f)
			{
				LocalPlayer.Rigidbody.velocity = new Vector3(0f, 0f, 0f);
				t += Time.deltaTime;
				yield return YieldPresets.WaitForFixedUpdate;
			}
			LocalPlayer.Animator.SetBoolReflected("drinkWater", false);
			LocalPlayer.Animator.SetBool("fillPotBool", false);
			yield return YieldPresets.WaitOneSecond;
			base.enabled = true;
			yield return YieldPresets.WaitPointSevenSeconds;
			LocalPlayer.Inventory.ShowAllEquiped(false);
			yield break;
		}

		
		private void resetFillPotBool()
		{
			LocalPlayer.Animator.SetBool("fillPotBool", false);
		}

		
		public override void Attached()
		{
			if (BoltNetwork.isServer)
			{
				base.state.amount = this._amount;
			}
			base.state.AddCallback("amount", new PropertyCallbackSimple(this.RefreshAmount));
		}

		
		private void RefreshAmount()
		{
			this.AmountReal = base.state.amount;
			base.SendMessage("CheckForWater", SendMessageOptions.DontRequireReceiver);
		}

		
		
		
		public float AmountReal
		{
			get
			{
				if (BoltNetwork.isRunning && this._entityCache && this._entityCache.isAttached)
				{
					return base.state.amount;
				}
				return this._amount;
			}
			set
			{
				if (BoltNetwork.isRunning && this._entityCache && this._entityCache.isAttached && this._entityCache.isOwner && base.state.amount != value)
				{
					base.state.amount = value;
				}
				this._amount = value;
			}
		}

		
		
		private bool CanDrink
		{
			get
			{
				if (this.AtLake)
				{
					return (!LocalPlayer.FpCharacter.swimming || LocalPlayer.Transform.position.y - LocalPlayer.WaterViz.WaterLevel > 1.3f) && !LocalPlayer.FpCharacter.jumping && (double)LocalPlayer.AnimControl.normCamX > 0.3 && LocalPlayer.Rigidbody.velocity.sqrMagnitude <= 0.1f && !this._terrainBlockDrink;
				}
				return base.enabled && (!LocalPlayer.FpCharacter.swimming || LocalPlayer.Transform.position.y - LocalPlayer.WaterViz.WaterLevel > 1.2f) && (this.AmountReal > this._minAmount || this._maxAmount == 0f) && (this._iconMode == WaterSource.IconModes.FixedPosition || (double)LocalPlayer.AnimControl.normCamX > 0.3) && !LocalPlayer.FpCharacter.jumping && !this._terrainBlockDrink && (double)LocalPlayer.Rigidbody.velocity.sqrMagnitude < 0.01;
			}
		}

		
		
		private bool CanGather
		{
			get
			{
				return this.CanDrink && LocalPlayer.Inventory && (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._potItemId) || LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._waterSkinItemId)) && this._allowGathering;
			}
		}

		
		
		private bool AtLake
		{
			get
			{
				return this._iconMode == WaterSource.IconModes.AutoPosition;
			}
		}

		
		[SerializeThis]
		public float _amount;

		
		public float _minAmount;

		
		public float _maxAmount;

		
		public int _pollutedDamage = 10;

		
		[SerializeThis]
		public bool _polluted;

		
		public bool _allowGathering = true;

		
		[ItemIdPicker]
		public int _potItemId;

		
		[ItemIdPicker]
		public int _waterSkinItemId;

		
		public GameObject _billboardDrink;

		
		public GameObject _billboardDrinkSheen;

		
		public GameObject _billboardGather;

		
		public GameObject _billboardGatherSheen;

		
		public WaterSource.IconModes _iconMode;

		
		public StatEffect[] _usedStatEffect;

		
		public float _useDelay = -1f;

		
		private BoltEntity _entityCache;

		
		private RaycastHit _hit;

		
		private LayerMask _layerMask;

		
		private bool _terrainBlockDrink;

		
		[SerializeThis]
		private float _lastUseTime = -1f;

		
		private float _nextUseTime;

		
		private bool _canAutoRepeat;

		
		private Vector3 _moveableIconPos;

		
		public enum IconModes
		{
			
			FixedPosition,
			
			AutoPosition
		}
	}
}
