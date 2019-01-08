using System;
using FMOD.Studio;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Audio
{
	public class Sfx : MonoBehaviour
	{
		private void Awake()
		{
			if (Sfx.Instance == null)
			{
				Sfx.Instance = this;
			}
			else
			{
				UnityEngine.Object.Destroy(this);
			}
		}

		private void OnDestroy()
		{
			if (Sfx.Instance == this)
			{
				Sfx.Instance = null;
			}
		}

		public static bool TryPlay<T>(Collider col, Transform source = null, bool mpSync = true) where T : SfxInfo
		{
			SfxInfo sfxInfo = col.GetComponent<T>();
			if (sfxInfo)
			{
				Sfx.Play(sfxInfo, (!source) ? col.transform : source, mpSync);
			}
			return sfxInfo;
		}

		public static void Play(SfxInfo info, Transform source = null, bool mpSync = true)
		{
			if (Sfx.Instance)
			{
				Sfx.Play(info._sfx, source, mpSync);
			}
		}

		public static void Play(SfxInfo.SfxTypes type, Transform source = null, bool mpSync = true)
		{
			if (Sfx.Instance && type < (SfxInfo.SfxTypes)Sfx.Instance._definitions.Length)
			{
				if (mpSync)
				{
					FMODCommon.PlayOneshotNetworked(Sfx.Instance._definitions[(int)type]._path, source, FMODCommon.NetworkRole.Any);
				}
				else
				{
					FMODCommon.PlayOneshot(Sfx.Instance._definitions[(int)type]._path, source);
				}
			}
		}

		protected static Sfx Instance { get; private set; }

		[NameFromEnumIndex(typeof(SfxInfo.SfxTypes))]
		public Sfx.SfxDefinition[] _definitions;

		[Serializable]
		public class SfxDefinition
		{
			public string _path;

			public EventInstance _event;
		}
	}
}
