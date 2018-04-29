using System;
using UnityEngine;

namespace TheForest.UI
{
	
	public class ActionIconUISprite : MonoBehaviour
	{
		
		private void Awake()
		{
			this._startHeight = this._sprite.height;
		}

		
		private void Update()
		{
			if (this._version != InputMappingIcons.Version)
			{
				this._sprite.height = this._startHeight;
				if (!InputMappingIcons.UsesText(this._action) || !this._label)
				{
					string mappingFor = InputMappingIcons.GetMappingFor(this._action);
					if (string.IsNullOrEmpty(mappingFor))
					{
						this._sprite.enabled = false;
						if (this._label)
						{
							this._label.enabled = false;
						}
					}
					else
					{
						this._sprite.spriteName = mappingFor;
						this._sprite.enabled = true;
						if (this._label)
						{
							this._label.enabled = false;
						}
					}
					UISpriteData atlasSprite = this._sprite.GetAtlasSprite();
					if (atlasSprite != null)
					{
						float num = (float)atlasSprite.width / (float)atlasSprite.height;
						this._sprite.width = Mathf.RoundToInt(num * (float)this._sprite.height);
					}
					else
					{
						Debug.LogError("Missing sprite: " + this._sprite.spriteName);
					}
				}
				else
				{
					this._sprite.spriteName = InputMappingIcons.TextIconBacking.name;
					this._sprite.enabled = true;
					this._label.text = InputMappingIcons.GetMappingFor(this._action);
					this._label.enabled = true;
					float num2;
					if (this._invertScale)
					{
						num2 = (float)this._label.width * 1f / this._label.transform.localScale.x / (float)this._startHeight;
					}
					else
					{
						num2 = (float)this._label.width * 1f * this._label.transform.localScale.x / (float)this._startHeight;
					}
					if (num2 > 1.5f)
					{
						this._sprite.width = Mathf.RoundToInt((float)this._startHeight * num2);
					}
					else
					{
						this._sprite.width = this._startHeight;
					}
				}
				this._version = InputMappingIcons.Version;
			}
		}

		
		public void ChangeAction(InputMappingIcons.Actions action)
		{
			if (this._action != action)
			{
				this._action = action;
				this._version--;
			}
		}

		
		public InputMappingIcons.Actions _action;

		
		public UISprite _sprite;

		
		public UILabel _label;

		
		public bool _invertScale;

		
		private int _version;

		
		private int _startHeight;
	}
}
