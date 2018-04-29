using System;
using UnityEngine;


public class disableBlur : MonoBehaviour
{
	
	private void Start()
	{
		if (this.cleanAmplify)
		{
			base.Invoke("doRemove", 0.05f);
		}
	}

	
	private void OnEnable()
	{
		if (this.cleanAmplify)
		{
			base.Invoke("doRemove", 0.05f);
		}
	}

	
	private void doRemove()
	{
		ParticleSystem[] componentsInChildren = base.transform.GetComponentsInChildren<ParticleSystem>(true);
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			AmplifyMotionObjectBase component = particleSystem.GetComponent<AmplifyMotionObjectBase>();
			if (component)
			{
				UnityEngine.Object.Destroy(component);
			}
		}
	}

	
	public bool cleanAmplify;
}
