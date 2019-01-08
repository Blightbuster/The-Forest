using System;
using Bolt;
using UnityEngine;

namespace TheForest.Networking
{
	public class UnfreezeAfter : EntityBehaviour
	{
		private void Update()
		{
			if (!this.frozenOnce && base.entity.isAttached && base.entity.isFrozen)
			{
				this.frozenOnce = true;
				Debug.Log("Hell gate first frozen on at " + Time.realtimeSinceStartup + "s");
			}
		}

		public override void Attached()
		{
			if (base.entity.isOwner)
			{
				base.Invoke("Unfreeze", this._delay);
			}
		}

		private void Unfreeze()
		{
			Debug.Log("Unfreezing (" + Time.realtimeSinceStartup + "s)");
			base.entity.Freeze(false);
		}

		public float _delay;

		private bool frozenOnce;
	}
}
