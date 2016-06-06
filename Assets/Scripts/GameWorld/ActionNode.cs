using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.Data;
using Frontiers.World.Gameplay;

using Frontiers.World.WIScripts;
using Frontiers.Story;

namespace Frontiers.World
{
		//used by characters to 'do' things
		//move from here to there, play animations, give speeches
		//sometimes uses a trigger to 'pull' characters in
		//in 99% of cases characters are spawned directly on an actio node
		[ExecuteInEditMode]
		public class ActionNode : MonoBehaviour, IVisible
		{
				public ActionNodeState State = new ActionNodeState();

				public WorldItem Occupant {
						get {
								return mOccupant;
						}
				}

				public bool IsReserved {
						get {
								if (HasOccupant) {
										return false;
								}
								if (WorldClock.AdjustedRealTime > mReservationExpire) {
										mReservant = null;
								}
								return mReservant != null;
						}
				}

				public bool HasOccupant {
						get {
								if (mOccupant != null && Vector3.Distance(mOccupant.transform.position, transform.position) > mMinimumRadius) {
										VacateNode(mOccupant);
								}
								return mOccupant != null;
						}
				}

				public bool CanOccupy(WorldItem newOccupant)
				{
						bool canOccupy = false;
						if (HasOccupant && mOccupant == newOccupant) {
								return true;
						}

						switch (State.Users) {
								case ActionNodeUsers.AnyOccupant:
										//ANY occupant you say? we'll see about that...
										switch (State.Type) {
												case ActionNodeType.QuestNode:
												case ActionNodeType.DailyRoutine:
														//we can only occupy if someone else isn't already occupying
														//this is because characters have to be able to finish their routines
														//without getting interrupted
														canOccupy = !HasOccupant;
														break;
					
												case ActionNodeType.Generic:
												default:
														//we can occupy even if it means displacing another occupant
														canOccupy = true;
														break;
										}
										break;

								case ActionNodeUsers.SpecifiedOccupantOnly:
										//if it's specified occupant only we have to match the file name
										canOccupy = (newOccupant.FileName == State.OccupantName);
										break;
						}
						return canOccupy;
				}

				public bool TryToReserve(WorldItem newReservant)
				{
						if (IsReserved && newReservant != mReservant) {
								return false;
						}

						if (CanOccupy(newReservant)) {
								mReservant = newReservant;
								mReservationExpire = WorldClock.AdjustedRealTime + WorldClock.RTSecondsToGameSeconds (10.0f);
								return true;
						}

						return false;
				}

				public bool HasOccupantReached(WorldItem newOccupant)
				{
						return (Vector3.Distance(newOccupant.transform.position, transform.position) < mMinimumRadius);
				}

				public void ForceOccupyNode(WorldItem newOccupant)
				{
						mOccupant = newOccupant;
				}

