using System;
using Bolt;
using UnityEngine;

namespace TheForest.Networking
{
	
	public class UnfreezeAfter : EntityBehaviour
	{
		
		private void Update()
		{
			if (!this.frozenOnce && this.entity.isAttached && this.entity.isFrozen)
			{
				this.frozenOnce = true;
				Debug.Log("Hell gate first frozen on at " + Time.realtimeSinceStartup + "s");
			}
		}

		
		public override void Attached()
		{
			if (this.entity.isOwner)
			{
				base.Invoke("Unfreeze", this._delay);
			}
		}

		
		private void Unfreeze()
		{
			Debug.Log("Unfreezing (" + Time.realtimeSinceStartup + "s)");
			this.entity.Freeze(false);
		}

		
		public float _delay;

		
		private bool frozenOnce;
	}
}
