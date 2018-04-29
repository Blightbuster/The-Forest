﻿using System;
using System.Collections.Generic;
using Pathfinding.RVO;
using Pathfinding.RVO.Sampled;
using UnityEngine;

namespace Pathfinding.Examples
{
	
	[HelpURL("http:
	public class GroupController : MonoBehaviour
	{
		
		public void Start()
		{
			this.cam = Camera.main;
			RVOSimulator rvosimulator = UnityEngine.Object.FindObjectOfType(typeof(RVOSimulator)) as RVOSimulator;
			if (rvosimulator == null)
			{
				base.enabled = false;
				throw new Exception("No RVOSimulator in the scene. Please add one");
			}
			this.sim = rvosimulator.GetSimulator();
		}

		
		public void Update()
		{
			if (Screen.fullScreen && Screen.width != Screen.resolutions[Screen.resolutions.Length - 1].width)
			{
				Screen.SetResolution(Screen.resolutions[Screen.resolutions.Length - 1].width, Screen.resolutions[Screen.resolutions.Length - 1].height, true);
			}
			if (this.adjustCamera)
			{
				List<Agent> agents = this.sim.GetAgents();
				float num = 0f;
				for (int i = 0; i < agents.Count; i++)
				{
					float num2 = Mathf.Max(Mathf.Abs(agents[i].Position.x), Mathf.Abs(agents[i].Position.y));
					if (num2 > num)
					{
						num = num2;
					}
				}
				float a = num / Mathf.Tan(this.cam.fieldOfView * 0.0174532924f / 2f);
				float b = num / Mathf.Tan(Mathf.Atan(Mathf.Tan(this.cam.fieldOfView * 0.0174532924f / 2f) * this.cam.aspect));
				float num3 = Mathf.Max(a, b) * 1.1f;
				num3 = Mathf.Max(num3, 20f);
				this.cam.transform.position = Vector3.Lerp(this.cam.transform.position, new Vector3(0f, num3, 0f), Time.smoothDeltaTime * 2f);
			}
			if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.Mouse0))
			{
				this.Order();
			}
		}

		
		private void OnGUI()
		{
			if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && !Input.GetKey(KeyCode.A))
			{
				this.Select(this.start, this.end);
				this.wasDown = false;
			}
			if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
			{
				this.end = Event.current.mousePosition;
				if (!this.wasDown)
				{
					this.start = this.end;
					this.wasDown = true;
				}
			}
			if (Input.GetKey(KeyCode.A))
			{
				this.wasDown = false;
			}
			if (this.wasDown)
			{
				Rect position = Rect.MinMaxRect(Mathf.Min(this.start.x, this.end.x), Mathf.Min(this.start.y, this.end.y), Mathf.Max(this.start.x, this.end.x), Mathf.Max(this.start.y, this.end.y));
				if (position.width > 4f && position.height > 4f)
				{
					GUI.Box(position, string.Empty, this.selectionBox);
				}
			}
		}

		
		public void Order()
		{
			Ray ray = this.cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit raycastHit;
			if (Physics.Raycast(ray, out raycastHit))
			{
				float num = 0f;
				for (int i = 0; i < this.selection.Count; i++)
				{
					num += this.selection[i].GetComponent<RVOController>().radius;
				}
				float num2 = num / 3.14159274f;
				num2 *= 2f;
				for (int j = 0; j < this.selection.Count; j++)
				{
					float num3 = 6.28318548f * (float)j / (float)this.selection.Count;
					Vector3 target = raycastHit.point + new Vector3(Mathf.Cos(num3), 0f, Mathf.Sin(num3)) * num2;
					this.selection[j].SetTarget(target);
					this.selection[j].SetColor(this.GetColor(num3));
					this.selection[j].RecalculatePath();
				}
			}
		}

		
		public void Select(Vector2 _start, Vector2 _end)
		{
			_start.y = (float)Screen.height - _start.y;
			_end.y = (float)Screen.height - _end.y;
			Vector2 b = Vector2.Min(_start, _end);
			Vector2 a = Vector2.Max(_start, _end);
			if ((a - b).sqrMagnitude < 16f)
			{
				return;
			}
			this.selection.Clear();
			RVOExampleAgent[] array = UnityEngine.Object.FindObjectsOfType(typeof(RVOExampleAgent)) as RVOExampleAgent[];
			for (int i = 0; i < array.Length; i++)
			{
				Vector2 vector = this.cam.WorldToScreenPoint(array[i].transform.position);
				if (vector.x > b.x && vector.y > b.y && vector.x < a.x && vector.y < a.y)
				{
					this.selection.Add(array[i]);
				}
			}
		}

		
		public Color GetColor(float angle)
		{
			return AstarMath.HSVToRGB(angle * 57.2957764f, 0.8f, 0.6f);
		}

		
		private const float rad2Deg = 57.2957764f;

		
		public GUIStyle selectionBox;

		
		public bool adjustCamera = true;

		
		private Vector2 start;

		
		private Vector2 end;

		
		private bool wasDown;

		
		private List<RVOExampleAgent> selection = new List<RVOExampleAgent>();

		
		private Simulator sim;

		
		private Camera cam;
	}
}
