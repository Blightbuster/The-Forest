using System;
using UnityEngine;

namespace Ceto.Common.Unity.Utility
{
	public class DisableShadows : MonoBehaviour
	{
		private void Start()
		{
		}

		private void OnPreRender()
		{
			this.storedShadowDistance = QualitySettings.shadowDistance;
			QualitySettings.shadowDistance = 0f;
		}

		private void OnPostRender()
		{
			QualitySettings.shadowDistance = this.storedShadowDistance;
		}

		private float storedShadowDistance;
	}
}
