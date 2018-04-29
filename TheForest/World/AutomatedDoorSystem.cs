using System;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.World
{
	
	public class AutomatedDoorSystem : MonoBehaviour
	{
		
		private void Awake()
		{
			base.enabled = false;
			this.UpdateDoors();
		}

		
		private void Update()
		{
			float num = Time.deltaTime * (1f / this._duration);
			if (this._locked || this._state < AutomatedDoorSystem.State.Opened)
			{
				this._alpha -= num;
				if (this._alpha <= 0f)
				{
					this._alpha = 0f;
					this.SetState(AutomatedDoorSystem.State.Closed);
					base.enabled = false;
				}
			}
			else
			{
				this._alpha += num;
				if (this._alpha >= 1f)
				{
					this._alpha = 1f;
					this.SetState(AutomatedDoorSystem.State.Opened);
					base.enabled = false;
				}
			}
			this.UpdateDoors();
		}

		
		private void OnTriggerEnter(Collider other)
		{
			if (!this._manual && !this._locked && this._state < AutomatedDoorSystem.State.Opened)
			{
				foreach (string tag in this._tags)
				{
					if (other.CompareTag(tag))
					{
						this.StartOpening();
						break;
					}
				}
			}
		}

		
		private void OnTriggerExit(Collider other)
		{
			if (!this._manual && this._state > AutomatedDoorSystem.State.Closing)
			{
				foreach (string tag in this._tags)
				{
					if (other.CompareTag(tag))
					{
						this.StartClosing();
						break;
					}
				}
			}
		}

		
		private void OnDisable()
		{
			this.StopCurrentEvent();
		}

		
		public void Lock()
		{
			this._locked = true;
		}

		
		public void Unlock()
		{
			this._locked = false;
		}

		
		private void UpdateDoors()
		{
			foreach (AutomatedDoorSystem.Door door in this._doors)
			{
				door._door.position = Vector3.Lerp(door._closedPosition.position, door._openedPosition.position, this._alpha);
				door._door.rotation = Quaternion.Slerp(door._closedPosition.rotation, door._openedPosition.rotation, this._alpha);
			}
		}

		
		public void StartOpening()
		{
			this.SetState(AutomatedDoorSystem.State.Opening);
			base.enabled = true;
			this.StartEvent("event:/endgame/sfx_endgame/slide_door_open");
		}

		
		public void StartClosing()
		{
			this.SetState(AutomatedDoorSystem.State.Closing);
			base.enabled = true;
			this.StartEvent("event:/endgame/sfx_endgame/slide_door_close");
		}

		
		private void SetState(AutomatedDoorSystem.State state)
		{
			if (this._state != state)
			{
				this._state = state;
				if (this._stateEvents != null)
				{
					foreach (AutomatedDoorSystem.StateEvent stateEvent in this._stateEvents)
					{
						if (stateEvent._state == state)
						{
							stateEvent._callback.Invoke();
						}
					}
				}
			}
		}

		
		private void StopCurrentEvent()
		{
			if (this._currentEvent != null && this._currentEvent.isValid())
			{
				UnityUtil.ERRCHECK(this._currentEvent.stop(STOP_MODE.ALLOWFADEOUT));
				this._currentEvent = null;
			}
		}

		
		private void StartEvent(string path)
		{
			this.StopCurrentEvent();
			if (FMOD_StudioSystem.instance)
			{
				this._currentEvent = FMOD_StudioSystem.instance.PlayOneShot(path, base.transform.position, null);
			}
		}

		
		private const string OPEN_EVENT = "event:/endgame/sfx_endgame/slide_door_open";

		
		private const string CLOSE_EVENT = "event:/endgame/sfx_endgame/slide_door_close";

		
		public AutomatedDoorSystem.Door[] _doors;

		
		public string[] _tags = new string[]
		{
			"Player"
		};

		
		public AutomatedDoorSystem.State _state;

		
		public float _duration = 0.75f;

		
		public bool _locked;

		
		public bool _manual;

		
		public AutomatedDoorSystem.StateEvent[] _stateEvents;

		
		private float _alpha;

		
		private EventInstance _currentEvent;

		
		[Serializable]
		public class Door
		{
			
			public Transform _door;

			
			public Transform _openedPosition;

			
			public Transform _closedPosition;
		}

		
		public enum State
		{
			
			Closed,
			
			Closing,
			
			Opened,
			
			Opening
		}

		
		[Serializable]
		public class StateEvent
		{
			
			public AutomatedDoorSystem.State _state;

			
			public UnityEvent _callback;
		}
	}
}
