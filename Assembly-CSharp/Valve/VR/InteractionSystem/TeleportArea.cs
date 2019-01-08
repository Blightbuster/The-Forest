using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class TeleportArea : TeleportMarkerBase
	{
		public Bounds meshBounds { get; private set; }

		public void Awake()
		{
			this.areaMesh = base.GetComponent<MeshRenderer>();
			this.tintColorId = Shader.PropertyToID("_TintColor");
			this.CalculateBounds();
		}

		public void Start()
		{
			this.visibleTintColor = Teleport.instance.areaVisibleMaterial.GetColor(this.tintColorId);
			this.highlightedTintColor = Teleport.instance.areaHighlightedMaterial.GetColor(this.tintColorId);
			this.lockedTintColor = Teleport.instance.areaLockedMaterial.GetColor(this.tintColorId);
		}

		public override bool ShouldActivate(Vector3 playerPosition)
		{
			return true;
		}

		public override bool ShouldMovePlayer()
		{
			return true;
		}

		public override void Highlight(bool highlight)
		{
			if (!this.locked)
			{
				this.highlighted = highlight;
				if (highlight)
				{
					this.areaMesh.material = Teleport.instance.areaHighlightedMaterial;
				}
				else
				{
					this.areaMesh.material = Teleport.instance.areaVisibleMaterial;
				}
			}
		}

		public override void SetAlpha(float tintAlpha, float alphaPercent)
		{
			Color tintColor = this.GetTintColor();
			tintColor.a *= alphaPercent;
			this.areaMesh.material.SetColor(this.tintColorId, tintColor);
		}

		public override void UpdateVisuals()
		{
			if (this.locked)
			{
				this.areaMesh.material = Teleport.instance.areaLockedMaterial;
			}
			else
			{
				this.areaMesh.material = Teleport.instance.areaVisibleMaterial;
			}
		}

		public void UpdateVisualsInEditor()
		{
			this.areaMesh = base.GetComponent<MeshRenderer>();
			if (this.locked)
			{
				this.areaMesh.sharedMaterial = Teleport.instance.areaLockedMaterial;
			}
			else
			{
				this.areaMesh.sharedMaterial = Teleport.instance.areaVisibleMaterial;
			}
		}

		private bool CalculateBounds()
		{
			MeshFilter component = base.GetComponent<MeshFilter>();
			if (component == null)
			{
				return false;
			}
			Mesh sharedMesh = component.sharedMesh;
			if (sharedMesh == null)
			{
				return false;
			}
			this.meshBounds = sharedMesh.bounds;
			return true;
		}

		private Color GetTintColor()
		{
			if (this.locked)
			{
				return this.lockedTintColor;
			}
			if (this.highlighted)
			{
				return this.highlightedTintColor;
			}
			return this.visibleTintColor;
		}

		private MeshRenderer areaMesh;

		private int tintColorId;

		private Color visibleTintColor = Color.clear;

		private Color highlightedTintColor = Color.clear;

		private Color lockedTintColor = Color.clear;

		private bool highlighted;
	}
}