				public bool TryToOccupyNode(WorldItem newOccupant)
				{
						if (newOccupant == mReservant) {
								mOccupant = mReservant;
								return true;
						}

						if (!CanOccupy(newOccupant) || !HasOccupantReached(newOccupant)) {//whoops, we either can't use this or we're too far away
								return false;
						}

						if (HasOccupant && newOccupant != mOccupant) {//if we've made it this far and we have an occupant
								//they need to be displaced
								VacateNode(mOccupant);
						}

						//hooray we're the new occupant
						State.NumTimesOccupied++;
						mOccupant = newOccupant;
						if (State.OccupantName == mOccupant.FileName) {//if we match the specific occupant for this node
								State.NumTimesOccupiedSpecific++;
						}

						if (!mSendingEvents) {
								StartCoroutine(SendEvents(ActionNodeBehavior.OnOccupy));
						}

						//align to action node direction
						//load custom animation
						//if we have a speech, this will be overridden
						Motile motile = null;
						if (mOccupant.Is <Motile>(out motile)) {
								//if we told the occupant to come here
								MotileAction topAction = motile.TopAction;
								if (topAction.Type == MotileActionType.GoToActionNode
								&&	topAction.LiveTarget == this) {	//finish the action
										topAction.TryToFinish();
								}
						}

						//alrighty time for the speech givin'
						//see if this new occupant is talkative
						Talkative talkative = null;
						if (mOccupant.Is <Talkative>(out talkative)) {
								Speech speech = null;
								bool foundSpeech	= false;
								switch (State.Speech) {
										case ActionNodeSpeech.None:
										default:
												//there'll be no speechifying today
												break;
					
										case ActionNodeSpeech.RandomAnyone:
												//use speech flags to pull a random speech for anyone who uses this node
												//not implemented
												break;
					
										case ActionNodeSpeech.CustomAnyone:
												//use speech flags to pull a specified speech for anyone who uses this node
												//not implemented
												break;
					
										case ActionNodeSpeech.RandomCharOnly:
												//use speech flags to pull a random speech for the specific character that uses this node
												//not implemented
												break;
					
										case ActionNodeSpeech.CustomCharOnly:
												//use speech flags to pull a specified speech for the specific character that uses this node
												foundSpeech = Mods.Get.Runtime.LoadMod <Speech>(ref speech, "Speech", State.CustomSpeech);
												break;
					
										case ActionNodeSpeech.SequenceCharOnly:
												//use the custom speech name to get the next speech in a sequence
												string speechName = State.CustomSpeech;
												int currentSpeechNumber = 1;
												bool keepLooking = true;
												//TODO look into putting this in Talkative
												while (keepLooking) {//get the next speech
														speechName = State.CustomSpeech.Replace("[#]", currentSpeechNumber.ToString());
														if (Mods.Get.Runtime.LoadMod <Speech>(ref speech, "Speech", speechName)) {//load the speech and see if it's been given by our character
																int numTimesStartedBy = speech.NumTimesStartedBy(mOccupant.FileName);
																if (numTimesStartedBy <= 0) {//if the speech hasn't been started by this character before, use it
																		keepLooking = false;
																		foundSpeech	= true;
																} else {//otherwise increment the speech number and keep looking
																		currentSpeechNumber++;
																}
														} else {//no more speeches to try, oh well
																keepLooking = false;
														}
												}
												break;
								}

								if (foundSpeech) {
										talkative.GiveSpeech(speech, this);
								}
						}
						//if we've gotten this far we're golden
						return true;
				}

				public void Start()
				{
						mTransform = transform;
						gameObject.isStatic = true;
						gameObject.layer = Globals.LayerNumHidden;
						Refresh();
				}

				protected IEnumerator SendEvents(ActionNodeBehavior behavior)
				{
						mSendingEvents = true;
						for (int i = State.Events.Count - 1; i >= 0; i--) {
								ActionNodeEvent nodeEvent = State.Events[i];
								if (nodeEvent == null) {
										State.Events.RemoveAt(i);
								} else if (Flags.Check((uint)nodeEvent.Behavior, (uint)behavior, Flags.CheckType.MatchAny)) {
										switch (nodeEvent.Type) {
												case ActionNodeEventType.ChangeConversationVariable:
														break;

												case ActionNodeEventType.ChangeMissionVariable:
														switch (nodeEvent.ChangeType) {
																case ChangeVariableType.Increment:
																		Missions.Get.IncrementVariable(nodeEvent.Target, nodeEvent.Param, nodeEvent.SetValue);
																		break;

																case ChangeVariableType.Decrement:
																		Missions.Get.DecrementValue(nodeEvent.Target, nodeEvent.Param, nodeEvent.SetValue);
																		break;

																case ChangeVariableType.SetValue:
																default:
																		Missions.Get.SetVariableValue(nodeEvent.Target, nodeEvent.Param, nodeEvent.SetValue);
																		break;
														}
														break;

												case ActionNodeEventType.SendMessageToOccupant:
														if (!string.IsNullOrEmpty(nodeEvent.Message)) {	//send the message
																if (!string.IsNullOrEmpty(nodeEvent.Param)) {
																		mOccupant.gameObject.SendMessage(nodeEvent.Message, nodeEvent.Param, SendMessageOptions.DontRequireReceiver);
																} else {
																		mOccupant.gameObject.SendMessage(nodeEvent.Message, SendMessageOptions.DontRequireReceiver);
																}
														}
														break;

												case ActionNodeEventType.SendSpeechToOccupant:
														break;

												case ActionNodeEventType.SpawnFX:
														if (!string.IsNullOrEmpty(nodeEvent.Param)) {
																FXManager.Get.SpawnFX(transform.position, nodeEvent.Param);
														}
														break;

												default:
														break;
										}
								}
								yield return null;
						}
						mSendingEvents = false;
						yield break;
				}

