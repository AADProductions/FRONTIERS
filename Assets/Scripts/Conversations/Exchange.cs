using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.World.Gameplay;
using Frontiers.World;
using Frontiers.Data;
using System.Linq;

namespace Frontiers.Story.Conversations
{
		[Serializable]
		public class Exchange : IComparable <Exchange>
		{
				public string Name = string.Empty;
				public int Index = 0;
				public int DisplayOrder = -1;

				[XmlIgnore]
				public bool Enabled {
						get {
								return (!Disable && RequirementsAreMet);
						}
				}

				[XmlIgnore]
				public bool Disable {
						get {
								return ParentConversation.IsDisabled(Name);
						}
						set {
								ParentConversation.SetDisabled(Name, value);
						}
				}

				[XmlIgnore]
				public List <Exchange> OutgoingChoices {
						get {
								if (mOutgoingChoices == null) {
										mOutgoingChoices = new List <Exchange>();
								}
								return mOutgoingChoices;
						}
				}

				[XmlIgnore]
				[NonSerialized]
				public Conversation ParentConversation = null;
				[XmlIgnore]
				[NonSerialized]
				public Exchange ParentExchange = null;
				public bool AlwaysInclude = false;
				public ExchangeOutgoingStyle OutgoingStyle = ExchangeOutgoingStyle.Normal;
				public AvailabilityBehavior Availability = AvailabilityBehavior.Always;
				public int MaxTimesChosen = 0;
				public List <string> DisabledIncomingChoices = new List <string>();
				public List <string> LinkedIncomingChoices = new List <string>();
				public List <string> LinkedOutgoingChoices	= new List <string>();
				public string PlayerDialog = "Player dialog.";
				public string PlayerDialogSummary = string.Empty;
				public string CharacterResponse = "Character dialog.";
				public string DtsOnFailure = string.Empty;
				public List <string> OutgoingChoiceNames = new List<string>();
				public List <ExchangeScript> Scripts = new List <ExchangeScript>();

				public string ParentExchangeName { 
						get {
								if (HasParentExchange) {
										mParentExchangeName = ParentExchange.Name;
								}
								return mParentExchangeName;
						}
						set {
								mParentExchangeName = value;
						}
				}

				public int GetWordCount()
				{
						int charCount = PlayerDialog.Count(Char.IsWhiteSpace) + CharacterResponse.Count(Char.IsWhiteSpace);
						for (int i = 0; i < OutgoingChoices.Count; i++) {
								charCount += OutgoingChoices[i].GetWordCount();
						}
						return charCount;
				}

				[XmlIgnore]
				public int NumTimesChosen {
						get {
								return ParentConversation.NumTimesChosen(Name);
						}
				}

				[XmlIgnore]
				public bool Continues {
						get {
								return PlayerOutgoingChoices.Count > 0;
						}
				}

