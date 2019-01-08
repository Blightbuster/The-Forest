using System;
using UnityEngine;
using UnityEngine.UI;

public class ParticleMenu : MonoBehaviour
{
	private void Start()
	{
		this.Navigate(0);
		this.currentIndex = 0;
	}

	public void Navigate(int i)
	{
		this.currentIndex = (this.particleSystems.Length + this.currentIndex + i) % this.particleSystems.Length;
		if (this.currentGO != null)
		{
			UnityEngine.Object.Destroy(this.currentGO);
		}
		this.currentGO = UnityEngine.Object.Instantiate<GameObject>(this.particleSystems[this.currentIndex].particleSystemGO, this.spawnLocation.position + this.particleSystems[this.currentIndex].particlePosition, Quaternion.Euler(this.particleSystems[this.currentIndex].particleRotation));
		this.gunGameObject.SetActive(this.particleSystems[this.currentIndex].isWeaponEffect);
		this.title.text = this.particleSystems[this.currentIndex].title;
		this.description.text = this.particleSystems[this.currentIndex].description;
		this.navigationDetails.text = string.Concat(new object[]
		{
			string.Empty,
			this.currentIndex + 1,
			" out of ",
			this.particleSystems.Length.ToString()
		});
	}

	public ParticleExamples[] particleSystems;

	public GameObject gunGameObject;

	private int currentIndex;

	private GameObject currentGO;

	public Transform spawnLocation;

	public Text title;

	public Text description;

	public Text navigationDetails;
}
