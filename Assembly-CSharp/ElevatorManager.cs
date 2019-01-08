using System;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorManager : MonoBehaviour
{
	public List<string> _spawnIds;

	public ElevatorGlobalState _sourcePrefab;

	private Dictionary<string, ElevatorGlobalState> _stateInstances;

	private static ElevatorManager _managerInstance;
}
