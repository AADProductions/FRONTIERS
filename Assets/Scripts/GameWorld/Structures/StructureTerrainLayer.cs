using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
		//used by player surroundings to catch terrain raycasts
		public class StructureTerrainLayer : MonoBehaviour, IItemOfInterest
		{
				public Transform tr;
				//public Rigidbody rb;
				public void Awake()
				{
						gameObject.layer = Globals.LayerNumStructureTerrain;
						tr = transform;
				}

				public ItemOfInterestType IOIType { get { return ItemOfInterestType.Scenery; } }

				public Vector3 Position { get { return tr.position; } }

				public bool Has(string scriptName)
				{
						return false;
				}

				public bool HasAtLeastOne(List <string> scriptNames)
				{
						return false;
				}

				public WorldItem worlditem { get { return null; } }

				public PlayerBase player { get { return null; } }

				public ActionNode node { get { return null; } }

				public WorldLight worldlight { get { return null; } }

				public Fire fire { get { return null; } }

				public bool Destroyed { get { return mDestroyed; } }

				public bool HasPlayerFocus { get; set; }

				public void OnDestroy()
				{
						mDestroyed = true;
				}

				protected bool mDestroyed = false;
		}
}
