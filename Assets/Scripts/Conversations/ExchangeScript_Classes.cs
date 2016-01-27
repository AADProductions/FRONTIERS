using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.WIScripts;
using Frontiers.World.Gameplay;
using Frontiers.Data;
using System.Xml.Serialization;

namespace Frontiers.Story.Conversations
{
	[Serializable]
	public class AcceptLoanOffer : ExchangeScript
	{
		protected override void Action ()
		{
			Moneylenders.Get.AcceptCurrentOffer ();
		}
	}

	[Serializable]
	public class ActivateAlbertsGuards : ExchangeScript
	{
		protected override void Action ()
		{
			//TODO implement!
			FamilyAlbertConfrontation.AlbertsGuards.ActivateGuards ();
		}
	}

	[Serializable]
	public class ActivateMission : ExchangeScript
	{
		public string MissionName = "Mission";
		public MissionOriginType OriginTypeOverride = MissionOriginType.Character;
		public string OriginNameOverride = string.Empty;

		protected override void Action ()
		{
			MissionOriginType originType = MissionOriginType.Character;
			string originName = conversation.SpeakingCharacter.worlditem.DisplayName;
			if (OriginTypeOverride != MissionOriginType.None) {
				originType = OriginTypeOverride;
			}
			if (!string.IsNullOrEmpty (OriginNameOverride)) {
				originName = OriginNameOverride;
			}

			Missions.Get.ActivateMission (MissionName, originType, originName);
		}
	}

	[Serializable]
	public class ActivateMissionObjective : ExchangeScript
	{
		public string MissionName = "Mission";
		public string ObjectiveName = "Objective";
		public MissionOriginType OriginTypeOverride = MissionOriginType.None;
		public string OriginNameOverride = string.Empty;

		protected override void Action ()
		{
			MissionOriginType originType = MissionOriginType.Character;
			string originName = conversation.SpeakingCharacter.worlditem.DisplayName;
			if (OriginTypeOverride != MissionOriginType.None) {
				originType = OriginTypeOverride;
			}
			if (!string.IsNullOrEmpty (OriginNameOverride)) {
				originName = OriginNameOverride;
			}

			Missions.Get.ActivateObjective (MissionName, ObjectiveName, originType, originName);
		}
	}

	[Serializable]
	public class AddOrRemoveDTSOverride : ExchangeScript
	{
		public string ConversationName = string.Empty;
		public string CharacterName = string.Empty;
		public string DTSConversationName = string.Empty;
		public bool AddOverride = true;

		protected override void  Action ()
		{

			if (AddOverride) {
				Debug.Log ("Adding DTS override");
				Frontiers.Conversations.Get.AddDTSOverride (ConversationName, DTSConversationName, CharacterName);
			} else {
				Debug.Log ("Removing DTS override");
				Frontiers.Conversations.Get.RemoveDTSOverride (ConversationName, DTSConversationName, CharacterName);
			}
		}
	}

	[Serializable]
	public class AddWIScriptToCharacter : ExchangeScript
	{
		public string CharacterName = "Character";
		public string ScriptName = "Feeble";
		public List <string> ScriptStateVariables	= new List <string> ();

		protected override void Action ()
		{
			Character character = null;
			if (Characters.Get.SpawnedCharacter (CharacterName, out character)) {
				character.worlditem.GetOrAdd (ScriptName);
			}
		}
	}

	[Serializable]
	public class CheckCuratorArtifacts : ExchangeScript
	{
		protected override void Action ()
		{	//works
			Museums.Get.RefreshAvailableArtifacts ();
		}
	}

	[Serializable]
	public class CuratorAquireArtifacts : ExchangeScript
	{
		public string ArtifactType = "DatedShard";

		protected override void Action ()
		{	//works
			Museums.Get.AquireLastOffer ();
		}
	}

	[Serializable]
	public class CalculateCuratorTotalOffer : ExchangeScript
	{
		public string ArtifactType = "UndatedShard";

		protected override void Action ()
		{	//works, needs better implementation
			int totalOffer = Museums.Get.CalculateOffer (ArtifactType);
			exchange.ParentExchange.ParentConversation.SetVariableValue ("TotalOffer", totalOffer);
		}
	}

	[Serializable]
	public class ChangeSocialStatus : ExchangeScript
	{
		//do we need this?
	}

	[Serializable]
	public class ChangeCharacterEmotion : ExchangeScript
	{
		public EmotionalState Emotion;

		protected override void Action ()
		{
			conversation.SpeakingCharacter.State.Emotion = Emotion;
		}
	}

	[Serializable]
	public class ChangeConversationVariable : ExchangeScript
	{
		public string VariableName = string.Empty;
		public string VariableEval = string.Empty;
		public ChangeVariableType ChangeType = ChangeVariableType.Increment;
		public int SetValue = 1;
		public bool SetToCurrentWorldTime;

		protected override void Action ()
		{
			if (SetToCurrentWorldTime) {
				conversation.SetVariableValue (VariableName, Mathf.FloorToInt ((float)WorldClock.AdjustedRealTime));
			} else {
				switch (ChangeType) {
				case ChangeVariableType.Increment:
				default:
					for (int i = 0; i < SetValue; i++) {
						conversation.IncrementVariable (VariableName);
					}
					break;

				case ChangeVariableType.Decrement:
					for (int i = 0; i < SetValue; i++) {
						conversation.DecrementValue (VariableName);
					}
					break;

				case ChangeVariableType.SetValue:
					conversation.SetVariableValue (VariableName, SetValue);
					break;
				}
			}
		}
	}

	[Serializable]
	public class ChangeMissionVariable : ExchangeScript
	{
		public string MissionName = string.Empty;
		public string VariableName = string.Empty;
		public string VariableEval = string.Empty;
		public ChangeVariableType ChangeType = ChangeVariableType.Increment;
		public int SetValue = 1;
		public bool SetToCurrentWorldTime;

