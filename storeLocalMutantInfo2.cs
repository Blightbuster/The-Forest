using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;


public class storeLocalMutantInfo2 : MonoBehaviour
{
	
	private void OnEnable()
	{
		this.jointAngles.Clear();
		this.stuckArrowsIndex.Clear();
		this.stuckArrowPos.Clear();
		this.stuckArrowRot.Clear();
		this.matColor = Color.white;
		base.Invoke("cleanUp", 10f);
	}

	
	private void OnDestroy()
	{
		this.jointAngles.Clear();
		this.stuckArrowsIndex.Clear();
		this.stuckArrowPos.Clear();
		this.stuckArrowRot.Clear();
		this.matColor = Color.white;
	}

	
	private void cleanUp()
	{
		if (base.gameObject)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (Scene.SceneTracker)
		{
			Scene.SceneTracker.storedRagDollPrefabs.RemoveAll((GameObject o) => o == null);
		}
	}

	
	public int identifier;

	
	public List<Quaternion> jointAngles = new List<Quaternion>();

	
	public Quaternion rootRotation;

	
	public Vector3 rootPosition;

	
	public Color matColor;

	
	public Material mat;

	
	public bool showHair;

	
	public bool hideExtraParts;

	
	public Vector3 scale;

	
	public MaterialPropertyBlock bloodPropertyBlock;

	
	public bool showMask;

	
	public bool isSnow;

	
	public bool onFire;

	
	public Dictionary<Transform, int> stuckArrowsIndex = new Dictionary<Transform, int>();

	
	public List<Vector3> stuckArrowPos = new List<Vector3>();

	
	public List<Quaternion> stuckArrowRot = new List<Quaternion>();

	
	public List<Transform> fire = new List<Transform>();

	
	public Dictionary<Transform, int> fireIndex = new Dictionary<Transform, int>();

	
	public List<Vector3> firePos = new List<Vector3>();

	
	public List<Quaternion> fireRot = new List<Quaternion>();
}
