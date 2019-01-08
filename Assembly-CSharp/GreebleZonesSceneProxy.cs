using System;
using UnityEngine;

[ExecuteInEditMode]
public class GreebleZonesSceneProxy : MonoBehaviour
{
	private void Awake()
	{
		this._gmz = UnityEngine.Object.FindObjectOfType<GreebleZonesManager>();
		if (this._gmz)
		{
			this._gmz.ProxyLoaded(this);
		}
	}

	private void OnDestroy()
	{
		if (this._gmz)
		{
			this._gmz.ProxyUnloaded(this);
			this._gmz = null;
		}
	}

	public void RefreshGreebleZones()
	{
		if (!Application.isPlaying)
		{
			this._currentVersion++;
		}
		this._greebleZones = UnityEngine.Object.FindObjectsOfType<GreebleZone>();
	}

	public GreebleZone[] _greebleZones;

	public int _currentVersion = 2;

	private GreebleZonesManager _gmz;
}
