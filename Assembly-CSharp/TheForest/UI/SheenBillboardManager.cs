using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	public class SheenBillboardManager : MonoBehaviour
	{
		private void Awake()
		{
			if (CoopPeerStarter.DedicatedHost)
			{
				base.enabled = false;
			}
		}

		private void Update()
		{
			if (!LocalPlayer.Transform)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			if (this._inactiveIcons.Count > 0)
			{
				int num3 = Mathf.Clamp(Mathf.Max(this._inactiveIcons.Count / 20, 10), 1, this._inactiveIcons.Count);
				for (int i = 0; i < num3; i++)
				{
					num = (i + this._lastInactiveIconsIndex + num2) % this._inactiveIcons.Count;
					if (this._inactiveIcons[num].gameObject.activeSelf)
					{
						this._activeIcons.Add(this._inactiveIcons[num]);
						this._inactiveIcons[num].CheckWsToken2();
						this._inactiveIcons.RemoveAt(num);
						num2--;
					}
				}
				this._lastInactiveIconsIndex = num;
			}
			num2 = 0;
			if (this._activeIcons.Count > 0)
			{
				int num4 = Mathf.Clamp(Mathf.Max(this._activeIcons.Count / 20, 10), 1, this._activeIcons.Count);
				for (int j = 0; j < num4; j++)
				{
					num = (j + this._lastActiveIconsIndex + num2) % this._activeIcons.Count;
					if (!this._activeIcons[num].gameObject.activeSelf)
					{
						this._inactiveIcons.Add(this._activeIcons[num]);
						this._activeIcons.RemoveAt(num);
						num2--;
					}
				}
				this._lastActiveIconsIndex = num;
			}
		}

		private void OnDestroy()
		{
			this._inactiveIcons.Clear();
			this._activeIcons.Clear();
			if (SheenBillboardManager._instance == this)
			{
				SheenBillboardManager._instance = null;
			}
		}

		public static void Register(SheenBillboard sb)
		{
			if (!CoopPeerStarter.DedicatedHost)
			{
				if (SheenBillboardManager.Instance)
				{
					if (sb.gameObject.activeSelf)
					{
						SheenBillboardManager.Instance._activeIcons.Add(sb);
					}
					else
					{
						SheenBillboardManager.Instance._inactiveIcons.Add(sb);
					}
				}
			}
			else
			{
				sb.enabled = false;
			}
		}

		public static void Unregister(SheenBillboard sb)
		{
			if (SheenBillboardManager._instance)
			{
				int num = SheenBillboardManager._instance._activeIcons.IndexOf(sb);
				if (num > -1)
				{
					SheenBillboardManager._instance._activeIcons.RemoveAt(num);
				}
				else
				{
					num = SheenBillboardManager._instance._inactiveIcons.IndexOf(sb);
					if (num > -1)
					{
						SheenBillboardManager._instance._inactiveIcons.RemoveAt(num);
					}
				}
			}
		}

		private static SheenBillboardManager Instance
		{
			get
			{
				if (!SheenBillboardManager._instance)
				{
					SheenBillboardManager._instance = new GameObject("SheenBillboardManager").AddComponent<SheenBillboardManager>();
				}
				return SheenBillboardManager._instance;
			}
		}

		public float _offshootRatio = 1.1f;

		public float _hFovRatio = 1f;

		public float _groupingRange = 0.1f;

		public float _dotUpdateThreshold = 0.05f;

		public bool _legacy;

		private int _lastInactiveIconsIndex;

		private int _lastActiveIconsIndex;

		private List<SheenBillboard> _inactiveIcons = new List<SheenBillboard>();

		private List<SheenBillboard> _activeIcons = new List<SheenBillboard>();

		private static SheenBillboardManager _instance;
	}
}
