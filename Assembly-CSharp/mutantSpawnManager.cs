using System;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UnityEngine;

public class mutantSpawnManager : MonoBehaviour
{
	private void Start()
	{
		float repeatRate = 160f;
		this.setMaxAmounts();
		if (!base.IsInvoking("addToMutantAmounts"))
		{
			base.InvokeRepeating("addToMutantAmounts", 1f, repeatRate);
		}
	}

	private void OnDeserialized()
	{
		float repeatRate = 220f;
		if (!base.IsInvoking("addToMutantAmounts"))
		{
			base.InvokeRepeating("addToMutantAmounts", 1f, repeatRate);
		}
	}

	public void offsetSleepAmounts()
	{
		if (Scene.MutantControler.hordeModeActive)
		{
			return;
		}
		if (this.offsetSkinny < 0)
		{
			this.offsetSkinny += 2;
		}
		if (this.offsetSkinnyPale < 0)
		{
			this.offsetSkinnyPale += 2;
		}
		if (this.offsetRegular < 0)
		{
			this.offsetRegular += 2;
		}
		if (this.offsetPale < 0)
		{
			this.offsetPale += 2;
		}
		if (this.offsetCreepy < 0)
		{
			this.offsetCreepy += 2;
		}
		if (this.offsetSkinned < 0)
		{
			this.offsetSkinned += 2;
		}
	}

	private void setMaxAmounts()
	{
		if (Scene.MutantControler.hordeModeActive)
		{
			return;
		}
		this.maxSkinny = this.desiredSkinny;
		this.maxSkinnyPale = this.desiredSkinnyPale;
		this.maxRegular = this.desiredRegular;
		this.maxPainted = this.desiredPainted;
		this.maxPale = this.desiredPale;
		this.maxCreepy = this.desiredCreepy;
		this.maxSkinned = this.desiredSkinned;
		this.desiredSkinny += this.offsetSkinny;
		this.desiredSkinnyPale += this.offsetSkinnyPale;
		this.desiredRegular += this.offsetRegular;
		this.desiredPainted += this.offsetPainted;
		this.desiredPale += this.offsetPale;
		this.desiredCreepy += this.offsetCreepy;
		this.desiredSkinned += this.offsetSkinned;
		if (this.desiredSkinny < 0)
		{
			this.maxSkinny = 0;
		}
		if (this.desiredSkinnyPale < 0)
		{
			this.maxSkinnyPale = 0;
		}
		if (this.desiredRegular < 0)
		{
			this.maxRegular = 0;
		}
		if (this.desiredPainted < 0)
		{
			this.maxPainted = 0;
		}
		if (this.desiredPale < 0)
		{
			this.maxPale = 0;
		}
		if (this.desiredCreepy < 0)
		{
			this.maxCreepy = 0;
		}
		if (this.desiredSkinned < 0)
		{
			this.maxSkinned = 0;
		}
	}

	private void addToMutantAmounts()
	{
		if (Scene.MutantControler.hordeModeActive)
		{
			return;
		}
		if (this.desiredSkinny < this.maxSkinny)
		{
			this.desiredSkinny++;
		}
		if (this.desiredSkinnyPale < this.maxSkinnyPale)
		{
			this.desiredSkinnyPale++;
		}
		if (this.desiredRegular < this.maxRegular)
		{
			this.desiredRegular++;
		}
		if (this.desiredPainted < this.maxPainted)
		{
			this.desiredPainted++;
		}
		if (this.desiredPale < this.maxPale)
		{
			this.desiredPale++;
		}
		if (this.desiredCreepy < this.maxCreepy)
		{
			this.desiredCreepy++;
		}
		if (this.desiredSkinned < this.maxSkinned)
		{
			this.desiredSkinned++;
		}
		if (this.offsetSkinny < 0)
		{
			this.offsetSkinny++;
		}
		if (this.offsetSkinnyPale < 0)
		{
			this.offsetSkinnyPale++;
		}
		if (this.offsetRegular < 0)
		{
			this.offsetRegular++;
		}
		if (this.offsetPainted < 0)
		{
			this.offsetPainted++;
		}
		if (this.offsetPale < 0)
		{
			this.offsetPale++;
		}
		if (this.offsetCreepy < 0)
		{
			this.offsetCreepy++;
		}
		if (this.offsetSkinned < 0)
		{
			this.offsetSkinned++;
		}
	}

	public int countAllSpawns()
	{
		this.numDesiredSpawns = this.desiredCreepy + this.desiredPale + this.desiredRegular + this.desiredSkinny + this.desiredSkinnyPale + this.desiredPainted + this.desiredSkinned;
		return this.numDesiredSpawns;
	}

