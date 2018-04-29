using System;
using System.Collections.Generic;
using ChromaSDK;
using ChromaSDK.Api;
using ChromaSDK.ChromaPackage.Model;
using UnityEngine;


public class ChromaExample01 : MonoBehaviour
{
	
	private void FixedUpdate()
	{
		if (this._mMainActions.Count > 0)
		{
			Action action = this._mMainActions[0];
			this._mMainActions.RemoveAt(0);
			action();
		}
	}

	
	private void RunOnMainThread(Action action)
	{
		this._mMainActions.Add(action);
	}

	
	private static void LogResult(string label, EffectResponse result)
	{
		if (result == null)
		{
			Debug.LogError(string.Format("{0} Result was null!", label));
		}
		else
		{
			Debug.Log(string.Format("{0} {1}", label, result));
		}
	}

	
	private static EffectInput GetEffectChromaStatic(Color color)
	{
		EffectInput effectInput = new EffectInput(EffectType.CHROMA_NONE, null);
		effectInput.Effect = EffectType.CHROMA_STATIC;
		int value = ChromaUtils.ToBGR(color);
		effectInput.Param = new EffectInputParam(new int?(value), null, null, null, null, null);
		return effectInput;
	}

	
	private void SetEffectNoneOnAll()
	{
		if (!this._mConnectionManager.Connected)
		{
			Debug.LogError("Chroma client is not yet connected!");
			return;
		}
		ChromaApi apiChromaInstance = this._mConnectionManager.ApiChromaInstance;
		ChromaExample01.LogResult("PutChromaLinkNone:", apiChromaInstance.PutChromaLinkNone());
		ChromaExample01.LogResult("PutHeadsetNone:", apiChromaInstance.PutHeadsetNone());
		ChromaExample01.LogResult("PutKeyboardNone:", apiChromaInstance.PutKeyboardNone());
		ChromaExample01.LogResult("PutKeypadNone:", apiChromaInstance.PutKeypadNone());
		ChromaExample01.LogResult("PutMouseNone:", apiChromaInstance.PutMouseNone());
		ChromaExample01.LogResult("PutMousepadNone:", apiChromaInstance.PutMousepadNone());
	}

	
	private void SetEffectStaticOnAll(Color color)
	{
		if (!this._mConnectionManager.Connected)
		{
			Debug.LogError("Chroma client is not yet connected!");
			return;
		}
		ChromaApi apiChromaInstance = this._mConnectionManager.ApiChromaInstance;
		int value = ChromaUtils.ToBGR(color);
		ChromaExample01.LogResult("PutChromaLinkStatic:", apiChromaInstance.PutChromaLinkStatic(new int?(value)));
		ChromaExample01.LogResult("PutHeadsetStatic:", apiChromaInstance.PutHeadsetStatic(new int?(value)));
		ChromaExample01.LogResult("PutKeyboardStatic:", apiChromaInstance.PutKeyboardStatic(new int?(value)));
		ChromaExample01.LogResult("PutKeypadStatic:", apiChromaInstance.PutKeypadStatic(new int?(value)));
		ChromaExample01.LogResult("PutMouseStatic:", apiChromaInstance.PutMouseStatic(new int?(value)));
		ChromaExample01.LogResult("PutMousepadStatic:", apiChromaInstance.PutMousepadStatic(new int?(value)));
	}

	
	private void SetEffectOnAll(EffectInput input)
	{
		if (!this._mConnectionManager.Connected)
		{
			Debug.LogError("Chroma client is not yet connected!");
			return;
		}
		ChromaApi apiChromaInstance = this._mConnectionManager.ApiChromaInstance;
		ChromaExample01.LogResult("PutChromaLink:", apiChromaInstance.PutChromaLink(input));
		ChromaExample01.LogResult("PutHeadset:", apiChromaInstance.PutHeadset(input));
		ChromaExample01.LogResult("PutKeyboard:", apiChromaInstance.PutKeyboard(input));
		ChromaExample01.LogResult("PutKeypad:", apiChromaInstance.PutKeypad(input));
		ChromaExample01.LogResult("PutMouse:", apiChromaInstance.PutMouse(input));
		ChromaExample01.LogResult("PutMousepad:", apiChromaInstance.PutMousepad(input));
	}

	
	private void SetKeyboardCustomEffect()
	{
		if (!this._mConnectionManager.Connected)
		{
			Debug.LogError("Chroma client is not yet connected!");
			return;
		}
		ChromaApi apiChromaInstance = this._mConnectionManager.ApiChromaInstance;
		ChromaExample01.LogResult("PutChromaLinkCustom:", apiChromaInstance.PutChromaLinkCustom(ChromaUtils.CreateRandomColors1D(ChromaDevice1DEnum.ChromaLink)));
		ChromaExample01.LogResult("PutHeadsetCustom:", apiChromaInstance.PutHeadsetCustom(ChromaUtils.CreateRandomColors1D(ChromaDevice1DEnum.Headset)));
		ChromaExample01.LogResult("PutKeyboardCustom:", apiChromaInstance.PutKeyboardCustom(ChromaUtils.CreateRandomColors2D(ChromaDevice2DEnum.Keyboard)));
		ChromaExample01.LogResult("PutKeypadCustom:", apiChromaInstance.PutKeypadCustom(ChromaUtils.CreateRandomColors2D(ChromaDevice2DEnum.Keypad)));
		ChromaExample01.LogResult("PutMouseCustom:", apiChromaInstance.PutMouseCustom(ChromaUtils.CreateRandomColors2D(ChromaDevice2DEnum.Mouse)));
		ChromaExample01.LogResult("PutMousepadCustom:", apiChromaInstance.PutMousepadCustom(ChromaUtils.CreateRandomColors1D(ChromaDevice1DEnum.Mousepad)));
	}

	
	private void LoopAnimation1D(ChromaSDKAnimation1D animation)
	{
		if (this._mPlayAnimation)
		{
			animation.PlayWithOnComplete(new ChromaSDKAnimation1D.ChomaOnComplete1D(this.LoopAnimation1D));
		}
	}

	
	private void LoopAnimation2D(ChromaSDKAnimation2D animation)
	{
		if (this._mPlayAnimation)
		{
			animation.PlayWithOnComplete(new ChromaSDKAnimation2D.ChomaOnComplete2D(this.LoopAnimation2D));
		}
	}

	
	private void ValidateAnimation(ChromaSDKBaseAnimation animation)
	{
		List<EffectResponseId> effects = animation.GetEffects();
		if (effects == null || effects.Count == 0)
		{
			Debug.LogError("Animation failed to create effects!");
		}
		else
		{
			for (int i = 0; i < effects.Count; i++)
			{
				EffectResponseId effectResponseId = effects[i];
				if (effectResponseId == null || effectResponseId.Result != 0)
				{
					Debug.LogError("Failed to create effect!");
				}
			}
		}
	}

	
	private void DoAnimations()
	{
		if (!this._mConnectionManager.Connected)
		{
			Debug.LogError("Chroma client is not yet connected!");
			return;
		}
		this._mPlayAnimation = true;
		foreach (ChromaSDKAnimation1D chromaSDKAnimation1D in this._mAnimations1D)
		{
			if (chromaSDKAnimation1D.IsLoaded())
			{
				chromaSDKAnimation1D.Unload();
			}
			chromaSDKAnimation1D.Load();
			this.ValidateAnimation(chromaSDKAnimation1D);
		}
		foreach (ChromaSDKAnimation2D chromaSDKAnimation2D in this._mAnimations2D)
		{
			if (chromaSDKAnimation2D.IsLoaded())
			{
				chromaSDKAnimation2D.Unload();
			}
			chromaSDKAnimation2D.Load();
			this.ValidateAnimation(chromaSDKAnimation2D);
		}
		foreach (ChromaSDKAnimation1D animation in this._mAnimations1D)
		{
			this.LoopAnimation1D(animation);
		}
		foreach (ChromaSDKAnimation2D animation2 in this._mAnimations2D)
		{
			this.LoopAnimation2D(animation2);
		}
	}

	
	private void StopAnimations()
	{
		if (!this._mConnectionManager.Connected)
		{
			Debug.LogError("Chroma client is not yet connected!");
			return;
		}
		foreach (ChromaSDKAnimation1D chromaSDKAnimation1D in this._mAnimations1D)
		{
			if (chromaSDKAnimation1D.IsLoaded())
			{
				chromaSDKAnimation1D.Unload();
			}
		}
		foreach (ChromaSDKAnimation2D chromaSDKAnimation2D in this._mAnimations2D)
		{
			if (chromaSDKAnimation2D.IsLoaded())
			{
				chromaSDKAnimation2D.Unload();
			}
		}
	}

	
	private void Start()
	{
		this._mConnectionManager = ChromaConnectionManager.Instance;
		if (Application.isPlaying)
		{
			for (int i = 0; i < this._mAnimations1D.Length; i++)
			{
				this._mAnimations1D[i] = UnityEngine.Object.Instantiate<ChromaSDKAnimation1D>(this._mAnimations1D[i]);
			}
			for (int j = 0; j < this._mAnimations2D.Length; j++)
			{
				this._mAnimations2D[j] = UnityEngine.Object.Instantiate<ChromaSDKAnimation2D>(this._mAnimations2D[j]);
			}
		}
	}

	
	private void OnGUI()
	{
		if (null == this._mConnectionManager)
		{
			GUILayout.Label("Waiting for start...", new GUILayoutOption[0]);
			return;
		}
		ChromaApi chromaApi = this._mConnectionManager.ApiChromaInstance;
		this._mTextStatus = this._mConnectionManager.ConnectionStatus;
		GUI.enabled = this._mConnectionManager.Connected;
		GUILayout.BeginHorizontal(new GUILayoutOption[0]);
		GUILayout.Label("Unity Plugin - Chroma REST API", new GUILayoutOption[0]);
		GUILayout.FlexibleSpace();
		GUILayout.Label(this._mTextStatus, new GUILayoutOption[0]);
		GUILayout.EndHorizontal();
		GUILayout.Label("Set a static color on all devices", new GUILayoutOption[0]);
		GUILayout.BeginHorizontal(new GUILayoutOption[0]);
		Color backgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = Color.blue;
		if (GUILayout.Button("Blue", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				this.SetEffectStaticOnAll(Color.blue);
			});
		}
		GUI.backgroundColor = Color.green;
		if (GUILayout.Button("Green", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				this.SetEffectStaticOnAll(Color.green);
			});
		}
		GUI.backgroundColor = Color.red;
		if (GUILayout.Button("Red", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				this.SetEffectStaticOnAll(Color.red);
			});
		}
		GUI.backgroundColor = new Color(1f, 0.5f, 0f);
		if (GUILayout.Button("Orange", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				this.SetEffectStaticOnAll(new Color(1f, 0.5f, 0f));
			});
		}
		GUI.backgroundColor = new Color(0f, 1f, 1f);
		if (GUILayout.Button("Aqua", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				this.SetEffectStaticOnAll(new Color(0f, 1f, 1f));
			});
		}
		GUI.backgroundColor = Color.white;
		if (GUILayout.Button("White", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				this.SetEffectStaticOnAll(Color.white);
			});
		}
		GUI.backgroundColor = backgroundColor;
		if (GUILayout.Button("Random", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				this.SetKeyboardCustomEffect();
			});
		}
		if (GUILayout.Button("Clear", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				this.SetEffectNoneOnAll();
			});
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("Set a built-in effect on all devices", new GUILayoutOption[0]);
		GUILayout.BeginHorizontal(new GUILayoutOption[0]);
		if (GUILayout.Button("Breathing 1", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectInput = new EffectInput(EffectType.CHROMA_BREATHING, null);
				effectInput.Param = new EffectInputParam(null, null, null, null, null, null);
				effectInput.Param.Color1 = new int?(ChromaUtils.ToBGR(Color.red));
				effectInput.Param.Color2 = new int?(ChromaUtils.ToBGR(Color.green));
				effectInput.Param.Type = new int?(1);
				this.SetEffectOnAll(effectInput);
			});
		}
		if (GUILayout.Button("Breathing 2", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectInput = new EffectInput(EffectType.CHROMA_BREATHING, null);
				effectInput.Param = new EffectInputParam(null, null, null, null, null, null);
				effectInput.Param.Color1 = new int?(ChromaUtils.ToBGR(Color.green));
				effectInput.Param.Color2 = new int?(ChromaUtils.ToBGR(Color.yellow));
				effectInput.Param.Type = new int?(2);
				this.SetEffectOnAll(effectInput);
			});
		}
		if (GUILayout.Button("Reactive 1", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectInput = new EffectInput(EffectType.CHROMA_REACTIVE, null);
				effectInput.Param = new EffectInputParam(null, null, null, null, null, null);
				effectInput.Param.Color = new int?(ChromaUtils.ToBGR(Color.red));
				effectInput.Param.Duration = new int?(1);
				this.SetEffectOnAll(effectInput);
			});
		}
		if (GUILayout.Button("Reactive 2", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectInput = new EffectInput(EffectType.CHROMA_REACTIVE, null);
				effectInput.Param = new EffectInputParam(null, null, null, null, null, null);
				effectInput.Param.Color = new int?(ChromaUtils.ToBGR(Color.green));
				effectInput.Param.Duration = new int?(2);
				this.SetEffectOnAll(effectInput);
			});
		}
		if (GUILayout.Button("Reactive 3", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectInput = new EffectInput(EffectType.CHROMA_REACTIVE, null);
				effectInput.Param = new EffectInputParam(null, null, null, null, null, null);
				effectInput.Param.Color = new int?(ChromaUtils.ToBGR(Color.blue));
				effectInput.Param.Duration = new int?(3);
				this.SetEffectOnAll(effectInput);
			});
		}
		if (GUILayout.Button("Spectrum Cycling", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectInput = new EffectInput(EffectType.CHROMA_SPECTRUMCYCLING, null);
				effectInput.Param = new EffectInputParam(null, null, null, null, null, null);
				this.SetEffectOnAll(effectInput);
			});
		}
		if (GUILayout.Button("Wave 1", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectInput = new EffectInput(EffectType.CHROMA_WAVE, null);
				effectInput.Param = new EffectInputParam(null, null, null, null, null, null);
				effectInput.Param.Direction = new int?(1);
				this.SetEffectOnAll(effectInput);
			});
		}
		if (GUILayout.Button("Wave 2", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectInput = new EffectInput(EffectType.CHROMA_WAVE, null);
				effectInput.Param = new EffectInputParam(null, null, null, null, null, null);
				effectInput.Param.Direction = new int?(2);
				this.SetEffectOnAll(effectInput);
			});
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("Set a different color to a specific device", new GUILayoutOption[0]);
		GUILayout.BeginHorizontal(new GUILayoutOption[0]);
		GUI.backgroundColor = Color.blue;
		if (GUILayout.Button("Keyboard", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectChromaStatic = ChromaExample01.GetEffectChromaStatic(Color.blue);
				ChromaExample01.LogResult("PutKeyboard:", chromaApi.PutKeyboard(effectChromaStatic));
			});
		}
		GUI.backgroundColor = Color.green;
		if (GUILayout.Button("Headset", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectChromaStatic = ChromaExample01.GetEffectChromaStatic(Color.green);
				ChromaExample01.LogResult("PutHeadset:", chromaApi.PutHeadset(effectChromaStatic));
			});
		}
		GUI.backgroundColor = Color.red;
		if (GUILayout.Button("Mouse", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectChromaStatic = ChromaExample01.GetEffectChromaStatic(Color.red);
				ChromaExample01.LogResult("PutMouse:", chromaApi.PutMouse(effectChromaStatic));
			});
		}
		GUI.backgroundColor = new Color(1f, 0.5f, 0f);
		if (GUILayout.Button("Mousepad", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectChromaStatic = ChromaExample01.GetEffectChromaStatic(new Color(1f, 0.5f, 0f));
				ChromaExample01.LogResult("PutMousepad:", chromaApi.PutMousepad(effectChromaStatic));
			});
		}
		GUI.backgroundColor = new Color(0f, 1f, 1f);
		if (GUILayout.Button("Keypad", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectChromaStatic = ChromaExample01.GetEffectChromaStatic(new Color(0f, 1f, 1f));
				ChromaExample01.LogResult("PutKeypad:", chromaApi.PutKeypad(effectChromaStatic));
			});
		}
		GUI.backgroundColor = Color.white;
		if (GUILayout.Button("ChromaLink", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			ChromaUtils.RunOnThread(delegate
			{
				EffectInput effectChromaStatic = ChromaExample01.GetEffectChromaStatic(Color.white);
				ChromaExample01.LogResult("PutChromaLink:", chromaApi.PutChromaLink(effectChromaStatic));
			});
		}
		GUI.backgroundColor = backgroundColor;
		GUILayout.EndHorizontal();
		GUILayout.Label("Play animation...", new GUILayoutOption[0]);
		GUILayout.BeginHorizontal(new GUILayoutOption[0]);
		if (GUILayout.Button("Start", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			this.DoAnimations();
		}
		if (GUILayout.Button("End", new GUILayoutOption[]
		{
			GUILayout.Height(50f)
		}))
		{
			this._mPlayAnimation = false;
			this.StopAnimations();
		}
		GUILayout.EndHorizontal();
		GUI.enabled = true;
	}

	
	public ChromaSDKAnimation1D[] _mAnimations1D;

	
	public ChromaSDKAnimation2D[] _mAnimations2D;

	
	private ChromaConnectionManager _mConnectionManager;

	
	private string _mTextStatus;

	
	private bool _mPlayAnimation;

	
	private List<Action> _mMainActions = new List<Action>();
}
