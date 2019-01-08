using System;
using TheForest.Player;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Tools
{
	public class PlayerCompletedStoryTester : MonoBehaviour
	{
		public void DoCompletedStoryTest()
		{
			if (AccountInfo.StoryCompleted)
			{
				this._completedCallback.Invoke();
			}
			else
			{
				this._notCompletedCallback.Invoke();
			}
		}

		public UnityEvent _completedCallback;

		public UnityEvent _notCompletedCallback;
	}
}
