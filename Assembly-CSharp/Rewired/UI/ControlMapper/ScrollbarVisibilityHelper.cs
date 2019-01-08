using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper
{
	[AddComponentMenu("")]
	public class ScrollbarVisibilityHelper : MonoBehaviour
	{
		private Scrollbar hScrollBar
		{
			get
			{
				return (!(this.scrollRect != null)) ? null : this.scrollRect.horizontalScrollbar;
			}
		}

		private Scrollbar vScrollBar
		{
			get
			{
				return (!(this.scrollRect != null)) ? null : this.scrollRect.verticalScrollbar;
			}
		}

		private void Awake()
		{
			if (this.scrollRect != null)
			{
				this.target = this.scrollRect.gameObject.AddComponent<ScrollbarVisibilityHelper>();
				this.target.onlySendMessage = true;
				this.target.target = this;
			}
		}

		private void OnRectTransformDimensionsChange()
		{
			if (this.onlySendMessage)
			{
				if (this.target != null)
				{
					this.target.ScrollRectTransformDimensionsChanged();
				}
			}
			else
			{
				this.EvaluateScrollbar();
			}
		}

		private void ScrollRectTransformDimensionsChanged()
		{
			this.OnRectTransformDimensionsChange();
		}

		private void EvaluateScrollbar()
		{
			if (this.scrollRect == null)
			{
				return;
			}
			if (this.vScrollBar == null && this.hScrollBar == null)
			{
				return;
			}
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			Rect rect = this.scrollRect.content.rect;
			Rect rect2 = (this.scrollRect.transform as RectTransform).rect;
			if (this.vScrollBar != null)
			{
				bool value = rect.height > rect2.height;
				this.SetActiveDeferred(this.vScrollBar.gameObject, value);
			}
			if (this.hScrollBar != null)
			{
				bool value2 = rect.width > rect2.width;
				this.SetActiveDeferred(this.hScrollBar.gameObject, value2);
			}
		}

		private void SetActiveDeferred(GameObject obj, bool value)
		{
			base.StopAllCoroutines();
			base.StartCoroutine(this.SetActiveCoroutine(obj, value));
		}

		private IEnumerator SetActiveCoroutine(GameObject obj, bool value)
		{
			yield return null;
			if (obj != null)
			{
				obj.SetActive(value);
			}
			yield break;
		}

		public ScrollRect scrollRect;

		private bool onlySendMessage;

		private ScrollbarVisibilityHelper target;
	}
}
