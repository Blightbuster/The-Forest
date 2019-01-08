using System;
using System.Collections;
using UnityEngine;

[AddComponentMenu("Storage/Tests/Coroutines")]
public class TestCoroutines : MonoBehaviour
{
	private void Start()
	{
		if (!LevelSerializer.IsDeserializing)
		{
			base.gameObject.StartExtendedCoroutine(this.MyCoroutine());
		}
		base.StartCoroutine("Hello");
	}

	private IEnumerator Hello()
	{
		int a = 1000;
		for (;;)
		{
			a++;
			yield return base.StartCoroutine(this.WaitSeconds(10f));
		}
		yield break;
	}

	private IEnumerator MyCoroutine()
	{
		int a = 0;
		for (;;)
		{
			a++;
			yield return this.WaitSeconds(1f);
		}
		yield break;
	}

	private IEnumerator WaitSeconds(float time)
	{
		for (float t = 0f; t < time; t += Time.deltaTime)
		{
			yield return null;
		}
		yield break;
	}
}
