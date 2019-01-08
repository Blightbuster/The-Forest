using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Graphics
{
	[ExecuteInEditMode]
	[AddComponentMenu("The Forest/Graphics/Lake")]
	public class Lake : Water
	{
		public Bounds GetBounds()
		{
			if (this._cachedBoundsRender != this.newRenderer)
			{
				this._cachedBoundsRender = this.newRenderer;
				this._bounds = ((!(this._cachedBoundsRender != null)) ? default(Bounds) : this._cachedBoundsRender.bounds);
			}
			return this._bounds;
		}

		public override Material SharedMaterial
		{
			get
			{
				if (this.newRenderer == null)
				{
					return null;
				}
				return this.newRenderer.sharedMaterial;
			}
		}

		public override Material InstanceMaterial
		{
			get
			{
				if (this.newRenderer == null)
				{
					return null;
				}
				return this.newRenderer.material;
			}
		}

		public float BoundsSize
		{
			get
			{
				if (this.newCollider)
				{
					return this.newCollider.bounds.size.magnitude;
				}
				if (this.newRenderer)
				{
					return this.GetBounds().size.magnitude;
				}
				return 0f;
			}
		}

		public bool IsInBounds(Vector3 position)
		{
			if (this.newCollider)
			{
				RaycastHit raycastHit;
				return this.newCollider.Raycast(new Ray(position, Vector3.down), out raycastHit, 0f);
			}
			if (!this.newRenderer)
			{
				return false;
			}
			Bounds bounds = this.GetBounds();
			if (Vector3.Distance(position, bounds.center) > bounds.size.magnitude / 2f)
			{
				return false;
			}
			position.y = bounds.center.y;
			return bounds.Contains(position);
		}

		public Vector3 ClosestPoint(Vector3 position)
		{
			if (this.newCollider)
			{
				return this.newCollider.ClosestPointOnBounds(position);
			}
			if (this.newRenderer)
			{
				return this.GetBounds().ClosestPoint(position);
			}
			return base.transform.position;
		}

		protected void OnEnable()
		{
			if (this.newRenderer == null)
			{
				this.newRenderer = base.GetComponent<Renderer>();
			}
			if (this.newRenderer && !this.normalQualityMaterial)
			{
				this.normalQualityMaterial = this.newRenderer.sharedMaterial;
			}
			WaterEngine.Lakes.Add(this);
			this.InitMaterial();
		}

		protected void OnDisable()
		{
			if (WaterEngine.Lakes.Contains(this))
			{
				WaterEngine.Lakes.Remove(this);
			}
		}

		public void InitMaterial()
		{
			if (this.newRenderer && this.normalQualityMaterial && Prefabs.Instance)
			{
				if (TheForestQualitySettings.UserSettings.MaterialQuality == TheForestQualitySettings.MaterialQualities.Low)
				{
					this.newRenderer.sharedMaterial = Prefabs.Instance.LowQualityWaterMaterial;
				}
				else
				{
					this.newRenderer.sharedMaterial = this.normalQualityMaterial;
				}
			}
		}

		public Renderer newRenderer;

		public Collider newCollider;

		public Utility.TextureResolution cubemapResolution = Utility.TextureResolution._512;

		public LayerMask cubemapLayers = -1;

		public bool cubemapSettingsFoldout;

		public bool cubemapMipMaps = true;

		public float cubemapNearClipPlane = 0.5f;

		public float cubemapFarClipPlane = 4000f;

		private Material normalQualityMaterial;

		private Renderer _cachedBoundsRender;

		private Bounds _bounds;
	}
}
