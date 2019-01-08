using System;
using UnityEngine;

namespace TheForest.UI
{
	public class ItemDualActionWidget : MonoBehaviour
	{
		public void Show(bool takeIcon, bool craftIcon, Transform follow = null)
		{
			if (takeIcon)
			{
				if (!this._takeActionIcon.gameObject.activeSelf)
				{
					this._takeActionIcon.gameObject.SetActive(true);
				}
			}
			else if (this._takeActionIcon.gameObject.activeSelf)
			{
				this._takeActionIcon.gameObject.SetActive(false);
				if (this._takeActionTweener)
				{
					this._takeActionTweener.ResetToBeginning();
					this._takeActionTweener.enabled = false;
				}
			}
			if (craftIcon)
			{
				if (!this._craftActionIcon.gameObject.activeSelf)
				{
					this._craftActionIcon.gameObject.SetActive(true);
				}
			}
			else if (this._craftActionIcon.gameObject.activeSelf)
			{
				this._craftActionIcon.gameObject.SetActive(false);
				if (this._craftActionTweener)
				{
					this._craftActionTweener.ResetToBeginning();
					this._craftActionTweener.enabled = false;
				}
			}
			if (this._follow)
			{
				this._follow._target = follow;
			}
			if (base.gameObject.activeSelf != (takeIcon || craftIcon))
			{
				base.gameObject.SetActive(takeIcon || craftIcon);
			}
		}

		public void ShutDown()
		{
			this.Show(false, false, null);
		}

		public UiFollowTarget _follow;

		public ActionIconUISprite _takeActionIcon;

		public UITweener _takeActionTweener;

		public ActionIconUISprite _craftActionIcon;

		public UITweener _craftActionTweener;
	}
}
