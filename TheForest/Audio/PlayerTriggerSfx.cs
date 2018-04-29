using System;
using UnityEngine;

namespace TheForest.Audio
{
	
	public class PlayerTriggerSfx : SfxInfo
	{
		
		private void OnCollisionEnter(Collision other)
		{
			if (!this._playedEvent && other.gameObject.CompareTag("Player") && other.relativeVelocity.sqrMagnitude > 25f)
			{
				this._playedEvent = true;
				Sfx.Play(this, base.transform, true);
				base.Invoke("Reset", (float)this._resetDelay);
			}
		}

		
		private void Reset()
		{
			this._playedEvent = false;
		}

		
		private int _resetDelay = 1;

		
		private bool _playedEvent;
	}
}
