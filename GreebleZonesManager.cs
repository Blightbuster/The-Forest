using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Commons.Delegates;
using TheForest.Utils;
using UniLinq;
using UnityEngine;


[DoNotSerializePublic]
public class GreebleZonesManager : MonoBehaviour
{
	
	private void Awake()
	{
		if (!LevelSerializer.IsDeserializing)
		{
			this.InitNonProxied();
		}
		WorkScheduler.RegisterGlobal(new WsTask(this.CheckInCave), false);
	}

	
	private void OnDestroy()
	{
		WorkScheduler.UnregisterGlobal(new WsTask(this.CheckInCave));
	}

	
	public void CheckInCave()
	{
		if ((!BoltNetwork.isRunning && LocalPlayer.IsInCaves) || (BoltNetwork.isRunning && Scene.SceneTracker.AreAllPlayersInCave()) || this._forcedUnload)
		{
			this.Unload();
		}
		else
		{
			this.Load();
		}
	}

	
	public void ForcedUnload(bool onoff)
	{
		this._forcedUnload = onoff;
	}

	
	private void Load()
	{
		if (!this._proxy && this._loader == null)
		{
			base.StartCoroutine(this.LoadRoutine());
		}
	}

	
	private IEnumerator LoadRoutine()
	{
		this._loader = Application.LoadLevelAdditiveAsync("MainSceneGreebles");
		yield return this._loader;
		this._loader = null;
		yield break;
	}

	
	private void Unload()
	{
		if (this._proxy)
		{
			this.OnSerializing();
			UnityEngine.Object.Destroy(this._proxy.gameObject);
			this._proxy = null;
			base.Invoke("DelayedCleanUp", 0.1f);
		}
	}

	
	private void DelayedCleanUp()
	{
		Resources.UnloadUnusedAssets();
	}

	
	public void ProxyLoaded(GreebleZonesSceneProxy proxy)
	{
		this._proxy = proxy;
		if (Application.isPlaying)
		{
			this.InitProxy();
		}
	}

	
	public void ProxyUnloaded(GreebleZonesSceneProxy proxy)
	{
		if (this._proxy == proxy)
		{
			this._proxy = null;
		}
	}

	
	public void InitProxy()
	{
		if (GameSetup.IsNewGame)
		{
			int num = Mathf.Min(this._proxy._greebleZones.Length, this._greebleZonesData.Length);
			for (int i = 0; i < num; i++)
			{
				GreebleZone greebleZone = this._proxy._greebleZones[i];
				if (greebleZone)
				{
					GreebleZonesManager.GZData gzdata = this._greebleZonesData[i];
					if (gzdata == null)
					{
						gzdata = (this._greebleZonesData[i] = new GreebleZonesManager.GZData());
					}
					int maxInstancesModified = this._proxy._greebleZones[i].MaxInstancesModified;
					if (gzdata._instancesState == null || gzdata._instancesState.Length != maxInstancesModified)
					{
						gzdata._instancesState = new byte[maxInstancesModified];
						for (int j = 0; j < gzdata._instancesState.Length; j++)
						{
							gzdata._instancesState[j] = 254;
						}
					}
					greebleZone.Data = this._greebleZonesData[i];
					greebleZone.Data.GZ = greebleZone;
					greebleZone.Data.GZMid = i;
				}
			}
		}
		else
		{
			this.OnDeserializedProxy();
		}
	}

	
	private void InitNonProxied()
	{
		int num = this._zoneCount - this._nonProxiedZones.Length;
		for (int i = 0; i < this._nonProxiedZones.Length; i++)
		{
			GreebleZone greebleZone = this._nonProxiedZones[i];
			if (greebleZone)
			{
				if (this._greebleZonesData[i + num] == null)
				{
					GreebleZonesManager.GZData gzdata = this._greebleZonesData[i + num] = new GreebleZonesManager.GZData();
					gzdata._instancesState = new byte[this._nonProxiedZones[i].MaxInstancesModified];
					for (int j = 0; j < gzdata._instancesState.Length; j++)
					{
						gzdata._instancesState[j] = 254;
					}
				}
				greebleZone.Data = this._greebleZonesData[i + num];
				greebleZone.Data.GZ = greebleZone;
				greebleZone.Data.GZMid = i;
			}
		}
	}

	
	private void OnSerializing()
	{
		if (this._proxy)
		{
			int num = Mathf.Min(new int[]
			{
				this._zoneCount,
				this._proxy._greebleZones.Length,
				this._greebleZonesData.Length
			});
			int i;
			for (i = 0; i < num; i++)
			{
				GreebleZone greebleZone = this._proxy._greebleZones[i];
				GreebleZonesManager.GZData gzdata = this._greebleZonesData[i];
				if (greebleZone && greebleZone.CurrentlyVisible && gzdata != null && gzdata._instancesState != null && gzdata._instancesState.Length == greebleZone.Instances.Length)
				{
					int j;
					for (j = 0; j < greebleZone.Instances.Length; j++)
					{
						if (greebleZone.Instances[j])
						{
							if (gzdata._instancesState[j] > 252)
							{
								gzdata._instancesState[j] = 253;
							}
							else if (gzdata._instancesState[j] == 252)
							{
								CustomActiveValueGreeble component = greebleZone.Instances[j].GetComponent<CustomActiveValueGreeble>();
								if (component)
								{
									component.UpdateGreebleData();
								}
							}
						}
						else
						{
							gzdata._instancesState[j] = byte.MaxValue;
						}
					}
					int maxInstancesModified = this._proxy._greebleZones[i].MaxInstancesModified;
					while (j < maxInstancesModified)
					{
						gzdata._instancesState[j] = 254;
						j++;
					}
				}
				else if (greebleZone && gzdata == null)
				{
					gzdata = (this._greebleZonesData[i] = new GreebleZonesManager.GZData());
					gzdata._instancesState = new byte[greebleZone.MaxInstancesModified];
					for (int k = 0; k < gzdata._instancesState.Length; k++)
					{
						gzdata._instancesState[k] = 254;
					}
				}
			}
			for (int l = 0; l < this._nonProxiedZones.Length; l++)
			{
				GreebleZone greebleZone2 = this._nonProxiedZones[l];
				GreebleZonesManager.GZData gzdata2 = this._greebleZonesData[i];
				if (greebleZone2 && greebleZone2.CurrentlyVisible && gzdata2 != null && gzdata2._instancesState != null && gzdata2._instancesState.Length == greebleZone2.Instances.Length)
				{
					int m;
					for (m = 0; m < greebleZone2.Instances.Length; m++)
					{
						if (greebleZone2.Instances[m])
						{
							if (gzdata2._instancesState[m] > 252)
							{
								gzdata2._instancesState[m] = 253;
							}
							else if (gzdata2._instancesState[m] == 252)
							{
								CustomActiveValueGreeble component2 = greebleZone2.Instances[m].GetComponent<CustomActiveValueGreeble>();
								if (component2)
								{
									component2.UpdateGreebleData();
								}
							}
						}
						else if (greebleZone2.CurrentlyVisible)
						{
							gzdata2._instancesState[m] = byte.MaxValue;
						}
					}
					int maxInstancesModified2 = this._nonProxiedZones[l].MaxInstancesModified;
					while (m < maxInstancesModified2)
					{
						gzdata2._instancesState[m] = 254;
						m++;
					}
				}
				else if (greebleZone2 && gzdata2 == null)
				{
					gzdata2 = (this._greebleZonesData[i] = new GreebleZonesManager.GZData());
					gzdata2._instancesState = new byte[greebleZone2.MaxInstancesModified];
					for (int n = 0; n < gzdata2._instancesState.Length; n++)
					{
						gzdata2._instancesState[n] = 254;
					}
				}
				i++;
			}
			this._savedVersion = this._proxy._currentVersion;
		}
	}

	
	private void OnDeserialized()
	{
		this.OnDeserializedNonProxied();
	}

	
	private void OnDeserializedProxy()
	{
		if (this._savedVersion == this._proxy._currentVersion)
		{
			if (this._greebleZonesData.Length != this._zoneCount)
			{
				if (this._greebleZonesData.Length <= this._zoneCount)
				{
					this.RefreshGreebleZones();
					return;
				}
				List<GreebleZonesManager.GZData> list = this._greebleZonesData.ToList<GreebleZonesManager.GZData>();
				list.RemoveRange(this._zoneCount, this._greebleZonesData.Length - this._zoneCount);
				this._greebleZonesData = list.ToArray();
			}
			int num = Mathf.Min(new int[]
			{
				this._zoneCount,
				this._proxy._greebleZones.Length,
				this._greebleZonesData.Length
			});
			for (int i = 0; i < num; i++)
			{
				GreebleZonesManager.GZData gzdata = this._greebleZonesData[i];
				if (gzdata == null)
				{
					gzdata = (this._greebleZonesData[i] = new GreebleZonesManager.GZData());
					gzdata._instancesState = new byte[this._proxy._greebleZones[i].MaxInstancesModified];
					for (int j = 0; j < gzdata._instancesState.Length; j++)
					{
						gzdata._instancesState[j] = 254;
					}
				}
				else
				{
					GreebleZone greebleZone = this._proxy._greebleZones[i];
					if (greebleZone)
					{
						if (gzdata._instancesState.Length != greebleZone.MaxInstancesModified)
						{
							gzdata._instancesState = new byte[greebleZone.MaxInstancesModified];
							for (int k = 0; k < gzdata._instancesState.Length; k++)
							{
								gzdata._instancesState[k] = 254;
							}
						}
						greebleZone.Data = gzdata;
						gzdata.GZ = greebleZone;
						gzdata.GZMid = i;
						if (greebleZone.RandomSeed != 0)
						{
							gzdata._seed = -1;
						}
					}
				}
			}
		}
		else if (!LocalPlayer.IsInCaves)
		{
			Debug.Log("Clearing up deprecated saved greeble data");
			this.RefreshGreebleZones();
		}
		else
		{
			Debug.Log("Greeble data out of sync. Restoring not handled in caves.");
		}
	}

	
	private void OnDeserializedNonProxied()
	{
		if (this._greebleZonesData.Length == this._zoneCount)
		{
			int num = this._zoneCount - this._nonProxiedZones.Length;
			for (int i = 0; i < this._nonProxiedZones.Length; i++)
			{
				GreebleZonesManager.GZData gzdata = this._greebleZonesData[i + num];
				if (gzdata == null)
				{
					gzdata = (this._greebleZonesData[i + num] = new GreebleZonesManager.GZData());
					gzdata._instancesState = new byte[this._nonProxiedZones[i].MaxInstancesModified];
					for (int j = 0; j < gzdata._instancesState.Length; j++)
					{
						gzdata._instancesState[j] = 254;
					}
				}
				else
				{
					GreebleZone greebleZone = this._nonProxiedZones[i];
					if (greebleZone)
					{
						if (gzdata._instancesState.Length != greebleZone.MaxInstancesModified)
						{
							gzdata._instancesState = new byte[greebleZone.MaxInstancesModified];
							for (int k = 0; k < gzdata._instancesState.Length; k++)
							{
								gzdata._instancesState[k] = 254;
							}
						}
						greebleZone.Data = gzdata;
						gzdata.GZ = greebleZone;
						gzdata.GZMid = i;
						if (greebleZone.RandomSeed != 0)
						{
							gzdata._seed = -1;
						}
					}
				}
			}
		}
	}

	
	public void RefreshGreebleZones()
	{
		if (this._proxy)
		{
			this._proxy.RefreshGreebleZones();
			this._zoneCount = this._proxy._greebleZones.Length + this._nonProxiedZones.Length;
			this._greebleZonesData = new GreebleZonesManager.GZData[this._zoneCount];
			int i;
			for (i = 0; i < this._proxy._greebleZones.Length; i++)
			{
				if (this._proxy._greebleZones[i])
				{
					GreebleZonesManager.GZData gzdata = this._greebleZonesData[i] = new GreebleZonesManager.GZData();
					gzdata._instancesState = new byte[this._proxy._greebleZones[i].MaxInstancesModified];
					for (int j = 0; j < gzdata._instancesState.Length; j++)
					{
						gzdata._instancesState[j] = 254;
					}
					gzdata._seed = -1;
					if (Application.isPlaying)
					{
						this._proxy._greebleZones[i].Data = gzdata;
						gzdata.GZ = this._proxy._greebleZones[i];
						gzdata.GZMid = i;
					}
				}
			}
			for (int k = 0; k < this._nonProxiedZones.Length; k++)
			{
				if (this._nonProxiedZones[k])
				{
					GreebleZonesManager.GZData gzdata2 = this._greebleZonesData[i] = new GreebleZonesManager.GZData();
					gzdata2._instancesState = new byte[this._nonProxiedZones[k].MaxInstancesModified];
					for (int l = 0; l < gzdata2._instancesState.Length; l++)
					{
						gzdata2._instancesState[l] = 254;
					}
					gzdata2._seed = -1;
					if (Application.isPlaying)
					{
						this._nonProxiedZones[k].Data = gzdata2;
						gzdata2.GZ = this._nonProxiedZones[k];
						gzdata2.GZMid = i;
					}
				}
				i++;
			}
		}
	}

	
	public const byte ZIS_DestroyedInstance = 255;

	
	public const byte ZIS_UnusedInstanceSlot = 254;

	
	public const byte ZIS_ActiveInstance = 253;

	
	public const byte ZIS_CustomActiveInstance = 252;

	
	public int _zoneCount;

	
	[SerializeThis]
	public GreebleZonesManager.GZData[] _greebleZonesData;

	
	public GreebleZonesSceneProxy _proxy;

	
	public GreebleZone[] _nonProxiedZones;

	
	[SerializeThis]
	private int _savedVersion;

	
	private AsyncOperation _loader;

	
	private bool _forcedUnload;

	
	[Serializable]
	public class GZData
	{
		
		
		
		[DoNotSerialize]
		public GreebleZone GZ { get; set; }

		
		
		
		[DoNotSerialize]
		public int GZMid { get; set; }

		
		public int _seed = -1;

		
		public byte[] _instancesState;
	}
}
