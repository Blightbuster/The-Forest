﻿using System;
using UnityEngine;

namespace Pathfinding
{
	
	[HelpURL("http:
	public class DebugUtility : MonoBehaviour
	{
		
		public void Awake()
		{
			DebugUtility.active = this;
		}

		
		public static void DrawCubes(Vector3[] topVerts, Vector3[] bottomVerts, Color[] vertexColors, float width)
		{
			if (DebugUtility.active == null)
			{
				DebugUtility.active = (UnityEngine.Object.FindObjectOfType(typeof(DebugUtility)) as DebugUtility);
			}
			if (DebugUtility.active == null)
			{
				throw new NullReferenceException();
			}
			if (topVerts.Length != bottomVerts.Length || topVerts.Length != vertexColors.Length)
			{
				Debug.LogError("Array Lengths are not the same");
				return;
			}
			if (topVerts.Length > 2708)
			{
				Vector3[] array = new Vector3[topVerts.Length - 2708];
				Vector3[] array2 = new Vector3[topVerts.Length - 2708];
				Color[] array3 = new Color[topVerts.Length - 2708];
				for (int i = 2708; i < topVerts.Length; i++)
				{
					array[i - 2708] = topVerts[i];
					array2[i - 2708] = bottomVerts[i];
					array3[i - 2708] = vertexColors[i];
				}
				Vector3[] array4 = new Vector3[2708];
				Vector3[] array5 = new Vector3[2708];
				Color[] array6 = new Color[2708];
				for (int j = 0; j < 2708; j++)
				{
					array4[j] = topVerts[j];
					array5[j] = bottomVerts[j];
					array6[j] = vertexColors[j];
				}
				DebugUtility.DrawCubes(array, array2, array3, width);
				topVerts = array4;
				bottomVerts = array5;
				vertexColors = array6;
			}
			width /= 2f;
			Vector3[] array7 = new Vector3[topVerts.Length * 4 * 6];
			int[] array8 = new int[topVerts.Length * 6 * 6];
			Color[] array9 = new Color[topVerts.Length * 4 * 6];
			for (int k = 0; k < topVerts.Length; k++)
			{
				Vector3 a = topVerts[k] + new Vector3(0f, DebugUtility.active.offset, 0f);
				Vector3 a2 = bottomVerts[k] - new Vector3(0f, DebugUtility.active.offset, 0f);
				Vector3 vector = a + new Vector3(-width, 0f, -width);
				Vector3 vector2 = a + new Vector3(width, 0f, -width);
				Vector3 vector3 = a + new Vector3(width, 0f, width);
				Vector3 vector4 = a + new Vector3(-width, 0f, width);
				Vector3 vector5 = a2 + new Vector3(-width, 0f, -width);
				Vector3 vector6 = a2 + new Vector3(width, 0f, -width);
				Vector3 vector7 = a2 + new Vector3(width, 0f, width);
				Vector3 vector8 = a2 + new Vector3(-width, 0f, width);
				int num = k * 4 * 6;
				int num2 = k * 6 * 6;
				Color color = vertexColors[k];
				for (int l = num; l < num + 24; l++)
				{
					array9[l] = color;
				}
				array7[num] = vector;
				array7[num + 1] = vector4;
				array7[num + 2] = vector3;
				array7[num + 3] = vector2;
				num += 4;
				array7[num + 3] = vector5;
				array7[num + 2] = vector8;
				array7[num + 1] = vector7;
				array7[num] = vector6;
				num += 4;
				array7[num] = vector6;
				array7[num + 1] = vector2;
				array7[num + 2] = vector3;
				array7[num + 3] = vector7;
				num += 4;
				array7[num + 3] = vector5;
				array7[num + 2] = vector;
				array7[num + 1] = vector4;
				array7[num] = vector8;
				num += 4;
				array7[num + 3] = vector7;
				array7[num + 2] = vector8;
				array7[num + 1] = vector4;
				array7[num] = vector3;
				num += 4;
				array7[num] = vector6;
				array7[num + 1] = vector5;
				array7[num + 2] = vector;
				array7[num + 3] = vector2;
				for (int m = 0; m < 6; m++)
				{
					int num3 = num + m * 4;
					int num4 = num2 + m * 6;
					array8[num4] = num3;
					array8[num4 + 1] = num3 + 1;
					array8[num4 + 2] = num3 + 2;
					array8[num4 + 3] = num3;
					array8[num4 + 4] = num3 + 2;
					array8[num4 + 5] = num3 + 3;
				}
			}
			Mesh mesh = new Mesh();
			mesh.vertices = array7;
			mesh.triangles = array8;
			mesh.colors = array9;
			mesh.name = "VoxelMesh";
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			if (DebugUtility.active.optimizeMeshes)
			{
			}
			GameObject gameObject = new GameObject("DebugMesh");
			MeshRenderer meshRenderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
			meshRenderer.material = DebugUtility.active.defaultMaterial;
			(gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter).mesh = mesh;
		}

		
		public static void DrawQuads(Vector3[] verts, float width)
		{
			if (verts.Length >= 16250)
			{
				Vector3[] array = new Vector3[verts.Length - 16250];
				for (int i = 16250; i < verts.Length; i++)
				{
					array[i - 16250] = verts[i];
				}
				Vector3[] array2 = new Vector3[16250];
				for (int j = 0; j < 16250; j++)
				{
					array2[j] = verts[j];
				}
				DebugUtility.DrawQuads(array, width);
				verts = array2;
			}
			width /= 2f;
			Vector3[] array3 = new Vector3[verts.Length * 4];
			int[] array4 = new int[verts.Length * 6];
			for (int k = 0; k < verts.Length; k++)
			{
				Vector3 a = verts[k];
				int num = k * 4;
				array3[num] = a + new Vector3(-width, 0f, -width);
				array3[num + 1] = a + new Vector3(-width, 0f, width);
				array3[num + 2] = a + new Vector3(width, 0f, width);
				array3[num + 3] = a + new Vector3(width, 0f, -width);
				int num2 = k * 6;
				array4[num2] = num;
				array4[num2 + 1] = num + 1;
				array4[num2 + 2] = num + 2;
				array4[num2 + 3] = num;
				array4[num2 + 4] = num + 2;
				array4[num2 + 5] = num + 3;
			}
			Mesh mesh = new Mesh();
			mesh.vertices = array3;
			mesh.triangles = array4;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			GameObject gameObject = new GameObject("DebugMesh");
			MeshRenderer meshRenderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
			meshRenderer.material = DebugUtility.active.defaultMaterial;
			(gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter).mesh = mesh;
		}

		
		public Material defaultMaterial;

		
		public static DebugUtility active;

		
		public float offset = 0.2f;

		
		public bool optimizeMeshes;
	}
}
