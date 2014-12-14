using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.World
{
		public class TerrainCollisionDamageDealer : MonoBehaviour
		{
				public float FallDamageMultiplier = 0.25f;

				public void OnCollisionEnter(Collision collision)
				{		
						switch (collision.collider.gameObject.layer) {			
								case Globals.LayerNumWorldItemActive:
										float attemptedDamage = collision.relativeVelocity.magnitude * Globals.DamageSumForceMultiplier * FallDamageMultiplier;
										float actualDamage;
										bool isDead;
										Vector3 damagePoint = collision.contacts[0].point;
										IDamageable damageHandler = (IDamageable)collision.gameObject.GetComponent(typeof(IDamageable));
				
										if (damageHandler == null) {
												return;
										}
			
										damageHandler.TakeDamage(WIMaterialType.Dirt, damagePoint, attemptedDamage, Vector3.zero, "Terrain", out actualDamage, out isDead);
										break;
				
								default:
										break;
						}
				}
		}
}
