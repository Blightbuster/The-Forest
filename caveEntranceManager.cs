using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;


public class caveEntranceManager : MonoBehaviour
{
	
	private void Start()
	{
		if (Scene.SceneTracker && !Scene.SceneTracker.caveEntrances.Contains(this))
		{
			Scene.SceneTracker.caveEntrances.Add(this);
		}
		if (LocalPlayer.IsInCaves || LocalPlayer.IsInEndgame)
		{
			this.disableCaveBlack();
		}
		else
		{
			this.enableCaveBlack();
		}
	}

	
	public void disableCaveBlack()
	{
		if (this.fadeToDarkGo)
		{
			this.fadeToDarkGo.SetActive(false);
		}
		if (this.blackBackingGo)
		{
			this.blackBackingGo.SetActive(false);
		}
		if (this.blackBackingFadeGo)
		{
			this.blackBackingFadeGo.SetActive(false);
		}
	}

	
	public void enableCaveBlack()
	{
		if (this.fadeToDarkGo)
		{
			this.fadeToDarkGo.SetActive(true);
		}
		if (this.blackBackingFadeGo)
		{
			this.blackBackingFadeGo.SetActive(true);
		}
		if (this.blackBackingGo)
		{
			this.blackBackingGo.SetActive(true);
		}
		if (this.climbEntrance)
		{
			return;
		}
		if (this.fadeToDarkGo)
		{
			Material sharedMaterial = this.fadeToDarkGo.GetComponent<MeshRenderer>().sharedMaterial;
			Color color = sharedMaterial.color;
			color.a = 0.741f;
			sharedMaterial.SetColor("_Color", color);
		}
		if (this.blackBackingFadeGo)
		{
			Material sharedMaterial2 = this.blackBackingFadeGo.GetComponent<MeshRenderer>().sharedMaterial;
			Color color2 = sharedMaterial2.color;
			color2.a = 1f;
			sharedMaterial2.SetColor("_Color", color2);
		}
	}

	
	public IEnumerator enableCaveBlackRoutine()
	{
		LocalPlayer.AnimControl.exitingACave = true;
		yield return YieldPresets.WaitThreeSeconds;
		if (this.climbEntrance)
		{
			if (this.blackBackingGo)
			{
				this.blackBackingGo.SetActive(true);
			}
			if (this.fadeToDarkGo)
			{
				this.fadeToDarkGo.SetActive(true);
			}
			this.enableAllCaveEntrances();
			LocalPlayer.AnimControl.exitingACave = false;
			yield break;
		}
		if (this.fadeToDarkGo)
		{
			this.fadeToDarkGo.SetActive(true);
		}
		if (this.blackBackingGo)
		{
			this.blackBackingGo.SetActive(true);
		}
		if (this.blackBackingFadeGo)
		{
			this.blackBackingFadeGo.SetActive(false);
		}
		if (this.fadeToDarkGo)
		{
			Material sharedMaterial = this.fadeToDarkGo.GetComponent<MeshRenderer>().sharedMaterial;
			Color color = sharedMaterial.color;
			color.a = 0.741f;
			sharedMaterial.SetColor("_Color", color);
		}
		if (this.blackBackingFadeGo)
		{
			Material sharedMaterial2 = this.blackBackingFadeGo.GetComponent<MeshRenderer>().sharedMaterial;
			Color color2 = sharedMaterial2.color;
			color2.a = 1f;
			sharedMaterial2.SetColor("_Color", color2);
		}
		this.enableAllCaveEntrances();
		yield return YieldPresets.WaitThreeSeconds;
		LocalPlayer.AnimControl.exitingACave = false;
		yield break;
	}

	
	public IEnumerator disableCaveBlackRoutine()
	{
		if (this.fadeToDarkGo == null | this.blackBackingGo == null)
		{
			yield break;
		}
		if (this.climbEntrance)
		{
			if (this.blackBackingGo)
			{
				this.blackBackingGo.SetActive(false);
			}
			if (this.fadeToDarkGo)
			{
				this.fadeToDarkGo.SetActive(false);
			}
			this.disableAllCaveEntrances();
			yield break;
		}
		float breakTimer = Time.time + 10f;
		while (!LocalPlayer.IsInCaves)
		{
			if (Time.time > breakTimer)
			{
				break;
			}
			yield return null;
		}
		this.blackBackingGo.SetActive(false);
		if (this.blackBackingFadeGo)
		{
			this.blackBackingFadeGo.SetActive(true);
		}
		Material fadeMat = this.fadeToDarkGo.GetComponent<MeshRenderer>().sharedMaterial;
		Material blackMat = this.blackBackingFadeGo.GetComponent<MeshRenderer>().sharedMaterial;
		Color blackColor = blackMat.color;
		Color color = fadeMat.color;
		float t = 0f;
		while (t < 1f)
		{
			Scene.Atmosphere.overrideVisibility = true;
			Scene.Atmosphere.Visibility = 3000f;
			blackColor.a = 1f - t;
			color.a = 0.741f - t;
			fadeMat.SetColor("_Color", color);
			blackMat.SetColor("_Color", blackColor);
			t += Time.deltaTime / 4f;
			yield return null;
		}
		this.fadeToDarkGo.SetActive(false);
		this.blackBackingGo.SetActive(false);
		this.blackBackingFadeGo.SetActive(false);
		Scene.Atmosphere.overrideVisibility = false;
		this.disableAllCaveEntrances();
		yield break;
	}

	
	private void disableAllCaveEntrances()
	{
		for (int i = 0; i < Scene.SceneTracker.caveEntrances.Count; i++)
		{
			Scene.SceneTracker.caveEntrances[i].disableCaveBlack();
		}
	}

	
	private void enableAllCaveEntrances()
	{
		for (int i = 0; i < Scene.SceneTracker.caveEntrances.Count; i++)
		{
			Scene.SceneTracker.caveEntrances[i].enableCaveBlack();
		}
	}

	
	public GameObject blackBackingGo;

	
	public GameObject blackBackingFadeGo;

	
	public GameObject fadeToDarkGo;

	
	public bool climbEntrance;
}
