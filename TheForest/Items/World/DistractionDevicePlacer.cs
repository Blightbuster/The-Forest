using System;
using Bolt;
using FMOD.Studio;
using TheForest.Items.Special;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	
	[AddComponentMenu("Items/World/Distraction Device Placer")]
	public class DistractionDevicePlacer : MonoBehaviour
	{
		
		private void Start()
		{
			base.enabled = false;
			this._placeIconSheen.SetActive(false);
		}

		
		private void FixedUpdate()
		{
			if (this._targetObject && !this._noValidTargetObject)
			{
				if (!this._locked && !Grabber.IsFocused && LocalPlayer.Inventory.HasInSlot(this._slot, this._itemId))
				{
					if (Physics.Raycast(LocalPlayer.MainCamTr.position, LocalPlayer.MainCamTr.forward, out this._hit, 10f, this._layerMask.value) && !this._hit.collider.isTrigger)
					{
						Rigidbody component = this._hit.transform.GetComponent<Rigidbody>();
						if (!component)
						{
							component = this._hit.transform.parent.GetComponent<Rigidbody>();
						}
						if (component && component.useGravity)
						{
							this._noValidTargetObject = true;
							return;
						}
						if (this._targetObject.CompareTag("trapTrigger"))
						{
							this._placeIconSheen.transform.position = this._targetObject.transform.position;
						}
						else
						{
							this._placeIconSheen.transform.position = this._hit.point + LocalPlayer.MainCamTr.forward * -0.1f;
						}
						if (!this._placeIconSheen.activeSelf)
						{
							this._placeIconSheen.transform.parent = null;
							this._placeIconSheen.SetActive(true);
						}
						if (TheForest.Utils.Input.GetButtonDown("Craft"))
						{
							if (this._playWhooshOnInput)
							{
								LocalPlayer.Sfx.PlayWhoosh();
							}
							bool flag = true;
							if (LocalPlayer.ActiveBurnableItem != null)
							{
								flag = !LocalPlayer.ActiveBurnableItem.IsUnlit();
							}
							if ((this._slot != Item.EquipmentSlot.RightHand) ? LocalPlayer.Inventory.RemoveItem(this._itemId, 1, false, true) : LocalPlayer.Inventory.ShuffleRemoveRightHandItem())
							{
								Vector3 forward;
								if (this._rotationMode == DistractionDevicePlacer.RotationModes.LookAtCenterOfTarget)
								{
									forward = this._hit.point - this._hit.transform.position;
								}
								else
								{
									forward = this._hit.normal;
								}
								forward.y = 0f;
								Vector3 position = this._hit.point;
								if (this._targetObject.CompareTag("trapTrigger"))
								{
									position = this._targetObject.transform.position;
								}
								if (BoltNetwork.isRunning && this._spawnMode == DistractionDevicePlacer.SpawnModes.ServerSide)
								{
									DropItem dropItem = DropItem.Create(GlobalTargets.OnlyServer);
									dropItem.PrefabId = ((!flag) ? this._distractionDevicePrefabUnlit : this._distractionDevicePrefab).GetComponent<BoltEntity>().prefabId;
									dropItem.Position = position;
									dropItem.Rotation = Quaternion.LookRotation(forward);
									dropItem.PreSpawned = null;
									dropItem.AvoidImpacts = false;
									dropItem.Send();
								}
								else
								{
									GameObject gameObject;
									if (flag)
									{
										gameObject = UnityEngine.Object.Instantiate<GameObject>(this._distractionDevicePrefab, position, Quaternion.LookRotation(forward));
									}
									else
									{
										gameObject = UnityEngine.Object.Instantiate<GameObject>(this._distractionDevicePrefabUnlit, position, Quaternion.LookRotation(forward));
									}
									if (this._targetObject.CompareTag("Tree") || this._targetObject.transform.root.CompareTag("Tree"))
									{
										TreeHealth component2 = this._targetObject.GetComponent<TreeHealth>();
										if (!component2)
										{
											component2 = this._targetObject.transform.root.GetComponent<TreeHealth>();
										}
										if (component2 && component2.LodTree)
										{
											component2.LodTree.AddTreeCutDownTarget(gameObject);
										}
									}
									if (BoltNetwork.isRunning)
									{
										BoltEntity component3 = BoltNetwork.Attach(gameObject).GetComponent<BoltEntity>();
										if (component3 && this._playAudio && WalkmanControler.HasCassetteReady)
										{
											component3.GetState<IDistractionDevice>().MusicTrack = WalkmanControler.CurrentTrack + 10;
										}
									}
									else if (this._playAudio)
									{
										EventInstance eventInstance = LocalPlayer.Sfx.RelinquishMusicTrack();
										if (eventInstance == null && WalkmanControler.HasCassetteReady)
										{
											eventInstance = LocalPlayer.Sfx.InstantiateMusicTrack(WalkmanControler.CurrentTrack);
										}
										if (eventInstance != null)
										{
											gameObject.SendMessage("ActivateDevice", eventInstance);
										}
										gameObject.SendMessage("SetPlayerSfx", LocalPlayer.Sfx);
									}
								}
								this._placeIconSheen.SetActive(false);
								this._placeIconSheen.transform.parent = base.transform;
								this.Deactivate();
							}
						}
					}
				}
				else if (this._placeIconSheen.activeSelf)
				{
					this._placeIconSheen.transform.parent = base.transform;
					this._placeIconSheen.SetActive(false);
				}
			}
			else
			{
				this.Deactivate();
			}
		}

		
		private void OnTriggerEnter(Collider other)
		{
			if ((!this._inCavesOnly || LocalPlayer.IsInCaves) && (1 << other.gameObject.layer & this._layerMask) > 0)
			{
				this._targetObject = other.gameObject;
				base.enabled = true;
			}
		}

		
		private void OnTriggerExit(Collider other)
		{
			if (other.gameObject == this._targetObject)
			{
				this.Deactivate();
			}
		}

		
		public void ToggleManualLock(bool locked)
		{
			this._locked = locked;
		}

		
		private void Deactivate()
		{
			this._targetObject = null;
			this._noValidTargetObject = false;
			if (this._placeIconSheen)
			{
				this._placeIconSheen.SetActive(false);
				this._placeIconSheen.transform.parent = base.transform;
			}
			base.enabled = false;
		}

		
		[ItemIdPicker]
		public int _itemId;

		
		public Item.EquipmentSlot _slot;

		
		public LayerMask _layerMask;

		
		public GameObject _distractionDevicePrefab;

		
		public GameObject _distractionDevicePrefabUnlit;

		
		public GameObject _placeIconSheen;

		
		public GameObject _targetObject;

		
		public GameObject _heldGo;

		
		public bool _playAudio;

		
		public bool _playWhooshOnInput;

		
		public bool _locked;

		
		public bool _inCavesOnly;

		
		public DistractionDevicePlacer.RotationModes _rotationMode;

		
		public DistractionDevicePlacer.SpawnModes _spawnMode;

		
		private bool _noValidTargetObject;

		
		private RaycastHit _hit;

		
		public enum RotationModes
		{
			
			LookAtCenterOfTarget,
			
			LookAtRaycastNormal
		}

		
		public enum SpawnModes
		{
			
			Local,
			
			ServerSide
		}
	}
}
