using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[DoNotSerializePublic]
	[RequireComponent(typeof(GUITexture))]
	public class OverlayIcon : MonoBehaviour
	{
		
		private void Start()
		{
			if (this.ID == 0)
			{
				Vector3 worldPosition = (!this.target) ? ((!base.transform.parent) ? base.transform.position : base.transform.parent.position) : this.target.position;
				float num = Terrain.activeTerrain.SampleHeight(worldPosition) + Terrain.activeTerrain.transform.position.y - 3f;
				this.IsInCaves = (worldPosition.y < num);
			}
			this.IgnoreInCaveStatus = Scene.IsInSinkhole((!base.transform.parent) ? base.transform.position : base.transform.parent.position);
			this.ToggleIcons(false);
			if (this._type == OverlayIconTypes.Hammer)
			{
				OverlayIconManager.Register(this);
			}
		}

		
		private void OnDestroy()
		{
			if (!MenuMain.exitingToMenu)
			{
				OverlayIconManager.Unregister(this);
			}
		}

		
		public void DoUpdate(bool showing, bool checkGroup)
		{
			this._showing = showing;
			if (this.target == null)
			{
				if (base.transform.parent != null)
				{
					this.target = base.transform.parent;
				}
				else
				{
					this.target = base.transform;
				}
				this._showing = false;
				this.ToggleIcons(false);
				return;
			}
			if (this._showing && LocalPlayer.MainCam)
			{
				if (!checkGroup)
				{
					if (this.ShouldRefreshTargetPosition)
					{
						this._relativePosition = LocalPlayer.MainCamTr.InverseTransformPoint(this.FinalTargetPosition);
					}
					else
					{
						this._relativePosition = LocalPlayer.MainCamTr.InverseTransformPoint(this.TargetPosition);
					}
				}
				else if (this.GroupDisplay)
				{
					if (this.SuperGroupDisplay && !this.BreakGroup)
					{
						if (!this.BreakSuperGroup)
						{
							this._relativePosition = LocalPlayer.MainCamTr.InverseTransformPoint(this.SuperGroupTargetPosition);
						}
						else if (this.BreakSuperGroupAlpha < 1f)
						{
							this._relativePosition = LocalPlayer.MainCamTr.InverseTransformPoint(Vector3.Lerp(this.SuperGroupTargetPosition, this.GroupTargetPosition, MathEx.Easing.EaseInQuart(this.BreakSuperGroupAlpha, 0f, 1f, 1f)));
						}
						else
						{
							this._relativePosition = LocalPlayer.MainCamTr.InverseTransformPoint(this.GroupTargetPosition);
						}
					}
					else if (this.CurrentSuperGroup && this.CurrentSuperGroup.BreakSuperGroupAlpha < 1f)
					{
						this.BreakSuperGroup = true;
						this.BreakSuperGroupAlpha = this.CurrentSuperGroup.BreakSuperGroupAlpha;
						this._relativePosition = LocalPlayer.MainCamTr.InverseTransformPoint(Vector3.Lerp(this.CurrentSuperGroup.SuperGroupTargetPosition, this.GroupTargetPosition, MathEx.Easing.EaseInQuart(this.BreakSuperGroupAlpha, 0f, 1f, 1f)));
					}
					else if (!this.BreakGroup)
					{
						this._relativePosition = LocalPlayer.MainCamTr.InverseTransformPoint(this.GroupTargetPosition);
					}
					else if (this.BreakGroupAlpha < 1f)
					{
						this._relativePosition = LocalPlayer.MainCamTr.InverseTransformPoint(Vector3.Lerp(this.GroupTargetPosition, this.TargetPosition, MathEx.Easing.EaseInQuart(this.BreakGroupAlpha, 0f, 1f, 1f)));
					}
					else
					{
						this._relativePosition = LocalPlayer.MainCamTr.InverseTransformPoint(this.TargetPosition);
					}
				}
				else if (this.CurrentGroup && this.CurrentGroup.BreakGroupAlpha < 1f)
				{
					this.BreakGroup = true;
					this.BreakGroupAlpha = this.CurrentGroup.BreakGroupAlpha;
					this._relativePosition = LocalPlayer.MainCamTr.InverseTransformPoint(Vector3.Lerp(this.CurrentGroup.GroupTargetPosition, this.FinalTargetPosition, MathEx.Easing.EaseInQuart(this.BreakGroupAlpha, 0f, 1f, 1f)));
				}
				else
				{
					this._relativePosition = LocalPlayer.MainCamTr.InverseTransformPoint(this.FinalTargetPosition);
				}
				this._relativePosition.z = Mathf.Max(this._relativePosition.z, 0.1f);
				base.transform.position = LocalPlayer.MainCam.WorldToViewportPoint(LocalPlayer.MainCamTr.TransformPoint(this._relativePosition + this._offset));
				this.ToggleIcons(true);
			}
			else
			{
				this.ToggleIcons(false);
			}
		}

		
		public void SetSubType(int subtype)
		{
			if (this._subtype != subtype)
			{
				if (this.ID == 0)
				{
					this.ID = OverlayIconManager.GetNewId();
					Vector3 worldPosition = (!this.target) ? ((!base.transform.parent) ? base.transform.position : base.transform.parent.position) : this.target.position;
					float num = Terrain.activeTerrain.SampleHeight(worldPosition) + Terrain.activeTerrain.transform.position.y - 3f;
					this.IsInCaves = (worldPosition.y < num);
				}
				OverlayIconManager.Unregister(this);
				this._subtype = subtype;
				OverlayIconManager.Register(this);
			}
		}

		
		private void ToggleIcons(bool on)
		{
			foreach (GUITexture guitexture in this._icons)
			{
				guitexture.enabled = on;
			}
		}

		
		
		
		public bool IsInCaves { get; private set; }

		
		
		
		public bool IgnoreInCaveStatus { get; private set; }

		
		
		
		public bool BreakGroup { get; set; }

		
		
		
		public bool BreakSuperGroup { get; set; }

		
		
		public bool GroupDisplay
		{
			get
			{
				return !this.CurrentGroup;
			}
		}

		
		
		public bool SuperGroupDisplay
		{
			get
			{
				return !this.CurrentSuperGroup;
			}
		}

		
		
		
		public float GroupMinBreakRange { get; set; }

		
		
		
		public float SuperGroupMinBreakRange { get; set; }

		
		
		
		public int ID { get; set; }

		
		
		public int Type
		{
			get
			{
				return (int)(this._type * (OverlayIconTypes)6 + this._subtype + ((!this.IsInCaves) ? 0 : 13));
			}
		}

		
		
		
		public bool ShouldRefreshTargetPosition { get; internal set; }

		
		
		
		public Vector3 TargetPosition { get; set; }

		
		
		
		public Vector3 GroupTargetPosition { get; set; }

		
		
		
		public Vector3 SuperGroupTargetPosition { get; set; }

		
		
		
		public Vector3 FinalTargetPosition { get; set; }

		
		
		
		public OverlayIcon CurrentGroup { get; set; }

		
		
		
		public OverlayIcon CurrentSuperGroup { get; set; }

		
		
		
		public Dictionary<int, float> DistanceToOtherIcons { get; set; }

		
		
		
		public int InGroupingRangeIcons { get; set; }

		
		
		
		public int InSuperGroupingRangeIcons { get; internal set; }

		
		
		
		public float BreakGroupAlpha { get; set; }

		
		
		
		public float BreakSuperGroupAlpha { get; set; }

		
		public GUITexture[] _icons;

		
		public Transform target;

		
		public Vector3 _offset = Vector3.zero;

		
		public bool _showing;

		
		public OverlayIconTypes _type;

		
		public int _subtype;

		
		private Vector3 _relativePosition;
	}
}
