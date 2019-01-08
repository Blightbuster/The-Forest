using System;
using UnityEngine;

[Serializable]
public class AnimalSpawnConfig
{
	public float WeightRunningTotal { get; set; }

	public float Weight;

	public GameObject Prefab;

	public BundleKey bundleKey;

	public bool largeAnimalType;

	public bool nocturnal;
}
