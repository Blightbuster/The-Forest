using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering;

public class VRInventoryTreeDisabler : MonoBehaviour
{
	private void Awake()
	{
		this.DefaultLayerMask = LocalPlayer.MainCam.cullingMask;
		this.fakeCaveVR.SetActive(false);
	}

	private void OnEnable()
	{
		if (!ForestVR.Enabled)
		{
			return;
		}
		if (LocalPlayer.IsInCaves && !LocalPlayer.IsInEndgame)
		{
			this.enableFakeCave();
		}
		else
		{
			this.disableFakeCave();
		}
		base.StartCoroutine("disableTreesRoutine");
	}

	private void enableFakeCave()
	{
		Debug.Log("fake cave enabled");
		this.fakeCaveVR.SetActive(true);
		LocalPlayer.MainCam.cullingMask = this.InCaveMask;
		LocalPlayer.vrAdapter.VRPlayerHands.gameObject.layer = 23;
	}

	private void disableFakeCave()
	{
		this.fakeCaveVR.SetActive(false);
		LocalPlayer.MainCam.cullingMask = this.DefaultLayerMask;
		LocalPlayer.vrAdapter.VRPlayerHands.gameObject.layer = 18;
	}

	private IEnumerator disableTreesRoutine()
	{
		yield return YieldPresets.WaitForEndOfFrame;
		this.culledRenderers.Clear();
		Collider[] allCol = Physics.OverlapSphere(base.transform.position, 5f, this.treeMask, QueryTriggerInteraction.Collide);
		for (int i = 0; i < allCol.Length; i++)
		{
			MeshRenderer component = allCol[i].transform.GetComponent<MeshRenderer>();
			if (!component && allCol[i].transform.parent)
			{
				component = allCol[i].transform.parent.GetComponent<MeshRenderer>();
			}
			if (allCol[i].transform.parent && (allCol[i].transform.parent.GetComponent<TreeHealth>() || allCol[i].transform.GetComponent<ExplodeTreeStump>()))
			{
				MeshRenderer[] componentsInChildren = allCol[i].transform.parent.GetComponentsInChildren<MeshRenderer>();
				foreach (MeshRenderer item in componentsInChildren)
				{
					this.culledRenderers.Add(item);
				}
			}
			if (component)
			{
				bool flag = false;
				UnderfootSurface component2 = allCol[i].transform.GetComponent<UnderfootSurface>();
				if (component2 && component2.surfaceType == UnderfootSurfaceDetector.SurfaceType.Rock)
				{
					flag = true;
				}
				if (component.transform.GetComponent<ExplodeTreeStump>())
				{
					flag = true;
				}
				if (component.transform.GetComponent<TreeHealth>())
				{
					flag = true;
				}
				if (component.transform.GetComponent<BushDamage>())
				{
					flag = true;
				}
				if (component.transform.GetComponent<CutBush2>())
				{
					flag = true;
				}
				if (flag)
				{
					this.culledRenderers.Add(component);
				}
			}
		}
		if (this.culledRenderers.Count > 0)
		{
			for (int k = 0; k < this.culledRenderers.Count; k++)
			{
				this.culledRenderers[k].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
				MeshRenderer[] componentsInChildren2 = this.culledRenderers[k].GetComponentsInChildren<MeshRenderer>();
				for (int l = 0; l < componentsInChildren2.Length; l++)
				{
					componentsInChildren2[l].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
				}
			}
		}
		yield break;
	}

	private void OnDisable()
	{
		if (!ForestVR.Enabled)
		{
			return;
		}
		this.disableFakeCave();
		base.StopCoroutine("disableTreesRoutine");
		if (this.culledRenderers.Count > 0)
		{
			for (int i = 0; i < this.culledRenderers.Count; i++)
			{
				if (this.culledRenderers[i])
				{
					this.culledRenderers[i].shadowCastingMode = ShadowCastingMode.On;
					MeshRenderer[] componentsInChildren = this.culledRenderers[i].GetComponentsInChildren<MeshRenderer>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						componentsInChildren[j].shadowCastingMode = ShadowCastingMode.On;
					}
				}
			}
		}
	}

	public List<MeshRenderer> culledRenderers = new List<MeshRenderer>();

	public LayerMask treeMask;

	public LayerMask DefaultLayerMask;

	public LayerMask InCaveMask;

	public GameObject fakeCaveVR;
}
