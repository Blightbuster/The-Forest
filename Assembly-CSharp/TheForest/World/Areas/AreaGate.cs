using System;
using UnityEngine;

namespace TheForest.World.Areas
{
	public class AreaGate : MonoBehaviour
	{
		private void OnTriggerExit(Collider other)
		{
			if (other.CompareTag(this._triggerTag))
			{
				bool flag = Vector3.Dot(base.transform.forward, other.transform.position - base.transform.position) > 0f;
				if (flag)
				{
					if (this._backArea)
					{
						this._backArea.OnLeave(this._forwardArea);
					}
					if (this._forwardArea)
					{
						this._forwardArea.OnEnter(this._backArea);
					}
				}
				else
				{
					if (this._forwardArea)
					{
						this._forwardArea.OnLeave(this._backArea);
					}
					if (this._backArea)
					{
						this._backArea.OnEnter(this._forwardArea);
					}
				}
			}
		}

		public string _triggerTag;

		public Area _forwardArea;

		public Area _backArea;
	}
}
