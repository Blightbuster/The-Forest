using System;
using Bolt;


public class ElevatorGlobalState : EntityBehaviour<IElevator>
{
	
	public string SpawnId;

	
	public bool _locked;

	
	public int _locationId;

	
	public int _doorState;

	
	public int _playerCount;
}
