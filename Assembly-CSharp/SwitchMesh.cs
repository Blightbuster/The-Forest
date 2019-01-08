using System;
using UnityEngine;

public class SwitchMesh : MonoBehaviour
{
	private void OnMouseDown()
	{
		base.GetComponent<MeshFilter>().mesh = this.mesh;
	}

	public Mesh mesh;
}
