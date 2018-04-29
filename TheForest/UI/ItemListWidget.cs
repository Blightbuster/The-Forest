using System;
using System.Collections.Generic;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	
	public class ItemListWidget : MonoBehaviour
	{
		
		public void ShowSingle(int itemId, Transform follow = null, SideIcons sideIcon = SideIcons.None)
		{
			this.ShowSingle(itemId, itemId, follow, sideIcon);
		}

		
		public void ShowSingle(int itemId, int amountItemId, Transform follow = null, SideIcons sideIcon = SideIcons.None)
		{
			bool useFillSprite = false;
			if (this._rotateIcon.activeSelf)
			{
				this._rotateIcon.SetActive(false);
			}
			foreach (ItemListWidgetEntry itemListWidgetEntry in this._singleItems)
			{
				if (itemListWidgetEntry == null)
				{
					Debug.LogWarning(base.gameObject.GetFullName() + " null items in SingleItems list - potential bug");
				}
				else
				{
					bool flag = itemListWidgetEntry.MatchItemId(itemId);
					if (itemListWidgetEntry._icon.gameObject.activeSelf != flag)
					{
						itemListWidgetEntry._icon.gameObject.SetActive(flag);
						this._grid.repositionNow = true;
					}
					if (itemListWidgetEntry._takeIcon)
					{
						itemListWidgetEntry._takeIcon.enabled = (flag && sideIcon == SideIcons.Take);
					}
					if (itemListWidgetEntry._craftIcon)
					{
						itemListWidgetEntry._craftIcon.enabled = (flag && sideIcon == SideIcons.Craft);
					}
					if (flag)
					{
						useFillSprite = itemListWidgetEntry._useFillSprite;
					}
				}
			}
			foreach (ItemListWidgetEntry itemListWidgetEntry2 in this._rotationItems)
			{
				bool flag = itemListWidgetEntry2.MatchItemId(itemId);
				if (itemListWidgetEntry2._icon.gameObject.activeSelf != flag)
				{
					itemListWidgetEntry2._icon.gameObject.SetActive(flag);
					this._grid.repositionNow = true;
				}
				if (this._checkOwnership && this._labelShowsAmount && itemListWidgetEntry2._amountLabel)
				{
					if (sideIcon == SideIcons.Craft)
					{
						int num = LocalPlayer.Inventory.AmountOf(amountItemId, this._allowOwnershipFallback);
						if (num != itemListWidgetEntry2._lastAmountDisplayed)
						{
							itemListWidgetEntry2._lastAmountDisplayed = num;
							itemListWidgetEntry2._amountLabel.text = "x" + num;
						}
						itemListWidgetEntry2._amountLabel.enabled = true;
					}
					else
					{
						itemListWidgetEntry2._amountLabel.enabled = false;
					}
				}
				if (itemListWidgetEntry2._takeIcon)
				{
					itemListWidgetEntry2._takeIcon.enabled = (flag && sideIcon == SideIcons.Take);
				}
				if (itemListWidgetEntry2._craftIcon)
				{
					itemListWidgetEntry2._craftIcon.enabled = (flag && sideIcon == SideIcons.Craft);
				}
				if (flag)
				{
					if (this._scaleWithDistanceToTargetEntry)
					{
						itemListWidgetEntry2._icon.transform.localScale = Vector3.one;
					}
					itemListWidgetEntry2._icon.alpha = 1f;
					useFillSprite = itemListWidgetEntry2._useFillSprite;
				}
			}
			if (this._noValidOptionfallbackIcon && this._noValidOptionfallbackIcon.activeSelf)
			{
				this._noValidOptionfallbackIcon.SetActive(false);
			}
			this.UpdateMasterActionIcon(sideIcon, useFillSprite);
			if (this._follow)
			{
				this._follow._target = follow;
			}
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
				this._grid.repositionNow = true;
			}
		}

		
		public void ShowList(int itemId, Transform follow = null, SideIcons sideIcon = SideIcons.None)
		{
			this.ShowList(itemId, itemId, follow, sideIcon);
		}

		
		public void ShowList(int itemId, int amountItemId = -1, Transform follow = null, SideIcons sideIcon = SideIcons.None)
		{
			int num = 0;
			bool useFillSprite = false;
			int num2 = 0;
			bool flag = false;
			foreach (ItemListWidgetEntry itemListWidgetEntry in this._rotationItems)
			{
				if (itemListWidgetEntry.MatchItemId(itemId))
				{
					flag = true;
					break;
				}
				num2++;
			}
			if (!flag && (this._missingItems == null || !this._missingItems.Contains(itemId)))
			{
				if (this._missingItems == null)
				{
					this._missingItems = new List<int>();
				}
				this._missingItems.Add(itemId);
				Debug.LogError(string.Concat(new string[]
				{
					"Could not find icon for \"",
					ItemDatabase.ItemById(itemId)._name,
					"\" on \"",
					base.gameObject.GetFullName(),
					"\""
				}));
			}
			if (this._lastTargetedListEntry >= 0 && ((this._follow && this._follow._target != follow) || (this._checkOwnership && !this._rotationItems[this._lastTargetedListEntry].OwnsAny())))
			{
				this._lastTargetedListEntry = int.MinValue;
			}
			if (this._lastTargetedListEntry >= 0)
			{
				int num3 = 1;
				if (this._checkOwnership)
				{
					int num4 = (this._lastTargetedListEntry + num3) % this._rotationItems.Length;
					while (!this._rotationItems[num4].OwnsAny())
					{
						if (++num3 == this._rotationItems.Length)
						{
							num3 = 1;
							break;
						}
						num4 = (this._lastTargetedListEntry + num3) % this._rotationItems.Length;
					}
				}
				if ((this._lastTargetedListEntry + num3) % this._rotationItems.Length == num2)
				{
					this._lastTargetChangeTime = Time.time;
					this._prevTargetedListEntry = this._lastTargetedListEntry;
					this._lastTargetedListEntry = int.MinValue;
					this._fixPrevTargetedEntryPosition = true;
				}
			}
			float num5 = Time.time - this._lastTargetChangeTime;
			if (num5 < this._leavingDuration && this._showLeavingEntry)
			{
				this._grid.transform.localPosition = Vector3.Lerp(Vector3.zero, this._leavingEntryLerpTo, num5 / this._leavingDuration);
				num2 = this._prevTargetedListEntry;
				this._maxDisplayed = 4;
			}
			else if (num5 < 1.5f)
			{
				if (this._fixPrevTargetedEntryPosition)
				{
					this._fixPrevTargetedEntryPosition = false;
					if (this._grid.maxPerLine > 1)
					{
						this._rotationItems[this._prevTargetedListEntry]._icon.transform.localPosition -= new Vector3(0f, this._grid.cellHeight * 3f, 0f);
					}
					else
					{
						this._rotationItems[this._prevTargetedListEntry]._icon.transform.localPosition += new Vector3(this._grid.cellWidth * 3f, 0f, 0f);
					}
					this._grid.transform.localPosition = Vector3.zero;
				}
				if (this._maxDisplayed == 4)
				{
					if (this._rotationItems[this._prevTargetedListEntry]._icon.gameObject.activeSelf)
					{
						this._rotationItems[this._prevTargetedListEntry]._icon.gameObject.SetActive(false);
					}
					this._grid.Reposition();
				}
				this._maxDisplayed = 3;
			}
			else
			{
				this._maxDisplayed = 1;
			}
			if (num2 < this._rotationItems.Length)
			{
				int num6 = 0;
				int num7 = num2;
				int num8 = this._rotationItems.Length;
				while (num8-- > 0)
				{
					bool flag2;
					if (num6 < 4 && (!this._checkOwnership || this._rotationItems[num7].OwnsAny()))
					{
						num++;
						flag2 = (num6 < this._maxDisplayed);
					}
					else
					{
						flag2 = false;
					}
					if (flag2)
					{
						bool flag3 = num6 == ((this._maxDisplayed != 4) ? 0 : 1);
						if (flag3)
						{
							useFillSprite = this._rotationItems[num7]._useFillSprite;
							this._lastTargetedListEntry = num7;
							if (this._checkOwnership && this._labelShowsAmount && this._rotationItems[num7]._amountLabel)
							{
								int num9 = LocalPlayer.Inventory.AmountOf(amountItemId, this._allowOwnershipFallback);
								if (num9 != this._rotationItems[num7]._lastAmountDisplayed)
								{
									this._rotationItems[num7]._lastAmountDisplayed = num9;
									this._rotationItems[num7]._amountLabel.text = "x" + num9;
								}
							}
						}
						if (this._rotationItems[num7]._takeIcon)
						{
							this._rotationItems[num7]._takeIcon.enabled = (flag3 && sideIcon == SideIcons.Take);
						}
						if (this._rotationItems[num7]._craftIcon)
						{
							this._rotationItems[num7]._craftIcon.enabled = (flag3 && sideIcon == SideIcons.Craft);
						}
						if (this._rotationItems[num7]._amountLabel && this._rotationItems[num7]._amountLabel.enabled != flag3)
						{
							this._rotationItems[num7]._amountLabel.enabled = flag3;
						}
						if (this._rotationItems[num7]._actionIcon && this._rotationItems[num7]._actionIcon.activeSelf != flag3)
						{
							this._rotationItems[num7]._actionIcon.SetActive(flag3);
							this._grid.repositionNow = true;
						}
						if (!this._rotationItems[num7]._icon.gameObject.activeSelf)
						{
							this._rotationItems[num7]._icon.gameObject.SetActive(true);
							this._grid.repositionNow = true;
						}
						if (this._scaleWithDistanceToTargetEntry)
						{
							float num10 = (float)num6;
							if (num5 < this._leavingDuration && this._showLeavingEntry && num6 > 0)
							{
								num10 -= num5 / this._leavingDuration;
							}
							this._rotationItems[num7]._icon.transform.localScale = Vector3.one * (1f - num10 / 3f);
						}
						if (num5 < this._leavingDuration && num7 == this._prevTargetedListEntry)
						{
							this._rotationItems[num7]._icon.alpha = 1f - num5 / this._leavingDuration;
						}
						else if (num5 < 1f || flag3)
						{
							this._rotationItems[num7]._icon.alpha = 1f - (float)(num6 - ((this._maxDisplayed != 4) ? 0 : 1)) / 3f;
						}
						else
						{
							this._rotationItems[num7]._icon.alpha = (1f - (float)num6 / 3f) * (1f - (num5 - 1f) / this._leavingDuration);
						}
						this._rotationItems[num7]._icon.transform.SetSiblingIndex(num6);
						num6++;
					}
					else if (this._rotationItems[num7]._icon.gameObject.activeSelf)
					{
						this._rotationItems[num7]._icon.gameObject.SetActive(false);
						this._grid.repositionNow = true;
					}
					if (++num7 == this._rotationItems.Length)
					{
						num7 = 0;
					}
				}
				if (this._noValidOptionfallbackIcon && this._noValidOptionfallbackIcon.activeSelf != (num == 0))
				{
					this._noValidOptionfallbackIcon.SetActive(num == 0);
				}
			}
			else
			{
				num = 2;
			}
			foreach (ItemListWidgetEntry itemListWidgetEntry2 in this._singleItems)
			{
				if (itemListWidgetEntry2._icon.gameObject.activeSelf)
				{
					itemListWidgetEntry2._icon.gameObject.SetActive(false);
				}
			}
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
				this._grid.repositionNow = true;
			}
			this.UpdateMasterActionIcon(sideIcon, useFillSprite);
			if (this._follow)
			{
				this._follow._target = follow;
			}
			if (this._rotateIcon.activeSelf != num > 1)
			{
				this._rotateIcon.SetActive(num > 1);
			}
		}

		
		public void Shutdown()
		{
			if (base.gameObject.activeSelf)
			{
				this._lastTargetChangeTime = 0f;
				base.gameObject.SetActive(false);
				this._grid.transform.localPosition = Vector3.zero;
				foreach (UITweener uitweener in this._tweeners)
				{
					if (uitweener)
					{
						uitweener.ResetToBeginning();
					}
				}
			}
		}

		
		public void ShowNoValidOption(Transform follow = null)
		{
			if (this._noValidOptionfallbackIcon)
			{
				foreach (ItemListWidgetEntry itemListWidgetEntry in this._rotationItems)
				{
					if (itemListWidgetEntry._icon.gameObject.activeSelf)
					{
						itemListWidgetEntry._icon.gameObject.SetActive(false);
					}
				}
				foreach (ItemListWidgetEntry itemListWidgetEntry2 in this._singleItems)
				{
					if (itemListWidgetEntry2._icon.gameObject.activeSelf)
					{
						itemListWidgetEntry2._icon.gameObject.SetActive(false);
					}
				}
				this.UpdateMasterActionIcon(SideIcons.None, false);
				if (this._rotateIcon.activeSelf)
				{
					this._rotateIcon.SetActive(false);
				}
				if (!this._noValidOptionfallbackIcon.activeSelf)
				{
					this._noValidOptionfallbackIcon.SetActive(true);
				}
				if (!base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(true);
				}
				if (this._follow)
				{
					this._follow._target = follow;
				}
			}
		}

		
		private void UpdateMasterActionIcon(SideIcons sideIcon, bool useFillSprite)
		{
			if (this._masterActionIcon)
			{
				if (sideIcon == SideIcons.None)
				{
					if (this._masterActionIcon.gameObject.activeSelf)
					{
						this._masterActionIcon.gameObject.SetActive(false);
					}
				}
				else
				{
					if (!this._masterActionIcon.gameObject.activeSelf)
					{
						this._masterActionIcon.gameObject.SetActive(true);
					}
					this._masterActionIcon.ChangeAction((sideIcon != SideIcons.Craft) ? InputMappingIcons.Actions.Take : InputMappingIcons.Actions.Craft, useFillSprite);
				}
			}
		}

		
		public UiFollowTarget _follow;

		
		public UIGrid _grid;

		
		public GameObject _rotateIcon;

		
		public GameObject _noValidOptionfallbackIcon;

		
		public ItemListWidgetEntry[] _singleItems;

		
		public ItemListWidgetEntry[] _rotationItems;

		
		public ActionIconUISprite _masterActionIcon;

		
		public UITweener[] _tweeners;

		
		public Vector3 _leavingEntryLerpTo;

		
		public float _leavingDuration = 0.35f;

		
		public bool _showLeavingEntry;

		
		public bool _scaleWithDistanceToTargetEntry;

		
		public bool _checkOwnership = true;

		
		public bool _allowOwnershipFallback;

		
		public bool _labelShowsAmount;

		
		private int _maxDisplayed;

		
		private int _lastAmountDisplayed;

		
		private float _lastTargetChangeTime;

		
		private int _prevTargetedListEntry = int.MinValue;

		
		private int _lastTargetedListEntry = int.MinValue;

		
		private bool _fixPrevTargetedEntryPosition;

		
		private List<int> _missingItems;
	}
}
