using System;
using System.Collections;
using UnityEngine;

namespace TheForest.Utils
{
	public class DisableAfterDelay : MonoBehaviour
	{
		private void OnEnable()
		{
			base.StartCoroutine(this.Disable());
		}

		private IEnumerator Disable()
		{
			yield return new WaitForSeconds(this.delay);
			base.gameObject.SetActive(false);
			yield break;
		}

		private void OnDisable()
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(false);
			}
		}

		public float delay;
	}
}
