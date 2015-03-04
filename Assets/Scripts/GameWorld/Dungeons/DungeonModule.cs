using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
	[ExecuteInEditMode]
	public class DungeonModule : MonoBehaviour
	{
		public Dungeon ParentDungeon;
		public WIGroup ModuleGroup;
		public CullingGroup_Manual OcclusionGroup;
		public int ModuleNumber = 0;

		public void Initialize ( )
		{
			name = Dungeon.ModuleName (ParentDungeon.name, ModuleNumber);
			OcclusionGroup.cullingGroupMasterName = name;
			if (Application.isPlaying) {
				ModuleGroup = WIGroups.GetOrAdd (name, ParentDungeon.DungeonGroup, null);
			}
		}

		public void Refresh ()
		{
			ModuleGroup.transform.position = transform.position;
			ModuleGroup.transform.rotation = transform.rotation;
			ModuleGroup.Load ();
		}

		public void BuildModule (StructureTemplateGroup dungeonModuleGroup, bool hasLoadedOnce)
		{
//			Transform normalFX = gameObject.FindOrCreateChild ("__FX");
//			Transform normalCol = gameObject.FindOrCreateChild ("__COLLIDERS");
//			Transform normalWIsGen = gameObject.FindOrCreateChild ("__WORLDITEMS_GENERIC");
//			Transform normalWindowsGen = gameObject.FindOrCreateChild ("__WINDOWS_GENERIC");
//			Transform normalDoorsGen = gameObject.FindOrCreateChild ("__DOORS_GENERIC");
//			Transform normalWIsUnique = gameObject.FindOrCreateChild ("__WORLDITEMS_UNIQUE");
//			Transform normalChrs = gameObject.FindOrCreateChild ("__ACTION_NODES");
//			Transform normalDyn = gameObject.FindOrCreateChild ("__DYNAMIC");
//			Transform normalStat = gameObject.FindOrCreateChild ("__STATIC");
//			Transform normalSub = gameObject.FindOrCreateChild ("__SUBSTRUCTURES");
//			Transform normalWindows = gameObject.FindOrCreateChild ("__WINDOWS");
//			Transform normalTriggers = gameObject.FindOrCreateChild ("__TRIGGERS");
//			//then add the children
//			for (int j = 0; j < dungeonModuleGroup.StaticStructureLayers.Count; j++) {
//				StructureTemplate.InstantiateStructureLayer (dungeonModuleGroup.StaticStructureLayers [j], normalStat);
//			}
//
//			if (!hasLoadedOnce) {
//				StartCoroutine (AddGenericWorldItemsToDungeon (dungeonModuleGroup.GenericWItems, normalWIsGen, false, ModuleGroup));
//				StartCoroutine (AddUniqueWorldItemsToDungeon (dungeonModuleGroup.UniqueWorlditems, normalWIsUnique, ModuleGroup));
//				ParentDungeon.ParentChunk.AddNodesToGroup (dungeonModuleGroup.ActionNodes, ModuleGroup, normalChrs);
//			} else {
//				Debug.Log ("Already loaded once, not spawning objects in " + name);
//			}
//
//			for (int i = 0; i < dungeonModuleGroup.ActionNodes.Count; i++) {
//				ActionNodeState acState = dungeonModuleGroup.ActionNodes [i];
//				if (acState.IsLoaded) {
//					Character character = null;
//					//TODO add our own region flags
//					Characters.SpawnCharacter (acState.actionNode, acState.OccupantName, ParentDungeon.ResidentFlags, ModuleGroup, out character);
//				}
//			}
//
//			WorldChunk chunk = ModuleGroup.GetParentChunk ();
//			foreach (KeyValuePair <string, KeyValuePair <string,string>> triggerStatePair in dungeonModuleGroup.Triggers) {
//				string triggerName = triggerStatePair.Key;
//				string triggerScriptName = triggerStatePair.Value.Key;
//				string triggerState = triggerStatePair.Value.Value;
//
//				GameObject newTriggerObject = normalTriggers.gameObject.CreateChild (triggerName).gameObject;
//				WorldTrigger worldTriggerScript	= newTriggerObject.AddComponent (triggerScriptName) as WorldTrigger;
//				worldTriggerScript.UpdateTriggerState (triggerState, chunk);
//			}
		}

		protected IEnumerator AddUniqueWorldItemsToDungeon (List <StackItem> wiPieces, Transform parentTransform, WIGroup group)
		{
//			foreach (StackItem piece in wiPieces) {	Debug.Log ("Adding piece " + piece.FileName + " to structure " + name);
//				WorldItem newWorldItem = null;
//				if (WorldItems.CloneFromStackItem (piece, group, out newWorldItem)) {
//					newWorldItem.tr.parent = group.transform;
//					newWorldItem.Initialize ();
//					piece.Props.Local.Transform.ApplyTo (newWorldItem.transform);
//				}
//			}
			yield break;
		}

		protected IEnumerator AddGenericWorldItemsToDungeon (string pieces, Transform parentTransform, bool exterior, WIGroup group)
		{
//			ChildPiece[] childPieces = StructureTemplate.ExtractChildPiecesFromLayer (pieces);
//			for (int i = 0; i < childPieces.Length; i++) {
//				ChildPiece childPiece = childPieces [i];
//				////Debug.Log ("Adding worlditem " + childPiece.ChildName + " to " + name + " at " + childPiece.Position + " exterior? " + exterior.ToString ());
//				WorldItem worlditem = null;
//				if (WorldItems.CloneWorldItem (childPiece.PackName, childPiece.ChildName, childPiece.Transform, false, group, out worlditem)) {
//					worlditem.tr.parent = group.transform;
//					worlditem.Initialize ();
//					worlditem.Props.Local.Transform.ApplyTo (worlditem.tr);
//				} else {
//					Debug.Log ("Couldn't clone generic world item " + childPiece.PackName + ", " + childPiece.ChildName);
//				}
//				worlditem.Props.Global.ParentUnderGroup = false;
//				worlditem.transform.parent = parentTransform;
//				if (!worlditem.CanEnterInventory && !worlditem.CanBeCarried) {	//for large pieces of furniture, etc.
//					if (exterior) {
//						mExteriorRenderers.AddRange (worlditem.Renderers);
//					} else {
//						mInteriorRenderers.AddRange (worlditem.Renderers);
//					}
//				}
//			}
			//TODO split this up
			yield break;
		}

		public void EditorSaveModuleToTemplate (StructureTemplateGroup moduleInteriorGroup)
		{
			Transform normalFX = gameObject.FindOrCreateChild ("__FX");
			Transform normalCol = gameObject.FindOrCreateChild ("__COLLIDERS");
			Transform normalWIsGen = gameObject.FindOrCreateChild ("__WORLDITEMS_GENERIC");
			Transform normalWindowsGen = gameObject.FindOrCreateChild ("__WINDOWS_GENERIC");
			Transform normalDoorsGen = gameObject.FindOrCreateChild ("__DOORS_GENERIC");
			Transform normalWIsUnique = gameObject.FindOrCreateChild ("__WORLDITEMS_UNIQUE");
			Transform normalChrs = gameObject.FindOrCreateChild ("__ACTION_NODES");
			Transform normalDyn = gameObject.FindOrCreateChild ("__DYNAMIC");
			Transform normalStat = gameObject.FindOrCreateChild ("__STATIC");
			Transform normalSub = gameObject.FindOrCreateChild ("__SUBSTRUCTURES");
			Transform normalWindows = gameObject.FindOrCreateChild ("__WINDOWS");
			Transform normalTriggers = gameObject.FindOrCreateChild ("__TRIGGERS");

			StructureTemplate.AddStaticChildrenToStructureTemplate (normalStat, moduleInteriorGroup.StaticStructureLayers);
			StructureTemplate.AddGenericWorldItemsToStructureTemplate (normalWIsGen, ref moduleInteriorGroup.GenericWItems);
			StructureTemplate.AddUniqueWorldItemsToStructureTemplate (normalWIsUnique, moduleInteriorGroup.UniqueWorlditems);
			StructureTemplate.AddActionNodesToTemplate (normalChrs, moduleInteriorGroup.ActionNodes);
			StructureTemplate.AddTriggersToStructureTemplate (normalTriggers, moduleInteriorGroup.Triggers);
		}
	}
}
