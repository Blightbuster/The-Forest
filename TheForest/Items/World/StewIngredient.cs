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
			IEnumerator enumerator = base.transform.parent.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					EatStew component = transform.GetComponent<EatStew>();
					if (component)
					{
						this.AddEffectTo(component);
						break;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
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
