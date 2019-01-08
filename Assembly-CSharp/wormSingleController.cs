using System;
using System.Collections;
using FMOD.Studio;
using TheForest.Utils;
using UnityEngine;

public class wormSingleController : MonoBehaviour
{
	private void Start()
	{
		if (!Scene.MutantControler.activeWorms.Contains(base.gameObject))
		{
			Scene.MutantControler.activeWorms.Add(base.gameObject);
		}
		this.bloodPropertyBlock = new MaterialPropertyBlock();
		this._lerpColor = this.DarkSkin;
		if (!CoopPeerStarter.DedicatedHost)
		{
			this.wormLoopInstance = FMOD_StudioSystem.instance.GetEvent("event:/mutants/creepies/Worm/worm_idle");
		}
		this.skinColorBlock = new MaterialPropertyBlock();
		this._colorPropertyId = Shader.PropertyToID("_Color");
		this.groundMask = 69345280;
		this.jumpDelay = Time.time + this.nextJumpDelay;
		this.rb = base.transform.GetComponent<Rigidbody>();
		this.animator = base.transform.GetComponent<Animator>();
		this.bodyCollider = base.transform.GetComponentInChildren<Collider>();
		this.health = base.transform.GetComponent<wormHealth>();
		this.fmodEmitter = base.transform.GetComponent<FMOD_StudioEventEmitter>();
		this.impactSfx = base.transform.GetComponent<PhysicsSfx>();
		this.rb.drag = 20f;
		this.rb.angularDrag = 20f;
		base.Invoke("resetDrag", 1f);
		this.randomX = (float)UnityEngine.Random.Range(-3, 3);
		this.randomZ = (float)UnityEngine.Random.Range(-3, 3);
		this.birthCoolDown = this.SetBirthCoolDown(false);
		if (UnityEngine.Random.value > 0.5f)
		{
			this.baseSkin.sharedMesh = this.bodyVar1.sharedMesh;
			this.baseSkin.sharedMaterial = this.bodyVar1Mat;
			this.bodyType = 1;
		}
		else
		{
			this.baseSkin.sharedMesh = this.bodyVar2.sharedMesh;
			this.baseSkin.sharedMaterial = this.bodyVar2Mat;
			this.bodyType = 2;
		}
		this.setRandomSkinDamage();
	}

	private void OnDestroy()
	{
		if (!Scene.MutantControler)
		{
			return;
		}
		if (Scene.MutantControler.activeWorms.Contains(base.gameObject))
		{
			Scene.MutantControler.activeWorms.Remove(base.gameObject);
		}
		this.setWormIdleLoop(false);
		if (this.hiveController && this.hiveController.activeWormSingle.Contains(base.gameObject))
		{
			this.hiveController.activeWormSingle.Remove(base.gameObject);
		}
	}

	private void resetDrag()
	{
		this.rb.drag = 0.05f;
		this.rb.angularDrag = 0.05f;
	}

	private void setHiveController(GameObject go)
	{
		this.hiveController = go.GetComponent<wormHiveController>();
		if (!this.hiveController.activeWormSingle.Contains(base.gameObject))
		{
			this.hiveController.activeWormSingle.Add(base.gameObject);
		}
	}

	private void setRandomSkinDamage()
	{
		this.baseSkin.GetPropertyBlock(this.bloodPropertyBlock);
		this.bloodPropertyBlock.SetFloat("_Damage1", UnityEngine.Random.Range(0f, 0.5f));
		this.bloodPropertyBlock.SetFloat("_Damage3", UnityEngine.Random.Range(0f, 0.5f));
		this.bloodPropertyBlock.SetFloat("_Damage2", UnityEngine.Random.Range(0f, 0.5f));
		this.bloodPropertyBlock.SetFloat("_Damage4", UnityEngine.Random.Range(0f, 0.5f));
		this.baseSkin.SetPropertyBlock(this.bloodPropertyBlock);
	}

