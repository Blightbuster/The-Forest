using System;
using System.Collections;
using UnityEngine;


public class enemyType : MonoBehaviour
{
	
	
	
	public EnemyType Type { get; set; }

	
	private void OnEnable()
	{
		base.StartCoroutine(this.SetTypeRoutine());
	}

	
	private IEnumerator SetTypeRoutine()
	{
		yield return null;
		mutantTypeSetup typeSetup = base.GetComponent<mutantTypeSetup>();
		if (typeSetup)
		{
			mutantAI ai = typeSetup.setup.ai;
			if (ai.skinned)
			{
				this.Type = EnemyType.skinMaskMale;
			}
			else if (ai.maleSkinny)
			{
				this.Type = ((!ai.pale) ? EnemyType.skinnyMale : EnemyType.skinnyMalePale);
			}
			else if (ai.femaleSkinny)
			{
				this.Type = ((!ai.pale) ? EnemyType.skinnyFemale : EnemyType.skinnyFemalePale);
			}
			else if (ai.male)
			{
				this.Type = ((!ai.pale) ? EnemyType.regularMale : EnemyType.paleMale);
			}
			else if (ai.female)
			{
				this.Type = ((!ai.pale) ? EnemyType.regularFemale : EnemyType.paleFemale);
			}
			else if (ai.creepy_male)
			{
				this.Type = ((!ai.pale) ? EnemyType.creepyArmsy : EnemyType.creepyArmsy);
			}
			else if (ai.creepy_fat)
			{
				this.Type = EnemyType.creepyFat;
			}
			else if (ai.creepy_baby)
			{
				this.Type = EnemyType.creepyBaby;
			}
			else if (ai.creepy)
			{
				this.Type = ((!ai.pale) ? EnemyType.creepySpiderLady : EnemyType.blueCreepySpiderLady);
			}
		}
		else
		{
			BoltEntity entity = base.GetComponent<BoltEntity>();
			mutantAI_net ai2 = base.GetComponentInChildren<mutantAI_net>();
			IMutantState state = entity.GetState<IMutantState>();
			while (!entity.isAttached || state.ai_mask == 0)
			{
				yield return null;
			}
			if (ai2)
			{
				if (ai2.skinned)
				{
					this.Type = EnemyType.skinMaskMale;
				}
				else if (ai2.maleSkinny)
				{
					this.Type = ((!ai2.pale) ? EnemyType.skinnyMale : EnemyType.skinnyMalePale);
				}
				else if (ai2.femaleSkinny)
				{
					this.Type = ((!ai2.pale) ? EnemyType.skinnyFemale : EnemyType.skinnyFemalePale);
				}
				else if (ai2.male)
				{
					this.Type = ((!ai2.pale) ? EnemyType.regularMale : EnemyType.paleMale);
				}
				else if (ai2.female)
				{
					this.Type = ((!ai2.pale) ? EnemyType.regularFemale : EnemyType.paleFemale);
				}
				else if (ai2.creepy_male)
				{
					this.Type = ((!ai2.pale) ? EnemyType.creepyArmsy : EnemyType.creepyArmsy);
				}
				else if (ai2.creepy_fat)
				{
					this.Type = EnemyType.creepyFat;
				}
				else if (ai2.creepy_baby)
				{
					this.Type = EnemyType.creepyBaby;
				}
				else if (ai2.creepy)
				{
					this.Type = ((!ai2.pale) ? EnemyType.creepySpiderLady : EnemyType.blueCreepySpiderLady);
				}
			}
		}
		yield break;
	}
}
