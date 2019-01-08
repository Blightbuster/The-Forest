using System;
using UnityEngine;

public class AeroplanePropellerAnimator : MonoBehaviour
{
	private void Awake()
	{
		this.plane = base.GetComponent<AeroplaneController>();
		this.propellorBlur.parent = this.propellorModel;
	}

	private void Update()
	{
		this.propellorModel.Rotate(0f, this.maxRpm * this.plane.Throttle * Time.deltaTime * 60f, 0f);
		int num = 0;
		if (this.plane.Throttle > this.throttleBlurStart)
		{
			float num2 = Mathf.InverseLerp(this.throttleBlurStart, this.throttleBlurEnd, this.plane.Throttle);
			num = Mathf.FloorToInt(num2 * (float)(this.propellorBlurTextures.Length - 1));
		}
		if (num != this.propellorBlurState)
		{
			this.propellorBlurState = num;
			if (this.propellorBlurState == 0)
			{
				this.propellorModel.GetComponent<Renderer>().enabled = true;
				this.propellorBlur.GetComponent<Renderer>().enabled = false;
			}
			else
			{
				this.propellorModel.GetComponent<Renderer>().enabled = false;
				this.propellorBlur.GetComponent<Renderer>().enabled = true;
				this.propellorBlur.GetComponent<Renderer>().material.mainTexture = this.propellorBlurTextures[this.propellorBlurState];
			}
		}
	}

	[SerializeField]
	private Transform propellorModel;

	[SerializeField]
	private Transform propellorBlur;

	[SerializeField]
	private Texture2D[] propellorBlurTextures;

	[SerializeField]
	[Range(0f, 1f)]
	private float throttleBlurStart = 0.25f;

	[SerializeField]
	[Range(0f, 1f)]
	private float throttleBlurEnd = 0.5f;

	[SerializeField]
	private float maxRpm = 2000f;

	private AeroplaneController plane;

	private int propellorBlurState = -1;

	private const float RpmToDps = 60f;
}
