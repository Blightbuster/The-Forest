using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	
	public class FirstSelectControl : MonoBehaviour
	{
		
		private void OnEnable()
		{
			if (Scene.ActiveMB)
			{
				Scene.ActiveMB.StartCoroutine(this.CheckHoveredOverride());
			}
			else
			{
				base.StartCoroutine(this.CheckHoveredOverride());
			}
		}

		
		private void OnDisable()
		{
			if (GamepadSelectedControl.Instance && GamepadSelectedControl.Instance._lastFirstSelectControl == this)
			{
				GamepadSelectedControl.Instance._lastFirstSelectControl = null;
			}
		}

		
		private void OnDestroy()
		{
			this.OnDisable();
		}

		
		private IEnumerator CheckHoveredOverride()
		{
			yield return null;
			yield return null;
			yield return null;
			if (GamepadSelectedControl.Instance)
			{
				GamepadSelectedControl.Instance._lastFirstSelectControl = this;
			}
			this.SelectFirstControl();
			yield break;
		}

		
		public void SelectFirstControl()
		{
			if (TheForest.Utils.Input.IsGamePad && base.gameObject.activeSelf)
			{
				Collider component = ((!this.ObjectToBeSelected) ? base.gameObject : this.ObjectToBeSelected).GetComponent<Collider>();
				if (((component && component.enabled) || base.GetComponent<ToggleUIPopupListSelection>()) && (!VirtualCursor.Instance.IsVisibleSoftwareCursorActive || VirtualCursor.Instance._cursorType == VirtualCursor.CursorTypes.None))
				{
					UICamera.currentScheme = UICamera.ControlScheme.Controller;
					if (this.ObjectToBeSelected)
					{
						UICamera.controllerNavigationObject = this.ObjectToBeSelected;
					}
					else
					{
						UICamera.controllerNavigationObject = base.gameObject;
					}
				}
			}
		}

		
		public GameObject ObjectToBeSelected;
	}
}
