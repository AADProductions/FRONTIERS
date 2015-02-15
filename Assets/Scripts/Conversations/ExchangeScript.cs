using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.Data;
using System.Xml.Serialization;

namespace Frontiers.Story.Conversations
{
		[Serializable]
		//this is GARBAGE - i had no idea what i was getting into with this XmlInclude nonsense
		//i will be replacing this system with something similar to WIScript, where the type is stored as a key
		//and the data is stored as raw XML
		[XmlInclude(typeof(AcceptLoanOffer))]
		[XmlInclude(typeof(ActivateAlbertsGuards))]
		[XmlInclude(typeof(ActivateMission))]
		[XmlInclude(typeof(ActivateMissionObjective))]
		[XmlInclude(typeof(AddOrRemoveDTSOverride))]
		[XmlInclude(typeof(AddWIScriptToCharacter))]
		[XmlInclude(typeof(CheckCuratorArtifacts))]
		[XmlInclude(typeof(CuratorAquireArtifacts))]
		[XmlInclude(typeof(CalculateCuratorTotalOffer))]
		[XmlInclude(typeof(ChangeSocialStatus))]
		[XmlInclude(typeof(ChangeCharacterEmotion))]
		[XmlInclude(typeof(ChangeConversationVariable))]
		[XmlInclude(typeof(ChangeMissionVariable))]
		[XmlInclude(typeof(ChangeMissionDescription))]
		[XmlInclude(typeof(ChangeObjectiveDescription))]
		[XmlInclude(typeof(ChangeRepuation))]
		[XmlInclude(typeof(DelegateExchangeScript))]
		[XmlInclude(typeof(DespawnCharacterAfterConversation))]
		[XmlInclude(typeof(FailMission))]
		[XmlInclude(typeof(FailMissionObjective))]
		[XmlInclude(typeof(GameOver))]
		[XmlInclude(typeof(GetOutstandingLoanAmount))]
		[XmlInclude(typeof(GiveBlueprintToPlayer))]
		[XmlInclude(typeof(GiveBookToPlayer))]
		[XmlInclude(typeof(GiveGenericItemsToPlayer))]
		[XmlInclude(typeof(GiveKeyToPlayer))]
		[XmlInclude(typeof(GiveMoneyToPlayer))]
		[XmlInclude(typeof(GivePotionToPlayer))]
		[XmlInclude(typeof(GotoExchange))]
		[XmlInclude(typeof(HouseOfHealingCalculateDonation))]
		[XmlInclude(typeof(HouseOfHealingHealAll))]
		[XmlInclude(typeof(IgnoreMissionObjective))]
		[XmlInclude(typeof(IgnoreMission))]
		[XmlInclude(typeof(InitiateTradeWithCharacter))]
		[XmlInclude(typeof(LibraryDeliverBookOrder))]
		[XmlInclude(typeof(MakeLoanPayment))]
		[XmlInclude(typeof(RequireBookStatus))]
		[XmlInclude(typeof(RequireCharacterSpokenToOnce))]
		[XmlInclude(typeof(RequireCharWIScript))]
		[XmlInclude(typeof(RequireConversationVariable))]
		[XmlInclude(typeof(RequireCuratorItemsAvailable))]
		[XmlInclude(typeof(RequireExchangeConcluded))]
		[XmlInclude(typeof(RequireExchangesConcluded))]
		[XmlInclude(typeof(RequireExchangeEnabled))]
		[XmlInclude(typeof(RequireLoanPaymentAvailable))]
		[XmlInclude(typeof(RequireMissionStatus))]
		[XmlInclude(typeof(RequireMissionVariable))]
		[XmlInclude(typeof(RequirePlayerVariable))]
		[XmlInclude(typeof(RequireObjectiveStatus))]
		[XmlInclude(typeof(RequireOutstandingLoan))]
		[XmlInclude(typeof(RequireLibraryBookOrder))]
		[XmlInclude(typeof(RequireQuestItem))]
		[XmlInclude(typeof(RequireQuestItems))]
		[XmlInclude(typeof(RequireReputation))]
		[XmlInclude(typeof(RequireSkillLearned))]
		[XmlInclude(typeof(RequireSkillMastered))]
		[XmlInclude(typeof(ReqireSocialStatus))]
		[XmlInclude(typeof(RequireStatusCondition))]
		[XmlInclude(typeof(RequireStructureOwner))]
		[XmlInclude(typeof(RequireTimeElapsed))]
		[XmlInclude(typeof(RequireValidLoanOffer))]
		[XmlInclude(typeof(RequireWorldItem))]
		[XmlInclude(typeof(RequireWorldTrigger))]
		[XmlInclude(typeof(ResetLoanOffer))]
		[XmlInclude(typeof(RevealLocation))]
		[XmlInclude(typeof(RevealNearestLocation))]
		[XmlInclude(typeof(SendMotileAction))]
		[XmlInclude(typeof(SetActiveMuseum))]
		[XmlInclude(typeof(SetLoanOfferProperties))]
		[XmlInclude(typeof(SetPlayerCredentials))]
		[XmlInclude(typeof(SetExchangeEnabled))]
		[XmlInclude(typeof(SetStructureOwner))]
		[XmlInclude(typeof(SetQuestItemVisibility))]
		[XmlInclude(typeof(ShowVariable))]
		[XmlInclude(typeof(SubstituteConversation))]
		[XmlInclude(typeof(TakeMoneyFromPlayer))]
		[XmlInclude(typeof(TakeQuestItemFromPlayer))]
		[XmlInclude(typeof(TakeWorldItemFromPlayer))]
		[XmlInclude(typeof(TeachPlayerSkill))]
		[XmlInclude(typeof(TriggerCutscene))]
		public class ExchangeScript
		{
				public ExchangeScript () {
						mScriptType = this.GetType();
						mScriptName = mScriptType.FullName;
				}

