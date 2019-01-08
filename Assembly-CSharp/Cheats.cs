using System;
using TheForest;
using TheForest.Modding.Bridge.Interfaces;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

public class Cheats : MonoBehaviour
{
	public static bool NoEnemies
	{
		get
		{
			return Cheats.NoEnemiesInternal || (!GameSetup.IsCreativeGame && GameSetup.IsPeacefulMode) || (GameSetup.IsCreativeGame && !PlayerPreferences.AllowEnemiesCreative);
		}
		set
		{
			Cheats.NoEnemiesInternal = value;
		}
	}

	public static bool Allowed { get; private set; }

	private void Start()
	{
		Cheats.Allowed = true;
		this.MeatModeCode = new string[]
		{
			"m",
			"e",
			"a",
			"t",
			"m",
			"o",
			"d",
			"e"
		};
		this.VeganModeCode = new string[]
		{
			"v",
			"e",
			"g",
			"a",
			"n",
			"m",
			"o",
			"d",
			"e"
		};
		this.VegetarianModeCode = new string[]
		{
			"v",
			"e",
			"g",
			"e",
			"t",
			"a",
			"r",
			"i",
			"a",
			"n",
			"m",
			"o",
			"d",
			"e"
		};
		this.RawMeatModeCode = new string[]
		{
			"r",
			"a",
			"w",
			"m",
			"e",
			"a",
			"t",
			"m",
			"o",
			"d",
			"e"
		};
		this.resetHolesCode = new string[]
		{
			"w",
			"o",
			"o",
			"d",
			"p",
			"a",
			"s",
			"t",
			"e"
		};
		this.debugConsoleCode = new string[]
		{
			"d",
			"e",
			"v",
			"e",
			"l",
			"o",
			"p",
			"e",
			"r",
			"m",
			"o",
			"d",
			"e",
			"o",
			"n"
		};
		switch (PlayerPrefs.GetInt("Mode", 0))
		{
		case 0:
			Cheats.NoEnemiesDuringDay = false;
			Cheats.NoEnemies = false;
			Cheats.PermaDeath = false;
			break;
		case 1:
			Cheats.NoEnemiesDuringDay = false;
			Cheats.NoEnemies = true;
			Cheats.PermaDeath = false;
			break;
		case 2:
			Cheats.NoEnemiesDuringDay = true;
			Cheats.NoEnemies = false;
			Cheats.PermaDeath = false;
			break;
		case 3:
			Cheats.NoEnemiesDuringDay = false;
			Cheats.NoEnemies = false;
			Cheats.PermaDeath = true;
			break;
		}
		Cheats.ResetHoles = false;
	}

