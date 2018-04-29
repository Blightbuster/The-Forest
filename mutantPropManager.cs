using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using PathologicalGames;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


public class mutantPropManager : EntityBehaviour
{
	
	private void Awake()
	{
		this.bloodPropertyBlock = new MaterialPropertyBlock();
		this.skinRenderer = this.MyBody.GetComponent<SkinnedMeshRenderer>();
		this.ai = base.GetComponent<mutantAI>();
		this.setup = base.GetComponent<mutantScriptSetup>();
		if (this.MyBodyParts.Length > 0)
		{
			this.bodyParentGo = this.MyBodyParts[0].transform.parent.gameObject;
		}
		if (this.MySkinnyParts.Length > 0)
		{
			this.skinnyParentGo = this.MySkinnyParts[0].transform.parent.gameObject;
		}
		if (!this.female)
		{
			this.MaterialLookup.Add(this.FireManMat.name, this.FireManMat);
			this.MaterialLookup.Add(this.DynamiteMat.name, this.DynamiteMat);
			this.MaterialLookup.Add(this.paleMat.name, this.paleMat);
			this.MaterialLookup.Add(this.cannibalMat.name, this.cannibalMat);
			this.MaterialLookup.Add(this.skinnyMat.name, this.skinnyMat);
			foreach (Material material in this.material)
			{
				if (!this.MaterialLookup.ContainsKey(material.name))
				{
					this.MaterialLookup.Add(material.name, material);
				}
			}
		}
	}

	
	public void setSkinnedMutant(bool onoff)
	{
		this.skinned = onoff;
		this.skinnedHeadMask.SetActive(onoff);
	}

	
	public void setRegularMale(int dice)
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		this.LeaderDice = UnityEngine.Random.Range(0, 3);
		this.regularMaleDice = UnityEngine.Random.Range(0, 5);
		if (this.dummySetup)
		{
			this.regularMaleDice = dice;
		}
		if (this.leaderCape)
		{
			this.leaderCape.SetActive(false);
		}
		if (this.leaderCape1)
		{
			this.leaderCape1.SetActive(false);
		}
		if (this.leaderCape2)
		{
			this.leaderCape2.SetActive(false);
		}
		if (this.dummySetup)
		{
			this.Club.SetActive(false);
		}
		if (this.regularStick)
		{
			this.regularStick.SetActive(false);
		}
		if (!this.dummySetup)
		{
			for (int i = 0; i < this.regularWeapons.Length; i++)
			{
				this.regularWeapons[i].SetActive(false);
			}
			for (int j = 0; j < this.advancedWeapons.Length; j++)
			{
				this.advancedWeapons[j].SetActive(false);
			}
			this.regularStick.SetActive(false);
		}
		switch (this.regularMaleDice)
		{
		case 0:
			this.hats[0].SetActive(false);
			this.props1Go.SetActive(true);
			this.props2Go.SetActive(false);
			this.props3Go.SetActive(false);
			if (!this.dummySetup && UnityEngine.Random.value > 0.65f && Clock.Day > 15)
			{
				this.enableRegularWeapon(-1);
			}
			this.SetSkin(this.material[0]);
			break;
		case 1:
			this.hats[0].SetActive(true);
			this.props1Go.SetActive(false);
			this.props2Go.SetActive(true);
			this.props3Go.SetActive(false);
			if (!this.dummySetup && UnityEngine.Random.value > 0.65f && Clock.Day > 15)
			{
				this.enableRegularWeapon(-1);
			}
			this.SetSkin(this.material[1]);
			break;
		case 2:
			this.hats[0].SetActive(false);
			this.props1Go.SetActive(false);
			this.props2Go.SetActive(false);
			this.props3Go.SetActive(true);
			if (!this.dummySetup && UnityEngine.Random.value > 0.65f && Clock.Day > 15)
			{
				this.enableRegularWeapon(-1);
			}
			this.SetSkin(this.material[2]);
			break;
		case 3:
			this.hats[0].SetActive(false);
			this.props1Go.SetActive(false);
			this.props2Go.SetActive(false);
			this.props3Go.SetActive(true);
			if (!this.dummySetup && UnityEngine.Random.value > 0.65f && Clock.Day > 15)
			{
				this.enableRegularWeapon(-1);
			}
			this.SetSkin(this.material[3]);
			break;
		case 4:
			this.hats[0].SetActive(false);
			this.props1Go.SetActive(false);
			this.props2Go.SetActive(true);
			this.props3Go.SetActive(false);
			if (!this.dummySetup && UnityEngine.Random.value > 0.65f && Clock.Day > 15)
			{
				this.enableRegularWeapon(-1);
			}
			this.SetSkin(this.material[0]);
			break;
		}
		this.setSkinColor();
		if (!this.dummySetup)
		{
			if (!this.hasRegularWeapon && !this.hasAdvancedWeapon)
			{
				this.Club.SetActive(true);
			}
			else
			{
				this.Club.SetActive(false);
			}
		}
	}

	
	public void setPaintedMale(int dice)
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		this.LeaderDice = UnityEngine.Random.Range(0, 3);
		this.regularMaleDice = UnityEngine.Random.Range(0, 3);
		if (this.dummySetup)
		{
			this.regularMaleDice = dice;
		}
		if (this.leaderCape)
		{
			this.leaderCape.SetActive(false);
		}
		if (this.leaderCape1)
		{
			this.leaderCape1.SetActive(false);
		}
		if (this.leaderCape2)
		{
			this.leaderCape2.SetActive(false);
		}
		if (this.dummySetup)
		{
			this.Club.SetActive(false);
		}
		if (!this.dummySetup)
		{
			for (int i = 0; i < this.regularWeapons.Length; i++)
			{
				this.regularWeapons[i].SetActive(false);
			}
			for (int j = 0; j < this.advancedWeapons.Length; j++)
			{
				this.advancedWeapons[j].SetActive(false);
			}
			this.regularStick.SetActive(false);
		}
		if (this.dummySetup)
		{
			this.skinnyParentGo.SetActive(false);
			this.bodyParentGo.SetActive(true);
		}
		else
		{
			this.SetBlendShapeWeight(28, 0);
		}
		int num = this.regularMaleDice;
		if (num != 0)
		{
			if (num != 1)
			{
				if (num == 2)
				{
					this.hats[0].SetActive(false);
					this.props1Go.SetActive(false);
					this.props2Go.SetActive(false);
					this.props3Go.SetActive(true);
					if (!this.dummySetup && UnityEngine.Random.value > 0.65f)
					{
						this.enableRegularWeapon(-1);
					}
					this.SetSkin(this.paintedTribeMats[0]);
				}
			}
			else
			{
				this.hats[0].SetActive(false);
				this.props1Go.SetActive(false);
				this.props2Go.SetActive(true);
				this.props3Go.SetActive(false);
				if (!this.dummySetup && UnityEngine.Random.value > 0.65f)
				{
					this.enableRegularWeapon(-1);
				}
				this.SetSkin(this.paintedTribeMats[1]);
			}
		}
		else
		{
			this.hats[0].SetActive(false);
			this.props1Go.SetActive(false);
			this.props2Go.SetActive(true);
			this.props3Go.SetActive(false);
			if (!this.dummySetup && UnityEngine.Random.value > 0.65f)
			{
				this.enableRegularWeapon(-1);
			}
			this.SetSkin(this.paintedTribeMats[0]);
		}
		this.setSkinColor();
		if (!this.dummySetup)
		{
			if (this.hasAdvancedWeapon || this.hasRegularWeapon)
			{
				this.Club.SetActive(false);
			}
			else
			{
				this.Club.SetActive(true);
			}
			this.FireStick.SetActive(false);
			this.regularStick.SetActive(false);
		}
	}

	
	private bool enableRegularWeapon(int index)
	{
		if (index == -1)
		{
			index = UnityEngine.Random.Range(0, 3);
		}
		this.regularWeapons[index].SetActive(true);
		HeldItemIdentifier component = this.regularWeapons[index].GetComponent<HeldItemIdentifier>();
		if (component)
		{
			if (SteamDSConfig.isDedicatedServer)
			{
				return true;
			}
			if (LocalPlayer.Inventory && LocalPlayer.Inventory.Owns(component._itemId, true))
			{
				this.hasRegularWeapon = true;
				return true;
			}
		}
		this.regularWeapons[index].SetActive(false);
		return false;
	}

	
	private bool enableAdvancedWeapon(int index)
	{
		if (index == -1)
		{
			index = UnityEngine.Random.Range(0, 3);
		}
		this.advancedWeapons[index].SetActive(true);
		HeldItemIdentifier component = this.advancedWeapons[index].GetComponent<HeldItemIdentifier>();
		if (component)
		{
			if (CoopPeerStarter.DedicatedHost)
			{
				return true;
			}
			if (LocalPlayer.Inventory.Owns(component._itemId, true))
			{
				this.hasAdvancedWeapon = true;
				return true;
			}
		}
		this.advancedWeapons[index].SetActive(false);
		return false;
	}

	
	private void OnDespawned()
	{
		if (this.ai.male && this.skinRenderer && !this.dummySetup)
		{
			this.skinRenderer.SetBlendShapeWeight(28, 0f);
		}
		if (this.leaderCape)
		{
			this.leaderCape.SetActive(false);
		}
		if (this.leaderCape1)
		{
			this.leaderCape1.SetActive(false);
		}
		if (this.leaderCape2)
		{
			this.leaderCape2.SetActive(false);
		}
		if (this.tennisBallBelt)
		{
			this.tennisBallBelt.SetActive(false);
		}
		if (this.dynamiteBelt)
		{
			this.dynamiteBelt.SetActive(false);
		}
		if (this.skinnedHeadMask)
		{
			this.skinnedHeadMask.SetActive(false);
		}
		this.FireMan = false;
		this.dynamiteMan = false;
		this.pale = false;
		this.female = false;
		this.skinned = false;
		this.hasRegularWeapon = false;
		this.hasAdvancedWeapon = false;
	}

	
	public void enableLeaderProps(int dice)
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (this.dummySetup)
		{
			this.LeaderDice = dice;
		}
		if (!this.dummySetup)
		{
			for (int i = 0; i < this.regularWeapons.Length; i++)
			{
				this.regularWeapons[i].SetActive(false);
			}
			for (int j = 0; j < this.advancedWeapons.Length; j++)
			{
				this.advancedWeapons[j].SetActive(false);
			}
		}
		if (this.LeaderDice == 0)
		{
			this.leaderCape.SetActive(true);
			if (!this.dummySetup && Clock.Day > 15)
			{
				this.enableRegularWeapon(0);
			}
		}
		if (this.LeaderDice == 1)
		{
			this.leaderCape1.SetActive(true);
			if (!this.dummySetup && Clock.Day > 15)
			{
				this.enableRegularWeapon(0);
			}
		}
		if (this.LeaderDice == 2)
		{
			this.leaderCape2.SetActive(true);
			if (!this.dummySetup && Clock.Day > 15)
			{
				this.enableRegularWeapon(0);
			}
		}
		if (this.dummySetup)
		{
			this.Club.SetActive(false);
			this.skinnyParentGo.SetActive(false);
			this.bodyParentGo.SetActive(true);
		}
		else
		{
			if (this.ai.painted)
			{
				this.SetSkin(this.paintedLeaderMat);
			}
			this.SetBlendShapeWeight(28, 0);
			this.FireStick.SetActive(false);
			if (this.hasAdvancedWeapon || this.hasRegularWeapon)
			{
				this.Club.SetActive(false);
			}
			else
			{
				this.Club.SetActive(true);
			}
			if (this.regularStick)
			{
				this.regularStick.SetActive(false);
			}
		}
		if (!this.female)
		{
			base.SendMessageUpwards("UpdateProps", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public void setPaintedLeader()
	{
		this.SetSkin(this.paintedLeaderMat);
	}

	
	public void enableFiremanProps()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		this.FireMan = true;
		this.Club.SetActive(false);
		if (this.regularStick)
		{
			this.regularStick.SetActive(false);
		}
		if (this.tennisBallBelt)
		{
			this.tennisBallBelt.SetActive(true);
		}
		if (this.dummySetup)
		{
			this.FireStick.SetActive(false);
		}
		else
		{
			this.FireStick.SetActive(true);
		}
		if (this.props1Go)
		{
			this.props1Go.SetActive(false);
		}
		if (this.props2Go)
		{
			this.props2Go.SetActive(true);
		}
		if (this.props3Go)
		{
			this.props3Go.SetActive(false);
		}
		if (this.dummySetup)
		{
			this.skinnyParentGo.SetActive(false);
			this.bodyParentGo.SetActive(true);
		}
		else
		{
			this.SetBlendShapeWeight(28, 0);
		}
		this.SetSkin(this.FireManMat);
		if (!this.female)
		{
			base.SendMessageUpwards("UpdateProps", SendMessageOptions.DontRequireReceiver);
		}
		this.setSkinColor();
	}

	
	public void enableDefaultProps()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		this.FireStick.SetActive(false);
		if (this.regularStick)
		{
			this.regularStick.SetActive(false);
		}
		if (this.dummySetup)
		{
			this.skinnyParentGo.SetActive(false);
			this.bodyParentGo.SetActive(true);
		}
		else
		{
			this.SetBlendShapeWeight(28, 0);
		}
		if (!this.female)
		{
			base.SendMessageUpwards("UpdateProps", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public void enableMaleSkinnyProps()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		this.resetProps();
		this.Club.SetActive(false);
		this.FireStick.SetActive(false);
		if (UnityEngine.Random.value > 0.6f)
		{
			if (this.regularStick)
			{
				this.regularStick.SetActive(true);
				this.setup.animator.SetBool("skinnyHasStick", true);
			}
		}
		else if (this.regularStick)
		{
			this.regularStick.SetActive(false);
			this.setup.animator.SetBool("skinnyHasStick", false);
		}
		if (this.dummySetup)
		{
			this.skinnyParentGo.SetActive(true);
			this.bodyParentGo.SetActive(false);
		}
		else
		{
			this.SetBlendShapeWeight(28, 100);
		}
		this.SetSkin(this.skinnyMat);
		this.setSkinColor();
		foreach (GameObject gameObject in this.hats)
		{
			if (gameObject)
			{
				gameObject.SetActive(false);
			}
		}
		if (this.props1Go)
		{
			this.props1Go.SetActive(false);
		}
		if (this.props2Go)
		{
			this.props2Go.SetActive(false);
		}
		if (this.props3Go)
		{
			this.props3Go.SetActive(false);
		}
		if (!this.female)
		{
			base.SendMessageUpwards("UpdateProps", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public void enableMaleSkinnyPaleProps()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		this.resetProps();
		this.Club.SetActive(false);
		this.FireStick.SetActive(false);
		if (this.regularStick)
		{
			this.regularStick.SetActive(false);
		}
		if (this.dummySetup)
		{
			this.skinnyParentGo.SetActive(true);
			this.bodyParentGo.SetActive(false);
		}
		else
		{
			this.SetBlendShapeWeight(28, 100);
		}
		this.SetSkin(this.paleMat);
		base.Invoke("setSkinColor", 0.2f);
		foreach (GameObject gameObject in this.hats)
		{
			if (gameObject)
			{
				gameObject.SetActive(false);
			}
		}
		if (this.props1Go)
		{
			this.props1Go.SetActive(false);
		}
		if (this.props2Go)
		{
			this.props2Go.SetActive(false);
		}
		if (this.props3Go)
		{
			this.props3Go.SetActive(false);
		}
		if (!this.female)
		{
			base.SendMessageUpwards("UpdateProps", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public void enablePaleProps()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		this.resetProps();
		this.pale = true;
		this.Club.SetActive(false);
		this.FireStick.SetActive(false);
		if (this.regularStick)
		{
			this.regularStick.SetActive(false);
		}
		if (this.dummySetup)
		{
			this.skinnyParentGo.SetActive(false);
			this.bodyParentGo.SetActive(true);
		}
		else
		{
			this.SetBlendShapeWeight(28, 0);
		}
		this.SetSkin(this.paleMat);
		base.Invoke("setSkinColor", 0.25f);
		foreach (GameObject gameObject in this.hats)
		{
			gameObject.SetActive(false);
		}
		if (this.props1Go)
		{
			this.props1Go.SetActive(false);
		}
		if (this.props2Go)
		{
			this.props2Go.SetActive(false);
		}
		if (this.props3Go)
		{
			this.props3Go.SetActive(false);
		}
		if (!this.female)
		{
			base.SendMessageUpwards("UpdateProps", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public void SetSkin(Material mat)
	{
		this.skinRenderer.sharedMaterial = mat;
		if (this.lowBody)
		{
			this.lowBody.GetComponent<SkinnedMeshRenderer>().sharedMaterial = mat;
		}
		if (this.lowSkinnyBody)
		{
			this.lowSkinnyBody.GetComponent<SkinnedMeshRenderer>().sharedMaterial = mat;
		}
		if (this.MyBodyParts.Length > 0)
		{
			foreach (GameObject gameObject in this.MyBodyParts)
			{
				gameObject.GetComponent<Renderer>().sharedMaterial = mat;
			}
		}
		if (this.MySkinnyParts.Length > 0)
		{
			foreach (GameObject gameObject2 in this.MySkinnyParts)
			{
				gameObject2.GetComponent<Renderer>().sharedMaterial = mat;
			}
		}
		if (!this.female)
		{
			this.SelectedMaterial = mat.name;
		}
	}

	
	public void spawnRandomArt()
	{
		Vector3 pos = base.transform.position + base.transform.forward * 2f;
		Transform transform = PoolManager.Pools["misc"].Spawn(this.art[UnityEngine.Random.Range(0, this.art.Length)].transform, pos, base.transform.rotation);
		transform.eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
		spawnTimeout component = transform.GetComponent<spawnTimeout>();
		component.despawnTime = 1000f;
		component.invokeDespawn();
	}

	
	public void SetBlendShapeWeight(int index, int value)
	{
		if (!this.skinRenderer)
		{
			this.skinRenderer = this.MyBody.GetComponent<SkinnedMeshRenderer>();
		}
		this.skinRenderer.SetBlendShapeWeight(index, (float)value);
		if (BoltNetwork.isServer && !this.female && index == 28)
		{
			base.entity.GetState<IMutantMaleState>().BlendShapeWeight0 = (float)value;
		}
	}

	
	public void resetSkinColor()
	{
	}

	
	public void resetProps()
	{
		if (this.leaderCape)
		{
			this.leaderCape.SetActive(false);
		}
		if (this.leaderCape1)
		{
			this.leaderCape1.SetActive(false);
		}
		if (this.leaderCape2)
		{
			this.leaderCape2.SetActive(false);
		}
		if (!this.female)
		{
			base.SendMessageUpwards("UpdateProps", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public void enableLights()
	{
		foreach (GameObject gameObject in this.lights)
		{
			gameObject.SetActive(true);
		}
		if (!this.female)
		{
			base.SendMessageUpwards("UpdateProps", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public void disableLights()
	{
		foreach (GameObject gameObject in this.lights)
		{
			gameObject.SetActive(false);
		}
		if (!this.female)
		{
			base.SendMessageUpwards("UpdateProps", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public void setBurntSkin()
	{
		if (this.burntMat)
		{
			this.SetSkin(this.burntMat);
		}
	}

	
	public IEnumerator setupFeedingProps(int n)
	{
		yield return YieldPresets.WaitTwoSeconds;
		if (n == 0)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.setup.feedingProps[UnityEngine.Random.Range(0, this.setup.feedingProps.Length)], this.setup.headJoint.transform.position, Quaternion.identity);
		}
		else
		{
			this.setup.heldMeat.SetActive(true);
		}
		if (this.setup.charLeftWeaponGo)
		{
			this.setup.charLeftWeaponGo.SetActive(false);
		}
		if (this.setup.fireWeapon)
		{
			this.setup.fireWeapon.SetActive(false);
		}
		yield return null;
		yield break;
	}

	
	private void dropHeldMeat()
	{
		if (this.setup.heldMeat.activeSelf)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.setup.spawnedMeat, this.setup.heldMeat.transform.position, this.setup.heldMeat.transform.rotation);
			this.setup.heldMeat.SetActive(false);
		}
		if (this.setup.ai.fireman)
		{
			this.setup.fireWeapon.SetActive(true);
		}
		if (this.setup.ai.male && !this.setup.ai.pale && !this.setup.ai.maleSkinny && !this.setup.ai.fireman)
		{
			this.setup.charLeftWeaponGo.SetActive(true);
		}
	}

	
	public void setDynamiteMan()
	{
		this.dynamiteMan = true;
	}

	
	private void setSkinDamage1(float val)
	{
		this.applySkinDamage("_Damage1", val);
	}

	
	private void setSkinDamage2(float val)
	{
		this.applySkinDamage("_Damage2", val);
	}

	
	private void setSkinDamage3(float val)
	{
		this.applySkinDamage("_Damage3", val);
	}

	
	private void setSkinDamage4(float val)
	{
		this.applySkinDamage("_Damage4", val);
	}

	
	private void setSkinDamageProperty(MaterialPropertyBlock block)
	{
		this.bloodPropertyBlock = block;
		if (this.MyBodyParts.Length > 0)
		{
			foreach (GameObject gameObject in this.MyBodyParts)
			{
				Renderer component = gameObject.GetComponent<Renderer>();
				component.SetPropertyBlock(block);
			}
		}
		if (this.MySkinnyParts.Length > 0)
		{
			foreach (GameObject gameObject2 in this.MySkinnyParts)
			{
				Renderer component2 = gameObject2.GetComponent<Renderer>();
				component2.SetPropertyBlock(block);
			}
		}
	}

	
	private void setSkinColor()
	{
		Color value = Scene.SceneTracker.regularMutantColor;
		if (this.pale && !this.skinned)
		{
			value = Scene.SceneTracker.paleMutantColor;
		}
		if (this.poisoned)
		{
			value = Scene.SceneTracker.poisonedColor;
		}
		this.sourceColor = value;
		if (this.MyBodyParts.Length > 0)
		{
			foreach (GameObject gameObject in this.MyBodyParts)
			{
				Renderer component = gameObject.GetComponent<Renderer>();
				component.GetPropertyBlock(this.bloodPropertyBlock);
				this.bloodPropertyBlock.SetColor("_Color", value);
				component.SetPropertyBlock(this.bloodPropertyBlock);
			}
		}
		if (this.MySkinnyParts.Length > 0)
		{
			foreach (GameObject gameObject2 in this.MySkinnyParts)
			{
				Renderer component2 = gameObject2.GetComponent<Renderer>();
				component2.GetPropertyBlock(this.bloodPropertyBlock);
				this.bloodPropertyBlock.SetColor("_Color", value);
				component2.SetPropertyBlock(this.bloodPropertyBlock);
			}
		}
		if (this.skinRenderer)
		{
			this.skinRenderer.GetPropertyBlock(this.bloodPropertyBlock);
			this.bloodPropertyBlock.SetColor("_Color", value);
			this.skinRenderer.SetPropertyBlock(this.bloodPropertyBlock);
		}
	}

	
	private void setSkinMaterialProperties(Material mat)
	{
	}

	
	private void setPoisoned(bool onoff)
	{
		this.poisoned = onoff;
	}

	
	private void applySkinDamage(string t, float s)
	{
		if (this.MyBodyParts.Length > 0)
		{
			foreach (GameObject gameObject in this.MyBodyParts)
			{
				Renderer component = gameObject.GetComponent<Renderer>();
				component.GetPropertyBlock(this.bloodPropertyBlock);
				this.bloodPropertyBlock.SetFloat(t, s);
				component.SetPropertyBlock(this.bloodPropertyBlock);
			}
		}
		if (this.MySkinnyParts.Length > 0)
		{
			foreach (GameObject gameObject2 in this.MySkinnyParts)
			{
				Renderer component2 = gameObject2.GetComponent<Renderer>();
				component2.GetPropertyBlock(this.bloodPropertyBlock);
				this.bloodPropertyBlock.SetFloat(t, s);
				component2.SetPropertyBlock(this.bloodPropertyBlock);
			}
		}
	}

	
	public GameObject[] art;

	
	public GameObject[] hats;

	
	public GameObject[] necklaces;

	
	public GameObject[] loinCloth;

	
	public GameObject[] bracelets;

	
	public GameObject[] anklets;

	
	public Material[] material;

	
	public Material FireManMat;

	
	public Material DynamiteMat;

	
	public Material paleMat;

	
	public Material cannibalMat;

	
	public Material skinnyMat;

	
	public Material burntMat;

	
	public Material[] paintedTribeMats;

	
	public Material paintedLeaderMat;

	
	public GameObject MyBody;

	
	public GameObject lowBody;

	
	public GameObject lowSkinnyBody;

	
	public GameObject[] MyBodyParts;

	
	public GameObject[] MySkinnyParts;

	
	private GameObject bodyParentGo;

	
	private GameObject skinnyParentGo;

	
	public GameObject props1Go;

	
	public GameObject props2Go;

	
	public GameObject props3Go;

	
	public GameObject skinnedHeadMask;

	
	public GameObject leaderCape;

	
	public GameObject leaderCape1;

	
	public GameObject leaderCape2;

	
	public GameObject[] lights;

	
	[HideInInspector]
	public string SelectedMaterial = string.Empty;

	
	[HideInInspector]
	public Dictionary<string, Material> MaterialLookup = new Dictionary<string, Material>();

	
	[HideInInspector]
	public SkinnedMeshRenderer skinRenderer;

	
	private mutantAI ai;

	
	private mutantScriptSetup setup;

	
	private Material[] mats;

	
	public Color sourceColor;

	
	public bool hasRegularWeapon;

	
	public bool hasAdvancedWeapon;

	
	public GameObject FireStick;

	
	public GameObject Club;

	
	public GameObject regularStick;

	
	public GameObject[] regularWeapons;

	
	public GameObject[] advancedWeapons;

	
	public GameObject tennisBallBelt;

	
	public GameObject dynamiteBelt;

	
	public int MyRandom;

	
	public bool FireMan;

	
	public bool dynamiteMan;

	
	public bool pale;

	
	public bool female;

	
	public bool skinny;

	
	public bool skinned;

	
	public bool poisoned;

	
	public int LeaderDice;

	
	public int regularMaleDice;

	
	public int regularFemaleDice;

	
	public bool skipSetup;

	
	public bool dummySetup;

	
	public MaterialPropertyBlock bloodPropertyBlock;
}
