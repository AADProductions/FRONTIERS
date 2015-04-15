using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.World.WIScripts
{
	public class CollisionDamageDealer : WIScript
	{
		public virtual void OnCollisionEnter (Collision collision)
		{			
			float attemptedDamage = collision.relativeVelocity.magnitude * Globals.DamageSumForceMultiplier * rigidbody.mass;
			float actualDamage;
			bool isDead;
			Vector3 damagePoint = collision.contacts [0].point;
			
			switch (collision.collider.gameObject.layer) {
			case Globals.LayerNumPlayer:
	//			//Debug.Log ("Dealing " + attemptedDamage + " damage to player!");
				Player.Local.DamageHandler.TakeDamage (WIMaterialType.Dirt, damagePoint, attemptedDamage, Vector3.zero, "Collision", out actualDamage, out isDead);
				break;
				
			case Globals.LayerNumWorldItemActive:					
				IDamageable damageHandler = (IDamageable)collision.gameObject.GetComponent (typeof(IDamageable));
				
				if (damageHandler == null) {
					return;
				}
				
				damageHandler.TakeDamage (WIMaterialType.Dirt, damagePoint, attemptedDamage, Vector3.zero, "Collision", out actualDamage, out isDead);
				break;
				
			default:
				break;
			}
		}
	}
}