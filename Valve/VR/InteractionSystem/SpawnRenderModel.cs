using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	
	public class SpawnRenderModel : MonoBehaviour
	{
		
		private void Awake()
		{
			this.renderModels = new SteamVR_RenderModel[this.materials.Length];
			this.renderModelLoadedAction = SteamVR_Events.RenderModelLoadedAction(new UnityAction<SteamVR_RenderModel, bool>(this.OnRenderModelLoaded));
		}

		
		private void OnEnable()
		{
			this.ShowController();
			this.renderModelLoadedAction.enabled = true;
			SpawnRenderModel.spawnRenderModels.Add(this);
		}

		
		private void OnDisable()
		{
			this.HideController();
			this.renderModelLoadedAction.enabled = false;
			SpawnRenderModel.spawnRenderModels.Remove(this);
		}

		
		private void OnAttachedToHand(Hand hand)
		{
			this.hand = hand;
			this.ShowController();
		}

		
		private void OnDetachedFromHand(Hand hand)
		{
			this.hand = null;
			this.HideController();
		}

		
		private void Update()
		{
			if (SpawnRenderModel.lastFrameUpdated == Time.renderedFrameCount)
			{
				return;
			}
			SpawnRenderModel.lastFrameUpdated = Time.renderedFrameCount;
			if (SpawnRenderModel.spawnRenderModelUpdateIndex >= SpawnRenderModel.spawnRenderModels.Count)
			{
				SpawnRenderModel.spawnRenderModelUpdateIndex = 0;
			}
			if (SpawnRenderModel.spawnRenderModelUpdateIndex < SpawnRenderModel.spawnRenderModels.Count)
			{
				SteamVR_RenderModel steamVR_RenderModel = SpawnRenderModel.spawnRenderModels[SpawnRenderModel.spawnRenderModelUpdateIndex].renderModels[0];
				if (steamVR_RenderModel != null)
				{
					steamVR_RenderModel.UpdateComponents(OpenVR.RenderModels);
				}
			}
			SpawnRenderModel.spawnRenderModelUpdateIndex++;
		}

		
		private void ShowController()
		{
			if (this.hand == null || this.hand.controller == null)
			{
				return;
			}
			for (int i = 0; i < this.renderModels.Length; i++)
			{
				if (this.renderModels[i] == null)
				{
					this.renderModels[i] = new GameObject("SteamVR_RenderModel").AddComponent<SteamVR_RenderModel>();
					this.renderModels[i].updateDynamically = false;
					this.renderModels[i].transform.parent = base.transform;
					Util.ResetTransform(this.renderModels[i].transform, true);
				}
				this.renderModels[i].gameObject.SetActive(true);
				this.renderModels[i].SetDeviceIndex((int)this.hand.controller.index);
			}
		}

		
		private void HideController()
		{
			for (int i = 0; i < this.renderModels.Length; i++)
			{
				if (this.renderModels[i] != null)
				{
					this.renderModels[i].gameObject.SetActive(false);
				}
			}
		}

		
		private void OnRenderModelLoaded(SteamVR_RenderModel renderModel, bool success)
		{
			for (int i = 0; i < this.renderModels.Length; i++)
			{
				if (renderModel == this.renderModels[i] && this.materials[i] != null)
				{
					this.renderers.Clear();
					this.renderModels[i].GetComponentsInChildren<MeshRenderer>(this.renderers);
					for (int j = 0; j < this.renderers.Count; j++)
					{
						Texture mainTexture = this.renderers[j].material.mainTexture;
						this.renderers[j].sharedMaterial = this.materials[i];
						this.renderers[j].material.mainTexture = mainTexture;
						this.renderers[j].gameObject.layer = base.gameObject.layer;
						this.renderers[j].tag = base.gameObject.tag;
					}
				}
			}
		}

		
		public Material[] materials;

		
		private SteamVR_RenderModel[] renderModels;

		
		private Hand hand;

		
		private List<MeshRenderer> renderers = new List<MeshRenderer>();

		
		private static List<SpawnRenderModel> spawnRenderModels = new List<SpawnRenderModel>();

		
		private static int lastFrameUpdated;

		
		private static int spawnRenderModelUpdateIndex;

		
		private SteamVR_Events.Action renderModelLoadedAction;
	}
}
