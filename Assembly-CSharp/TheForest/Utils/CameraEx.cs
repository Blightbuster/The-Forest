using System;
using UnityEngine;

namespace TheForest.Utils
{
	public static class CameraEx
	{
		public static Rect GetScreenRectOf(this Camera cam, Collider collider)
		{
			Vector3 center = collider.bounds.center;
			Vector3 extents = collider.bounds.extents;
			Vector2 vector = cam.WorldToScreenPoint(collider.transform.TransformPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z)));
			Vector2 vector2 = vector;
			Vector2 vector3 = vector;
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(collider.transform.TransformPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(collider.transform.TransformPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(collider.transform.TransformPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(collider.transform.TransformPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(collider.transform.TransformPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(collider.transform.TransformPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(collider.transform.TransformPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			return new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
		}

		public static Rect GetScreenRectOf(this Camera cam, Renderer renderer)
		{
			MeshFilter component = renderer.GetComponent<MeshFilter>();
			Mesh mesh;
			if (component)
			{
				mesh = component.sharedMesh;
			}
			else if (renderer is SkinnedMeshRenderer)
			{
				mesh = (renderer as SkinnedMeshRenderer).sharedMesh;
			}
			else
			{
				mesh = null;
			}
			Vector3 center;
			Vector3 extents;
			if (mesh)
			{
				center = mesh.bounds.center;
				extents = mesh.bounds.extents;
			}
			else
			{
				center = renderer.bounds.center;
				extents = renderer.bounds.extents;
			}
			Vector2 vector = cam.WorldToScreenPoint(renderer.transform.TransformPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z)));
			Vector2 vector2 = vector;
			Vector2 vector3 = vector;
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(renderer.transform.TransformPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(renderer.transform.TransformPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(renderer.transform.TransformPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(renderer.transform.TransformPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(renderer.transform.TransformPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(renderer.transform.TransformPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			vector3 = cam.WorldToScreenPoint(renderer.transform.TransformPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z)));
			vector = new Vector2((vector.x < vector3.x) ? vector.x : vector3.x, (vector.y < vector3.y) ? vector.y : vector3.y);
			vector2 = new Vector2((vector2.x > vector3.x) ? vector2.x : vector3.x, (vector2.y > vector3.y) ? vector2.y : vector3.y);
			return new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
		}
	}
}
