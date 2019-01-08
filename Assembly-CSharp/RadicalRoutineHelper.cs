using System;
using System.Collections.Generic;
using System.Reflection;
using Serialization;
using UnityEngine;

[Deferred]
[AddComponentMenu("Storage/Resumable Coroutine Support")]
public class RadicalRoutineHelper : MonoBehaviour, IDeserialized
{
	static RadicalRoutineHelper()
	{
		DelegateSupport.RegisterFunctionType<RadicalRoutineHelper, string>();
		DelegateSupport.RegisterFunctionType<RadicalRoutineHelper, bool>();
		DelegateSupport.RegisterFunctionType<RadicalRoutineHelper, Transform>();
	}

	public static RadicalRoutineHelper Current
	{
		get
		{
			if (RadicalRoutineHelper._current == null)
			{
				GameObject gameObject = new GameObject("Radical Routine Helper (AUTO)");
				RadicalRoutineHelper._current = gameObject.AddComponent<RadicalRoutineHelper>();
			}
			return RadicalRoutineHelper._current;
		}
	}

	private void Awake()
	{
		if (!base.GetComponent<StoreInformation>())
		{
			UniqueIdentifier component;
			if (component = base.GetComponent<UniqueIdentifier>())
			{
				string id = component.Id;
				UnityEngine.Object.DestroyImmediate(component);
				EmptyObjectIdentifier emptyObjectIdentifier = base.gameObject.AddComponent<EmptyObjectIdentifier>();
				emptyObjectIdentifier.Id = id;
			}
			else
			{
				base.gameObject.AddComponent<EmptyObjectIdentifier>();
			}
		}
	}

	private void OnDestroy()
	{
		if (RadicalRoutineHelper._current == this)
		{
			RadicalRoutineHelper._current = null;
		}
	}

	public void Run(RadicalRoutine routine)
	{
		this.Running.Add(routine);
		if (routine.trackedObject)
		{
			routine.trackedObject.StartCoroutine(routine.enumerator);
		}
		else
		{
			base.StartCoroutine(routine.enumerator);
		}
	}

	public void Finished(RadicalRoutine routine)
	{
		this.Running.Remove(routine);
		if (!string.IsNullOrEmpty(routine.Method) && routine.Target != null)
		{
			try
			{
				MethodInfo method = routine.Target.GetType().GetMethod(routine.Method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null)
				{
					method.Invoke(routine.Target, new object[0]);
				}
			}
			catch
			{
			}
		}
	}

	void IDeserialized.Deserialized()
	{
		try
		{
			Loom.QueueOnMainThread(delegate
			{
				foreach (RadicalRoutine radicalRoutine in this.Running)
				{
					try
					{
						if (radicalRoutine.trackedObject)
						{
							radicalRoutine.trackedObject.StartCoroutine(radicalRoutine.enumerator);
						}
						else
						{
							base.StartCoroutine(radicalRoutine.enumerator);
						}
					}
					catch (Exception ex)
					{
					}
				}
			}, 0.02f);
		}
		catch (Exception ex)
		{
		}
	}

	private static RadicalRoutineHelper _current;

	public List<RadicalRoutine> Running = new List<RadicalRoutine>();
}
