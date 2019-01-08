using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper
{
	[AddComponentMenu("")]
	public class UIControl : MonoBehaviour
	{
		public int id
		{
			get
			{
				return this._id;
			}
		}

		private void Awake()
		{
			this._id = UIControl.GetNextUid();
		}

		public bool showTitle
		{
			get
			{
				return this._showTitle;
			}
			set
			{
				if (this.title == null)
				{
					return;
				}
				this.title.gameObject.SetActive(value);
				this._showTitle = value;
			}
		}

		public virtual void SetCancelCallback(Action cancelCallback)
		{
		}

		private static int GetNextUid()
		{
			if (UIControl._uidCounter == 2147483647)
			{
				UIControl._uidCounter = 0;
			}
			int uidCounter = UIControl._uidCounter;
			UIControl._uidCounter++;
			return uidCounter;
		}

		public Text title;

		private int _id;

		private bool _showTitle;

		private static int _uidCounter;
	}
}
