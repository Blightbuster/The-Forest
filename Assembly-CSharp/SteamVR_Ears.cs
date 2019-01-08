using System;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

[RequireComponent(typeof(AudioListener))]
public class SteamVR_Ears : MonoBehaviour
{
	private void OnNewPosesApplied()
	{
		Transform origin = this.vrcam.origin;
		Quaternion lhs = (!(origin != null)) ? Quaternion.identity : origin.rotation;
		base.transform.rotation = lhs * this.offset;
	}

	private void OnEnable()
	{
		this.usingSpeakers = false;
		CVRSettings settings = OpenVR.Settings;
		if (settings != null)
		{
			EVRSettingsError evrsettingsError = EVRSettingsError.None;
			if (settings.GetBool("steamvr", "usingSpeakers", ref evrsettingsError))
			{
				this.usingSpeakers = true;
				float @float = settings.GetFloat("steamvr", "speakersForwardYawOffsetDegrees", ref evrsettingsError);
				this.offset = Quaternion.Euler(0f, @float, 0f);
			}
		}
		if (this.usingSpeakers)
		{
			SteamVR_Events.NewPosesApplied.Listen(new UnityAction(this.OnNewPosesApplied));
		}
	}

	private void OnDisable()
	{
		if (this.usingSpeakers)
		{
			SteamVR_Events.NewPosesApplied.Remove(new UnityAction(this.OnNewPosesApplied));
		}
	}

	public SteamVR_Camera vrcam;

	private bool usingSpeakers;

	private Quaternion offset;
}
