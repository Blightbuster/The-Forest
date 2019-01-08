using System;
using System.Collections;
using Bolt;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	[DoNotSerializePublic]
	public class HomeIcon : EntityBehaviour<IBuildingDestructibleState>
	{
		private void Start()
		{
			base.enabled = false;
			if (!LevelSerializer.IsDeserializing)
			{
				base.StartCoroutine(this.DelayedSetupIcon());
			}
		}

		private IEnumerator DelayedSetupIcon()
		{
			yield return YieldPresets.WaitForEndOfFrame;
			yield return YieldPresets.WaitForEndOfFrame;
			this.SetupIcon();
			yield break;
		}

		private void Update()
		{
			if (base.enabled)
			{
				if (TheForest.Utils.Input.GetButtonDown("Rotate"))
				{
					this.SetColorIndex(this._color + 1);
				}
				Scene.HudGui.OverlayIconWidgets[(int)this._type].ShowList(this._color, base.transform, SideIcons.None);
			}
		}

		private void OnDisable()
		{
			if (Scene.HudGui)
			{
				Scene.HudGui.OverlayIconWidgets[(int)this._type].Shutdown();
			}
		}

		private void OnDeserialized()
		{
			if (base.GetComponent<EmptyObjectIdentifier>())
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			if (!BoltNetwork.isRunning || (base.entity.isAttached && base.entity.isOwner))
			{
				this.SetupIcon();
			}
		}

		private void GrabEnter()
		{
			base.enabled = true;
			Scene.HudGui.OverlayIconWidgets[(int)this._type].ShowList(this._color, base.transform, SideIcons.None);
			if (this._iconGo.activeSelf)
			{
				this._iconGo.SetActive(false);
				if (this._vrIconGo)
				{
					this._vrIconGo.SetActive(false);
				}
			}
		}

		private void GrabExit()
		{
			base.enabled = false;
			Scene.HudGui.OverlayIconWidgets[(int)this._type].Shutdown();
			bool flag = this._color > 0;
			if (this._iconGo.activeSelf != flag)
			{
				this._iconGo.SetActive(flag);
				if (this._vrIconGo)
				{
					this._vrIconGo.SetActive(flag);
				}
			}
		}

		public void SetColorIndex(int index)
		{
			this._color = index % Scene.HudGui.OverlayIconWidgets[(int)this._type]._rotationItems.Length;
			if (BoltNetwork.isRunning && base.entity && base.entity.isAttached)
			{
				this.SetMpStateFlagAny();
			}
			else
			{
				this.SetupIcon();
			}
		}

		public void SetupIcon()
		{
			OverlayIcon component = this._iconGo.GetComponent<OverlayIcon>();
			if (component)
			{
				component.SetSubType(this._color);
			}
			bool flag = this._color > 0 && !base.enabled;
			if (this._iconGo.activeSelf != flag)
			{
				this._iconGo.SetActive(flag);
				if (this._vrIconGo)
				{
					this._vrIconGo.SetActive(flag);
				}
			}
			this._guiTex.color = this.CurrentColor;
		}

		public override void Attached()
		{
			if (base.entity.isOwner)
			{
				this.SetMpStateFlagAny();
				base.state.AddCallback("Flag", new PropertyCallbackSimple(this.OnMpStateFlagUpdated));
				base.StartCoroutine(this.DelayedMpStateUpdate());
			}
			else
			{
				base.StartCoroutine(this.WaitForParenting());
			}
		}

		private IEnumerator DelayedMpStateUpdate()
		{
			yield return YieldPresets.WaitForEndOfFrame;
			yield return YieldPresets.WaitForEndOfFrame;
			this.OnMpStateFlagUpdated();
			yield break;
		}

		private IEnumerator WaitForParenting()
		{
			CoopAutoParent cap = base.GetComponentInParent<CoopAutoParent>();
			if (cap)
			{
				while (!cap.Ready)
				{
					yield return null;
				}
			}
			base.state.AddCallback("Flag", new PropertyCallbackSimple(this.OnMpStateFlagUpdated));
			this.OnMpStateFlagUpdated();
			yield break;
		}

		private void OnMpStateFlagUpdated()
		{
			this._color = base.state.Flag;
			this.SetupIcon();
		}

		public void SetMpStateFlagHost(int flag)
		{
			base.state.Flag = flag;
		}

		private void SetMpStateFlagAny()
		{
			if (base.entity.isOwner)
			{
				this.SetMpStateFlagHost(this._color);
			}
			else
			{
				SetBuildingFlag setBuildingFlag = SetBuildingFlag.Create(base.entity.source);
				setBuildingFlag.Flag = this._color;
				setBuildingFlag.BuildingEntity = base.entity;
				setBuildingFlag.Send();
			}
		}

		public Color CurrentColor
		{
			get
			{
				Color color = Scene.HudGui.OverlayIconWidgets[(int)this._type]._rotationItems[this._color]._icon.color;
				color.a = 0.4f;
				return color;
			}
		}

		public GameObject _iconGo;

		public GUITexture _guiTex;

		public GameObject _vrIconGo;

		[SerializeThis]
		public int _color;

		public OverlayIconTypes _type;
	}
}
