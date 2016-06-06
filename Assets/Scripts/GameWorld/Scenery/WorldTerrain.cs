using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
	//this helps us to treat the terrain as a spot to place things etc
	//TODO see if TerrainCollider is still necessary?
	public class WorldTerrain : MonoBehaviour, IItemOfInterest
	{
		public ItemOfInterestType IOIType { get { return ItemOfInterestType.Scenery; } }

		public Vector3 Position { get { return transform.position; } }

		public Vector3 FocusPosition { get { return transform.position; } }

		public bool Has (string scriptName)
		{
			return false;
		}

		public bool HasAtLeastOne (List <string> scriptNames)
		{
			return scriptNames.Count == 0;
		}

		public WorldItem worlditem { get { return null; } }

		public PlayerBase player { get { return null; } }

		public ActionNode node { get { return null; } }

		public WorldLight worldlight { get { return null; } }

		public Fire fire { get { return null; } }

		public bool Destroyed { get { return false; } }

		public bool HasPlayerFocus { get; set; }
	}
}