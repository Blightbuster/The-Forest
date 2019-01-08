using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public static class SteamVR_Events
{
	public static SteamVR_Events.Action CalibratingAction(UnityAction<bool> action)
	{
		return new SteamVR_Events.Action<bool>(SteamVR_Events.Calibrating, action);
	}

	public static SteamVR_Events.Action DeviceConnectedAction(UnityAction<int, bool> action)
	{
		return new SteamVR_Events.Action<int, bool>(SteamVR_Events.DeviceConnected, action);
	}

	public static SteamVR_Events.Action FadeAction(UnityAction<Color, float, bool> action)
	{
		return new SteamVR_Events.Action<Color, float, bool>(SteamVR_Events.Fade, action);
	}

	public static SteamVR_Events.Action FadeReadyAction(UnityAction action)
	{
		return new SteamVR_Events.ActionNoArgs(SteamVR_Events.FadeReady, action);
	}

	public static SteamVR_Events.Action HideRenderModelsAction(UnityAction<bool> action)
	{
		return new SteamVR_Events.Action<bool>(SteamVR_Events.HideRenderModels, action);
	}

	public static SteamVR_Events.Action InitializingAction(UnityAction<bool> action)
	{
		return new SteamVR_Events.Action<bool>(SteamVR_Events.Initializing, action);
	}

	public static SteamVR_Events.Action InputFocusAction(UnityAction<bool> action)
	{
		return new SteamVR_Events.Action<bool>(SteamVR_Events.InputFocus, action);
	}

	public static SteamVR_Events.Action LoadingAction(UnityAction<bool> action)
	{
		return new SteamVR_Events.Action<bool>(SteamVR_Events.Loading, action);
	}

	public static SteamVR_Events.Action LoadingFadeInAction(UnityAction<float> action)
	{
		return new SteamVR_Events.Action<float>(SteamVR_Events.LoadingFadeIn, action);
	}

	public static SteamVR_Events.Action LoadingFadeOutAction(UnityAction<float> action)
	{
		return new SteamVR_Events.Action<float>(SteamVR_Events.LoadingFadeOut, action);
	}

	public static SteamVR_Events.Action NewPosesAction(UnityAction<TrackedDevicePose_t[]> action)
	{
		return new SteamVR_Events.Action<TrackedDevicePose_t[]>(SteamVR_Events.NewPoses, action);
	}

	public static SteamVR_Events.Action NewPosesAppliedAction(UnityAction action)
	{
		return new SteamVR_Events.ActionNoArgs(SteamVR_Events.NewPosesApplied, action);
	}

	public static SteamVR_Events.Action OutOfRangeAction(UnityAction<bool> action)
	{
		return new SteamVR_Events.Action<bool>(SteamVR_Events.OutOfRange, action);
	}

	public static SteamVR_Events.Action RenderModelLoadedAction(UnityAction<SteamVR_RenderModel, bool> action)
	{
		return new SteamVR_Events.Action<SteamVR_RenderModel, bool>(SteamVR_Events.RenderModelLoaded, action);
	}

	public static SteamVR_Events.Event<VREvent_t> System(EVREventType eventType)
	{
		SteamVR_Events.Event<VREvent_t> @event;
		if (!SteamVR_Events.systemEvents.TryGetValue(eventType, out @event))
		{
			@event = new SteamVR_Events.Event<VREvent_t>();
			SteamVR_Events.systemEvents.Add(eventType, @event);
		}
		return @event;
	}

	public static SteamVR_Events.Action SystemAction(EVREventType eventType, UnityAction<VREvent_t> action)
	{
		return new SteamVR_Events.Action<VREvent_t>(SteamVR_Events.System(eventType), action);
	}

	public static SteamVR_Events.Event<bool> Calibrating = new SteamVR_Events.Event<bool>();

	public static SteamVR_Events.Event<int, bool> DeviceConnected = new SteamVR_Events.Event<int, bool>();

	public static SteamVR_Events.Event<Color, float, bool> Fade = new SteamVR_Events.Event<Color, float, bool>();

	public static SteamVR_Events.Event FadeReady = new SteamVR_Events.Event();

	public static SteamVR_Events.Event<bool> HideRenderModels = new SteamVR_Events.Event<bool>();

	public static SteamVR_Events.Event<bool> Initializing = new SteamVR_Events.Event<bool>();

	public static SteamVR_Events.Event<bool> InputFocus = new SteamVR_Events.Event<bool>();

	public static SteamVR_Events.Event<bool> Loading = new SteamVR_Events.Event<bool>();

	public static SteamVR_Events.Event<float> LoadingFadeIn = new SteamVR_Events.Event<float>();

	public static SteamVR_Events.Event<float> LoadingFadeOut = new SteamVR_Events.Event<float>();

	public static SteamVR_Events.Event<TrackedDevicePose_t[]> NewPoses = new SteamVR_Events.Event<TrackedDevicePose_t[]>();

	public static SteamVR_Events.Event NewPosesApplied = new SteamVR_Events.Event();

	public static SteamVR_Events.Event<bool> OutOfRange = new SteamVR_Events.Event<bool>();

	public static SteamVR_Events.Event<SteamVR_RenderModel, bool> RenderModelLoaded = new SteamVR_Events.Event<SteamVR_RenderModel, bool>();

	private static Dictionary<EVREventType, SteamVR_Events.Event<VREvent_t>> systemEvents = new Dictionary<EVREventType, SteamVR_Events.Event<VREvent_t>>();

	public abstract class Action
	{
		public abstract void Enable(bool enabled);

		public bool enabled
		{
			set
			{
				this.Enable(value);
			}
		}
	}

	[Serializable]
	public class ActionNoArgs : SteamVR_Events.Action
	{
		public ActionNoArgs(SteamVR_Events.Event _event, UnityAction action)
		{
			this._event = _event;
			this.action = action;
		}

		public override void Enable(bool enabled)
		{
			if (enabled)
			{
				this._event.Listen(this.action);
			}
			else
			{
				this._event.Remove(this.action);
			}
		}

		private SteamVR_Events.Event _event;

		private UnityAction action;
	}

	[Serializable]
	public class Action<T> : SteamVR_Events.Action
	{
		public Action(SteamVR_Events.Event<T> _event, UnityAction<T> action)
		{
			this._event = _event;
			this.action = action;
		}

		public override void Enable(bool enabled)
		{
			if (enabled)
			{
				this._event.Listen(this.action);
			}
			else
			{
				this._event.Remove(this.action);
			}
		}

		private SteamVR_Events.Event<T> _event;

		private UnityAction<T> action;
	}

	[Serializable]
	public class Action<T0, T1> : SteamVR_Events.Action
	{
		public Action(SteamVR_Events.Event<T0, T1> _event, UnityAction<T0, T1> action)
		{
			this._event = _event;
			this.action = action;
		}

		public override void Enable(bool enabled)
		{
			if (enabled)
			{
				this._event.Listen(this.action);
			}
			else
			{
				this._event.Remove(this.action);
			}
		}

		private SteamVR_Events.Event<T0, T1> _event;

		private UnityAction<T0, T1> action;
	}

	[Serializable]
	public class Action<T0, T1, T2> : SteamVR_Events.Action
	{
		public Action(SteamVR_Events.Event<T0, T1, T2> _event, UnityAction<T0, T1, T2> action)
		{
			this._event = _event;
			this.action = action;
		}

		public override void Enable(bool enabled)
		{
			if (enabled)
			{
				this._event.Listen(this.action);
			}
			else
			{
				this._event.Remove(this.action);
			}
		}

		private SteamVR_Events.Event<T0, T1, T2> _event;

		private UnityAction<T0, T1, T2> action;
	}

	public class Event : UnityEvent
	{
		public void Listen(UnityAction action)
		{
			base.AddListener(action);
		}

		public void Remove(UnityAction action)
		{
			base.RemoveListener(action);
		}

		public void Send()
		{
			base.Invoke();
		}
	}

	public class Event<T> : UnityEvent<T>
	{
		public void Listen(UnityAction<T> action)
		{
			base.AddListener(action);
		}

		public void Remove(UnityAction<T> action)
		{
			base.RemoveListener(action);
		}

		public void Send(T arg0)
		{
			base.Invoke(arg0);
		}
	}

	public class Event<T0, T1> : UnityEvent<T0, T1>
	{
		public void Listen(UnityAction<T0, T1> action)
		{
			base.AddListener(action);
		}

		public void Remove(UnityAction<T0, T1> action)
		{
			base.RemoveListener(action);
		}

		public void Send(T0 arg0, T1 arg1)
		{
			base.Invoke(arg0, arg1);
		}
	}

	public class Event<T0, T1, T2> : UnityEvent<T0, T1, T2>
	{
		public void Listen(UnityAction<T0, T1, T2> action)
		{
			base.AddListener(action);
		}

		public void Remove(UnityAction<T0, T1, T2> action)
		{
			base.RemoveListener(action);
		}

		public void Send(T0 arg0, T1 arg1, T2 arg2)
		{
			base.Invoke(arg0, arg1, arg2);
		}
	}
}