	private void Update()
	{
		if (UnityEngine.Input.anyKeyDown)
		{
			if (UnityEngine.Input.GetKeyDown(this.MeatModeCode[this.MeatModeIndex]))
			{
				this.MeatModeIndex++;
			}
			else
			{
				this.MeatModeIndex = 0;
			}
			if (UnityEngine.Input.GetKeyDown(this.RawMeatModeCode[this.RawMeatModeIndex]))
			{
				this.RawMeatModeIndex++;
				this.MeatModeIndex = 0;
			}
			else
			{
				this.RawMeatModeIndex = 0;
			}
			if (UnityEngine.Input.GetKeyDown(this.VeganModeCode[this.VeganModeIndex]))
			{
				this.VeganModeIndex++;
			}
			else
			{
				this.VeganModeIndex = 0;
			}
			if (UnityEngine.Input.GetKeyDown(this.VegetarianModeCode[this.VegetarianModeIndex]))
			{
				this.VegetarianModeIndex++;
			}
			else
			{
				this.VegetarianModeIndex = 0;
			}
			if (UnityEngine.Input.GetKeyDown(this.resetHolesCode[this.ResetHolesIndex]))
			{
				this.ResetHolesIndex++;
			}
			else
			{
				this.ResetHolesIndex = 0;
			}
			if (UnityEngine.Input.GetKeyDown(this.debugConsoleCode[this.DebugConsoleIndex]))
			{
				this.DebugConsoleIndex++;
			}
			else
			{
				this.DebugConsoleIndex = 0;
			}
		}
		if (this.MeatModeIndex == this.MeatModeCode.Length)
		{
			Debug.Log("Normal mode set");
			Cheats.NoEnemiesDuringDay = false;
			Cheats.NoEnemies = false;
			Cheats.PermaDeath = false;
			this.MeatModeIndex = 0;
			PlayerPrefs.SetInt("Mode", 0);
			PlayerPrefs.Save();
		}
		if (this.VeganModeIndex == this.VeganModeCode.Length)
		{
			Debug.Log("Vegan mode set");
			Cheats.NoEnemiesDuringDay = false;
			Cheats.NoEnemies = true;
			Cheats.PermaDeath = false;
			this.VeganModeIndex = 0;
			PlayerPrefs.SetInt("Mode", 1);
			PlayerPrefs.Save();
		}
		if (this.VegetarianModeIndex == this.VegetarianModeCode.Length)
		{
			Debug.Log("Vegetarian mode set");
			Cheats.NoEnemiesDuringDay = true;
			Cheats.NoEnemies = false;
			Cheats.PermaDeath = false;
			this.VegetarianModeIndex = 0;
			PlayerPrefs.SetInt("Mode", 2);
			PlayerPrefs.Save();
		}
		if (this.RawMeatModeIndex == this.RawMeatModeCode.Length)
		{
			Debug.Log("PermaDeath mode set");
			Cheats.NoEnemiesDuringDay = false;
			Cheats.NoEnemies = false;
			Cheats.PermaDeath = true;
			this.RawMeatModeIndex = 0;
			PlayerPrefs.SetInt("Mode", 3);
			PlayerPrefs.Save();
		}
		if (this.ResetHolesIndex == this.resetHolesCode.Length)
		{
			Cheats.ResetHoles = true;
			this.ResetHolesIndex = 0;
			Debug.Log("WoodPaste " + ((!Cheats.ResetHoles) ? "unset" : "set"));
		}
		if (this.DebugConsoleIndex == this.debugConsoleCode.Length)
		{
			Cheats.DebugConsole = !Cheats.DebugConsole;
			this.DebugConsoleIndex = 0;
			DebugConsole exists = UnityEngine.Object.FindObjectOfType<DebugConsole>();
			if (Cheats.DebugConsole)
			{
				if (!exists)
				{
					GameObject gameObject = new GameObject("DebugConsole");
					gameObject.AddComponent<DebugConsole>();
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
					Debug.Log("Enabling debug console");
				}
			}
			else if (exists)
			{
				UnityEngine.Object.Destroy(this);
				Debug.Log("Disabling debug console");
			}
		}
	}

	public static void SetAllowed(bool onoff)
	{
		Cheats.Allowed = onoff;
		EventRegistry.Game.Publish(TfEvent.CheatAllowedSet, onoff);
	}

	public static Cheats.CheatsBridge Bridge = new Cheats.CheatsBridge();

	private string[] VeganModeCode;

	private string[] VegetarianModeCode;

	private string[] MeatModeCode;

	private string[] RawMeatModeCode;

	private string[] creativeCode;

	private string[] resetHolesCode;

	private string[] debugConsoleCode;

	private int VeganModeIndex;

	private int VegetarianModeIndex;

	private int IronForestModeIndex;

	private int MeatModeIndex;

	private int RawMeatModeIndex;

	private int ResetHolesIndex;

	private int DebugConsoleIndex;

	public static bool NoEnemiesInternal;

	public static bool NoEnemiesDuringDay;

	public static bool Creative;

	public static bool GodMode;

	public static bool InfiniteEnergy;

	public static bool PermaDeath;

	public static bool ResetHoles;

	public static bool NoSurvival;

	public static bool DebugConsole;

	public static bool UnlimitedHairspray;

	public class CheatsBridge : ICheatsBridge
	{
		public void SetCreative(bool onoff)
		{
			Cheats.Creative = onoff;
		}

		public void SetGodMode(bool onoff)
		{
			Cheats.GodMode = onoff;
		}

		public void SetInfiniteEnergy(bool onoff)
		{
			Cheats.InfiniteEnergy = onoff;
		}

		public void SetPermaDeath(bool onoff)
		{
			Cheats.PermaDeath = onoff;
		}

		public void SetNoSurvival(bool onoff)
		{
			Cheats.NoSurvival = onoff;
		}

		public void SetUnlimitedHairspray(bool onoff)
		{
			Cheats.UnlimitedHairspray = onoff;
		}
	}
}
