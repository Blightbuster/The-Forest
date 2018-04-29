using System;
using TheForest.Commons.Delegates;
using UnityEngine;

namespace TheForest.World
{
	
	public class MaterialSlideShow : MonoBehaviour
	{
		
		private void OnEnable()
		{
			if (this._wsToken == -1 && this._scrollingDelay >= 0f)
			{
				this._wsToken = WorkScheduler.Register(new WsTask(this.CheckNextImage), base.transform.position, true);
			}
			if (this._sfxPosition == null)
			{
				this._sfxPosition = base.transform;
			}
		}

		
		private void OnDisable()
		{
			if (this._wsToken != -1)
			{
				WorkScheduler.Unregister(new WsTask(this.CheckNextImage), this._wsToken);
				this._wsToken = -1;
			}
		}

		
		private void CheckNextImage()
		{
			if (this._lastMaterialNum + 1 < this._slidesMaterials.Length)
			{
				if (Time.time - this._lastSwapTime >= this._scrollingDelay)
				{
					this._lastMaterialNum++;
					this._lastSwapTime = Time.time;
					this._targetRenderer.sharedMaterial = this._slidesMaterials[this._lastMaterialNum];
					FMODCommon.PlayOneshot("event:/endgame/sfx_endgame/projector_click", (!this._sfxPosition) ? base.transform : this._sfxPosition);
				}
			}
			else if (this._restartDelay >= 0f)
			{
				if (Time.time - this._lastSwapTime >= this._restartDelay)
				{
					this._lastMaterialNum = 0;
					this._lastSwapTime = Time.time;
					this._targetRenderer.sharedMaterial = this._slidesMaterials[this._lastMaterialNum];
					FMODCommon.PlayOneshot("event:/endgame/sfx_endgame/projector_click", (!this._sfxPosition) ? base.transform : this._sfxPosition);
				}
			}
			else
			{
				base.enabled = false;
			}
		}

		
		private const string _swapEvent = "event:/endgame/sfx_endgame/projector_click";

		
		public Renderer _targetRenderer;

		
		public Material[] _slidesMaterials;

		
		public float _scrollingDelay = 1f;

		
		public float _restartDelay = 1f;

		
		public Transform _sfxPosition;

		
		private int _wsToken = -1;

		
		private int _lastMaterialNum;

		
		private float _lastSwapTime;
	}
}
