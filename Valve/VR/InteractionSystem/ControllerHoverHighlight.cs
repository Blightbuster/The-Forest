using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	
	public class ControllerHoverHighlight : MonoBehaviour
	{
		
		private void Start()
		{
			this.hand = base.GetComponentInParent<Hand>();
		}

		
		private void Awake()
		{
			this.renderModelLoadedAction = SteamVR_Events.RenderModelLoadedAction(new UnityAction<SteamVR_RenderModel, bool>(this.OnRenderModelLoaded));
		}

		
		private void OnEnable()
		{
			this.renderModelLoadedAction.enabled = true;
		}

		
		private void OnDisable()
		{
			this.renderModelLoadedAction.enabled = false;
		}

		
		private void OnHandInitialized(int deviceIndex)
		{
			this.renderModel = base.gameObject.AddComponent<SteamVR_RenderModel>();
			this.renderModel.SetDeviceIndex(deviceIndex);
			this.renderModel.updateDynamically = false;
		}

		
		private void OnRenderModelLoaded(SteamVR_RenderModel renderModel, bool success)
		{
			if (renderModel != this.renderModel)
			{
				return;
			}
			Transform transform = base.transform.Find("body");
			if (transform != null)
			{
				transform.gameObject.layer = base.gameObject.layer;
				transform.gameObject.tag = base.gameObject.tag;
				this.bodyMeshRenderer = transform.GetComponent<MeshRenderer>();
				this.bodyMeshRenderer.material = this.highLightMaterial;
				this.bodyMeshRenderer.enabled = false;
			}
			Transform transform2 = base.transform.Find("trackhat");
			if (transform2 != null)
			{
				transform2.gameObject.layer = base.gameObject.layer;
				transform2.gameObject.tag = base.gameObject.tag;
				this.trackingHatMeshRenderer = transform2.GetComponent<MeshRenderer>();
				this.trackingHatMeshRenderer.material = this.highLightMaterial;
				this.trackingHatMeshRenderer.enabled = false;
			}
			IEnumerator enumerator = base.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform3 = (Transform)obj;
					if (transform3.name != "body" && transform3.name != "trackhat")
					{
						UnityEngine.Object.Destroy(transform3.gameObject);
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
			this.renderModelLoaded = true;
		}

		
		private void OnParentHandHoverBegin(Interactable other)
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			if (other.transform.parent != base.transform.parent)
			{
				this.ShowHighlight();
			}
		}

		
		private void OnParentHandHoverEnd(Interactable other)
		{
			this.HideHighlight();
		}

		
		private void OnParentHandInputFocusAcquired()
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			if (this.hand.hoveringInteractable && this.hand.hoveringInteractable.transform.parent != base.transform.parent)
			{
				this.ShowHighlight();
			}
		}

		
		private void OnParentHandInputFocusLost()
		{
			this.HideHighlight();
		}

		
		public void ShowHighlight()
		{
			if (!this.renderModelLoaded)
			{
				return;
			}
			if (this.fireHapticsOnHightlight)
			{
				this.hand.controller.TriggerHapticPulse(500, EVRButtonId.k_EButton_Axis0);
			}
			if (this.bodyMeshRenderer != null)
			{
				this.bodyMeshRenderer.enabled = true;
			}
			if (this.trackingHatMeshRenderer != null)
			{
				this.trackingHatMeshRenderer.enabled = true;
			}
		}

		
		public void HideHighlight()
		{
			if (!this.renderModelLoaded)
			{
				return;
			}
			if (this.fireHapticsOnHightlight)
			{
				this.hand.controller.TriggerHapticPulse(300, EVRButtonId.k_EButton_Axis0);
			}
			if (this.bodyMeshRenderer != null)
			{
				this.bodyMeshRenderer.enabled = false;
			}
			if (this.trackingHatMeshRenderer != null)
			{
				this.trackingHatMeshRenderer.enabled = false;
			}
		}

		
		public Material highLightMaterial;

		
		public bool fireHapticsOnHightlight = true;

		
		private Hand hand;

		
		private MeshRenderer bodyMeshRenderer;

		
		private MeshRenderer trackingHatMeshRenderer;

		
		private SteamVR_RenderModel renderModel;

		
		private bool renderModelLoaded;

		
		private SteamVR_Events.Action renderModelLoadedAction;
	}
}
