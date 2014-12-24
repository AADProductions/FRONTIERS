using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers
{
		public class PlayerEncounterer : MonoBehaviour
		{
				public SphereCollider EncounterCollider;

				public void Awake()
				{
						gameObject.layer = Globals.LayerNumTrigger;
						EncounterCollider = gameObject.AddComponent <SphereCollider>();
						EncounterCollider.radius = Globals.PlayerEncounterRadius;
						EncounterCollider.isTrigger = true;
						Rigidbody rb = gameObject.AddComponent <Rigidbody>();
						rb.isKinematic = true;
				}

				public PlayerBase TargetPlayer;
				public static List <string> ObstructionTypes = new List <string>() {
						"Obstruction",
						"Trap",
						"Machine",
						"Creature"
				};
				public HashSet <WorldItem> LastItemsEncountered = new HashSet <WorldItem>();
				protected IItemOfInterest mIoiCheck = null;

				public void OnTriggerEnter (Collider other)
				{
						if (other.isTrigger) {
								return;
						}

						bool sendAvatarAction = false;
						switch (other.gameObject.layer) {
								case Globals.LayerNumWorldItemActive:
										mIoiCheck = null;
										if (WorldItems.GetIOIFromCollider(other, out mIoiCheck) && mIoiCheck.IOIType == ItemOfInterestType.WorldItem) {
												if (mIoiCheck.HasAtLeastOne(ObstructionTypes)) {
														LastItemsEncountered.Add(mIoiCheck.worlditem);
														mCleanListNextFrame = true;
														sendAvatarAction = true;
												}
												//always send OnPlayerEncounter regardless of whether it's an obstruction
												//this is used by creatures and plants etc.
												mIoiCheck.worlditem.OnPlayerEncounter.SafeInvoke();
												if (sendAvatarAction) {
														//only send the avatar action if we've encountered one of the obstruction types
														Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.PathEncounterObstruction), WorldClock.Time);
												}
										}
										break;

								case Globals.LayerNumSolidTerrain:
										//for scenery scripts
										//we don't use send message often but in this case it makes sense to
//										if (other.attachedRigidbody != null) {
//												other.attachedRigidbody.gameObject.SendMessage("OnPlayerEncounter", SendMessageOptions.DontRequireReceiver);
//										}
										break;
				
								default:
										break;
						}
				}

				protected bool mCleanListNextFrame = false;

				public void LateUpdate()
				{
						transform.position = TargetPlayer.Position;

						if (mCleanListNextFrame) {
								LastItemsEncountered.Clear();
								mCleanListNextFrame = false;
						}
				}
		}
}