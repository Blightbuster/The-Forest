using System;
using ChromaSDK.ChromaPackage.Model;

namespace ChromaSDK.Api
{
	
	public interface IChromaApi
	{
		
		DeleteChromaSdkResponse DeleteChromaSdk();

		
		void Heartbeat();

		
		EffectResponseId PostChromaLink(EffectInput data);

		
		EffectResponseId PostChromaLinkCustom(EffectArray1dInput data);

		
		EffectResponseId PostChromaLinkNone();

		
		EffectResponseId PostChromaLinkStatic(int? color);

		
		EffectResponseId PostHeadset(EffectInput data);

		
		EffectResponseId PostHeadsetCustom(EffectArray1dInput data);

		
		EffectResponseId PostHeadsetNone();

		
		EffectResponseId PostHeadsetStatic(int? color);

		
		EffectResponseId PostKeyboard(EffectInput data);

		
		EffectResponseId PostKeyboardCustom(EffectArray2dInput data);

		
		EffectResponseId PostKeyboardNone();

		
		EffectResponseId PostKeyboardStatic(int? color);

		
		EffectResponseId PostKeypad(EffectInput data);

		
		EffectResponseId PostKeypadCustom(EffectArray2dInput data);

		
		EffectResponseId PostKeypadNone();

		
		EffectResponseId PostKeypadStatic(int? color);

		
		EffectResponseId PostMouse(EffectInput data);

		
		EffectResponseId PostMouseCustom(EffectArray2dInput data);

		
		EffectResponseId PostMouseNone();

		
		EffectResponseId PostMouseStatic(int? color);

		
		EffectResponseId PostMousepad(EffectInput data);

		
		EffectResponseId PostMousepadCustom(EffectArray1dInput data);

		
		EffectResponseId PostMousepadNone();

		
		EffectResponseId PostMousepadStatic(int? color);

		
		EffectResponse PutChromaLink(EffectInput data);

		
		EffectResponse PutChromaLinkCustom(EffectArray1dInput data);

		
		EffectResponse PutChromaLinkNone();

		
		EffectResponse PutChromaLinkStatic(int? color);

		
		EffectIdentifierResponse PutEffect(EffectIdentifierInput data);

		
		EffectResponse PutHeadset(EffectInput data);

		
		EffectResponse PutHeadsetCustom(EffectArray1dInput data);

		
		EffectResponse PutHeadsetNone();

		
		EffectResponse PutHeadsetStatic(int? color);

		
		EffectResponse PutKeyboard(EffectInput data);

		
		EffectResponse PutKeyboardCustom(EffectArray2dInput data);

		
		EffectResponse PutKeyboardNone();

		
		EffectResponse PutKeyboardStatic(int? color);

		
		EffectResponse PutKeypad(EffectInput data);

		
		EffectResponse PutKeypadCustom(EffectArray2dInput data);

		
		EffectResponse PutKeypadNone();

		
		EffectResponse PutKeypadStatic(int? color);

		
		EffectResponse PutMouse(EffectInput data);

		
		EffectResponse PutMouseCustom(EffectArray2dInput data);

		
		EffectResponse PutMouseNone();

		
		EffectResponse PutMouseStatic(int? color);

		
		EffectResponse PutMousepad(EffectInput data);

		
		EffectResponse PutMousepadCustom(EffectArray1dInput data);

		
		EffectResponse PutMousepadNone();

		
		EffectResponse PutMousepadStatic(int? color);

		
		EffectIdentifierResponse RemoveEffect(EffectIdentifierInput data);
	}
}