				public void VacateNode(WorldItem occupant)
				{
						if (mOccupant == occupant) {
								mOccupant = null;
								mReservant = null;
								Refresh();
						}
				}

				public bool IsReservant(WorldItem reservant)
				{
						if (IsReserved) {
								return mReservant == reservant;
						}
						return false;
				}

				public bool IsOccupant(WorldItem occupant)
				{
						if (HasOccupant) {
								return mOccupant == occupant;
						}
						return false;
				}

				public void OnDrawGizmos()
				{
						Gizmos.color = Color.white;
						Vector3 startFeet = transform.position;
						Gizmos.DrawLine(startFeet, startFeet + (transform.up * 2.0f));
						Color bodyColor = Color.cyan;
						Color pointColor = Color.white;
						Color spawnPointColor = Color.red;

						if (State.Type == ActionNodeType.PlayerSpawn) {
								bodyColor = Color.magenta;
								pointColor = Color.magenta;
						} else if (State.Type == ActionNodeType.StructureOwnerSpawn) {
								bodyColor = Color.yellow;
								pointColor = Color.yellow;
						} else if (State.Type == ActionNodeType.Generic) {
								bodyColor = Color.white;
						}

						if (State.UseGenericTemplate) {
								spawnPointColor = Color.green;
						}

						Gizmos.color = bodyColor;
						Vector3 startEyes = startFeet + (transform.up * 1.75f);
						Vector3 endEyes = startEyes + (transform.forward * 0.5f);
						Gizmos.DrawLine(startEyes, endEyes);
						Gizmos.DrawLine(endEyes, startFeet + transform.up);
						Gizmos.color = Colors.Alpha(pointColor, 0.25f);
						Gizmos.DrawSphere(startFeet, 0.25f);
						Gizmos.color = pointColor;
						Gizmos.DrawWireCube(startFeet, new Vector3(0.25f, 0.01f, 0.25f));

						if (State.UseAsSpawnPoint) {
								Gizmos.color = Colors.Alpha(spawnPointColor, 0.65f);
								Gizmos.DrawCube(startEyes - (transform.up * 0.75f), new Vector3(0.25f, 1.5f, 0.25f));
						}

						if (State.UseTrigger) {
								Gizmos.color = Colors.Alpha(Color.green, 0.15f);
								Gizmos.DrawSphere(startFeet, State.TriggerRadius);
						}
						if (mReservant != null) {
								Gizmos.color = Color.yellow;
								Gizmos.DrawWireSphere(transform.position, 0.5f);
						}
						if (mOccupant != null) {
								Gizmos.color = Color.red;
								Gizmos.DrawWireSphere(transform.position, 0.565f);
						}
				}

				public void Refresh()
				{
						name = State.FullName;
						if (string.IsNullOrEmpty(State.Description)) {
								State.Description = State.Name;
						}
						State.actionNode = this;
						if (State.Transform == null) {
								State.Transform = new STransform (transform, true);
						} else {
								State.Transform.CopyFrom (transform);
						}

						if (State.UseTrigger && gameObject.collider == null) {
								SphereCollider col = gameObject.AddComponent <SphereCollider>();
								col.radius = State.TriggerRadius;
								mMinimumRadius = State.TriggerRadius;
								col.isTrigger = true;
								gameObject.layer = Globals.LayerNumTrigger;
						}

						if (mOccupant == null) {
								State.CurretOccupantName = string.Empty;
								State.TimeOccupationBegan = 0.0f;
						} else {
								State.CurretOccupantName = mOccupant.FileName;
						}

						if (State.Type == ActionNodeType.PlayerSpawn || State.Type == ActionNodeType.StructureOwnerSpawn) {
								State.UseAsSpawnPoint = true;
						}
				}

