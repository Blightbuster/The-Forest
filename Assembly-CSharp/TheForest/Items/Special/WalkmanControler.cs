using System;
using System.Collections;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Special
{
	[DoNotSerializePublic]
	public class WalkmanControler : SpecialItemControlerBase
	{
		private void Awake()
		{
			WalkmanControler._instance = this;
		}

		protected override void Update()
		{
			if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._walkmanId) && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
			{
				if (TheForest.Utils.Input.GetButtonDown("Fire1"))
				{
					LocalPlayer.Animator.SetBool("attack", true);
					this.ToggleSpecial(true);
				}
				else if (TheForest.Utils.Input.GetButtonUp("Fire1"))
				{
					LocalPlayer.Animator.SetBool("attack", false);
				}
			}
		}

		private void OnDestroy()
		{
			if (WalkmanControler._instance == this)
			{
				WalkmanControler._instance = null;
			}
		}

		public override bool ToggleSpecial(bool enable)
		{
			if (enable && !distractionDevice.IsActive)
			{
				base.StartCoroutine(this.PlayMusicTrack());
			}
			return true;
		}

		public static void LoadCassette(int cassetteId)
		{
			WalkmanControler._instance.LoadCassetteInternal(cassetteId);
		}

		public static void PlayCassette()
		{
			if (!PlayerSfx.MusicPlaying)
			{
				int loadedCassetteTupleIndex = WalkmanControler._instance._loadedCassetteTupleIndex;
				int trackNum = WalkmanControler._instance._cassetteTrackTuples[loadedCassetteTupleIndex]._trackNum;
				LocalPlayer.Sfx.PlayMusicTrack(trackNum);
			}
		}

		private IEnumerator PlayMusicTrack()
		{
			if (this._waitForWalkman)
			{
				yield break;
			}
			this._waitForWalkman = true;
			float waitTime = Time.time + 4f;
			while (this._waitForWalkman)
			{
				if (Time.time > waitTime)
				{
					this._waitForWalkman = false;
					yield break;
				}
				yield return null;
			}
			if (!PlayerSfx.MusicPlaying && this._loadedCassetteTupleIndex >= 0 && this.IsActive)
			{
				LocalPlayer.Sfx.PlayMusicTrack(this._cassetteTrackTuples[this._loadedCassetteTupleIndex]._trackNum);
				base.StartCoroutine("enableSoundDetect");
				if (LocalPlayer.Stats.DaySurvived > (float)this._lastDayUsed)
				{
					this._lastDayUsed = Mathf.FloorToInt(LocalPlayer.Stats.DaySurvived);
					base.StartCoroutine(this.RegenerationRoutine());
				}
			}
			else
			{
				LocalPlayer.Sfx.StopMusic();
				base.StopCoroutine("enableSoundDetect");
				LocalPlayer.ScriptSetup.soundDetectGo.GetComponent<SphereCollider>().radius = 0.5f;
			}
			yield break;
		}

		private IEnumerator RegenerationRoutine()
		{
			yield return null;
			Scene.HudGui.EnergyFlash.SetActive(true);
			while (LocalPlayer.Stats.Energy < 100f && PlayerSfx.MusicPlaying)
			{
				LocalPlayer.Stats.Energy += Time.deltaTime * this._energyPerSecond;
				yield return null;
			}
			Scene.HudGui.EnergyFlash.SetActive(false);
			yield break;
		}

		private IEnumerator enableSoundDetect()
		{
			for (;;)
			{
				LocalPlayer.ScriptSetup.soundDetectGo.SetActive(true);
				yield return YieldPresets.WaitOneSecond;
				LocalPlayer.ScriptSetup.soundDetectGo.GetComponent<SphereCollider>().radius = 40f;
				yield return YieldPresets.WaitOneSecond;
				LocalPlayer.ScriptSetup.soundDetectGo.GetComponent<SphereCollider>().radius = 0.5f;
			}
			yield break;
		}

		protected override void OnActivating()
		{
		}

		protected override void OnDeactivating()
		{
		}

		private IEnumerator DelayedStop(bool equipPrevious)
		{
			this.ToggleSpecial(false);
			LocalPlayer.Sfx.PlayItemCustomSfx(this._itemId, true);
			LocalPlayer.Animator.SetBoolReflected("walkmanHeld", false);
			yield return new WaitForSeconds(1f);
			LocalPlayer.Inventory.SkipNextAddItemWoosh = true;
			if (equipPrevious && LocalPlayer.Inventory.EquipmentSlotsPrevious[1] != null)
			{
				LocalPlayer.Inventory.EquipPreviousUtility(false);
			}
			else
			{
				LocalPlayer.Inventory.StashLeftHand();
			}
			yield break;
		}

		private void LoadCassetteInternal(int cassetteId)
		{
			LocalPlayer.Sfx.StopMusic();
			base.StopCoroutine("enableSoundDetect");
			LocalPlayer.ScriptSetup.soundDetectGo.GetComponent<SphereCollider>().radius = 0.5f;
			if (this._loadedCassetteTupleIndex >= 0)
			{
				LocalPlayer.Inventory.AddItem(this._cassetteTrackTuples[this._loadedCassetteTupleIndex]._cassetteItemId, 1, false, false, null);
			}
			this._loadedCassetteTupleIndex = this._cassetteTrackTuples.FindIndex((WalkmanControler.CassetteTrackTuple ctt) => ctt._cassetteItemId == cassetteId);
		}

		public static bool HasCassetteReady
		{
			get
			{
				return WalkmanControler._instance._loadedCassetteTupleIndex >= 0;
			}
		}

		public static int CurrentTrack
		{
			get
			{
				int loadedCassetteTupleIndex = WalkmanControler._instance._loadedCassetteTupleIndex;
				return WalkmanControler._instance._cassetteTrackTuples[loadedCassetteTupleIndex]._trackNum;
			}
		}

		protected override bool IsActive
		{
			get
			{
				return LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._walkmanId);
			}
		}

		private void disableWaitForWalkman()
		{
			this._waitForWalkman = false;
		}

		public WalkmanControler.CassetteTrackTuple[] _cassetteTrackTuples;

		public float _energyPerSecond = 1f;

		public bool _waitForWalkman;

		[SerializeThis]
		private int _loadedCassetteTupleIndex = -1;

		[SerializeThis]
		private int _lastDayUsed = -1;

		private static WalkmanControler _instance;

		[Serializable]
		public class CassetteTrackTuple
		{
			[ItemIdPicker]
			public int _cassetteItemId;

			public int _trackNum;
		}
	}
}
