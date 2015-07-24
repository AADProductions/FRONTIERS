using UnityEngine;
using System.Collections;
using System;
using Frontiers.World.WIScripts;

namespace Frontiers.World.WIScripts
{
	public class StructureTerrainTypeController : WIScript {
		public Structure structure;

		public StructureTerrainTypeControllerState State = new StructureTerrainTypeControllerState ( );

		public override void OnInitialized ()
		{
			structure = worlditem.Get <Structure> ();
			structure.OnInteriorLoaded += OnInteriorLoaded;
			structure.OnExteriorLoaded += OnExteriorLoaded;
			structure.OnStructureDestroyed += OnStructureDestroyed;
			Player.Get.AvatarActions.Subscribe (AvatarAction.LocationUndergroundEnter, LocationUndergroundEnter);
			Player.Get.AvatarActions.Subscribe (AvatarAction.LocationUndergroundExit, LocationUndergroundExit);
		}

		public void OnStructureDestroyed ( ) {
			Finish ();
		}

		public void OnInteriorLoaded ( ) {
			//set the interior renderers
			switch (State.InteriorTerrainType) {
			case LocationTerrainType.AboveGround:
				structure.DisplayInterior = !Player.Local.Surroundings.IsUnderground;
				break;

			case LocationTerrainType.BelowGround:
				structure.DisplayInterior = Player.Local.Surroundings.IsUnderground;
				break;

			default://transition
				structure.DisplayInterior = true;
				break;
			}
			//the structure will refresh its colliders immediately afterwards
		}

		public void OnExteriorLoaded ( ) {
			//set the exterior renderers
			switch (State.ExteriorTerrainType) {
			case LocationTerrainType.AboveGround:
				structure.DisplayExterior = !Player.Local.Surroundings.IsUnderground;
				break;

			case LocationTerrainType.BelowGround:
				structure.DisplayExterior = Player.Local.Surroundings.IsUnderground;
				break;

			default://transition
				structure.DisplayExterior = true;
				break;
			}
			//the structure will refresh its colliders immediately afterwards
		}

		public bool LocationUndergroundEnter (double timeStamp) {
			//Debug.Log ("Location underground enter");
			//set the exterior renderers
			switch (State.ExteriorTerrainType) {
			case LocationTerrainType.AboveGround:
				//Debug.Log ("DO NOT display exterior");
				structure.DisplayExterior = false;
				break;

			case LocationTerrainType.BelowGround:
			default://transition
				//Debug.Log ("DO display exterior");
				structure.DisplayExterior = true;
				break;
			}
			//set the interior renderers
			switch (State.InteriorTerrainType) {
			case LocationTerrainType.AboveGround:
				//Debug.Log ("DO NOT display interior");
				structure.DisplayInterior = false;
				break;

			case LocationTerrainType.BelowGround:
			default://transition
				//Debug.Log ("DO display interior");
				structure.DisplayInterior = true;
				break;
			}
			//force refresh to apply changes
			structure.RefreshColliders (true);
			structure.RefreshRenderers (true);

			return true;
		}

		public bool LocationUndergroundExit (double timeStamp) {
			//Debug.Log ("Location underground exit");
			//set the exterior renderers
			switch (State.ExteriorTerrainType) {
			case LocationTerrainType.AboveGround:
			default://transition
				//Debug.Log ("DO display exterior");
				structure.DisplayExterior = true;
				break;

			case LocationTerrainType.BelowGround:
				//Debug.Log ("DO NOT display exterior");
				structure.DisplayExterior = false;
				break;
			}
			//set the interior renderers
			switch (State.InteriorTerrainType) {
			case LocationTerrainType.AboveGround:
			default://transition
				//Debug.Log ("DO display interior");
				structure.DisplayInterior = true;
				break;

			case LocationTerrainType.BelowGround:
				//Debug.Log ("DO NOT display interior");
				structure.DisplayInterior = false;
				break;
			}
			//force refresh to apply changes
			structure.RefreshColliders (true);
			structure.RefreshRenderers (true);

			return true;
		}
	}

	[Serializable]
	public class StructureTerrainTypeControllerState {
		public LocationTerrainType ExteriorTerrainType = LocationTerrainType.AboveGround;
		public LocationTerrainType InteriorTerrainType = LocationTerrainType.AboveGround;
	}
}
