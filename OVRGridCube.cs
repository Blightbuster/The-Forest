using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;


public class OVRGridCube : MonoBehaviour
{
	
	private void Update()
	{
		this.UpdateCubeGrid();
	}

	
	public void SetOVRCameraController(ref OVRCameraRig cameraController)
	{
		this.CameraController = cameraController;
	}

	
	private void UpdateCubeGrid()
	{
		if (Input.GetKeyDown(this.GridKey))
		{
			if (!this.CubeGridOn)
			{
				this.CubeGridOn = true;
				Debug.LogWarning("CubeGrid ON");
				if (this.CubeGrid != null)
				{
					this.CubeGrid.SetActive(true);
				}
				else
				{
					this.CreateCubeGrid();
				}
			}
			else
			{
				this.CubeGridOn = false;
				Debug.LogWarning("CubeGrid OFF");
				if (this.CubeGrid != null)
				{
					this.CubeGrid.SetActive(false);
				}
			}
		}
		if (this.CubeGrid != null)
		{
			this.CubeSwitchColor = !OVRManager.tracker.isPositionTracked;
			if (this.CubeSwitchColor != this.CubeSwitchColorOld)
			{
				this.CubeGridSwitchColor(this.CubeSwitchColor);
			}
			this.CubeSwitchColorOld = this.CubeSwitchColor;
		}
	}

	
	private void CreateCubeGrid()
	{
		Debug.LogWarning("Create CubeGrid");
		this.CubeGrid = new GameObject("CubeGrid");
		this.CubeGrid.layer = this.CameraController.gameObject.layer;
		for (int i = -this.gridSizeX; i <= this.gridSizeX; i++)
		{
			for (int j = -this.gridSizeY; j <= this.gridSizeY; j++)
			{
				for (int k = -this.gridSizeZ; k <= this.gridSizeZ; k++)
				{
					int num = 0;
					if ((i == 0 && j == 0) || (i == 0 && k == 0) || (j == 0 && k == 0))
					{
						if (i == 0 && j == 0 && k == 0)
						{
							num = 2;
						}
						else
						{
							num = 1;
						}
					}
					GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
					BoxCollider component = gameObject.GetComponent<BoxCollider>();
					component.enabled = false;
					gameObject.layer = this.CameraController.gameObject.layer;
					Renderer component2 = gameObject.GetComponent<Renderer>();
					component2.shadowCastingMode = ShadowCastingMode.Off;
					component2.receiveShadows = false;
					if (num == 0)
					{
						component2.material.color = Color.red;
					}
					else if (num == 1)
					{
						component2.material.color = Color.white;
					}
					else
					{
						component2.material.color = Color.yellow;
					}
					gameObject.transform.position = new Vector3((float)i * this.gridScale, (float)j * this.gridScale, (float)k * this.gridScale);
					float num2 = 0.7f;
					if (num == 1)
					{
						num2 = 1f;
					}
					if (num == 2)
					{
						num2 = 2f;
					}
					gameObject.transform.localScale = new Vector3(this.cubeScale * num2, this.cubeScale * num2, this.cubeScale * num2);
					gameObject.transform.parent = this.CubeGrid.transform;
				}
			}
		}
	}

	
	private void CubeGridSwitchColor(bool CubeSwitchColor)
	{
		Color color = Color.red;
		if (CubeSwitchColor)
		{
			color = Color.blue;
		}
		IEnumerator enumerator = this.CubeGrid.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				Material material = transform.GetComponent<Renderer>().material;
				if (material.color == Color.red || material.color == Color.blue)
				{
					material.color = color;
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
	}

	
	public KeyCode GridKey = KeyCode.G;

	
	private GameObject CubeGrid;

	
	private bool CubeGridOn;

	
	private bool CubeSwitchColorOld;

	
	private bool CubeSwitchColor;

	
	private int gridSizeX = 6;

	
	private int gridSizeY = 4;

	
	private int gridSizeZ = 6;

	
	private float gridScale = 0.3f;

	
	private float cubeScale = 0.03f;

	
	private OVRCameraRig CameraController;
}
