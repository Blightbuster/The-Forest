using System;
using System.Collections;
using UnityEngine;

public class flyingObjectFixer : MonoBehaviour
{
	private void OnEnable()
	{
		if (!this.rb)
		{
			this.rb = base.transform.GetComponent<Rigidbody>();
		}
		if (this.rb)
		{
			this.initDrag = this.rb.drag;
			this.initAngularDrag = this.rb.angularDrag;
			base.StartCoroutine(this.dampRigidBody());
		}
	}

	private void OnDisable()
	{
		base.StopAllCoroutines();
		if (this.rb)
		{
			this.rb.drag = this.initDrag;
			this.rb.angularDrag = this.initAngularDrag;
		}
	}

	private IEnumerator dampRigidBody()
	{
		this.rb.drag = this._dragSetting;
		this.rb.angularDrag = this._angularDrag;
		if (this._lerpDownOverTime)
		{
			float startTime = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup - startTime < this._fixTime)
			{
				float timeElapsed = Time.realtimeSinceStartup - startTime;
				this.rb.drag = Mathf.Lerp(this._dragSetting, this.initDrag, timeElapsed / this._fixTime);
				this.rb.angularDrag = Mathf.Lerp(this._angularDrag, this.initDrag, timeElapsed / this._fixTime);
				yield return null;
			}
		}
		else
		{
			yield return new WaitForSeconds(this._fixTime);
		}
		this.rb.drag = this.initDrag;
		this.rb.angularDrag = this.initAngularDrag;
		yield break;
	}

	public float _dragSetting = 20f;

	public float _angularDrag = 20f;

	public float _fixTime = 2f;

	public bool _lerpDownOverTime;

	private Rigidbody rb;

	private float initDrag;

	private float initAngularDrag;
}
