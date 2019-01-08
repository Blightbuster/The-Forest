using System;
using UnityEngine;

public class SteamVR_GazeTracker : MonoBehaviour
{
	public event GazeEventHandler GazeOn;

	public event GazeEventHandler GazeOff;

	private void Start()
	{
	}

	public virtual void OnGazeOn(GazeEventArgs e)
	{
		if (this.GazeOn != null)
		{
			this.GazeOn(this, e);
		}
	}

	public virtual void OnGazeOff(GazeEventArgs e)
	{
		if (this.GazeOff != null)
		{
			this.GazeOff(this, e);
		}
	}

	private void Update()
	{
		if (this.hmdTrackedObject == null)
		{
			SteamVR_TrackedObject[] array = UnityEngine.Object.FindObjectsOfType<SteamVR_TrackedObject>();
			foreach (SteamVR_TrackedObject steamVR_TrackedObject in array)
			{
				if (steamVR_TrackedObject.index == SteamVR_TrackedObject.EIndex.Hmd)
				{
					this.hmdTrackedObject = steamVR_TrackedObject.transform;
					break;
				}
			}
		}
		if (this.hmdTrackedObject)
		{
			Ray ray = new Ray(this.hmdTrackedObject.position, this.hmdTrackedObject.forward);
			Plane plane = new Plane(this.hmdTrackedObject.forward, base.transform.position);
			float d = 0f;
			if (plane.Raycast(ray, out d))
			{
				Vector3 a = this.hmdTrackedObject.position + this.hmdTrackedObject.forward * d;
				float num = Vector3.Distance(a, base.transform.position);
				if (num < this.gazeInCutoff && !this.isInGaze)
				{
					this.isInGaze = true;
					GazeEventArgs e;
					e.distance = num;
					this.OnGazeOn(e);
				}
				else if (num >= this.gazeOutCutoff && this.isInGaze)
				{
					this.isInGaze = false;
					GazeEventArgs e2;
					e2.distance = num;
					this.OnGazeOff(e2);
				}
			}
		}
	}

	public bool isInGaze;

	public float gazeInCutoff = 0.15f;

	public float gazeOutCutoff = 0.4f;

	private Transform hmdTrackedObject;
}
