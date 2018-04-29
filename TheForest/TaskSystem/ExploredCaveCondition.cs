using System;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.TaskSystem
{
	
	[DoNotSerializePublic]
	[Serializable]
	public class ExploredCaveCondition : ACondition
	{
		
		public override void Init()
		{
			if (!LocalPlayer.MapDrawer._caveAreas[Mathf.Clamp(this._caveNumber, 0, LocalPlayer.MapDrawer._caveAreas.Count - 1)]._completed)
			{
				EventRegistry.Player.Subscribe(TfEvent.ExploredCaveArea, new EventRegistry.SubscriberCallback(this.ExploredCaveArea));
			}
			else
			{
				this.SetDone();
				this.Clear();
			}
		}

		
		public override void Clear()
		{
			EventRegistry.Player.Unsubscribe(TfEvent.ExploredCaveArea, new EventRegistry.SubscriberCallback(this.ExploredCaveArea));
			base.Clear();
		}

		
		public virtual void ExploredCaveArea(object o)
		{
			if (!this._done)
			{
				int num = (int)o;
				if (num == this._caveNumber)
				{
					this.SetDone();
					this.Clear();
				}
			}
		}

		
		[SerializeThis]
		public int _caveNumber = -1;
	}
}
