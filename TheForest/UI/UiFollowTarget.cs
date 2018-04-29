using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	
	public class UiFollowTarget : MonoBehaviour
	{
		
		private void LateUpdate()
		{
			if (this._target)
			{
				Vector3 position;
				if (this._inBook)
				{
					position = LocalPlayer.MainCam.WorldToViewportPoint(this._target.position) + this._viewportOffsetBook;
				}
				else if (this._inInventory)
				{
					position = LocalPlayer.InventoryCam.WorldToViewportPoint(this._target.position) + this._viewportOffsetBook;
					position.z = position.z * UiFollowTarget.MasterDepthRatio + UiFollowTarget.MasterDepthOffset;
				}
				else
				{
					position = LocalPlayer.MainCam.WorldToViewportPoint(this._target.position + this._worldOffset);
					if (this._target2)
					{
						Vector3 vector = LocalPlayer.MainCam.WorldToViewportPoint(this._target2.position + this._worldOffset);
						if (vector.z > 0.25f && (vector.z < position.z || position.z < 0.5f || position.x < 0f || position.x > 1f || position.y < 0f || position.y > 1f))
						{
							position = vector;
						}
					}
					if (position.z > 0f)
					{
						position.z = Mathf.Clamp(position.z, this._minDepth, this._maxDepth);
					}
					position.z = position.z * UiFollowTarget.MasterDepthRatio + UiFollowTarget.MasterDepthOffset;
				}
				if (this._scaleDepthByMonitorHeight)
				{
					position.z *= (float)Screen.height / this._baseMonitorHeight;
				}
				if (position.z > 0f)
				{
					if (this._inBook)
					{
						base.transform.position = Scene.HudGui.BookCam.ViewportToWorldPoint(position);
					}
					else
					{
						base.transform.position = Scene.HudGui.ActionIconCams.ViewportToWorldPoint(position);
					}
					if (this._followTargetRotationY)
					{
						Transform parent = this._target.parent;
						this._target.parent = LocalPlayer.MainCamTr;
						base.transform.rotation = this._target.localRotation;
						this._target.parent = parent;
					}
				}
				else
				{
					base.transform.position = Scene.HudGui.ActionIconCams.transform.position - Scene.HudGui.ActionIconCams.transform.forward;
				}
			}
		}

		
		public void SetTarget(Transform target, Transform target2 = null)
		{
			this._target = target;
			this._target2 = target2;
			base.enabled = true;
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
			}
		}

		
		public void SetTarget(Transform target, Transform target2, Vector3 worldOffset, float maxDepth = float.PositiveInfinity, float minDepth = 1.45f)
		{
			this.SetTarget(target, target2);
			this._worldOffset = worldOffset;
			this._maxDepth = maxDepth;
			this._minDepth = minDepth;
		}

		
		public void UnsetTarget()
		{
			this._target = null;
			this._target2 = null;
			base.enabled = false;
			this._maxDepth = float.PositiveInfinity;
			this._minDepth = 1.45f;
		}

		
		public void Shutdown()
		{
			this.UnsetTarget();
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(false);
			}
		}

		
		public Transform _target;

		
		public Transform _target2;

		
		public Vector3 _worldOffset;

		
		public float _minDepth = 1.45f;

		
		public float _maxDepth = float.PositiveInfinity;

		
		public Vector3 _viewportOffsetBook;

		
		public float _depthRatioBook = 5f;

		
		public bool _inBook;

		
		public bool _inInventory;

		
		public bool _scaleDepthByMonitorHeight;

		
		public float _baseMonitorHeight;

		
		public bool _followTargetRotationY;

		
		public static float MasterDepthRatio = 1f;

		
		public static float MasterDepthOffset;
	}
}
