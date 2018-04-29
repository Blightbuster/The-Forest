using System;
using UnityEngine;


public class CorpseBlood : MonoBehaviour
{
	
	private void Start()
	{
		this._skinProperties = new MaterialPropertyBlock();
	}

	
	public void Hit(int damage)
	{
		if (!this._skin)
		{
			return;
		}
		this._totalDamage = Mathf.Clamp01(this._totalDamage + 0.2f);
		this._skin.GetPropertyBlock(this._skinProperties);
		this._skinProperties.SetFloat("_Damage1", this._damageScaling1 * this._totalDamage);
		this._skinProperties.SetFloat("_Damage2", this._damageScaling2 * this._totalDamage);
		this._skinProperties.SetFloat("_Damage3", this._damageScaling3 * this._totalDamage);
		this._skinProperties.SetFloat("_Damage4", this._damageScaling4 * this._totalDamage);
		this._skin.SetPropertyBlock(this._skinProperties);
	}

	
	public Renderer _skin;

	
	[Range(0f, 1f)]
	public float _damageScaling1 = 1f;

	
	[Range(0f, 1f)]
	public float _damageScaling2 = 1f;

	
	[Range(0f, 1f)]
	public float _damageScaling3 = 1f;

	
	[Range(0f, 1f)]
	public float _damageScaling4 = 1f;

	
	private MaterialPropertyBlock _skinProperties;

	
	private float _totalDamage;
}
