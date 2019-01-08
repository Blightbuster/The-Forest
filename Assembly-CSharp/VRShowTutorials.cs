using System;
using TheForest.Utils;
using UnityEngine;

public class VRShowTutorials : MonoBehaviour
{
	private void Awake()
	{
		this._materialProp = new MaterialPropertyBlock();
		this._renderer.enabled = false;
	}

	private void Update()
	{
		float num = Vector3.Distance(base.transform.position, LocalPlayer.MainCamTr.position);
		if (num < this._distanceToCam)
		{
			float num2 = Vector3.Dot(base.transform.up, LocalPlayer.MainCamTr.forward);
			if (num2 > this._dotMin && num2 < this._dotMax)
			{
				if (this._timeInView < this._timeInViewToShow)
				{
					this._timeInView += Time.deltaTime;
				}
			}
			else
			{
				this._timeInView = 0f;
			}
		}
		else
		{
			this._timeInView = 0f;
		}
		this.RefreshVisibility();
		this.RefreshHudCamVisibility();
		this.RefreshTutCamVisibility();
	}

	private void RefreshVisibility()
	{
		if (this._timeInView >= this._timeInViewToShow)
		{
			if (this._alpha < 1f)
			{
				if (!this._renderer.enabled)
				{
					this._renderer.enabled = true;
					Scene.HudGui.VR_Tut_Backing.Execute();
				}
				this._alpha += Time.deltaTime / this._fadeDuration;
				if (this._alpha > 1f)
				{
					this._alpha = 1f;
				}
				this.UpdateAlpha();
			}
		}
		else if (this._renderer.enabled)
		{
			this._alpha -= Time.deltaTime / this._fadeDuration;
			if (this._alpha > 0f)
			{
				this.UpdateAlpha();
			}
			else
			{
				this._renderer.enabled = false;
			}
		}
	}

	private void RefreshHudCamVisibility()
	{
		if (this._timeInView >= this._timeInViewToShowHud)
		{
			if (!Scene.HudGui.VRGuiCamC.enabled)
			{
				Scene.HudGui.VRGuiCamC.enabled = true;
			}
			if (this._powerOffGo.activeSelf)
			{
				this._powerOffGo.SetActive(false);
			}
			if (!this._hudGo.activeSelf)
			{
				this._hudGo.SetActive(true);
			}
		}
		else
		{
			if (Scene.HudGui.VRGuiCamC.enabled)
			{
				Scene.HudGui.VRGuiCamC.enabled = false;
			}
			if (!this._powerOffGo.activeSelf)
			{
				this._powerOffGo.SetActive(true);
			}
			if (this._hudGo.activeSelf)
			{
				this._hudGo.SetActive(false);
			}
		}
	}

	private void RefreshTutCamVisibility()
	{
		if (this._alpha <= 0f || !this.anyTutorialActive())
		{
			if (Scene.HudGui.VRTutorialCam.enabled)
			{
				Scene.HudGui.VRTutorialCam.enabled = false;
			}
		}
		else if (!Scene.HudGui.VRTutorialCam.enabled)
		{
			Scene.HudGui.VRTutorialCam.enabled = true;
		}
	}

	private void UpdateAlpha()
	{
		this._renderer.GetPropertyBlock(this._materialProp);
		this._materialProp.SetFloat("_CutOff", Mathf.Lerp(0f, 0.3f, this._alpha));
		this._renderer.SetPropertyBlock(this._materialProp);
	}

	private bool anyTutorialActive()
	{
		return Scene.HudGui.Tut_Axe.activeSelf || Scene.HudGui.Tut_Bloody.activeSelf || Scene.HudGui.Tut_BookStage1.activeSelf || Scene.HudGui.Tut_BookStage2.activeSelf || Scene.HudGui.Tut_BookStage3.activeSelf || Scene.HudGui.Tut_Cold.activeSelf || Scene.HudGui.Tut_ColdDamage.activeSelf || Scene.HudGui.Tut_DeathMP.activeSelf || Scene.HudGui.Tut_Energy.activeSelf || Scene.HudGui.Tut_Health.activeSelf || Scene.HudGui.Tut_Hungry.activeSelf || Scene.HudGui.Tut_Lighter.activeSelf || Scene.HudGui.Tut_MolotovTutorial.activeSelf || Scene.HudGui.Tut_NewBuildingsAvailable.activeSelf || Scene.HudGui.Tut_NoInventoryUnderwater.activeSelf || Scene.HudGui.Tut_Crafting.activeSelf || Scene.HudGui.Tut_ReviveMP.activeSelf || Scene.HudGui.Tut_OpenBook.activeSelf || Scene.HudGui.Tut_Shelter.activeSelf || Scene.HudGui.Tut_Sledding.activeSelf || Scene.HudGui.Tut_Starvation.activeSelf || Scene.HudGui.Tut_StoryClue.activeSelf || Scene.HudGui.Tut_ThirstDamage.activeSelf || Scene.HudGui.Tut_Thirsty.activeSelf;
	}

	public float _distanceToCam;

	public float _dotMin = -1f;

	public float _dotMax = -0.5f;

	public float _timeInViewToShow = 0.75f;

	public float _timeInViewToShowHud = 0.35f;

	public float _fadeDuration = 0.35f;

	public Renderer _renderer;

	public GameObject _powerOffGo;

	public GameObject _hudGo;

	private bool _visible;

	private float _timeInView;

	private float _alpha;

	private MaterialPropertyBlock _materialProp;
}
