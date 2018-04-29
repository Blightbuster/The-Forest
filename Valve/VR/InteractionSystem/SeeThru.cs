using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	public class SeeThru : MonoBehaviour
	{
		
		private void Awake()
		{
			this.interactable = base.GetComponentInParent<Interactable>();
			this.seeThru = new GameObject("_see_thru");
			this.seeThru.transform.parent = base.transform;
			this.seeThru.transform.localPosition = Vector3.zero;
			this.seeThru.transform.localRotation = Quaternion.identity;
			this.seeThru.transform.localScale = Vector3.one;
			MeshFilter component = base.GetComponent<MeshFilter>();
			if (component != null)
			{
				MeshFilter meshFilter = this.seeThru.AddComponent<MeshFilter>();
				meshFilter.sharedMesh = component.sharedMesh;
			}
			MeshRenderer component2 = base.GetComponent<MeshRenderer>();
			if (component2 != null)
			{
				this.sourceRenderer = component2;
				this.destRenderer = this.seeThru.AddComponent<MeshRenderer>();
			}
			SkinnedMeshRenderer component3 = base.GetComponent<SkinnedMeshRenderer>();
			if (component3 != null)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = this.seeThru.AddComponent<SkinnedMeshRenderer>();
				this.sourceRenderer = component3;
				this.destRenderer = skinnedMeshRenderer;
				skinnedMeshRenderer.sharedMesh = component3.sharedMesh;
				skinnedMeshRenderer.rootBone = component3.rootBone;
				skinnedMeshRenderer.bones = component3.bones;
				skinnedMeshRenderer.quality = component3.quality;
				skinnedMeshRenderer.updateWhenOffscreen = component3.updateWhenOffscreen;
			}
			if (this.sourceRenderer != null && this.destRenderer != null)
			{
				int num = this.sourceRenderer.sharedMaterials.Length;
				Material[] array = new Material[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = this.seeThruMaterial;
				}
				this.destRenderer.sharedMaterials = array;
				for (int j = 0; j < this.destRenderer.materials.Length; j++)
				{
					this.destRenderer.materials[j].renderQueue = 2001;
				}
				for (int k = 0; k < this.sourceRenderer.materials.Length; k++)
				{
					if (this.sourceRenderer.materials[k].renderQueue == 2000)
					{
						this.sourceRenderer.materials[k].renderQueue = 2002;
					}
				}
			}
			this.seeThru.gameObject.SetActive(false);
		}

		
		private void OnEnable()
		{
			this.interactable.onAttachedToHand += this.AttachedToHand;
			this.interactable.onDetachedFromHand += this.DetachedFromHand;
		}

		
		private void OnDisable()
		{
			this.interactable.onAttachedToHand -= this.AttachedToHand;
			this.interactable.onDetachedFromHand -= this.DetachedFromHand;
		}

		
		private void AttachedToHand(Hand hand)
		{
			this.seeThru.SetActive(true);
		}

		
		private void DetachedFromHand(Hand hand)
		{
			this.seeThru.SetActive(false);
		}

		
		private void Update()
		{
			if (this.seeThru.activeInHierarchy)
			{
				int num = Mathf.Min(this.sourceRenderer.materials.Length, this.destRenderer.materials.Length);
				for (int i = 0; i < num; i++)
				{
					this.destRenderer.materials[i].mainTexture = this.sourceRenderer.materials[i].mainTexture;
					this.destRenderer.materials[i].color = this.destRenderer.materials[i].color * this.sourceRenderer.materials[i].color;
				}
			}
		}

		
		public Material seeThruMaterial;

		
		private GameObject seeThru;

		
		private Interactable interactable;

		
		private Renderer sourceRenderer;

		
		private Renderer destRenderer;
	}
}
