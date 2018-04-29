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

		
		private IEnumerator CheckHoveredOverride()
		{
			yield return null;
			yield return null;
			yield return null;
			if (TheForest.Utils.Input.IsGamePad && base.gameObject.activeSelf && base.GetComponent<Collider>().enabled && (!VirtualCursor.Instance.IsVisibleSoftwareCursorActive || VirtualCursor.Instance._cursorType == VirtualCursor.CursorTypes.None))
			{
				UICamera.currentScheme = UICamera.ControlScheme.Controller;
				if (this.ObjectToBeSelected)
				{
					UICamera.controllerNavigationObject = this.ObjectToBeSelected;
					this.ObjectToBeSelected = null;
				}
				else
				{
					UICamera.controllerNavigationObject = base.gameObject;
				}
			}
			yield break;
		}

		
		public GameObject ObjectToBeSelected;
	}
}
