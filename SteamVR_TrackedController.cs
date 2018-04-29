using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Valve.VR;


public class SteamVR_TrackedController : MonoBehaviour
{
	
	
	
	public event ClickedEventHandler MenuButtonClicked;

	
	
	
	public event ClickedEventHandler MenuButtonUnclicked;

	
	
	
	public event ClickedEventHandler TriggerClicked;

	
	
	
	public event ClickedEventHandler TriggerUnclicked;

	
	
	
	public event ClickedEventHandler SteamClicked;

	
	
	
	public event ClickedEventHandler PadClicked;

	
	
	
	public event ClickedEventHandler PadUnclicked;

	
	
	
	public event ClickedEventHandler PadTouched;

	
	
	
	public event ClickedEventHandler PadUntouched;

	
	
	
	public event ClickedEventHandler Gripped;

	
	
	
	public event ClickedEventHandler Ungripped;

	
	protected virtual void Start()
	{
		if (base.GetComponent<SteamVR_TrackedObject>() == null)
		{
			base.gameObject.AddComponent<SteamVR_TrackedObject>();
		}
		if (this.controllerIndex != 0u)
		{
			base.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex)this.controllerIndex;
			if (base.GetComponent<SteamVR_RenderModel>() != null)
			{
				base.GetComponent<SteamVR_RenderModel>().index = (SteamVR_TrackedObject.EIndex)this.controllerIndex;
			}
		}
		else
		{
			this.controllerIndex = (uint)base.GetComponent<SteamVR_TrackedObject>().index;
		}
	}

	
	public void SetDeviceIndex(int index)
	{
		this.controllerIndex = (uint)index;
	}

	
	public virtual void OnTriggerClicked(ClickedEventArgs e)
	{
		if (this.TriggerClicked != null)
		{
			this.TriggerClicked(this, e);
		}
	}

	
	public virtual void OnTriggerUnclicked(ClickedEventArgs e)
	{
		if (this.TriggerUnclicked != null)
		{
			this.TriggerUnclicked(this, e);
		}
	}

	
	public virtual void OnMenuClicked(ClickedEventArgs e)
	{
		if (this.MenuButtonClicked != null)
		{
			this.MenuButtonClicked(this, e);
		}
	}

	
	public virtual void OnMenuUnclicked(ClickedEventArgs e)
	{
		if (this.MenuButtonUnclicked != null)
		{
			this.MenuButtonUnclicked(this, e);
		}
	}

	
	public virtual void OnSteamClicked(ClickedEventArgs e)
	{
		if (this.SteamClicked != null)
		{
			this.SteamClicked(this, e);
		}
	}

	
	public virtual void OnPadClicked(ClickedEventArgs e)
	{
		if (this.PadClicked != null)
		{
			this.PadClicked(this, e);
		}
	}

	
	public virtual void OnPadUnclicked(ClickedEventArgs e)
	{
		if (this.PadUnclicked != null)
		{
			this.PadUnclicked(this, e);
		}
	}

	
	public virtual void OnPadTouched(ClickedEventArgs e)
	{
		if (this.PadTouched != null)
		{
			this.PadTouched(this, e);
		}
	}

	
	public virtual void OnPadUntouched(ClickedEventArgs e)
	{
		if (this.PadUntouched != null)
		{
			this.PadUntouched(this, e);
		}
	}

	
	public virtual void OnGripped(ClickedEventArgs e)
	{
		if (this.Gripped != null)
		{
			this.Gripped(this, e);
		}
	}

	
	public virtual void OnUngripped(ClickedEventArgs e)
	{
		if (this.Ungripped != null)
		{
			this.Ungripped(this, e);
		}
	}

	
	protected virtual void Update()
	{
		CVRSystem system = OpenVR.System;
		if (system != null && system.GetControllerState(this.controllerIndex, ref this.controllerState, (uint)Marshal.SizeOf(typeof(VRControllerState_t))))
		{
			ulong num = this.controllerState.ulButtonPressed & 8589934592UL;
			if (num > 0UL && !this.triggerPressed)
			{
				this.triggerPressed = true;
				ClickedEventArgs e;
				e.controllerIndex = this.controllerIndex;
				e.flags = (uint)this.controllerState.ulButtonPressed;
				e.padX = this.controllerState.rAxis0.x;
				e.padY = this.controllerState.rAxis0.y;
				this.OnTriggerClicked(e);
			}
			else if (num == 0UL && this.triggerPressed)
			{
				this.triggerPressed = false;
				ClickedEventArgs e2;
				e2.controllerIndex = this.controllerIndex;
				e2.flags = (uint)this.controllerState.ulButtonPressed;
				e2.padX = this.controllerState.rAxis0.x;
				e2.padY = this.controllerState.rAxis0.y;
				this.OnTriggerUnclicked(e2);
			}
			ulong num2 = this.controllerState.ulButtonPressed & 4UL;
			if (num2 > 0UL && !this.gripped)
			{
				this.gripped = true;
				ClickedEventArgs e3;
				e3.controllerIndex = this.controllerIndex;
				e3.flags = (uint)this.controllerState.ulButtonPressed;
				e3.padX = this.controllerState.rAxis0.x;
				e3.padY = this.controllerState.rAxis0.y;
				this.OnGripped(e3);
			}
			else if (num2 == 0UL && this.gripped)
			{
				this.gripped = false;
				ClickedEventArgs e4;
				e4.controllerIndex = this.controllerIndex;
				e4.flags = (uint)this.controllerState.ulButtonPressed;
				e4.padX = this.controllerState.rAxis0.x;
				e4.padY = this.controllerState.rAxis0.y;
				this.OnUngripped(e4);
			}
			ulong num3 = this.controllerState.ulButtonPressed & 4294967296UL;
			if (num3 > 0UL && !this.padPressed)
			{
				this.padPressed = true;
				ClickedEventArgs e5;
				e5.controllerIndex = this.controllerIndex;
				e5.flags = (uint)this.controllerState.ulButtonPressed;
				e5.padX = this.controllerState.rAxis0.x;
				e5.padY = this.controllerState.rAxis0.y;
				this.OnPadClicked(e5);
			}
			else if (num3 == 0UL && this.padPressed)
			{
				this.padPressed = false;
				ClickedEventArgs e6;
				e6.controllerIndex = this.controllerIndex;
				e6.flags = (uint)this.controllerState.ulButtonPressed;
				e6.padX = this.controllerState.rAxis0.x;
				e6.padY = this.controllerState.rAxis0.y;
				this.OnPadUnclicked(e6);
			}
			ulong num4 = this.controllerState.ulButtonPressed & 2UL;
			if (num4 > 0UL && !this.menuPressed)
			{
				this.menuPressed = true;
				ClickedEventArgs e7;
				e7.controllerIndex = this.controllerIndex;
				e7.flags = (uint)this.controllerState.ulButtonPressed;
				e7.padX = this.controllerState.rAxis0.x;
				e7.padY = this.controllerState.rAxis0.y;
				this.OnMenuClicked(e7);
			}
			else if (num4 == 0UL && this.menuPressed)
			{
				this.menuPressed = false;
				ClickedEventArgs e8;
				e8.controllerIndex = this.controllerIndex;
				e8.flags = (uint)this.controllerState.ulButtonPressed;
				e8.padX = this.controllerState.rAxis0.x;
				e8.padY = this.controllerState.rAxis0.y;
				this.OnMenuUnclicked(e8);
			}
			num3 = (this.controllerState.ulButtonTouched & 4294967296UL);
			if (num3 > 0UL && !this.padTouched)
			{
				this.padTouched = true;
				ClickedEventArgs e9;
				e9.controllerIndex = this.controllerIndex;
				e9.flags = (uint)this.controllerState.ulButtonPressed;
				e9.padX = this.controllerState.rAxis0.x;
				e9.padY = this.controllerState.rAxis0.y;
				this.OnPadTouched(e9);
			}
			else if (num3 == 0UL && this.padTouched)
			{
				this.padTouched = false;
				ClickedEventArgs e10;
				e10.controllerIndex = this.controllerIndex;
				e10.flags = (uint)this.controllerState.ulButtonPressed;
				e10.padX = this.controllerState.rAxis0.x;
				e10.padY = this.controllerState.rAxis0.y;
				this.OnPadUntouched(e10);
			}
		}
	}

	
	public uint controllerIndex;

	
	public VRControllerState_t controllerState;

	
	public bool triggerPressed;

	
	public bool steamPressed;

	
	public bool menuPressed;

	
	public bool padPressed;

	
	public bool padTouched;

	
	public bool gripped;
}
