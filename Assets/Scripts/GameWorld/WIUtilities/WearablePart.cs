using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;

public class WearablePart : MonoBehaviour
{		//accompanies a body part
		//used to put wearable items on characters / creatures
		[BitMaskAttribute(typeof(WearableType))]
		public WearableType Type = WearableType.Armor;
		public BodyOrientation Orientation = BodyOrientation.Both;
		public BodyPartType BodyPart = BodyPartType.Arm;
		public WorldItem Occupant;

		public bool IsOccupied { get { return Occupant != null; } }

		public void OnDrawGizmos()
		{
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(transform.position, 0.015f);
		}
}
