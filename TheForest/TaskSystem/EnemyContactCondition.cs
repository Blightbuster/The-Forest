using System;
using TheForest.Tools;

namespace TheForest.TaskSystem
{
	
	[DoNotSerializePublic]
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
					if (this._type == (EnemyType)o)
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
