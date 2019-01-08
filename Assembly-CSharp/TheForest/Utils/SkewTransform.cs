using System;
using UnityEngine;

namespace TheForest.Utils
{
	[Serializable]
	public class SkewTransform
	{
		public void SetSkew(Transform skewer, Transform skewee, float angle)
		{
			skewer.localScale = this._skewerScale;
			skewee.localScale = new Vector3(1f, this._skeweeScaleY.Evaluate(angle), this._skeweeScaleZ.Evaluate(angle));
			skewee.localEulerAngles = new Vector3(this._skeweeRotX.Evaluate(angle), 0f, 0f);
		}

		public Vector3 _skewerScale = new Vector3(1f, 1f, 10f);

		public AnimationCurve _skeweeScaleY;

		public AnimationCurve _skeweeScaleZ;

		public AnimationCurve _skeweeRotX;
	}
}