	private void Update()
	{
		if (Time.time > this.lodUpdateTimer)
		{
			this.updateLodSettings();
			this.lodUpdateTimer = Time.time + 1f;
		}
		if (this.connectedWormTop && this.connectedWormTop.parent == null)
		{
			this.resetTopConnector();
		}
		if (this.connectedWormBottom && this.connectedWormBottom.parent == null)
		{
			this.resetBottomConnector();
		}
		if (!this.attached && Time.time > this.terrainCheckTimer)
		{
			this.CheckBelowTerrain();
			this.terrainCheckTimer = Time.time + 1.5f;
		}
		bool flag = false;
		if (this.attached || this.birthing)
		{
			flag = true;
		}
		this._lerpColor = Color.Lerp(this._lerpColor, (!flag) ? this.NormalSkin : this.AngrySkin, Time.deltaTime / 2f);
		if (this.baseSkin)
		{
			this.baseSkin.GetPropertyBlock(this.skinColorBlock);
			this.skinColorBlock.SetColor(this._colorPropertyId, this._lerpColor);
			this.baseSkin.SetPropertyBlock(this.skinColorBlock);
		}
		if (this.attached)
		{
			this.bodyCollider.isTrigger = true;
			return;
		}
		this.bodyCollider.isTrigger = false;
		if (Time.time > this.farDestroyCoolDown)
		{
			this.DestroyDistantWorm();
			this.farDestroyCoolDown = Time.time + 10f;
		}
		if (this.hiveController == null)
		{
			return;
		}
		if (Time.time < this.artifactRepelTimer && this.currentArtifactTarget)
		{
			this.currentTarget = this.currentArtifactTarget;
		}
		else if (Time.time < this.artifactAttractTimer && this.currentArtifactTarget)
		{
			this.currentTarget = this.currentArtifactTarget;
		}
		else if (this.hiveController.activeWormAngels.Count > 0)
		{
			this.currentTarget = this.hiveController.activeWormAngels[0].gameObject;
		}
		else if (this.hiveController.activeWormTrees.Count > 0)
		{
			this.currentTarget = this.hiveController.activeWormTrees[0].gameObject;
		}
		else if (this.hiveController.activeWormWalkers.Count > 0)
		{
			this.currentTarget = this.hiveController.activeWormWalkers[0].gameObject;
		}
		else
		{
			this.currentTarget = Scene.SceneTracker.GetClosestPlayerFromPos(base.transform.position);
		}
		if ((Time.time > this.birthCoolDown && !this.assembleFormMode) || this.birthing)
		{
			this.birthCoolDown = this.SetBirthCoolDown(false);
			if (this.hiveController.activeWormSingle.Count < this.hiveController.maxWorms && this.hiveController.respawnCount < this.hiveController.maxRespawnAmount)
			{
				this.doBirthRoutine();
				return;
			}
			this.birthing = false;
		}
		if (Time.time < this.attachCoolDown)
		{
			return;
		}
		if (this.assembleCoolDown > 1.5f)
		{
			this.CheckTargetConditions();
			this.assembleCoolDown = 0f;
		}
		else
		{
			this.assembleCoolDown += Time.deltaTime;
		}
		if (this.assembleFormMode)
		{
			this.goToAssembleForm();
		}
	}

	private void FixedUpdate()
	{
		if (this.attached)
		{
			for (int i = 0; i < this.storeLastPos.Length; i++)
			{
				int num = i + 1;
				if (num < this.storeLastPos.Length)
				{
					this.storeLastPos[num] = this.storeLastPos[i];
				}
			}
			this.storeLastPos[0] = base.transform.position;
			return;
		}
		this.CalculateGravityForces();
		if (this.assembleFormMode)
		{
			this.WiggleTowardTarget();
		}
		else
		{
			this.JumpTowardTarget();
		}
	}

	private void LateUpdate()
	{
		if (this.TargetAnchorTop && this.connectedWormTop)
		{
			this.ConnectorJointTop.position = this.TargetAnchorTop.transform.position;
			this.ConnectorJointTop.rotation = this.TargetAnchorTop.transform.rotation;
		}
		if (this.TargetAnchorBottom && this.connectedWormBottom)
		{
			this.ConnectorJointBottom.position = this.TargetAnchorBottom.transform.position;
			this.ConnectorJointBottom.rotation = this.TargetAnchorBottom.transform.rotation;
		}
	}

