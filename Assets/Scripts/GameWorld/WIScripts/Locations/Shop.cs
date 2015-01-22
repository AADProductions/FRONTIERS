using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class Shop : WIScript
		{
				public Signboard Sign;
				public Location location = null;
				public ShopState State = new ShopState();

				public override void OnInitialized()
				{
						Structure structure = worlditem.Get<Structure>();
						//subscribe to actions
						structure.OnOwnerCharacterSpawned += OnOwnerCharacterSpawned;
						structure.OnCharacterSpawned += OnCharacterSpawned;
						location = worlditem.Get <Location>();
						location.OnLocationGroupLoaded += CreateSign;
				}

				public void CreateSign()
				{
						Sign = Signboards.AddShop(Sign, worlditem, location.LocationGroup, State.SignboardOffset, State.SignboardTexture);
				}

				public void OnOwnerCharacterSpawned()
				{	
						//when the shop owner is spawned, set the owner
						Structure structure = worlditem.Get <Structure>();
						WorldItem structureOwner = structure.StructureOwner.worlditem;
						structure.StructureGroup.Owner = structureOwner;
						structureOwner.GetOrAdd <ShopOwner>();
				}

				public void OnCharacterSpawned()
				{

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
						location.State.Type = "Shop";
				}
				#endif
				public void OnDrawGizmos()
				{
						if (State != null && State.SignboardOffset != null) {
								Gizmos.DrawWireSphere((transform.position + State.SignboardOffset.Position), 0.5f);
						}
				}
		}

		[Serializable]
		public class ShopState
		{
				public string OwnerName = string.Empty;
				[BitMask(typeof(TimeOfDay))]
				public TimeOfDay OperatingHours = TimeOfDay.db_WorkWorkingHour;
				public string Type = "General";
				[FrontiersAvailableModsAttribute("Signboard")]
				public string SignboardTexture;
				public STransform SignboardOffset;
		}
}