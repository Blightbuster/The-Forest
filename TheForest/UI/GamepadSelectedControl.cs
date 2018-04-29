using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	
	public class GamepadSelectedControl : MonoBehaviour
	{
		
		private void Update()
		{
			if (TheForest.Utils.Input.IsGamePad && !VirtualCursor.Instance.OverridingPosition)
			{
				if (UICamera.controller != null && UICamera.controller.current != this._currentTarget)
				{
					this._speed = Mathf.Max(this._speed, 0.1f);
					this._currentTarget = ((!UICamera.controller.current || UICamera.controller.current.GetComponent<UIPanel>()) ? null : UICamera.controller.current);
					if (this._currentTarget)
					{
						this._currentTargetCol = this._currentTarget.GetComponent<Collider>();
						this._cursor.gameObject.SetActive(true);
					}
					else if (this._cursor.gameObject.activeSelf)
					{
						this._cursor.gameObject.SetActive(false);
					}
				}
			}
			else if (this._cursor.gameObject.activeSelf)
			{
				this._cursor.gameObject.SetActive(false);
			}
			if (this._currentTarget)
			{
				if (this._currentTargetCol)
				{
					if (this._currentTargetCol is BoxCollider)
					{
						this._cursor.position = this._currentTarget.transform.TransformPoint(((BoxCollider)this._currentTargetCol).center) + this._offset;
					}
					else if (this._currentTargetCol is SphereCollider)
					{
						this._cursor.position = this._currentTarget.transform.TransformPoint(((SphereCollider)this._currentTargetCol).center) + this._offset;
					}
				}
				else
				{
					this._cursor.position = this._currentTarget.transform.position + this._offset;
				}
			}
		}

		
		public Transform _cursor;

		
		public Vector3 _offset = new Vector3(0f, -0.025f, 0f);

		
		private GameObject _currentTarget;

		
		private Collider _currentTargetCol;

		
		private float _speed;
	}
}
