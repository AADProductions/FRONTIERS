using UnityEngine;
using System;
using System.Collections;

namespace Frontiers.World.WIScripts
{
		public class Bar : WIScript
		{
				public Signboard Sign;
				public Location location = null;
				public BarState State = new BarState();
				public Structure structure = null;
				public Bartender bartender = null;

				public override void OnInitialized()
				{
						Structure structure = null;
						if (worlditem.Is <Structure>(out structure)) {
								structure.OnOwnerCharacterSpawned += OnOwnerCharacterSpawned;
						}
						location = worlditem.Get <Location>();
						location.OnLocationGroupLoaded += CreateSign;
				}

				public void CreateSign()
				{
						Sign = Signboards.AddInn(Sign, worlditem, location.LocationGroup, State.SignboardOffset, State.SignboardTexture);
				}

				public void OnOwnerCharacterSpawned()
				{
						if (structure == null) {
								//shouldn't happen, let us know
								Debug.Log("STRUCTURE WAS NULL IN BAR");
								structure = worlditem.Get <Structure>();
						}
						Character structureOwner = structure.StructureOwner;
						bartender = structureOwner.worlditem.GetOrAdd <Bartender>();
						structure.StructureGroup.Owner = structureOwner.worlditem;
				}

				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						Transform signboard = transform.FindChild("Signboard");
						if (signboard != null) {
								State.SignboardOffset.CopyFrom(signboard);
						} else {
								Structure parentStructure = gameObject.GetComponent <Structure>();
								signboard = parentStructure.CreateSignboardTransform();
								signboard.transform.parent = parentStructure.transform;
								State.SignboardOffset.CopyFrom(signboard);
								signboard.transform.parent = parentStructure.StructureBase.transform;
						}
						Location location = gameObject.GetComponent <Location>();
						location.State.Type = "Tavern";
				}
				#endif
				public void OnDrawGizmos()
				{
						if (State.SignboardOffset != null) {
								Gizmos.DrawWireSphere((transform.position + State.SignboardOffset.Position), 0.5f);
						}
				}
		}

		[Serializable]
		public class BarState
		{
				[FrontiersAvailableModsAttribute("Signboard")]
				public string SignboardTexture;
				public STransform SignboardOffset;
		}
}