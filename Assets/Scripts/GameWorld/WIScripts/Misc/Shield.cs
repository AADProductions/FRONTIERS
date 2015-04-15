using UnityEngine;
using System.Collections;
using Frontiers.World.WIScripts;

namespace Frontiers.World.WIScripts
{
		public class Shield : WIScript
		{
				Damageable damageable;
				Equippable equippable;
				public float ShakeAmount = 0f;

				public override void OnInitialized()
				{		//set up our equippable and damageable props
						//they have to be just so
						damageable = worlditem.GetOrAdd <Damageable>();
						equippable = worlditem.GetOrAdd <Equippable>();
						//we want to be equipped as a worlditem, always
						equippable.UseDoppleganger = false;
						equippable.Type = PlayerToolType.Generic;

						damageable.OnTakeDamage += OnTakeDamage;
						//when the shield is gone it's gone
						damageable.State.Result = DamageableResult.RemoveFromGame;
				}

				public void OnTakeDamage()
				{
						if (worlditem.Is(WIMode.Equipped)) {
								//TODO move these magic numbers into Globals or the object's state
								float magnitude = ((Vector3)damageable.State.LastDamageForce).magnitude;
								//shake the tool / carry item to reflect the damage
								if (Player.Local.Tool.worlditem == worlditem) {
										Player.Local.Tool.FPSWeapon.AddForce(Player.Local.FocusVector * 0.005f);//push back
										DamageManager.Get.PlayDamageSound(Player.Local.Tool.tr.position, worlditem.Props.Global.MaterialType, 1f);
										DamageManager.Get.SpawnDamageFX(Player.Local.Tool.tr.position, worlditem.Props.Global.MaterialType, 1f);
								} else if (Player.Local.Carrier.worlditem == worlditem) {
										Player.Local.Carrier.FPSWeapon.AddForce(Player.Local.FocusVector * 0.005f);//push back
										DamageManager.Get.PlayDamageSound(Player.Local.Carrier.tr.position, worlditem.Props.Global.MaterialType, 1f);
										DamageManager.Get.SpawnDamageFX(Player.Local.Carrier.tr.position, worlditem.Props.Global.MaterialType, 1f);
								}
								Player.Local.FPSCamera.DoBomb(Vector3.one, 0.00001f * magnitude, 0.0001f * magnitude);
								Player.Local.FPSController.AddSoftForce(Vector3.one * magnitude * 0.15f, 3f);
						}
				}
		}
}