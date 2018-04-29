using System;
using System.Collections.Generic;
using FMOD.Studio;
using TheForest.Utils;
using UnityEngine;


public class WaterOnTerrainSFX : MonoBehaviour
{
	
	private float GradientForLocalPosition(WaterOnTerrainSFX.IntVector v)
	{
		return this.sampleGrid[v.x, v.y].gradient;
	}

	
	private Vector3 EventPositionForLocalPosition(WaterOnTerrainSFX.IntVector v)
	{
		Rect rect = this.GridCellRect(this.SampleGridToWorldGrid(v));
		Vector3 position = base.transform.position;
		position.x = Mathf.Clamp(position.x, rect.xMin, rect.xMax);
		position.z = Mathf.Clamp(position.z, rect.yMin, rect.yMax);
		return position;
	}

	
	private WaterOnTerrainSFX.IntVector SampleGridToWorldGrid(WaterOnTerrainSFX.IntVector v)
	{
		return v + this.gridPosition - WaterOnTerrainSFX.SAMPLE_GRID_CENTRE;
	}

	
	private Vector3 GridCellCentre(WaterOnTerrainSFX.IntVector v)
	{
		return new Vector3(((float)v.x + 0.5f) * this.gridCellSize, 0f, ((float)v.y + 0.5f) * this.gridCellSize);
	}

	
	private Rect GridCellRect(WaterOnTerrainSFX.IntVector v)
	{
		return new Rect((float)v.x * this.gridCellSize, (float)v.y * this.gridCellSize, this.gridCellSize, this.gridCellSize);
	}

	
	private void Start()
	{
		if (!SteamDSConfig.isDedicatedServer)
		{
			this.eventDescription = FMOD_StudioSystem.instance.GetEventDescription("event:/ambient/water/water_runoff", true);
			this.gradientParameterIndex = FMODCommon.FindParameterIndex(this.eventDescription, "gradient");
			this.wetnessParameterIndex = FMODCommon.FindParameterIndex(this.eventDescription, "wetness");
			UnityUtil.ERRCHECK(this.eventDescription.getMaximumDistance(out this.gridCellSize));
			this.gridCellSize /= 10f;
			int num = this.targetSourceCount - 1;
			this.sampleGrid = new WaterOnTerrainSFX.GridCell[21, 21];
			this.sources = new List<WaterOnTerrainSFX.Source>(num);
			this.sourcePool = new Stack<WaterOnTerrainSFX.Source>(num);
			this.centreSource = new WaterOnTerrainSFX.Source();
			for (int i = 0; i < num; i++)
			{
				this.sourcePool.Push(new WaterOnTerrainSFX.Source());
			}
			FMOD_Listener.DrawWaterOnTerrain = new Action(this.DrawDebug);
		}
		else
		{
			base.enabled = false;
		}
	}

	
	private void Update()
	{
		this.UpdateSampleGrid();
		if (WaterOnTerrainSFX.Wetness > 0f && !LocalPlayer.IsInEndgame)
		{
			this.UpdateSources();
		}
		else
		{
			this.StopAllSources();
		}
	}

	
	private void UpdateSources()
	{
		float num = this.maximumScore / 5f;
		int i = 0;
		while (i < this.sources.Count)
		{
			WaterOnTerrainSFX.Source source = this.sources[i];
			WaterOnTerrainSFX.IntVector localPosition = source.localPosition;
			float num2 = this.ScoreForSource(source.localPosition, source);
			for (int j = 0; j < WaterOnTerrainSFX.NEIGHBOURS.Length; j++)
			{
				WaterOnTerrainSFX.IntVector intVector = source.localPosition + WaterOnTerrainSFX.NEIGHBOURS[j];
				float num3 = this.ScoreForSource(intVector, source);
				if (num3 > num2)
				{
					num2 = num3;
					localPosition = intVector;
				}
			}
			if (num2 > num)
			{
				source.localPosition = localPosition;
				source.SetGradient(this.GradientForLocalPosition(source.localPosition));
				i++;
			}
			else
			{
				this.StopSource(i);
			}
		}
		if (!this.centreSource.IsPlaying())
		{
			this.centreSource.Start(this.eventDescription, this.gradientParameterIndex, this.wetnessParameterIndex, WaterOnTerrainSFX.SAMPLE_GRID_CENTRE, base.transform.position, this.GradientForLocalPosition(WaterOnTerrainSFX.SAMPLE_GRID_CENTRE));
		}
		while (this.sourcePool.Count > 0)
		{
			WaterOnTerrainSFX.IntVector intVector2 = new WaterOnTerrainSFX.IntVector(0, 0);
			float num4 = num;
			WaterOnTerrainSFX.IntVector intVector3;
			intVector3.x = 0;
			while (intVector3.x < 21)
			{
				intVector3.y = 0;
				while (intVector3.y < 21)
				{
					float num5 = this.ScoreForSource(intVector3, null);
					if (num5 > num4)
					{
						num4 = num5;
						intVector2 = intVector3;
					}
					intVector3.y++;
				}
				intVector3.x++;
			}
			if (num4 <= num)
			{
				break;
			}
			WaterOnTerrainSFX.Source source2 = this.sourcePool.Pop();
			source2.Start(this.eventDescription, this.gradientParameterIndex, this.wetnessParameterIndex, intVector2, this.EventPositionForLocalPosition(intVector2), this.GradientForLocalPosition(intVector2));
			this.sources.Add(source2);
		}
		float distance = Time.deltaTime * this.sourceSpeed;
		this.centreSource.UpdateEvent(base.transform.position, float.MaxValue, WaterOnTerrainSFX.Wetness);
		for (int k = 0; k < this.sources.Count; k++)
		{
			WaterOnTerrainSFX.Source source3 = this.sources[k];
			source3.UpdateEvent(this.EventPositionForLocalPosition(source3.localPosition), distance, WaterOnTerrainSFX.Wetness);
		}
	}

	
	private void StopAllSources()
	{
		this.centreSource.Stop();
		for (int i = 0; i < this.sources.Count; i++)
		{
			this.sources[i].Stop();
			this.sourcePool.Push(this.sources[i]);
		}
		this.sources.Clear();
	}

	
	private void StopSource(int index)
	{
		if (index >= 0 && index < this.sources.Count)
		{
			WaterOnTerrainSFX.Source source = this.sources[index];
			this.sources.RemoveAt(index);
			source.Stop();
			this.sourcePool.Push(source);
		}
	}

	
	private static bool IsWithinSampleGrid(WaterOnTerrainSFX.IntVector v)
	{
		return v.x >= 0 && v.x < 21 && v.y >= 0 && v.y < 21;
	}

	
	private float ScoreForSource(WaterOnTerrainSFX.IntVector position, WaterOnTerrainSFX.Source source)
	{
		float num = 0f;
		if (WaterOnTerrainSFX.IsWithinSampleGrid(position))
		{
			num = this.sampleGrid[position.x, position.y].score;
			for (int i = 0; i < this.sources.Count; i++)
			{
				if (this.sources[i] != source)
				{
					float t = (float)((position - this.sources[i].localPosition).squareMagnitude / 5);
					num *= Mathf.Lerp(0.1f, 1f, t);
				}
			}
		}
		return num;
	}

	
	private void MoveSampleGridEntries(WaterOnTerrainSFX.IntVector offset)
	{
		WaterOnTerrainSFX.IntVector intVector;
		WaterOnTerrainSFX.IntVector intVector2;
		WaterOnTerrainSFX.IntVector intVector3;
		if (offset.x <= 0)
		{
			intVector.x = 0;
			intVector2.x = 21 + offset.x;
			intVector3.x = 1;
		}
		else
		{
			intVector.x = 20;
			intVector2.x = offset.x - 1;
			intVector3.x = -1;
		}
		if (offset.y <= 0)
		{
			intVector.y = 0;
			intVector2.y = 21 + offset.y;
			intVector3.y = 1;
		}
		else
		{
			intVector.y = 20;
			intVector2.y = offset.y - 1;
			intVector3.y = -1;
		}
		for (int num = intVector.x; num != intVector2.x; num += intVector3.x)
		{
			for (int num2 = intVector.y; num2 != intVector2.y; num2 += intVector3.y)
			{
				this.sampleGrid[num, num2] = this.sampleGrid[num - offset.x, num2 - offset.y];
			}
		}
	}

	
	private void CacheGradients(WaterOnTerrainSFX.IntVector basePosition, WaterOnTerrainSFX.IntVector start, WaterOnTerrainSFX.IntVector end)
	{
		for (int i = start.x; i < end.x; i++)
		{
			for (int j = start.y; j < end.y; j++)
			{
				WaterOnTerrainSFX.IntVector b = new WaterOnTerrainSFX.IntVector(i, j) - WaterOnTerrainSFX.SAMPLE_GRID_CENTRE;
				Vector3 position = this.GridCellCentre(basePosition + b);
				position = Terrain.activeTerrain.transform.InverseTransformPoint(position);
				TerrainData terrainData = Terrain.activeTerrain.terrainData;
				position.x /= terrainData.size.x;
				position.z /= terrainData.size.z;
				this.sampleGrid[i, j].gradient = terrainData.GetSteepness(position.x, position.z);
			}
		}
	}

	
	private void CalculateSampleGridScores()
	{
		this.maximumScore = 0f;
		for (int i = 0; i < 21; i++)
		{
			for (int j = 0; j < 21; j++)
			{
				int squareMagnitude = (new WaterOnTerrainSFX.IntVector(i, j) - WaterOnTerrainSFX.SAMPLE_GRID_CENTRE).squareMagnitude;
				if (squareMagnitude == 0)
				{
					this.sampleGrid[i, j].score = 0f;
				}
				else if (squareMagnitude <= 100)
				{
					this.sampleGrid[i, j].score = this.sampleGrid[i, j].gradient / Mathf.Sqrt((float)squareMagnitude);
					this.maximumScore = Mathf.Max(this.maximumScore, this.sampleGrid[i, j].score);
				}
				else
				{
					this.sampleGrid[i, j].score = 0f;
				}
			}
		}
	}

	
	private void UpdateSampleGrid()
	{
		WaterOnTerrainSFX.IntVector intVector = new WaterOnTerrainSFX.IntVector(Mathf.FloorToInt(base.transform.position.x / this.gridCellSize), Mathf.FloorToInt(base.transform.position.z / this.gridCellSize));
		if (intVector != this.gridPosition)
		{
			WaterOnTerrainSFX.IntVector intVector2 = intVector - this.gridPosition;
			if (Math.Abs(intVector2.x) < 21 && Math.Abs(intVector2.y) < 21)
			{
				this.MoveSampleGridEntries(-intVector2);
				WaterOnTerrainSFX.IntVector intVector3;
				WaterOnTerrainSFX.IntVector intVector4;
				if (intVector2.x < 0)
				{
					intVector3.x = 0;
					intVector4.x = -intVector2.x;
				}
				else
				{
					intVector3.x = 21 - intVector2.x;
					intVector4.x = 21;
				}
				if (intVector2.y < 0)
				{
					intVector3.y = 0;
					intVector4.y = -intVector2.y;
				}
				else
				{
					intVector3.y = 21 - intVector2.y;
					intVector4.y = 21;
				}
				this.CacheGradients(intVector, new WaterOnTerrainSFX.IntVector(0, intVector3.y), new WaterOnTerrainSFX.IntVector(21, intVector4.y));
				if (intVector3.y > 0)
				{
					this.CacheGradients(intVector, new WaterOnTerrainSFX.IntVector(intVector3.x, 0), new WaterOnTerrainSFX.IntVector(intVector4.x, intVector3.y));
				}
				else
				{
					this.CacheGradients(intVector, new WaterOnTerrainSFX.IntVector(intVector3.x, intVector4.y), new WaterOnTerrainSFX.IntVector(intVector4.x, 21));
				}
			}
			else
			{
				this.CacheGradients(intVector, new WaterOnTerrainSFX.IntVector(0, 0), new WaterOnTerrainSFX.IntVector(21, 21));
			}
			this.CalculateSampleGridScores();
			int i = 0;
			while (i < this.sources.Count)
			{
				WaterOnTerrainSFX.Source source = this.sources[i];
				this.sources[i].localPosition -= intVector2;
				int squareMagnitude = (source.localPosition - WaterOnTerrainSFX.SAMPLE_GRID_CENTRE).squareMagnitude;
				if (!WaterOnTerrainSFX.IsWithinSampleGrid(source.localPosition) || squareMagnitude > 100)
				{
					this.StopSource(i);
				}
				else
				{
					i++;
				}
			}
			this.gridPosition = intVector;
		}
	}

	
	private static void CreateDebugTextures()
	{
		WaterOnTerrainSFX.cellTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		WaterOnTerrainSFX.cellTexture.SetPixel(0, 0, Color.white);
		WaterOnTerrainSFX.cellTexture.Apply();
		Color color = new Color(1f, 1f, 1f, 0.125f);
		Color color2 = new Color(0f, 0f, 0f, 0f);
		WaterOnTerrainSFX.sourceTexture = new Texture2D(5, 5, TextureFormat.ARGB32, false);
		FMOD_Listener.Fill(WaterOnTerrainSFX.sourceTexture, new Color(1f, 1f, 1f, 0.5f));
		WaterOnTerrainSFX.sourceTexture.SetPixel(0, 0, color);
		WaterOnTerrainSFX.sourceTexture.SetPixel(0, 4, color);
		WaterOnTerrainSFX.sourceTexture.SetPixel(4, 0, color);
		WaterOnTerrainSFX.sourceTexture.SetPixel(4, 4, color);
		WaterOnTerrainSFX.sourceTexture.SetPixel(1, 2, color);
		WaterOnTerrainSFX.sourceTexture.SetPixel(2, 1, color);
		WaterOnTerrainSFX.sourceTexture.SetPixel(3, 2, color);
		WaterOnTerrainSFX.sourceTexture.SetPixel(2, 3, color);
		WaterOnTerrainSFX.sourceTexture.SetPixel(2, 2, color2);
		WaterOnTerrainSFX.sourceTexture.Apply();
		WaterOnTerrainSFX.eventTexture = new Texture2D(3, 3, TextureFormat.ARGB32, false);
		FMOD_Listener.Fill(WaterOnTerrainSFX.eventTexture, new Color(1f, 1f, 1f, 0.5f));
		WaterOnTerrainSFX.eventTexture.SetPixel(0, 0, color);
		WaterOnTerrainSFX.eventTexture.SetPixel(0, 2, color);
		WaterOnTerrainSFX.eventTexture.SetPixel(2, 0, color);
		WaterOnTerrainSFX.eventTexture.SetPixel(2, 2, color);
		WaterOnTerrainSFX.eventTexture.Apply();
	}

	
	public void DrawDebug()
	{
		if (WaterOnTerrainSFX.cellTexture == null)
		{
			WaterOnTerrainSFX.CreateDebugTextures();
		}
		if (Camera.main != null)
		{
			Vector2 vector = new Vector2(110f, (float)(Camera.main.pixelHeight - 110));
			Vector2 vector2 = vector - new Vector2(105f, 105f);
			GUI.Box(new Rect(vector2.x, vector2.y - 35f, 210f, 245f), "Water On Terrain");
			GUI.Label(new Rect(vector2.x, vector2.y - 10f, 210f, 35f), string.Format("Wetness: {0:f2}", WaterOnTerrainSFX.Wetness));
			float num = 10f * this.gridCellSize * Mathf.Sqrt(2f);
			float d = 100f / num;
			float num2 = 0f;
			float num3 = 0f;
			for (int i = 0; i < 21; i++)
			{
				for (int j = 0; j < 21; j++)
				{
					num2 = Mathf.Max(num2, this.ScoreForSource(new WaterOnTerrainSFX.IntVector(i, j), null));
					num3 = Mathf.Max(num3, this.sampleGrid[i, j].gradient);
				}
			}
			float num4 = (num2 <= 0f) ? 0f : (1f / num2);
			float num5 = (num3 <= 0f) ? 0f : (1f / num3);
			Matrix4x4 matrix4x = Matrix4x4.TRS(vector, Quaternion.Euler(0f, 0f, -LocalPlayer.Transform.eulerAngles.y), new Vector3(1f, -1f, 1f) * d);
			matrix4x *= Matrix4x4.TRS(new Vector3(-LocalPlayer.Transform.position.x, -LocalPlayer.Transform.position.z, 0f), Quaternion.identity, Vector3.one);
			Matrix4x4 matrix = GUI.matrix;
			Color color = GUI.color;
			GUI.matrix *= matrix4x;
			for (int k = 0; k < 21; k++)
			{
				for (int l = 0; l < 21; l++)
				{
					WaterOnTerrainSFX.IntVector intVector = new WaterOnTerrainSFX.IntVector(k, l);
					GUI.color = new Color(0f, this.sampleGrid[k, l].gradient * num5, this.ScoreForSource(intVector, null) * num4, 0.5f);
					GUI.DrawTexture(this.GridCellRect(this.SampleGridToWorldGrid(intVector)), WaterOnTerrainSFX.cellTexture);
				}
			}
			GUI.matrix = matrix;
			GUI.color = color;
			if (this.centreSource.IsPlaying())
			{
				this.DrawSource(this.centreSource, matrix4x);
			}
			for (int m = 0; m < this.sources.Count; m++)
			{
				this.DrawSource(this.sources[m], matrix4x);
			}
		}
	}

	
	private void DrawSource(WaterOnTerrainSFX.Source source, Matrix4x4 worldToScreenMatrix)
	{
		Vector3 v = this.EventPositionForLocalPosition(source.localPosition);
		v = worldToScreenMatrix.MultiplyPoint3x4(new Vector3(v.x, v.z));
		this.DrawTexture(v, WaterOnTerrainSFX.sourceTexture);
		v = source.eventPosition;
		v = worldToScreenMatrix.MultiplyPoint3x4(new Vector3(v.x, v.z));
		this.DrawTexture(v, WaterOnTerrainSFX.eventTexture);
	}

	
	private void DrawTexture(Vector2 position, Texture2D texture)
	{
		float num = (float)texture.width;
		float height = (float)texture.height;
		GUI.DrawTexture(new Rect(position.x - num / 2f, position.y - num / 2f, num, height), texture);
	}

	
	private static Vector2 GuiPosition(Vector3 worldPosition)
	{
		return new Vector2(worldPosition.x, -worldPosition.z);
	}

	
	private const int MAXIMUM_GRID_DISTANCE = 10;

	
	private const int SAMPLE_GRID_SIZE = 21;

	
	private const int MAXIMUM_GRID_DISTANCE_SQUARED = 100;

	
	private const string EVENT_PATH = "event:/ambient/water/water_runoff";

	
	private const float MAXIMUM_GRADIENT = 60f;

	
	private const float MAXIMUM_SCORE_RATIO = 5f;

	
	private const int SOURCE_EFFECT_RADIUS = 5;

	
	private const float SOURCE_EFFECT_AMOUNT = 0.1f;

	
	public static float Wetness = 0f;

	
	public int targetSourceCount = 3;

	
	public float sourceSpeed = 50f;

	
	private static readonly WaterOnTerrainSFX.IntVector SAMPLE_GRID_CENTRE = new WaterOnTerrainSFX.IntVector(10, 10);

	
	private WaterOnTerrainSFX.IntVector gridPosition;

	
	private WaterOnTerrainSFX.GridCell[,] sampleGrid;

	
	private WaterOnTerrainSFX.Source centreSource;

	
	private List<WaterOnTerrainSFX.Source> sources;

	
	private Stack<WaterOnTerrainSFX.Source> sourcePool;

	
	private EventDescription eventDescription;

	
	private int gradientParameterIndex;

	
	private int wetnessParameterIndex;

	
	private float gridCellSize = 10f;

	
	private float maximumScore;

	
	private static readonly WaterOnTerrainSFX.IntVector[] NEIGHBOURS = new WaterOnTerrainSFX.IntVector[]
	{
		new WaterOnTerrainSFX.IntVector(-1, -1),
		new WaterOnTerrainSFX.IntVector(0, -1),
		new WaterOnTerrainSFX.IntVector(1, -1),
		new WaterOnTerrainSFX.IntVector(-1, 0),
		new WaterOnTerrainSFX.IntVector(1, 0),
		new WaterOnTerrainSFX.IntVector(-1, 1),
		new WaterOnTerrainSFX.IntVector(0, 1),
		new WaterOnTerrainSFX.IntVector(1, 1)
	};

	
	private static Texture2D cellTexture = null;

	
	private static Texture2D sourceTexture = null;

	
	private static Texture2D eventTexture = null;

	
	private struct IntVector
	{
		
