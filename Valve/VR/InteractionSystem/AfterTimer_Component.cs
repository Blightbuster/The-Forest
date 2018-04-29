using System;
using System.Collections;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	[Serializable]
	public class AfterTimer_Component : MonoBehaviour
	{
		
		public void Init(float _time, Action _callback, bool earlydestroy)
		{
			this.triggerTime = _time;
			this.callback = _callback;
			this.triggerOnEarlyDestroy = earlydestroy;
			this.timerActive = true;
			base.StartCoroutine(this.Wait());
		}

		
		private IEnumerator Wait()
		{
			yield return new WaitForSeconds(this.triggerTime);
			this.timerActive = false;
			this.callback();
			UnityEngine.Object.Destroy(this);
			yield break;
		}

		
		private void OnDestroy()
		{
			if (this.timerActive)
			{
				base.StopCoroutine(this.Wait());
				this.timerActive = false;
				if (this.triggerOnEarlyDestroy)
				{
					this.callback();
				}
			}
		}

		
		private Action callback;

		
		private float triggerTime;

		
		private bool timerActive;

		
		private bool triggerOnEarlyDestroy;
	}
}