				public void OnTriggerEnter(Collider other)
				{
						if (!Application.isPlaying) {
								return;
						}
			
						bool checkWorldItem = false;
						bool checkPlayer	= false;
			
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
			
						if (!checkWorldItem || checkPlayer) {//get out before we do anything
								return;
						}
			
						//okay so we've got a world item layer, now see if we actually have a world item
						WorldItem worlditem = null;
						if (checkWorldItem) {//see if the actual object is a world item
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

						Character character = null;
						Motile motile = null;
						if (worlditem.Is <Character>(out character)
						 &&	worlditem.Is <Motile>(out motile)) {
								OnCharacterEnter(character, motile);
						}

						if (!mSendingEvents) {
								StartCoroutine(SendEvents(ActionNodeBehavior.OnTriggerEnter));
						}
				}

				public void OnTriggerExit(Collider other)
				{
						WorldItem worlditem = null;
						if (other.gameObject.HasComponent <WorldItem>(out worlditem)) {
								mRecentWorldItems.Remove(worlditem);
						}
						BodyPart bodyPart = null;
						if (other.gameObject.HasComponent <BodyPart>(out bodyPart)) {
								mRecentWorldItems.Remove(bodyPart.Owner.worlditem);
						}			
				}

				public void OnFinishSpeech()
				{
						if (!mSendingEvents) {
								StartCoroutine(SendEvents(ActionNodeBehavior.OnFinishSpeech));
						}
				}

				protected void OnCharacterEnter(Character character, Motile motile)
				{

						if (State.Users == ActionNodeUsers.SpecifiedOccupantOnly
						 && character.worlditem.FileName != State.OccupantName) {
								return;
						}

						//if we're supposed to tell characters to occupy on enter
						//and if we don'
						//and if the character is not the current occupant
						if (State.TryToOccupyOnEnter
						 &&	!(State.TryToOccupyOnce && State.NumTimesOccupied > 0)
						 && mOccupant != character.worlditem) {	//make sure we should try to enter first
								//if it's the right character, wuhoo!
								MotileAction newAction = new MotileAction();
								newAction.Type = MotileActionType.GoToActionNode;
								newAction.Target = new MobileReference(State.Name, State.ParentGroupPath);
								newAction.LiveTarget = this;
								newAction.Expiration = MotileExpiration.TargetOutOfRange;
								if (State.UseTrigger) {	//keep the range smaller so we get to the middle
										newAction.Range = 2.5f;//TODO kludge
								} else {//use the standard minimum range
										newAction.Range = mMinimumRadius;
								}
								newAction.OutOfRange = State.TriggerRadius * 2.0f;
								motile.PushMotileAction(newAction, MotileActionPriority.ForceTop);
						}
				}

				#region IVisible implementation

				public ItemOfInterestType IOIType { get { return ItemOfInterestType.ActionNode; } }

				public bool IsVisible { get { return true; } }

				public float AwarenessDistanceMultiplier { get { return 1.0f; } }

				public float FieldOfViewMultiplier { get { return 1.0f; } }

				public bool Has(string scriptName)
				{
						return false;
				}

				public bool HasAtLeastOne(List <string> scriptNames)
				{
						return false;
				}

				public Vector3 Position {
						get {
								if (mTransform == null) {
										mTransform = transform;
								}
								return mTransform.position;
						}
				}

				public Vector3 FocusPosition {
					get {
						if (mTransform == null) {
							mTransform = transform;
						}
						return mTransform.position;
					}
				}

				public WorldItem worlditem { get { return null; } }

				public PlayerBase player { get { return null; } }

				public ActionNode node { get { return this; } }

				public WorldLight worldlight { get { return null; } }

				public Fire fire { get { return null; } }

				public bool HasPlayerFocus { get; set; }

				public void LookerFailToSee()
				{
						//do nothing
				}

				#endregion

				public bool Destroyed { get { return false; } }

				protected Transform mTransform;
				protected bool mSendingEvents = false;
				protected float mMinimumRadius = 0.35f;
				protected double mReservationExpire	= 0.0f;
				protected WorldItem mOccupant = null;
				protected WorldItem mReservant = null;
				protected HashSet <WorldItem> mRecentWorldItems = new HashSet<WorldItem>();
		}