		public IntVector(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		
		
		public int squareMagnitude
		{
			get
			{
				return this.x * this.x + this.y * this.y;
			}
		}

		
		
		public float magnitude
		{
			get
			{
				return Mathf.Sqrt((float)this.squareMagnitude);
			}
		}

		
		public static bool operator ==(WaterOnTerrainSFX.IntVector a, WaterOnTerrainSFX.IntVector b)
		{
			return a.x == b.x && a.y == b.y;
		}

		
		public static bool operator !=(WaterOnTerrainSFX.IntVector a, WaterOnTerrainSFX.IntVector b)
		{
			return !(a == b);
		}

		
		public static WaterOnTerrainSFX.IntVector operator -(WaterOnTerrainSFX.IntVector a, WaterOnTerrainSFX.IntVector b)
		{
			return new WaterOnTerrainSFX.IntVector(a.x - b.x, a.y - b.y);
		}

		
		public static WaterOnTerrainSFX.IntVector operator -(WaterOnTerrainSFX.IntVector v)
		{
			return new WaterOnTerrainSFX.IntVector(-v.x, -v.y);
		}

		
		public static WaterOnTerrainSFX.IntVector operator +(WaterOnTerrainSFX.IntVector a, WaterOnTerrainSFX.IntVector b)
		{
			return new WaterOnTerrainSFX.IntVector(a.x + b.x, a.y + b.y);
		}

		
		public int x;

		
		public int y;
	}

	
	private struct GridCell
	{
		
