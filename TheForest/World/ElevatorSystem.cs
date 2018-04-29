using System;
using System.Collections;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.World
{
	
	public class ElevatorSystem : MonoBehaviour
	{
		
		private void Awake()
		{
			this.GrabExit();
			this.ToggleGoArray(this._movingGos, false);
			this.ToggleGoArray(this._idleGos, true);
		}

		
		private void Update()
		{
			if (!this._moving && (!this._door || this._door._state == AutomatedDoorSystem.State.Closed))
			{
				this._sheenBillboardIcon.SetActive(false);
				this._pickupBillboardIcon.SetActive(true);
				if (TheForest.Utils.Input.GetButtonDown("Take"))
				{
					if (this._sequence)
					{
						this._sequence.BeginStage(this._sequenceStage);
					}
					else
					{
						this.GotoRemotePoint();
					}
				}
			}
			else
			{
				this._sheenBillboardIcon.SetActive(false);
				this._pickupBillboardIcon.SetActive(false);
			}
		}

		
		private void GrabEnter()
		{
			if (Vector3.Dot(LocalPlayer.Transform.forward, base.transform.forward) < 0f)
			{
				base.enabled = true;
			}
		}

		
		private void GrabExit()
		{
			base.enabled = false;
			this._sheenBillboardIcon.SetActive(true);
			this._pickupBillboardIcon.SetActive(false);
		}

		
		public void MoveToDownPosition()
		{
			this._rb.transform.position = this._downPosition.position;
			this._rb.transform.rotation = this._downPosition.rotation;
		}

		
		public void MoveToUpPosition()
		{
			this._rb.transform.position = this._upPosition.position;
			this._rb.transform.rotation = this._upPosition.rotation;
		}

		
		public void GotoRemotePoint()
		{
			if (this._useLimit > 0 && this._useCount >= this._useLimit)
			{
				return;
			}
			float num = Vector3.Distance(this._rb.position, this._upPosition.position);
			float num2 = Vector3.Distance(this._rb.position, this._downPosition.position);
			if (num > num2)
			{
				base.StartCoroutine(this.Goto(this._upPosition.position, this._upPosition.rotation));
			}
			else
			{
				base.StartCoroutine(this.Goto(this._downPosition.position, this._downPosition.rotation));
			}
		}

		
		private IEnumerator Goto(Vector3 targetPos, Quaternion targetRotation)
		{
			this._useCount++;
			this._moving = true;
			this.ToggleGoArray(this._movingGos, this._moving);
			this.ToggleGoArray(this._idleGos, !this._moving);
			if (this._playKeycardAnim)
			{
				this._onStartKeycard.Invoke();
				if (!this._sequence || this._sequence.IsActor)
				{
					LocalPlayer.SpecialActions.SendMessage("setKeycardId", this._keycardId);
					LocalPlayer.SpecialActions.SendMessage("setShortSequence", true);
					LocalPlayer.SpecialActions.SendMessage("openDoorRoutine", this._playerPos);
				}
				yield return YieldPresets.WaitFiveSeconds;
				this._onFinishKeycard.Invoke();
			}
			LocalPlayer.ImageEffectOptimizer.SkipMotionBlur = true;
			yield return null;
			GameObject trackerGo = new GameObject("_tracker_");
			trackerGo.transform.position = LocalPlayer.Transform.position;
			trackerGo.transform.rotation = LocalPlayer.Transform.rotation;
			trackerGo.transform.parent = this._rb.transform;
			this._rb.transform.position = targetPos;
			this._rb.transform.rotation = targetRotation;
			LocalPlayer.MainRotator.enabled = false;
			LocalPlayer.Transform.position = trackerGo.transform.position;
			LocalPlayer.Transform.rotation = trackerGo.transform.rotation;
			LocalPlayer.MainRotator.resetOriginalRotation = true;
			LocalPlayer.MainRotator.enabled = true;
			LocalPlayer.PlayerBase.SendMessage("ForcedUpdate");
			yield return YieldPresets.WaitForFixedUpdate;
			LocalPlayer.Transform.position = trackerGo.transform.position;
			UnityEngine.Object.Destroy(trackerGo);
			this._onStartMoving.Invoke();
			yield return null;
			LocalPlayer.ImageEffectOptimizer.SkipMotionBlur = false;
			yield return new WaitForSeconds(this._duration);
			this._moving = false;
			this.ToggleGoArray(this._movingGos, this._moving);
			this.ToggleGoArray(this._idleGos, !this._moving);
			this._onFinishMoving.Invoke();
			yield break;
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

		
		public Rigidbody _rb;

		
		public Transform _upPosition;

		
		public Transform _downPosition;

		
		public float _movementSmoothTime = 2f;

		
		public GameObject _sheenBillboardIcon;

		
		public GameObject _pickupBillboardIcon;

		
		public float _duration = 10f;

		
		public bool _playKeycardAnim;

		
		public Transform _playerPos;

		
		public GameObject[] _movingGos;

		
		public GameObject[] _idleGos;

		
		public UnityEvent _onStartKeycard;

		
		public UnityEvent _onFinishKeycard;

		
		public UnityEvent _onStartMoving;

		
		public UnityEvent _onFinishMoving;

		
		[ItemIdPicker]
		public int _keycardId;

		
		public AnimationSequence _sequence;

		
		public int _sequenceStage;

		
		public TextMesh[] _stateTextDebugReadout;

		
		public int _useLimit;

		
		private int _useCount;

		
		private bool _moving;

		
		[Serializable]
		public class MessageToGo
		{
			
			public GameObject _target;

			
			public string _message;
		}
	}
}
