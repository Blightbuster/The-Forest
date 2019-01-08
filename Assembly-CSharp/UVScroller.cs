using System;
using UnityEngine;

public class UVScroller : MonoBehaviour
{
	private void Start()
	{
		MeshRenderer component = base.GetComponent<MeshRenderer>();
		if (component != null)
		{
			this._instancedMaterial = component.material;
		}
	}

	private void Update()
	{
		if (this._instancedMaterial == null)
		{
			return;
		}
		Vector2 mainTextureOffset = this._instancedMaterial.mainTextureOffset + this.ScrollAmount * this.Speed * Time.deltaTime;
		if (this.AutoWrap)
		{
			mainTextureOffset.x %= 1f;
			mainTextureOffset.y %= 1f;
		}
		this._instancedMaterial.mainTextureOffset = mainTextureOffset;
	}

	private void OnDestroy()
	{
		if (this._instancedMaterial != null)
		{
			UnityEngine.Object.Destroy(this._instancedMaterial);
		}
		this._instancedMaterial = null;
	}

	public float Speed = 1f;

	public Vector2 ScrollAmount;

	public bool AutoWrap = true;

	private Material _instancedMaterial;
}
