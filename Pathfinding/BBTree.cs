using System;
using System.Diagnostics;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	
	public class BBTree : IAstarPooledObject
	{
		
		
		public Rect Size
		{
			get
			{
				if (this.count == 0)
				{
					return new Rect(0f, 0f, 0f, 0f);
				}
				IntRect rect = this.arr[0].rect;
				return Rect.MinMaxRect((float)rect.xmin * 0.001f, (float)rect.ymin * 0.001f, (float)rect.xmax * 0.001f, (float)rect.ymax * 0.001f);
			}
		}

		
		public void Clear()
		{
			this.count = 0;
		}

		
		public void OnEnterPool()
		{
			for (int i = 0; i < this.arr.Length; i++)
			{
				this.arr[i].node = null;
			}
			this.Clear();
		}

		
		private void EnsureCapacity(int c)
		{
			if (this.arr.Length < c)
			{
				BBTree.BBTreeBox[] array = new BBTree.BBTreeBox[Math.Max(c, (int)((float)this.arr.Length * 2f))];
				for (int i = 0; i < this.count; i++)
				{
					array[i] = this.arr[i];
				}
				this.arr = array;
			}
		}

		
		private int GetBox(MeshNode node)
		{
			if (this.count >= this.arr.Length)
			{
				this.EnsureCapacity(this.count + 1);
			}
			this.arr[this.count] = new BBTree.BBTreeBox(node);
			this.count++;
			return this.count - 1;
		}

		
		private int GetBox(IntRect rect)
		{
			if (this.count >= this.arr.Length)
			{
				this.EnsureCapacity(this.count + 1);
			}
			this.arr[this.count] = new BBTree.BBTreeBox(rect);
			this.count++;
			return this.count - 1;
		}

		
		public void RebuildFrom(MeshNode[] nodes)
		{
			this.Clear();
			if (nodes.Length == 0)
			{
				return;
			}
			if (nodes.Length == 1)
			{
				this.GetBox(nodes[0]);
				return;
			}
			this.EnsureCapacity(Mathf.CeilToInt((float)nodes.Length * 2.1f));
			MeshNode[] array = new MeshNode[nodes.Length];
			for (int i = 0; i < nodes.Length; i++)
			{
				array[i] = nodes[i];
			}
			this.RebuildFromInternal(array, 0, nodes.Length, false);
		}

		
		private static int SplitByX(MeshNode[] nodes, int from, int to, int divider)
		{
			int num = to;
			for (int i = from; i < num; i++)
			{
				if (nodes[i].position.x > divider)
				{
					num--;
					MeshNode meshNode = nodes[num];
					nodes[num] = nodes[i];
					nodes[i] = meshNode;
					i--;
				}
			}
			return num;
		}

		
		private static int SplitByZ(MeshNode[] nodes, int from, int to, int divider)
		{
			int num = to;
			for (int i = from; i < num; i++)
			{
				if (nodes[i].position.z > divider)
				{
					num--;
					MeshNode meshNode = nodes[num];
					nodes[num] = nodes[i];
					nodes[i] = meshNode;
					i--;
				}
			}
			return num;
		}

		
		private int RebuildFromInternal(MeshNode[] nodes, int from, int to, bool odd)
		{
			if (to - from <= 0)
			{
				throw new ArgumentException();
			}
			if (to - from == 1)
			{
				return this.GetBox(nodes[from]);
			}
			IntRect rect = BBTree.NodeBounds(nodes, from, to);
			int box = this.GetBox(rect);
			if (to - from == 2)
			{
				this.arr[box].left = this.GetBox(nodes[from]);
				this.arr[box].right = this.GetBox(nodes[from + 1]);
				return box;
			}
			int num;
			if (odd)
			{
				int divider = (rect.xmin + rect.xmax) / 2;
				num = BBTree.SplitByX(nodes, from, to, divider);
			}
			else
			{
				int divider2 = (rect.ymin + rect.ymax) / 2;
				num = BBTree.SplitByZ(nodes, from, to, divider2);
			}
			if (num == from || num == to)
			{
				if (!odd)
				{
					int divider3 = (rect.xmin + rect.xmax) / 2;
					num = BBTree.SplitByX(nodes, from, to, divider3);
				}
				else
				{
					int divider4 = (rect.ymin + rect.ymax) / 2;
					num = BBTree.SplitByZ(nodes, from, to, divider4);
				}
				if (num == from || num == to)
				{
					num = (from + to) / 2;
				}
			}
			this.arr[box].left = this.RebuildFromInternal(nodes, from, num, !odd);
			this.arr[box].right = this.RebuildFromInternal(nodes, num, to, !odd);
			return box;
		}

		
		private static IntRect NodeBounds(MeshNode[] nodes, int from, int to)
		{
			if (to - from <= 0)
			{
				throw new ArgumentException();
			}
			Int3 vertex = nodes[from].GetVertex(0);
			Int2 @int = new Int2(vertex.x, vertex.z);
			Int2 int2 = @int;
			for (int i = from; i < to; i++)
			{
				MeshNode meshNode = nodes[i];
				int vertexCount = meshNode.GetVertexCount();
				for (int j = 0; j < vertexCount; j++)
				{
					Int3 vertex2 = meshNode.GetVertex(j);
					@int.x = Math.Min(@int.x, vertex2.x);
					@int.y = Math.Min(@int.y, vertex2.z);
					int2.x = Math.Max(int2.x, vertex2.x);
					int2.y = Math.Max(int2.y, vertex2.z);
				}
			}
			return new IntRect(@int.x, @int.y, int2.x, int2.y);
		}

		
		[Conditional("ASTARDEBUG")]
		private static void DrawDebugRect(IntRect rect)
		{
			UnityEngine.Debug.DrawLine(new Vector3((float)rect.xmin, 0f, (float)rect.ymin), new Vector3((float)rect.xmax, 0f, (float)rect.ymin), Color.white);
			UnityEngine.Debug.DrawLine(new Vector3((float)rect.xmin, 0f, (float)rect.ymax), new Vector3((float)rect.xmax, 0f, (float)rect.ymax), Color.white);
			UnityEngine.Debug.DrawLine(new Vector3((float)rect.xmin, 0f, (float)rect.ymin), new Vector3((float)rect.xmin, 0f, (float)rect.ymax), Color.white);
			UnityEngine.Debug.DrawLine(new Vector3((float)rect.xmax, 0f, (float)rect.ymin), new Vector3((float)rect.xmax, 0f, (float)rect.ymax), Color.white);
		}

		
		[Conditional("ASTARDEBUG")]
		private static void DrawDebugNode(MeshNode node, float yoffset, Color color)
		{
			UnityEngine.Debug.DrawLine((Vector3)node.GetVertex(1) + Vector3.up * yoffset, (Vector3)node.GetVertex(2) + Vector3.up * yoffset, color);
			UnityEngine.Debug.DrawLine((Vector3)node.GetVertex(0) + Vector3.up * yoffset, (Vector3)node.GetVertex(1) + Vector3.up * yoffset, color);
			UnityEngine.Debug.DrawLine((Vector3)node.GetVertex(2) + Vector3.up * yoffset, (Vector3)node.GetVertex(0) + Vector3.up * yoffset, color);
		}

		
		public void Insert(MeshNode node)
		{
			int box = this.GetBox(node);
			if (box == 0)
			{
				return;
			}
			BBTree.BBTreeBox bbtreeBox = this.arr[box];
			int num = 0;
			BBTree.BBTreeBox bbtreeBox2;
			for (;;)
			{
				bbtreeBox2 = this.arr[num];
				bbtreeBox2.rect = BBTree.ExpandToContain(bbtreeBox2.rect, bbtreeBox.rect);
				if (bbtreeBox2.node != null)
				{
					break;
				}
				this.arr[num] = bbtreeBox2;
				int num2 = BBTree.ExpansionRequired(this.arr[bbtreeBox2.left].rect, bbtreeBox.rect);
				int num3 = BBTree.ExpansionRequired(this.arr[bbtreeBox2.right].rect, bbtreeBox.rect);
				if (num2 < num3)
				{
					num = bbtreeBox2.left;
				}
				else if (num3 < num2)
				{
					num = bbtreeBox2.right;
				}
				else
				{
					num = ((BBTree.RectArea(this.arr[bbtreeBox2.left].rect) >= BBTree.RectArea(this.arr[bbtreeBox2.right].rect)) ? bbtreeBox2.right : bbtreeBox2.left);
				}
			}
			bbtreeBox2.left = box;
			int box2 = this.GetBox(bbtreeBox2.node);
			bbtreeBox2.right = box2;
			bbtreeBox2.node = null;
			this.arr[num] = bbtreeBox2;
		}

		
		public NNInfoInternal Query(Vector3 p, NNConstraint constraint)
		{
			if (this.count == 0)
			{
				return new NNInfoInternal(null);
			}
			NNInfoInternal result = default(NNInfoInternal);
			this.SearchBox(0, p, constraint, ref result);
			result.UpdateInfo();
			return result;
		}

		
		public NNInfoInternal QueryCircle(Vector3 p, float radius, NNConstraint constraint)
		{
			if (this.count == 0)
			{
				return new NNInfoInternal(null);
			}
			NNInfoInternal result = new NNInfoInternal(null);
			this.SearchBoxCircle(0, p, radius, constraint, ref result);
			result.UpdateInfo();
			return result;
		}

		
		public NNInfoInternal QueryClosest(Vector3 p, NNConstraint constraint, out float distance)
		{
			distance = float.PositiveInfinity;
			return this.QueryClosest(p, constraint, ref distance, new NNInfoInternal(null));
		}

		
		public NNInfoInternal QueryClosestXZ(Vector3 p, NNConstraint constraint, ref float distance, NNInfoInternal previous)
		{
			if (this.count == 0)
			{
				return previous;
			}
			this.SearchBoxClosestXZ(0, p, ref distance, constraint, ref previous);
			return previous;
		}

		
		private void SearchBoxClosestXZ(int boxi, Vector3 p, ref float closestDist, NNConstraint constraint, ref NNInfoInternal nnInfo)
		{
			BBTree.BBTreeBox bbtreeBox = this.arr[boxi];
			if (bbtreeBox.node != null)
			{
				Vector3 constClampedPosition = bbtreeBox.node.ClosestPointOnNodeXZ(p);
				if (constraint == null || constraint.Suitable(bbtreeBox.node))
				{
					float num = (constClampedPosition.x - p.x) * (constClampedPosition.x - p.x) + (constClampedPosition.z - p.z) * (constClampedPosition.z - p.z);
					if (nnInfo.constrainedNode == null || num < closestDist * closestDist)
					{
						nnInfo.constrainedNode = bbtreeBox.node;
						nnInfo.constClampedPosition = constClampedPosition;
						closestDist = (float)Math.Sqrt((double)num);
					}
				}
			}
			else
			{
				if (BBTree.RectIntersectsCircle(this.arr[bbtreeBox.left].rect, p, closestDist))
				{
					this.SearchBoxClosestXZ(bbtreeBox.left, p, ref closestDist, constraint, ref nnInfo);
				}
				if (BBTree.RectIntersectsCircle(this.arr[bbtreeBox.right].rect, p, closestDist))
				{
					this.SearchBoxClosestXZ(bbtreeBox.right, p, ref closestDist, constraint, ref nnInfo);
				}
			}
		}

		
		public NNInfoInternal QueryClosest(Vector3 p, NNConstraint constraint, ref float distance, NNInfoInternal previous)
		{
			if (this.count == 0)
			{
				return previous;
			}
			this.SearchBoxClosest(0, p, ref distance, constraint, ref previous);
			return previous;
		}

		
		private void SearchBoxClosest(int boxi, Vector3 p, ref float closestDist, NNConstraint constraint, ref NNInfoInternal nnInfo)
		{
			BBTree.BBTreeBox bbtreeBox = this.arr[boxi];
			if (bbtreeBox.node != null)
			{
				if (BBTree.NodeIntersectsCircle(bbtreeBox.node, p, closestDist))
				{
					Vector3 vector = bbtreeBox.node.ClosestPointOnNode(p);
					if (constraint == null || constraint.Suitable(bbtreeBox.node))
					{
						float sqrMagnitude = (vector - p).sqrMagnitude;
						if (nnInfo.constrainedNode == null || sqrMagnitude < closestDist * closestDist)
						{
							nnInfo.constrainedNode = bbtreeBox.node;
							nnInfo.constClampedPosition = vector;
							closestDist = (float)Math.Sqrt((double)sqrMagnitude);
						}
					}
				}
			}
			else
			{
				if (BBTree.RectIntersectsCircle(this.arr[bbtreeBox.left].rect, p, closestDist))
				{
					this.SearchBoxClosest(bbtreeBox.left, p, ref closestDist, constraint, ref nnInfo);
				}
				if (BBTree.RectIntersectsCircle(this.arr[bbtreeBox.right].rect, p, closestDist))
				{
					this.SearchBoxClosest(bbtreeBox.right, p, ref closestDist, constraint, ref nnInfo);
				}
			}
		}

		
		public MeshNode QueryInside(Vector3 p, NNConstraint constraint)
		{
			return (this.count == 0) ? null : this.SearchBoxInside(0, p, constraint);
		}

		
		private MeshNode SearchBoxInside(int boxi, Vector3 p, NNConstraint constraint)
		{
			BBTree.BBTreeBox bbtreeBox = this.arr[boxi];
			if (bbtreeBox.node != null)
			{
				if (bbtreeBox.node.ContainsPoint((Int3)p))
				{
					if (constraint == null || constraint.Suitable(bbtreeBox.node))
					{
						return bbtreeBox.node;
					}
				}
			}
			else
			{
				if (this.arr[bbtreeBox.left].Contains(p))
				{
					MeshNode meshNode = this.SearchBoxInside(bbtreeBox.left, p, constraint);
					if (meshNode != null)
					{
						return meshNode;
					}
				}
				if (this.arr[bbtreeBox.right].Contains(p))
				{
					MeshNode meshNode = this.SearchBoxInside(bbtreeBox.right, p, constraint);
					if (meshNode != null)
					{
						return meshNode;
					}
				}
			}
			return null;
		}

		
		private void SearchBoxCircle(int boxi, Vector3 p, float radius, NNConstraint constraint, ref NNInfoInternal nnInfo)
		{
			BBTree.BBTreeBox bbtreeBox = this.arr[boxi];
			if (bbtreeBox.node != null)
			{
				if (BBTree.NodeIntersectsCircle(bbtreeBox.node, p, radius))
				{
					Vector3 vector = bbtreeBox.node.ClosestPointOnNode(p);
					float sqrMagnitude = (vector - p).sqrMagnitude;
					if (nnInfo.node == null)
					{
						nnInfo.node = bbtreeBox.node;
						nnInfo.clampedPosition = vector;
					}
					else if (sqrMagnitude < (nnInfo.clampedPosition - p).sqrMagnitude)
					{
						nnInfo.node = bbtreeBox.node;
						nnInfo.clampedPosition = vector;
					}
					if ((constraint == null || constraint.Suitable(bbtreeBox.node)) && (nnInfo.constrainedNode == null || sqrMagnitude < (nnInfo.constClampedPosition - p).sqrMagnitude))
					{
						nnInfo.constrainedNode = bbtreeBox.node;
						nnInfo.constClampedPosition = vector;
					}
				}
				return;
			}
			if (BBTree.RectIntersectsCircle(this.arr[bbtreeBox.left].rect, p, radius))
			{
				this.SearchBoxCircle(bbtreeBox.left, p, radius, constraint, ref nnInfo);
			}
			if (BBTree.RectIntersectsCircle(this.arr[bbtreeBox.right].rect, p, radius))
			{
				this.SearchBoxCircle(bbtreeBox.right, p, radius, constraint, ref nnInfo);
			}
		}

		
		private void SearchBox(int boxi, Vector3 p, NNConstraint constraint, ref NNInfoInternal nnInfo)
		{
			BBTree.BBTreeBox bbtreeBox = this.arr[boxi];
			if (bbtreeBox.node != null)
			{
				if (bbtreeBox.node.ContainsPoint((Int3)p))
				{
					if (nnInfo.node == null)
					{
						nnInfo.node = bbtreeBox.node;
					}
					else if (Mathf.Abs(((Vector3)bbtreeBox.node.position).y - p.y) < Mathf.Abs(((Vector3)nnInfo.node.position).y - p.y))
					{
						nnInfo.node = bbtreeBox.node;
					}
					if (constraint.Suitable(bbtreeBox.node) && (nnInfo.constrainedNode == null || Mathf.Abs((float)bbtreeBox.node.position.y - p.y) < Mathf.Abs((float)nnInfo.constrainedNode.position.y - p.y)))
					{
						nnInfo.constrainedNode = bbtreeBox.node;
					}
				}
				return;
			}
			if (this.arr[bbtreeBox.left].Contains(p))
			{
				this.SearchBox(bbtreeBox.left, p, constraint, ref nnInfo);
			}
			if (this.arr[bbtreeBox.right].Contains(p))
			{
				this.SearchBox(bbtreeBox.right, p, constraint, ref nnInfo);
			}
		}

		
		public void OnDrawGizmos()
		{
			Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
			if (this.count == 0)
			{
				return;
			}
			this.OnDrawGizmos(0, 0);
		}

		
		private void OnDrawGizmos(int boxi, int depth)
		{
			BBTree.BBTreeBox bbtreeBox = this.arr[boxi];
			Vector3 a = (Vector3)new Int3(bbtreeBox.rect.xmin, 0, bbtreeBox.rect.ymin);
			Vector3 vector = (Vector3)new Int3(bbtreeBox.rect.xmax, 0, bbtreeBox.rect.ymax);
			Vector3 vector2 = (a + vector) * 0.5f;
			Vector3 size = (vector - vector2) * 2f;
			size = new Vector3(size.x, 1f, size.z);
			vector2.y += (float)(depth * 2);
			Gizmos.color = AstarMath.IntToColor(depth, 1f);
			Gizmos.DrawCube(vector2, size);
			if (bbtreeBox.node == null)
			{
				this.OnDrawGizmos(bbtreeBox.left, depth + 1);
				this.OnDrawGizmos(bbtreeBox.right, depth + 1);
			}
		}

		
		private static bool NodeIntersectsCircle(MeshNode node, Vector3 p, float radius)
		{
			return float.IsPositiveInfinity(radius) || (p - node.ClosestPointOnNode(p)).sqrMagnitude < radius * radius;
		}

		
		private static bool RectIntersectsCircle(IntRect r, Vector3 p, float radius)
		{
			if (float.IsPositiveInfinity(radius))
			{
				return true;
			}
			Vector3 vector = p;
			p.x = Math.Max(p.x, (float)r.xmin * 0.001f);
			p.x = Math.Min(p.x, (float)r.xmax * 0.001f);
			p.z = Math.Max(p.z, (float)r.ymin * 0.001f);
			p.z = Math.Min(p.z, (float)r.ymax * 0.001f);
			return (p.x - vector.x) * (p.x - vector.x) + (p.z - vector.z) * (p.z - vector.z) < radius * radius;
		}

		
		private static int ExpansionRequired(IntRect r, IntRect r2)
		{
			int num = Math.Min(r.xmin, r2.xmin);
			int num2 = Math.Max(r.xmax, r2.xmax);
			int num3 = Math.Min(r.ymin, r2.ymin);
			int num4 = Math.Max(r.ymax, r2.ymax);
			return (num2 - num) * (num4 - num3) - BBTree.RectArea(r);
		}

		
		private static IntRect ExpandToContain(IntRect r, IntRect r2)
		{
			return IntRect.Union(r, r2);
		}

		
		private static int RectArea(IntRect r)
		{
			return r.Width * r.Height;
		}

		
		private BBTree.BBTreeBox[] arr = new BBTree.BBTreeBox[6];

		
		private int count;

		
		private struct BBTreeBox
		{
			
			public BBTreeBox(IntRect rect)
			{
				this.node = null;
				this.rect = rect;
				this.left = (this.right = -1);
			}

			
			public BBTreeBox(MeshNode node)
			{
				this.node = node;
				Int3 vertex = node.GetVertex(0);
				Int2 @int = new Int2(vertex.x, vertex.z);
				Int2 int2 = @int;
				for (int i = 1; i < node.GetVertexCount(); i++)
				{
					Int3 vertex2 = node.GetVertex(i);
					@int.x = Math.Min(@int.x, vertex2.x);
					@int.y = Math.Min(@int.y, vertex2.z);
					int2.x = Math.Max(int2.x, vertex2.x);
					int2.y = Math.Max(int2.y, vertex2.z);
				}
				this.rect = new IntRect(@int.x, @int.y, int2.x, int2.y);
				this.left = (this.right = -1);
			}

			
			
			public bool IsLeaf
			{
				get
				{
					return this.node != null;
				}
			}

			
			public bool Contains(Vector3 p)
			{
				Int3 @int = (Int3)p;
				return this.rect.Contains(@int.x, @int.z);
			}

			
			public IntRect rect;

			
			public MeshNode node;

			
			public int left;

			
			public int right;
		}
	}
}