				[XmlIgnore]
				[NonSerialized]
				public Exchange exchange;
				[XmlIgnore]
				[NonSerialized]
				public Conversation conversation;
				public bool Global = false;
				public string Description = string.Empty;
				public string Summary = string.Empty;
				public ExchangeAction CallOn = ExchangeAction.Choose;
				public AvailabilityBehavior Availability = AvailabilityBehavior.Always;
				public int NumTimesTriggered = 0;
				public int MaxTimesTriggered = 0;
				public bool IgnoreChoiceToLeave = false;
				public bool RequireAll = true;
				public bool FlipRequirements = false;
				public string SpecificIncomingChoice = string.Empty;
				public string SpecificOutgoingChoice = string.Empty;
				public string LastIncomingChoice = string.Empty;
				public string LastOutgoingChoice = string.Empty;
				public string DtsOnFailure = string.Empty;

				public KeyValuePair <string,string> SaveState {
						get {	//TODO update this when we move serialization to gamedata
								return new KeyValuePair<string,string> (ScriptName, WIScript.XmlSerializeToString(this));
						}
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

				public bool RequirementsMet {
						get {
								if (FlipRequirements) {
										return !CheckRequirementsMet();
								} else {
										return CheckRequirementsMet();
								}
						}
				}

				public bool IsAvailable {
						get {
								bool isAvailable	= true;

								switch (Availability) {
										case AvailabilityBehavior.Max:
												if (NumTimesTriggered >= MaxTimesTriggered) {
														isAvailable = false;
												}
												break;

										case AvailabilityBehavior.Once:
												if (NumTimesTriggered > 0) {
														isAvailable = false;
												}
												break;

										case AvailabilityBehavior.Always:
										default:
												break;
								}
								return isAvailable;
						}
				}

				public void Initialize(Conversation parentConversation)
				{
						Global = true;
						exchange = null;
						conversation = parentConversation;
				}

				public virtual void Initialize(Exchange parentExchange, Conversation parentConversation)
				{
						Global = false;
						exchange = parentExchange;
						conversation = parentConversation;
				}

				protected virtual bool CheckRequirementsMet()
				{
						return true;
				}

				public void OnConclude(string outgoingChoice)
				{
						if (!IsAvailable) {
								return;
						}

						LastOutgoingChoice = outgoingChoice;
						bool callAction = true;

						switch (CallOn) {
								case ExchangeAction.Conclude:
								case ExchangeAction.Both:
										if ((string.IsNullOrEmpty(outgoingChoice) && IgnoreChoiceToLeave)
										||	(!string.IsNullOrEmpty(SpecificOutgoingChoice) && outgoingChoice != SpecificOutgoingChoice)) {
												callAction = false;
										}
										break;

								default:
										callAction = false;
										break;
						}

						if (callAction) {
								NumTimesTriggered++;
								Action();
						}
				}

				public void OnChoose(string incomingChoice)
				{
						if (!IsAvailable) {
								return;
						}

						LastIncomingChoice = incomingChoice;
						bool callAction = true;

						switch (CallOn) {
								case ExchangeAction.Choose:
								case ExchangeAction.Both:
										if (!string.IsNullOrEmpty(SpecificIncomingChoice) && incomingChoice != SpecificIncomingChoice) {
												callAction = false;
										}
										break;

								default:
										callAction = false;
										break;
						}

						if (callAction) {
								NumTimesTriggered++;
								Action();
						}
				}

				protected virtual void Action()
				{
						//do stuff here
				}

				protected Type mScriptType;
				protected string mScriptName;
		}
}