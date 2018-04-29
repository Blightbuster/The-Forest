using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace OrbCreationExtensions
{
	
	public static class GameObjectExtensions
	{
		
		public static Bounds GetWorldBounds(this GameObject go)
		{
			if (go.transform == null)
			{
				return default(Bounds);
			}
			Bounds bounds = new Bounds(go.transform.position, Vector3.zero);
			Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>(true);
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				bounds = componentsInChildren[0].bounds;
			}
			foreach (Renderer renderer in componentsInChildren)
			{
				Bounds bounds2 = renderer.bounds;
				bounds.Encapsulate(bounds2);
			}
			return bounds;
		}

		
		public static Vector3[] GetBoundsCorners(this Bounds bounds)
		{
			Vector3[] array = new Vector3[8];
			for (int i = 0; i < 8; i++)
			{
				array[i] = bounds.min;
				if ((i & 1) > 0)
				{
					Vector3[] array2 = array;
					int num = i;
					array2[num].x = array2[num].x + bounds.size.x;
				}
				if ((i & 2) > 0)
				{
					Vector3[] array3 = array;
					int num2 = i;
					array3[num2].y = array3[num2].y + bounds.size.y;
				}
				if ((i & 4) > 0)
				{
					Vector3[] array4 = array;
					int num3 = i;
					array4[num3].z = array4[num3].z + bounds.size.z;
				}
			}
			return array;
		}

		
		public static Vector3[] GetBoundsCenterAndCorners(this Bounds bounds)
		{
			Vector3[] array = new Vector3[9];
			array[0] = bounds.center;
			for (int i = 1; i < 9; i++)
			{
				array[i] = bounds.min;
				if ((i & 1) > 0)
				{
					Vector3[] array2 = array;
					int num = i;
					array2[num].x = array2[num].x + bounds.size.x;
				}
				if ((i & 2) > 0)
				{
					Vector3[] array3 = array;
					int num2 = i;
					array3[num2].y = array3[num2].y + bounds.size.y;
				}
				if ((i & 4) > 0)
				{
					Vector3[] array4 = array;
					int num3 = i;
					array4[num3].z = array4[num3].z + bounds.size.z;
				}
			}
			return array;
		}

		
		public static Vector3[] GetWorldBoundsCorners(this GameObject go)
		{
			return go.GetWorldBounds().GetBoundsCorners();
		}

		
		public static Vector3[] GetWorldBoundsCenterAndCorners(this GameObject go)
		{
			return go.GetWorldBounds().GetBoundsCenterAndCorners();
		}

		
		public static float GetModelComplexity(this GameObject go)
		{
			float num = 0f;
			MeshFilter[] componentsInChildren = go.GetComponentsInChildren<MeshFilter>(true);
			foreach (MeshFilter meshFilter in componentsInChildren)
			{
				Mesh sharedMesh = meshFilter.sharedMesh;
				float num2 = 1f;
				for (int j = 0; j < sharedMesh.subMeshCount; j++)
				{
					num += num2 * (float)sharedMesh.GetTriangles(j).Length / 3f / 65536f;
					num2 *= 1.1f;
				}
			}
			return num;
		}

		
		public static string GetModelInfoString(this GameObject go)
		{
			string arg = string.Empty;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			Bounds worldBounds = go.GetWorldBounds();
			MeshFilter[] componentsInChildren = go.GetComponentsInChildren<MeshFilter>(true);
			foreach (MeshFilter meshFilter in componentsInChildren)
			{
				Mesh sharedMesh = meshFilter.sharedMesh;
				num++;
				num2 += sharedMesh.subMeshCount;
				num3 += sharedMesh.vertices.Length;
				num4 += sharedMesh.triangles.Length / 3;
			}
			arg = arg + num + " meshes\n";
			arg = arg + num2 + " submeshes\n";
			arg = arg + num3 + " vertices\n";
			arg = arg + num4 + " triangles\n";
			return arg + worldBounds.size + " meters";
		}

		
		public static GameObject TopParent(this GameObject go)
		{
			Transform parent = go.transform.parent;
			if (parent == null)
			{
				return go;
			}
			return parent.gameObject.TopParent();
		}

		
		public static GameObject FindParentWithName(this GameObject go, string parentName)
		{
			if (go.name == parentName)
			{
				return go;
			}
			Transform parent = go.transform.parent;
			if (parent == null)
			{
				return null;
			}
			return parent.gameObject.FindParentWithName(parentName);
		}

		
		public static GameObject FindMutualParent(this GameObject go1, GameObject go2)
		{
			if (go2 == null || go1 == go2)
			{
				return null;
			}
			Transform transform = go2.transform;
			while (transform != null)
			{
				if (go1 == transform.gameObject)
				{
					return go1;
				}
				transform = transform.parent;
			}
			Transform parent = go1.transform.parent;
			if (parent == null)
			{
				return null;
			}
			return parent.gameObject.FindMutualParent(go2);
		}

		
		public static GameObject FindFirstChildWithName(this GameObject go, string childName)
		{
			if (go.name == childName)
			{
				return go;
			}
			Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].gameObject.name == childName)
				{
					return componentsInChildren[i].gameObject;
				}
			}
			return null;
		}

		
		public static bool IsChildWithNameUnique(this GameObject go, string childName)
		{
			int num = 0;
			go.CountChildrenWithName(childName, ref num);
			return num <= 1;
		}

		
		public static void CountChildrenWithName(this GameObject go, string childName, ref int total)
		{
			if (go.name == childName)
			{
				total++;
			}
			IEnumerator enumerator = go.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					transform.gameObject.CountChildrenWithName(childName, ref total);
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

		
		public static GameObject GetGameObjectNamed(this GameObject go, string aStr, GameObject parentGO)
		{
			Transform[] componentsInChildren = parentGO.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].gameObject.name == aStr)
				{
					return componentsInChildren[i].gameObject;
				}
			}
			return null;
		}

		
		public static void DestroyChildren(this GameObject go, bool disabledOnly)
		{
			List<Transform> list = new List<Transform>();
			IEnumerator enumerator = go.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					if (!transform.gameObject.activeSelf || !disabledOnly)
					{
						list.Add(transform);
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
			for (int i = list.Count - 1; i >= 0; i--)
			{
				list[i].SetParent(null);
				UnityEngine.Object.Destroy(list[i].gameObject);
			}
		}

		
		public static T GetFirstComponentInParents<T>(this GameObject go) where T : Component
		{
			T component = go.GetComponent<T>();
			if (component != null)
			{
				return component;
			}
			if (go.transform.parent != null && go.transform.parent.gameObject != go)
			{
				return go.transform.parent.gameObject.GetFirstComponentInParents<T>();
			}
			return (T)((object)null);
		}

		
		public static T GetFirstComponentInChildren<T>(this GameObject go) where T : Component
		{
			T[] componentsInChildren = go.GetComponentsInChildren<T>();
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				return componentsInChildren[0];
			}
			return (T)((object)null);
		}

		
		public static Mesh[] GetMeshes(this GameObject aGo)
		{
			return aGo.GetMeshes(true);
		}

		
		public static Mesh[] GetMeshes(this GameObject aGo, bool includeDisabled)
		{
			MeshFilter[] componentsInChildren = aGo.GetComponentsInChildren<MeshFilter>(includeDisabled);
			SkinnedMeshRenderer[] componentsInChildren2 = aGo.GetComponentsInChildren<SkinnedMeshRenderer>(includeDisabled);
			int num = 0;
			if (componentsInChildren != null)
			{
				num += componentsInChildren.Length;
			}
			if (componentsInChildren2 != null)
			{
				num += componentsInChildren2.Length;
			}
			if (num == 0)
			{
				return null;
			}
			Mesh[] array = new Mesh[num];
			int num2 = 0;
			while (componentsInChildren != null && num2 < componentsInChildren.Length)
			{
				array[num2] = componentsInChildren[num2].sharedMesh;
				num2++;
			}
			int num3 = num2;
			num2 = 0;
			while (componentsInChildren2 != null && num2 < componentsInChildren2.Length)
			{
				array[num2 + num3] = componentsInChildren2[num2].sharedMesh;
				num2++;
			}
			return array;
		}

		
		public static int GetTotalVertexCount(this GameObject aGo)
		{
			MeshFilter[] componentsInChildren = aGo.GetComponentsInChildren<MeshFilter>(false);
			SkinnedMeshRenderer[] componentsInChildren2 = aGo.GetComponentsInChildren<SkinnedMeshRenderer>(false);
			int num = 0;
			int num2 = 0;
			while (componentsInChildren != null && num2 < componentsInChildren.Length)
			{
				Mesh sharedMesh = componentsInChildren[num2].sharedMesh;
				if (sharedMesh != null)
				{
					num += sharedMesh.vertexCount;
				}
				num2++;
			}
			int num3 = 0;
			while (componentsInChildren2 != null && num3 < componentsInChildren2.Length)
			{
				Mesh sharedMesh2 = componentsInChildren2[num3].sharedMesh;
				if (sharedMesh2 != null)
				{
					num += sharedMesh2.vertexCount;
				}
				num3++;
			}
			return num;
		}

		
		public static Mesh Get1stSharedMesh(this GameObject aGo)
		{
			MeshFilter[] componentsInChildren = aGo.GetComponentsInChildren<MeshFilter>(false);
			int num = 0;
			while (componentsInChildren != null && num < componentsInChildren.Length)
			{
				if (componentsInChildren[num].sharedMesh != null)
				{
					return componentsInChildren[num].sharedMesh;
				}
				num++;
			}
			SkinnedMeshRenderer[] componentsInChildren2 = aGo.GetComponentsInChildren<SkinnedMeshRenderer>(false);
			int num2 = 0;
			while (componentsInChildren2 != null && num2 < componentsInChildren2.Length)
			{
				if (componentsInChildren2[num2].sharedMesh != null)
				{
					return componentsInChildren2[num2].sharedMesh;
				}
				num2++;
			}
			return null;
		}

		
		public static void SetMeshes(this GameObject aGo, Mesh[] meshes)
		{
			aGo.SetMeshes(meshes, true, 0);
		}

		
		public static void SetMeshes(this GameObject aGo, Mesh[] meshes, int lodLevel)
		{
			aGo.SetMeshes(meshes, true, lodLevel);
		}

		
		public static void SetMeshes(this GameObject aGo, Mesh[] meshes, bool includeDisabled, int lodLevel)
		{
			MeshFilter[] componentsInChildren = aGo.GetComponentsInChildren<MeshFilter>(includeDisabled);
			SkinnedMeshRenderer[] componentsInChildren2 = aGo.GetComponentsInChildren<SkinnedMeshRenderer>(includeDisabled);
			int num = 0;
			if (componentsInChildren != null)
			{
				num += componentsInChildren.Length;
			}
			if (componentsInChildren2 != null)
			{
				num += componentsInChildren2.Length;
			}
			if (num == 0)
			{
				return;
			}
			int num2 = 0;
			while (componentsInChildren != null && num2 < componentsInChildren.Length)
			{
				LODSwitcher lodswitcher = componentsInChildren[num2].gameObject.GetComponent<LODSwitcher>();
				if (meshes != null && meshes.Length > num2)
				{
					if (lodLevel == 0)
					{
						componentsInChildren[num2].sharedMesh = meshes[num2];
					}
					if (lodswitcher == null && lodLevel > 0)
					{
						lodswitcher = componentsInChildren[num2].gameObject.AddComponent<LODSwitcher>();
						lodswitcher.SetMesh(componentsInChildren[num2].sharedMesh, 0);
					}
					if (lodswitcher != null)
					{
						lodswitcher.SetMesh(meshes[num2], lodLevel);
					}
				}
				else
				{
					if (lodswitcher != null)
					{
						lodswitcher.SetMesh(null, lodLevel);
					}
					if (lodLevel == 0)
					{
						componentsInChildren[num2].sharedMesh = null;
					}
				}
				num2++;
			}
			int num3 = num2;
			num2 = 0;
			while (componentsInChildren2 != null && num2 < componentsInChildren2.Length)
			{
				LODSwitcher lodswitcher2 = componentsInChildren2[num2].gameObject.GetComponent<LODSwitcher>();
				if (meshes != null && meshes.Length > num2 + num3)
				{
					if (lodLevel == 0)
					{
						componentsInChildren2[num2].sharedMesh = meshes[num2 + num3];
					}
					if (lodswitcher2 == null && lodLevel > 0)
					{
						lodswitcher2 = componentsInChildren2[num2].gameObject.AddComponent<LODSwitcher>();
						lodswitcher2.SetMesh(componentsInChildren2[num2].sharedMesh, 0);
					}
					if (lodswitcher2 != null)
					{
						lodswitcher2.SetMesh(meshes[num2 + num3], lodLevel);
					}
				}
				else
				{
					if (lodswitcher2 != null)
					{
						lodswitcher2.SetMesh(null, lodLevel);
					}
					if (lodLevel == 0)
					{
						componentsInChildren2[num2].sharedMesh = null;
					}
				}
				num2++;
			}
		}

		
		public static Material[] GetMaterials(this GameObject aGo, bool includeDisabled)
		{
			List<Material> list = new List<Material>();
			MeshRenderer[] componentsInChildren = aGo.GetComponentsInChildren<MeshRenderer>(includeDisabled);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				list.AddRange(componentsInChildren[i].sharedMaterials);
			}
			SkinnedMeshRenderer[] componentsInChildren2 = aGo.GetComponentsInChildren<SkinnedMeshRenderer>(includeDisabled);
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				list.AddRange(componentsInChildren2[j].sharedMaterials);
			}
			return list.ToArray();
		}

		
		public static Mesh[] CombineMeshes(this GameObject aGO)
		{
			return aGO.CombineMeshes(new string[0]);
		}

		
		public static Mesh[] CombineMeshes(this GameObject aGO, string[] skipSubmeshNames)
		{
			List<Mesh> list = new List<Mesh>();
			MeshRenderer[] componentsInChildren = aGO.GetComponentsInChildren<MeshRenderer>(false);
			SkinnedMeshRenderer[] componentsInChildren2 = aGO.GetComponentsInChildren<SkinnedMeshRenderer>(false);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = -999;
			bool flag = false;
			if (aGO.GetComponent<SkinnedMeshRenderer>() != null || aGO.GetComponent<MeshRenderer>() != null)
			{
				flag = true;
			}
			if (componentsInChildren2 != null && componentsInChildren2.Length > 0)
			{
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren2)
				{
					if (skinnedMeshRenderer.sharedMesh != null)
					{
						num += skinnedMeshRenderer.sharedMesh.vertexCount;
						num2++;
						num3++;
					}
				}
			}
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				foreach (MeshRenderer meshRenderer in componentsInChildren)
				{
					MeshFilter component = meshRenderer.gameObject.GetComponent<MeshFilter>();
					if (component != null && component.sharedMesh != null)
					{
						if (num4 == -999 && meshRenderer.lightmapIndex >= 0 && meshRenderer.lightmapIndex <= 253)
						{
							num4 = meshRenderer.lightmapIndex;
						}
						if (num4 < 0 || meshRenderer.lightmapIndex < 0 || meshRenderer.lightmapIndex > 253 || num4 == meshRenderer.lightmapIndex)
						{
							num += component.sharedMesh.vertexCount;
							num2++;
						}
					}
					num3++;
				}
			}
			if (num2 == 0)
			{
				throw new ApplicationException("No meshes found in children. There's nothing to combine.");
			}
			if (flag)
			{
				GameObject gameObject = new GameObject();
				string text = aGO.name + "_Merged";
				string name = text;
				int num5 = 0;
				while (GameObject.Find(name) != null)
				{
					name = text + "_" + num5;
					num5++;
				}
				gameObject.name = name;
				gameObject.transform.SetParent(aGO.transform.parent);
				gameObject.transform.localPosition = aGO.transform.localPosition;
				gameObject.transform.localRotation = aGO.transform.localRotation;
				gameObject.transform.localScale = aGO.transform.localScale;
				aGO = gameObject;
			}
			int num6 = 1;
			int num7 = -1;
			int k = 0;
			while (k < num2)
			{
				if (num7 == k)
				{
					break;
				}
				num7 = k;
				GameObject gameObject2 = aGO;
				if (num > 65534)
				{
					gameObject2 = new GameObject();
					gameObject2.name = "Merged part " + num6++;
					gameObject2.transform.SetParent(aGO.transform);
					gameObject2.transform.localPosition = Vector3.zero;
					gameObject2.transform.localRotation = Quaternion.identity;
					gameObject2.transform.localScale = Vector3.one;
				}
				Mesh mesh = null;
				List<Vector3> list2 = new List<Vector3>();
				List<Vector3> list3 = new List<Vector3>();
				List<Vector2> list4 = new List<Vector2>();
				List<Vector2> list5 = new List<Vector2>();
				List<Vector2> list6 = new List<Vector2>();
				List<Vector2> list7 = new List<Vector2>();
				List<Color32> list8 = new List<Color32>();
				List<Transform> list9 = new List<Transform>();
				List<Matrix4x4> list10 = new List<Matrix4x4>();
				List<BoneWeight> list11 = new List<BoneWeight>();
				Dictionary<Material, List<int>> dictionary = new Dictionary<Material, List<int>>();
				if (componentsInChildren2 != null && componentsInChildren2.Length > 0)
				{
					bool flag2 = false;
					bool flag3 = false;
					int num8 = 0;
					int num9 = -1;
					int num10 = 0;
					foreach (SkinnedMeshRenderer skinnedMeshRenderer2 in componentsInChildren2)
					{
						if (skinnedMeshRenderer2.sharedMesh != null)
						{
							if (list2.Count + skinnedMeshRenderer2.sharedMesh.vertexCount > 65534)
							{
								flag2 = true;
							}
							if (mesh == null && skinnedMeshRenderer2.sharedMesh.blendShapeCount > 0 && k <= num8 && !flag2)
							{
								mesh = UnityEngine.Object.Instantiate<Mesh>(skinnedMeshRenderer2.sharedMesh);
								mesh.uv4 = null;
								mesh.uv3 = null;
								mesh.uv2 = null;
								mesh.uv2 = null;
								mesh.boneWeights = null;
								mesh.colors32 = null;
								mesh.triangles = null;
								mesh.tangents = null;
								mesh.normals = null;
								mesh.vertices = null;
								bool flag4 = GameObjectExtensions.MergeMeshInto(skinnedMeshRenderer2.sharedMesh, skinnedMeshRenderer2.bones, skinnedMeshRenderer2.sharedMaterials, list2, list3, list4, list5, list6, list7, list8, list11, list9, list10, dictionary, skinnedMeshRenderer2.transform.localScale.x * skinnedMeshRenderer2.transform.localScale.y * skinnedMeshRenderer2.transform.localScale.z < 0f, new Vector4(1f, 1f, 0f, 0f), skinnedMeshRenderer2.transform, gameObject2.transform, skinnedMeshRenderer2.gameObject.name + "_" + skinnedMeshRenderer2.sharedMesh.name, skipSubmeshNames);
								if (flag4 && skinnedMeshRenderer2.gameObject != gameObject2)
								{
									skinnedMeshRenderer2.gameObject.SetActive(false);
								}
								num9 = num10;
							}
							num10++;
						}
					}
					foreach (SkinnedMeshRenderer skinnedMeshRenderer3 in componentsInChildren2)
					{
						if (skinnedMeshRenderer3.sharedMesh != null)
						{
							if (list2.Count + skinnedMeshRenderer3.sharedMesh.vertexCount > 65534)
							{
								flag2 = true;
							}
							if (k <= num8 && !flag2)
							{
								if (num8 != num9)
								{
									bool flag5 = GameObjectExtensions.MergeMeshInto(skinnedMeshRenderer3.sharedMesh, skinnedMeshRenderer3.bones, skinnedMeshRenderer3.sharedMaterials, list2, list3, list4, list5, list6, list7, list8, list11, list9, list10, dictionary, skinnedMeshRenderer3.transform.localScale.x * skinnedMeshRenderer3.transform.localScale.y * skinnedMeshRenderer3.transform.localScale.z < 0f, new Vector4(1f, 1f, 0f, 0f), skinnedMeshRenderer3.transform, gameObject2.transform, skinnedMeshRenderer3.gameObject.name + "_" + skinnedMeshRenderer3.sharedMesh.name, skipSubmeshNames);
									if (flag5 && skinnedMeshRenderer3.gameObject != gameObject2)
									{
										skinnedMeshRenderer3.gameObject.SetActive(false);
									}
								}
								flag3 = true;
								k++;
							}
							num8++;
						}
					}
					if (componentsInChildren != null && componentsInChildren.Length > 0 && flag3)
					{
						foreach (MeshRenderer meshRenderer2 in componentsInChildren)
						{
							MeshFilter component2 = meshRenderer2.gameObject.GetComponent<MeshFilter>();
							if (component2 != null && component2.sharedMesh != null && component2.gameObject != gameObject2)
							{
								if (list2.Count + component2.sharedMesh.vertexCount > 65534)
								{
									flag2 = true;
								}
								if (k <= num8 && !flag2)
								{
									bool flag6 = GameObjectExtensions.MergeMeshInto(component2.sharedMesh, null, meshRenderer2.sharedMaterials, list2, list3, list4, list5, list6, list7, list8, list11, list9, list10, dictionary, component2.transform.localScale.x * component2.transform.localScale.y * component2.transform.localScale.z < 0f, meshRenderer2.lightmapScaleOffset, component2.transform, gameObject2.transform, component2.gameObject.name + "_" + component2.sharedMesh.name, skipSubmeshNames);
									if (flag6)
									{
										meshRenderer2.enabled = false;
									}
									k++;
								}
								num8++;
							}
						}
					}
				}
				else if (componentsInChildren != null && componentsInChildren.Length > 0)
				{
					int num11 = 0;
					foreach (MeshRenderer meshRenderer3 in componentsInChildren)
					{
						if (num4 < 0 || meshRenderer3.lightmapIndex < 0 || meshRenderer3.lightmapIndex > 253 || num4 == meshRenderer3.lightmapIndex)
						{
							MeshFilter component3 = meshRenderer3.gameObject.GetComponent<MeshFilter>();
							if (component3 != null && component3.sharedMesh != null)
							{
								if (k <= num11 && list2.Count + component3.sharedMesh.vertexCount <= 65534)
								{
									bool flag7 = GameObjectExtensions.MergeMeshInto(component3.sharedMesh, null, meshRenderer3.sharedMaterials, list2, list3, list4, list5, list6, list7, list8, list11, list9, list10, dictionary, component3.transform.localScale.x * component3.transform.localScale.y * component3.transform.localScale.z < 0f, meshRenderer3.lightmapScaleOffset, component3.transform, gameObject2.transform, component3.gameObject.name + "_" + component3.sharedMesh.name, skipSubmeshNames);
									if (flag7 && component3.gameObject != gameObject2)
									{
										component3.gameObject.SetActive(false);
										Transform parent = component3.gameObject.transform.parent;
										if (parent != null && parent.gameObject != gameObject2)
										{
											parent.gameObject.SetActive(false);
										}
									}
									k++;
								}
								num11++;
							}
						}
					}
				}
				LODMaker.RemoveUnusedVertices(list2, list3, list4, list5, list6, list7, list8, list11, dictionary);
				if (mesh == null)
				{
					mesh = new Mesh();
				}
				mesh.vertices = list2.ToArray();
				if (list3.Count > 0)
				{
					mesh.normals = list3.ToArray();
				}
				bool flag8 = false;
				for (int num13 = 0; num13 < list4.Count; num13++)
				{
					if (list4[num13].x != 0f || list4[num13].y != 0f)
					{
						flag8 = true;
						break;
					}
				}
				if (flag8)
				{
					mesh.uv = list4.ToArray();
				}
				flag8 = false;
				for (int num14 = 0; num14 < list5.Count; num14++)
				{
					if (list5[num14].x != 0f || list5[num14].y != 0f)
					{
						flag8 = true;
						break;
					}
				}
				if (flag8)
				{
					mesh.uv2 = list5.ToArray();
				}
				flag8 = false;
				for (int num15 = 0; num15 < list6.Count; num15++)
				{
					if (list6[num15].x != 0f || list6[num15].y != 0f)
					{
						flag8 = true;
						break;
					}
				}
				if (flag8)
				{
					mesh.uv3 = list6.ToArray();
				}
				flag8 = false;
				for (int num16 = 0; num16 < list7.Count; num16++)
				{
					if (list7[num16].x != 0f || list7[num16].y != 0f)
					{
						flag8 = true;
						break;
					}
				}
				if (flag8)
				{
					mesh.uv4 = list7.ToArray();
				}
				flag8 = false;
				for (int num17 = 0; num17 < list8.Count; num17++)
				{
					if (list8[num17].r > 0 || list8[num17].g > 0 || list8[num17].b > 0)
					{
						flag8 = true;
						break;
					}
				}
				if (flag8)
				{
					mesh.colors32 = list8.ToArray();
				}
				if (list10.Count > 0)
				{
					mesh.bindposes = list10.ToArray();
				}
				if (list11.Count > 0)
				{
					if (list11.Count == list2.Count)
					{
						mesh.boneWeights = list11.ToArray();
					}
					else
					{
						Debug.LogWarning("Nr of bone weights not equal to nr of vertices.");
					}
				}
				mesh.subMeshCount = dictionary.Keys.Count;
				Material[] array7 = new Material[dictionary.Keys.Count];
				int num18 = 0;
				foreach (Material material in dictionary.Keys)
				{
					array7[num18] = material;
					mesh.SetTriangles(dictionary[material].ToArray(), num18++);
				}
				if (list3 == null || list3.Count <= 0)
				{
					mesh.RecalculateNormals();
				}
				mesh.RecalculateTangents();
				mesh.RecalculateBounds();
				if (componentsInChildren2 != null && componentsInChildren2.Length > 0)
				{
					SkinnedMeshRenderer skinnedMeshRenderer4 = gameObject2.GetComponent<SkinnedMeshRenderer>();
					if (skinnedMeshRenderer4 == null)
					{
						skinnedMeshRenderer4 = gameObject2.AddComponent<SkinnedMeshRenderer>();
					}
					skinnedMeshRenderer4.quality = componentsInChildren2[0].quality;
					skinnedMeshRenderer4.sharedMesh = mesh;
					skinnedMeshRenderer4.sharedMaterials = array7;
					skinnedMeshRenderer4.bones = list9.ToArray();
				}
				else if (componentsInChildren != null && componentsInChildren.Length > 0)
				{
					MeshRenderer meshRenderer4 = gameObject2.GetComponent<MeshRenderer>();
					if (meshRenderer4 == null)
					{
						meshRenderer4 = gameObject2.AddComponent<MeshRenderer>();
					}
					if (num4 >= 0 && num4 <= 253)
					{
						meshRenderer4.lightmapIndex = num4;
					}
					meshRenderer4.sharedMaterials = array7;
					MeshFilter meshFilter = gameObject2.GetComponent<MeshFilter>();
					if (meshFilter == null)
					{
						meshFilter = gameObject2.AddComponent<MeshFilter>();
					}
					meshFilter.sharedMesh = mesh;
				}
				list.Add(mesh);
			}
			return list.ToArray();
		}

		
		private static int GiveUniqueNameIfNeeded(GameObject aGo, GameObject topGO, int uniqueId)
		{
			if (topGO.IsChildWithNameUnique(aGo.name))
			{
				return uniqueId;
			}
			aGo.name = aGo.name + "_simpleLod" + ++uniqueId;
			return uniqueId;
		}

		
		public static void SetUpLODLevels(this GameObject go)
		{
			go.SetUpLODLevels(1f);
		}

		
		public static void SetUpLODLevels(this GameObject go, float maxWeight)
		{
			go.SetUpLODLevels(new float[]
			{
				0.6f,
				0.3f,
				0.15f
			}, new float[]
			{
				maxWeight * 0.65f,
				maxWeight,
				maxWeight * 1.5f
			});
		}

		
		public static void SetUpLODLevels(this GameObject go, float[] lodScreenSizes, float[] maxWeights)
		{
			MeshFilter[] componentsInChildren = go.GetComponentsInChildren<MeshFilter>(false);
			if (componentsInChildren == null || componentsInChildren.Length == 0)
			{
				return;
			}
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.SetUpLODLevelsWithLODSwitcher(lodScreenSizes, maxWeights, true, 1f, 1f, 1f, 1f, 1f, 1);
			}
		}

		
		public static Mesh[] SetUpLODLevelsWithLODSwitcher(this GameObject go, float[] lodScreenSizes, float[] maxWeights, bool recalcNormals, float removeSmallParts = 1f, float protectNormals = 1f, float protectUvs = 1f, float protectSubMeshesAndSharpEdges = 1f, float smallTrianglesFirst = 1f, int nrOfSteps = 1)
		{
			Mesh mesh = null;
			LODSwitcher lodswitcher = go.GetComponent<LODSwitcher>();
			if (lodswitcher != null)
			{
				lodswitcher.ReleaseFixedLODLevel();
				lodswitcher.SetLODLevel(0);
			}
			SkinnedMeshRenderer component = go.GetComponent<SkinnedMeshRenderer>();
			if (component != null)
			{
				mesh = component.sharedMesh;
			}
			else
			{
				MeshFilter component2 = go.GetComponent<MeshFilter>();
				if (component2 != null)
				{
					mesh = component2.sharedMesh;
				}
			}
			if (mesh == null)
			{
				throw new ApplicationException("No mesh found in " + go.name + ". Maybe you need to select a child object?");
			}
			for (int i = 0; i < maxWeights.Length; i++)
			{
				if (maxWeights[i] <= 0f)
				{
					throw new ApplicationException("MaxWeight should be more that 0 or else this operation will have no effect");
				}
			}
			Mesh[] array = mesh.MakeLODMeshes(maxWeights, recalcNormals, removeSmallParts, protectNormals, protectUvs, protectSubMeshesAndSharpEdges, smallTrianglesFirst, nrOfSteps);
			if (array == null)
			{
				return null;
			}
			if (lodswitcher == null)
			{
				lodswitcher = go.AddComponent<LODSwitcher>();
			}
			Array.Resize<Mesh>(ref array, maxWeights.Length + 1);
			for (int j = maxWeights.Length; j > 0; j--)
			{
				array[j] = array[j - 1];
			}
			array[0] = mesh;
			lodswitcher.lodMeshes = array;
			lodswitcher.lodScreenSizes = lodScreenSizes;
			lodswitcher.ComputeDimensions();
			lodswitcher.enabled = true;
			return array;
		}

		
		public static IEnumerator SetUpLODLevelsWithLODSwitcherInBackground(this GameObject go, float[] lodScreenSizes, float[] maxWeights, bool recalcNormals, float removeSmallParts = 1f, float protectNormals = 1f, float protectUvs = 1f, float protectSubMeshesAndSharpEdges = 1f, float smallTrianglesFirst = 1f)
		{
			yield return null;
			Mesh mesh = null;
			LODSwitcher lodSwitcher = go.GetComponent<LODSwitcher>();
			if (lodSwitcher != null)
			{
				lodSwitcher.ReleaseFixedLODLevel();
				lodSwitcher.SetLODLevel(0);
			}
			SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
			if (smr != null)
			{
				mesh = smr.sharedMesh;
			}
			else
			{
				MeshFilter component = go.GetComponent<MeshFilter>();
				if (component != null)
				{
					mesh = component.sharedMesh;
				}
			}
			if (mesh == null)
			{
				throw new ApplicationException("No mesh found in " + go.name + ". Maybe you need to select a child object?");
			}
			for (int j = 0; j < maxWeights.Length; j++)
			{
				if (maxWeights[j] <= 0f)
				{
					throw new ApplicationException("MaxWeight should be more that 0 or else this operation will have no effect");
				}
			}
			Mesh mesh2 = mesh;
			Mesh[] lodMeshes = new Mesh[maxWeights.Length];
			for (int i = 0; i < maxWeights.Length; i++)
			{
				yield return null;
				Hashtable lodInfo = new Hashtable();
				lodInfo["maxWeight"] = maxWeights[i];
				lodInfo["removeSmallParts"] = removeSmallParts;
				Vector3[] vs = mesh.vertices;
				if (vs.Length <= 0)
				{
					throw new ApplicationException("Mesh was empty");
				}
				Vector3[] ns = mesh.normals;
				if (ns.Length == 0)
				{
					mesh.RecalculateNormals();
					ns = mesh.normals;
				}
				Vector2[] uv1s = mesh.uv;
				Vector2[] uv2s = mesh.uv2;
				Vector2[] uv3s = mesh.uv3;
				Vector2[] uv4s = mesh.uv4;
				Color32[] colors32 = mesh.colors32;
				int[] ts = mesh.triangles;
				Matrix4x4[] bindposes = mesh.bindposes;
				BoneWeight[] bws = mesh.boneWeights;
				int[] subMeshOffsets = new int[mesh.subMeshCount];
				if (mesh.subMeshCount > 1)
				{
					for (int k = 0; k < mesh.subMeshCount; k++)
					{
						int[] triangles = mesh.GetTriangles(k);
						int l;
						for (l = 0; l < triangles.Length; l++)
						{
							ts[subMeshOffsets[k] + l] = triangles[l];
						}
						if (k + 1 < mesh.subMeshCount)
						{
							subMeshOffsets[k + 1] = subMeshOffsets[k] + l;
						}
					}
				}
				Bounds meshBounds = mesh.bounds;
				lodInfo["vertices"] = vs;
				lodInfo["normals"] = ns;
				lodInfo["uv1s"] = uv1s;
				lodInfo["uv2s"] = uv2s;
				lodInfo["uv3s"] = uv3s;
				lodInfo["uv4s"] = uv4s;
				lodInfo["colors32"] = colors32;
				lodInfo["triangles"] = ts;
				lodInfo["bindposes"] = bindposes;
				lodInfo["boneWeights"] = bws;
				lodInfo["subMeshOffsets"] = subMeshOffsets;
				lodInfo["meshBounds"] = meshBounds;
				if (GameObjectExtensions.<>f__mg$cache0 == null)
				{
					GameObjectExtensions.<>f__mg$cache0 = new ParameterizedThreadStart(LODMaker.MakeLODMeshInBackground);
				}
				Thread thread = new Thread(GameObjectExtensions.<>f__mg$cache0);
				thread.Start(lodInfo);
				while (!lodInfo.ContainsKey("ready"))
				{
					yield return new WaitForSeconds(0.2f);
				}
				lodMeshes[i] = LODMaker.CreateNewMesh((Vector3[])lodInfo["vertices"], (Vector3[])lodInfo["normals"], (Vector2[])lodInfo["uv1s"], (Vector2[])lodInfo["uv2s"], (Vector2[])lodInfo["uv3s"], (Vector2[])lodInfo["uv4s"], (Color32[])lodInfo["colors32"], (int[])lodInfo["triangles"], (BoneWeight[])lodInfo["boneWeights"], (Matrix4x4[])lodInfo["bindposes"], (int[])lodInfo["subMeshOffsets"], recalcNormals);
				mesh = lodMeshes[i];
				go.transform.parent.gameObject.BroadcastMessage("LOD" + (i + 1) + "IsReady", go, SendMessageOptions.DontRequireReceiver);
			}
			yield return null;
			if (lodMeshes != null)
			{
				if (lodSwitcher == null)
				{
					lodSwitcher = go.AddComponent<LODSwitcher>();
				}
				Array.Resize<Mesh>(ref lodMeshes, maxWeights.Length + 1);
				for (int m = maxWeights.Length; m > 0; m--)
				{
					lodMeshes[m] = lodMeshes[m - 1];
				}
				lodMeshes[0] = mesh2;
				lodSwitcher.lodMeshes = lodMeshes;
				lodSwitcher.lodScreenSizes = lodScreenSizes;
				lodSwitcher.ComputeDimensions();
				lodSwitcher.enabled = true;
			}
			go.transform.parent.gameObject.BroadcastMessage("LODsAreReady", go, SendMessageOptions.DontRequireReceiver);
			yield break;
		}

		
		public static Mesh[] SetUpLODLevelsAndChildrenWithLODSwitcher(this GameObject go, float[] lodScreenSizes, float[] maxWeights, bool recalcNormals, float removeSmallParts, float protectNormals = 1f, float protectUvs = 1f, float protectSubMeshesAndSharpEdges = 1f, float smallTrianglesFirst = 1f, int nrOfSteps = 1)
		{
			Mesh mesh = null;
			LODSwitcher lodswitcher = go.GetComponent<LODSwitcher>();
			if (lodswitcher != null)
			{
				lodswitcher.ReleaseFixedLODLevel();
				lodswitcher.SetLODLevel(0);
			}
			SkinnedMeshRenderer component = go.GetComponent<SkinnedMeshRenderer>();
			Material[] sharedMaterials;
			if (component != null)
			{
				mesh = component.sharedMesh;
				sharedMaterials = component.sharedMaterials;
				component.enabled = false;
			}
			else
			{
				MeshFilter component2 = go.GetComponent<MeshFilter>();
				if (component2 != null)
				{
					mesh = component2.sharedMesh;
				}
				MeshRenderer component3 = go.GetComponent<MeshRenderer>();
				if (component3 == null)
				{
					throw new ApplicationException("No MeshRenderer found");
				}
				sharedMaterials = component3.sharedMaterials;
				component3.enabled = false;
			}
			if (mesh == null)
			{
				throw new ApplicationException("No mesh found in " + go.name + ". Maybe you need to select a child object?");
			}
			for (int i = 0; i < maxWeights.Length; i++)
			{
				if (maxWeights[i] <= 0f)
				{
					throw new ApplicationException("MaxWeight should be more that 0 or else this operation will have no effect");
				}
			}
			Mesh[] array = mesh.MakeLODMeshes(maxWeights, recalcNormals, removeSmallParts, protectNormals, protectUvs, protectSubMeshesAndSharpEdges, smallTrianglesFirst, nrOfSteps);
			if (array == null)
			{
				return null;
			}
			Mesh[] array2 = new Mesh[array.Length + 1];
			array2[0] = mesh;
			for (int j = 0; j < array.Length; j++)
			{
				array2[j + 1] = array[j];
			}
			if (lodswitcher == null)
			{
				lodswitcher = go.AddComponent<LODSwitcher>();
			}
			lodswitcher.lodScreenSizes = lodScreenSizes;
			GameObject[] array3 = new GameObject[array2.Length];
			for (int k = 0; k < array2.Length; k++)
			{
				Transform transform = go.transform.FindFirstChildWithName(go.name + "_LOD" + k);
				if (transform != null)
				{
					array3[k] = transform.gameObject;
					array3[k].SetActive(true);
				}
				if (array3[k] == null)
				{
					array3[k] = new GameObject(go.name + "_LOD" + k);
					array3[k].transform.SetParent(go.transform);
					array3[k].transform.localPosition = Vector3.zero;
					array3[k].transform.localRotation = Quaternion.identity;
					array3[k].transform.localScale = Vector3.one;
				}
				MeshFilter meshFilter = array3[k].GetComponent<MeshFilter>();
				if (meshFilter == null)
				{
					meshFilter = array3[k].AddComponent<MeshFilter>();
				}
				meshFilter.sharedMesh = array2[k];
				MeshRenderer meshRenderer = array3[k].GetComponent<MeshRenderer>();
				if (meshRenderer == null)
				{
					meshRenderer = array3[k].AddComponent<MeshRenderer>();
				}
				meshRenderer.sharedMaterials = sharedMaterials;
				array3[k].SetActive(k == 0);
			}
			lodswitcher.lodGameObjects = array3;
			lodswitcher.ComputeDimensions();
			lodswitcher.enabled = true;
			return array2;
		}

		
		public static Mesh[] SetUpLODLevelsAndChildrenWithLODGroup(this GameObject go, float[] relativeTransitionHeights, float[] maxWeights, bool recalcNormals, float removeSmallParts, float protectNormals = 1f, float protectUvs = 1f, float protectSubMeshesAndSharpEdges = 1f, float smallTrianglesFirst = 1f, int nrOfSteps = 1)
		{
			LODGroup lodgroup = null;
			if (relativeTransitionHeights.Length < 0 || relativeTransitionHeights.Length != maxWeights.Length)
			{
				throw new ApplicationException("relativeTransitionHeights and maxWeights arrays need to have equal length and be longer than 0. Example: SetUpLODLevelsWithLODGroup(go, new float[2] {0.6f, 0.4f}, new float[2] {1f, 1.75f})");
			}
			for (int i = 0; i < maxWeights.Length; i++)
			{
				if (maxWeights[i] <= 0f)
				{
					throw new ApplicationException("MaxWeight should be more that 0 or else this operation will have no effect");
				}
			}
			GameObject gameObject = new GameObject(go.name + "_$LodGrp");
			if (go.transform.parent != null)
			{
				gameObject.transform.SetParent(go.transform.parent);
			}
			gameObject.transform.localPosition = go.transform.localPosition;
			gameObject.transform.localRotation = go.transform.localRotation;
			gameObject.transform.localScale = go.transform.localScale;
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(go);
			gameObject2.name = go.name + "_$Lod:0";
			gameObject2.transform.SetParent(gameObject.transform);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = Vector3.one;
			if (lodgroup == null)
			{
				lodgroup = gameObject.AddComponent<LODGroup>();
			}
			else
			{
				Transform transform = gameObject.transform.FindFirstChildWhereNameContains(go.name + "_$Lod:");
				int num = 0;
				while (transform != null && num++ < 10)
				{
					transform.SetParent(null);
					UnityEngine.Object.Destroy(transform.gameObject);
					transform = gameObject.transform.FindFirstChildWhereNameContains(go.name + "_$Lod:");
				}
			}
			LOD[] array = new LOD[maxWeights.Length + 1];
			array[0] = new LOD(relativeTransitionHeights[0], gameObject2.GetComponentsInChildren<MeshRenderer>(false));
			List<Mesh> list = new List<Mesh>();
			Mesh[] array2 = go.GetMeshes(false);
			for (int j = 0; j < array2.Length; j++)
			{
				list.Add(array2[j]);
			}
			float num2 = 0f;
			for (int k = 1; k < array.Length; k++)
			{
				Mesh[] array3 = new Mesh[array2.Length];
				for (int l = 0; l < array2.Length; l++)
				{
					Mesh mesh = array2[l];
					if (nrOfSteps < 1)
					{
						nrOfSteps = 1;
					}
					for (int m = 0; m < nrOfSteps; m++)
					{
						float num3 = maxWeights[k - 1] - num2;
						mesh = mesh.MakeLODMesh((float)(m + 1) * (num3 / (float)nrOfSteps) + num2, recalcNormals, removeSmallParts, protectNormals, protectUvs, protectSubMeshesAndSharpEdges, smallTrianglesFirst);
					}
					num2 = maxWeights[k - 1];
					array3[l] = mesh;
					mesh.name = string.Concat(new object[]
					{
						go.name,
						"_",
						l,
						"_LOD",
						k
					});
					list.Add(mesh);
				}
				GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(go);
				gameObject3.name = go.name + "_$Lod:" + k;
				gameObject3.transform.SetParent(gameObject.transform);
				gameObject3.transform.localPosition = Vector3.zero;
				gameObject3.transform.localRotation = Quaternion.identity;
				gameObject3.transform.localScale = new Vector3(1f, 1f, 1f);
				gameObject3.SetMeshes(array3);
				float screenRelativeTransitionHeight = (k >= relativeTransitionHeights.Length) ? 0f : relativeTransitionHeights[k];
				array[k] = new LOD(screenRelativeTransitionHeight, gameObject3.GetComponentsInChildren<MeshRenderer>(false));
				array2 = array3;
			}
			lodgroup.SetLODs(array);
			lodgroup.RecalculateBounds();
			lodgroup.ForceLOD(-1);
			go.SetActive(false);
			return list.ToArray();
		}

		
		public static Mesh GetSimplifiedMesh(this GameObject go, float maxWeight, bool recalcNormals, float removeSmallParts, float protectNormals = 1f, float protectUvs = 1f, float protectSubMeshesAndSharpEdges = 1f, float smallTrianglesFirst = 1f, int nrOfSteps = 1)
		{
			Mesh mesh = null;
			LODSwitcher component = go.GetComponent<LODSwitcher>();
			if (component != null)
			{
				component.ReleaseFixedLODLevel();
				component.SetLODLevel(0);
			}
			MeshFilter meshFilter = null;
			SkinnedMeshRenderer component2 = go.GetComponent<SkinnedMeshRenderer>();
			if (maxWeight <= 0f)
			{
				throw new ApplicationException("MaxWeight should be more that 0 or else this operation will have no effect");
			}
			if (component2 != null)
			{
				mesh = component2.sharedMesh;
			}
			else
			{
				meshFilter = go.GetComponent<MeshFilter>();
				if (meshFilter != null)
				{
					mesh = meshFilter.sharedMesh;
				}
			}
			if (mesh == null)
			{
				throw new ApplicationException("No mesh found. Maybe you need to select a child object?");
			}
			Mesh mesh2 = mesh;
			if (nrOfSteps < 1)
			{
				nrOfSteps = 1;
			}
			for (int i = 0; i < nrOfSteps; i++)
			{
				mesh2 = mesh2.MakeLODMesh((float)(i + 1) * (maxWeight / (float)nrOfSteps), recalcNormals, removeSmallParts, protectNormals, protectUvs, protectSubMeshesAndSharpEdges, smallTrianglesFirst);
			}
			if (component2 != null)
			{
				component2.sharedMesh = mesh2;
			}
			else if (meshFilter != null)
			{
				meshFilter.sharedMesh = mesh2;
			}
			return mesh2;
		}

		
		public static IEnumerator GetSimplifiedMeshInBackground(this GameObject go, float maxWeight, bool recalcNormals, float removeSmallParts, Action<Mesh> result)
		{
			Mesh mesh = null;
			LODSwitcher lodSwitcher = go.GetComponent<LODSwitcher>();
			if (lodSwitcher != null)
			{
				lodSwitcher.ReleaseFixedLODLevel();
				lodSwitcher.SetLODLevel(0);
			}
			SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
			if (maxWeight <= 0f)
			{
				throw new ApplicationException("MaxWeight should be more that 0 or else this operation will have no effect");
			}
			if (smr != null)
			{
				mesh = smr.sharedMesh;
			}
			else
			{
				MeshFilter component = go.GetComponent<MeshFilter>();
				if (component != null)
				{
					mesh = component.sharedMesh;
				}
			}
			if (mesh == null)
			{
				throw new ApplicationException("No mesh found. Maybe you need to select a child object?");
			}
			Hashtable lodInfo = new Hashtable();
			lodInfo["maxWeight"] = maxWeight;
			lodInfo["removeSmallParts"] = removeSmallParts;
			Vector3[] vs = mesh.vertices;
			if (vs.Length <= 0)
			{
				throw new ApplicationException("Mesh was empty");
			}
			Vector3[] ns = mesh.normals;
			if (ns.Length == 0)
			{
				mesh.RecalculateNormals();
				ns = mesh.normals;
			}
			Vector2[] uv1s = mesh.uv;
			Vector2[] uv2s = mesh.uv2;
			Vector2[] uv3s = mesh.uv3;
			Vector2[] uv4s = mesh.uv4;
			Color32[] colors32 = mesh.colors32;
			int[] ts = mesh.triangles;
			Matrix4x4[] bindposes = mesh.bindposes;
			BoneWeight[] bws = mesh.boneWeights;
			int[] subMeshOffsets = new int[mesh.subMeshCount];
			if (mesh.subMeshCount > 1)
			{
				for (int i = 0; i < mesh.subMeshCount; i++)
				{
					int[] triangles = mesh.GetTriangles(i);
					int j;
					for (j = 0; j < triangles.Length; j++)
					{
						ts[subMeshOffsets[i] + j] = triangles[j];
					}
					if (i + 1 < mesh.subMeshCount)
					{
						subMeshOffsets[i + 1] = subMeshOffsets[i] + j;
					}
				}
			}
			Bounds meshBounds = mesh.bounds;
			lodInfo["vertices"] = vs;
			lodInfo["normals"] = ns;
			lodInfo["uv1s"] = uv1s;
			lodInfo["uv2s"] = uv2s;
			lodInfo["uv3s"] = uv3s;
			lodInfo["uv4s"] = uv4s;
			lodInfo["colors32"] = colors32;
			lodInfo["triangles"] = ts;
			lodInfo["bindposes"] = bindposes;
			lodInfo["boneWeights"] = bws;
			lodInfo["subMeshOffsets"] = subMeshOffsets;
			lodInfo["meshBounds"] = meshBounds;
			if (GameObjectExtensions.<>f__mg$cache1 == null)
			{
				GameObjectExtensions.<>f__mg$cache1 = new ParameterizedThreadStart(LODMaker.MakeLODMeshInBackground);
			}
			Thread thread = new Thread(GameObjectExtensions.<>f__mg$cache1);
			thread.Start(lodInfo);
			while (!lodInfo.ContainsKey("ready"))
			{
				yield return new WaitForSeconds(0.2f);
			}
			result(LODMaker.CreateNewMesh((Vector3[])lodInfo["vertices"], (Vector3[])lodInfo["normals"], (Vector2[])lodInfo["uv1s"], (Vector2[])lodInfo["uv2s"], (Vector2[])lodInfo["uv3s"], (Vector2[])lodInfo["uv4s"], (Color32[])lodInfo["colors32"], (int[])lodInfo["triangles"], (BoneWeight[])lodInfo["boneWeights"], (Matrix4x4[])lodInfo["bindposes"], (int[])lodInfo["subMeshOffsets"], recalcNormals));
			yield break;
		}

		
		private static bool MergeMeshInto(Mesh fromMesh, Transform[] fromBones, Material[] fromMaterials, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv1s, List<Vector2> uv2s, List<Vector2> uv3s, List<Vector2> uv4s, List<Color32> colors32, List<BoneWeight> boneWeights, List<Transform> bones, List<Matrix4x4> bindposes, Dictionary<Material, List<int>> subMeshes, bool usesNegativeScale, Vector4 lightmapScaleOffset, Transform fromTransform, Transform topTransform, string submeshName, string[] skipSubmeshNames)
		{
			if (fromMesh == null)
			{
				return false;
			}
			bool result = true;
			int count = vertices.Count;
			Vector3[] vertices2 = fromMesh.vertices;
			Vector3[] normals2 = fromMesh.normals;
			Vector2[] array = fromMesh.uv;
			Vector2[] array2 = fromMesh.uv2;
			Vector2[] array3 = fromMesh.uv3;
			Vector2[] array4 = fromMesh.uv4;
			Color32[] array5 = fromMesh.colors32;
			BoneWeight[] array6 = fromMesh.boneWeights;
			Matrix4x4[] array7 = fromMesh.bindposes;
			List<int> list = new List<int>();
			Vector3 localPosition = fromTransform.localPosition;
			Quaternion localRotation = fromTransform.localRotation;
			Vector3 localScale = fromTransform.localScale;
			bool flag = false;
			if (fromBones != null)
			{
				fromTransform.localPosition = Vector3.zero;
				fromTransform.localRotation = Quaternion.identity;
				fromTransform.localScale = Vector3.one;
			}
			if (fromBones == null || fromBones.Length == 0)
			{
				flag = true;
			}
			if ((fromBones == null || fromBones.Length == 0) && bones != null && bones.Count > 0)
			{
				fromBones = new Transform[]
				{
					fromTransform
				};
				Matrix4x4 matrix4x = fromTransform.worldToLocalMatrix * topTransform.localToWorldMatrix;
				array7 = new Matrix4x4[]
				{
					matrix4x
				};
				array6 = new BoneWeight[vertices2.Length];
				for (int i = 0; i < vertices2.Length; i++)
				{
					array6[i].boneIndex0 = 0;
					array6[i].weight0 = 1f;
				}
			}
			if (fromBones != null)
			{
				bool flag2 = false;
				for (int j = 0; j < fromBones.Length; j++)
				{
					int k = 0;
					list.Add(j);
					while (k < bones.Count)
					{
						if (fromBones[j] == bones[k])
						{
							list[j] = k;
							if (array7[j] != bindposes[k])
							{
								flag2 = true;
								if (fromBones[j] != null)
								{
									Debug.Log(fromTransform.gameObject.name + ": The bindpose of " + fromBones[j].gameObject.name + " is different, vertices will be moved to match the bindpose of the merged mesh");
								}
								else
								{
									Debug.LogError(fromTransform.gameObject.name + ": There is an error in the bonestructure. A bone could not be found.");
								}
							}
						}
						k++;
					}
					if (k >= bones.Count)
					{
						list[j] = bones.Count;
						bones.Add(fromBones[j]);
						bindposes.Add(array7[j]);
					}
				}
				if (flag2)
				{
					for (int l = 0; l < vertices2.Length; l++)
					{
						Vector3 vector = vertices2[l];
						BoneWeight boneWeight = array6[l];
						if (fromBones[boneWeight.boneIndex0] != null)
						{
							vector = GameObjectExtensions.ApplyBindPose(vertices2[l], fromBones[boneWeight.boneIndex0], array7[boneWeight.boneIndex0], boneWeight.weight0);
							if (boneWeight.weight1 > 0f)
							{
								vector += GameObjectExtensions.ApplyBindPose(vertices2[l], fromBones[boneWeight.boneIndex1], array7[boneWeight.boneIndex1], boneWeight.weight1);
							}
							if (boneWeight.weight2 > 0f)
							{
								vector += GameObjectExtensions.ApplyBindPose(vertices2[l], fromBones[boneWeight.boneIndex2], array7[boneWeight.boneIndex2], boneWeight.weight2);
							}
							if (boneWeight.weight3 > 0f)
							{
								vector += GameObjectExtensions.ApplyBindPose(vertices2[l], fromBones[boneWeight.boneIndex3], array7[boneWeight.boneIndex3], boneWeight.weight3);
							}
							Vector3 vertex = vector;
							vector = GameObjectExtensions.UnApplyBindPose(vertex, bones[list[boneWeight.boneIndex0]], bindposes[list[boneWeight.boneIndex0]], boneWeight.weight0);
							if (boneWeight.weight1 > 0f)
							{
								vector += GameObjectExtensions.UnApplyBindPose(vertex, bones[list[boneWeight.boneIndex1]], bindposes[list[boneWeight.boneIndex1]], boneWeight.weight1);
							}
							if (boneWeight.weight2 > 0f)
							{
								vector += GameObjectExtensions.UnApplyBindPose(vertex, bones[list[boneWeight.boneIndex2]], bindposes[list[boneWeight.boneIndex2]], boneWeight.weight2);
							}
							if (boneWeight.weight3 > 0f)
							{
								vector += GameObjectExtensions.UnApplyBindPose(vertex, bones[list[boneWeight.boneIndex3]], bindposes[list[boneWeight.boneIndex3]], boneWeight.weight3);
							}
							vertices2[l] = vector;
						}
					}
				}
			}
			if (boneWeights != null && array6 != null && array6.Length > 0)
			{
				for (int m = 0; m < array6.Length; m++)
				{
					boneWeights.Add(new BoneWeight
					{
						boneIndex0 = list[array6[m].boneIndex0],
						boneIndex1 = list[array6[m].boneIndex1],
						boneIndex2 = list[array6[m].boneIndex2],
						boneIndex3 = list[array6[m].boneIndex3],
						weight0 = array6[m].weight0,
						weight1 = array6[m].weight1,
						weight2 = array6[m].weight2,
						weight3 = array6[m].weight3
					});
				}
			}
			Matrix4x4 matrix4x2 = topTransform.worldToLocalMatrix * fromTransform.localToWorldMatrix;
			if (flag)
			{
				for (int n = 0; n < vertices2.Length; n++)
				{
					Vector3 v = vertices2[n];
					vertices2[n] = matrix4x2.MultiplyPoint3x4(v);
				}
			}
			vertices.AddRange(vertices2);
			Quaternion rotation = Quaternion.LookRotation(matrix4x2.GetColumn(2), matrix4x2.GetColumn(1));
			if (normals2 != null && normals2.Length > 0)
			{
				for (int num = 0; num < normals2.Length; num++)
				{
					normals2[num] = rotation * normals2[num];
				}
				normals.AddRange(normals2);
			}
			if (array == null || array.Length != vertices2.Length)
			{
				array = new Vector2[vertices2.Length];
			}
			if (array != null && array.Length > 0)
			{
				uv1s.AddRange(array);
			}
			if (array2 == null || array2.Length != vertices2.Length)
			{
				array2 = new Vector2[vertices2.Length];
			}
			int num2 = 0;
			while (array2 != null && num2 < array2.Length)
			{
				uv2s.Add(new Vector2(lightmapScaleOffset.z + array2[num2].x * lightmapScaleOffset.x, lightmapScaleOffset.w + array2[num2].y * lightmapScaleOffset.y));
				num2++;
			}
			if (array3 == null || array3.Length != vertices2.Length)
			{
				array3 = new Vector2[vertices2.Length];
			}
			if (array3 != null && array3.Length > 0)
			{
				uv3s.AddRange(array3);
			}
			if (array4 == null || array4.Length != vertices2.Length)
			{
				array4 = new Vector2[vertices2.Length];
			}
			if (array4 != null && array4.Length > 0)
			{
				uv4s.AddRange(array3);
			}
			if (array5 == null || array5.Length != vertices2.Length)
			{
				array5 = new Color32[vertices2.Length];
			}
			if (array5 != null && array5.Length > 0)
			{
				colors32.AddRange(array5);
			}
			int num3 = 0;
			for (int num4 = 0; num4 < fromMaterials.Length; num4++)
			{
				if (num4 < fromMesh.subMeshCount)
				{
					string a = submeshName + "_" + num4;
					int num5;
					for (num5 = 0; num5 < skipSubmeshNames.Length; num5++)
					{
						if (a == skipSubmeshNames[num5])
						{
							break;
						}
					}
					if (num5 >= skipSubmeshNames.Length)
					{
						int[] triangles = fromMesh.GetTriangles(num4);
						if (triangles.Length > 0)
						{
							if (fromMaterials[num4] != null && !subMeshes.ContainsKey(fromMaterials[num4]))
							{
								subMeshes.Add(fromMaterials[num4], new List<int>());
							}
							List<int> list2 = subMeshes[fromMaterials[num4]];
							for (int num6 = 0; num6 < triangles.Length; num6 += 3)
							{
								if (usesNegativeScale)
								{
									int num7 = triangles[num6 + 1];
									int num8 = triangles[num6 + 2];
									triangles[num6 + 1] = num8;
									triangles[num6 + 2] = num7;
									num3++;
								}
								triangles[num6] += count;
								triangles[num6 + 1] += count;
								triangles[num6 + 2] += count;
							}
							list2.AddRange(triangles);
						}
					}
					else
					{
						result = false;
					}
				}
			}
			if (fromBones != null)
			{
				fromTransform.localPosition = localPosition;
				fromTransform.localRotation = localRotation;
				fromTransform.localScale = localScale;
			}
			return result;
		}

		
		private static Vector3 ApplyBindPose(Vector3 vertex, Transform bone, Matrix4x4 bindpose, float boneWeight)
		{
			Matrix4x4 matrix4x = bone.localToWorldMatrix * bindpose;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int row;
					int column;
					matrix4x[row = i, column = j] = matrix4x[row, column] * boneWeight;
				}
			}
			return matrix4x.MultiplyPoint3x4(vertex);
		}

		
		private static Vector3 UnApplyBindPose(Vector3 vertex, Transform bone, Matrix4x4 bindpose, float boneWeight)
		{
			Matrix4x4 inverse = (bone.localToWorldMatrix * bindpose).inverse;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int row;
					int column;
					inverse[row = i, column = j] = inverse[row, column] * boneWeight;
				}
			}
			return inverse.MultiplyPoint3x4(vertex);
		}

		
		[CompilerGenerated]
		private static ParameterizedThreadStart <>f__mg$cache0;

		
		[CompilerGenerated]
		private static ParameterizedThreadStart <>f__mg$cache1;
	}
}
