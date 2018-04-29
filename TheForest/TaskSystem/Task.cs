using System;

namespace TheForest.TaskSystem
{
	
	[DoNotSerializePublic]
	[Serializable]
	public class Task : ACondition
	{
		
		
		
		public virtual ACondition _availableCondition { get; set; }

		
		
		
		public virtual ACondition _completeCondition { get; set; }

		
		public override void Init()
		{
			if (!this._done)
			{
				if (!this._available)
				{
					if (this._availableCondition != null)
					{
						this._availableCondition.Prepare(new Action(this.SetAvailable));
						this._availableCondition.Init();
					}
				}
				else if (this._completeCondition != null)
				{
					this._completeCondition.Prepare(new Action(this.SetDone));
					this._completeCondition.Init();
				}
			}
		}

		
		public virtual void SetAvailable()
		{
			if (this != null && !this._available)
			{
				this._available = true;
				if (this.OnStatusChange != null)
				{
					this.OnStatusChange();
				}
				this.Init();
			}
		}

		
		public virtual void Clone(Task other)
		{
			this._available = other._available;
			base.Clone(other);
		}

		
		[SerializeThis]
		public bool _available;
	}
}
