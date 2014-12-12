using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World {
	public class Builder : MonoBehaviour {

		public BuilderState State;
		public BuilderMode Mode;
		public GameObject StructureBase;
		public Transform StructurePiece;
		public MinorStructure MinorParent;
		public StructureLoadPriority Priority;
		public Dictionary <int,Material> MaterialLookup;// = new Dictionary <int,Material> ();

		public void Awake ( ) {
			MaterialLookup = new Dictionary<int, Material> ();
		}

		public enum BuilderState
		{
			Dormant,
			Initialized,
			BuildingMeshes,
			WaitingForMeshes,
			HandlingMeshes,
			BuildingItems,
			Finished,
			Error,
		}

		public enum BuilderMode
		{
			Exterior,
			Interior,
			Minor,
		}

	}
}
