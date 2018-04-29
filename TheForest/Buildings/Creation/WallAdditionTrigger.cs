using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[DoNotSerializePublic]
	[AddComponentMenu("Buildings/Creation/Wall Addition Trigger")]
	public class WallAdditionTrigger : MonoBehaviour
	{
		
		private void Awake()
		{
			base.enabled = false;
			this._wallChunk = base.GetComponentInParent<WallChunkArchitect>();
		}

		
		private void Update()
		{
			if ((!TheForest.Utils.Input.IsGamePad && TheForest.Utils.Input.GetButtonDown("Rotate")) || (TheForest.Utils.Input.IsGamePad && TheForest.Utils.Input.GetAxisDown("Rotate") > 0f))
			{
				this._wallChunk.ToggleSegmentAddition();
				LocalPlayer.Sfx.PlayWhoosh();
			}
			this._wallChunk.ShowToggleAdditionIcon();
		}

		
		private void OnDisable()
		{
			if (this._wallChunk)
			{
				this._wallChunk.HideToggleAdditionIcon();
			}
		}

		
		private void OnDestroy()
		{
			if (this._wallChunk)
			{
				this._wallChunk.HideToggleAdditionIcon();
			}
		}

		
		private void GrabEnter()
		{
			if (!base.enabled)
			{
				this._wallChunk.ShowToggleAdditionIcon();
				base.enabled = true;
			}
		}

		
		private void GrabExit()
		{
			if (base.enabled)
			{
				this._wallChunk.HideToggleAdditionIcon();
				base.enabled = false;
			}
		}

		
		public int _edgeNum;

		
		public int _segmentNum;

		
		private WallChunkArchitect _wallChunk;
	}
}
