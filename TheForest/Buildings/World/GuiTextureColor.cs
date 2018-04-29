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
			if (BoltNetwork.isRunning)
			{
				while (!this.entity || !this.entity.isAttached)
				{
					yield return null;
				}
			}
			if (this._color.a > 0f)
			{
				int color = ColorEx.ClosestColorIndex(this._color, new Color[]
				{
					Color.white,
					Color.yellow,
					Color.green,
					Color.magenta,
					Color.red,
					Color.blue
				});
				this._color.a = -1f;
				this._overlayIcon.SetColorIndex(Mathf.Max(color, 1));
			}
			this.SetTargetColor();
			yield break;
		}

		
		private void SetTargetColor()
		{
			if (this._targetRenderer)
			{
				if (BoltNetwork.isRunning)
				{
					this._overlayIcon._color = base.state.Flag;
				}
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				this._targetRenderer.GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetColor("_Color", this._overlayIcon.CurrentColor);
				this._targetRenderer.SetPropertyBlock(materialPropertyBlock);
			}
		}

		
		public override void Attached()
		{
			if (!this.entity.isOwner)
			{
				this.SetTargetColor();
			}
			base.state.AddCallback("Flag", new PropertyCallbackSimple(this.SetTargetColor));
		}

		
		[SerializeThis]
		public Color _color;

		
		public Renderer _targetRenderer;

		
		public HomeIcon _overlayIcon;

		
		private int _currentColorNum;
	}
}
