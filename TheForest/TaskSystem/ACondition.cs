using System;

namespace TheForest.TaskSystem
{
	
	[DoNotSerializePublic]
	[Serializable]
	public abstract class ACondition
	{
		
		public abstract void Init();

		
		public virtual void Prepare(Action onStatusChange)
		{
			if (this._allowInMp || !BoltNetwork.isRunning)
			{
				this.OnStatusChange = onStatusChange;
			}
		}

		
		public virtual void Clear()
		{
			this.OnStatusChange = null;
		}

		
		public virtual void SetDone()
		{
			if (!this._done)
			{
				this._done = true;
				if (this.OnStatusChange != null)
				{
					this.OnStatusChange();
				}
				this.OnStatusChange = null;
			}
		}

		
		public virtual void Clone(ACondition other)
		{
			this._done = other._done;
		}

		
		public bool _allowInMp = true;

		
		[SerializeThis]
		public bool _done;

		
		public Action OnStatusChange;
	}
}
