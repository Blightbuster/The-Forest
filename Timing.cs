using System;


public class Timing : IDisposable
{
	
	public Timing(string caption)
	{
		this._caption = caption;
		this._start = DateTime.Now;
	}

	
	public void Dispose()
	{
		Radical.LogNow("{0} - {1:0.000}", new object[]
		{
			this._caption,
			(DateTime.Now - this._start).TotalSeconds
		});
	}

	
	private readonly string _caption;

	
	private readonly DateTime _start;
}
