using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	
	[DoNotSerializePublic]
	public class StewIngredient : MonoBehaviour
	{
		
		private IEnumerator Start()
		{
			while (!base.transform.parent)
			{
				yield return null;
			}
			foreach (object obj in base.transform.parent)
			{
				Transform sibbling = (Transform)obj;
				EatStew es = sibbling.GetComponent<EatStew>();
				if (es)
				{
					this.AddEffectTo(es);
					break;
				}
			}
			yield break;
		}

		
		public void AddEffectTo(EatStew es)
		{
			foreach (StewItemEffect stewItemEffect in this._effects)
			{
				switch (stewItemEffect._type)
				{
				case StewItemEffectTypes.Fullness:
					es._fullness += stewItemEffect._amount;
					break;
				case StewItemEffectTypes.Hydration:
					es._hydration += stewItemEffect._amount;
					break;
				case StewItemEffectTypes.Energy:
					es._energy += stewItemEffect._amount;
					break;
				case StewItemEffectTypes.Health:
					es._health += stewItemEffect._amount;
					break;
				case StewItemEffectTypes.IsFresh:
					es._isFresh += stewItemEffect._amount;
					break;
				case StewItemEffectTypes.IsMeat:
					es._meats++;
					break;
				case StewItemEffectTypes.IsLimb:
					es.IsLimb = true;
					break;
				case StewItemEffectTypes.IsMushroom:
					es._mushrooms++;
					break;
				case StewItemEffectTypes.IsHerb:
					es._herbs++;
					break;
				}
			}
		}

		
		private void Cooked()
		{
			if (this._renderer && this._cookedMat)
			{
				this._renderer.sharedMaterial = this._cookedMat;
			}
		}

		
		[NameFromProperty("_type", 0)]
		public StewItemEffect[] _effects;

		
		public Renderer _renderer;

		
		public Material _cookedMat;

		
		public Material _burnt;
	}
}
