using System;
using System.Collections;
using UnityEngine;

public class CoopMutantPredictor : MonoBehaviour
{
	private void getAttackDirection(int hitDir)
	{
	}

	private void HitHead()
	{
	}

	private void getCombo(int combo)
	{
	}

	private void StartPrediction()
	{
		base.StopCoroutine("PredictionRoutine");
		base.StartCoroutine("PredictionRoutine");
	}

	private IEnumerator PredictionRoutine()
	{
		this.resetWeight = this.anim.GetLayerWeight(this.NormalLayer);
		this.anim.SetLayerWeight(this.NormalLayer, 0f);
		this.anim.SetLayerWeight(this.HitLayer, this.Weight);
		yield return new WaitForSeconds(this.PredictionTime);
		float dt = 0f;
		while (dt <= 1f)
		{
			dt += Time.deltaTime * this.LerpSpeed;
			this.anim.SetLayerWeight(this.NormalLayer, Mathf.Lerp(0f, this.resetWeight, dt));
			this.anim.SetLayerWeight(this.HitLayer, Mathf.Lerp(this.Weight, 0f, dt));
			yield return null;
		}
		yield break;
	}

	public Animator anim;

	private float resetWeight;

	public float LerpSpeed = 2f;

	public int HitLayer = 1;

	public int NormalLayer;

	public float Weight = 1f;

	public float PredictionTime = 1f;
}
