﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Valve.VR.InteractionSystem
{
	
	public static class Util
	{
		
		public static float RemapNumber(float num, float low1, float high1, float low2, float high2)
		{
			return low2 + (num - low1) * (high2 - low2) / (high1 - low1);
		}

		
		public static float RemapNumberClamped(float num, float low1, float high1, float low2, float high2)
		{
			return Mathf.Clamp(Util.RemapNumber(num, low1, high1, low2, high2), Mathf.Min(low2, high2), Mathf.Max(low2, high2));
		}

		
		public static float Approach(float target, float value, float speed)
		{
			float num = target - value;
			if (num > speed)
			{
				value += speed;
			}
			else if (num < -speed)
			{
				value -= speed;
			}
			else
			{
				value = target;
			}
			return value;
		}

		
		public static Vector3 BezierInterpolate3(Vector3 p0, Vector3 c0, Vector3 p1, float t)
		{
			Vector3 a = Vector3.Lerp(p0, c0, t);
			Vector3 b = Vector3.Lerp(c0, p1, t);
			return Vector3.Lerp(a, b, t);
		}

		
		public static Vector3 BezierInterpolate4(Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p1, float t)
		{
			Vector3 a = Vector3.Lerp(p0, c0, t);
			Vector3 vector = Vector3.Lerp(c0, c1, t);
			Vector3 b = Vector3.Lerp(c1, p1, t);
			Vector3 a2 = Vector3.Lerp(a, vector, t);
			Vector3 b2 = Vector3.Lerp(vector, b, t);
			return Vector3.Lerp(a2, b2, t);
		}

		
		public static Vector3 Vector3FromString(string szString)
		{
			string[] array = szString.Substring(1, szString.Length - 1).Split(new char[]
			{
				','
			});
			float x = float.Parse(array[0]);
			float y = float.Parse(array[1]);
			float z = float.Parse(array[2]);
			Vector3 result = new Vector3(x, y, z);
			return result;
		}

		
		public static Vector2 Vector2FromString(string szString)
		{
			string[] array = szString.Substring(1, szString.Length - 1).Split(new char[]
			{
				','
			});
			float x = float.Parse(array[0]);
			float y = float.Parse(array[1]);
			Vector3 v = new Vector2(x, y);
			return v;
		}

		
		public static float Normalize(float value, float min, float max)
		{
			return (value - min) / (max - min);
		}

		
		public static Vector3 Vector2AsVector3(Vector2 v)
		{
			return new Vector3(v.x, 0f, v.y);
		}

		
		public static Vector2 Vector3AsVector2(Vector3 v)
		{
			return new Vector2(v.x, v.z);
		}

		
		public static float AngleOf(Vector2 v)
		{
			float magnitude = v.magnitude;
			if (v.y >= 0f)
			{
				return Mathf.Acos(v.x / magnitude);
			}
			return Mathf.Acos(-v.x / magnitude) + 3.14159274f;
		}

		
		public static float YawOf(Vector3 v)
		{
			float magnitude = v.magnitude;
			if (v.z >= 0f)
			{
				return Mathf.Acos(v.x / magnitude);
			}
			return Mathf.Acos(-v.x / magnitude) + 3.14159274f;
		}

		
		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T t = lhs;
			lhs = rhs;
			rhs = t;
		}

		
		public static void Shuffle<T>(T[] array)
		{
			for (int i = array.Length - 1; i > 0; i--)
			{
				int num = UnityEngine.Random.Range(0, i);
				Util.Swap<T>(ref array[i], ref array[num]);
			}
		}

		
		public static void Shuffle<T>(List<T> list)
		{
			for (int i = list.Count - 1; i > 0; i--)
			{
				int index = UnityEngine.Random.Range(0, i);
				T value = list[i];
				list[i] = list[index];
				list[index] = value;
			}
		}

		
		public static int RandomWithLookback(int min, int max, List<int> history, int historyCount)
		{
			int num = UnityEngine.Random.Range(min, max - history.Count);
			for (int i = 0; i < history.Count; i++)
			{
				if (num >= history[i])
				{
					num++;
				}
			}
			history.Add(num);
			if (history.Count > historyCount)
			{
				history.RemoveRange(0, history.Count - historyCount);
			}
			return num;
		}

		
		public static Transform FindChild(Transform parent, string name)
		{
			if (parent.name == name)
			{
				return parent;
			}
			IEnumerator enumerator = parent.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform parent2 = (Transform)obj;
					Transform transform = Util.FindChild(parent2, name);
					if (transform != null)
					{
						return transform;
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
			return null;
		}

		
		public static bool IsNullOrEmpty<T>(T[] array)
		{
			return array == null || array.Length == 0;
		}

		
		public static bool IsValidIndex<T>(T[] array, int i)
		{
			return array != null && i >= 0 && i < array.Length;
		}

		
		public static bool IsValidIndex<T>(List<T> list, int i)
		{
			return list != null && list.Count != 0 && i >= 0 && i < list.Count;
		}

		
		public static int FindOrAdd<T>(List<T> list, T item)
		{
			int num = list.IndexOf(item);
			if (num == -1)
			{
				list.Add(item);
				num = list.Count - 1;
			}
			return num;
		}

		
		public static List<T> FindAndRemove<T>(List<T> list, Predicate<T> match)
		{
			List<T> result = list.FindAll(match);
			list.RemoveAll(match);
			return result;
		}

		
		public static T FindOrAddComponent<T>(GameObject gameObject) where T : Component
		{
			T component = gameObject.GetComponent<T>();
			if (component)
			{
				return component;
			}
			return gameObject.AddComponent<T>();
		}

		
		public static void FastRemove<T>(List<T> list, int index)
		{
			list[index] = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
		}

		
		public static void ReplaceGameObject<T, U>(T replace, U replaceWith) where T : MonoBehaviour where U : MonoBehaviour
		{
			replace.gameObject.SetActive(false);
			replaceWith.gameObject.SetActive(true);
		}

		
		public static void SwitchLayerRecursively(Transform transform, int fromLayer, int toLayer)
		{
			if (transform.gameObject.layer == fromLayer)
			{
				transform.gameObject.layer = toLayer;
			}
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Util.SwitchLayerRecursively(transform.GetChild(i), fromLayer, toLayer);
			}
		}

		
		public static void DrawCross(Vector3 origin, Color crossColor, float size)
		{
			Vector3 start = origin + Vector3.right * size;
			Vector3 end = origin - Vector3.right * size;
			UnityEngine.Debug.DrawLine(start, end, crossColor);
			Vector3 start2 = origin + Vector3.up * size;
			Vector3 end2 = origin - Vector3.up * size;
			UnityEngine.Debug.DrawLine(start2, end2, crossColor);
			Vector3 start3 = origin + Vector3.forward * size;
			Vector3 end3 = origin - Vector3.forward * size;
			UnityEngine.Debug.DrawLine(start3, end3, crossColor);
		}

		
		public static void ResetTransform(Transform t, bool resetScale = true)
		{
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			if (resetScale)
			{
				t.localScale = new Vector3(1f, 1f, 1f);
			}
		}

		
		public static Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
		{
			Vector3 rhs = vPoint - vA;
			Vector3 normalized = (vB - vA).normalized;
			float num = Vector3.Distance(vA, vB);
			float num2 = Vector3.Dot(normalized, rhs);
			if (num2 <= 0f)
			{
				return vA;
			}
			if (num2 >= num)
			{
				return vB;
			}
			Vector3 b = normalized * num2;
			return vA + b;
		}

		
		public static void AfterTimer(GameObject go, float _time, Action callback, bool trigger_if_destroyed_early = false)
		{
			AfterTimer_Component afterTimer_Component = go.AddComponent<AfterTimer_Component>();
			afterTimer_Component.Init(_time, callback, trigger_if_destroyed_early);
		}

		
		public static void SendPhysicsMessage(Collider collider, string message, SendMessageOptions sendMessageOptions)
		{
			Rigidbody attachedRigidbody = collider.attachedRigidbody;
			if (attachedRigidbody && attachedRigidbody.gameObject != collider.gameObject)
			{
				attachedRigidbody.SendMessage(message, sendMessageOptions);
			}
			collider.SendMessage(message, sendMessageOptions);
		}

		
		public static void SendPhysicsMessage(Collider collider, string message, object arg, SendMessageOptions sendMessageOptions)
		{
			Rigidbody attachedRigidbody = collider.attachedRigidbody;
			if (attachedRigidbody && attachedRigidbody.gameObject != collider.gameObject)
			{
				attachedRigidbody.SendMessage(message, arg, sendMessageOptions);
			}
			collider.SendMessage(message, arg, sendMessageOptions);
		}

		
		public static void IgnoreCollisions(GameObject goA, GameObject goB)
		{
			Collider[] componentsInChildren = goA.GetComponentsInChildren<Collider>();
			Collider[] componentsInChildren2 = goB.GetComponentsInChildren<Collider>();
			if (componentsInChildren.Length == 0 || componentsInChildren2.Length == 0)
			{
				return;
			}
			foreach (Collider collider in componentsInChildren)
			{
				foreach (Collider collider2 in componentsInChildren2)
				{
					if (collider.enabled && collider2.enabled)
					{
						Physics.IgnoreCollision(collider, collider2, true);
					}
				}
			}
		}

		
		public static IEnumerator WrapCoroutine(IEnumerator coroutine, Action onCoroutineFinished)
		{
			while (coroutine.MoveNext())
			{
				object obj = coroutine.Current;
				yield return obj;
			}
			onCoroutineFinished();
			yield break;
		}

		
		public static Color ColorWithAlpha(this Color color, float alpha)
		{
			color.a = alpha;
			return color;
		}

		
		public static void Quit()
		{
			Process.GetCurrentProcess().Kill();
		}

		
		public static decimal FloatToDecimal(float value, int decimalPlaces = 2)
		{
			return Math.Round((decimal)value, decimalPlaces);
		}

		
		public static T Median<T>(this IEnumerable<T> source)
		{
			if (source == null)
			{
				throw new ArgumentException("Argument cannot be null.", "source");
			}
			int num = source.Count<T>();
			if (num == 0)
			{
				throw new InvalidOperationException("Enumerable must contain at least one element.");
			}
			return (from x in source
			orderby x
			select x).ElementAt(num / 2);
		}

		
		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			if (source == null)
			{
				throw new ArgumentException("Argument cannot be null.", "source");
			}
			foreach (T obj in source)
			{
				action(obj);
			}
		}

		
		public static string FixupNewlines(string text)
		{
			bool flag = true;
			while (flag)
			{
				int num = text.IndexOf("\\n");
				if (num == -1)
				{
					flag = false;
				}
				else
				{
					text = text.Remove(num - 1, 3);
					text = text.Insert(num - 1, "\n");
				}
			}
			return text;
		}

		
		public static float PathLength(NavMeshPath path)
		{
			if (path.corners.Length < 2)
			{
				return 0f;
			}
			Vector3 a = path.corners[0];
			float num = 0f;
			for (int i = 1; i < path.corners.Length; i++)
			{
				Vector3 vector = path.corners[i];
				num += Vector3.Distance(a, vector);
				a = vector;
			}
			return num;
		}

		
		public static bool HasCommandLineArgument(string argumentName)
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (commandLineArgs[i].Equals(argumentName))
				{
					return true;
				}
			}
			return false;
		}

		
		public static int GetCommandLineArgValue(string argumentName, int nDefaultValue)
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			int i = 0;
			while (i < commandLineArgs.Length)
			{
				if (commandLineArgs[i].Equals(argumentName))
				{
					if (i == commandLineArgs.Length - 1)
					{
						return nDefaultValue;
					}
					return int.Parse(commandLineArgs[i + 1]);
				}
				else
				{
					i++;
				}
			}
			return nDefaultValue;
		}

		
		public static float GetCommandLineArgValue(string argumentName, float flDefaultValue)
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			int i = 0;
			while (i < commandLineArgs.Length)
			{
				if (commandLineArgs[i].Equals(argumentName))
				{
					if (i == commandLineArgs.Length - 1)
					{
						return flDefaultValue;
					}
					return (float)double.Parse(commandLineArgs[i + 1]);
				}
				else
				{
					i++;
				}
			}
			return flDefaultValue;
		}

		
		public static void SetActive(GameObject gameObject, bool active)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(active);
			}
		}

		
		public static string CombinePaths(params string[] paths)
		{
			if (paths.Length == 0)
			{
				return string.Empty;
			}
			string text = paths[0];
			for (int i = 1; i < paths.Length; i++)
			{
				text = Path.Combine(text, paths[i]);
			}
			return text;
		}

		
		public const float FeetToMeters = 0.3048f;

		
		public const float FeetToCentimeters = 30.48f;

		
		public const float InchesToMeters = 0.0254f;

		
		public const float InchesToCentimeters = 2.54f;

		
		public const float MetersToFeet = 3.28084f;

		
		public const float MetersToInches = 39.3701f;

		
		public const float CentimetersToFeet = 0.0328084f;

		
		public const float CentimetersToInches = 0.393701f;

		
		public const float KilometersToMiles = 0.621371f;

		
		public const float MilesToKilometers = 1.60934f;
	}
}
