using System;
using TheForest.Tools;
using UnityEngine;


public class setupFishRagdoll : MonoBehaviour
{
	
	private void Start()
	{
		if (this.testFishInt > 0)
		{
			this.doSkinSetup(this.testFishInt);
		}
	}

	
	private void doSkinSetup(int n)
	{
		if (this.skin)
		{
			this.skin.sharedMaterial = this.mat[n];
			this.skin.sharedMesh = this.mesh[n];
		}
		if (n == 3)
		{
			AnimalType animalType = AnimalType.ButterFlyFish;
			EventRegistry.Player.Publish(TfEvent.InspectedAnimal, animalType);
			Debug.Log("publishing yellow tail fish");
		}
	}

	
	public int testFishInt;

	
	public Mesh[] mesh;

	
	public Material[] mat;

	
	public SkinnedMeshRenderer skin;
}
