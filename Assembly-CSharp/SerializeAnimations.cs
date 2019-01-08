using System;
using System.Collections.Generic;
using Serialization;
using UniLinq;
using UnityEngine;

[ComponentSerializerFor(typeof(Animation))]
public class SerializeAnimations : IComponentSerializer
{
	public byte[] Serialize(Component component)
	{
		return UnitySerializer.Serialize((from AnimationState a in (Animation)component
		select new SerializeAnimations.StoredState
		{
			data = UnitySerializer.SerializeForDeserializeInto(a),
			name = a.name,
			asset = SaveGameManager.Instance.GetAssetId(a.clip)
		}).ToList<SerializeAnimations.StoredState>());
	}

	public void Deserialize(byte[] data, Component instance)
	{
		UnitySerializer.AddFinalAction(delegate
		{
			Animation animation = (Animation)instance;
			animation.Stop();
			Dictionary<string, AnimationState> dictionary = animation.Cast<AnimationState>().ToDictionary((AnimationState a) => a.name);
			List<SerializeAnimations.StoredState> list = UnitySerializer.Deserialize<List<SerializeAnimations.StoredState>>(data);
			foreach (SerializeAnimations.StoredState storedState in list)
			{
				if (storedState.asset != null && !dictionary.ContainsKey(storedState.name))
				{
					animation.AddClip(SaveGameManager.Instance.GetAsset(storedState.asset) as AnimationClip, storedState.name);
				}
				if (storedState.name.Contains(" - Queued Clone"))
				{
					AnimationState instance2 = animation.PlayQueued(storedState.name.Replace(" - Queued Clone", string.Empty));
					UnitySerializer.DeserializeInto(storedState.data, instance2);
				}
				else
				{
					UnitySerializer.DeserializeInto(storedState.data, animation[storedState.name]);
				}
			}
		});
	}

	public class StoredState
	{
		public byte[] data;

		public string name;

		public SaveGameManager.AssetReference asset;
	}
}