		protected override void Action ()
		{
			if (!string.IsNullOrEmpty (VariableEval)) {
				ChangeType = ChangeVariableType.SetValue;
				SetValue = GameData.Evaluate (VariableEval, conversation);
			}

			if (SetToCurrentWorldTime) {
				Missions.Get.SetVariableValue (MissionName, VariableName, Mathf.FloorToInt ((float)WorldClock.AdjustedRealTime));
			} else {
				switch (ChangeType) {
				case ChangeVariableType.Increment:
				default:
					Missions.Get.IncrementVariable (MissionName, VariableName, SetValue);
					break;

				case ChangeVariableType.Decrement:
					Missions.Get.DecrementValue (MissionName, VariableName, SetValue);
					break;

				case ChangeVariableType.SetValue:
					Missions.Get.SetVariableValue (MissionName, VariableName, SetValue);
					break;
				}
			}
		}
	}

	[Serializable]
	public class ChangeMissionDescription : ExchangeScript
	{
		public string MissionName = "Mission";
		public string NewDescription = "Mission description";
		public bool OnlyWhenActive = true;
		public bool OnlyFirstTime = true;

		protected override void Action ()
		{
			NewDescription = NewDescription.Replace ("_", " ");

			MissionState state = null;
			if (Missions.Get.MissionStateByName (MissionName, out state)) {	//if only when active and it's NOT active...
				if (OnlyWhenActive && !Flags.Check ((uint)state.Status, (uint)MissionStatus.Active, Flags.CheckType.MatchAny)) {	//we're done
					return;
				} else if (OnlyFirstTime && exchange.NumTimesChosen > 0) {//if only first time and it's not first time
					return;
				}
				//otherwise we're good to go
				state.Description = NewDescription;
			}
		}
	}

	[Serializable]
	public class ChangeObjectiveDescription : ExchangeScript
	{
		public string MissionName;
		public string ObjectiveName;
		public string NewDescription = "Objective description";
		public bool OnlyWhenActive = true;
		public bool OnlyFirstTime = true;

		protected override void Action ()
		{
			NewDescription = NewDescription.Replace ("_", " ");

			MissionState state = null;
			if (Missions.Get.MissionStateByName (MissionName, out state)) {
				ObjectiveState objState = state.GetObjective (ObjectiveName);
				if (OnlyWhenActive && !Flags.Check ((uint)objState.Status, (uint)MissionStatus.Active, Flags.CheckType.MatchAny)) {	//we're done
					return;
				} else if (OnlyFirstTime && exchange.NumTimesChosen > 0) {//if only first time and it's not first time
					return;
				}
				//otherwise we're good to go
				objState.Description = NewDescription;
			}
		}
	}

	[Serializable]
	public class ChangeRepuation : ExchangeScript
	{
		public string CharacterName = string.Empty;
		public int ReputationAmount = 1;
		public WISize ReputationChangeSize = WISize.NoLimit;
		public ChangeVariableType ChangeType = ChangeVariableType.Increment;

		protected override void Action ()
		{
			if (string.IsNullOrEmpty (CharacterName)) {
				CharacterName = conversation.SpeakingCharacter.worlditem.FileName;
			}

			int reputationAmount = ReputationAmount;
			switch (ReputationChangeSize) {
			case WISize.NoLimit:
			default:
										//use regular size
				break;

			case WISize.Tiny:
				reputationAmount = Globals.ReputationChangeTiny;
				break;

			case WISize.Small:
				reputationAmount = Globals.ReputationChangeSmall;
				break;

			case WISize.Medium:
				reputationAmount = Globals.ReputationChangeMedium;
				break;

			case WISize.Large:
				reputationAmount = Globals.ReputationChangeLarge;
				break;

			case WISize.Huge:
				reputationAmount = Globals.ReputationChangeHuge;
				break;
			}
			string characterDisplayName = string.Empty;
			if (CharacterName == exchange.ParentConversation.SpeakingCharacter.worlditem.FileName) {
				characterDisplayName = exchange.ParentConversation.SpeakingCharacter.worlditem.DisplayName;
			}

			switch (ChangeType) {
			case ChangeVariableType.Increment:
				Profile.Get.CurrentGame.Character.Rep.GainPersonalReputation (CharacterName, characterDisplayName, reputationAmount);
				break;

			case ChangeVariableType.Decrement:
				Profile.Get.CurrentGame.Character.Rep.LosePersonalReputation (CharacterName, characterDisplayName, reputationAmount);
				break;

			case ChangeVariableType.SetValue:
			default:
				Profile.Get.CurrentGame.Character.Rep.SetPersonalReputation (CharacterName, characterDisplayName, reputationAmount);
				break;
			}
		}
	}

	[Serializable]
	public class DelegateExchangeScript : ExchangeScript
	{
	}

	[Serializable]
	public class DespawnCharacterAfterConversation : ExchangeScript
	{
		public bool Despawn = true;

		protected override void Action ()
		{
			conversation.DespawnCharacterAfterConversation = Despawn;
		}
	}

	[Serializable]
	public class FailMission : ExchangeScript
	{
		public string MissionName = "Mission";

		protected override void Action ()
		{
			Missions.Get.ForceFailMission (MissionName);
		}
	}

	[Serializable]
	public class FailMissionObjective : ExchangeScript
	{
		public string MissionName = "Mission";
		public string ObjectiveName = "Objective";

		protected override void Action ()
		{
			Missions.Get.ForceFailObjective (MissionName, ObjectiveName);
		}
	}

	[Serializable]
	public class GameOver : ExchangeScript
	{
		public string Reason = "You have died permanently";

		protected override void Action ()
		{
			GameManager.Get.GameOver (Reason);
		}
	}

	[Serializable]
	public class GetOutstandingLoanAmount : ExchangeScript
	{
		public string OrganizationName = "Moneylender";
		public string VariableName = string.Empty;

		protected override void Action ()
		{
			Loan loan = null;
			if (Moneylenders.Get.HasOutstandingLoan (OrganizationName, out loan)) {
				Conversation.LastInitiatedConversation.SetVariableValue (VariableName, loan.AmountOwed);
			}
		}
	}

	[Serializable]
	public class GiveBlueprintToPlayer : ExchangeScript
	{
		public string BlueprintName = string.Empty;

		protected override void Action ()
		{
			Blueprints.Get.Reveal (BlueprintName, BlueprintRevealMethod.Character, conversation.SpeakingCharacter.FullName);
		}
	}

