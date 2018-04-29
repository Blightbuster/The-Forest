using System;
using UnityEngine;


public class PlatformSpecificContent : MonoBehaviour
{
	
	private void OnEnable()
	{
		this.CheckEnableContent();
	}

	
	private void CheckEnableContent()
	{
		if (this.showOnlyOn == PlatformSpecificContent.BuildTargetGroup.Mobile)
		{
			this.EnableContent(false);
		}
		else
		{
			this.EnableContent(true);
		}
	}

	
	private void EnableContent(bool enabled)
	{
		if (this.content.Length > 0)
		{
			foreach (GameObject gameObject in this.content)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(enabled);
				}
			}
		}
		if (this.childrenOfThisObject)
		{
			foreach (object obj in base.transform)
			{
				Transform transform = (Transform)obj;
				transform.gameObject.SetActive(enabled);
			}
		}
	}

	
	[SerializeField]
	private PlatformSpecificContent.BuildTargetGroup showOnlyOn;

	
	[SerializeField]
	private GameObject[] content = new GameObject[0];

	
	[SerializeField]
	private bool childrenOfThisObject;

	
	private enum BuildTargetGroup
	{
		
		Standalone,
		
		Mobile
	}
}
