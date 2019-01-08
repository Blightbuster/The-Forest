using System;
using UnityEngine;

public class JunkCreator : MonoBehaviour
{
	public static JunkCreator Instance
	{
		get
		{
			if (JunkCreator.instance == null)
			{
				GameObject gameObject = new GameObject("JunkCreator");
				JunkCreator.instance = gameObject.AddComponent<JunkCreator>();
				JunkCreator.instance.Init();
			}
			return JunkCreator.instance;
		}
	}

	private void OnDestroy()
	{
		if (this == JunkCreator.instance)
		{
			JunkCreator.instance = null;
		}
	}

	public void Init()
	{
		this.junkArray = new GameObject[0];
	}

	public void AddJunk(int amount)
	{
		this.arraySize += amount;
		this.SetJunk(this.arraySize);
	}

	public void SetJunk(int amount)
	{
		GameObject[] array = new GameObject[amount];
		Array.Copy(this.junkArray, array, this.junkArray.Length);
		for (int i = this.junkArray.Length; i < array.Length; i++)
		{
			GameObject gameObject = new GameObject("Junk");
			gameObject.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.enabled = false;
			meshRenderer.material.mainTexture = new Texture2D(512, 512);
			array[i] = gameObject;
			gameObject.transform.parent = base.transform;
		}
		this.junkArray = array;
	}

	public void Clear()
	{
		this.arraySize = 0;
		for (int i = 0; i < this.junkArray.Length; i++)
		{
			UnityEngine.Object.Destroy(this.junkArray[i].GetComponent<MeshRenderer>().material.mainTexture);
			UnityEngine.Object.Destroy(this.junkArray[i]);
		}
		this.junkArray = new GameObject[0];
	}

	private static JunkCreator instance;

	private int arraySize;

	private GameObject[] junkArray;
}
