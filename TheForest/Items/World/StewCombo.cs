using System;
using UnityEngine;

namespace TheForest.Items.World
{
	
	public static class StewCombo
	{
		
		public static void SetIngredients(int meats, int mushrooms, int herbs, bool water)
		{
			if (meats > 0 && meats == mushrooms && meats == herbs)
			{
				StewCombo.HealthRatio = 1f;
				StewCombo.EnergyRatio = 1f + Mathf.Clamp((float)mushrooms * 0.05f, 0.05f, 0.3f);
				StewCombo.FullnessRatio = 1f + Mathf.Clamp((float)mushrooms * 0.05f, 0.05f, 0.3f);
				StewCombo.Log("balanced");
			}
			else if (herbs > 0 && meats == mushrooms && herbs < meats && herbs < mushrooms)
			{
				StewCombo.HealthRatio = 1f;
				StewCombo.EnergyRatio = 1f + Mathf.Clamp((float)mushrooms * 0.1f, 0.1f, 0.4f);
				StewCombo.FullnessRatio = 1f + Mathf.Clamp((float)meats * 0.1f, 0.1f, 0.4f);
				StewCombo.Log("realistic");
			}
			else if (herbs > 0 && meats > 0 && water && herbs < mushrooms && meats < mushrooms)
			{
				StewCombo.HealthRatio = 1f;
				StewCombo.EnergyRatio = 1f + Mathf.Clamp((float)mushrooms * 0.1f, 0.1f, 0.4f);
				StewCombo.FullnessRatio = 1f + Mathf.Clamp((float)meats * 0.05f, 0.05f, 0.2f);
				StewCombo.Log("mushroom sauce");
			}
			else if (mushrooms == 0 && !water && meats > 0 && meats == herbs)
			{
				StewCombo.HealthRatio = 0.8f;
				StewCombo.EnergyRatio = 1f;
				StewCombo.FullnessRatio = 1f + Mathf.Clamp((float)meats * 0.15f, 0.15f, 0.5f);
				StewCombo.Log("BBQ");
			}
			else if (meats == 0 && meats == 0 && mushrooms > 0 && herbs > 0 && water)
			{
				StewCombo.HealthRatio = 1f + Mathf.Clamp((float)(herbs + herbs) * 0.05f, 0.05f, 0.4f);
				StewCombo.EnergyRatio = 1f + Mathf.Clamp((float)(herbs + herbs) * 0.05f, 0.05f, 0.4f);
				StewCombo.FullnessRatio = 0.8f;
				StewCombo.Log("ointment");
			}
			else if (meats == 0 && mushrooms == 0 && herbs > 0 && water)
			{
				StewCombo.HealthRatio = 1f + Mathf.Clamp((float)herbs * 0.2f, 0.2f, 1f);
				StewCombo.EnergyRatio = 0.8f;
				StewCombo.FullnessRatio = 0.5f;
				StewCombo.Log("tisane");
			}
			else
			{
				StewCombo.HealthRatio = 1f;
				StewCombo.EnergyRatio = 1f;
				StewCombo.FullnessRatio = 1f;
			}
		}

		
		private static void Log(string name)
		{
			if (StewCombo.Verbose)
			{
				Debug.Log(string.Concat(new object[]
				{
					"Stew combo: ",
					name,
					", H:",
					StewCombo.HealthRatio,
					" E:",
					StewCombo.EnergyRatio,
					" F:",
					StewCombo.FullnessRatio
				}));
			}
		}

		
		
		
		public static float HealthRatio { get; private set; }

		
		
		
		public static float EnergyRatio { get; private set; }

		
		
		
		public static float FullnessRatio { get; private set; }

		
		
		
		public static bool Verbose { get; set; }
	}
}
