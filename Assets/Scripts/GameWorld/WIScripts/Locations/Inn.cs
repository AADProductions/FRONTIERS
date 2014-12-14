using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World
{
		public class Inn : WIScript
		{		//used by beds to tell whether they should charge
				public Signboard Sign;
				public Location location = null;
				public InnKeeper innkeeper = null;
				public Structure structure = null;
				public InnState State = new InnState();

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
								Debug.Log("STRUCTURE WAS NULL IN INN");
								structure = worlditem.Get <Structure>();
						}
						Character structureOwner = structure.StructureOwner;
						innkeeper = structureOwner.worlditem.GetOrAdd <InnKeeper>();
						structure.StructureGroup.Owner = structureOwner.worlditem;
				}

				public int PricePerNight {
						get {
								if (innkeeper != null) {
										return innkeeper.PricePerNight;
								} else {
										return (int)Globals.InnBasePricePerNight;
								}
						}
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
						location.State.Type = "Inn";
				}
				#endif
				public void OnDrawGizmos()
				{
						Gizmos.DrawWireSphere((transform.position + State.SignboardOffset.Position), 0.5f);
				}
		}

		[Serializable]
		public class InnState
		{
				[FrontiersAvailableModsAttribute("Signboard")]
				public string SignboardTexture;
				public STransform SignboardOffset;
		}
}