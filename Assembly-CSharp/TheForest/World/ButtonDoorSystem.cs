using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.World
{
	public class ButtonDoorSystem : MonoBehaviour
	{
		private bool IsReady
		{
			get
			{
				return (this._useLimit <= 0 || this._useCount < this._useLimit) && (this._door._state == AutomatedDoorSystem.State.Closed || this._door._state == AutomatedDoorSystem.State.Opened);
			}
		}

		private void Awake()
		{
			base.enabled = false;
			this.ToggleGoArray(this._lockedGos, this._door._locked);
			this.ToggleGoArray(this._unlockedGos, !this._door._locked);
		}

		private void Update()
		{
			if (TheForest.Utils.Input.GetButtonDown("Take") && this.IsReady)
			{
				if (this._sequence)
				{
					this._sequence.BeginStage(this._sequenceStage);
				}
				else
				{
					this.ActivateButton();
				}
			}
		}

		private void GrabEnter()
		{
			if (Vector3.Dot(LocalPlayer.Transform.forward, base.transform.forward) < 0f)
			{
				this.ToggleIcons(true);
				base.enabled = true;
			}
		}

		private void GrabExit()
		{
			base.enabled = false;
			this.ToggleIcons(false);
		}

		public void ActivateButton()
		{
			this._useCount++;
			this._alpha = 0f;
			this.ToggleGoArray(this._lockedGos, false);
			this.ToggleGoArray(this._unlockedGos, false);
			this.HideIcons();
			if (this._door._state == AutomatedDoorSystem.State.Closed)
			{
				this._door.StartOpening();
			}
			else
			{
				this._door.StartClosing();
			}
			if (this._unlockDoor)
			{
				this._door._locked = false;
			}
		}

		private void ToggleIcons(bool showPickup)
		{
			this._sheenIcon.SetActive(!showPickup);
			this._pickupIcon.SetActive(showPickup);
		}

		private void HideIcons()
		{
			this._sheenIcon.SetActive(false);
			this._pickupIcon.SetActive(false);
		}

		private void ToggleGoArray(GameObject[] gos, bool active)
		{
			foreach (GameObject gameObject in gos)
			{
				if (gameObject)
				{
					gameObject.SetActive(active);
				}
			}
		}

		public AutomatedDoorSystem _door;

		public GameObject[] _lockedGos;

		public GameObject[] _unlockedGos;

		public GameObject _sheenIcon;

		public GameObject _pickupIcon;

		public float _autoCloseDelay = 20f;

		public bool _unlockDoor = true;

		public AnimationSequence _sequence;

		public int _sequenceStage;

		public int _useLimit;

		private int _useCount;

		private float _alpha;

		private bool _stashKeycard;
	}
}
