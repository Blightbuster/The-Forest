using System;
using PathologicalGames;
using TheForest.Buildings.Creation;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI.Anim
{
	
	public class BuildMissionWidget : MonoBehaviour
	{
		
		private void Reset()
		{
			if (!this._buildLog)
			{
				this._buildLog = base.GetComponentInChildren<BuildLog>();
			}
		}

		
		private void Update()
		{
			BuildMissionWidget.States state = this._state;
			if (state != BuildMissionWidget.States.CrossingOff)
			{
				if (state == BuildMissionWidget.States.Hiding)
				{
					if (this.CheckTweenersFinished(this._hideTweeners))
					{
						this.Pool();
					}
				}
			}
			else if (this.CheckTweenersFinished(this._crossOffTweeners))
			{
				this.Hide();
			}
		}

		
		public void Init(int itemId)
		{
			this._state = BuildMissionWidget.States.QueuedForDisplay;
			if (this._buildLog._itemId != itemId)
			{
				this._buildLog._itemId = itemId;
				this._itemNameLabel.text = Scene.HudGui.GetItemName(itemId, true, true);
				this._buildLog._label.UpdateAnchors();
			}
			if (this._crossOffGo.activeSelf)
			{
				this._crossOffGo.SetActive(false);
			}
		}

		
		public void Show()
		{
			this._state = BuildMissionWidget.States.Displayed;
			foreach (UIWidget uiwidget in this._hideWidgetsToReset)
			{
				uiwidget.alpha = 1f;
			}
			foreach (UITweener uitweener in this._showTweeners)
			{
				if (!uitweener.gameObject.activeSelf)
				{
					uitweener.gameObject.SetActive(true);
				}
				uitweener.ResetToBeginning();
				uitweener.PlayForward();
			}
			base.gameObject.SetActive(true);
			Scene.HudGui.Grid.repositionNow = true;
		}

		
		public void CrossOff()
		{
			Scene.HudGui.SetBuildMissionWidgetForItem(this._buildLog._itemId, null);
			this._state = BuildMissionWidget.States.CrossingOff;
			foreach (UITweener uitweener in this._crossOffTweeners)
			{
				if (!uitweener.gameObject.activeSelf)
				{
					uitweener.gameObject.SetActive(true);
				}
				uitweener.ResetToBeginning();
				uitweener.PlayForward();
			}
			if (!this._crossOffGo.activeSelf)
			{
				this._crossOffGo.SetActive(true);
			}
		}

		
		public void Hide()
		{
			Scene.HudGui.SetBuildMissionWidgetForItem(this._buildLog._itemId, null);
			this._state = BuildMissionWidget.States.Hiding;
			foreach (UITweener uitweener in this._hideTweeners)
			{
				uitweener.ResetToBeginning();
				uitweener.PlayForward();
			}
		}

		
		public void Pool()
		{
			this._state = BuildMissionWidget.States.Idle;
			if (PoolManager.Pools["misc"].IsSpawned(base.transform))
			{
				PoolManager.Pools["misc"].Despawn(base.transform);
			}
			else
			{
				Debug.Log("Not pooled BM widget ??");
				UnityEngine.Object.Destroy(base.gameObject);
			}
			Scene.HudGui.Grid.repositionNow = true;
		}

		
		private bool CheckTweenersFinished(UITweener[] tweenerArray)
		{
			foreach (UITweener uitweener in tweenerArray)
			{
				if (uitweener.enabled)
				{
					return false;
				}
			}
			return true;
		}

		
		public BuildLog _buildLog;

		
		public UILabel _itemNameLabel;

		
		public UITweener[] _showTweeners;

		
		public UITweener[] _hideTweeners;

		
		public UITweener[] _crossOffTweeners;

		
		public UIWidget[] _hideWidgetsToReset;

		
		public GameObject _crossOffGo;

		
		public BuildMissionWidget.States _state;

		
		public enum States
		{
			
			Idle,
			
			QueuedForDisplay,
			
			Displayed,
			
			CrossingOff,
			
			Hiding
		}
	}
}
