using System;
using UnityEngine;

namespace TheForest.Utils.Physics
{
	
	public interface IOnCollisionStayProxy
	{
		
		void OnCollisionStayProxied(Collision col);
	}
}
