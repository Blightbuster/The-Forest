using System;
using UnityEngine;

public class randomRitualArt : MonoBehaviour
{
	private void Awake()
	{
		int num = UnityEngine.Random.Range(0, this.go.Length);
		for (int i = 0; i < this.go.Length; i++)
		{
			bool flag = i == num;
			if (this.go[i].activeSelf != flag)
			{
				this.go[i].SetActive(flag);
			}
		}
	}

	public GameObject[] go;
}
