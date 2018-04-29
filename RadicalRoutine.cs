using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RadicalRoutine : IDeserialized
{
	
	
	
	public MonoBehaviour trackedObject
	{
		get
		{
			return this._trackedObject;
		}
		set
		{
			this._trackedObject = value;
			this.isTracking = (this._trackedObject != null);
		}
	}

	
	
	
	public event Action Cancelled = delegate
	{
	};

	
	
	
	public event Action Finished = delegate
	{
	};

	
	public void Cancel()
	{
		this.cancel = true;
		if (this.extended is INotifyStartStop)
		{
			(this.extended as INotifyStartStop).Stop();
		}
	}

	
	public static RadicalRoutine Run(IEnumerator extendedCoRoutine)
	{
		return RadicalRoutine.Run(extendedCoRoutine, string.Empty, null);
	}

	
	public static RadicalRoutine Run(IEnumerator extendedCoRoutine, string methodName)
	{
		return RadicalRoutine.Run(extendedCoRoutine, methodName, null);
	}

	
	public static RadicalRoutine Run(IEnumerator extendedCoRoutine, string methodName, object target)
	{
		RadicalRoutine radicalRoutine = new RadicalRoutine();
		radicalRoutine.Method = methodName;
		radicalRoutine.Target = target;
		radicalRoutine.extended = extendedCoRoutine;
		if (radicalRoutine.extended is INotifyStartStop)
		{
			(radicalRoutine.extended as INotifyStartStop).Start();
		}
		radicalRoutine.enumerator = radicalRoutine.Execute(extendedCoRoutine);
		RadicalRoutineHelper.Current.Run(radicalRoutine);
		return radicalRoutine;
	}

	
	public static RadicalRoutine Create(IEnumerator extendedCoRoutine)
	{
		RadicalRoutine radicalRoutine = new RadicalRoutine();
		radicalRoutine.extended = extendedCoRoutine;
		if (radicalRoutine.extended is INotifyStartStop)
		{
			(radicalRoutine.extended as INotifyStartStop).Start();
		}
		radicalRoutine.enumerator = radicalRoutine.Execute(extendedCoRoutine);
		return radicalRoutine;
	}

	
	public void Run()
	{
		this.Run(string.Empty, null);
	}

	
	public void Run(string methodName)
	{
		this.Run(methodName, null);
	}

	
	public void Run(string methodName, object target)
	{
		this.Method = methodName;
		this.Target = target;
		if (this.trackedObject != null)
		{
			RadicalRoutineHelper radicalRoutineHelper = this.trackedObject.GetComponent<RadicalRoutineHelper>() ?? this.trackedObject.gameObject.AddComponent<RadicalRoutineHelper>();
			radicalRoutineHelper.Run(this);
		}
		else
		{
			RadicalRoutineHelper.Current.Run(this);
		}
	}

	
	private IEnumerator Execute(IEnumerator extendedCoRoutine)
	{
		return this.Execute(extendedCoRoutine, null);
	}

	
	private IEnumerator Execute(IEnumerator extendedCoRoutine, Action complete)
	{
		Stack<IEnumerator> stack = new Stack<IEnumerator>();
		stack.Push(extendedCoRoutine);
		while (stack.Count > 0)
		{
			extendedCoRoutine = stack.Pop();
			while (!this.cancel && extendedCoRoutine != null && (!this.isTracking || (this.trackedObject != null && this.trackedObject.enabled)) && (LevelSerializer.IsDeserializing || extendedCoRoutine.MoveNext()))
			{
				object v = extendedCoRoutine.Current;
				CoroutineReturn cr = v as CoroutineReturn;
				if (cr != null)
				{
					if (cr.cancel)
					{
						this.cancel = true;
						break;
					}
					while (!cr.finished)
					{
						if (cr.cancel)
						{
							this.cancel = true;
							break;
						}
						yield return null;
					}
					if (this.cancel)
					{
						break;
					}
				}
				else if (v is IYieldInstruction)
				{
					yield return (v as IYieldInstruction).Instruction;
				}
				if (v is IEnumerator)
				{
					stack.Push(extendedCoRoutine);
					extendedCoRoutine = (v as IEnumerator);
				}
				else if (v is RadicalRoutine)
				{
					RadicalRoutine rr = v as RadicalRoutine;
					while (!rr.finished)
					{
						yield return null;
					}
				}
				else
				{
					yield return v;
				}
			}
		}
		this.finished = true;
		this.Cancel();
		if (this.cancel)
		{
			this.Cancelled();
		}
		this.Finished();
		if (complete != null)
		{
			complete();
		}
		yield break;
	}

	
	public void Deserialized()
	{
	}

	
	public bool cancel;

	
	private IEnumerator extended;

	
	public IEnumerator enumerator;

	
	public object Notify;

	
	public string Method;

	
	public bool finished;

	
	public object Target;

	
	private bool isTracking;

	
	private MonoBehaviour _trackedObject;
}
