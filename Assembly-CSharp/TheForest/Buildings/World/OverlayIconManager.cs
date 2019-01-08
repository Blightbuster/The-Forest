using System;
using System.Collections.Generic;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	public class OverlayIconManager : MonoBehaviour
	{
		private void Awake()
		{
			if (CoopPeerStarter.DedicatedHost)
			{
				base.enabled = false;
			}
		}

		private void LateUpdate()
		{
			if (!LocalPlayer.Transform)
			{
				return;
			}
			float num = LocalPlayer.MainCam.fieldOfView * 0.0174532924f;
			double num2 = 2.0 * Math.Atan((double)(Mathf.Tan(num / 2f) * LocalPlayer.MainCam.aspect));
			OverlayIconManager._hFOV = Convert.ToSingle(57.295780181884766 * num2) * this._hFovRatio;
			this._legacy = !PlayerPreferences.OverlayIconsGrouping;
			for (int i = 0; i < this._icons.Count; i++)
			{
				OverlayIconManager.OverlayIconType overlayIconType = this._icons[i];
				if (overlayIconType != null)
				{
					overlayIconType.CheckGroupBreak(this._groupingBreakRange, this._superGroupingBreakRange);
					overlayIconType.Update(!this._legacy, this._groupingBreakExpandDuration);
				}
			}
			this._dynamicIcons.Update(false, 0f);
		}

		private void OnDestroy()
		{
			this._icons.Clear();
			if (OverlayIconManager._instance == this)
			{
				OverlayIconManager._instance = null;
			}
		}

		public static void Register(OverlayIcon oi)
		{
			if (!CoopPeerStarter.DedicatedHost)
			{
				if (OverlayIconManager.Instance)
				{
					if (!oi.GetComponentInParent<Rigidbody>())
					{
						if (oi.ID == 0)
						{
							oi.ID = OverlayIconManager.GetNewId();
						}
						while (OverlayIconManager._instance._icons.Count <= oi.Type)
						{
							OverlayIconManager._instance._icons.Add(null);
						}
						if (OverlayIconManager._instance._icons[oi.Type] == null)
						{
							OverlayIconManager._instance._icons[oi.Type] = new OverlayIconManager.OverlayIconType();
						}
						if (oi.DistanceToOtherIcons == null)
						{
							oi.DistanceToOtherIcons = new Dictionary<int, float>();
							oi.TargetPosition = oi.target.position;
						}
						if (oi.Type != 0 && oi.Type != 6)
						{
							for (int i = 0; i < OverlayIconManager._instance._icons[oi.Type]._icons.Count; i++)
							{
								OverlayIcon overlayIcon = OverlayIconManager._instance._icons[oi.Type]._icons[i];
								float num = Vector3.Distance(oi.TargetPosition, overlayIcon.TargetPosition);
								oi.DistanceToOtherIcons.Add(overlayIcon.ID, num);
								overlayIcon.DistanceToOtherIcons.Add(oi.ID, num);
								if (num < OverlayIconManager._instance._groupingRange)
								{
									oi.InGroupingRangeIcons++;
									overlayIcon.InGroupingRangeIcons++;
								}
								if (num < OverlayIconManager._instance._superGroupingRange)
								{
									oi.InSuperGroupingRangeIcons++;
									overlayIcon.InSuperGroupingRangeIcons++;
								}
							}
							OverlayIconManager._instance._icons[oi.Type]._icons.Add(oi);
							OverlayIconManager._instance._icons[oi.Type].ProcessGroups(OverlayIconManager._instance._groupingRange, OverlayIconManager._instance._superGroupingRange);
						}
						else
						{
							OverlayIconManager._instance._icons[oi.Type]._icons.Add(oi);
						}
					}
					else
					{
						OverlayIconManager._instance._dynamicIcons._icons.Add(oi);
						oi.ShouldRefreshTargetPosition = true;
						oi.BreakGroupAlpha = 1f;
						oi.BreakSuperGroupAlpha = 1f;
						oi.CurrentGroup = oi;
						if (oi.target == oi.transform)
						{
							oi.target = oi.transform.parent;
						}
					}
				}
			}
			else
			{
				oi.DoUpdate(false, false);
			}
		}

		public static void Unregister(OverlayIcon oi)
		{
			if (OverlayIconManager._instance)
			{
				if (OverlayIconManager._instance._icons.Count > oi.Type && OverlayIconManager._instance._icons[oi.Type] != null && OverlayIconManager._instance._icons[oi.Type]._icons.Contains(oi))
				{
					for (int i = 0; i < OverlayIconManager._instance._icons[oi.Type]._icons.Count; i++)
					{
						float num;
						if (OverlayIconManager._instance._icons[oi.Type]._icons[i].DistanceToOtherIcons.TryGetValue(oi.ID, out num))
						{
							if (num < OverlayIconManager._instance._groupingRange)
							{
								OverlayIconManager._instance._icons[oi.Type]._icons[i].InGroupingRangeIcons--;
							}
							if (num < OverlayIconManager._instance._superGroupingRange)
							{
								OverlayIconManager._instance._icons[oi.Type]._icons[i].InSuperGroupingRangeIcons--;
							}
						}
						if (OverlayIconManager._instance._icons[oi.Type]._icons[i].DistanceToOtherIcons.ContainsKey(oi.ID))
						{
							OverlayIconManager._instance._icons[oi.Type]._icons[i].DistanceToOtherIcons.Remove(oi.ID);
						}
					}
					oi.DistanceToOtherIcons.Clear();
					OverlayIconManager._instance._icons[oi.Type]._icons.Remove(oi);
					if (oi.GroupDisplay)
					{
						OverlayIconManager._instance._icons[oi.Type].ProcessGroups(OverlayIconManager._instance._groupingRange, OverlayIconManager._instance._superGroupingRange);
					}
				}
				else if (OverlayIconManager._instance._dynamicIcons._icons.Contains(oi))
				{
					OverlayIconManager._instance._dynamicIcons._icons.Remove(oi);
				}
			}
		}

		public static int GetNewId()
		{
			if (OverlayIconManager.Instance)
			{
				return ++OverlayIconManager.Instance._lastId;
			}
			return 0;
		}

		public static void Clear()
		{
			if (OverlayIconManager._instance)
			{
				for (int i = 0; i < OverlayIconManager._instance._icons.Count; i++)
				{
					OverlayIconManager.OverlayIconType overlayIconType = OverlayIconManager._instance._icons[i];
					if (overlayIconType != null)
					{
						overlayIconType.Clear();
					}
				}
			}
		}

		private static OverlayIconManager Instance
		{
			get
			{
				if (!OverlayIconManager._instance)
				{
					OverlayIconManager._instance = new GameObject("OverlayIconManager").AddComponent<OverlayIconManager>();
				}
				return OverlayIconManager._instance;
			}
		}

		public float _hFovRatio = 1f;

		public float _groupingRange = 70f;

		public float _superGroupingRange = 400f;

		public float _groupingBreakRange = 25f;

		public float _superGroupingBreakRange = 75f;

		public float _groupingBreakExpandDuration = 0.25f;

		public bool _legacy;

		public bool _showAllGroupGizmos;

		[NameFromEnumIndex(typeof(OverlayIconManager.IconTypes))]
		[SerializeField]
		private List<OverlayIconManager.OverlayIconType> _icons = new List<OverlayIconManager.OverlayIconType>();

		[SerializeField]
		private OverlayIconManager.OverlayIconType _dynamicIcons = new OverlayIconManager.OverlayIconType();

		private int _lastId;

		private bool _refresh;

		private static OverlayIconManager _instance;

		private static float _hFOV;

		public class InRangeIconsSorting : IComparer<OverlayIcon>
		{
			public int Compare(OverlayIcon x, OverlayIcon y)
			{
				return (x.InGroupingRangeIcons <= y.InGroupingRangeIcons) ? 1 : -1;
			}
		}

		public class InSuperRangeIconsSorting : IComparer<OverlayIcon>
		{
			public int Compare(OverlayIcon x, OverlayIcon y)
			{
				return (x.InSuperGroupingRangeIcons <= y.InSuperGroupingRangeIcons) ? 1 : -1;
			}
		}

		[Serializable]
		public class OverlayIconType
		{
			public void ProcessGroups(float groupRange, float superGroupRange)
			{
				this._iconGroups.Clear();
				this._unprocessedIcons.Clear();
				for (int i = 0; i < this._icons.Count; i++)
				{
					this._unprocessedIcons.Add(this._icons[i]);
				}
				this._unprocessedIcons.Sort(this._inRangeIconsSorting);
				while (this._unprocessedIcons.Count > 0)
				{
					OverlayIcon overlayIcon = this._unprocessedIcons[0];
					this._unprocessedIcons.RemoveAt(0);
					if (overlayIcon)
					{
						Vector3 vector = overlayIcon.TargetPosition;
						int num = 1;
						float num2 = 0f;
						if (this._unprocessedIcons.Count > 0 && overlayIcon.InGroupingRangeIcons > 0)
						{
							foreach (KeyValuePair<int, float> keyValuePair in overlayIcon.DistanceToOtherIcons)
							{
								if (keyValuePair.Value < groupRange)
								{
									for (int j = 0; j < this._unprocessedIcons.Count; j++)
									{
										if (this._unprocessedIcons[j].ID == keyValuePair.Key)
										{
											if (keyValuePair.Value > num2)
											{
												num2 = keyValuePair.Value;
											}
											this._unprocessedIcons[j].CurrentGroup = overlayIcon;
											this._unprocessedIcons[j].CurrentSuperGroup = overlayIcon;
											this._unprocessedIcons[j].FinalTargetPosition = this._unprocessedIcons[j].TargetPosition;
											vector += this._unprocessedIcons[j].TargetPosition;
											num++;
											this._unprocessedIcons.RemoveAt(j);
											break;
										}
									}
									if (num > overlayIcon.InGroupingRangeIcons)
									{
										break;
									}
								}
							}
						}
						if (num > 1)
						{
							vector /= (float)num;
							if (Terrain.activeTerrain && !overlayIcon.IsInCaves)
							{
								vector.y = Terrain.activeTerrain.SampleHeight(vector) + 5f;
							}
						}
						overlayIcon.GroupTargetPosition = vector;
						overlayIcon.GroupMinBreakRange = num2;
						overlayIcon.CurrentGroup = null;
						this._iconGroups.Add(overlayIcon);
					}
				}
				this._iconSuperGroups.Clear();
				this._unprocessedIcons.Clear();
				for (int k = 0; k < this._iconGroups.Count; k++)
				{
					this._unprocessedIcons.Add(this._iconGroups[k]);
				}
				this._unprocessedIcons.Sort(this._inSuperRangeIconsSorting);
				while (this._unprocessedIcons.Count > 0)
				{
					OverlayIcon overlayIcon2 = this._unprocessedIcons[0];
					this._unprocessedIcons.RemoveAt(0);
					if (overlayIcon2)
					{
						int num3 = 1;
						Vector3 a = overlayIcon2.GroupTargetPosition + new Vector3(-overlayIcon2.GroupMinBreakRange, 0f, -overlayIcon2.GroupMinBreakRange);
						Vector3 b = overlayIcon2.GroupTargetPosition + new Vector3(overlayIcon2.GroupMinBreakRange, 0f, overlayIcon2.GroupMinBreakRange);
						if (this._unprocessedIcons.Count > 0 && overlayIcon2.InSuperGroupingRangeIcons > 0)
						{
							foreach (KeyValuePair<int, float> keyValuePair2 in overlayIcon2.DistanceToOtherIcons)
							{
								if (keyValuePair2.Value < superGroupRange)
								{
									for (int l = 0; l < this._unprocessedIcons.Count; l++)
									{
										if (this._unprocessedIcons[l].ID == keyValuePair2.Key)
										{
											float num4 = this._unprocessedIcons[l].GroupTargetPosition.x - this._unprocessedIcons[l].GroupMinBreakRange;
											float num5 = this._unprocessedIcons[l].GroupTargetPosition.x + this._unprocessedIcons[l].GroupMinBreakRange;
											float y = this._unprocessedIcons[l].GroupTargetPosition.y;
											float y2 = this._unprocessedIcons[l].GroupTargetPosition.y;
											float num6 = this._unprocessedIcons[l].GroupTargetPosition.z - this._unprocessedIcons[l].GroupMinBreakRange;
											float num7 = this._unprocessedIcons[l].GroupTargetPosition.z + this._unprocessedIcons[l].GroupMinBreakRange;
											if (num4 < a.x)
											{
												a.x = num4;
											}
											if (y < a.y)
											{
												a.y = y;
											}
											if (num6 < a.z)
											{
												a.z = num6;
											}
											if (num5 > b.x)
											{
												b.x = num5;
											}
											if (y2 > b.y)
											{
												b.y = y2;
											}
											if (num7 > b.z)
											{
												b.z = num7;
											}
											this._unprocessedIcons[l].CurrentSuperGroup = overlayIcon2;
											num3++;
											this._unprocessedIcons.RemoveAt(l);
											break;
										}
									}
									if (num3 > overlayIcon2.InSuperGroupingRangeIcons)
									{
										break;
									}
								}
							}
						}
						if (num3 > 1)
						{
							Vector3 vector2 = (a + b) / 2f;
							if (Terrain.activeTerrain && !overlayIcon2.IsInCaves)
							{
								vector2.y = Terrain.activeTerrain.SampleHeight(vector2) + 10f;
							}
							overlayIcon2.SuperGroupTargetPosition = vector2;
							overlayIcon2.SuperGroupMinBreakRange = Vector3.Distance(a, b) / 2f;
							overlayIcon2.CurrentSuperGroup = null;
							this._iconSuperGroups.Add(overlayIcon2);
						}
						else
						{
							overlayIcon2.CurrentSuperGroup = null;
							overlayIcon2.BreakSuperGroupAlpha = 1f;
							overlayIcon2.BreakSuperGroup = true;
						}
					}
				}
			}

			public void CheckGroupBreak(float breakRange, float superbreakRange)
			{
				for (int i = 0; i < this._iconSuperGroups.Count; i++)
				{
					bool flag = Vector3.Distance(LocalPlayer.Transform.position, this._iconSuperGroups[i].SuperGroupTargetPosition) < this._iconSuperGroups[i].SuperGroupMinBreakRange + superbreakRange;
					if (this._iconSuperGroups[i].BreakSuperGroup != flag)
					{
						if (flag)
						{
							this._iconSuperGroups[i].BreakSuperGroupAlpha = 0f;
						}
						this._iconSuperGroups[i].BreakSuperGroup = flag;
					}
				}
				for (int j = 0; j < this._iconGroups.Count; j++)
				{
					bool flag2 = Vector3.Distance(LocalPlayer.Transform.position, this._iconGroups[j].GroupTargetPosition) < this._iconGroups[j].GroupMinBreakRange + breakRange;
					if (this._iconGroups[j].BreakGroup != flag2)
					{
						if (flag2)
						{
							this._iconGroups[j].BreakGroupAlpha = 0f;
						}
						this._iconGroups[j].BreakGroup = flag2;
					}
				}
			}

			public void Update(bool checkGroup, float breakGroupExpandDuration)
			{
				bool flag = LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World && Scene.Atmosphere;
				bool flag2 = PlayerPreferences.ShowOverlayIcons && flag;
				for (int i = 0; i < this._iconGroups.Count; i++)
				{
					if (this._iconGroups[i].BreakGroupAlpha < 1f)
					{
						this._iconGroups[i].BreakGroupAlpha = Mathf.Clamp01(this._iconGroups[i].BreakGroupAlpha + Time.deltaTime / breakGroupExpandDuration);
					}
				}
				for (int j = 0; j < this._iconSuperGroups.Count; j++)
				{
					if (this._iconSuperGroups[j].BreakSuperGroupAlpha < 1f)
					{
						this._iconSuperGroups[j].BreakSuperGroupAlpha = Mathf.Clamp01(this._iconSuperGroups[j].BreakSuperGroupAlpha + Time.deltaTime / breakGroupExpandDuration);
					}
				}
				for (int k = 0; k < this._icons.Count; k++)
				{
					OverlayIcon overlayIcon = this._icons[k];
					bool flag3 = flag2;
					if (flag2)
					{
						flag3 = (overlayIcon.IgnoreInCaveStatus || LocalPlayer.IsInCaves == overlayIcon.IsInCaves);
					}
					if (checkGroup && flag3)
					{
						if (overlayIcon.GroupDisplay)
						{
							flag3 = (overlayIcon.SuperGroupDisplay || overlayIcon.CurrentSuperGroup.BreakSuperGroup);
						}
						else if (overlayIcon.CurrentGroup.SuperGroupDisplay)
						{
							flag3 = (overlayIcon.CurrentGroup.BreakSuperGroup && overlayIcon.CurrentGroup.BreakGroup);
						}
						else
						{
							flag3 = (overlayIcon.CurrentGroup.CurrentSuperGroup.BreakSuperGroup && overlayIcon.CurrentGroup.BreakGroup);
						}
					}
					if (flag3 && overlayIcon.ShouldRefreshTargetPosition)
					{
						if (!overlayIcon.target)
						{
							OverlayIconManager.Unregister(overlayIcon);
							return;
						}
						overlayIcon.FinalTargetPosition = overlayIcon.target.position;
					}
					if (flag3 || overlayIcon._showing != flag3)
					{
						overlayIcon.DoUpdate(flag3, checkGroup);
					}
				}
			}

			public void Clear()
			{
				this._icons.Clear();
				this._iconGroups.Clear();
				this._unprocessedIcons.Clear();
			}

			public bool _showGroupsGizmos;

			public List<OverlayIcon> _icons = new List<OverlayIcon>();

			public List<OverlayIcon> _iconGroups = new List<OverlayIcon>();

			public List<OverlayIcon> _iconSuperGroups = new List<OverlayIcon>();

			private List<OverlayIcon> _unprocessedIcons = new List<OverlayIcon>(200);

			private OverlayIconManager.InRangeIconsSorting _inRangeIconsSorting = new OverlayIconManager.InRangeIconsSorting();

			private OverlayIconManager.InSuperRangeIconsSorting _inSuperRangeIconsSorting = new OverlayIconManager.InSuperRangeIconsSorting();
		}

		public enum IconTypes
		{
			HouseBlank,
			HouseYellow,
			HouseGreen,
			HousePurple,
			HouseRed,
			HouseBlue,
			FlagBlank,
			FlagYellow,
			FlagGreen,
			FlagPurple,
			FlagRed,
			FlagBlue,
			Hammer,
			CaveHouseBlank,
			CaveHouseYellow,
			CaveHouseGreen,
			CaveHousePurple,
			CaveHouseRed,
			CaveHouseBlue,
			CaveFlagBlank,
			CaveFlagYellow,
			CaveFlagGreen,
			CaveFlagPurple,
			CaveFlagRed,
			CaveFlagBlue,
			CaveHammer
		}
	}
}
