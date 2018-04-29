using System;
using System.Collections;
using UnityEngine;


public class TimedObjectActivator : MonoBehaviour
{
	
	private void Awake()
	{
		foreach (TimedObjectActivator.Entry entry in this.entries.entries)
		{
			switch (entry.action)
			{
			case TimedObjectActivator.Action.Activate:
				base.StartCoroutine(this.Activate(entry));
				break;
			case TimedObjectActivator.Action.Deactivate:
				base.StartCoroutine(this.Deactivate(entry));
				break;
			case TimedObjectActivator.Action.Destroy:
				UnityEngine.Object.Destroy(entry.target, entry.delay);
				break;
			case TimedObjectActivator.Action.ReloadLevel:
				base.StartCoroutine(this.ReloadLevel(entry));
				break;
			}
		}
	}

	
	private IEnumerator Activate(TimedObjectActivator.Entry entry)
	{
		yield return new WaitForSeconds(entry.delay);
		entry.target.SetActive(true);
		yield break;
	}

	
	private IEnumerator Deactivate(TimedObjectActivator.Entry entry)
	{
		yield return new WaitForSeconds(entry.delay);
		entry.target.SetActive(false);
		yield break;
	}

	
	private IEnumerator ReloadLevel(TimedObjectActivator.Entry entry)
	{
		yield return new WaitForSeconds(entry.delay);
		Application.LoadLevel(Application.loadedLevel);
		yield break;
	}

	
	public TimedObjectActivator.Entries entries = new TimedObjectActivator.Entries();

	
	public enum Action
	{
		
		Activate,
		
		Deactivate,
		
		Destroy,
		
		ReloadLevel,
		
		Call
	}

	
	[Serializable]
	public class Entry
	{
		
		public GameObject target;

		
		public TimedObjectActivator.Action action;

		
		public float delay;
	}

	
	[Serializable]
	public class Entries
	{
		
		public TimedObjectActivator.Entry[] entries;
	}
}
