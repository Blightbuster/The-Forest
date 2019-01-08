using System;
using System.Collections;
using UnityEngine;

public class ColorMe : MonoBehaviourEx
{
	private void Start()
	{
		if (!base.GetComponent<UniqueIdentifier>().IsDeserializing)
		{
			base.StartCoroutine("DoColorMe");
		}
	}

	private IEnumerator DoColorMe()
	{
		Color color = base.GetComponent<Renderer>().material.color;
		Color target = Color.blue;
		for (;;)
		{
			float t = 0f;
			while (t < 1f)
			{
				base.GetComponent<Renderer>().material.color = Color.Lerp(color, target, t);
				t += Time.deltaTime / 3f;
				yield return null;
			}
			t = 0f;
			while (t < 1f)
			{
				base.GetComponent<Renderer>().material.color = Color.Lerp(target, color, t);
				t += Time.deltaTime / 3f;
				yield return null;
			}
		}
		yield break;
	}
}
