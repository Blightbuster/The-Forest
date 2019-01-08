using System;
using System.Collections;
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
			IEnumerator enumerator = base.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					transform.gameObject.SetActive(enabled);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
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
