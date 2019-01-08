using System;
using UnityEngine;

namespace TheForest.Buildings.World
{
	[DoNotSerializePublic]
	public class MultiItemHolderSlotProxy : MonoBehaviour
	{
		private void GrabEnter()
		{
			this._holder.Current = this._contentId;
			this._holder.GrabEnter();
		}

		private void GrabExit()
		{
			if (this._holder.Current == this._contentId)
			{
				this._holder.GrabExit();
			}
		}

		public MultiItemHolder _holder;

		public int _contentId;
	}
}
