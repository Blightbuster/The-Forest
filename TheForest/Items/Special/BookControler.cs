using System;
using System.Collections;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Special
{
	
	public class BookControler : SpecialItemControlerBase
	{
		
		public override bool ToggleSpecial(bool enable)
		{
			if (enable)
			{
				if ((!LocalPlayer.FpCharacter.PushingSled && !LocalPlayer.WaterViz.InWater && !LocalPlayer.AnimControl.holdingGirl && !LocalPlayer.AnimControl.WaterBlock) || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory)
				{
					if (!LocalPlayer.FpCharacter.Grounded)
					{
						base.StartCoroutine(this.openBookAirborneRoutine());
					}
					else if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory && LocalPlayer.AnimControl.bookHeldGo.activeSelf)
					{
						LocalPlayer.Create.Invoke("OpenBook", 0.6f);
						this._openBookCoolDown = Time.time + 0.2f;
					}
					else
					{
						LocalPlayer.Create.OpenBook();
						this._openBookCoolDown = Time.time + 0.2f;
					}
				}
			}
			else if (Time.time > this._openBookCoolDown)
			{
				LocalPlayer.Create.CloseTheBook(false);
			}
			return true;
		}

		
		protected override bool CurrentViewTest()
		{
			return LocalPlayer.Inventory.CurrentView > PlayerInventory.PlayerViews.Loading && (LocalPlayer.Inventory.CurrentView < PlayerInventory.PlayerViews.Pause || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlayerList) && !LocalPlayer.Inventory.IsOpenningInventory;
		}

		
		protected override void OnActivating()
		{
			if (!LocalPlayer.Animator.GetBool("drawBowBool"))
			{
				this.ToggleSpecial(true);
			}
		}

		
		protected override void OnDeactivating()
		{
			this.ToggleSpecial(false);
		}

		
		private IEnumerator openBookAirborneRoutine()
		{
			if (this._doingOpenBookAirborne)
			{
				this._timer = 0f;
				yield break;
			}
			this._doingOpenBookAirborne = true;
			while (this._timer < 0.5f)
			{
				if (LocalPlayer.FpCharacter.Grounded)
				{
					this._timer = 1f;
				}
				this._timer += Time.deltaTime;
				yield return null;
			}
			if ((LocalPlayer.FpCharacter.Grounded && !LocalPlayer.FpCharacter.PushingSled && !LocalPlayer.WaterViz.InWater && !LocalPlayer.AnimControl.holdingGirl && !LocalPlayer.AnimControl.WaterBlock) || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory)
			{
				if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory && LocalPlayer.AnimControl.bookHeldGo.activeSelf)
				{
					LocalPlayer.Create.Invoke("OpenBook", 0.6f);
					this._openBookCoolDown = Time.time + 0.2f;
				}
				else
				{
					LocalPlayer.Create.OpenBook();
					this._openBookCoolDown = Time.time + 0.2f;
				}
			}
			this._doingOpenBookAirborne = false;
			this._timer = 0f;
			yield break;
		}

		
		
		protected override bool IsActive
		{
			get
			{
				return LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book;
			}
		}

		
		public SurvivalBook _book;

		
		private float _openBookCoolDown;

		
		private bool _doingOpenBookAirborne;

		
		private float _timer;
	}
}
