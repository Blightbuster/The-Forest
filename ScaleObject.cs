using System;
using System.Collections;
using UnityEngine;


public class ScaleObject : MonoBehaviour
{
	
	private void Start()
	{
		if (!base.GetComponent<UniqueIdentifier>().IsDeserializing)
		{
			this.StartExtendedCoroutine(this.ScaleMe());
		}
	}

	
	private IEnumerator ScaleMe()
	{
		Vector3 scale = base.transform.localScale;
		Vector3 newScale = scale * 5f;
		for (;;)
		{
			float t = 0f;
			while (t < 1f)
			{
				base.transform.localScale = Vector3.Lerp(scale, newScale, t);
				t += Time.deltaTime / 3f;
				yield return null;
			}
			t = 0f;
			while (t < 1f)
			{
				base.transform.localScale = Vector3.Lerp(newScale, scale, t);
				t += Time.deltaTime / 3f;
				yield return null;
			}
		}
		yield break;
	}
}