		public float gradient;

		
		public float score;
	}

	
	private class Source
	{
		
		
		
		public Vector3 eventPosition { get; private set; }

		
		public void Start(EventDescription eventDescription, int gradientParameterIndex, int wetnessParameterIndex, WaterOnTerrainSFX.IntVector localPosition, Vector3 eventPosition, float gradient)
		{
			this.localPosition = localPosition;
			this.eventPosition = eventPosition;
			this.gradientParameterIndex = gradientParameterIndex;
			this.wetnessParameterIndex = wetnessParameterIndex;
			UnityUtil.ERRCHECK(eventDescription.createInstance(out this.eventInstance));
			UnityUtil.ERRCHECK(this.eventInstance.set3DAttributes(eventPosition.to3DAttributes()));
			this.SetGradient(gradient);
			UnityUtil.ERRCHECK(this.eventInstance.start());
		}

		
		public bool IsPlaying()
		{
			return this.eventInstance != null;
		}

		
		public void SetGradient(float gradient)
		{
			if (this.gradientParameterIndex >= 0)
			{
				gradient /= 60f;
				UnityUtil.ERRCHECK(this.eventInstance.setParameterValueByIndex(this.gradientParameterIndex, gradient));
			}
		}

		
		public void UpdateEvent(Vector3 targetPosition, float distance, float wetness)
		{
			if (this.eventPosition != targetPosition)
			{
				Vector3 a = targetPosition - this.eventPosition;
				this.eventPosition += a * Mathf.Clamp01(distance / a.magnitude);
				UnityUtil.ERRCHECK(this.eventInstance.set3DAttributes(this.eventPosition.to3DAttributes()));
			}
			if (this.wetnessParameterIndex >= 0)
			{
				UnityUtil.ERRCHECK(this.eventInstance.setParameterValueByIndex(this.wetnessParameterIndex, wetness));
			}
		}

		
		public void Stop()
		{
			if (this.eventInstance != null)
			{
				UnityUtil.ERRCHECK(this.eventInstance.stop(STOP_MODE.ALLOWFADEOUT));
				UnityUtil.ERRCHECK(this.eventInstance.release());
				this.eventInstance = null;
			}
		}

		
		public WaterOnTerrainSFX.IntVector localPosition;

		
		private EventInstance eventInstance;

		
		private int gradientParameterIndex;

		
		private int wetnessParameterIndex;
	}
}