	[Serializable]
	public class GiveBookToPlayer : ExchangeScript
	{
		public string BookName = string.Empty;

		protected override void Action ()
		{
			Books.AquireBook (BookName);
		}
	}

	[Serializable]
	public class GiveGenericItemsToPlayer : ExchangeScript
	{
		public string PackName = string.Empty;
		public string PrefabName = string.Empty;
		public string StackName = string.Empty;
		public string State = string.Empty;
		public string Subcategory = string.Empty;
		public int NumItems = 1;
		public bool InstantiateFirst = false;
		public bool MakeQuestItem = false;
		public string QuestName = string.Empty;
		public string DisplayName = string.Empty;

		protected override void Action ()
		{
			if (gGiveToPlayer == null) {
				gGiveToPlayer = new GenericWorldItem ();
			}
			gGiveToPlayer.PackName = PackName;
			gGiveToPlayer.PrefabName = PrefabName.Replace ("_", " ");
			gGiveToPlayer.StackName = StackName;
			gGiveToPlayer.State = State;
			gGiveToPlayer.Subcategory = Subcategory;

			InstantiateFirst = InstantiateFirst | !string.IsNullOrEmpty (QuestName);

			WIStackError error = WIStackError.None;
			StackItem stackItem = gGiveToPlayer.ToStackItem ();
			if (InstantiateFirst) {
				//we want to instantiate it as a world item first so it can do something with its scripts
				WorldItem worlditem = null;
				for (int i = 0; i < NumItems; i++) {
					if (WorldItems.CloneFromStackItem (stackItem, WIGroups.Get.Player, out worlditem)) {
						//initialize immediately
						worlditem.Initialize ();
						if (!string.IsNullOrEmpty (QuestName)) {
							QuestItem questItem = worlditem.GetOrAdd <QuestItem> ();
							questItem.State.QuestName = QuestName;
							questItem.State.DisplayName = DisplayName;
							if (string.IsNullOrEmpty (questItem.State.DisplayName)) {
								questItem.State.DisplayName = worlditem.DisplayName;
							}
							//just to be sure in case we change initialization order later
							worlditem.Props.Name.DisplayName = questItem.State.DisplayName;
							worlditem.Props.Name.QuestName = questItem.State.QuestName;
							worlditem.Props.Name.QuestName = QuestName;
						}
						Player.Local.Inventory.TryToEquip (worlditem);
					}
				}
			} else {
				//we don't care, just instantiate it as a stack item
				for (int i = 0; i < NumItems; i++) {
					Player.Local.Inventory.TryToEquip (stackItem.GetDuplicate (false));
				}
			}
		}

		protected static GenericWorldItem gGiveToPlayer;
		// = new GenericWorldItem ();
	}

	[Serializable]
	public class GiveKeyToPlayer : ExchangeScript
	{
		public string KeyName = "Key";
		public string KeyType = "SimpleKey";
		public string KeyTag = "Master";

		protected override void Action ()
		{
			Player.Local.Inventory.State.PlayerKeyChain.AddKey (KeyName, KeyType, KeyTag);
		}
	}

	[Serializable]
	public class GiveMoneyToPlayer : ExchangeScript
	{
		public int CurrencyValue = 0;
		public WICurrencyType CurrencyType = WICurrencyType.A_Bronze;
		public string CurrencyValueEval = string.Empty;

		protected override void Action ()
		{
			if (!string.IsNullOrEmpty (CurrencyValueEval)) {
				CurrencyValue = GameData.Evaluate (CurrencyValueEval, conversation);
			}
			int numRemoved = 0;
			Player.Local.Inventory.InventoryBank.Add (CurrencyValue, CurrencyType);
		}
	}

	[Serializable]
	public class GivePotionToPlayer : ExchangeScript
	{
		public int NumItems = 1;
		public string PotionName;

		protected override void Action ()
		{
			Potions.Get.AquirePotion (PotionName, NumItems);
		}
	}

	[Serializable]
	public class GotoExchange : ExchangeScript
	{
		public string ExchangeName = string.Empty;
		public bool RequireVariableCheck	= true;
		public string VariableName = string.Empty;
		public int VariableValue = 0;
		public VariableCheckType CheckType = VariableCheckType.GreaterThanOrEqualTo;

		protected override void  Action ()
		{
			bool gotoResult = true;
			if (RequireVariableCheck) {
				int currentValue = conversation.GetVariableValue (VariableName);
				gotoResult = GameData.CheckVariable (CheckType, VariableValue, currentValue);
			}
			if (gotoResult) {
				conversation.GotoExchange (ExchangeName);
			}
		}
	}

	[Serializable]
	public class HouseOfHealingCalculateDonation : ExchangeScript
	{
		protected override void Action ()
		{
			exchange.ParentConversation.SetVariableValue ("Donation", HouseOfHealing.CalculateHealDonation ());
		}
	}

	[Serializable]
	public class HouseOfHealingHealAll : ExchangeScript
	{
		public bool CanAfford = true;

		protected override void Action ()
		{
			HouseOfHealing.HealAll (CanAfford);
		}
	}

	[Serializable]
	public class IgnoreMissionObjective : ExchangeScript
	{
		public string MissionName = "Mission";
		public string ObjectiveName = "Objective";

		protected override void Action ()
		{
			Missions.Get.IgnoreObjective (MissionName, ObjectiveName);
		}
	}

	[Serializable]
	public class IgnoreMission : ExchangeScript
	{
		public string MissionName = string.Empty;

		protected override void Action ()
		{
			Missions.Get.IgnoreMission (MissionName);
		}
	}

	[Serializable]
	public class InitiateTradeWithCharacter : ExchangeScript
	{
		public bool ZeroCostMode = false;

		protected override void Action ()
		{
			Character barteringCharacter = conversation.SpeakingCharacter;
			conversation.ForceEnd ();

			Skill barterSkill = null;
			int zeroCostModeFlavor = 0;
			if (ZeroCostMode) {
				zeroCostModeFlavor = 1;
			}
			if (Skills.Get.SkillByName ("Barter", out barterSkill)) {
				barterSkill.Use (barteringCharacter.worlditem, zeroCostModeFlavor);
			}
		}
	}

