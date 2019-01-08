using System;
using System.Collections;
using UnityEngine;

namespace TheForest.UI
{
	public class OptionMenuTweens : MonoBehaviour
	{
		private IEnumerator Start()
		{
			yield return null;
			if (this._refreshOnStart != null)
			{
				foreach (GameObject gameObject in this._refreshOnStart)
				{
					gameObject.SetActive(false);
				}
			}
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			if (this._refreshOnStart != null)
			{
				foreach (GameObject gameObject2 in this._refreshOnStart)
				{
					gameObject2.SetActive(true);
				}
			}
			yield break;
		}

		public GameObject[] _refreshOnStart;

		public UIPlayTween[] _forwardTweener;

		public UIPlayTween[] _backwardTweener;

		public bool _centerOnScreen;
	}
}
