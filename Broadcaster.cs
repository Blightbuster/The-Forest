using System;
using System.Collections.Generic;
using System.Reflection;
using UniLinq;


public static class Broadcaster
{
	
	public static void RegisterInterest(this object target, object interestedParty)
	{
		Broadcaster.Cleanup();
		Broadcaster.InterestList.Add(new Broadcaster.Pair
		{
			target = new WeakReference(target),
			interest = new WeakReference(interestedParty)
		});
	}

	
	public static void UnregisterInterest(this object target, object interestedParty)
	{
		Broadcaster.Cleanup();
		Broadcaster.InterestList.Remove(Broadcaster.InterestList.FirstOrDefault((Broadcaster.Pair p) => p.target.Target == target && p.interest.Target == interestedParty));
	}

	
	public static void Broadcast(this object obj, string message)
	{
		Broadcaster.Cleanup();
		foreach (WeakReference weakReference in (from p in Broadcaster.InterestList
		where p.target.Target == obj
		select p.interest into r
		where r.IsAlive
		select r).ToList<WeakReference>())
		{
			MethodInfo method = weakReference.Target.GetType().GetMethod(message, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method.GetParameters().Length == 1)
			{
				method.Invoke(weakReference.Target, new object[]
				{
					obj
				});
			}
			else
			{
				method.Invoke(weakReference.Target, null);
			}
		}
	}

	
	private static void Cleanup()
	{
		List<Broadcaster.Pair> list = (from k in Broadcaster.InterestList
		where !k.target.IsAlive || !k.interest.IsAlive
		select k).ToList<Broadcaster.Pair>();
		foreach (Broadcaster.Pair item in list)
		{
			Broadcaster.InterestList.Remove(item);
		}
	}

	
	private static List<Broadcaster.Pair> InterestList = new List<Broadcaster.Pair>();

	
	public class Pair
	{
		
		public WeakReference target;

		
		public WeakReference interest;
	}
}
