using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using UnityEngine;


public class inventoryItemSpreadSetup : MonoBehaviour
{
	
	private void Start()
	{
		if (base.transform.GetComponentInParent<CraftingCog>())
		{
			this.onCraftingMat = true;
			UnityEngine.Object.Destroy(this);
			return;
		}
		this.setupTargetTriggers();
	}

	
	private void OnDisable()
	{
		if (!this.onCraftingMat)
		{
			this.resetItemPositions();
		}
	}

	
	private void setupTargetTriggers()
	{
		this.startPositions.Clear();
		this.startRotations.Clear();
		if (this.sourceObjects.Count > 0)
		{
			for (int i = 0; i < this.sourceObjects.Count; i++)
			{
				if (!this.savingCompatibilityMode)
				{
					inventoryItemSpreadTrigger inventoryItemSpreadTrigger = this.sourceObjects[i].gameObject.GetComponent<inventoryItemSpreadTrigger>();
					if (!inventoryItemSpreadTrigger)
					{
						inventoryItemSpreadTrigger = this.sourceObjects[i].gameObject.AddComponent<inventoryItemSpreadTrigger>();
					}
					int num = i - 1;
					if (num > -1)
					{
						inventoryItemSpreadTrigger.neighbour1 = this.sourceObjects[num];
					}
					int num2 = i + 1;
					if (num2 < this.sourceObjects.Count)
					{
						inventoryItemSpreadTrigger.neighbour2 = this.sourceObjects[num2];
					}
					inventoryItemSpreadTrigger.index = i;
				}
				if (this.savingCompatibilityMode)
				{
					InventoryItemView component = this.sourceObjects[i].gameObject.GetComponent<InventoryItemView>();
					if (component)
					{
						component._itemSpreadIndex = i;
					}
				}
				if (this.offsetRenderersMode)
				{
					this.startPositions.Add(this.offsetSourceObjects[i].transform.localPosition);
					this.startRotations.Add(this.offsetSourceObjects[i].transform.localRotation);
				}
				else
				{
					this.startPositions.Add(this.sourceObjects[i].transform.localPosition);
					this.startRotations.Add(this.sourceObjects[i].transform.localRotation);
				}
			}
		}
	}

	
	private void removeTargetTriggers()
	{
		if (this.sourceObjects.Count > 0)
		{
			for (int i = 0; i < this.sourceObjects.Count; i++)
			{
				inventoryItemSpreadTrigger component = this.sourceObjects[i].gameObject.GetComponent<inventoryItemSpreadTrigger>();
				if (component)
				{
					UnityEngine.Object.Destroy(component);
				}
			}
		}
	}

	
	public void gatherTargetObjects()
	{
		if (this.offsetRenderersMode)
		{
			Debug.Log("can't gather in offset renderer mode");
			return;
		}
		this.sourceObjects.Clear();
		this.startPositions.Clear();
		this.startRotations.Clear();
		if (this.singleItemMode)
		{
			this.sourceObjects.Add(base.gameObject);
			this.startPositions.Add(base.transform.localPosition);
			this.startRotations.Add(base.transform.localRotation);
			return;
		}
		Collider[] componentsInChildren = base.transform.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			this.sourceObjects.Add(componentsInChildren[i].gameObject);
			this.startPositions.Add(componentsInChildren[i].transform.localPosition);
			this.startRotations.Add(componentsInChildren[i].transform.localRotation);
		}
	}

	
	public void recordTargetPositions()
	{
		if (this.sourceObjects.Count == 0)
		{
			Debug.Log("please populate the source objects list!");
			return;
		}
		this.targetPositions.Clear();
		this.targetRotations.Clear();
		if (this.offsetRenderersMode)
		{
			foreach (GameObject gameObject in this.offsetSourceObjects)
			{
				this.targetPositions.Add(gameObject.transform.localPosition);
				this.targetRotations.Add(gameObject.transform.localRotation);
			}
		}
		else
		{
			foreach (GameObject gameObject2 in this.sourceObjects)
			{
				this.targetPositions.Add(gameObject2.transform.localPosition);
				this.targetRotations.Add(gameObject2.transform.localRotation);
			}
		}
		Debug.Log("finished recording target positions!");
	}

	
	public void recordStartPositions()
	{
		if (this.sourceObjects.Count == 0)
		{
			Debug.Log("please populate the source objects list!");
			return;
		}
		this.startPositions.Clear();
		this.startRotations.Clear();
		if (this.offsetRenderersMode)
		{
			foreach (GameObject gameObject in this.offsetSourceObjects)
			{
				this.startPositions.Add(gameObject.transform.localPosition);
				this.startRotations.Add(gameObject.transform.localRotation);
			}
		}
		else
		{
			foreach (GameObject gameObject2 in this.sourceObjects)
			{
				this.startPositions.Add(gameObject2.transform.localPosition);
				this.startRotations.Add(gameObject2.transform.localRotation);
			}
		}
		Debug.Log("finished recording START positions!");
	}

	
	public void resetTargetPositions()
	{
		if (this.sourceObjects.Count == 0)
		{
			Debug.Log("no source objects to reset");
			return;
		}
		if (this.offsetRenderersMode)
		{
			for (int i = 0; i < this.offsetSourceObjects.Count; i++)
			{
				this.offsetSourceObjects[i].transform.localPosition = this.startPositions[i];
				this.offsetSourceObjects[i].transform.localRotation = this.startRotations[i];
			}
		}
		else
		{
			for (int j = 0; j < this.sourceObjects.Count; j++)
			{
				this.sourceObjects[j].transform.localPosition = this.startPositions[j];
				this.sourceObjects[j].transform.localRotation = this.startRotations[j];
			}
		}
	}

	
	public void editTargetPositions()
	{
		if (this.sourceObjects.Count == 0)
		{
			Debug.Log("no source objects to edit");
			return;
		}
		if (this.offsetRenderersMode)
		{
			for (int i = 0; i < this.offsetSourceObjects.Count; i++)
			{
				this.offsetSourceObjects[i].transform.localPosition = this.targetPositions[i];
				this.offsetSourceObjects[i].transform.localRotation = this.targetRotations[i];
			}
		}
		else
		{
			for (int j = 0; j < this.sourceObjects.Count; j++)
			{
				this.sourceObjects[j].transform.localPosition = this.targetPositions[j];
				this.sourceObjects[j].transform.localRotation = this.targetRotations[j];
			}
		}
	}

	
	private IEnumerator enableItemSpreadRoutine()
	{
		base.StopCoroutine("disableItemSpreadRoutine");
		this.doingItemSpread = true;
		float t = 0f;
		while (t < 1f)
		{
			for (int i = 0; i < this.sourceObjects.Count; i++)
			{
				if (this.offsetRenderersMode)
				{
					this.offsetSourceObjects[i].transform.localPosition = Vector3.Slerp(this.offsetSourceObjects[i].transform.localPosition, this.targetPositions[i], t);
					this.offsetSourceObjects[i].transform.localRotation = Quaternion.Slerp(this.offsetSourceObjects[i].transform.localRotation, this.targetRotations[i], t);
				}
				else
				{
					this.sourceObjects[i].transform.localPosition = Vector3.Slerp(this.sourceObjects[i].transform.localPosition, this.targetPositions[i], t);
					this.sourceObjects[i].transform.localRotation = Quaternion.Slerp(this.sourceObjects[i].transform.localRotation, this.targetRotations[i], t);
				}
			}
			t += Time.unscaledDeltaTime * 2f;
			yield return null;
		}
		yield break;
	}

	
	private IEnumerator disableItemSpreadRoutine()
	{
		base.StopCoroutine("enableItemSpreadRoutine");
		this.doingItemSpread = false;
		float t = 0f;
		while (t < 1f)
		{
			for (int i = 0; i < this.sourceObjects.Count; i++)
			{
				if (this.offsetRenderersMode)
				{
					this.offsetSourceObjects[i].transform.localPosition = Vector3.Slerp(this.offsetSourceObjects[i].transform.localPosition, this.startPositions[i], t);
					this.offsetSourceObjects[i].transform.localRotation = Quaternion.Slerp(this.offsetSourceObjects[i].transform.localRotation, this.startRotations[i], t);
				}
				else
				{
					this.sourceObjects[i].transform.localPosition = Vector3.Slerp(this.sourceObjects[i].transform.localPosition, this.startPositions[i], t);
					this.sourceObjects[i].transform.localRotation = Quaternion.Slerp(this.sourceObjects[i].transform.localRotation, this.startRotations[i], t);
				}
			}
			t += Time.unscaledDeltaTime * 2f;
			yield return null;
		}
		yield break;
	}

	
	private void resetItemPositions()
	{
		if (this.sourceObjects.Count == 0)
		{
			return;
		}
		for (int i = 0; i < this.sourceObjects.Count; i++)
		{
			if (this.offsetRenderersMode)
			{
				this.offsetSourceObjects[i].transform.localPosition = this.startPositions[i];
				this.offsetSourceObjects[i].transform.localRotation = this.startRotations[i];
			}
			else
			{
				this.sourceObjects[i].transform.localPosition = this.startPositions[i];
				this.sourceObjects[i].transform.localRotation = this.startRotations[i];
			}
		}
		this.doingItemSpread = false;
		this.spreadActive = false;
	}

	
	public void offsetTargetPositions()
	{
		if (this.sourceObjects.Count == 0)
		{
			return;
		}
		for (int i = 0; i < this.sourceObjects.Count; i++)
		{
			Vector3 localPosition = this.sourceObjects[i].transform.localPosition;
			localPosition.y += 0.02f;
			this.sourceObjects[i].transform.localPosition = localPosition;
		}
	}

	
	public void offsetMeshComponent()
	{
		if (this.offsetRenderersMode)
		{
			return;
		}
		for (int i = 0; i < this.sourceObjects.Count; i++)
		{
			List<Transform> list = new List<Transform>();
			IEnumerator enumerator = this.sourceObjects[i].transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					if (!transform.name.Contains("ModelOffset"))
					{
						list.Add(transform);
						Debug.Log("found child = " + transform.name);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			GameObject gameObject = new GameObject();
			gameObject.name = this.sourceObjects[i].name + "_OFFSET";
			gameObject.transform.parent = this.sourceObjects[i].transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshFilter.sharedMesh = this.sourceObjects[i].GetComponent<MeshFilter>().sharedMesh;
			meshRenderer.sharedMaterials = this.sourceObjects[i].GetComponent<MeshRenderer>().sharedMaterials;
			gameObject.layer = this.sourceObjects[i].gameObject.layer;
			foreach (Transform transform2 in list)
			{
				transform2.transform.parent = gameObject.transform;
			}
			this.sourceObjects[i].GetComponent<MeshRenderer>().enabled = false;
			this.sourceObjects[i].transform.localPosition = this.targetPositions[i];
			this.sourceObjects[i].transform.localRotation = this.targetRotations[i];
			gameObject.transform.parent = null;
			this.sourceObjects[i].transform.localPosition = this.startPositions[i];
			this.sourceObjects[i].transform.localRotation = this.startRotations[i];
			gameObject.transform.parent = this.sourceObjects[i].transform;
			this.targetPositions[i] = gameObject.transform.localPosition;
			this.targetRotations[i] = gameObject.transform.localRotation;
			this.startPositions[i] = Vector3.zero;
			this.startRotations[i] = Quaternion.identity;
			this.offsetSourceObjects.Add(gameObject);
		}
		this.offsetRenderersMode = true;
		this.resetTargetPositions();
		this.fixIiv();
	}

	
	public void fixIiv()
	{
		if (this.offsetSourceObjects.Count == 0)
		{
			Debug.Log("only needs to run when using offset renderer mode!");
			return;
		}
		for (int i = 0; i < this.sourceObjects.Count; i++)
		{
			InventoryItemView component = this.sourceObjects[i].GetComponent<InventoryItemView>();
			if (component && component._renderers.Length > 0)
			{
				for (int j = 0; j < component._renderers.Length; j++)
				{
					MeshRenderer component2 = this.offsetSourceObjects[i].GetComponent<MeshRenderer>();
					if (component2)
					{
						component._renderers[j]._renderer = component2;
					}
				}
			}
		}
	}

	
	public List<GameObject> sourceObjects = new List<GameObject>();

	
	public List<GameObject> offsetSourceObjects = new List<GameObject>();

	
	public List<Vector3> targetPositions = new List<Vector3>();

	
	public List<Quaternion> targetRotations = new List<Quaternion>();

	
	public List<Vector3> startPositions = new List<Vector3>();

	
	public List<Quaternion> startRotations = new List<Quaternion>();

	
	private bool onCraftingMat;

	
	[HideInInspector]
	public bool doingItemSpread;

	
	[HideInInspector]
	public bool spreadActive;

	
	public bool spreadHoveredItemOnlyMode;

	
	public bool singleItemMode;

	
	public bool offsetRenderersMode;

	
	public bool savingCompatibilityMode;

	
	public int minSpreadTargetAmount;
}
