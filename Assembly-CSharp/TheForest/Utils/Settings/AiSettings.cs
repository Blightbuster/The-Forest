using System;

namespace TheForest.Utils.Settings
{
	public class AiSettings
	{
		public AiSettings()
		{
			this.Refresh();
		}

		public void Refresh()
		{
			if (GameSetup.IsHardMode || GameSetup.IsHardSurvivalMode)
			{
				this.skinnyDamageRatio = 2f;
				this.paleSkinnyDamageRatio = 2f;
				this.regularMaleDamageRatio = 2f;
				this.regularFemaleDamageRatio = 2f;
				this.largePaleDamageRatio = 1.65f;
				this.creepyDamageRatio = 1.5f;
				this.creepyBabyDamageRatio = 2f;
				this.regularStructureDamageRatio = 2f;
				this.creepyStructureDamageRatio = 2f;
				this.paintedDamageRatio = 1.4f;
				this.skinMaskDamageRatio = 1.6f;
				this.skinnyHealthRatio = 1.5f;
				this.paleSkinnyHealthRatio = 2.25f;
				this.regularHealthRatio = 1.75f;
				this.leaderHealthRatio = 2.5f;
				this.largePaleHealthRatio = 1.75f;
				this.creepyHealthRatio = 1.5f;
				this.creepyBabyHealthRatio = 1.5f;
				this.paintedHealthAmount = 35f;
				this.skinMaskHealthAmount = 45f;
				this.aiAttackChanceRatio = 2.5f;
				this.aiFollowUpAfterAttackRatio = 2.5f;
				this.playerSearchRadiusRadio = 1f;
				this.skinnySpawnAmountRatio = 0.5f;
				this.regularSpawnAmountRatio = 1f;
				this.largePaleSpawnAmountRatio = 1.5f;
				this.creepySpawnAmountRatio = 1.5f;
				this.fireDamageRatio = 0.5f;
				this.fireDamageCreepyRatio = 0.35f;
				this.heavyAttackKnockDownChance = 0.25f;
				this.firemanThrowTimeRatio = 0.65f;
			}
			else
			{
				this.skinnyDamageRatio = 1f;
				this.paleSkinnyDamageRatio = 1f;
				this.regularMaleDamageRatio = 1f;
				this.regularFemaleDamageRatio = 1f;
				this.largePaleDamageRatio = 1f;
				this.creepyDamageRatio = 1f;
				this.creepyBabyDamageRatio = 1f;
				this.regularStructureDamageRatio = 1f;
				this.creepyStructureDamageRatio = 1f;
				this.paintedDamageRatio = 1.25f;
				this.skinMaskDamageRatio = 1.45f;
				this.skinnyHealthRatio = 1f;
				this.paleSkinnyHealthRatio = 1f;
				this.regularHealthRatio = 1f;
				this.leaderHealthRatio = 2f;
				this.largePaleHealthRatio = 1f;
				this.creepyHealthRatio = 1f;
				this.creepyBabyHealthRatio = 1f;
				this.paintedHealthAmount = 25f;
				this.skinMaskHealthAmount = 35f;
				this.aiAttackChanceRatio = 1f;
				this.aiFollowUpAfterAttackRatio = 1f;
				this.playerSearchRadiusRadio = 1f;
				this.skinnySpawnAmountRatio = 1f;
				this.regularSpawnAmountRatio = 1f;
				this.largePaleSpawnAmountRatio = 1f;
				this.creepySpawnAmountRatio = 1f;
				this.fireDamageRatio = 1f;
				this.fireDamageCreepyRatio = 1f;
				this.heavyAttackKnockDownChance = 1f;
				this.firemanThrowTimeRatio = 1f;
			}
		}

		public float skinnyDamageRatio;

		public float paleSkinnyDamageRatio;

		public float regularMaleDamageRatio;

		public float regularFemaleDamageRatio;

		public float largePaleDamageRatio;

		public float creepyDamageRatio;

		public float creepyBabyDamageRatio;

		public float regularStructureDamageRatio;

		public float creepyStructureDamageRatio;

		public float paintedDamageRatio;

		public float skinMaskDamageRatio;

		public float skinnyHealthRatio;

		public float paleSkinnyHealthRatio;

		public float regularHealthRatio;

		public float leaderHealthRatio;

		public float largePaleHealthRatio;

		public float creepyHealthRatio;

		public float creepyBabyHealthRatio;

		public float paintedHealthAmount;

		public float skinMaskHealthAmount;

		public float aiAttackChanceRatio;

		public float aiFollowUpAfterAttackRatio;

		public float playerSearchRadiusRadio;

		public float skinnySpawnAmountRatio;

		public float regularSpawnAmountRatio;

		public float largePaleSpawnAmountRatio;

		public float creepySpawnAmountRatio;

		public float fireDamageRatio;

		public float fireDamageCreepyRatio;

		public float heavyAttackKnockDownChance;

		public float firemanThrowTimeRatio;
	}
}