	[Serializable]
	public class LibraryDeliverBookOrder : ExchangeScript
	{
		//delivers the last order made to the player, if it has arrived
		public string LibraryName = "GuildLibrary";

		protected override void Action ()
		{
			Debug.Log ("Delivering book order");
			Books.Get.DeliverBookOrder (LibraryName);
		}

		protected override bool CheckRequirementsMet ()
		{
			LibraryCatalogueEntry order = null;
			if (Books.Get.HasPlacedOrder (LibraryName, out order)) {
				if (order.HasArrived) {
					Debug.Log ("order has arrived");
					return true;
				}
				Debug.Log ("We have an order but it hasn't arrived");
			}
			return false;
		}
	}

	[Serializable]
	public class MakeLoanPayment : ExchangeScript
	{
		public string OrganizationName = "Moneylender";
		public int PaymentAmount = 0;
		public bool PaymentInFull = false;

		protected override void Action ()
		{
			if (PaymentInFull) {
				Moneylenders.Get.RepayLoan (OrganizationName);
			} else {
				Moneylenders.Get.MakePayment (OrganizationName, PaymentAmount);
			}
		}
	}

	[Serializable]
	public class RequireBookStatus : ExchangeScript
	{
		public string BookName = "Book";
		public BookStatus Status = BookStatus.Dormant;
		public bool RequireStatus = true;

		protected override bool CheckRequirementsMet ()
		{
			BookStatus status;
			if (Books.GetBookStatus (BookName, out status)) {
				if (Flags.Check ((uint)status, (uint)Status, Flags.CheckType.MatchAny)) {
					if (RequireStatus) {
						return true;
					} else {
						return false;
					}
				} else {
					if (RequireStatus) {
						return false;
					} else {
						return true;
					}
				}
			}
			return false;
		}
	}

	[Serializable]
	public class RequireCharacterSpokenToOnce : ExchangeScript
	{
		public string CharacterName = string.Empty;

		protected override bool CheckRequirementsMet ()
		{
			return Profile.Get.CurrentGame.Character.HasSpokenToCharacter (CharacterName);
		}
	}

	[Serializable]
	public class RequireCharWIScript : ExchangeScript
	{
		public string WIScriptName = string.Empty;

		protected override bool CheckRequirementsMet ()
		{			
			if (string.IsNullOrEmpty (WIScriptName)) {
				return true;
			}

			return conversation.SpeakingCharacter.worlditem.Is (WIScriptName);
		}
	}

	[Serializable]
	public class RequireConversationVariable : ExchangeScript
	{
		public string VariableName	= string.Empty;
		public int VariableValue = 0;
		public VariableCheckType CheckType = VariableCheckType.GreaterThanOrEqualTo;

		protected override bool CheckRequirementsMet ()
		{			
			int currentValue = conversation.GetVariableValue (VariableName);
			return GameData.CheckVariable (CheckType, VariableValue, currentValue);
		}
	}

	[Serializable]
	public class RequireCuratorItemsAvailable : ExchangeScript
	{
		public string ItemName = "DatedLarge";
		public bool DuplicatesOK = false;
		public bool RequireAvailable = true;

		protected override bool CheckRequirementsMet ()
		{	//doesn't seem to work?
			if (RequireAvailable) {
				return Museums.Get.NumItemsAvailable (ItemName, DuplicatesOK) > 0;
			} else {
				return Museums.Get.NumItemsAvailable (ItemName, DuplicatesOK) <= 0;
			}
		}
	}

	[Serializable]
	public class RequireExchangeConcluded : ExchangeScript
	{
		public string ConversationName = string.Empty;
		public string ExchangeName = string.Empty;
		public int NumTimes = 1;
		public bool RequireConversationInitiated = true;
		public VariableCheckType CheckType = VariableCheckType.GreaterThanOrEqualTo;

		protected override bool CheckRequirementsMet ()
		{
			if (string.IsNullOrEmpty (ConversationName)) {
				ConversationName = exchange.ParentConversation.Props.Name;
			}
			if (ConversationName.Contains ("*")) {
				ConversationName = SubstituteConversation.Substitution (conversation.Props.Name, ConversationName);
			}

			Debug.Log ("Require exchange " + ExchangeName + " concluded in " + ConversationName);

			int numTimes = 0;
			if (Frontiers.Conversations.Get.HasCompletedExchange (ConversationName, ExchangeName, RequireConversationInitiated, out numTimes)) {
				bool result = GameData.CheckVariable (CheckType, NumTimes, numTimes);
				Debug.Log ("Result: " + result.ToString ());
				return result;
			}
			return false;
		}
	}

	[Serializable]
	public class RequireExchangesConcluded : ExchangeScript
	{
		public List <string> Exchanges = new List <string> ();
		public bool RequireAllExchanges = true;
		public bool RequireConversationInitiated = true;
		public bool RequireConcluded = true;
		public string ConversationName;

		protected override bool CheckRequirementsMet ()
		{
			if (string.IsNullOrEmpty (ConversationName)) {
				ConversationName = exchange.ParentConversation.Props.Name;
			}
			if (ConversationName.Contains ("*")) {
				ConversationName = SubstituteConversation.Substitution (conversation.Props.Name, ConversationName);
			}
			string conversationName = ConversationName;
			List<bool> results = new List<bool> ();
			foreach (string exchangeName in Exchanges) {
				string finalExchangeName = exchangeName;
				if (exchangeName.Contains (":")) {
					//whoops, this exchange contains the conversation name
					string[] splitExchange = exchangeName.Split (new string [] { ":" }, StringSplitOptions.RemoveEmptyEntries);
					conversationName = splitExchange [0];
					finalExchangeName = splitExchange [1];
				}
				int exchangeIndex = 0;
				if (Int32.TryParse (finalExchangeName, out exchangeIndex)) {
					//it must be an integer
					finalExchangeName = Frontiers.Conversations.Get.ExchangeNameFromIndex (conversationName, exchangeIndex);
				}
				bool completedThis = Frontiers.Conversations.Get.HasCompletedExchange (conversationName, finalExchangeName, RequireConversationInitiated);
				//do we want them completed or not completed?
				if (RequireConcluded) {
					results.Add (completedThis);
				} else {
					results.Add (!completedThis);

				}
			}
			bool result = true;
			if (RequireAllExchanges) {
				result = true;//clarity
				foreach (bool r in results) {
					if (!r) {
						result = false;
						#if DEBUG_CONVOS
						Debug.Log ("---- Require all exchanges and one was not met in " + exchange.Name);
						#endif
						break;
					}
				}
			} else {
				result = false;
				foreach (bool r in results) {
					if (r) {
						#if DEBUG_CONVOS
						Debug.Log ("---- Require one exchanges and one was met in " + exchange.Name);
						#endif
						result = true;
						break;
					}
				}
			}
			return result;
		}
	}

