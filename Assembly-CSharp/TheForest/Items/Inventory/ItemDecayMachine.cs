using System;
using System.Collections.Generic;
using TheForest.Utils.Settings;
using UniLinq;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	[DoNotSerializePublic]
	public class ItemDecayMachine : MonoBehaviour
	{
		private void Awake()
		{
			base.enabled = false;
		}

		private void Update()
		{
			if (this._commands.Count == 0)
			{
				base.enabled = false;
			}
			else if (this._commands[0]._decayTime < Time.time)
			{
				this.DecayNext();
			}
		}

		public void NewDecayCommand(DecayingInventoryItemView diiv, float decayDuration)
		{
			int num = 0;
			float decayTime = Time.time + decayDuration * GameSettings.Survival.MeatDecayDurationRatio;
			this.CancelCommandFor(diiv);
			if (this._commands.Count > 0)
			{
				num = this._commands.IndexOf(this._commands.FirstOrDefault((ItemDecayMachine.ItemDecayCommand c) => c._decayTime > decayTime));
				if (num < 0)
				{
					num = this._commands.Count;
				}
			}
			this._commands.Insert(num, new ItemDecayMachine.ItemDecayCommand
			{
				_diiv = diiv,
				_decayTime = decayTime
			});
			base.enabled = true;
		}

		public void CancelCommandFor(DecayingInventoryItemView diiv)
		{
			int num = this._commands.IndexOf(this._commands.FirstOrDefault((ItemDecayMachine.ItemDecayCommand c) => c._diiv.Equals(diiv)));
			if (num >= 0)
			{
				this._commands.RemoveAt(num);
			}
			if (this._commands.Count == 0)
			{
				base.enabled = false;
			}
		}

		public bool DecayNext()
		{
			if (this._commands.Count > 0)
			{
				ItemDecayMachine.ItemDecayCommand itemDecayCommand = this._commands[0];
				this._commands.RemoveAt(0);
				itemDecayCommand._diiv.Decay();
				return true;
			}
			return false;
		}

		private List<ItemDecayMachine.ItemDecayCommand> _commands = new List<ItemDecayMachine.ItemDecayCommand>();

		[Serializable]
		private class ItemDecayCommand
		{
			public DecayingInventoryItemView _diiv;

			public float _decayTime;
		}
	}
}
