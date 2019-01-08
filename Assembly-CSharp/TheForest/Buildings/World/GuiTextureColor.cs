using System;
using System.Collections;
using Bolt;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	[DoNotSerializePublic]
	public class GuiTextureColor : EntityBehaviour<IStickMarkerState>
	{
		private IEnumerator Start()
		{
			if (!LevelSerializer.IsDeserializing)
			{
				yield return null;
				base.enabled = false;
			}
			else if (BoltNetwork.isRunning)
			{
				base.enabled = false;
			}
			yield break;
		}

		private void Update()
		{
			if (this._overlayIcon && this._overlayIcon._color != this._currentColorNum)
			{
				this._currentColorNum = this._overlayIcon._color;
				this.SetTargetColor();
			}
		}

		private void OnDisable()
		{
			this.Update();
		}

		private IEnumerator OnDeserialized()
		{
			if (base.GetComponent<EmptyObjectIdentifier>())
			{
				UnityEngine.Object.Destroy(base.gameObject);
				yield break;
			}
			if (BoltNetwork.isRunning)
			{
				while (!base.entity || !base.entity.isAttached)
				{
					yield return null;
				}
			}
			if (this._color.a > 0f)
			{
				int a = ColorEx.ClosestColorIndex(this._color, new Color[]
				{
					Color.white,
					Color.yellow,
					Color.green,
					Color.magenta,
					Color.red,
					Color.blue
				});
				this._color.a = -1f;
				this._overlayIcon.SetColorIndex(Mathf.Max(a, 1));
			}
			this.SetTargetColor();
			yield break;
		}

		private void SetTargetColor()
		{
			if (this._targetRenderer)
			{
				if (BoltNetwork.isRunning && base.entity.isAttached)
				{
					this._overlayIcon._color = base.state.Flag;
				}
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				this._targetRenderer.GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetColor("_Color", this._overlayIcon.CurrentColor);
				this._targetRenderer.SetPropertyBlock(materialPropertyBlock);
				if (this._targetRendererVr)
				{
					this._targetRendererVr.GetPropertyBlock(materialPropertyBlock);
					materialPropertyBlock.SetColor("_Color", this._overlayIcon.CurrentColor);
					this._targetRendererVr.SetPropertyBlock(materialPropertyBlock);
				}
			}
		}

		public override void Attached()
		{
			if (!base.entity.isOwner)
			{
				this.SetTargetColor();
			}
			base.state.AddCallback("Flag", new PropertyCallbackSimple(this.SetTargetColor));
		}

		[SerializeThis]
		public Color _color;

		public Renderer _targetRenderer;

		public Renderer _targetRendererVr;

		public HomeIcon _overlayIcon;

		private int _currentColorNum;
	}
}