	[Serializable]
	public class RequireExchangeEnabled : ExchangeScript
	{
		public string ExchangeName	= string.Empty;
		public bool Enabled = false;

		protected override bool CheckRequirementsMet ()
		{
			Exchange target = null;
			if (conversation.GetExchange (ExchangeName, out target)) {
				bool result = target.Enabled == Enabled;
				return result;
			}
			return false;
		}
	}

	[Serializable]
	public class RequireLoanPaymentAvailable : ExchangeScript
	{
		public string OrganizationName = "Moneylender";
		public bool PaymentInFull = false;
		public float PaymentPercentage = 0;

		protected override bool CheckRequirementsMet ()
		{
			Loan outstandingLoan = null;
			bool canAfford = false;
			if (Moneylenders.Get.HasOutstandingLoan (OrganizationName, out outstandingLoan)) {
				if (PaymentInFull) {
					canAfford = Player.Local.Inventory.InventoryBank.CanAfford (outstandingLoan.AmountOwed, outstandingLoan.CurrencyType);
				} else {
					int paymentAmount = Mathf.FloorToInt (outstandingLoan.AmountOwed * PaymentPercentage);
					canAfford = Player.Local.Inventory.InventoryBank.CanAfford (paymentAmount, outstandingLoan.CurrencyType);
				}
			}
			return canAfford;
		}
	}

	[Serializable]
	public class RequireMissionStatus : ExchangeScript
	{
		public string MissionName = "Mission";
		public MissionStatus Status = MissionStatus.Active;

		protected override bool CheckRequirementsMet ()
		{
			MissionStatus status = MissionStatus.Dormant;
			if (Missions.Get.MissionStatusByName (MissionName, ref status)) {
				return Flags.Check ((uint)status, (uint)Status, Flags.CheckType.MatchAny);
			}
			return false;
		}
	}

	[Serializable]
	public class RequireMissionVariable : ExchangeScript
	{
		public string MissionName = "Mission";
		public string VariableName = "Variable";
		public int VariableValue = 0;
		public VariableCheckType CheckType = VariableCheckType.GreaterThanOrEqualTo;

		protected override bool CheckRequirementsMet ()
		{		
			int currentValue = 0;
			if (Missions.Get.MissionVariable (MissionName, VariableName, ref currentValue)) {
				#if DEBUG_CONVOS
				Debug.Log ("Found mission variable " + VariableName);
				#endif
				return GameData.CheckVariable (CheckType, VariableValue, currentValue);
			}
			return false;
		}
	}

	[Serializable]
	public class RequirePlayerVariable : ExchangeScript
	{
		public string VariableName = string.Empty;
		public string VariableEval = string.Empty;
		public int VariableValue = 0;
		public VariableCheckType CheckType = VariableCheckType.GreaterThanOrEqualTo;

		protected override bool CheckRequirementsMet ()
		{
			int defaultValue = 0;
			int currentValue = Player.GetPlayerVariable (VariableName, ref defaultValue);
			int checkValue = VariableValue;

			if (!string.IsNullOrEmpty (VariableEval)) {
				checkValue = GameData.Evaluate (VariableEval, conversation);
			}
			bool result = GameData.CheckVariable (CheckType, checkValue, currentValue);
			return result;
		}
	}

	[Serializable]
	public class RequireObjectiveStatus : ExchangeScript
	{
		public string MissionName = "Mission";
		public string ObjectiveName	= "Objective";
		public MissionStatus Status = MissionStatus.Active;
		public bool RequireHasStatus = true;

		protected override bool CheckRequirementsMet ()
		{
			#if DEBUG_CONVOS
			Debug.Log ("RequireObjectiveStatus: Mission: " + MissionName + ", Objective: " + ObjectiveName + ", Status: " + Status + ", RequireHasStatus: " + RequireHasStatus.ToString ());
			#endif
			MissionStatus status = MissionStatus.Dormant;
			if (Missions.Get.ObjectiveStatusByName (MissionName, ObjectiveName, ref status)) {
				bool hasStatus = Flags.Check ((uint)status, (uint)Status, Flags.CheckType.MatchAny);
				if (RequireHasStatus) {
					#if DEBUG_CONVOS
					Debug.Log ("Result: " + hasStatus.ToString ());
					#endif
					return hasStatus;
				} else {
					#if DEBUG_CONVOS
					Debug.Log ("Result: " + (!hasStatus).ToString ());
					#endif
					return !hasStatus;
				}
			} else {
				#if DEBUG_CONVOS
				Debug.Log ("Couldn't get objective status");
				#endif
			}
			return false;
		}
	}

	[Serializable]
	public class RequireOutstandingLoan : ExchangeScript
	{
		public string OrganizationName = "Moneylender";
		public string AmountOwedVariableName;
		public string CollateralName;

		protected override bool CheckRequirementsMet ()
		{
			Loan outstandingLoan = null;
			bool hasOutstanding = Moneylenders.Get.HasOutstandingLoan (OrganizationName, out outstandingLoan);
			if (hasOutstanding && !string.IsNullOrEmpty (AmountOwedVariableName)) {
				exchange.ParentExchange.ParentConversation.SetVariableValue (AmountOwedVariableName, outstandingLoan.AmountOwed);
			}
			return hasOutstanding;
		}
	}

	[Serializable]
	public class RequireLibraryBookOrder : ExchangeScript
	{
		public string LibraryName = "GuildLibrary";

