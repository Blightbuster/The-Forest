using System;
using UnityEngine;

namespace TheForest.Player
{
	public class StoryCluesFolderActivator : MonoBehaviour
	{
		private void Awake()
		{
			this._target.Init();
		}

		public StoryCluesFolder _target;
	}
}
