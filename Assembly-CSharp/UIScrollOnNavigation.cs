using System;
using UnityEngine;

public class UIScrollOnNavigation : MonoBehaviour
{
	private void OnEnable()
	{
		if (this.ResetOnEnable && this.scrollView != null)
		{
			this.scrollView.ResetPosition();
		}
		UICamera.onNavigate = (UICamera.KeyCodeDelegate)Delegate.Combine(UICamera.onNavigate, new UICamera.KeyCodeDelegate(this.UICamera_OnNavigate));
	}

	private void OnDisable()
	{
		UICamera.onNavigate = (UICamera.KeyCodeDelegate)Delegate.Remove(UICamera.onNavigate, new UICamera.KeyCodeDelegate(this.UICamera_OnNavigate));
	}

	private void UICamera_OnNavigate(GameObject aObject, KeyCode key)
	{
		if (aObject != null && aObject.GetComponentInParent<UIScrollView>() == this.scrollView)
		{
			UICamera uicamera = UICamera.FindCameraForLayer(aObject.layer);
			if (uicamera == null)
			{
				return;
			}
			UIWidget componentInChildren = aObject.GetComponentInChildren<UIWidget>();
			Vector3 vector = uicamera.cachedCamera.WorldToViewportPoint(aObject.transform.position);
			float num = (float)componentInChildren.height * 3f / this.scrollView.bounds.size.y;
			if (key == KeyCode.DownArrow && vector.y < this.m_BottomScreenThresholdY)
			{
				this.scrollBar.value += num;
			}
			else if (key == KeyCode.UpArrow && vector.y > this.m_UpperScreenThreadholdY)
			{
				this.scrollBar.value -= num;
			}
		}
	}

	public UIScrollView scrollView;

	public UIScrollBar scrollBar;

	public bool ResetOnEnable;

	public float m_BottomScreenThresholdY;

	public float m_UpperScreenThreadholdY;
}
