using System;
using UnityEngine;


public class VariablesToSave : MonoBehaviour
{
	
	static VariablesToSave()
	{
		DelegateSupport.RegisterFunctionType<VariablesToSave, string>();
		DelegateSupport.RegisterFunctionType<VariablesToSave, bool>();
		DelegateSupport.RegisterFunctionType<VariablesToSave, int>();
	}

	
	
	
	public static int RandomNumber
	{
		get
		{
			return VariablesToSave._randomNumber;
		}
		set
		{
			VariablesToSave._randomNumber = value;
		}
	}

	
	private void Awake()
	{
		if (!VariablesToSave.hasInitialized)
		{
			VariablesToSave.hasInitialized = true;
			this.useMe = true;
			this.myStruct.value = UnityEngine.Random.Range(0, 100000);
		}
	}

	
	private void Update()
	{
	}

	
	public const int SomeValue = 1;

	
	public VariablesToSave.SomeStruct myStruct;

	
	private static int _randomNumber = UnityEngine.Random.Range(10, 200);

	
	public string oneVariable;

	
	public int anotherVariable;

	
	public static bool hasInitialized;

	
	public bool useMe;

	
	public struct SomeStruct
	{
		
		public int value;
	}
}
