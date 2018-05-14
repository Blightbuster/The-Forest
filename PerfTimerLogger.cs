using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


public class PerfTimerLogger : IDisposable
{
	
	public PerfTimerLogger(string message, PerfTimerLogger.LogResultType resultType = PerfTimerLogger.LogResultType.Milliseconds, Action<string> logAction = null)
	{
		this._message = message;
		this._logResultType = resultType;
		this._logAction = logAction;
		this._timer = new Stopwatch();
		this._timer.Start();
		if (PerfTimerLogger._instances == null)
		{
			PerfTimerLogger._instances = new Dictionary<string, PerfTimerLogger>();
		}
		PerfTimerLogger._instances.Add(message, this);
	}

	
	public static PerfTimerLogger Get(string message)
	{
		if (PerfTimerLogger._instances == null || !PerfTimerLogger._instances.ContainsKey(message))
		{
			return null;
		}
		return PerfTimerLogger._instances[message];
	}

	
	public void Pause()
	{
		this._timer.Stop();
	}

	
	public void Unpause()
	{
		this._timer.Start();
	}

	
	public void Stop()
	{
		this.Dispose();
	}

	
	public void Dispose()
	{
		this._timer.Stop();
		long elapsedMilliseconds = this._timer.ElapsedMilliseconds;
		long elapsedTicks = this._timer.ElapsedTicks;
		PerfTimerLogger.LogResultType logResultType = this._logResultType;
		string text;
		if (logResultType != PerfTimerLogger.LogResultType.Ticks)
		{
			text = string.Format("{0} - Elapsed Milliseconds: {1}", this._message, elapsedMilliseconds);
		}
		else
		{
			text = string.Format("{0} - Elapsed Ticks: {1}", this._message, elapsedTicks);
		}
		if (this._logAction != null)
		{
			this._logAction(text);
		}
		else
		{
			UnityEngine.Debug.Log(text);
		}
		if (PerfTimerLogger._instances != null && PerfTimerLogger._instances.ContainsKey(this._message))
		{
			PerfTimerLogger._instances.Remove(this._message);
		}
	}

	
	private static Dictionary<string, PerfTimerLogger> _instances;

	
	private string _message;

	
	private Stopwatch _timer;

	
	private PerfTimerLogger.LogResultType _logResultType;

	
	private Action<string> _logAction;

	
	public enum LogResultType
	{
		
		Milliseconds,
		
		Ticks
	}
}
