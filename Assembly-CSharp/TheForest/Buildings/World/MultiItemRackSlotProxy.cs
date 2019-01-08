using System;
using UnityEngine;

namespace TheForest.Buildings.World
{
	[DoNotSerializePublic]
	public class MultiItemRackSlotProxy : MonoBehaviour
	{
		private void GrabEnter()
		{
			this._holder.CurrentSlot = this._contentId;
			this._holder.GrabEnter(base.transform);
		}

		private void GrabExit()
		{
			if (this._holder.CurrentSlot == this._contentId)
			{
				this._holder.GrabExit();
			}
		}

		public MultiItemRack _holder;

		public int _contentId;
	}
}