				[XmlIgnore]
				public bool IsAvailable {
						get {
								if (!Enabled) {
										return false;
								}

								bool isAvailable = true;

								switch (Availability) {
										case AvailabilityBehavior.Once:
												int numTimesChosen = NumTimesChosen;
												if (numTimesChosen > 0) {
														isAvailable = false;
												}
												break;

										case AvailabilityBehavior.Max:
												if (NumTimesChosen >= MaxTimesChosen) {
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

				[XmlIgnore]
				public bool HasParentExchange {
						get {
								return ParentExchange != null;
						}
				}

				[XmlIgnore]
				public bool HasIncomingChoices {
						get {
								return mIncomingChoices.Count > 0;
						}
				}

				[XmlIgnore]
				public bool HasOutgoingChoices {
						get {
								return PlayerOutgoingChoices.Count > 0;
						}
				}

				[XmlIgnore]
				public bool RequirementsAreMet {
						get {
								CheckRequirements();
								return mRequirementsMet;
						}
				}

				[XmlIgnore]
				public Exchange IncomingChoice {
						get {
								Exchange lastIncomingChoice = null;
								foreach (Exchange incomingChoice in mIncomingChoices) {
										if (incomingChoice.Name == mLastIncomingChoice) {
												lastIncomingChoice = incomingChoice;
												break;
										}
								}
								return lastIncomingChoice;
						}
				}

				[XmlIgnore]
				public List <Exchange> IncomingChoices {
						get {
								List <Exchange> incomingChoices = new List <Exchange>();
								incomingChoices.AddRange(mIncomingChoices);
								return incomingChoices;
						}
				}

				[XmlIgnore]
				public Exchange LastOutgoingChoice {
						get {
								Exchange lastOutgoingChoice = null;
								foreach (Exchange outgoingChoice in PlayerOutgoingChoices) {
										if (outgoingChoice.Name == mLastOutgoingChoice) {
												lastOutgoingChoice = outgoingChoice;
												break;
										}
								}
								return lastOutgoingChoice;
						}
				}

				[XmlIgnore]
				public HashSet <Exchange> PlayerOutgoingChoices {
						get {
								RefreshOutgoingChoices();
								return mPlayerOutgoingChoices;
						}
				}

				[XmlIgnore]
				public string CleanPlayerDialog {
						get {
								if (ParentConversation != null) {
										return ParentConversation.CleanDialog(PlayerDialog);
								}
								return PlayerDialog;
						}
				}

				[XmlIgnore]
				public string CleanCharacterResponse {
						get {
								if (ParentConversation != null) {
										return ParentConversation.CleanDialog(CharacterResponse);
								}
								return CharacterResponse;
						}
				}

				[XmlIgnore]
				public int NumResponsePages {
						get {
								//holy crap, find a way to do this that doesn't generate so much garbage
								string[] characterResponsePages = CharacterResponse.Split(Conversation.PageBreakStrings, StringSplitOptions.RemoveEmptyEntries);
								return characterResponsePages.Length;
						}
				}

				[XmlIgnore]
				public string ResultsSummary {
						get {
								string resultsSummary = string.Empty;
								foreach (string resultDescription in mResultsSummaryList) {
										if (!resultsSummary.Contains(resultDescription)) {
												resultsSummary += resultDescription;
										}
								}
								return resultsSummary;
						}
						set {
								mResultsSummaryList.Add(value);
						}
				}

				public bool Choose(Exchange incomingChoice)
				{
						if (incomingChoice != null) {
								mLastIncomingChoice = incomingChoice.Name;
						} else {
								mLastIncomingChoice = string.Empty;
						}
						foreach (ExchangeScript script in Scripts) {
								script.OnChoose(mLastIncomingChoice);
						}
						return true;
				}

				public void CheckRequirements()
				{
						if (mCheckingRequirements) {
								//if we're already checking
								//this means a loop is in progress
								//the loop will return false if the requirements really aren't met
								//so return true in the meantime
								mRequirementsMet = true;
								return;
						}

						mCheckingRequirements = true;
						for (int i = Scripts.Count - 1; i >= 0; i--) {
								ExchangeScript requirement = Scripts[i];
								if (requirement == null) {
										Scripts.RemoveAt(i);
								} else if (!Scripts[i].RequirementsMet) {
										mRequirementsMet = false;
										DtsOnFailure = Scripts[i].DtsOnFailure;
										mCheckingRequirements = false;
										return;
								}
						}
						mCheckingRequirements = false;
						mRequirementsMet = true;
				}

				protected bool mCheckingRequirements = false;

				public void SetOwners(Conversation newOwner, Exchange newParentExchange)
				{
						if (newOwner != null) {
								ParentConversation = newOwner;
						}

						if (newParentExchange != null && newParentExchange != this) {
								ParentExchange = newParentExchange;
						}

						if (mOutgoingChoices != null) {
								mOutgoingChoices.Clear();
						} else {
								mOutgoingChoices = new List <Exchange>();
						}
						Exchange outgoingChoice = null;
						for (int i = 0; i < OutgoingChoiceNames.Count; i++) {
								if (ParentConversation.GetExchange(OutgoingChoiceNames[i], out outgoingChoice)) {
										mOutgoingChoices.Add(outgoingChoice);
								}
						}
				}

				public void OnConcludeExchange()
				{
						foreach (ExchangeScript script in Scripts) {
								script.OnConclude(mLastOutgoingChoice);
						}
				}

				public void Refresh()
				{
						//the purpose of this function is NOT to gather the final outgoing choices
						//it's to link everything up appropriately
						mIncomingChoices.Clear();

						RefreshOutgoingChoices();

						if (HasParentExchange) {
								mIncomingChoices.Add(ParentExchange);
						}

						foreach (string incomingName in LinkedIncomingChoices) {
								Exchange incoming = null;
								if (ParentConversation.GetExchange(incomingName, out incoming)) {
										if (!mIncomingChoices.Contains(incoming) && incoming != this) {
												mIncomingChoices.Add(incoming);
										}
								}
						}

						for (int i = 0; i < Scripts.Count; i++) {
								Scripts[i].Initialize(this, ParentConversation);
						}
				}

				protected void RefreshOutgoingChoices()
				{
						if (mPlayerOutgoingChoices == null) {
								mPlayerOutgoingChoices = new HashSet <Exchange>();
								for (int i = 0; i < OutgoingChoices.Count; i++) {
										mPlayerOutgoingChoices.Add(OutgoingChoices[i]);
								}
								for (int i = 0; i < LinkedOutgoingChoices.Count; i++) {
										string linkedOutgoingChoiceName = LinkedOutgoingChoices[i];
										Exchange linkedOutgoingChoice = null;
										if (ParentConversation.GetExchange(linkedOutgoingChoiceName, out linkedOutgoingChoice)) {
												mPlayerOutgoingChoices.Add(linkedOutgoingChoice);
										} else {
												Debug.LogWarning("DIDN'T FIND LINKED OUTGOING CHOICE NAME " + linkedOutgoingChoiceName);
										}
								}
						}
				}

				#region add, remove and create

				public void AddScript <T>() where T : ExchangeScript, new()
				{
						T newExchangeScript = new T();
						newExchangeScript.Initialize(this, ParentConversation);
						Scripts.Add(newExchangeScript);
				}

				#endregion

				#region IComparable implementation

				public int CompareTo(Exchange o)
				{
						if (o.DisplayOrder < DisplayOrder) {
								return 1;
						} else if (o.DisplayOrder > DisplayOrder) {
								return -1;
						}
						return 0;
				}

				#endregion

				protected string mParentExchangeName = string.Empty;
				protected string mLastIncomingChoice = string.Empty;
				protected string mLastOutgoingChoice = string.Empty;
				protected bool mRequirementsMet = true;
				protected HashSet <string> mResultsSummaryList = new HashSet <string>();
				[NonSerialized]
				protected HashSet <Exchange> mPlayerOutgoingChoices = null;
				[NonSerialized]
				protected List <Exchange> mIncomingChoices = new List <Exchange>();
				[NonSerialized]
				protected List <Exchange> mOutgoingChoices = null;
		}
}
