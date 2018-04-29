using System;
using Bolt;
using TheForest.Utils;
using UnityEngine;


public class playerEnemyAmountManager : MonoBehaviour
{
	
	private void Update()
	{
		if (Time.time > this.updateRate)
		{
			this.updateCloseEnemies();
			this.updateRate = Time.time + 3f;
		}
	}

	
	private void updateCloseEnemies()
	{
		if (LocalPlayer.Transform && LocalPlayer.IsInCaves)
		{
			return;
		}
		Scene.SceneTracker.closeEnemies.RemoveAll((GameObject o) => o == null);
		if (Scene.SceneTracker.closeEnemies.Count > 6)
		{
			for (int i = 6; i < Scene.SceneTracker.closeEnemies.Count; i++)
			{
				if (Scene.SceneTracker.closeEnemies[i] && Scene.SceneTracker.closeEnemies[i].activeSelf)
				{
					if (BoltNetwork.isClient)
					{
						enemyRunAwayOverride enemyRunAwayOverride = enemyRunAwayOverride.Create(GlobalTargets.OnlyServer);
						enemyRunAwayOverride.target = Scene.SceneTracker.closeEnemies[i].GetComponent<BoltEntity>();
						enemyRunAwayOverride.Send();
					}
					else
					{
						Scene.SceneTracker.closeEnemies[i].SendMessage("setRunAwayFromPlayer", SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
	}

	
	private float updateRate = 3f;
}
