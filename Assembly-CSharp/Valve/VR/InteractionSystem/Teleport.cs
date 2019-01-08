using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	public class Teleport : MonoBehaviour
	{
		public static SteamVR_Events.Action<float> ChangeSceneAction(UnityAction<float> action)
		{
			return new SteamVR_Events.Action<float>(Teleport.ChangeScene, action);
		}

		public static SteamVR_Events.Action<TeleportMarkerBase> PlayerAction(UnityAction<TeleportMarkerBase> action)
		{
			return new SteamVR_Events.Action<TeleportMarkerBase>(Teleport.Player, action);
		}

		public static SteamVR_Events.Action<TeleportMarkerBase> PlayerPreAction(UnityAction<TeleportMarkerBase> action)
		{
			return new SteamVR_Events.Action<TeleportMarkerBase>(Teleport.PlayerPre, action);
		}

		public static Teleport instance
		{
			get
			{
				if (Teleport._instance == null)
				{
					Teleport._instance = UnityEngine.Object.FindObjectOfType<Teleport>();
				}
				return Teleport._instance;
			}
		}

		private void Awake()
		{
			Teleport._instance = this;
			this.chaperoneInfoInitializedAction = ChaperoneInfo.InitializedAction(new UnityAction(this.OnChaperoneInfoInitialized));
			this.pointerLineRenderer = base.GetComponentInChildren<LineRenderer>();
			this.teleportPointerObject = this.pointerLineRenderer.gameObject;
			int nameID = Shader.PropertyToID("_TintColor");
			this.fullTintAlpha = this.pointVisibleMaterial.GetColor(nameID).a;
			this.teleportArc = base.GetComponent<TeleportArc>();
			this.teleportArc.traceLayerMask = this.traceLayerMask;
			this.loopingAudioMaxVolume = this.loopingAudioSource.volume;
			this.playAreaPreviewCorner.SetActive(false);
			this.playAreaPreviewSide.SetActive(false);
			float x = this.invalidReticleTransform.localScale.x;
			this.invalidReticleMinScale *= x;
			this.invalidReticleMaxScale *= x;
		}

		private void Start()
		{
			this.teleportMarkers = UnityEngine.Object.FindObjectsOfType<TeleportMarkerBase>();
			this.HidePointer();
			this.player = Valve.VR.InteractionSystem.Player.instance;
			if (this.player == null)
			{
				Debug.LogError("Teleport: No Player instance found in map.");
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			this.CheckForSpawnPoint();
			base.Invoke("ShowTeleportHint", 5f);
		}

		private void OnEnable()
		{
			this.chaperoneInfoInitializedAction.enabled = true;
			this.OnChaperoneInfoInitialized();
		}

		private void OnDisable()
		{
			this.chaperoneInfoInitializedAction.enabled = false;
			this.HidePointer();
		}

		private void CheckForSpawnPoint()
		{
			foreach (TeleportMarkerBase teleportMarkerBase in this.teleportMarkers)
			{
				TeleportPoint teleportPoint = teleportMarkerBase as TeleportPoint;
				if (teleportPoint && teleportPoint.playerSpawnPoint)
				{
					this.teleportingToMarker = teleportMarkerBase;
					this.TeleportPlayer();
					break;
				}
			}
		}

		public void HideTeleportPointer()
		{
			if (this.pointerHand != null)
			{
				this.HidePointer();
			}
		}

		private void Update()
		{
			Hand oldPointerHand = this.pointerHand;
			Hand hand = null;
			foreach (Hand hand2 in this.player.hands)
			{
				if (this.visible && this.WasTeleportButtonReleased(hand2) && this.pointerHand == hand2)
				{
					this.TryTeleportPlayer();
				}
				if (this.WasTeleportButtonPressed(hand2))
				{
					hand = hand2;
				}
			}
			if (this.allowTeleportWhileAttached && !this.allowTeleportWhileAttached.teleportAllowed)
			{
				this.HidePointer();
			}
			else if (!this.visible && hand != null)
			{
				this.ShowPointer(hand, oldPointerHand);
			}
			else if (this.visible)
			{
				if (hand == null && !this.IsTeleportButtonDown(this.pointerHand))
				{
					this.HidePointer();
				}
				else if (hand != null)
				{
					this.ShowPointer(hand, oldPointerHand);
				}
			}
			if (this.visible)
			{
				this.UpdatePointer();
				if (this.meshFading)
				{
					this.UpdateTeleportColors();
				}
				if (this.onActivateObjectTransform.gameObject.activeSelf && Time.time - this.pointerShowStartTime > this.activateObjectTime)
				{
					this.onActivateObjectTransform.gameObject.SetActive(false);
				}
			}
			else if (this.onDeactivateObjectTransform.gameObject.activeSelf && Time.time - this.pointerHideStartTime > this.deactivateObjectTime)
			{
				this.onDeactivateObjectTransform.gameObject.SetActive(false);
			}
		}

		private void UpdatePointer()
		{
			Vector3 position = this.pointerStartTransform.position;
			Vector3 forward = this.pointerStartTransform.forward;
			bool flag = false;
			bool active = false;
			Vector3 vector = this.player.trackingOriginTransform.position - this.player.feetPositionGuess;
			Vector3 velocity = forward * this.arcDistance;
			TeleportMarkerBase teleportMarkerBase = null;
			float num = Vector3.Dot(forward, Vector3.up);
			float num2 = Vector3.Dot(forward, this.player.hmdTransform.forward);
			bool flag2 = false;
			if ((num2 > 0f && num > 0.75f) || (num2 < 0f && num > 0.5f))
			{
				flag2 = true;
			}
			this.teleportArc.SetArcData(position, velocity, true, flag2);
			RaycastHit raycastHit;
			if (this.teleportArc.DrawArc(out raycastHit))
			{
				flag = true;
				teleportMarkerBase = raycastHit.collider.GetComponentInParent<TeleportMarkerBase>();
			}
			if (flag2)
			{
				teleportMarkerBase = null;
			}
			this.HighlightSelected(teleportMarkerBase);
			Vector3 vector2;
			if (teleportMarkerBase != null)
			{
				if (teleportMarkerBase.locked)
				{
					this.teleportArc.SetColor(this.pointerLockedColor);
					this.pointerLineRenderer.startColor = this.pointerLockedColor;
					this.pointerLineRenderer.endColor = this.pointerLockedColor;
					this.destinationReticleTransform.gameObject.SetActive(false);
				}
				else
				{
					this.teleportArc.SetColor(this.pointerValidColor);
					this.pointerLineRenderer.startColor = this.pointerValidColor;
					this.pointerLineRenderer.endColor = this.pointerValidColor;
					this.destinationReticleTransform.gameObject.SetActive(teleportMarkerBase.showReticle);
				}
				this.offsetReticleTransform.gameObject.SetActive(true);
				this.invalidReticleTransform.gameObject.SetActive(false);
				this.pointedAtTeleportMarker = teleportMarkerBase;
				this.pointedAtPosition = raycastHit.point;
				if (this.showPlayAreaMarker)
				{
					TeleportArea teleportArea = this.pointedAtTeleportMarker as TeleportArea;
					if (teleportArea != null && !teleportArea.locked && this.playAreaPreviewTransform != null)
					{
						Vector3 b = vector;
						if (!this.movedFeetFarEnough)
						{
							float num3 = Vector3.Distance(vector, this.startingFeetOffset);
							if (num3 < 0.1f)
							{
								b = this.startingFeetOffset;
							}
							else if (num3 < 0.4f)
							{
								b = Vector3.Lerp(this.startingFeetOffset, vector, (num3 - 0.1f) / 0.3f);
							}
							else
							{
								this.movedFeetFarEnough = true;
							}
						}
						this.playAreaPreviewTransform.position = this.pointedAtPosition + b;
						active = true;
					}
				}
				vector2 = raycastHit.point;
			}
			else
			{
				this.destinationReticleTransform.gameObject.SetActive(false);
				this.offsetReticleTransform.gameObject.SetActive(false);
				this.teleportArc.SetColor(this.pointerInvalidColor);
				this.pointerLineRenderer.startColor = this.pointerInvalidColor;
				this.pointerLineRenderer.endColor = this.pointerInvalidColor;
				this.invalidReticleTransform.gameObject.SetActive(!flag2);
				Vector3 toDirection = raycastHit.normal;
				float num4 = Vector3.Angle(raycastHit.normal, Vector3.up);
				if (num4 < 15f)
				{
					toDirection = Vector3.up;
				}
				this.invalidReticleTargetRotation = Quaternion.FromToRotation(Vector3.up, toDirection);
				this.invalidReticleTransform.rotation = Quaternion.Slerp(this.invalidReticleTransform.rotation, this.invalidReticleTargetRotation, 0.1f);
				float num5 = Vector3.Distance(raycastHit.point, this.player.hmdTransform.position);
				float num6 = Util.RemapNumberClamped(num5, this.invalidReticleMinScaleDistance, this.invalidReticleMaxScaleDistance, this.invalidReticleMinScale, this.invalidReticleMaxScale);
				this.invalidReticleScale.x = num6;
				this.invalidReticleScale.y = num6;
				this.invalidReticleScale.z = num6;
				this.invalidReticleTransform.transform.localScale = this.invalidReticleScale;
				this.pointedAtTeleportMarker = null;
				if (flag)
				{
					vector2 = raycastHit.point;
				}
				else
				{
					vector2 = this.teleportArc.GetArcPositionAtTime(this.teleportArc.arcDuration);
				}
				if (this.debugFloor)
				{
					this.floorDebugSphere.gameObject.SetActive(false);
					this.floorDebugLine.gameObject.SetActive(false);
				}
			}
			if (this.playAreaPreviewTransform != null)
			{
				this.playAreaPreviewTransform.gameObject.SetActive(active);
			}
			if (!this.showOffsetReticle)
			{
				this.offsetReticleTransform.gameObject.SetActive(false);
			}
			this.destinationReticleTransform.position = this.pointedAtPosition;
			this.invalidReticleTransform.position = vector2;
			this.onActivateObjectTransform.position = vector2;
			this.onDeactivateObjectTransform.position = vector2;
			this.offsetReticleTransform.position = vector2 - vector;
			this.reticleAudioSource.transform.position = this.pointedAtPosition;
			this.pointerLineRenderer.SetPosition(0, position);
			this.pointerLineRenderer.SetPosition(1, vector2);
		}

		private void FixedUpdate()
		{
			if (!this.visible)
			{
				return;
			}
			if (this.debugFloor)
			{
				TeleportArea x = this.pointedAtTeleportMarker as TeleportArea;
				if (x != null && this.floorFixupMaximumTraceDistance > 0f)
				{
					this.floorDebugSphere.gameObject.SetActive(true);
					this.floorDebugLine.gameObject.SetActive(true);
					Vector3 down = Vector3.down;
					down.x = 0.01f;
					RaycastHit raycastHit;
					if (Physics.Raycast(this.pointedAtPosition + 0.05f * down, down, out raycastHit, this.floorFixupMaximumTraceDistance, this.floorFixupTraceLayerMask))
					{
						this.floorDebugSphere.transform.position = raycastHit.point;
						this.floorDebugSphere.material.color = Color.green;
						this.floorDebugLine.startColor = Color.green;
						this.floorDebugLine.endColor = Color.green;
						this.floorDebugLine.SetPosition(0, this.pointedAtPosition);
						this.floorDebugLine.SetPosition(1, raycastHit.point);
					}
					else
					{
						Vector3 position = this.pointedAtPosition + down * this.floorFixupMaximumTraceDistance;
						this.floorDebugSphere.transform.position = position;
						this.floorDebugSphere.material.color = Color.red;
						this.floorDebugLine.startColor = Color.red;
						this.floorDebugLine.endColor = Color.red;
						this.floorDebugLine.SetPosition(0, this.pointedAtPosition);
						this.floorDebugLine.SetPosition(1, position);
					}
				}
			}
		}

		private void OnChaperoneInfoInitialized()
		{
			ChaperoneInfo instance = ChaperoneInfo.instance;
			if (instance.initialized && instance.roomscale)
			{
				if (this.playAreaPreviewTransform == null)
				{
					this.playAreaPreviewTransform = new GameObject("PlayAreaPreviewTransform").transform;
					this.playAreaPreviewTransform.parent = base.transform;
					Util.ResetTransform(this.playAreaPreviewTransform, true);
					this.playAreaPreviewCorner.SetActive(true);
					this.playAreaPreviewCorners = new Transform[4];
					this.playAreaPreviewCorners[0] = this.playAreaPreviewCorner.transform;
					this.playAreaPreviewCorners[1] = UnityEngine.Object.Instantiate<Transform>(this.playAreaPreviewCorners[0]);
					this.playAreaPreviewCorners[2] = UnityEngine.Object.Instantiate<Transform>(this.playAreaPreviewCorners[0]);
					this.playAreaPreviewCorners[3] = UnityEngine.Object.Instantiate<Transform>(this.playAreaPreviewCorners[0]);
					this.playAreaPreviewCorners[0].transform.parent = this.playAreaPreviewTransform;
					this.playAreaPreviewCorners[1].transform.parent = this.playAreaPreviewTransform;
					this.playAreaPreviewCorners[2].transform.parent = this.playAreaPreviewTransform;
					this.playAreaPreviewCorners[3].transform.parent = this.playAreaPreviewTransform;
					this.playAreaPreviewSide.SetActive(true);
					this.playAreaPreviewSides = new Transform[4];
					this.playAreaPreviewSides[0] = this.playAreaPreviewSide.transform;
					this.playAreaPreviewSides[1] = UnityEngine.Object.Instantiate<Transform>(this.playAreaPreviewSides[0]);
					this.playAreaPreviewSides[2] = UnityEngine.Object.Instantiate<Transform>(this.playAreaPreviewSides[0]);
					this.playAreaPreviewSides[3] = UnityEngine.Object.Instantiate<Transform>(this.playAreaPreviewSides[0]);
					this.playAreaPreviewSides[0].transform.parent = this.playAreaPreviewTransform;
					this.playAreaPreviewSides[1].transform.parent = this.playAreaPreviewTransform;
					this.playAreaPreviewSides[2].transform.parent = this.playAreaPreviewTransform;
					this.playAreaPreviewSides[3].transform.parent = this.playAreaPreviewTransform;
				}
				float playAreaSizeX = instance.playAreaSizeX;
				float playAreaSizeZ = instance.playAreaSizeZ;
				this.playAreaPreviewSides[0].localPosition = new Vector3(0f, 0f, 0.5f * playAreaSizeZ - 0.25f);
				this.playAreaPreviewSides[1].localPosition = new Vector3(0f, 0f, -0.5f * playAreaSizeZ + 0.25f);
				this.playAreaPreviewSides[2].localPosition = new Vector3(0.5f * playAreaSizeX - 0.25f, 0f, 0f);
				this.playAreaPreviewSides[3].localPosition = new Vector3(-0.5f * playAreaSizeX + 0.25f, 0f, 0f);
				this.playAreaPreviewSides[0].localScale = new Vector3(playAreaSizeX - 0.5f, 1f, 1f);
				this.playAreaPreviewSides[1].localScale = new Vector3(playAreaSizeX - 0.5f, 1f, 1f);
				this.playAreaPreviewSides[2].localScale = new Vector3(playAreaSizeZ - 0.5f, 1f, 1f);
				this.playAreaPreviewSides[3].localScale = new Vector3(playAreaSizeZ - 0.5f, 1f, 1f);
				this.playAreaPreviewSides[0].localRotation = Quaternion.Euler(0f, 0f, 0f);
				this.playAreaPreviewSides[1].localRotation = Quaternion.Euler(0f, 180f, 0f);
				this.playAreaPreviewSides[2].localRotation = Quaternion.Euler(0f, 90f, 0f);
				this.playAreaPreviewSides[3].localRotation = Quaternion.Euler(0f, 270f, 0f);
				this.playAreaPreviewCorners[0].localPosition = new Vector3(0.5f * playAreaSizeX - 0.25f, 0f, 0.5f * playAreaSizeZ - 0.25f);
				this.playAreaPreviewCorners[1].localPosition = new Vector3(0.5f * playAreaSizeX - 0.25f, 0f, -0.5f * playAreaSizeZ + 0.25f);
				this.playAreaPreviewCorners[2].localPosition = new Vector3(-0.5f * playAreaSizeX + 0.25f, 0f, -0.5f * playAreaSizeZ + 0.25f);
				this.playAreaPreviewCorners[3].localPosition = new Vector3(-0.5f * playAreaSizeX + 0.25f, 0f, 0.5f * playAreaSizeZ - 0.25f);
				this.playAreaPreviewCorners[0].localRotation = Quaternion.Euler(0f, 0f, 0f);
				this.playAreaPreviewCorners[1].localRotation = Quaternion.Euler(0f, 90f, 0f);
				this.playAreaPreviewCorners[2].localRotation = Quaternion.Euler(0f, 180f, 0f);
				this.playAreaPreviewCorners[3].localRotation = Quaternion.Euler(0f, 270f, 0f);
				this.playAreaPreviewTransform.gameObject.SetActive(false);
			}
		}

		private void HidePointer()
		{
			if (this.visible)
			{
				this.pointerHideStartTime = Time.time;
			}
			this.visible = false;
			if (this.pointerHand)
			{
				if (this.ShouldOverrideHoverLock())
				{
					if (this.originalHoverLockState)
					{
						this.pointerHand.HoverLock(this.originalHoveringInteractable);
					}
					else
					{
						this.pointerHand.HoverUnlock(null);
					}
				}
				this.loopingAudioSource.Stop();
				this.PlayAudioClip(this.pointerAudioSource, this.pointerStopSound);
			}
			this.teleportPointerObject.SetActive(false);
			this.teleportArc.Hide();
			foreach (TeleportMarkerBase teleportMarkerBase in this.teleportMarkers)
			{
				if (teleportMarkerBase != null && teleportMarkerBase.markerActive && teleportMarkerBase.gameObject != null)
				{
					teleportMarkerBase.gameObject.SetActive(false);
				}
			}
			this.destinationReticleTransform.gameObject.SetActive(false);
			this.invalidReticleTransform.gameObject.SetActive(false);
			this.offsetReticleTransform.gameObject.SetActive(false);
			if (this.playAreaPreviewTransform != null)
			{
				this.playAreaPreviewTransform.gameObject.SetActive(false);
			}
			if (this.onActivateObjectTransform.gameObject.activeSelf)
			{
				this.onActivateObjectTransform.gameObject.SetActive(false);
			}
			this.onDeactivateObjectTransform.gameObject.SetActive(true);
			this.pointerHand = null;
		}

		private void ShowPointer(Hand newPointerHand, Hand oldPointerHand)
		{
			if (!this.visible)
			{
				this.pointedAtTeleportMarker = null;
				this.pointerShowStartTime = Time.time;
				this.visible = true;
				this.meshFading = true;
				this.teleportPointerObject.SetActive(false);
				this.teleportArc.Show();
				foreach (TeleportMarkerBase teleportMarkerBase in this.teleportMarkers)
				{
					if (teleportMarkerBase.markerActive && teleportMarkerBase.ShouldActivate(this.player.feetPositionGuess))
					{
						teleportMarkerBase.gameObject.SetActive(true);
						teleportMarkerBase.Highlight(false);
					}
				}
				this.startingFeetOffset = this.player.trackingOriginTransform.position - this.player.feetPositionGuess;
				this.movedFeetFarEnough = false;
				if (this.onDeactivateObjectTransform.gameObject.activeSelf)
				{
					this.onDeactivateObjectTransform.gameObject.SetActive(false);
				}
				this.onActivateObjectTransform.gameObject.SetActive(true);
				this.loopingAudioSource.clip = this.pointerLoopSound;
				this.loopingAudioSource.loop = true;
				this.loopingAudioSource.Play();
				this.loopingAudioSource.volume = 0f;
			}
			if (oldPointerHand && this.ShouldOverrideHoverLock())
			{
				if (this.originalHoverLockState)
				{
					oldPointerHand.HoverLock(this.originalHoveringInteractable);
				}
				else
				{
					oldPointerHand.HoverUnlock(null);
				}
			}
			this.pointerHand = newPointerHand;
			if (this.visible && oldPointerHand != this.pointerHand)
			{
				this.PlayAudioClip(this.pointerAudioSource, this.pointerStartSound);
			}
			if (this.pointerHand)
			{
				this.pointerStartTransform = this.GetPointerStartTransform(this.pointerHand);
				if (this.pointerHand.currentAttachedObject != null)
				{
					this.allowTeleportWhileAttached = this.pointerHand.currentAttachedObject.GetComponent<AllowTeleportWhileAttachedToHand>();
				}
				this.originalHoverLockState = this.pointerHand.hoverLocked;
				this.originalHoveringInteractable = this.pointerHand.hoveringInteractable;
				if (this.ShouldOverrideHoverLock())
				{
					this.pointerHand.HoverLock(null);
				}
				this.pointerAudioSource.transform.SetParent(this.pointerStartTransform);
				this.pointerAudioSource.transform.localPosition = Vector3.zero;
				this.loopingAudioSource.transform.SetParent(this.pointerStartTransform);
				this.loopingAudioSource.transform.localPosition = Vector3.zero;
			}
		}

		private void UpdateTeleportColors()
		{
			float num = Time.time - this.pointerShowStartTime;
			if (num > this.meshFadeTime)
			{
				this.meshAlphaPercent = 1f;
				this.meshFading = false;
			}
			else
			{
				this.meshAlphaPercent = Mathf.Lerp(0f, 1f, num / this.meshFadeTime);
			}
			foreach (TeleportMarkerBase teleportMarkerBase in this.teleportMarkers)
			{
				teleportMarkerBase.SetAlpha(this.fullTintAlpha * this.meshAlphaPercent, this.meshAlphaPercent);
			}
		}

		private void PlayAudioClip(AudioSource source, AudioClip clip)
		{
			source.clip = clip;
			source.Play();
		}

		private void PlayPointerHaptic(bool validLocation)
		{
			if (this.pointerHand.controller != null)
			{
				if (validLocation)
				{
					this.pointerHand.controller.TriggerHapticPulse(800, EVRButtonId.k_EButton_Axis0);
				}
				else
				{
					this.pointerHand.controller.TriggerHapticPulse(100, EVRButtonId.k_EButton_Axis0);
				}
			}
		}

		private void TryTeleportPlayer()
		{
			if (this.visible && !this.teleporting && this.pointedAtTeleportMarker != null && !this.pointedAtTeleportMarker.locked)
			{
				this.teleportingToMarker = this.pointedAtTeleportMarker;
				this.InitiateTeleportFade();
				this.CancelTeleportHint();
			}
		}

		private void InitiateTeleportFade()
		{
			this.teleporting = true;
			this.currentFadeTime = this.teleportFadeTime;
			TeleportPoint teleportPoint = this.teleportingToMarker as TeleportPoint;
			if (teleportPoint != null && teleportPoint.teleportType == TeleportPoint.TeleportPointType.SwitchToNewScene)
			{
				this.currentFadeTime *= 3f;
				Teleport.ChangeScene.Send(this.currentFadeTime);
			}
			SteamVR_Fade.Start(Color.clear, 0f, false);
			SteamVR_Fade.Start(Color.black, this.currentFadeTime, false);
			this.headAudioSource.transform.SetParent(this.player.hmdTransform);
			this.headAudioSource.transform.localPosition = Vector3.zero;
			this.PlayAudioClip(this.headAudioSource, this.teleportSound);
			base.Invoke("TeleportPlayer", this.currentFadeTime);
		}

		private void TeleportPlayer()
		{
			this.teleporting = false;
			Teleport.PlayerPre.Send(this.pointedAtTeleportMarker);
			SteamVR_Fade.Start(Color.clear, this.currentFadeTime, false);
			TeleportPoint teleportPoint = this.teleportingToMarker as TeleportPoint;
			Vector3 a = this.pointedAtPosition;
			if (teleportPoint != null)
			{
				a = teleportPoint.transform.position;
				if (teleportPoint.teleportType == TeleportPoint.TeleportPointType.SwitchToNewScene)
				{
					teleportPoint.TeleportToScene();
					return;
				}
			}
			TeleportArea x = this.teleportingToMarker as TeleportArea;
			RaycastHit raycastHit;
			if (x != null && this.floorFixupMaximumTraceDistance > 0f && Physics.Raycast(a + 0.05f * Vector3.down, Vector3.down, out raycastHit, this.floorFixupMaximumTraceDistance, this.floorFixupTraceLayerMask))
			{
				a = raycastHit.point;
			}
			if (this.teleportingToMarker.ShouldMovePlayer())
			{
				Vector3 b = this.player.trackingOriginTransform.position - this.player.feetPositionGuess;
				this.player.trackingOriginTransform.position = a + b;
			}
			else
			{
				this.teleportingToMarker.TeleportPlayer(this.pointedAtPosition);
			}
			Teleport.Player.Send(this.pointedAtTeleportMarker);
		}

		private void HighlightSelected(TeleportMarkerBase hitTeleportMarker)
		{
			if (this.pointedAtTeleportMarker != hitTeleportMarker)
			{
				if (this.pointedAtTeleportMarker != null)
				{
					this.pointedAtTeleportMarker.Highlight(false);
				}
				if (hitTeleportMarker != null)
				{
					hitTeleportMarker.Highlight(true);
					this.prevPointedAtPosition = this.pointedAtPosition;
					this.PlayPointerHaptic(!hitTeleportMarker.locked);
					this.PlayAudioClip(this.reticleAudioSource, this.goodHighlightSound);
					this.loopingAudioSource.volume = this.loopingAudioMaxVolume;
				}
				else if (this.pointedAtTeleportMarker != null)
				{
					this.PlayAudioClip(this.reticleAudioSource, this.badHighlightSound);
					this.loopingAudioSource.volume = 0f;
				}
			}
			else if (hitTeleportMarker != null && Vector3.Distance(this.prevPointedAtPosition, this.pointedAtPosition) > 1f)
			{
				this.prevPointedAtPosition = this.pointedAtPosition;
				this.PlayPointerHaptic(!hitTeleportMarker.locked);
			}
		}

		public void ShowTeleportHint()
		{
			this.CancelTeleportHint();
			this.hintCoroutine = base.StartCoroutine(this.TeleportHintCoroutine());
		}

		public void CancelTeleportHint()
		{
			if (this.hintCoroutine != null)
			{
				foreach (Hand hand in this.player.hands)
				{
					ControllerButtonHints.HideTextHint(hand, EVRButtonId.k_EButton_Axis0);
				}
				base.StopCoroutine(this.hintCoroutine);
				this.hintCoroutine = null;
			}
			base.CancelInvoke("ShowTeleportHint");
		}

		private IEnumerator TeleportHintCoroutine()
		{
			float prevBreakTime = Time.time;
			float prevHapticPulseTime = Time.time;
			for (;;)
			{
				bool pulsed = false;
				foreach (Hand hand in this.player.hands)
				{
					bool flag = this.IsEligibleForTeleport(hand);
					bool flag2 = !string.IsNullOrEmpty(ControllerButtonHints.GetActiveHintText(hand, EVRButtonId.k_EButton_Axis0));
					if (flag)
					{
						if (!flag2)
						{
							ControllerButtonHints.ShowTextHint(hand, EVRButtonId.k_EButton_Axis0, "Teleport", true);
							prevBreakTime = Time.time;
							prevHapticPulseTime = Time.time;
						}
						if (Time.time > prevHapticPulseTime + 0.05f)
						{
							pulsed = true;
							hand.controller.TriggerHapticPulse(500, EVRButtonId.k_EButton_Axis0);
						}
					}
					else if (!flag && flag2)
					{
						ControllerButtonHints.HideTextHint(hand, EVRButtonId.k_EButton_Axis0);
					}
				}
				if (Time.time > prevBreakTime + 3f)
				{
					yield return new WaitForSeconds(3f);
					prevBreakTime = Time.time;
				}
				if (pulsed)
				{
					prevHapticPulseTime = Time.time;
				}
				yield return null;
			}
			yield break;
		}

		public bool IsEligibleForTeleport(Hand hand)
		{
			if (hand == null)
			{
				return false;
			}
			if (!hand.gameObject.activeInHierarchy)
			{
				return false;
			}
			if (hand.hoveringInteractable != null)
			{
				return false;
			}
			if (hand.noSteamVRFallbackCamera == null)
			{
				if (hand.controller == null)
				{
					return false;
				}
				if (hand.currentAttachedObject != null)
				{
					AllowTeleportWhileAttachedToHand component = hand.currentAttachedObject.GetComponent<AllowTeleportWhileAttachedToHand>();
					return component != null && component.teleportAllowed;
				}
			}
			return true;
		}

		private bool ShouldOverrideHoverLock()
		{
			return !this.allowTeleportWhileAttached || this.allowTeleportWhileAttached.overrideHoverLock;
		}

		private bool WasTeleportButtonReleased(Hand hand)
		{
			if (!this.IsEligibleForTeleport(hand))
			{
				return false;
			}
			if (hand.noSteamVRFallbackCamera != null)
			{
				return Input.GetKeyUp(KeyCode.T);
			}
			return hand.controller.GetPressUp(4294967296UL);
		}

		private bool IsTeleportButtonDown(Hand hand)
		{
			if (!this.IsEligibleForTeleport(hand))
			{
				return false;
			}
			if (hand.noSteamVRFallbackCamera != null)
			{
				return Input.GetKey(KeyCode.T);
			}
			return hand.controller.GetPress(4294967296UL);
		}

		private bool WasTeleportButtonPressed(Hand hand)
		{
			if (!this.IsEligibleForTeleport(hand))
			{
				return false;
			}
			if (hand.noSteamVRFallbackCamera != null)
			{
				return Input.GetKeyDown(KeyCode.T);
			}
			return hand.controller.GetPressDown(4294967296UL);
		}

		private Transform GetPointerStartTransform(Hand hand)
		{
			if (hand.noSteamVRFallbackCamera != null)
			{
				return hand.noSteamVRFallbackCamera.transform;
			}
			return this.pointerHand.GetAttachmentTransform("Attach_ControllerTip");
		}

		public LayerMask traceLayerMask;

		public LayerMask floorFixupTraceLayerMask;

		public float floorFixupMaximumTraceDistance = 1f;

		public Material areaVisibleMaterial;

		public Material areaLockedMaterial;

		public Material areaHighlightedMaterial;

		public Material pointVisibleMaterial;

		public Material pointLockedMaterial;

		public Material pointHighlightedMaterial;

		public Transform destinationReticleTransform;

		public Transform invalidReticleTransform;

		public GameObject playAreaPreviewCorner;

		public GameObject playAreaPreviewSide;

		public Color pointerValidColor;

		public Color pointerInvalidColor;

		public Color pointerLockedColor;

		public bool showPlayAreaMarker = true;

		public float teleportFadeTime = 0.1f;

		public float meshFadeTime = 0.2f;

		public float arcDistance = 10f;

		[Header("Effects")]
		public Transform onActivateObjectTransform;

		public Transform onDeactivateObjectTransform;

		public float activateObjectTime = 1f;

		public float deactivateObjectTime = 1f;

		[Header("Audio Sources")]
		public AudioSource pointerAudioSource;

		public AudioSource loopingAudioSource;

		public AudioSource headAudioSource;

		public AudioSource reticleAudioSource;

		[Header("Sounds")]
		public AudioClip teleportSound;

		public AudioClip pointerStartSound;

		public AudioClip pointerLoopSound;

		public AudioClip pointerStopSound;

		public AudioClip goodHighlightSound;

		public AudioClip badHighlightSound;

		[Header("Debug")]
		public bool debugFloor;

		public bool showOffsetReticle;

		public Transform offsetReticleTransform;

		public MeshRenderer floorDebugSphere;

		public LineRenderer floorDebugLine;

		private LineRenderer pointerLineRenderer;

		private GameObject teleportPointerObject;

		private Transform pointerStartTransform;

		private Hand pointerHand;

		private Player player;

		private TeleportArc teleportArc;

		private bool visible;

		private TeleportMarkerBase[] teleportMarkers;

		private TeleportMarkerBase pointedAtTeleportMarker;

		private TeleportMarkerBase teleportingToMarker;

		private Vector3 pointedAtPosition;

		private Vector3 prevPointedAtPosition;

		private bool teleporting;

		private float currentFadeTime;

		private float meshAlphaPercent = 1f;

		private float pointerShowStartTime;

		private float pointerHideStartTime;

		private bool meshFading;

		private float fullTintAlpha;

		private float invalidReticleMinScale = 0.2f;

		private float invalidReticleMaxScale = 1f;

		private float invalidReticleMinScaleDistance = 0.4f;

		private float invalidReticleMaxScaleDistance = 2f;

		private Vector3 invalidReticleScale = Vector3.one;

		private Quaternion invalidReticleTargetRotation = Quaternion.identity;

		private Transform playAreaPreviewTransform;

		private Transform[] playAreaPreviewCorners;

		private Transform[] playAreaPreviewSides;

		private float loopingAudioMaxVolume;

		private Coroutine hintCoroutine;

		private bool originalHoverLockState;

		private Interactable originalHoveringInteractable;

		private AllowTeleportWhileAttachedToHand allowTeleportWhileAttached;

		private Vector3 startingFeetOffset = Vector3.zero;

		private bool movedFeetFarEnough;

		private SteamVR_Events.Action chaperoneInfoInitializedAction;

		public static SteamVR_Events.Event<float> ChangeScene = new SteamVR_Events.Event<float>();

		public static SteamVR_Events.Event<TeleportMarkerBase> Player = new SteamVR_Events.Event<TeleportMarkerBase>();

		public static SteamVR_Events.Event<TeleportMarkerBase> PlayerPre = new SteamVR_Events.Event<TeleportMarkerBase>();

		private static Teleport _instance;
	}
}
