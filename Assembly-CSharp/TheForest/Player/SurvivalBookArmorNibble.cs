using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	public class SurvivalBookArmorNibble : MonoBehaviour
	{
		private void Awake()
		{
			this._propertyBlock = new MaterialPropertyBlock();
			this._renderer = base.GetComponent<Renderer>();
			this.InitNibble();
		}

		private void OnEnable()
		{
			int num = (int)Mathf.Repeat((float)(Scene.HudGui.ArmorNibbles.Length - (this._nibbleId - LocalPlayer.Stats.ArmorVis + 1)), (float)Scene.HudGui.ArmorNibbles.Length);
			PlayerStats.ArmorTypes armorTypes = LocalPlayer.Stats.CurrentArmorTypes[num];
			if (armorTypes != PlayerStats.ArmorTypes.None)
			{
				if (this._armorType != armorTypes)
				{
					this.SwitchMatColor(LocalPlayer.Stats.GetArmorSetColor(armorTypes));
					this._armorType = armorTypes;
				}
			}
			else
			{
				this._armorType = armorTypes;
				this._renderer.enabled = false;
			}
		}

		private void InitNibble()
		{
			if (this._propertyBlock == null)
			{
				this._propertyBlock = new MaterialPropertyBlock();
			}
			this._renderer.GetPropertyBlock(this._propertyBlock);
			this._propertyBlock.SetVector("_MainTex_ST", new Vector4(0.1f, 1f, (float)this._nibbleId / 10f, 1f));
			this._renderer.SetPropertyBlock(this._propertyBlock);
		}

		private void SwitchMatColor(Color color)
		{
			if (this._propertyBlock == null)
			{
				this._propertyBlock = new MaterialPropertyBlock();
			}
			this._renderer.GetPropertyBlock(this._propertyBlock);
			this._propertyBlock.SetColor("_Color", color);
			this._renderer.SetPropertyBlock(this._propertyBlock);
			this._renderer.enabled = true;
		}

		public int _nibbleId;

		private PlayerStats.ArmorTypes _armorType = (PlayerStats.ArmorTypes)(-1);

		private Renderer _renderer;

		private MaterialPropertyBlock _propertyBlock;
	}
}
