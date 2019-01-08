using System;
using UnityEngine;

namespace TheForest.UI
{
	public class SelectSpecificControl : MonoBehaviour
	{
		public void EnableSpecificControl()
		{
			if (this.ObjectToBeSelected)
			{
				this.FirstSelectedControlObject.ObjectToBeSelected = this.ObjectToBeSelected;
			}
		}

		public GameObject ObjectToBeSelected;

		public FirstSelectControl FirstSelectedControlObject;
	}
}
