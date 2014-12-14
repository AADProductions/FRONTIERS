using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World
{
		public class DungeonEntrance : WIScript
		{		//dungeons have been removed for now
				public DungeonEntranceState State = new DungeonEntranceState();
				public Dungeon ParentDungeon;

				public bool HasParentDungeon {
						get {
								return ParentDungeon != null;
						}
				}

				public Transform EntranceStructureParent = null;

				public override void OnInitialized()
				{
						worlditem.OnVisible += Refresh;
						worlditem.OnActive += Refresh;
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						foreach (Transform child in transform) {
								if (child.name.Contains("-STR")) {
										//this is our structure template
										State.EntranceStructure.TemplateName = StructureBuilder.GetTemplateName(child.name);
										State.EntranceStructure.Position = child.localPosition;
										State.EntranceStructure.Rotation = child.localRotation.eulerAngles;
										UnityEditor.EditorUtility.SetDirty(this);
										UnityEditor.EditorUtility.SetDirty(gameObject);
										break;
								}
						}
				}
				#endif
				public void Refresh()
				{
						if (!HasParentDungeon) {
								//if dungeon chunk is -1 then it can be found in our chunk
								WorldChunk chunk = null;
								if (State.DungeonChunkID < 0) {
										chunk = worlditem.Group.GetParentChunk();
								} else {
										if (!GameWorld.Get.ChunkByID(State.DungeonChunkID, out chunk)) {
												Debug.Log("Couldn't get dungeon parent chunk in dungeon entrance " + name);
										}
								}
								if (!chunk.GetOrCreateDungeon(State.DungeonName, out ParentDungeon)) {
										Debug.Log("Couldn't get parent dungeon in dungeon entrance " + name);
										return;
								}
						}
						ParentDungeon.OnEntranceVisible();
						Structures.AddMinorToload(State.EntranceStructure, 0, worlditem);
				}
		}

		[Serializable]
		public class DungeonEntranceState
		{
				public string DungeonName;
				public MinorStructure EntranceStructure = new MinorStructure();
				public int DungeonChunkID = -1;
		}
}
