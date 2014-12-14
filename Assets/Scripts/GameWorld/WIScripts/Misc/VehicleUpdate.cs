using UnityEngine;
using System.Collections;
using ExtensionMethods;

namespace Frontiers.World
{
	//updates the position of a vehicle
	public class VehicleUpdate : MonoBehaviour
	{
		public Vehicle ParentVehicle;

		public void Update ( )
		{
			if (ParentVehicle == null || !ParentVehicle.IsOccupied) {
				enabled = false;
				GameObject.Destroy (this);
				return;
			}
		}

		public void UpdateOccupantPosition (Transform occupant)
		{
			transform.position = occupant.position;
			transform.rotation = occupant.rotation;
		}
	}
}