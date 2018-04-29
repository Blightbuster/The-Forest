using System;
using Bolt;


internal class CoopEnemyHealthProxy : EntityBehaviour
{
	
	private void takeDamage(int direction)
	{
		this.hitDir = direction;
	}

	
	private void Hit(int damage)
	{
	}

	
	private int hitDir;
}
