using System;
using System.Collections.Generic;
using Serialization;
using UnityEngine;


[Serializer(typeof(AudioClip))]
[Serializer(typeof(AnimationClip))]
[Serializer(typeof(Font))]
[SubTypeSerializer(typeof(Mesh))]
[Serializer(typeof(TextAsset))]
[SubTypeSerializer(typeof(Texture))]
public class SerializeAssetReference : SerializerExtensionBase<object>
{
	
	public override IEnumerable<object> Save(object target)
	{
		return new object[]
		{
			SaveGameManager.Instance.GetAssetId(target as UnityEngine.Object)
		};
	}

	
	public override bool CanBeSerialized(Type targetType, object instance)
	{
		return instance == null || typeof(UnityEngine.Object).IsAssignableFrom(targetType);
	}

	
	public override object Load(object[] data, object instance)
	{
		return SaveGameManager.Instance.GetAsset((SaveGameManager.AssetReference)data[0]);
	}

	
	public static SerializeAssetReference instance = new SerializeAssetReference();
}