	public void setMutantSpawnAmounts()
	{
		if (Scene.MutantControler.hordeModeActive)
		{
			return;
		}
		if (Clock.Day == 0)
		{
			this.setAmountDay0();
		}
		else if (Clock.Day == 1)
		{
			this.setAmountDay1();
		}
		else if (Clock.Day == 2)
		{
			this.setAmountDay2();
		}
		else if (Clock.Day == 3)
		{
			this.setAmountDay3();
		}
		else if (Clock.Day == 4)
		{
			this.setAmountDay4();
		}
		else if (Clock.Day == 5)
		{
			this.setAmountDay5();
		}
		else if (Clock.Day == 6)
		{
			this.setAmountDay6();
		}
		else if (Clock.Day == 7)
		{
			this.setAmountDay7();
		}
		else if (Clock.Day == 8)
		{
			this.setAmountDay8();
		}
		else if (Clock.Day == 9)
		{
			this.setAmountDay9();
		}
		else if (Clock.Day >= 10 && Clock.Day < 25)
		{
			this.setAmountDay10();
		}
		else if (Clock.Day >= 15 && Clock.Day < 22)
		{
			this.setAmountDay15();
		}
		else if (Clock.Day >= 22)
		{
			this.setAmountDay22();
		}
	}

	private void setAmountDay0()
	{
		if (!Clock.Dark)
		{
			if (GameSetup.IsHardMode)
			{
				this.desiredSkinny = 4;
			}
			else
			{
				this.desiredSkinny = 6;
			}
			this.desiredSkinnyPale = 0;
			if (GameSetup.IsHardMode)
			{
				this.desiredRegular = 2;
			}
			else
			{
				this.desiredRegular = 0;
			}
			this.desiredPale = 0;
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		else
		{
			if (GameSetup.IsHardMode)
			{
				this.desiredSkinny = 4;
			}
			else
			{
				this.desiredSkinny = 6;
			}
			this.desiredSkinnyPale = 0;
			if (GameSetup.IsHardMode)
			{
				this.desiredRegular = 2;
			}
			else
			{
				this.desiredRegular = 0;
			}
			this.desiredPale = 0;
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		this.maxSleepingSpawns = 3;
		this.setMaxAmounts();
	}

	private void setAmountDay1()
	{
		if (!Clock.Dark)
		{
			if (GameSetup.IsHardMode)
			{
				this.desiredSkinny = 4;
			}
			else
			{
				this.desiredSkinny = 6;
			}
			this.desiredSkinnyPale = 0;
			if (GameSetup.IsHardMode)
			{
				this.desiredRegular = 2;
			}
			else
			{
				this.desiredRegular = 0;
			}
			this.desiredPale = 0;
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		else
		{
			this.desiredSkinny = 6;
			this.desiredSkinnyPale = 0;
			this.desiredRegular = 2;
			this.desiredPale = 0;
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		this.maxSleepingSpawns = 3;
		this.setMaxAmounts();
	}

	private void setAmountDay2()
	{
		if (!Clock.Dark)
		{
			if (GameSetup.IsHardMode)
			{
				this.desiredSkinny = 3;
			}
			else
			{
				this.desiredSkinny = 5;
			}
			this.desiredSkinnyPale = 0;
			if (GameSetup.IsHardMode)
			{
				this.desiredRegular = 3;
			}
			else
			{
				this.desiredRegular = 2;
			}
			this.desiredPale = 0;
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		else
		{
			if (GameSetup.IsHardMode)
			{
				this.desiredSkinny = 3;
			}
			else
			{
				this.desiredSkinny = 5;
			}
			this.desiredSkinnyPale = 0;
			if (GameSetup.IsHardMode)
			{
				this.desiredRegular = 3;
			}
			else
			{
				this.desiredRegular = 2;
			}
			this.desiredPale = 0;
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		this.maxSleepingSpawns = 3;
		this.setMaxAmounts();
	}

	private void setAmountDay3()
	{
		if (!Clock.Dark)
		{
			this.desiredSkinny = 3;
			this.desiredSkinnyPale = 0;
			this.desiredRegular = 3;
			this.desiredPale = 0;
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		else
		{
			this.desiredSkinny = 3;
			this.desiredSkinnyPale = 0;
			this.desiredRegular = 3;
			this.desiredPale = 0;
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		this.maxSleepingSpawns = 3;
		this.setMaxAmounts();
	}

	private void setAmountDay4()
	{
		if (!Clock.Dark)
		{
			this.desiredSkinny = 2;
			this.desiredSkinnyPale = 0;
			this.desiredRegular = 6;
			this.desiredPale = 0;
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		else
		{
			this.desiredSkinny = 1;
			this.desiredSkinnyPale = 0;
			this.desiredRegular = 4;
			this.desiredPale = 0;
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		this.maxSleepingSpawns = 3;
		this.setMaxAmounts();
	}

	private void setAmountDay5()
	{
		if (!Clock.Dark)
		{
			this.desiredSkinny = 1;
			this.desiredSkinnyPale = 0;
			this.desiredRegular = 6;
			this.desiredPale = 0;
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		else
		{
			this.desiredSkinny = 1;
			this.desiredSkinnyPale = 3;
			this.desiredRegular = 4;
			this.desiredPale = 0;
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		this.maxSleepingSpawns = 3;
		this.setMaxAmounts();
	}

	private void setAmountDay6()
	{
		if (!Clock.Dark)
		{
			this.desiredSkinny = 1;
			this.desiredSkinnyPale = 3;
			this.desiredRegular = 7;
			this.desiredPale = Mathf.FloorToInt(1f * GameSettings.Ai.largePaleSpawnAmountRatio);
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		else
		{
			this.desiredSkinny = 1;
			this.desiredSkinnyPale = 2;
			this.desiredRegular = 4;
			this.desiredPale = Mathf.FloorToInt(2f * GameSettings.Ai.largePaleSpawnAmountRatio);
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		this.maxSleepingSpawns = 3;
		this.setMaxAmounts();
	}

	private void setAmountDay7()
	{
		if (!Clock.Dark)
		{
			this.desiredSkinny = 1;
			this.desiredSkinnyPale = 1;
			this.desiredRegular = 7;
			this.desiredPale = Mathf.FloorToInt(2f * GameSettings.Ai.largePaleSpawnAmountRatio);
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		else
		{
			this.desiredSkinny = 1;
			this.desiredSkinnyPale = 1;
			this.desiredRegular = 4;
			this.desiredPale = Mathf.FloorToInt(3f * GameSettings.Ai.largePaleSpawnAmountRatio);
			this.desiredCreepy = Mathf.FloorToInt(1f * GameSettings.Ai.creepySpawnAmountRatio);
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		this.maxSleepingSpawns = 2;
		this.setMaxAmounts();
	}

	private void setAmountDay8()
	{
		if (!Clock.Dark)
		{
			this.desiredSkinny = 0;
			this.desiredSkinnyPale = Mathf.FloorToInt(3f * GameSettings.Ai.skinnySpawnAmountRatio);
			this.desiredRegular = 4;
			this.desiredPale = Mathf.FloorToInt(2f * GameSettings.Ai.largePaleSpawnAmountRatio);
			this.desiredCreepy = 0;
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		else
		{
			this.desiredSkinny = 0;
			this.desiredSkinnyPale = 1;
			this.desiredRegular = 4;
			this.desiredPale = Mathf.FloorToInt(3f * GameSettings.Ai.largePaleSpawnAmountRatio);
			this.desiredCreepy = Mathf.FloorToInt(1f * GameSettings.Ai.creepySpawnAmountRatio);
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		this.maxSleepingSpawns = 2;
		this.setMaxAmounts();
	}

	private void setAmountDay9()
	{
		if (!Clock.Dark)
		{
			this.desiredSkinny = 1;
			this.desiredSkinnyPale = 1;
			this.desiredRegular = Mathf.FloorToInt(6f * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredPale = Mathf.FloorToInt(3f * GameSettings.Ai.largePaleSpawnAmountRatio);
			this.desiredCreepy = Mathf.FloorToInt(2f * GameSettings.Ai.creepySpawnAmountRatio);
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		else
		{
			this.desiredSkinny = 1;
			this.desiredSkinnyPale = 1;
			this.desiredRegular = Mathf.FloorToInt(5f * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredPale = Mathf.FloorToInt(3f * GameSettings.Ai.largePaleSpawnAmountRatio);
			this.desiredCreepy = Mathf.FloorToInt(2f * GameSettings.Ai.creepySpawnAmountRatio);
			this.desiredPainted = 0;
			this.desiredSkinned = 0;
		}
		this.maxSleepingSpawns = 2;
		this.setMaxAmounts();
	}

	private void setAmountDay10()
	{
		if (!Clock.Dark)
		{
			this.desiredSkinny = Mathf.FloorToInt((float)UnityEngine.Random.Range(0, 2) * GameSettings.Ai.skinnySpawnAmountRatio);
			this.desiredRegular = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 6) * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredSkinnyPale = Mathf.FloorToInt((float)UnityEngine.Random.Range(0, 2) * GameSettings.Ai.skinnySpawnAmountRatio);
			this.desiredPale = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 3) * GameSettings.Ai.largePaleSpawnAmountRatio);
			this.desiredCreepy = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 3) * GameSettings.Ai.creepySpawnAmountRatio);
		}
		else
		{
			this.desiredSkinny = Mathf.FloorToInt((float)UnityEngine.Random.Range(0, 2) * GameSettings.Ai.skinnySpawnAmountRatio);
			this.desiredRegular = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 6) * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredSkinnyPale = Mathf.FloorToInt((float)UnityEngine.Random.Range(0, 2) * GameSettings.Ai.skinnySpawnAmountRatio);
			this.desiredPale = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 3) * GameSettings.Ai.largePaleSpawnAmountRatio);
			this.desiredCreepy = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 3) * GameSettings.Ai.creepySpawnAmountRatio);
		}
		this.maxSleepingSpawns = 1;
		this.setMaxAmounts();
	}

	private void setAmountDay15()
	{
		if (!Clock.Dark)
		{
			this.desiredSkinny = Mathf.FloorToInt((float)UnityEngine.Random.Range(0, 2) * GameSettings.Ai.skinnySpawnAmountRatio);
			this.desiredRegular = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 3) * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredPainted = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 5) * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredSkinnyPale = Mathf.FloorToInt((float)UnityEngine.Random.Range(0, 2) * GameSettings.Ai.skinnySpawnAmountRatio);
			this.desiredPale = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 3) * GameSettings.Ai.largePaleSpawnAmountRatio);
			this.desiredCreepy = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 3) * GameSettings.Ai.creepySpawnAmountRatio);
		}
		else
		{
			this.desiredSkinny = Mathf.FloorToInt((float)UnityEngine.Random.Range(0, 2) * GameSettings.Ai.skinnySpawnAmountRatio);
			this.desiredRegular = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 3) * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredPainted = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 6) * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredSkinnyPale = Mathf.FloorToInt((float)UnityEngine.Random.Range(0, 2) * GameSettings.Ai.skinnySpawnAmountRatio);
			this.desiredPale = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 3) * GameSettings.Ai.largePaleSpawnAmountRatio);
			this.desiredCreepy = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 3) * GameSettings.Ai.creepySpawnAmountRatio);
		}
		this.maxSleepingSpawns = 0;
		this.setMaxAmounts();
	}

	private void setAmountDay22()
	{
		if (!Clock.Dark)
		{
			this.desiredSkinned = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 4) * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredRegular = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 2) * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredPainted = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 4) * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredSkinnyPale = 0;
			this.desiredPale = 0;
			this.desiredCreepy = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 3) * GameSettings.Ai.creepySpawnAmountRatio);
			this.desiredSkinny = Mathf.FloorToInt((float)UnityEngine.Random.Range(0, 3) * GameSettings.Ai.skinnySpawnAmountRatio);
		}
		else
		{
			this.desiredSkinned = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 4) * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredRegular = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 2) * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredPainted = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 5) * GameSettings.Ai.regularSpawnAmountRatio);
			this.desiredSkinnyPale = 0;
			this.desiredPale = 0;
			this.desiredCreepy = Mathf.FloorToInt((float)UnityEngine.Random.Range(1, 3) * GameSettings.Ai.creepySpawnAmountRatio);
			this.desiredSkinny = Mathf.FloorToInt((float)UnityEngine.Random.Range(0, 3) * GameSettings.Ai.skinnySpawnAmountRatio);
		}
		this.maxSleepingSpawns = 0;
		this.setMaxAmounts();
	}

	public int desiredSkinny;

	public int desiredSkinnyPale;

	public int desiredRegular;

	public int desiredPainted;

	public int desiredPale;

	public int desiredCreepy;

	public int desiredSkinned;

	[SerializeThis]
	public int offsetSkinny;

	[SerializeThis]
	public int offsetSkinnyPale;

	[SerializeThis]
	public int offsetRegular;

	[SerializeThis]
	public int offsetPale;

	[SerializeThis]
	public int offsetCreepy;

	[SerializeThis]
	public int offsetPainted;

	[SerializeThis]
	public int offsetSkinned;

	public int maxSkinny;

	public int maxSkinnyPale;

	public int maxRegular;

	public int maxPainted;

	public int maxPale;

	public int maxCreepy;

	public int maxSkinned;

	public int maxSleepingSpawns;

	public int numDesiredSpawns;
}
