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
				foreach (GameObject go in this._refreshOnStart)
				{
					go.SetActive(false);
				}
			}
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			if (this._refreshOnStart != null)
			{
				foreach (GameObject go2 in this._refreshOnStart)
				{
					go2.SetActive(true);
				}
			}
			yield break;
		}

		
		public GameObject[] _refreshOnStart;

		
		public UIPlayTween _forwardTweener;

		
		public UIPlayTween _backwardTweener;

		
		public UIPlayTween _controlSettingsForwardTweener;

		
		public UIPlayTween _controlSettingsBackwardTweener;
	}
}
