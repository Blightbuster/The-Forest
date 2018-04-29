﻿using System;
using TheForest.Tools;

namespace TheForest.SerializableTaskSystem
{
	
	[Serializable]
	public class EnemyContactCondition : ACondition
	{
		
		public override void Init()
		{
			EventRegistry.Enemy.Subscribe(TfEvent.EnemyContact, new EventRegistry.SubscriberCallback(this.OnEnemyContact));
		}

		
		public override void Clear()
		{
			EventRegistry.Enemy.Unsubscribe(TfEvent.EnemyContact, new EventRegistry.SubscriberCallback(this.OnEnemyContact));
			base.Clear();
		}

		
		public void OnEnemyContact(object o)
		{
			if (!this._done)
			{
				if (this._type != EnemyType.none)
				{
					if (this._type == (EnemyType)((int)o))
					{
						this.SetDone();
						this.Clear();
					}
				}
				else
				{
					this.SetDone();
					this.Clear();
				}
			}
		}

		
		public EnemyType _type = EnemyType.none;
	}
}
