using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class VRInventoryTreeDisabler : MonoBehaviour
{
	
	private void OnEnable()
	{
		if (!ForestVR.Enabled)
		{
			return;
		}
		base.StartCoroutine("disableTreesRoutine");
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
			if (component)
			{
				bool flag = false;
				UnderfootSurface component2 = allCol[i].transform.GetComponent<UnderfootSurface>();
				if (component2 && component2.surfaceType == UnderfootSurfaceDetector.SurfaceType.Rock)
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
			for (int j = 0; j < this.culledRenderers.Count; j++)
			{
				this.culledRenderers[j].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
				MeshRenderer[] componentsInChildren = this.culledRenderers[j].GetComponentsInChildren<MeshRenderer>();
				for (int k = 0; k < componentsInChildren.Length; k++)
				{
					componentsInChildren[k].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
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
}