		[Serializable]
		public class ActionNodeState
		{
				public string Description = string.Empty;

				public string FullName {
						get {
								if (!string.IsNullOrEmpty(ParentGroupPath)) {
										if (Users == ActionNodeUsers.SpecifiedOccupantOnly) {
												return (ParentGroupPath + "." + OccupantName + "." + Name);
										} else {
												return ParentGroupPath + "." + Name;
										}
								} else if (Users == ActionNodeUsers.SpecifiedOccupantOnly) {
										return (OccupantName + "." + Name);
								} else {
										return Name;
								}
						}
				}

				public bool HasOccupant {
						get {
								if (actionNode != null) {
										return actionNode.HasOccupant;
								} else {
										return !string.IsNullOrEmpty(CurretOccupantName);
								}
						}
				}
				//public int InteriorVariant = 0;
				public string Name = "Node";
				public ActionNodeType Type = ActionNodeType.Generic;
				public ActionNodeUsers Users = ActionNodeUsers.AnyOccupant;
				public ActionNodeSpeech Speech = ActionNodeSpeech.RandomAnyone;
				public bool UseAsSpawnPoint = true;
				public bool UseGenericTemplate = true;
				public bool UseTrigger = false;
				public float TriggerRadius = 1.0f;
				[FrontiersAvailableModsAttribute("Character")]
				public string OccupantName = string.Empty;
				public string CurretOccupantName = string.Empty;
				public bool OccupantIsDead = false;
				public bool TryToOccupyOnEnter = true;
				public bool TryToOccupyOnce = true;
				[FrontiersAvailableModsAttribute("Speech")]
				public string CustomSpeech = string.Empty;
				[FrontiersAvailableModsAttribute("Conversation")]
				public string CustomConversation = string.Empty;
				//custom speech name to give
				public ActionNodeBehavior MessageBehavior = ActionNodeBehavior.None;
				[FrontiersBitMask("IdleAnimation")]
				public int IdleAnimation = 0;
				public int NumTimesOccupied = 0;
				public int NumTimesOccupiedSpecific	= 0;
				public float TimeOccupationBegan = 0.0f;
				[BitMask(typeof(TimeOfDay))]
				public TimeOfDay RoutineHours = TimeOfDay.aa_TimeMidnight;
				public int RoutineFlags = 0;
				public int RandomSpeechFlags = 0;
				public int ActionFlags = 0;
				public string ParentGroupPath = string.Empty;
				public STransform Transform = new STransform();
				public SVector3 ChunkOffset = new SVector3();
				[XmlIgnore]
				public ActionNode actionNode = null;

				public bool IsLoaded {
						get {
								return actionNode != null;
						}
				}

				public override int GetHashCode()
				{
						return Name.GetHashCode();
				}

				public List <ActionNodeEvent> Events = new List <ActionNodeEvent>();
		}

		[Serializable]
		public class ActionNodeEvent
		{
				public ActionNodeBehavior Behavior = ActionNodeBehavior.OnOccupy;
				public ActionNodeEventType Type = ActionNodeEventType.ChangeMissionVariable;
				public ActionNodeSpeech Speech = ActionNodeSpeech.None;
				public string Target = string.Empty;
				public string Message = string.Empty;
				public string Param = string.Empty;
				public ChangeVariableType ChangeType = ChangeVariableType.Increment;
				public int SetValue	= 1;
		}
}