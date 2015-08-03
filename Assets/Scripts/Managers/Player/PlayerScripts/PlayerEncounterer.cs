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
		Rigidbody rb;

		public void Awake ()
		{
			gameObject.layer = Globals.LayerNumAwarenessReceiver;
			EncounterCollider = gameObject.AddComponent <SphereCollider> ();
			EncounterCollider.radius = Globals.PlayerEncounterRadius;
			EncounterCollider.isTrigger = true;
			rb = gameObject.AddComponent <Rigidbody> ();
			rb.isKinematic = true;
		}

		public PlayerBase TargetPlayer;
		public static List <string> ObstructionTypes = new List <string> () {
			"Obstruction",
			"Trap",
			"Machine",
			"Creature",
			"ObexTransmitter",
			"ObexKey",
		};
		public HashSet <WorldItem> LastItemsEncountered = new HashSet <WorldItem> ();
		protected IItemOfInterest mIoiCheck = null;

		public void HandleControllerCollision (Collider other) {
			if (WorldItems.GetIOIFromCollider (other, out mIoiCheck) && mIoiCheck.IOIType == ItemOfInterestType.WorldItem) {
				if (mIoiCheck.HasAtLeastOne (ObstructionTypes)) {
					LastItemsEncountered.Add (mIoiCheck.worlditem);
				}
				//always send OnPlayerCollide regardless of whether it's an obstruction
				//this is used by creatures and plants etc.
				mIoiCheck.worlditem.OnPlayerCollide.SafeInvoke ();
			}
		}

		public void OnTriggerEnter (Collider other)
		{
			if (other.isTrigger) {
				return;
			}

			bool sendAvatarAction = false;
			if (WorldItems.GetIOIFromCollider (other, out mIoiCheck) && mIoiCheck.IOIType == ItemOfInterestType.WorldItem) {
				if (mIoiCheck.HasAtLeastOne (ObstructionTypes)) {
					LastItemsEncountered.Add (mIoiCheck.worlditem);
					mCleanListNextFrame = true;
					sendAvatarAction = true;
				}
				//always send OnPlayerEncounter regardless of whether it's an obstruction
				//this is used by creatures and plants etc.
				mIoiCheck.worlditem.OnPlayerEncounter.SafeInvoke ();
				if (sendAvatarAction) {
					//only send the avatar action if we've encountered one of the obstruction types
					Player.Get.AvatarActions.ReceiveAction ((AvatarAction.PathEncounterObstruction), WorldClock.AdjustedRealTime);
				}
			}
		}

		protected bool mCleanListNextFrame = false;

		public void LateUpdate ()
		{
			rb.MovePosition (TargetPlayer.Position);

			if (mCleanListNextFrame) {
				LastItemsEncountered.Clear ();
				mCleanListNextFrame = false;
			}
		}
	}
}