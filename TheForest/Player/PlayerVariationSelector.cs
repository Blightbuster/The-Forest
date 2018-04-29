using System;
using System.Collections;
using UnityEngine;

namespace TheForest.Player
{
	
	public class PlayerVariationSelector : MonoBehaviour
	{
		
		private void Awake()
		{
			this._instanceModel = UnityEngine.Object.Instantiate<CoopPlayerVariations>(this._baseModelPrefab);
			for (int i = this._instanceModel.transform.childCount - 1; i >= 0; i--)
			{
				Transform child = this._instanceModel.transform.GetChild(i);
				if (child.name != "player_BASE")
				{
					UnityEngine.Object.DestroyImmediate(child.gameObject);
				}
				else
				{
					MonoBehaviour[] components = child.GetComponents<MonoBehaviour>();
					foreach (MonoBehaviour obj in components)
					{
						UnityEngine.Object.DestroyImmediate(obj);
					}
					this._animator = child.GetComponent<Animator>();
				}
			}
			MonoBehaviour[] components2 = this._instanceModel.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour monoBehaviour in components2)
			{
				if (!(monoBehaviour is CoopPlayerVariations))
				{
					UnityEngine.Object.DestroyImmediate(monoBehaviour);
				}
			}
			itemConstrainToHand[] componentsInChildren = this._instanceModel.GetComponentsInChildren<itemConstrainToHand>();
			foreach (itemConstrainToHand itemConstrainToHand in componentsInChildren)
			{
				if (!itemConstrainToHand.fixedItems)
				{
					UnityEngine.Object.DestroyImmediate(itemConstrainToHand.gameObject);
				}
			}
			this._instanceModel.transform.parent = base.transform;
			this._instanceModel.transform.localPosition = Vector3.zero;
			this._instanceModel.transform.localRotation = Quaternion.identity;
		}

		
		private void Update()
		{
			if (this.nextbodyVariation)
			{
				this.nextbodyVariation = false;
				this.NextBodyVariation();
			}
			if (this.nextheadVariation)
			{
				this.nextheadVariation = false;
				this.NextHeadVariation();
			}
			if (this.nexthairVariation)
			{
				this.nexthairVariation = false;
				this.NextHairVariation();
			}
			if (this.nextskinVariation)
			{
				this.nextskinVariation = false;
				this.NextSkinVariation();
			}
		}

		
		private void SetBodyVariation(int variationNumber)
		{
			if (this._instanceModel)
			{
				this._bodyVariation = variationNumber;
			}
		}

		
		private void NextBodyVariation()
		{
			if (this._instanceModel)
			{
			}
		}

		
		private void SetHeadVariation(int variationNumber)
		{
			if (this._instanceModel)
			{
				this._headVariation = variationNumber;
				CoopPlayerVariation[] variations = this._instanceModel.Variations;
				for (int i = 0; i < variations.Length; i++)
				{
					if (variations[i].Head)
					{
						if (i == variationNumber)
						{
							variations[i].Head.gameObject.SetActive(true);
						}
						else
						{
							variations[i].Head.gameObject.SetActive(false);
						}
					}
				}
			}
		}

		
		private void NextHeadVariation()
		{
			if (this._instanceModel)
			{
				this.SetHeadVariation((int)Mathf.Repeat((float)(this._headVariation + 1), (float)this._instanceModel.Variations.Length));
			}
		}

		
		private void SetHairVariation(int variationNumber)
		{
			if (this._instanceModel)
			{
				this._hairVariation = variationNumber;
				CoopPlayerVariation[] variations = this._instanceModel.Variations;
				for (int i = 0; i < variations.Length; i++)
				{
				}
			}
		}

		
		private void NextHairVariation()
		{
			if (this._instanceModel)
			{
				this.SetHairVariation((int)Mathf.Repeat((float)(this._hairVariation + 1), (float)this._instanceModel.Variations.Length));
			}
		}

		
		private void SetSkinVariation(int variationNumber)
		{
			if (this._instanceModel)
			{
				this._skinVariation = variationNumber;
				CoopPlayerVariation[] variations = this._instanceModel.Variations;
				base.StartCoroutine(this.DoCheckArms());
			}
		}

		
		private void NextSkinVariation()
		{
			if (this._instanceModel)
			{
				this.SetSkinVariation((int)Mathf.Repeat((float)(this._skinVariation + 1), (float)this._instanceModel.Variations.Length));
			}
		}

		
		private IEnumerator DoCheckArms()
		{
			if (!this._animator.GetBool("checkArms"))
			{
				this._animator.SetBoolReflected("checkArms", true);
				yield return YieldPresets.WaitThreeSeconds;
				this._animator.SetBoolReflected("checkArms", false);
			}
			yield break;
		}

		
		public CoopPlayerVariations _baseModelPrefab;

		
		private Animator _animator;

		
		private CoopPlayerVariations _instanceModel;

		
		private int _bodyVariation;

		
		private int _headVariation;

		
		private int _hairVariation;

		
		private int _skinVariation;

		
		public bool nextbodyVariation;

		
		public bool nextheadVariation;

		
		public bool nexthairVariation;

		
		public bool nextskinVariation;
	}
}
