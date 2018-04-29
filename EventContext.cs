using System;


public class EventContext
{
	
	public void Reset()
	{
		this.controllerId = 0;
		this.layer = 0;
		this.stateHash = 0;
		this.tagHash = 0;
		this.current = null;
	}

	
	public int controllerId;

	
	public int layer;

	
	public int stateHash;

	
	public int tagHash;

	
	public MecanimEvent current;
}