	private void CheckTargetConditions()
	{
		float num = Vector3.Distance(this.currentTarget.transform.position, base.transform.position);
		if ((num < 100f && num > 45f && this.hiveController.activeWormSingle.Count > this.minWalkerFormAmount - 1) || this.hiveController.activeWormWalkers.Count > 0)
		{
			this.assembleFormMode = true;
		}
		if (((this.currentTarget.transform.position - base.transform.position).sqrMagnitude < 225f && this.hiveController.activeWormSingle.Count > this.minTreeFormAmount) || this.hiveController.activeWormTrees.Count > 0)
		{
			this.assembleFormMode = true;
		}
	}

	private void CalculateGravityForces()
	{
		this.rb.AddForce(0f, -1f * this.rb.mass, 0f, ForceMode.Force);
	}

	private void doBirthRoutine()
	{
		this.birthing = true;
		this.animator.SetBoolReflected("wiggle", false);
		this.animator.SetBoolReflected("jump", false);
		this.animator.SetBoolReflected("birth", true);
		AnimatorStateInfo currentAnimatorStateInfo = this.animator.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.shortNameHash == this.birthHash && currentAnimatorStateInfo.normalizedTime > 0.65f)
		{
			if (this.hiveController.activeWormSingle.Count < this.hiveController.maxWorms)
			{
				this.hiveController.SpawnWorm(base.transform.position);
				this.hiveController.respawnCount++;
			}
			this.animator.SetBoolReflected("birth", false);
			this.birthCoolDown = this.SetBirthCoolDown(false);
			this.birthing = false;
			this.rb.AddTorque(base.transform.forward * this.rb.mass * 20f, ForceMode.VelocityChange);
		}
	}

	private void JumpTowardTarget()
	{
		if (this.currentTarget == null)
		{
			return;
		}
		if (Time.time > this.jumpDelay)
		{
			Vector3 a = this.Circle2(8f);
			a += this.currentTarget.transform.position;
			Vector3 a2 = (a - base.transform.position).normalized;
			if (Time.time < this.artifactRepelTimer)
			{
				a2 *= -1f;
			}
			a2.y += 0.5f;
			this.rb.AddForce(a2 * 15f * this.rb.mass, ForceMode.Impulse);
			this.rb.AddTorque(a2 * this.rb.mass * 10f, ForceMode.Impulse);
			this.animator.SetBoolReflected("jump", true);
			this.jumpDelay = Time.time + this.nextJumpDelay + UnityEngine.Random.Range(0f, 2f);
		}
	}

	private void WiggleTowardTarget()
	{
		if (this.currentFormSpawn == null)
		{
			this.assembleFormMode = false;
			return;
		}
		if (this.currentTarget == null)
		{
			return;
		}
		Vector3 a = (this.currentTarget.transform.position - base.transform.position).normalized;
		if (Time.time < this.artifactRepelTimer)
		{
			a *= -1f;
		}
		this.rb.AddForce(a * this.rb.mass, ForceMode.Impulse);
		this.doSmoothLookAtDir(this.currentFormSpawn.transform.position, 3f);
		this.animator.SetBoolReflected("wiggle", true);
		this.animator.SetBoolReflected("jump", false);
	}

	private void goToAssembleForm()
	{
		if (Time.time > this.hiveController.spawnFormCoolDown)
		{
			if (this.hiveController.activeWormSingle.Count > this.minAngelFormAmount && !this.hiveController.anyFormSpawned && UnityEngine.Random.value > 0.5f)
			{
				if (this.hiveController.activeWormAngels.Count < 1)
				{
					this.spawnWormAngel();
				}
				else if (this.hiveController.activeWormAngels[0] != null)
				{
					this.currentFormSpawn = this.hiveController.activeWormAngels[0];
				}
			}
			else if (this.hiveController.activeWormSingle.Count > this.minWalkerFormAmount && !this.hiveController.anyFormSpawned && UnityEngine.Random.value > 0.4f)
			{
				if (this.hiveController.activeWormWalkers.Count < 1)
				{
					this.spawnWormWalker();
				}
				else if (this.hiveController.activeWormWalkers[0] != null)
				{
					this.currentFormSpawn = this.hiveController.activeWormWalkers[0];
				}
			}
			else if (this.hiveController.activeWormSingle.Count > this.minTreeFormAmount && !this.hiveController.anyFormSpawned)
			{
				if (this.hiveController.activeWormTrees.Count < 1)
				{
					this.spawnWormTree();
				}
				else if (this.currentAttachController && this.currentAttachController.numAttachedWorms > 13 && this.hiveController.activeWormTrees.Count < 2)
				{
					this.spawnWormTree();
				}
				else if (!this.attached)
				{
					this.currentFormSpawn = this.hiveController.activeWormTrees[UnityEngine.Random.Range(0, this.hiveController.activeWormTrees.Count)];
				}
			}
		}
		if (this.hiveController.activeWormAngels.Count > 0)
		{
			this.currentFormSpawn = this.hiveController.activeWormAngels[0];
		}
		else if (this.hiveController.activeWormWalkers.Count > 0)
		{
			this.currentFormSpawn = this.hiveController.activeWormWalkers[0];
		}
		else if (this.hiveController.activeWormTrees.Count > 0)
		{
			this.currentFormSpawn = this.hiveController.activeWormTrees[0];
		}
		if (this.currentFormSpawn == null)
		{
			return;
		}
		if ((base.transform.position - this.currentFormSpawn.transform.position).sqrMagnitude > 400f)
		{
			return;
		}
		if (this.currentAttachController == null)
		{
			this.currentAttachController = this.currentFormSpawn.GetComponent<wormAttachController>();
		}
		if (!this.currentAttachController.canAttach)
		{
			return;
		}
		if (this.currentAttachJoint < 0)
		{
			this.currentAttachJoint = this.findFormAttachPoint(this.currentAttachController);
		}
		if (this.currentAttachJoint < 0)
		{
			return;
		}
		if (this.currentJointPos < 0)
		{
			return;
		}
		this.currentAttachController.AttachPoints[this.currentAttachJoint].AttachedWorm[this.currentJointPos] = base.transform;
		this.attached = true;
		base.StartCoroutine(this.attachToFormRoutine(this.currentAttachJoint, this.currentJointPos));
	}

	private IEnumerator attachToFormRoutine(int attachIndex, int attachPos)
	{
		this.attached = true;
		Transform attachJoint = this.currentAttachController.AttachPoints[attachIndex].attachJoint;
		Vector3 localOffset = default(Vector3);
		switch (attachPos)
		{
		case 0:
			localOffset = new Vector3(0f, 0.5f, 0f);
			break;
		case 1:
			localOffset = new Vector3(0f, -0.5f, 0f);
			break;
		case 2:
			localOffset = new Vector3(0f, 0f, 0.5f);
			break;
		case 3:
			localOffset = new Vector3(0f, 0f, -0.5f);
			break;
		}
		this.rb.isKinematic = true;
		this.rb.useGravity = false;
		int dir = 1;
		if (attachJoint.name.Contains("rt_"))
		{
			dir = -1;
		}
		float t = 0f;
		while (t < 1f)
		{
			if (this.currentFormSpawn == null)
			{
				this.Detach();
				yield break;
			}
			Vector3 targetLocalPos = new Vector3(0f, -0.4f, -1.5f * (float)dir) + localOffset;
			this.storeAttachPos = targetLocalPos;
			this.offsetTr.localPosition = Vector3.Lerp(this.offsetTr.localPosition, targetLocalPos, t);
			base.transform.position = Vector3.Lerp(base.transform.position, attachJoint.position, t);
			Quaternion targetRotation = attachJoint.rotation * Quaternion.Euler(this.randomX, 90f, this.randomZ);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, targetRotation, t);
			t += Time.deltaTime / 2f;
			yield return null;
		}
		if (this.attached)
		{
			this.SetupConnectorJointsTop(attachIndex);
			this.SetupConnectorJointsBottom(attachIndex);
			this.animator.SetBoolReflected("wiggle", false);
			this.animator.SetBoolReflected("jump", false);
			this.animator.SetBoolReflected("birth", false);
			this.animator.SetBoolReflected("attached", true);
			base.transform.parent = attachJoint;
		}
		yield return null;
		yield break;
	}

	private void SetupConnectorJointsTop(int segment)
	{
		int num = segment + 1;
		if (this.currentAttachController == null)
		{
			return;
		}
		if (num >= this.currentAttachController.AttachPoints.Length || num < 0)
		{
			return;
		}
		GameObject gameObject = null;
		wormSingleController wormSingleController = null;
		foreach (Transform transform in this.currentAttachController.AttachPoints[num].AttachedWorm)
		{
			if (transform != null)
			{
				wormSingleController = transform.GetComponent<wormSingleController>();
				if (!wormSingleController.AnchorJointBottom.gameObject.activeSelf)
				{
					gameObject = transform.gameObject;
					break;
				}
			}
		}
		if (gameObject == null)
		{
			return;
		}
		if (wormSingleController.AnchorJointBottom.gameObject.activeSelf || Vector3.Distance(this.ConnectorJointTop.position, wormSingleController.AnchorJointBottom.position) > 8f)
		{
			return;
		}
		this.TargetAnchorTop = wormSingleController.AnchorJointBottom;
		this.ConnectorSkinTop.gameObject.SetActive(true);
		this.connectedWormTop = gameObject.transform;
	}

	private void SetupConnectorJointsBottom(int segment)
	{
		int num = segment - 1;
		if (this.currentAttachController == null)
		{
			return;
		}
		if (num >= this.currentAttachController.AttachPoints.Length || num < 0)
		{
			return;
		}
		GameObject gameObject = null;
		wormSingleController wormSingleController = null;
		foreach (Transform transform in this.currentAttachController.AttachPoints[num].AttachedWorm)
		{
			if (transform != null)
			{
				wormSingleController = transform.GetComponent<wormSingleController>();
				if (!wormSingleController.AnchorJointTop.gameObject.activeSelf)
				{
					gameObject = transform.gameObject;
					break;
				}
			}
		}
		if (gameObject == null)
		{
			return;
		}
		if (wormSingleController.AnchorJointTop.gameObject.activeSelf || Vector3.Distance(this.ConnectorJointBottom.position, wormSingleController.AnchorJointTop.position) > 8f)
		{
			return;
		}
		this.TargetAnchorBottom = wormSingleController.AnchorJointTop;
		this.ConnectorSkinBottom.gameObject.SetActive(true);
		this.connectedWormBottom = gameObject.transform;
	}

	private int findFormAttachPoint(wormAttachController controller)
	{
		for (int i = 0; i < controller.AttachPoints.Length; i++)
		{
			if (controller.AttachPoints[i].currentEmptySlot < controller.AttachPoints[i].AttachedWorm.Length)
			{
				this.currentJointPos = controller.AttachPoints[i].currentEmptySlot;
				controller.AttachPoints[i].currentEmptySlot++;
				return i;
			}
		}
		return -1;
	}

	private void DestroyDistantWorm()
	{
		if (Scene.SceneTracker.GetClosestPlayerDistanceFromPos(base.transform.position) > 300f && !this.attached)
		{
			this.health.Die();
		}
	}

	public void Detach()
	{
		if (this.attached)
		{
			this.birthCoolDown = this.SetBirthCoolDown(true);
			this.animator.SetBoolReflected("attached", false);
			this.animator.SetBoolReflected("wiggle", false);
			base.transform.parent = null;
			this.rb.isKinematic = false;
			this.rb.useGravity = true;
			this.assembleFormMode = false;
			this.currentFormSpawn = null;
			this.currentAttachController = null;
			this.currentAttachJoint = -1;
			this.currentJointPos = -1;
			this.attachCoolDown = Time.time + UnityEngine.Random.Range(5f, 9f);
			this.attached = false;
			base.transform.localScale = new Vector3(1f, 1f, 1f);
			this.resetTopConnector();
			this.resetBottomConnector();
			base.StartCoroutine(this.resetLocalOffset());
			Vector3 a = this.storeLastPos[this.storeLastPos.Length - 1] - base.transform.position;
			this.rb.AddForce(-a * this.rb.mass * 250f, ForceMode.Force);
		}
	}

	private float SetBirthCoolDown(bool shortCoolDown)
	{
		float num = 1f;
		if (shortCoolDown)
		{
			num = 0.5f;
		}
		if (Scene.SceneTracker.allPlayers.Count > 1)
		{
			return Time.time + UnityEngine.Random.Range(9f, 18f) * num;
		}
		return Time.time + UnityEngine.Random.Range(10f, 21f) * num;
	}

	private void resetTopConnector()
	{
		this.TargetAnchorTop = null;
		this.ConnectorJointTop.parent = this.AnchorJointTop;
		this.ConnectorJointTop.localPosition = Vector3.zero;
		this.ConnectorSkinTop.gameObject.SetActive(false);
		this.AnchorJointTop.gameObject.SetActive(false);
		this.connectedWormTop = null;
	}

	private void resetBottomConnector()
	{
		this.TargetAnchorBottom = null;
		this.ConnectorJointBottom.parent = this.AnchorJointBottom;
		this.ConnectorJointBottom.localPosition = Vector3.zero;
		this.ConnectorSkinBottom.gameObject.SetActive(false);
		this.AnchorJointBottom.gameObject.SetActive(false);
		this.connectedWormBottom = null;
	}

	private IEnumerator resetLocalOffset()
	{
		float t = 0f;
		while (t < 1f)
		{
			this.offsetTr.localPosition = Vector3.Lerp(this.offsetTr.localPosition, Vector3.zero, t);
			t += Time.deltaTime * 2f;
			yield return null;
		}
		yield break;
	}

	private void updateLodSettings()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		float sqrMagnitude = (LocalPlayer.Transform.position - base.transform.position).sqrMagnitude;
		this.setWormIdleLoop(sqrMagnitude <= 1600f);
		if (sqrMagnitude > 625f)
		{
			this.impactSfx.enabled = false;
		}
		else
		{
			this.impactSfx.enabled = true;
		}
	}

	private void CheckBelowTerrain()
	{
		if (this.attached)
		{
			return;
		}
		float num = Terrain.activeTerrain.SampleHeight(this.rb.position) + Terrain.activeTerrain.transform.position.y;
		if (base.transform.position.y < num)
		{
			Vector3 position = this.rb.position;
			position.y = num + 0.5f;
			this.rb.MovePosition(position);
			this.rb.velocity = Vector3.zero;
		}
	}

	private void spawnWormTree()
	{
		this.currentFormSpawn = UnityEngine.Object.Instantiate<GameObject>(this.hiveController.WormTreePrefab, base.transform.position, Quaternion.identity);
		this.hiveController.activeWormTrees.Add(this.currentFormSpawn);
		this.currentFormSpawn.SendMessage("setHiveController", this.hiveController);
		RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position, Vector3.down, out raycastHit, 15f, this.groundMask, QueryTriggerInteraction.Ignore))
		{
			this.currentFormSpawn.transform.position = new Vector3(this.currentFormSpawn.transform.position.x, raycastHit.point.y, this.currentFormSpawn.transform.position.z);
		}
	}

	private void spawnWormWalker()
	{
		this.currentFormSpawn = UnityEngine.Object.Instantiate<GameObject>(this.hiveController.WormWalkerPrefab, base.transform.position, Quaternion.identity);
		this.hiveController.activeWormWalkers.Add(this.currentFormSpawn);
		this.currentFormSpawn.SendMessage("setHiveController", this.hiveController);
		RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position, Vector3.down, out raycastHit, 15f, this.groundMask, QueryTriggerInteraction.Ignore))
		{
			this.currentFormSpawn.transform.position = new Vector3(this.currentFormSpawn.transform.position.x, raycastHit.point.y, this.currentFormSpawn.transform.position.z);
		}
	}

	private void spawnWormAngel()
	{
		this.currentFormSpawn = UnityEngine.Object.Instantiate<GameObject>(this.hiveController.WormAngelPrefab, base.transform.position, Quaternion.identity);
		this.hiveController.activeWormAngels.Add(this.currentFormSpawn);
		this.currentFormSpawn.SendMessage("setHiveController", this.hiveController);
		RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position, Vector3.down, out raycastHit, 15f, this.groundMask, QueryTriggerInteraction.Ignore))
		{
			this.currentFormSpawn.transform.position = new Vector3(this.currentFormSpawn.transform.position.x, raycastHit.point.y, this.currentFormSpawn.transform.position.z);
		}
	}

	private void onArtifactRepel(Transform artifact)
	{
		if ((base.transform.position - artifact.position).sqrMagnitude > 4900f)
		{
			return;
		}
		this.artifactRepelTimer = Time.time + 5f;
		this.currentArtifactTarget = artifact.gameObject;
	}

	private void onArtifactAttract(Transform artifact)
	{
		this.artifactAttractTimer = Time.time + 5f;
		this.currentArtifactTarget = artifact.gameObject;
	}

	public void setWormIdleLoop(bool onOff)
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		PLAYBACK_STATE state;
		UnityUtil.ERRCHECK(this.wormLoopInstance.getPlaybackState(out state));
		if (onOff)
		{
			UnityUtil.ERRCHECK(this.wormLoopInstance.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
			if (!state.isPlaying())
			{
				UnityUtil.ERRCHECK(this.wormLoopInstance.start());
			}
		}
		else if (state.isPlaying())
		{
			UnityUtil.ERRCHECK(this.wormLoopInstance.stop(STOP_MODE.ALLOWFADEOUT));
		}
	}

	private void doSmoothLookAtDir(Vector3 lookAtPos, float speed)
	{
		lookAtPos.y = base.transform.position.y;
		Vector3 lhs = lookAtPos - base.transform.position;
		Vector3 vector = Vector3.Cross(lhs, base.transform.up);
		Quaternion quaternion = base.transform.rotation;
		if (vector != Vector3.zero && vector.sqrMagnitude > 0f)
		{
			this.desiredRotation = Quaternion.LookRotation(vector, Vector3.up);
		}
		quaternion = Quaternion.Slerp(quaternion, this.desiredRotation, speed * Time.deltaTime);
		this.rb.rotation = quaternion;
	}

	private Vector2 Circle2(float radius)
	{
		Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
		return normalized * radius;
	}

	public wormHiveController hiveController;

	private wormHealth health;

	private FMOD_StudioEventEmitter fmodEmitter;

	private PhysicsSfx impactSfx;

	public Color NormalSkin;

	public Color AngrySkin;

	public Color DarkSkin;

	private Color _lerpColor;

	private int _colorPropertyId;

	private MaterialPropertyBlock skinColorBlock;

	private float nextJumpDelay = 2f;

	public Collider[] dynamicColliders;

	public Transform offsetTr;

	public SkinnedMeshRenderer baseSkin;

	public SkinnedMeshRenderer bodyVar1;

	public SkinnedMeshRenderer bodyVar2;

	public Material bodyVar1Mat;

	public Material bodyVar2Mat;

	public GameObject currentArtifactTarget;

	public GameObject currentTarget;

	public GameObject currentFormSpawn;

	public wormAttachController currentAttachController;

	private Collider bodyCollider;

	public int currentAttachJoint = -1;

	public int currentJointPos = -1;

	public bool attached;

	public bool birthing;

	public Vector3 storeAttachPos;

	private Vector3 artifactAttractPos;

	private float jumpDelay;

	private Rigidbody rb;

	private Animator animator;

	public bool assembleFormMode;

	private Quaternion desiredRotation;

	private int groundMask;

	private float attachCoolDown;

	private float birthCoolDown;

	private float assembleCoolDown;

	private float farDestroyCoolDown;

	private float lodUpdateTimer;

	private float terrainCheckTimer;

	private float attachDieTimer;

	private float artifactRepelTimer;

	private float artifactAttractTimer;

	public Vector3[] storeLastPos = new Vector3[5];

	private int storePosIndex;

	public Transform ConnectorJointBottom;

	public Transform ConnectorJointTop;

	public Transform AnchorJointBottom;

	public Transform AnchorJointTop;

	public Transform TargetAnchorTop;

	public Transform TargetAnchorBottom;

	public SkinnedMeshRenderer ConnectorSkinTop;

	public SkinnedMeshRenderer ConnectorSkinBottom;

	public Transform connectedWormTop;

	public Transform connectedWormBottom;

	private EventInstance wormLoopInstance;

	private float randomX;

	private float randomZ;

	private int minTreeFormAmount = 14;

	private int minWalkerFormAmount = 24;

	private int minAngelFormAmount = 29;

	public int bodyType;

	private int birthHash = Animator.StringToHash("birth");

	private MaterialPropertyBlock bloodPropertyBlock;
}
