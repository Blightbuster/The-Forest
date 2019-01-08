using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.Utils
{
	public class FakeParentCleanup : MonoBehaviour
	{
		public void AddFakeParentComponent(FakeParent component)
		{
			if (this._fakeParents == null)
			{
				this._fakeParents = new List<FakeParent>();
			}
			if (this._fakeParents.Contains(component))
			{
				return;
			}
			this._fakeParents.Add(component);
		}

		private void OnDestroy()
		{
			if (this._fakeParents == null)
			{
				return;
			}
			foreach (FakeParent fakeParent in this._fakeParents)
			{
				if (!(fakeParent == null))
				{
					fakeParent.Dispose();
				}
			}
			this._fakeParents = null;
		}

		[SerializeField]
		private List<FakeParent> _fakeParents = new List<FakeParent>();
	}
}
