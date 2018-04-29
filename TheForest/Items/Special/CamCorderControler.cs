using System;
using System.Collections;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Special
{
	
	[DoNotSerializePublic]
	public class CamCorderControler : SpecialItemControlerBase
	{
		
		private void Awake()
		{
			CamCorderControler._instance = this;
		}

		
		private void OnDestroy()
		{
			if (CamCorderControler._instance == this)
			{
				CamCorderControler._instance = null;
			}
		}

		
		public override bool ToggleSpecial(bool enable)
		{
			if (enable)
			{
				if (!this._videoPlayer.IsReading && !this._videoPlayer.IsPlaying)
				{
					LocalPlayer.Inventory.StashLeftHand();
					base.StartCoroutine("PlayVideoTapeRoutine", YieldPresets.WaitOnePointThreeSeconds);
				}
				return true;
			}
			base.StopCoroutine("PlayVideoTapeRoutine");
			this.StopVideo();
			this.OnDeactivating();
			return false;
		}

		
		public static bool HasLoadedTape()
		{
			return CamCorderControler._instance._loadedTapeIndex > -1;
		}

		
		public static bool IsRewinding()
		{
			return CamCorderControler._instance._rewinding;
		}

		
		public static void LoadTape(int tapeId)
		{
			CamCorderControler._instance.LoadTapeInternal(tapeId);
		}

		
		public static void PlayTape()
		{
			int loadedTapeIndex = CamCorderControler._instance._loadedTapeIndex;
			CamCorderControler._instance._videoPlayer.Play(CamCorderControler._instance._tapesInfo[loadedTapeIndex]._path);
		}

		
		public static void Rewind()
		{
			CamCorderControler._instance._rewinding = true;
			CamCorderControler._instance.StartCoroutine("PlayVideoTapeRoutine", YieldPresets.WaitForEndOfFrame);
		}

		
		public void OnReadTape()
		{
			if (this._rewinding)
			{
				LocalPlayer.Sfx.PlayCamcorderRewindLoop(this._videoPlayer.transform, true);
			}
			else if (this._loadedTapeIndex >= 0)
			{
				LocalPlayer.Sfx.PlayCamcorderLoop(this._videoPlayer.transform, true, this._tapesInfo[this._loadedTapeIndex]._sfxEvent);
			}
		}

		
		public void OnStoppedTape()
		{
			if (this._rewinding)
			{
				LocalPlayer.Sfx.PlayCamcorderRewindLoop(null, false);
			}
			else
			{
				LocalPlayer.Sfx.PlayCamcorderLoop(null, false, null);
			}
		}

		
		private IEnumerator PlayVideoTapeRoutine(YieldInstruction delay)
		{
			yield return delay;
			if (!this._videoPlayer.IsPlaying && this._loadedTapeIndex >= 0 && this.IsActive)
			{
				if (this._rewinding)
				{
					this._videoPlayer.Play(this._rewindClipName);
				}
				else if (!string.IsNullOrEmpty(this._tapesInfo[this._loadedTapeIndex]._path))
				{
					this._videoPlayer.Play(this._tapesInfo[this._loadedTapeIndex]._path);
				}
			}
			while (this._videoPlayer.IsReading || this._videoPlayer.IsPlaying)
			{
				this.OnReadTape();
				yield return null;
			}
			if (this._rewinding)
			{
				this._rewinding = false;
				base.StartCoroutine("PlayVideoTapeRoutine", YieldPresets.WaitForEndOfFrame);
			}
			else
			{
				this.StopVideo();
			}
			yield break;
		}

		
		private void StopVideo()
		{
			if (this._videoPlayer.IsPlaying || this._videoPlayer.IsReading)
			{
				this._videoPlayer.Stop();
			}
		}

		
		protected override void OnActivating()
		{
			if (!LocalPlayer.Animator.GetBool("drawBowBool"))
			{
				LocalPlayer.Inventory.Equip(this._itemId, false);
			}
		}

		
		protected override void OnDeactivating()
		{
			if (LocalPlayer.Animator.GetBool("camCorderHeld") && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
			{
				base.StartCoroutine(this.DelayedStop(false));
			}
		}

		
		private IEnumerator DelayedStop(bool equipPrevious)
		{
			LocalPlayer.Sfx.PlayWhoosh();
			LocalPlayer.Animator.SetBoolReflected("camCorderHeld", false);
			yield return YieldPresets.WaitPointNineSeconds;
			this._videoPlayer.gameObject.SetActive(false);
			yield break;
		}

		
		private void LoadTapeInternal(int tapeId)
		{
			this._videoPlayer.Stop();
			base.StopCoroutine("enableSoundDetect");
			LocalPlayer.ScriptSetup.soundDetectGo.GetComponent<SphereCollider>().radius = 0.5f;
			if (this._loadedTapeIndex >= 0)
			{
				LocalPlayer.Inventory.AddItem(this._tapesInfo[this._loadedTapeIndex]._itemId, 1, false, false, null);
			}
			this._loadedTapeIndex = this._tapesInfo.FindIndex((CamCorderControler.TapeInfo ctt) => ctt._itemId == tapeId);
		}

		
		
		public static bool HasTapeReady
		{
			get
			{
				return CamCorderControler._instance._loadedTapeIndex >= 0;
			}
		}

		
		
		public static int CurrentTrack
		{
			get
			{
				int loadedTapeIndex = CamCorderControler._instance._loadedTapeIndex;
				return CamCorderControler._instance._tapesInfo[loadedTapeIndex]._trackNum;
			}
		}

		
		
		protected override bool IsActive
		{
			get
			{
				return LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._itemId);
			}
		}

		
		public CamCorderControler.TapeInfo[] _tapesInfo;

		
		public VideoStreaming _videoPlayer;

		
		public string _rewindClipName;

		
		[SerializeThis]
		private int _loadedTapeIndex = -1;

		
		private static CamCorderControler _instance;

		
		private bool _rewinding;

		
		[Serializable]
		public class TapeInfo
		{
			
			[ItemIdPicker]
			public int _itemId;

			
			public int _trackNum;

			
			public string _path;

			
			public string _sfxEvent;
		}
	}
}