		protected override bool CheckRequirementsMet ()
		{
			LibraryCatalogueEntry order = null;
			return Books.Get.HasPlacedOrder (LibraryName, out order);
		}
	}

	[Serializable]
	public class RequireQuestItem : ExchangeScript
	{
		public string ItemName = string.Empty;

		protected override bool CheckRequirementsMet ()
		{

			return Player.Local.Inventory.HasQuestItem (ItemName);
		}
	}

	[Serializable]
	public class RequireQuestItems : ExchangeScript
	{
		public List <string> ItemNames = new List <string> ();
		public bool RequireAllItems = true;
		public int MinimumNumber = 1;

		protected override bool CheckRequirementsMet ()
		{
			bool result = false;
			if (RequireAll) {
				bool hasAll = true;
				foreach (string questItemName in ItemNames) {
					if (!Player.Local.Inventory.HasQuestItem (questItemName)) {
						hasAll = false;
						break;
					}
				}
				result = hasAll;
			} else {
				int number = 0;
				foreach (string questItemName in ItemNames) {
					if (Player.Local.Inventory.HasQuestItem (questItemName)) {
						number++;
						if (number >= MinimumNumber) {
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}
	}

	[Serializable]
	public class RequireReputation : ExchangeScript
	{
		string CharacterName = string.Empty;
		int ReputationAmount = 50;
		VariableCheckType CheckType = VariableCheckType.GreaterThanOrEqualTo;

		protected override bool CheckRequirementsMet ()
		{
			string characterName = exchange.ParentConversation.SpeakingCharacter.worlditem.FileName;
			if (string.IsNullOrEmpty (CharacterName)) {
				CharacterName = conversation.SpeakingCharacter.worlditem.FileName;
			}
			int currentRep = Profile.Get.CurrentGame.Character.Rep.GetPersonalReputation (CharacterName);
			return GameData.CheckVariable (CheckType, ReputationAmount, currentRep);
		}
	}

	[Serializable]
	public class RequireSkillLearned: ExchangeScript
	{
		public string SkillName = "Sprint";
		public bool Learned = true;

		protected override bool CheckRequirementsMet ()
		{
			if (Skills.Get.HasLearnedSkill (SkillName)) {
				if (Learned) {
					return true;
				} else {
					return false;
				}
			} else {
				if (Learned) {
					return false;
				} else {
					return true;
				}
			}
		}
	}

	[Serializable]
	public class RequireSkillMastered : ExchangeScript
	{
		public string SkillName = "Sprint";
		public bool Mastered = true;

		protected override bool CheckRequirementsMet ()
		{
			if (Skills.Get.HasMasteredSkill (SkillName)) {
				//we've mastered it
				if (Mastered) {
					//and we want it to be mastered
					return true;
				} else {
					//we DON'T want it to be mastered
					return false;
				}
			} else {
				//we haven't mastered it
				if (Mastered) {
					//and we want it to be mastered
					return false;
				} else {
					//we DON'T want it to be mastered
					return true;
				}
			}
		}
	}

	[Serializable]
	public class ReqireSocialStatus : ExchangeScript
	{
		//do we need this?
	}

	[Serializable]
	public class RequireStatusCondition : ExchangeScript
	{
		public string ConditionName = string.Empty;

		protected override bool CheckRequirementsMet ()
		{
			return Player.Local.Status.HasCondition (ConditionName);
		}
	}

	[Serializable]
	public class RequireStructureOwner : ExchangeScript
	{
		public string OwnerName = "[Player]";
		public string StructurePath;

		protected override bool CheckRequirementsMet ()
		{
			StackItem structureStackItem = null;
			if (WIGroups.LoadStackItem (new MobileReference (StructurePath), out structureStackItem)) {
				//get the shingle state from the structure and see if it's owned
				System.Object shingleStateObject = null;
				if (structureStackItem.GetStateOf <Shingle> (out shingleStateObject)) {	//get state using generic method since it may be a stackitem
					ShingleState shingleState = (ShingleState)shingleStateObject;
					if (shingleState.IsOwnedBy (OwnerName)) {
						return true;
					}
				}
			}
			return false;
		}
	}

	[Serializable]
	public class RequireTimeElapsed : ExchangeScript
	{
		public string MissionName;
		public string VariableName;
		public int InGameHours = 1;
		public string InGameHoursEval;
		public bool RequireElapsed = true;
		//this assumes a mission variable was set to the current time in an earlier exchange
		protected override bool CheckRequirementsMet ()
		{
			int worldTimeWhenSet = 0;
			if (!string.IsNullOrEmpty (InGameHoursEval)) {
				InGameHours = GameData.Evaluate (InGameHoursEval, conversation);
			}
			if (Missions.Get.MissionVariable (MissionName, VariableName, ref worldTimeWhenSet)) {
				bool hasElapsed = (WorldClock.AdjustedRealTime >= (worldTimeWhenSet + WorldClock.HoursToSeconds (InGameHours)));
				if (RequireElapsed) {
					return hasElapsed;
				} else {
					return !hasElapsed;
				}
			}
			return false;
		}
	}

	[Serializable]
	public class RequireValidLoanOffer : ExchangeScript
	{
		public string VariableName;

		protected override bool CheckRequirementsMet ()
		{
			if (!string.IsNullOrEmpty (VariableName)) {
				exchange.ParentExchange.ParentConversation.SetVariableValue (VariableName, Moneylenders.Get.CurrentOfferPrincipal);
			}
			return Moneylenders.Get.CanMakeCurrentOffer;

		}
	}

	[Serializable]
	public class RequireWorldItem : ExchangeScript
	{
		public string WorldItemName = string.Empty;
		public string PackName = string.Empty;
		public string PrefabName = string.Empty;
		public int NumItems = 1;
		public VariableCheckType CheckType = VariableCheckType.GreaterThanOrEqualTo;

		protected override bool CheckRequirementsMet ()
		{			
			if (string.IsNullOrEmpty (WorldItemName)) {
				return true;
			}

			return Player.Local.Inventory.HasItem (WorldItemName);
		}
	}

	[Serializable]
	public class RequireWorldTrigger : ExchangeScript
	{
		public string TriggerName = "Trigger";
		public int ChunkID = 0;
		public int NumTimesWorldTriggered = 1;
		VariableCheckType CheckType = VariableCheckType.GreaterThanOrEqualTo;

		protected override bool CheckRequirementsMet ()
		{
			WorldChunk worldChunk = null;
			if (GameWorld.Get.ChunkByID (ChunkID, out worldChunk)) {
				WorldTriggerState worldTriggerState = null;
				if (worldChunk.GetTriggerState (TriggerName, out worldTriggerState)) {
					if (GameData.CheckVariable (CheckType, NumTimesWorldTriggered, worldTriggerState.NumTimesTriggered)) {
						return true;
					}
				}
			}
			return false;
		}
	}

	[Serializable]
	public class ResetLoanOffer : ExchangeScript
	{
		protected override void Action ()
		{
			//Debug.Log ("ResetLoanOffer");
			Moneylenders.Get.ResetCurrentOffer ();
		}
	}

	[Serializable]
	public class RevealLocation : ExchangeScript
	{
		public string LocationPath = string.Empty;

		protected override void Action ()
		{
			MobileReference mr = new MobileReference (LocationPath.Trim ());
			Player.Local.Surroundings.Reveal (mr);
			WorldMap.MarkLocation (mr);
		}
	}

	[Serializable]
	public class RevealNearestLocation : ExchangeScript
	{
		public string LocationCategory = "OrbSightings";

		protected override void Action ()
		{
			MobileReference mr = null;
			if (GameWorld.Get.GetNearestCategoryLocation (LocationCategory, Player.Local.Position, out mr)) {
				Player.Local.Surroundings.Reveal (mr);
				WorldMap.MarkLocation (mr);
			}

		}
	}

	[Serializable]
	public class SendMotileAction : ExchangeScript
	{
		public string CharacterName	= "[SpeakingCharacter]";
		public string LiveTargetName	= "[SpeakingCharacter]";
		public LiveTargetType TargetType = LiveTargetType.None;
		public MotileActionPriority Priority = MotileActionPriority.ForceBase;
		public MotileAction NewAction = new MotileAction ();

		protected override void Action ()
		{
			Character character = null;
			bool foundCharacter = true;

			switch (CharacterName) {
			case "[SpeakingCharacter]":
				character = conversation.SpeakingCharacter;
				break;

			default:
				if (!Characters.Get.SpawnedCharacter (CharacterName, out character)) {
					foundCharacter = false;
				}
				break;
			}

			if (!foundCharacter) {	//oops
				return;
			}

			switch (TargetType) {
			case LiveTargetType.None:
			default:
								//don't send a live target
				break;

			case LiveTargetType.Character:
				switch (LiveTargetName) {
				case "[SpeakingCharacter]":
					NewAction.LiveTarget = character.worlditem;
					break;

				default:
					Character liveTargetCharacter = null;
					if (Characters.Get.SpawnedCharacter (LiveTargetName, out liveTargetCharacter)) {
						NewAction.LiveTarget = liveTargetCharacter.worlditem;
					}
					break;
				}
				break;

//								case LiveTargetType.Conversation:
//										NewAction.LiveTarget = conversation.gameObject;
//										break;
//
//								case LiveTargetType.Mission:
//										NewAction.LiveTarget = Missions.Get.MissionByName(LiveTargetName).gameObject;//dangerous
//										break;

			case LiveTargetType.Player:
				NewAction.LiveTarget = Player.Local;
				break;
			}


			Motile motile = null;
			if (character.worlditem.Is< Motile> (out motile)) {
				motile.PushMotileAction (NewAction, Priority);
			}
		}
	}

	[Serializable]
	public class SetActiveMuseum : ExchangeScript
	{
		public string MuseumName = "GuildMuseum";

		protected override void Action ()
		{	//works
			Museums.Get.SetActiveMuseum (MuseumName);
		}
	}

	[Serializable]
	public class SetLoanOfferProperties : ExchangeScript
	{
		public string OrganizationName = string.Empty;
		public float DailyInterestRate = -1f;
		public string Collateral = string.Empty;
		public int PrincipalBase = -1;
		public int PrincipalAdd = -1;
		public string CollateralReputation = string.Empty;
		public WICurrencyType CurrencyType = WICurrencyType.None;

		protected override void Action ()
		{	//this will set current offer props based on which ones are NOT set to default
			//so exchange scripts can define only the ones they intend to use
			Moneylenders.Get.SetCurrentOfferOrganizationName (OrganizationName);
			Moneylenders.Get.SetCurrentOfferDailyInterestRate (DailyInterestRate);
			Moneylenders.Get.AddCollateralToCurrentOffer (Collateral);
			Moneylenders.Get.SetCurrentOfferPrincipal (PrincipalBase);
			Moneylenders.Get.AddToCurrentOfferPrincipal (PrincipalAdd);
			Moneylenders.Get.SetCurrentOfferCurrencyType (CurrencyType);
			Moneylenders.Get.AddCollateralReputationToCurrentOffer (CollateralReputation);
		}
	}

	[Serializable]
	public class SetPlayerCredentials : ExchangeScript
	{
		public string CredentialsFlagset = "CredentialsGuild";
		public string CredentialsValue = "Novice";

		protected override void Action ()
		{
			int credentialsValue = Profile.Get.CurrentGame.Character.Exp.LastCredByFlagset (CredentialsFlagset);
			credentialsValue++;
			Profile.Get.CurrentGame.Character.Exp.SetLastCredentials (CredentialsFlagset, credentialsValue);
		}
	}

	[Serializable]
	public class SetExchangeEnabled : ExchangeScript
	{
		public List <string> Exchanges = new List <string> ();
		public bool Enabled = false;

		protected override void Action ()
		{
			List <Exchange> exchangesToChange = conversation.GetExchanges (Exchanges);
			foreach (Exchange exchangeToChange in exchangesToChange) {
				if (Enabled) {
					exchangeToChange.Disable = false;
				} else {
					exchangeToChange.Disable = true;
				}
			}
		}
	}

	[Serializable]
	public class SetStructureOwner : ExchangeScript
	{
		public string StructurePath;
		public string OwnerName;

		protected override void Action ()
		{
			StackItem structureStackItem = null;
			if (WIGroups.LoadStackItem (new MobileReference (StructurePath), out structureStackItem)) {
				//get the shingle state from the structure and see if it's owned
				System.Object shingleStateObject = null;
				if (structureStackItem.GetStateOf <Shingle> (out shingleStateObject)) {	//get state using generic method since it may be a stackitem
					ShingleState shingleState = (ShingleState)shingleStateObject;
					shingleState.MoneylenderOwner = OwnerName;
					//TODO save stackitem
				}
			}
		}
	}

	[Serializable]
	public class SetQuestItemVisibility : ExchangeScript
	{
		public string ItemName;
		public bool Visible = true;

		protected override void Action ()
		{
			WorldItem worlditem = null;
			if (GameWorld.Get.QuestItem (ItemName, out worlditem)) {
				QuestItem questItem = worlditem.Get <QuestItem> ();
				questItem.SetQuestItemVisibility (Visible);
			}
		}
	}

	[Serializable]
	public class ShowVariable : ExchangeScript
	{
		public string VariableName = string.Empty;
		public bool Show = true;

		protected override void Action ()
		{
			conversation.ShowVariable (VariableName, Show);
		}
	}

	[Serializable]
	public class SubstituteConversation : ExchangeScript
	{
		public string OldConversationName = string.Empty;
		public string NewConversationName = string.Empty;
		public string CharacterName = string.Empty;
		public bool DTSOverride = false;

		protected override void Action ()
		{
			if (string.IsNullOrEmpty (NewConversationName)) {
				return;
			}
			if (string.IsNullOrEmpty (OldConversationName)) {
				OldConversationName = conversation.Props.Name;
			}
			if (string.IsNullOrEmpty (CharacterName)) {
				CharacterName = conversation.SpeakingCharacter.worlditem.FileName;
			}
			if (NewConversationName.Contains ("*")) {
				NewConversationName = Substitution (OldConversationName, NewConversationName);
			}

			if (DTSOverride) {
				Frontiers.Conversations.Get.AddDTSOverride (OldConversationName, NewConversationName, CharacterName);
			} else {
				Frontiers.Conversations.Get.AddSubstitution (OldConversationName, NewConversationName, CharacterName);
			}
		}

		public static string Substitution (string oldConversationName, string newConversationName)
		{
			#if DEBUG_CONVOS
			Debug.Log ("Substituting old " + oldConversationName + " with new " + newConversationName);
			#endif
			//oldConversationName - the original - conversation name format is CharName-Enc-Act-##-Mission-##
			//newConversationName - the new - this uses the format of * or *_#
			if (newConversationName.Equals("*")) {
				//in the case of * we just have to replace the thing outright
				newConversationName = oldConversationName;
				#if DEBUG_CONVOS
				Debug.Log ("Straight replacement");
				#endif
			} else {
				//in the case of *_# we're trying to swap out the last number
				//we have to split the string and replace the number at the end
				string[] splitConversationName = oldConversationName.Split (new String [] { "-" }, StringSplitOptions.RemoveEmptyEntries);
				//this will give us [blah][blah][blah][etc][#]
				//replace the [#] with [*_#], minus the *
				string newNumber = newConversationName.Replace ("*", "");
				splitConversationName [splitConversationName.Length - 1] = newNumber;
				#if DEBUG_CONVOS
				Debug.Log ("Replacing old number with new number " + newNumber);
				#endif
				//the re-join everything so we get the new conversation name with the new number
				newConversationName = string.Join ("-", splitConversationName);
				//finally clean the file name so we get rid of all "_" characters
				newConversationName = DataImporter.GetNameFromDialogName (newConversationName);
			}
			#if DEBUG_CONVOS
			Debug.Log ("Got " + newConversationName + " from " + oldConversationName);
			#endif
			return newConversationName;
		}
	}

	[Serializable]
	public class TakeMoneyFromPlayer : ExchangeScript
	{
		public int CurrencyValue = 0;
		public WICurrencyType CurrencyType = WICurrencyType.A_Bronze;
		public string CurrencyValueEval = string.Empty;

		protected override bool CheckRequirementsMet ()
		{
			if (!string.IsNullOrEmpty (CurrencyValueEval)) {
				CurrencyValue = GameData.Evaluate (CurrencyValueEval, conversation);
			}
			return Player.Local.Inventory.InventoryBank.CanAfford (CurrencyValue, CurrencyType);
		}

		protected override void Action ()
		{
			if (!string.IsNullOrEmpty (CurrencyValueEval)) {
				CurrencyValue = GameData.Evaluate (CurrencyValueEval, conversation);
			}
			int numRemoved = 0;
			Player.Local.Inventory.InventoryBank.TryToRemove (CurrencyValue, CurrencyType);
		}
	}

	[Serializable]
	public class TakeQuestItemFromPlayer : ExchangeScript
	{
		public string ItemName = string.Empty;

		protected override void Action ()
		{
			Player.Local.Inventory.TakeQuestItemFromPlayer (ItemName);
		}
	}

	[Serializable]
	public class TakeWorldItemFromPlayer : ExchangeScript
	{
		public string ItemName = string.Empty;
		public bool TakeAll = true;
		public int NumItems = 0;

		protected override void Action ()
		{
			throw new NotImplementedException ();
		}
	}

	[Serializable]
	public class TeachPlayerSkill : ExchangeScript
	{
		public string SkillName;

		protected override void Action ()
		{
			Skills.LearnSkill (SkillName);
		}
	}

	[Serializable]
	public class TriggerCutscene : ExchangeScript
	{
		public string CutsceneName;
		public string AnchorNodeName;
		public int AnchorNodeChunkID;

		protected override void Action ()
		{
			WorldChunk chunk = null;
			ActionNodeState nodeState = null;
			if (GameWorld.Get.ChunkByID (AnchorNodeChunkID, out chunk)) {
				if (chunk.GetNode (AnchorNodeName, false, out nodeState)) {
					//end the current conversation
					conversation.ShowCutsceneOnFinish (CutsceneName, nodeState.actionNode.gameObject);
				}
			}
		}
	}
}