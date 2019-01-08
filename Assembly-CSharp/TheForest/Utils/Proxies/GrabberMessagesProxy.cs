using System;
using UnityEngine;

namespace TheForest.Utils.Proxies
{
	public class GrabberMessagesProxy : MonoBehaviour
	{
		private void GrabEnter()
		{
			if (this._target)
			{
				this._target.enabled = true;
			}
		}

		private void GrabExit()
		{
			if (this._target)
			{
				this._target.enabled = false;
			}
		}

		public MonoBehaviour _target;
	}
}
