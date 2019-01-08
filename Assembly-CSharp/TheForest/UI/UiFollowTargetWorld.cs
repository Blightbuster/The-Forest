using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	public class UiFollowTargetWorld : MonoBehaviour
	{
		private void LateUpdate()
		{
			bool flag = this._uiFollow && this._uiFollow._target && this._uiFollow.gameObject.activeSelf;
			if (flag)
			{
				base.transform.position = this._uiFollow._target.position;
				Vector3 forward = base.transform.position - LocalPlayer.Transform.position;
				if (this._alignWithUpAxis)
				{
					forward.y = 0f;
				}
				forward.Normalize();
				base.transform.rotation = Quaternion.LookRotation(forward);
			}
			if (this._visibilityFilter && this._visibilityFilter.activeSelf != flag)
			{
				this._visibilityFilter.SetActive(flag);
			}
		}

		public UiFollowTarget _uiFollow;

		public GameObject _visibilityFilter;

		public bool _alignWithUpAxis = true;
	}
}
