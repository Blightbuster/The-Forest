using System;
using TheForest.Items.Special;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	
	public class RewindVideoStreaming : MonoBehaviour
	{
		
		private void Update()
		{
			if (this._video.IsReading || CamCorderControler.IsRewinding() || !CamCorderControler.HasLoadedTape())
			{
				if (Scene.HudGui.RewindCamcorderIcon.activeSelf)
				{
					Scene.HudGui.RewindCamcorderIcon.SetActive(false);
				}
			}
			else if (LocalPlayer.Animator.GetBool("camCorderHeld"))
			{
				if (!Scene.HudGui.RewindCamcorderIcon.activeSelf)
				{
					Scene.HudGui.RewindCamcorderIcon.SetActive(true);
				}
				if (TheForest.Utils.Input.GetButtonDown("Rotate"))
				{
					CamCorderControler.Rewind();
				}
			}
		}

		
		private void OnDisable()
		{
			if (Scene.HudGui.RewindCamcorderIcon.activeSelf)
			{
				Scene.HudGui.RewindCamcorderIcon.SetActive(false);
			}
		}

		
		public VideoStreaming _video;

		
		private bool _rewinding;
	}
}
