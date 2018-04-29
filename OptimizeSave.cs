using System;
using System.Collections;
using UnityEngine;


public class OptimizeSave : MonoBehaviour
{
	
	private void Start()
	{
		IEnumerator enumerator = base.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				transform.gameObject.SetActive(false);
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
