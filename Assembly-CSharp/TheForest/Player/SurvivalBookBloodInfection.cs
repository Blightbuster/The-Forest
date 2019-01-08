using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	public class SurvivalBookBloodInfection : MonoBehaviour
	{
		private void OnEnable()
		{
			foreach (Renderer r in this._renderers)
			{
				this.SwitchMatColor(r, (!LocalPlayer.Stats.BloodInfection.Infected) ? this._notInfecterColor : this._infectedColor);
			}
			this._text.color = ((!LocalPlayer.Stats.BloodInfection.Infected) ? this._textNotInfecterColor : this._textInfectedColor);
		}

		private void SwitchMatColor(Renderer r, Color color)
		{
			if (this._propertyBlock == null)
			{
				this._propertyBlock = new MaterialPropertyBlock();
			}
			r.GetPropertyBlock(this._propertyBlock);
			this._propertyBlock.SetColor("_Color", color);
			r.SetPropertyBlock(this._propertyBlock);
		}

		public TextMesh _text;

		public Color _textNotInfecterColor;

		public Color _textInfectedColor;

		public Renderer[] _renderers;

		public Color _notInfecterColor;

		public Color _infectedColor;

		private MaterialPropertyBlock _propertyBlock;
	}
}
