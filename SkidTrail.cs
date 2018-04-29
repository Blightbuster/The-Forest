using System;
using System.Collections;
using UnityEngine;


public class SkidTrail : MonoBehaviour
{
	
	private IEnumerator Start()
	{
		for (;;)
		{
			yield return new WaitForSeconds(1f);
			if (base.transform.parent == null)
			{
				Color startCol = base.GetComponent<Renderer>().material.color;
				yield return new WaitForSeconds(this.persistTime);
				float t = Time.time;
				while (Time.time < t + this.fadeDuration)
				{
					float i = Mathf.InverseLerp(t, t + this.fadeDuration, Time.time);
					base.GetComponent<Renderer>().material.color = startCol * new Color(1f, 1f, 1f, 1f - i);
					yield return null;
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		yield break;
	}

	
	[SerializeField]
	private float persistTime;

	
	[SerializeField]
	private float fadeDuration;

	
	private float startAlpha;
}
