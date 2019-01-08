using System;
using TheForest.Commons.Delegates;
using UnityEngine;

namespace TheForest.Utils
{
	public class TimedStippling_Distance : MonoBehaviour
	{
		private void Awake()
		{
			this._alpha = 0.001f;
			if (!this._renderer)
			{
				this._renderer = base.GetComponent<Renderer>();
			}
			this._block = new MaterialPropertyBlock();
			this._alpha = 1E-05f;
			this.SetStipplingAlpha(Mathf.Clamp01(this._alpha));
			base.enabled = false;
			this.VisibilityCheck();
			if (!base.enabled)
			{
				this.WSRegister();
			}
		}

		private void Update()
		{
			this.SetStipplingAlpha(Mathf.Clamp01(this._alpha));
			if (this._stipplingIn)
			{
				this._alpha += Time.deltaTime;
				if (this._alpha >= 0.9375f)
				{
					this._isVisible = true;
					this._alpha = 0f;
					this.SetStipplingAlpha(this._alpha);
					base.enabled = false;
				}
			}
			else
			{
				this._alpha -= Time.deltaTime;
				if (this._alpha <= 0f)
				{
					this._renderer.enabled = false;
					this._isVisible = false;
					this._alpha = 1E-05f;
					this.SetStipplingAlpha(this._alpha);
					base.enabled = false;
				}
			}
		}

		private void OnEnable()
		{
			this.WSUnregister();
		}

		private void OnDisable()
		{
			this.WSRegister();
		}

		private void OnDestroy()
		{
			this.WSUnregister();
		}

		private void VisibilityCheck()
		{
			if (this._renderer)
			{
				if (LocalPlayer.Transform)
				{
					float num = Vector3.Distance(LocalPlayer.Transform.position, base.transform.position);
					if (this._isVisible)
					{
						if (num > this._stippleOutDistance)
						{
							this.BeginStipplingOut();
						}
					}
					else if (num < this._stippleInDistance)
					{
						this.BeginStipplingIn();
					}
				}
			}
			else
			{
				this.WSUnregister();
				UnityEngine.Object.Destroy(this);
			}
		}

		private void SetStipplingAlpha(float alpha)
		{
			if (this._renderer)
			{
				this._renderer.GetPropertyBlock(this._block);
				this._block.SetVector("_StippleAlpha", new Vector4(alpha, Mathf.Ceil(alpha * 16f) * 0.0625f, 0f, 0f));
				this._renderer.SetPropertyBlock(this._block);
			}
		}

		private void BeginStipplingIn()
		{
			this._renderer.enabled = true;
			this._stipplingIn = true;
			this._alpha = 0.001f;
			this.SetStipplingAlpha(Mathf.Clamp01(this._alpha));
			base.enabled = true;
		}

		private void BeginStipplingOut()
		{
			this._stipplingIn = false;
			this._alpha = 0.9374f;
			this.SetStipplingAlpha(Mathf.Clamp01(this._alpha));
			base.enabled = true;
		}

		private void WSRegister()
		{
			if (this._wsToken == -1)
			{
				this._wsToken = WorkScheduler.Register(new WsTask(this.VisibilityCheck), base.transform.position, false);
			}
		}

		private void WSUnregister()
		{
			if (this._wsToken != -1)
			{
				WorkScheduler.Unregister(new WsTask(this.VisibilityCheck), this._wsToken);
				this._wsToken = -1;
			}
		}

		public Renderer _renderer;

		public float _stippleOutDistance = 110f;

		public float _stippleInDistance = 90f;

		private bool _isVisible;

		private bool _stipplingIn;

		private float _alpha;

		private MaterialPropertyBlock _block;

		private int _wsToken = -1;
	}
}
