using System;
using Steamworks;
using UnityEngine;

namespace TheForest.UI
{
	
	public class UsernameLabel : MonoBehaviour
	{
		
		private void OnEnable()
		{
			UILabel component = base.GetComponent<UILabel>();
			if (component)
			{
				try
				{
					string personaName = SteamFriends.GetPersonaName();
					component.text = personaName;
				}
				catch (Exception ex)
				{
					component.text = "Anonymous";
				}
			}
		}
	}
}
