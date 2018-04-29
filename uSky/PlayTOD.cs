using System;
using UnityEngine;

namespace uSky
{
	
	[RequireComponent(typeof(uSkyManager))]
	[AddComponentMenu("uSky/Play TOD")]
	public class PlayTOD : MonoBehaviour
	{
		
		
		private uSkyManager uSM
		{
			get
			{
				if (this.m_uSM == null)
				{
					this.m_uSM = base.gameObject.GetComponent<uSkyManager>();
					if (this.m_uSM == null)
					{
						Debug.Log("Can't not find uSkyManager");
					}
				}
				return this.m_uSM;
			}
		}

		
		private void Start()
		{
			if (this.PlayTimelapse)
			{
				this.uSM.SkyUpdate = true;
			}
		}

		
		private void Update()
		{
			if (this.PlayTimelapse)
			{
				this.uSM.Timeline = this.uSM.Timeline + Time.deltaTime * this.PlaySpeed;
			}
		}

		
		public bool PlayTimelapse = true;

		
		public float PlaySpeed = 0.1f;

		
		private uSkyManager m_uSM;
	}
}
