using System;
using System.Collections.Generic;
using UnityEngine;


public class playerNetworkProxySetup : MonoBehaviour
{
	
	private void Awake()
	{
	}

	
	public void doNetworkSetup()
	{
	}

	
	public GameObject[] deleteGoList;

	
	public GameObject[] disableGoList;

	
	private HashSet<Type> allowedTypes;
}
