using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Data;

namespace Frontiers.World.WIScripts
{
		public class Dynamic : WIScript
		{		//anything that's part of a structure that moves
				//facilitates structure owner ship and the linking up of triggers
				//doors and windows are the most common
				public Structure ParentStructure = null;
				public DynamicObjectState State = new DynamicObjectState();
				public List <Trigger> Triggers = new List<Trigger>();
				public DynamicPrefab DynamicPrefabBase;

				public bool UsesDynamicePrefabBase {
						get {
								return DynamicPrefabBase != null;
						}
				}

				public override bool CanBeCarried {
						get {
								return false;
						}
				}

				public override bool CanEnterInventory {
						get {
								return false;
						}
				}

				public bool TriggersLoaded {
						get {
								return mTriggersLoaded;
						}
				}

				public Action OnTriggersLoaded;

				public void OnAddedToGroup()
				{
						//this will be the case the second time it's spawned
						ParentStructure = worlditem.Group.GetParentStructure();
						//if the structure has already loaded, this will call on structure loaded
						ParentStructure.AddDynamicPrefab(this);
						ParentStructure.OnInteriorLoaded += OnStructureLoaded;
						ParentStructure.OnExteriorLoaded += OnStructureLoaded;
				}

				public override void OnInitialized()
				{
						worlditem.OnAddedToGroup += OnAddedToGroup;
				}

				public void OnStructureLoaded()
				{
						Triggers.Clear();
						Trigger trigger = null;
						if (worlditem.Is <Trigger>(out trigger)) {
								Triggers.Add(trigger);
						}
						for (int i = 0; i < State.TriggerNames.Count; i++) {
								if (ParentStructure.GetDynamicTrigger(State.TriggerNames[i], out trigger)) {
										Triggers.SafeAdd(trigger);
								}
						}
						mTriggersLoaded = true;
						OnTriggersLoaded.SafeInvoke();
				}

				//TODO do we really need these in here any more? move them into door / window?
				public void OnPassThrough()
				{
						if (State.Type == WorldStructureObjectType.OuterEntrance) {
								Player.Local.Surroundings.PassThroughEntrance(this, true);
						}
				}

				public void OnPassThroughEnter()
				{
						if (State.Type == WorldStructureObjectType.OuterEntrance) {
								Player.Local.Surroundings.PassThroughEntrance(this, true);
						}
				}

				public void OnPassThroughExit()
				{
						if (State.Type == WorldStructureObjectType.OuterEntrance) {
								Player.Local.Surroundings.PassThroughEntrance(this, false);
						}
				}

				protected bool mTriggersLoaded = false;

				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						if (Triggers.Count > 0) {
								State.TriggerNames.Clear();
								for (int i = 0; i < Triggers.Count; i++) {
										if (Triggers[i] != null) {
												WorldItem TriggerWorlditem = Triggers[i].GetComponent <WorldItem>();
												State.TriggerNames.Add(TriggerWorlditem.FileName);
										}
								}
						}
				}
				#endif
		}

		[Serializable]
		public class DynamicObjectState
		{
				public string Name = string.Empty;
				public List <string> TriggerNames = new List <string>();
				public WorldStructureObjectType Type = WorldStructureObjectType.Machine;
				public bool MakePublic = false;

				public bool HasBeenEncountered {
						get { return NumTimesEncountered > 0; }
				}

				public bool HasBeenEnabled {
						get { return NumTimesEnabled > 0; }
				}

				public bool HasBeenDisabled {
						get { return NumTimesDisabled > 0; }
				}

				public int NumTimesEncountered = 0;
				public int NumTimesDisabled = 0;
				public int NumTimesEnabled = 0;
				public float TimeFirstEncountered = 0.0f;
				public float TimeFirstTriggered = 0.0f;
				public float TimeFirstEnabled = 0.0f;
				public float TimeFirstDisabled = 0.0f;
				public float TimeLastEncountered = 0.0f;
				public float TimeLastTriggered = 0.0f;
				public float TimeLastEnabled = 0.0f;
				public float TimeLastDisabled = 0.0f;
		}
}