using System;
using System.Text;
using FMOD;
using UnityEngine;

public class VRAudioDriver : MonoBehaviour
{
	private void Start()
	{
		if (!VRAudioDriver.DriverSet)
		{
			System system = null;
			FMOD_StudioSystem.instance.System.getLowLevelSystem(out system);
			int num;
			system.getNumDrivers(out num);
			StringBuilder[] array = new StringBuilder[num];
			Guid[] array2 = new Guid[num];
			int[] array3 = new int[num];
			SPEAKERMODE[] array4 = new SPEAKERMODE[num];
			int[] array5 = new int[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = new StringBuilder(200);
				system.getDriverInfo(i, array[i], 200, out array2[i], out array3[i], out array4[i], out array5[i]);
				UnityEngine.Debug.Log(string.Concat(new object[]
				{
					">>> AUDIO DRIVER ",
					i,
					" '",
					array[i].ToString(),
					"' GUID='",
					array2[i],
					"' rate=",
					array3[i],
					" speakerMode=",
					array4[i],
					" speakerModeChannel=",
					array5[i]
				}));
				if (array[i].ToString().ToLower().Contains("rift"))
				{
					system.setDriver(i);
					UnityEngine.Debug.Log("[VR] Setting " + array[i].ToString() + " as audio driver");
					VRAudioDriver.DriverSet = true;
					if (!this._logAllAudioDrivers)
					{
						break;
					}
				}
				if (array[i].ToString().ToLower().Contains("htc vive"))
				{
					system.setDriver(i);
					UnityEngine.Debug.Log("[VR] Setting " + array[i].ToString() + " as audio driver");
					VRAudioDriver.DriverSet = true;
					if (!this._logAllAudioDrivers)
					{
						break;
					}
				}
			}
		}
	}

	public bool _logAllAudioDrivers;

	private static bool DriverSet;
}
