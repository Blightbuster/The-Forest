using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	[DoNotSerializePublic]
	[RequireComponent(typeof(MeshFilter))]
	public class CaveMapDrawer : MonoBehaviour
	{
		private void Awake()
		{
			this._renderer = this._targetMeshFilter.GetComponent<Renderer>();
			this.Clear();
			base.enabled = false;
			if (!LevelSerializer.IsDeserializing)
			{
				base.InvokeRepeating("CheckMapOwnerchip", 1f, 1f);
				base.InvokeRepeating("CheckMaterialToggle", 1f, 2f);
			}
		}

		private void Update()
		{
			if (LocalPlayer.IsInCaves)
			{
				this._mapZmin = -1178.557f;
			}
			else
			{
				this._mapZmin = -1708f;
			}
			float num = Mathf.Abs(this._mapXmin - this._mapXmax);
			float num2 = Mathf.Abs(this._mapZmin - this._mapZmax);
			float t = Mathf.Abs(LocalPlayer.Transform.position.x - this._mapXmin) / num;
			float t2 = Mathf.Abs(LocalPlayer.Transform.position.z - this._mapZmin) / num2;
			float num3 = Mathf.Lerp(0f, (float)this._gridSize, t);
			float num4 = Mathf.Lerp(0f, (float)this._gridSize, t2);
			int num5 = Mathf.RoundToInt(num3);
			int num6 = Mathf.RoundToInt(num4);
			int num7 = (LocalPlayer.IsInCaves || num3 - (float)((int)num3) >= 0.25f || num5 <= 1) ? 0 : -1;
			int num8 = (LocalPlayer.IsInCaves || num4 - (float)((int)num4) >= 0.25f || num6 <= 1) ? 0 : -1;
			int num9 = (LocalPlayer.IsInCaves || num3 - (float)((int)num3) <= 0.75f) ? 0 : ((int)Mathf.Clamp01((float)(this._gridSize - num5 - 1)));
			int num10 = (LocalPlayer.IsInCaves || num4 - (float)((int)num4) <= 0.75f) ? 0 : ((int)Mathf.Clamp01((float)(this._gridSize - num6 - 1)));
			for (int i = (num5 <= 0) ? 0 : num7; i <= num9; i++)
			{
				for (int j = (num6 <= 0) ? 0 : num8; j <= num10; j++)
				{
					if (LocalPlayer.IsInCaves)
					{
						if (!this._visitedAreas[num6 + j, num5 + i])
						{
							this._visitedAreas[num6 + j, num5 + i] = true;
							this.CheckCaveAreas(true);
							this.ToggleMeshAt(this._gridSize - num5 + i, num6 + j);
						}
					}
					else if (!this._worldVisitedAreas[num6 + j, num5 + i])
					{
						this._worldVisitedAreas[num6 + j, num5 + i] = true;
						this.ToggleMeshAt(this._gridSize - num5 + i, num6 + j);
					}
				}
			}
			Vector3 localPos = new Vector3(Mathf.Lerp(0.5f, -0.5f, t), Mathf.Lerp(-0.5f, 0.5f, t2), 0f);
			this.PlantPlayerPositionPin(localPos);
		}

		private void CheckCaveAreas(bool sendExploredCaveEvent)
		{
			for (int i = 0; i < this._caveAreas.Count; i++)
			{
				if (!this._caveAreas[i]._completed)
				{
					int num = Mathf.CeilToInt((float)this._caveAreas[i]._coords.Count / 10f) + 1;
					this._caveAreas[i]._completed = true;
					for (int j = 0; j < this._caveAreas[i]._coords.Count; j++)
					{
						CaveMapDrawer.Coords coords = this._caveAreas[i]._coords[j];
						if (!this._visitedAreas[coords.y, 20 - coords.x] && num-- == 0)
						{
							this._caveAreas[i]._completed = false;
							break;
						}
					}
					if (sendExploredCaveEvent && this._caveAreas[i]._completed)
					{
						EventRegistry.Player.Publish(TfEvent.ExploredCaveArea, i);
					}
				}
			}
		}

		private IEnumerator OnDeserialized()
		{
			if (this._visitedAreas == null || this._visitedAreas.GetLength(0) != this._gridSize + 1)
			{
				bool[,] array = new bool[this._gridSize + 1, this._gridSize + 1];
				if (this._visitedAreas != null)
				{
					try
					{
						int num = Mathf.Min(this._visitedAreas.GetLength(0), this._gridSize + 1);
						int num2 = Mathf.Min(this._visitedAreas.GetLength(1), this._gridSize + 1);
						for (int i = 0; i < num; i++)
						{
							for (int j = 0; j < num2; j++)
							{
								array[i, j] = this._visitedAreas[i, j];
							}
						}
					}
					catch (Exception arg)
					{
						Debug.LogError("Error copying cave map data: " + arg);
					}
				}
				this._visitedAreas = array;
				array = new bool[this._gridSize + 1, this._gridSize + 1];
				if (this._worldVisitedAreas != null)
				{
					try
					{
						int num3 = Mathf.Min(this._worldVisitedAreas.GetLength(0), this._gridSize + 1);
						int num4 = Mathf.Min(this._worldVisitedAreas.GetLength(1), this._gridSize + 1);
						for (int k = 0; k < num3; k++)
						{
							for (int l = 0; l < num4; l++)
							{
								array[k, l] = this._worldVisitedAreas[k, l];
							}
						}
					}
					catch (Exception arg2)
					{
						Debug.LogError("Error copying cave map data: " + arg2);
					}
				}
				this._worldVisitedAreas = array;
			}
			yield return null;
			this._renderer.sharedMaterial = (LocalPlayer.IsInCaves ? this._worldMap : this._caveMap);
			base.InvokeRepeating("CheckMapOwnerchip", 1f, 1f);
			base.InvokeRepeating("CheckMaterialToggle", 1f, 2f);
			yield break;
		}

		private void Reload()
		{
			if (this._visitedAreas == null)
			{
				return;
			}
			for (int i = 0; i < this._gridSize; i++)
			{
				for (int j = 0; j < this._gridSize; j++)
				{
					if ((!LocalPlayer.IsInCaves) ? this._worldVisitedAreas[j, i] : this._visitedAreas[j, i])
					{
						this.ToggleMeshAt(this._gridSize - i, j);
					}
				}
			}
		}

		private void CheckMapOwnerchip()
		{
			bool flag = LocalPlayer.Inventory != null && LocalPlayer.Inventory.Owns(this._caveMapItemId, true);
			if (base.enabled || !flag)
			{
				return;
			}
			base.enabled = true;
			if (this._visitedAreas == null)
			{
				this.InitializeVisitedAreas();
			}
			base.CancelInvoke("CheckMapOwnerchip");
		}

		private void InitializeVisitedAreas()
		{
			this._visitedAreas = new bool[this._gridSize + 1, this._gridSize + 1];
			this._worldVisitedAreas = new bool[this._gridSize + 1, this._gridSize + 1];
			float num = Mathf.Abs(this._mapXmin - this._mapXmax);
			float num2 = Mathf.Abs(this._mapZmin - this._mapZmax);
			float t = Mathf.Abs(LocalPlayer.Transform.position.x - this._mapXmin) / num;
			float t2 = Mathf.Abs(LocalPlayer.Transform.position.z - this._mapZmin) / num2;
			float num3 = Mathf.Lerp(0f, (float)this._gridSize, t);
			float num4 = Mathf.Lerp(0f, (float)this._gridSize, t2);
			int num5 = Mathf.RoundToInt(num3);
			int num6 = Mathf.RoundToInt(num4);
			int num7 = (num3 - (float)((int)num3) >= 0.5f) ? 0 : ((int)Mathf.Clamp01((float)(num5 - 1)));
			int num8 = (num4 - (float)((int)num4) >= 0.5f) ? 0 : ((int)Mathf.Clamp01((float)(num6 - 1)));
			int num9 = (num3 - (float)((int)num3) <= 0.5f) ? 0 : ((int)Mathf.Clamp01((float)(this._gridSize - num5 - 1)));
			int num10 = (num4 - (float)((int)num4) <= 0.5f) ? 0 : ((int)Mathf.Clamp01((float)(this._gridSize - num6 - 1)));
			for (int i = (num5 <= 0) ? 0 : num7; i <= num9; i++)
			{
				for (int j = (num6 <= 0) ? 0 : num8; j <= num10; j++)
				{
					if (!this._visitedAreas[num6 + j, num5 + i])
					{
						this._visitedAreas[num6 + j, num5 + i] = true;
						this.ToggleMeshAt(this._gridSize - num5 + i, num6 + j);
					}
				}
			}
		}

		private void CheckCompassOwnerchip()
		{
			if (LocalPlayer.Inventory == null || this._compass == null)
			{
				return;
			}
			bool flag = !LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.LeftHand, this._compassItemId) && LocalPlayer.Inventory.Owns(this._compassItemId, true);
			if (flag != this._compass.activeSelf)
			{
				this._compass.SetActive(flag);
			}
		}

		private void CheckMaterialToggle()
		{
			if (this._togglingMaterial || LocalPlayer.Inventory == null || this._renderer == null)
			{
				return;
			}
			bool flag = this._renderer.sharedMaterial.Equals(this._caveMap);
			if (flag == LocalPlayer.IsInCaves)
			{
				return;
			}
			if (this._targetMeshFilter.gameObject.activeSelf)
			{
				LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
				LocalPlayer.Inventory.StashEquipedWeapon(true);
				this._togglingMaterial = true;
				base.Invoke("MaterialToggle", 0.5f);
			}
			else
			{
				this.MaterialToggle();
			}
		}

		private void MaterialToggle()
		{
			if (this._renderer == null || this._rendererBG == null)
			{
				return;
			}
			this._togglingMaterial = false;
			this._renderer.sharedMaterial = ((!LocalPlayer.IsInCaves) ? this._worldMap : this._caveMap);
			this._rendererBG.sharedMaterial = ((!LocalPlayer.IsInCaves) ? this._worldMapBG : this._caveMapBG);
			this.Clear();
			this.Reload();
		}

		private void CheckMesh()
		{
			if (this._targetMeshFilter == null)
			{
				return;
			}
			if (this._mesh != null && this._mesh == this._targetMeshFilter.mesh)
			{
				return;
			}
			this._targetMeshFilter.mesh = base.GetComponent<MeshFilter>().sharedMesh;
			Mesh mesh = this._targetMeshFilter.mesh;
			mesh.name += UnityEngine.Random.Range(0, 100000).ToString();
			this._mesh = this._targetMeshFilter.mesh;
		}

		private int WorldToGridRounded(float worldPosition)
		{
			return Mathf.FloorToInt((worldPosition - this._gridCellOffset) / this._gridCellWorldSize);
		}

		private void ToggleMeshAt(int x, int y)
		{
			this.CheckMesh();
			base.StartCoroutine(this.RevealMapRoutine(x * (this._gridSize + 1) + y));
		}

		public IEnumerator RevealMapRoutine(int index)
		{
			float time = Time.deltaTime;
			Color defaultColor = this._fromColor;
			while (time < 1f)
			{
				if (this._mesh.colors.Length != this._mesh.vertices.Length)
				{
					this._colors = new Color[this._mesh.vertices.Length];
				}
				float a = Mathf.Lerp(defaultColor.a, this._toColor.a, time);
				if (a <= this._colors[index].a)
				{
					yield break;
				}
				this._colors[index].a = a;
				this._mesh.colors = this._colors;
				time += Time.deltaTime;
				yield return null;
			}
			this._colors[index] = this._toColor;
			this._mesh.colors = this._colors;
			yield break;
		}

		private void Clear()
		{
			this.CheckMesh();
			if (this._mesh.colors.Length != this._mesh.vertices.Length)
			{
				this._colors = new Color[this._mesh.vertices.Length];
			}
			else
			{
				this._colors = this._mesh.colors;
			}
			for (int i = 0; i < this._colors.Length; i++)
			{
				this._colors[i] = new Color(0f, 0f, 0f, 0f);
			}
			this._mesh.colors = this._colors;
		}

		private void PlantPlayerPositionPin(Vector3 localPos)
		{
			if (this._targetMeshFilter.gameObject.activeSelf)
			{
				if (!this._playerPositionPin.gameObject.activeSelf)
				{
					this._playerPositionPin.gameObject.SetActive(true);
				}
			}
			else if (this._playerPositionPin.gameObject.activeSelf)
			{
				this._playerPositionPin.gameObject.SetActive(false);
			}
			if (this._playerPositionPin.gameObject.activeSelf)
			{
				this._playerPositionPin.localPosition = localPos;
			}
		}

		private IEnumerator PlayerPositionPinAnim(Vector3 targetPos)
		{
			this._pinAlpha = 0f;
			Vector3 velocity = Vector3.zero;
			Vector3 animVelocity = Vector3.zero;
			Transform pinAnim = this._playerPositionPin.GetChild(0);
			while (this._pinAlpha < 1f)
			{
				if (this._pinAlpha < 0.5f)
				{
					if (pinAnim.localPosition.y < 0.99f)
					{
						pinAnim.localPosition = Vector3.SmoothDamp(pinAnim.localPosition, Vector3.up, ref animVelocity, 0.35f);
					}
					else
					{
						animVelocity = Vector3.zero;
					}
				}
				else if (pinAnim.localPosition.y > 0f)
				{
					pinAnim.localPosition = Vector3.SmoothDamp(pinAnim.localPosition, Vector3.zero, ref animVelocity, 0.25f);
				}
				this._playerPositionPin.localPosition = Vector3.SmoothDamp(this._playerPositionPin.localPosition, targetPos, ref velocity, 0.49f);
				this._pinAlpha += Time.deltaTime * 2f;
				yield return null;
			}
			pinAnim.localPosition = Vector3.zero;
			this._playerPositionPin.localPosition = targetPos;
			yield break;
		}

		public Color _fromColor = new Color(1f, 0f, 0f, 0f);

		public Color _toColor = new Color(1f, 1f, 1f, 1f);

		public Material _caveMap;

		public Material _caveMapBG;

		public Material _worldMap;

		public Material _worldMapBG;

		public MeshFilter _targetMeshFilter;

		public Renderer _rendererBG;

		public Transform _playerPositionPin;

		public GameObject _compass;

		public int _gridSize = 10;

		public float _gridCellWorldSize = 350f;

		public float _gridCellOffset = -1750f;

		public float _mapXmin = 1443.252f;

		public float _mapXmax = -1305.252f;

		public float _mapZmin = -1178.557f;

		public float _mapZmax = 1470.557f;

		public Renderer _landmarksRoot;

		public List<CaveMapDrawer.Area> _caveAreas;

		[ItemIdPicker]
		public int _caveMapItemId;

		[ItemIdPicker]
		public int _compassItemId;

		[Header("Gizmos")]
		public bool _showVisitedAreaGrid;

		public int _textX;

		public int _textY;

		public bool _test;

		public bool _clear;

		public bool _showCaveAreasColors;

		[SerializeThis]
		public bool[,] _visitedAreas;

		[SerializeThis]
		public bool[,] _worldVisitedAreas;

		private Renderer _renderer;

		private Mesh _mesh;

		private float _pinAlpha;

		private bool _togglingMaterial;

		public Color[] _colors;

		[Serializable]
		public class Area
		{
			public string _name;

			public Color _color;

			public List<CaveMapDrawer.Coords> _coords;

			public bool _completed;

			public bool _showDebug = true;

			public float _mapXmin = 1443.252f;

			public float _mapXmax = -1305.252f;

			public float _mapZmin = -1178.557f;

			public float _mapZmax = 1470.557f;

			public Renderer _landmark;
		}

		[Serializable]
		public class Coords
		{
			public int x;

			public int y;
		}
	}
}
