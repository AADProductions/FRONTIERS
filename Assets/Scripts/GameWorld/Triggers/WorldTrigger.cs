#pragma warning disable 0219
using UnityEngine;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using Frontiers.Story;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		[ExecuteInEditMode]
		public class WorldTrigger : MonoBehaviour
		{
				//used to make things happen
				//stored in chunks and in structures
				public WorldChunk ParentChunk;

				public void OnTriggerExit(Collider other)
				{
						if (!Application.isPlaying || !mInitialized) {
								return;
						}

						if (mBaseState.OnExit) {
								HandleTriggerStart(other);
						} else {
								HandleTriggerFinish(other);
						}
				}

				public void OnTriggerEnter(Collider other)
				{
						if (!Application.isPlaying || !mInitialized) {
								return;
						}

						if (mBaseState.OnExit) {
								HandleTriggerFinish(other);
						} else {
								HandleTriggerStart(other);
						}
				}

				//this function causes tons of lag
				//there are better ways to do this stuff i just need to update it
				public void HandleTriggerStart(Collider other)
				{
						//TODO use WorldItems.GetIOIFromCollider
						//replace all this other crap

						bool quit = false;
						switch (mBaseState.Behavior) {
								case AvailabilityBehavior.Once:
										if (mBaseState.NumTimesTriggered > 0) {
												quit = true;
										}
										break;

								case AvailabilityBehavior.Max:
										if (mBaseState.NumTimesTriggered >= mBaseState.MaxTimesTriggered) {
												quit = true;
										}
										break;

								default:
										break;
						}

						if (quit) {
								return;
						}

						bool checkWorldItem = false;
						bool checkPlayer = false;

						switch (other.gameObject.layer) {
								case Globals.LayerNumWorldItemActive:
										checkWorldItem = true;
										break;

								case Globals.LayerNumPlayer:
										checkPlayer = true;
										break;

								default:
										break;
						}

						if (!(checkWorldItem || checkPlayer)) {	//get out before we do anything
								return;
						}

						//okay so we've got a world item layer, now see if we actually have a world item
						WorldItem worlditem = null;
						if (checkWorldItem) {	//see if the actual object is a world item
								if (!other.gameObject.HasComponent <WorldItem>(out worlditem)) {//if the object isn't a worlditem then maybe it's a body part
										BodyPart bodyPart = null;
										if (other.gameObject.HasComponent <BodyPart>(out bodyPart)) {//store the body part's world item for later
												worlditem = bodyPart.Owner.worlditem;
										}
								}
								//did we find one?
								if (worlditem != null) {//if we're still null at this point it's a bust
										if (!mRecentWorldItems.Add(worlditem)) {
												return;
										}
								} else {
										return;
								}
						}

						//okay, next check if we're actually a target
						bool meetsTargetRequirements = true;
						WorldTriggerTarget finalTarget	= WorldTriggerTarget.None;
						mCharacter = null;
						mQuestItem = null;
						if (checkPlayer) {
								finalTarget = WorldTriggerTarget.Player;
								if (!Flags.Check((uint)mBaseState.Targets, (uint)WorldTriggerTarget.Player, Flags.CheckType.MatchAny)) {
										meetsTargetRequirements = false;
								}
						} else if (checkWorldItem) {//worlditems can be characters creatures and all
								if (Flags.Check((uint)mBaseState.Targets, (uint)WorldTriggerTarget.WorldItem, Flags.CheckType.MatchAny)) {//it's just true outright no matter what else it is
										meetsTargetRequirements = true;
										finalTarget = WorldTriggerTarget.WorldItem;
								} else {//otherwise check for quest item, character and creature scripts
										//the heirarchy is WorldItem->Character->Creature->QuestItem
										if (Flags.Check((uint)mBaseState.Targets, (uint)WorldTriggerTarget.Character, Flags.CheckType.MatchAny)) {
												meetsTargetRequirements = worlditem.Has <Character>(out mCharacter);
												finalTarget = WorldTriggerTarget.Character;
										} else if (Flags.Check((uint)mBaseState.Targets, (uint)WorldTriggerTarget.QuestItem, Flags.CheckType.MatchAny)) {
												meetsTargetRequirements = worlditem.Has <QuestItem>(out mQuestItem);
												finalTarget = WorldTriggerTarget.QuestItem;
										}
								}
						}
						//if we made it this far without meeting requirements we're done
						if (!meetsTargetRequirements) {	//get out now
								return;
						}

						if (!WorldClock.Is(mBaseState.TimeOfDayRequirement)) {
								return;
						}

						if (!string.IsNullOrEmpty(mBaseState.QuestItemRequirement) && !Player.Local.Inventory.HasQuestItem(mBaseState.QuestItemRequirement)) {
								return;
						}

						//at this point we're ready to waste time checking if we meet mission requirements
						bool meetsMissionRequirements = true;
						bool missionCompleted = false;
						bool objectiveCompleted = false;
						MissionStatus missionStatus = MissionStatus.Dormant;
						MissionStatus objectiveStatus = MissionStatus.Dormant;
						switch (mBaseState.MissionRequirement) {
								case MissionRequireType.None:
								default:
										break;

								case MissionRequireType.RequireActive:
										Missions.Get.MissionStatusByName(mBaseState.MissionName, ref missionStatus);
										meetsMissionRequirements &= Flags.Check((uint)missionStatus, (uint)MissionStatus.Active, Flags.CheckType.MatchAny);
										break;

								case MissionRequireType.RequireActiveAndNotFailed:
										Missions.Get.MissionStatusByName(mBaseState.MissionName, ref missionStatus);
										missionCompleted = Missions.Get.MissionCompletedByName(mBaseState.MissionName, ref missionCompleted);
										meetsMissionRequirements &= (missionCompleted && !Flags.Check((uint)missionStatus, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny));
										break;

								case MissionRequireType.RequireCompleted:
										Missions.Get.MissionCompletedByName(mBaseState.MissionName, ref missionCompleted);
										meetsMissionRequirements &= missionCompleted;
										break;

								case MissionRequireType.RequireNotCompleted:
										Missions.Get.MissionCompletedByName(mBaseState.MissionName, ref missionCompleted);
										meetsMissionRequirements &= !missionCompleted;
										break;

								case MissionRequireType.RequireDormant:
										Missions.Get.MissionStatusByName(mBaseState.MissionName, ref missionStatus);
										meetsMissionRequirements &= !Flags.Check((uint)missionStatus, (uint)MissionStatus.Active, Flags.CheckType.MatchAny);
										break;
						}
						//then try objective requirements
						switch (mBaseState.ObjectiveRequirement) {
								case MissionRequireType.None:
								default:
										break;

								case MissionRequireType.RequireActive:
										Missions.Get.ObjectiveStatusByName(mBaseState.MissionName, mBaseState.ObjectiveName, ref objectiveStatus);
										meetsMissionRequirements &= Flags.Check((uint)objectiveStatus, (uint)MissionStatus.Active, Flags.CheckType.MatchAny);
										break;

								case MissionRequireType.RequireActiveAndNotFailed:
										Missions.Get.ObjectiveStatusByName(mBaseState.MissionName, mBaseState.ObjectiveName, ref objectiveStatus);
										meetsMissionRequirements &= (Flags.Check((uint)objectiveStatus, (uint)MissionStatus.Active, Flags.CheckType.MatchAny)
										&& !Flags.Check((uint)objectiveStatus, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny));
										break;

								case MissionRequireType.RequireCompleted:
										Missions.Get.ObjectiveCompletedByName(mBaseState.MissionName, mBaseState.ObjectiveName, ref objectiveCompleted);
										meetsMissionRequirements &= objectiveCompleted;
										break;

								case MissionRequireType.RequireNotCompleted:
										Missions.Get.ObjectiveCompletedByName(mBaseState.MissionName, mBaseState.ObjectiveName, ref objectiveCompleted);
										meetsMissionRequirements &= !objectiveCompleted;
										break;

								case MissionRequireType.RequireDormant:
										Missions.Get.ObjectiveStatusByName(mBaseState.MissionName, mBaseState.ObjectiveName, ref objectiveStatus);
										meetsMissionRequirements &= !Flags.Check((uint)objectiveStatus, (uint)MissionStatus.Active, Flags.CheckType.MatchAny);
										break;
						}
						//if we don't meet those requirements...
						if (!meetsMissionRequirements) {//get out before we start messing with worlditems
								return;
						}

						bool meetsTriggerRequirements = true;
						//finally check if we need to wait on other triggers
						switch (mBaseState.TriggerRequirement) {
								case TriggerRequireType.None:
								default:
										break;

								case TriggerRequireType.RequireNotTriggered:

										break;

								case TriggerRequireType.RequireTriggered:
										break;
						}
						if (!meetsTriggerRequirements) {
								//oops
								return;
						}

						//well, we've made it this far! we've actually triggered
						//alright, time to call the appropriate function based on the final target type
						bool triggered = false;
						switch (finalTarget) {
								case WorldTriggerTarget.Player:
										triggered = OnPlayerEnter();
										break;

								case WorldTriggerTarget.WorldItem:
										triggered = OnWorldItemEnter(worlditem);
										break;

								case WorldTriggerTarget.Character:
										if (!mBaseState.ExcludeCharacters.Contains(mCharacter.worlditem.FileName)) {
												triggered = OnCharacterEnter(mCharacter);
										}
										break;

								case WorldTriggerTarget.Creature:
										triggered = true;//mBaseState.NumTimesTriggered++;
										//OnCreatureEnter (creature);
										break;

								case WorldTriggerTarget.QuestItem:
										triggered = OnQuestItemEnter(mQuestItem);
										break;

								case WorldTriggerTarget.None:
								default:
										break;
						}

						if (triggered) {
								mBaseState.NumTimesTriggered++;
								ParentChunk.SaveTriggers();
								Player.Get.AvatarActions.ReceiveAction(AvatarAction.TriggerWorldTrigger, WorldClock.AdjustedRealTime);
						}
				}

				protected static Character mCharacter;
				protected static QuestItem mQuestItem;

				public void HandleTriggerFinish(Collider other)
				{
						bool quit = false;
						switch (mBaseState.Behavior) {
								case AvailabilityBehavior.Once:
										if (mBaseState.NumTimesTriggered > 0) {
												quit = true;
										}
										break;

								case AvailabilityBehavior.Max:
										if (mBaseState.NumTimesTriggered >= mBaseState.MaxTimesTriggered) {
												quit = true;
										}
										break;

								default:
										break;
						}

						if (quit) {
								return;
						}

						WorldItem worlditem = null;
						if (other.gameObject.HasComponent <WorldItem>(out worlditem)) {
								mRecentWorldItems.Remove(worlditem);
						}
						BodyPart bodyPart = null;
						if (other.gameObject.HasComponent <BodyPart>(out bodyPart)) {
								mRecentWorldItems.Remove(bodyPart.Owner.worlditem);
						}
				}

				public virtual bool OnPlayerEnter()
				{
						return true;
				}

				protected virtual bool OnCharacterEnter(Character character)
				{
						return true;
				}

				public virtual bool OnQuestItemEnter(QuestItem questitem)
				{
						return true;
				}

				public virtual bool OnWorldItemEnter(WorldItem worlditem)
				{
						return true;
				}

				#region initialization

				public virtual void Awake()
				{
						CheckScriptProps();
				}

				public void CheckScriptProps()
				{
						if (mScriptType == null) {
								mScriptType = GetType();
						}
			
						mTrueType = GetType();
						mScriptName = mTrueType.Name;
						mTriggerStateField	= mTrueType.GetField("State");
			
						if (mTriggerStateField != null) {
								//only save data if we have a member called 'State'
								mHasTriggerStateField = true;
						} else {
								mHasTriggerStateField = false;
						}
						mInitialized = true;
				}

				public Type ScriptType {
						get {
								return mScriptType;
						}
				}

				public string ScriptName {
						get {
								return mScriptName;
						}
				}

				public void RefreshTransform()
				{
						mBaseState.Transform.CopyFrom(transform);
				}

				public string TriggerState {
						get {
								string saveData = string.Empty;
								if (mHasTriggerStateField) {
										try {
												if (mTriggerStateField == null) {
														Debug.Log ("Trigger state field is NULL");
												}
												saveData = XmlSerializeToString(mTriggerStateField.GetValue(this));
										} catch (Exception e) {
												Debug.LogError ("triggerScript save data error, returning empty string! E:" + e.ToString ());
												return string.Empty;
										}
								}
								return saveData;
						}
				}

				public object StateObject {
						get {
								object stateObject = null;
								if (mHasTriggerStateField) {
										stateObject = mTriggerStateField.GetValue(this);
								}
								return stateObject;
						}
				}

				protected void InitializeFromState(WorldChunk parentChunk)
				{
						ParentChunk = parentChunk;

						if (mHasTriggerStateField) {
								gameObject.layer = Globals.LayerNumTrigger;
								if (mBaseState == null) {
										//Debug.Log ("Base state was null in trigger, not sure why");
								}
								gameObject.name = mBaseState.Name;
								CreateCollider(mBaseState.ColliderType);
								mBaseState.Transform.ApplyTo(transform);
								mBaseState.trigger = this;
						}

						OnInitialized();
				}

				protected void CreateCollider(WIColliderType colliderType)
				{
						switch (colliderType) {
								case WIColliderType.Sphere:
										SphereCollider sc = gameObject.GetOrAdd <SphereCollider>();
										sc.center = mBaseState.ColliderTransform.Position;
										sc.radius = mBaseState.ColliderTransform.Scale.x;
										sc.isTrigger = true;
										break;
				
								default:
										BoxCollider bc = gameObject.GetOrAdd <BoxCollider>();
										bc.center = mBaseState.ColliderTransform.Position;
										bc.size = mBaseState.ColliderTransform.Scale;
										bc.isTrigger = true;
										break;

								case WIColliderType.None:
										//it's a custom script, must be creating its own
										break;
						}
				}

				#endregion

				#region trigger save/load

				#if UNITY_EDITOR
				public virtual void OnEditorRefresh()
				{
						return;
				}
				#endif
				public virtual void OnInitialized()
				{
						return;
				}

				public WorldTriggerState BaseState {
						get {
								return mBaseState;
						}
				}

				public virtual bool GetTriggerState(out string triggerState)
				{
						triggerState = TriggerState;
						return mHasTriggerStateField;
				}

				//TODO move XML stuff into GameData
				public virtual void UpdateTriggerState(string triggerState, WorldChunk parentChunk)
				{		//script props should have been updated by now
						if (mHasTriggerStateField && !string.IsNullOrEmpty(triggerState)) {
								object triggerStateValue = null;
								try {
										if (XmlDeserializeFromString(triggerState, mTriggerStateField.FieldType, out triggerStateValue)) {
												mTriggerStateField.SetValue(this, triggerStateValue);
												//set the base state so we can access stuff like max times available
												mBaseState = triggerStateValue as WorldTriggerState;
												//save the script name so we can load it properly later
												mBaseState.ScriptName = ScriptName;
										} else {
												Debug.Log ("Couldn't deserialize " + triggerState + " from string");
										}
								} catch (Exception e) {
										Debug.LogError ("Trigger init from save data error! E: " + e.ToString ());
								}
						}

						InitializeFromState(parentChunk);
				}

				public void RefreshState(bool local)
				{
						CheckScriptProps();

						if (mHasTriggerStateField) {
								mBaseState = mTriggerStateField.GetValue(this) as WorldTriggerState;
								gameObject.layer = Globals.LayerNumTrigger;
								mBaseState.Name = gameObject.name;
								//get the GLOBAL transform not the LOCAL transform
								//the trigger goes into the chunk TRIGGER transform
								mBaseState.Transform = new STransform(transform, local);
								//get the collider props
								if (gameObject.collider != null) {
										SphereCollider sc = gameObject.GetComponent <SphereCollider>();
										if (sc == null) {
												BoxCollider bc = gameObject.GetComponent <BoxCollider>();
												mBaseState.ColliderTransform = new STransform(bc.center, Vector3.zero, bc.size);
												mBaseState.ColliderType = WIColliderType.Box;
										} else {
												mBaseState.ColliderTransform = new STransform(sc.center, Vector3.zero, Vector3.one * sc.radius);
												mBaseState.ColliderType = WIColliderType.Sphere;
										}
								} else {
										mBaseState.ColliderType = WIColliderType.None;
								}
						}
				}

				protected static string XmlSerializeToString(object objectInstance)
				{
						if (objectInstance == null) {
								Debug.Log ("Object instance is null");
						}
						XmlSerializer serializer = null;
						try {
								serializer = new XmlSerializer(objectInstance.GetType());
						} catch (Exception e) {
								Debug.LogError(e.InnerException.ToString());
						}

						StringBuilder sb = new StringBuilder();

						using (TextWriter writer = new StringWriter(sb)) {
								serializer.Serialize(writer, objectInstance);
						}
						return sb.ToString();
				}

				protected static T XmlDeserializeFromString <T>(string objectData) where T : new()
				{
						object deserializedObject = new T();

						XmlDeserializeFromString(objectData, typeof(T), out deserializedObject);

						return (T)deserializedObject;
				}

				protected static bool XmlDeserializeFromString(string objectData, Type type, out object deserializedObject)
				{
						deserializedObject	= null;
						XmlSerializer serializer = null;
						try {
								serializer = new XmlSerializer(type);
						} catch (Exception e) {
								Debug.LogError(e.InnerException.ToString());
						}

						using (TextReader reader = new StringReader(objectData)) {
								deserializedObject = serializer.Deserialize(reader);
						}

						if (deserializedObject != null) {
								return true;
						}

						return false;
				}

				public static object XmlDeserializeFromString(string objectData, string typeName)
				{
						object deserializedObject = null;
						XmlDeserializeFromString(objectData, Type.GetType(typeName), out deserializedObject);
						return deserializedObject;
				}

				public static void GenerateCollider(GameObject triggerObject, WIColliderType colliderType, STransform colliderTransform)
				{
						switch (colliderType) {
								case WIColliderType.ConvexMesh:
										MeshCollider mcc = triggerObject.gameObject.GetOrAdd <MeshCollider>();
										mcc.sharedMesh = triggerObject.gameObject.GetOrAdd <MeshFilter>().mesh;
										mcc.convex = true;
										break;

								case WIColliderType.Mesh:
										MeshCollider mc = triggerObject.gameObject.GetOrAdd <MeshCollider>();
										mc.sharedMesh = triggerObject.gameObject.GetOrAdd <MeshFilter>().mesh;
										mc.convex = false;
										break;

								case WIColliderType.Box:
										BoxCollider bc = triggerObject.GetOrAdd <BoxCollider>();
										break;

								case WIColliderType.Sphere:
										SphereCollider sc = triggerObject.GetOrAdd <SphereCollider>();
										sc.radius = colliderTransform.Scale.x;
										sc.center = colliderTransform.Position;
										sc.isTrigger = true;
										break;

								case WIColliderType.Capsule:
										CapsuleCollider cc = triggerObject.GetOrAdd <CapsuleCollider>();
										break;

								case WIColliderType.UseExisting:
										break;

								default:
										break;
						}
				}

				#endregion

				protected WorldTriggerState mBaseState = null;
				protected string mScriptName = string.Empty;
				protected Type mScriptType;
				protected Type mTrueType;
				protected bool mHasTriggerStateField = false;
				protected FieldInfo mTriggerStateField;
				protected bool mInitialized = false;
				protected HashSet <WorldItem> mRecentWorldItems = new HashSet <WorldItem>();
				protected HashSet <Collider> mRecentObjects = new HashSet <Collider>();
		}

		[Serializable]
		public class WorldTriggerState
		{
				public string Name = "Trigger";
				public AvailabilityBehavior Behavior = AvailabilityBehavior.Always;
				public bool OnExit = false;
				public int NumTimesTriggered = 0;
				public int MaxTimesTriggered = 0;
				[HideInInspector]
				public string ScriptName = "WorldTrigger";
				[BitMask(typeof(WorldTriggerTarget))]
				public WorldTriggerTarget Targets = WorldTriggerTarget.Player;
				public WIColliderType ColliderType = WIColliderType.Sphere;
				[HideInInspector]
				public STransform ColliderTransform = STransform.zero;
				[HideInInspector]
				public STransform Transform = STransform.zero;
				public List <string> ExcludeCharacters = new List <string>();
				public List <string> IncludeCharacters = new List <string>();
				public List <string> IncludeCreatures = new List <string>();
				public List <string> ExcludeCreatures = new List <string>();
				[BitMaskAttribute(typeof(TimeOfDay))]
				public TimeOfDay TimeOfDayRequirement = TimeOfDay.ff_All;
				public MissionRequireType MissionRequirement = MissionRequireType.None;
				public MissionRequireType ObjectiveRequirement	= MissionRequireType.None;
				public string MissionName = string.Empty;
				public string ObjectiveName = string.Empty;
				public TriggerRequireType TriggerRequirement = TriggerRequireType.None;
				public string TriggerName = string.Empty;
				public string QuestItemRequirement = string.Empty;
				[XmlIgnore]
				public WorldTrigger trigger;
		}
}