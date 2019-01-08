using System;
using System.Collections.Generic;
using Steamworks;
using TheForest.Tools;
using UnityEngine;

namespace TheForest.Player
{
	public class AchievementData
	{
		public AchievementData(string name, string description, string key, string statKey, EventRegistry reg, object evnt, Func<AchievementData, object, bool> customAction, Func<AchievementData, object, bool> setupAction = null) : this(name, description, key, statKey, null, reg, evnt, customAction, setupAction)
		{
		}

		public AchievementData(string name, string description, string key, string statKey, List<int> unlockValues, EventRegistry reg, object evnt, Func<AchievementData, object, bool> customAction, Func<AchievementData, object, bool> setupAction = null)
		{
			try
			{
				this.Name = name;
				this.Description = description;
				this.Key = key;
				this.StatKey = statKey;
				this.UnlockValues = unlockValues;
				this.Registry = reg;
				this.Event = evnt;
				this.CustomAction = customAction;
				this.Init(AccountInfo.IsAchievementUnlocked(this), setupAction);
			}
			catch (Exception exception)
			{
				if (Achievements.ShowExceptions)
				{
					Debug.LogException(exception);
				}
			}
		}

		private void Init(bool completed, Func<AchievementData, object, bool> setupAction)
		{
			if (completed)
			{
				this.Completed = completed;
			}
			else
			{
				this.AchHandle = new InteropHelp.UTF8StringHandle(this.Key);
				if (!string.IsNullOrEmpty(this.StatKey))
				{
					this.StatHandle = new InteropHelp.UTF8StringHandle(this.StatKey);
					this.CurrentValue = AccountInfo.GetIntStat(this);
					while (this.UnlockValues.Count > 0 && this.UnlockValues[0] < this.CurrentValue)
					{
						this.UnlockValues.RemoveAt(0);
					}
				}
				if (this.Registry != null)
				{
					if (string.IsNullOrEmpty(this.StatKey))
					{
						this.Registry.Subscribe(this.Event, new EventRegistry.SubscriberCallback(this.Unlock));
					}
					else
					{
						this.Registry.Subscribe(this.Event, new EventRegistry.SubscriberCallback(this.TickStat));
					}
				}
				if (setupAction != null)
				{
					setupAction(this, null);
				}
			}
		}

		public void Clear()
		{
			this.ReleaseHandles();
			this.CustomAction = null;
			if (this.Registry != null)
			{
				this.Registry.Unsubscribe(this.Event, new EventRegistry.SubscriberCallback(this.TickStat));
				this.Registry.Unsubscribe(this.Event, new EventRegistry.SubscriberCallback(this.Unlock));
				this.Registry = null;
			}
		}

		public void TickStat(object o)
		{
			try
			{
				if (!this.Completed)
				{
					if (this.CustomAction != null)
					{
						if (Achievements.ShowLogs)
						{
							Debug.Log(string.Concat(new string[]
							{
								"# Doing custom tick action for stat: ",
								this.StatKey,
								" [",
								this.Key,
								"]"
							}));
						}
						if (!this.CustomAction(this, o))
						{
							return;
						}
					}
					else
					{
						if (Achievements.ShowLogs)
						{
							Debug.Log(string.Concat(new string[]
							{
								"# Ticking stat: ",
								this.StatKey,
								" [",
								this.Key,
								"]"
							}));
						}
						AccountInfo.SetIntStat(this, this.CurrentValue + 1);
					}
				}
				if (this.Completed || AccountInfo.IsAchievementUnlocked(this))
				{
					if (Achievements.ShowLogs)
					{
						Debug.Log("# Unlocked [" + this.Key + "] after ticking " + this.StatKey);
					}
					this.Completed = true;
					this.Clear();
				}
			}
			catch (Exception exception)
			{
				if (Achievements.ShowExceptions)
				{
					Debug.LogException(exception);
				}
			}
		}

		public void Unlock(object o)
		{
			try
			{
				if (this.CustomAction != null)
				{
					if (Achievements.ShowLogs)
					{
						Debug.Log("# Doing custom unlock action for [" + this.Key + "]");
					}
					if (!this.CustomAction(this, o))
					{
						return;
					}
				}
				else
				{
					if (Achievements.ShowLogs)
					{
						Debug.Log("# Unlocked [" + this.Key + "]");
					}
					AccountInfo.UnlockAchievement(this);
				}
				if (this.Completed || AccountInfo.IsAchievementUnlocked(this))
				{
					this.Completed = true;
					this.Clear();
				}
			}
			catch (Exception exception)
			{
				if (Achievements.ShowExceptions)
				{
					Debug.LogException(exception);
				}
			}
		}

		private void ReleaseHandles()
		{
			if (this.AchHandle != null)
			{
				this.AchHandle.Dispose();
				this.AchHandle = null;
			}
			if (this.StatHandle != null)
			{
				this.StatHandle.Dispose();
				this.StatHandle = null;
			}
		}

		public readonly string Name;

		public readonly string Description;

		public readonly string Key;

		public readonly string StatKey;

		public readonly List<int> UnlockValues;

		public InteropHelp.UTF8StringHandle AchHandle;

		public InteropHelp.UTF8StringHandle StatHandle;

		public int CurrentValue;

		public bool Completed;

		private Func<AchievementData, object, bool> CustomAction;

		private EventRegistry Registry;

		private object Event;
	}
}
