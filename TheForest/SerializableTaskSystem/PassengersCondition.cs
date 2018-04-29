using System;
using TheForest.Tools;
using TheForest.Utils;

namespace TheForest.SerializableTaskSystem
{
	
	[Serializable]
	public class PassengersCondition : ACondition
	{
		
		public override void Init()
		{
			EventRegistry.Player.Subscribe(TfEvent.FoundPassenger, new EventRegistry.SubscriberCallback(this.OnPassengerFound));
		}

		
		public override void Clear()
		{
			EventRegistry.Player.Unsubscribe(TfEvent.FoundPassenger, new EventRegistry.SubscriberCallback(this.OnPassengerFound));
			base.Clear();
		}

		
		public void OnPassengerFound(object o)
		{
			if (!this._done && Scene.GameStats._stats._passengersFound == LocalPlayer.PassengerManifest._foundGOs.Length)
			{
				this.SetDone();
				this.Clear();
			}
		}
	}
}
