using System;
using TheForest.Tools;
using TheForest.Utils;

namespace TheForest.SerializableTaskSystem
{
	[Serializable]
	public class StoryCondition : ACondition
	{
		public override void Init()
		{
			EventRegistry.Player.Subscribe(TfEvent.StoryProgress, new EventRegistry.SubscriberCallback(this.OnStoryProgress));
		}

		public override void Clear()
		{
			EventRegistry.Player.Unsubscribe(TfEvent.StoryProgress, new EventRegistry.SubscriberCallback(this.OnStoryProgress));
			base.Clear();
		}

		public virtual void OnStoryProgress(object o)
		{
			if (!this._done)
			{
				GameStats.StoryElements storyElements = (GameStats.StoryElements)o;
				if (storyElements == this._type)
				{
					this.SetDone();
					this.Clear();
				}
			}
		}

		public GameStats.StoryElements _type;
	}
}
