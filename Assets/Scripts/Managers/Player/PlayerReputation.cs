using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Frontiers.GUI;

namespace Frontiers
{
		[Serializable]
		public class PlayerReputation
		{
				public int DefaultReputation {
						get {
								return 50;
						}
				}

				public float GetNormalizedGlobalReputation()
				{
						return ((float)GlobalReputation) / 100f;
				}

				public float NormalizedOffsetGlobalReputation {
						get {
								return (NormalizedGlobalReputation * 2f) - 1f;
						}
				}

				public float NormalizedGlobalReputation {
						get {
								return ((float)GlobalReputation) / 100f;
						}
				}

				public int GlobalReputation {
						get {
								return GlobalReputationModifier.ReputationChange;
						}
						set {
								GlobalReputationModifier.ReputationChange = value;
						}
				}

				public void ChangeGlobalReputation(int repChange)
				{
						if (repChange == 0) {
								return;
						}

						if (repChange > 0) {
								GainGlobalReputation(repChange);
						} else {
								LoseGlobalReputation(Mathf.Abs(repChange));
						}
				}
				//global reputation is added to every newly created reputation
				public ReputationModifier GlobalReputationModifier = new ReputationModifier("GlobalReputation", 50, 0.5f, false, 0f, 0f, 0);
				public SDictionary <string, ReputationState> CharacterReputation = new SDictionary <string, ReputationState>();

				public ReputationState GetNewRepForCharacter(string characterName)
				{
						//TODO add faction etc.
						ReputationState repState = new ReputationState();
						repState.Modifiers.Add(GlobalReputationModifier);
						return repState;
				}

				public ReputationState GetReputation(string characterName)
				{
						ReputationState rep = null;
						if (!CharacterReputation.TryGetValue(characterName, out rep)) {
								rep = GetNewRepForCharacter(characterName);
								CharacterReputation.Add(characterName, rep);
								Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcReputationChange, WorldClock.AdjustedRealTime);
						}
						return rep;
				}

				public int GetPersonalReputation(string characterName)
				{
						ReputationState rep = null;
						if (!CharacterReputation.TryGetValue(characterName, out rep)) {
								rep = GetNewRepForCharacter(characterName);
								CharacterReputation.Add(characterName, rep);
								Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcReputationChange, WorldClock.AdjustedRealTime);
						}
						return rep.FinalReputation;
				}

				public void GainGlobalReputation(int reputationGain)
				{
						reputationGain = Mathf.Clamp(reputationGain, 1, Globals.ReputationChangeMurderer);
						GlobalReputation = (int)Mathf.Clamp(GlobalReputation + reputationGain, Globals.MinReputation, Globals.MaxReputation);
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcReputationGain, WorldClock.AdjustedRealTime);
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcReputationChange, WorldClock.AdjustedRealTime);
						GUIManager.PostSuccess("Gained " + reputationGain.ToString() + " reputation in general");
				}

				public void LoseGlobalReputation(int reputationLoss)
				{
						reputationLoss = Mathf.Clamp(reputationLoss, 1, Globals.ReputationChangeLarge);
						GlobalReputation = (int)Mathf.Clamp(GlobalReputation - reputationLoss, Globals.MinReputation, Globals.MaxReputation);
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcReputationLose, WorldClock.AdjustedRealTime);
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcReputationChange, WorldClock.AdjustedRealTime);
						GUIManager.PostDanger("Lost " + reputationLoss.ToString() + " reputation in general");
				}

				public void GainPersonalReputation(string characterName, string characterDisplayName, int reputationGain)
				{
						reputationGain = Mathf.Clamp(reputationGain, 1, Globals.ReputationChangeMurderer);
						ReputationState rep = null;
						if (!CharacterReputation.TryGetValue(characterName, out rep)) {
								rep = GetNewRepForCharacter(characterName);
								CharacterReputation.Add(characterName, rep);
						}
						rep.PersonalReputation = (int)Mathf.Clamp(rep.PersonalReputation + reputationGain, Globals.MinReputation, Globals.MaxReputation);
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcReputationGain, WorldClock.AdjustedRealTime);
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcReputationChange, WorldClock.AdjustedRealTime);
						if (!string.IsNullOrEmpty(characterDisplayName)) {
								GUIManager.PostSuccess("Gained " + reputationGain.ToString() + " reputation with " + characterDisplayName);
						}
				}

				public void LosePersonalReputation(string characterName, string characterDisplayName, int reputationLoss)
				{
						reputationLoss = Mathf.Clamp(reputationLoss, 1, Globals.ReputationChangeMurderer);
						ReputationState rep = null;
						if (!CharacterReputation.TryGetValue(characterName, out rep)) {
								rep = GetNewRepForCharacter(characterName);
								CharacterReputation.Add(characterName, rep);
						}
						rep.PersonalReputation = (int)Mathf.Clamp(rep.PersonalReputation - reputationLoss, Globals.MinReputation, Globals.MaxReputation);
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcReputationLose, WorldClock.AdjustedRealTime);
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcReputationChange, WorldClock.AdjustedRealTime);
						if (!string.IsNullOrEmpty(characterDisplayName)) {
								GUIManager.PostDanger("Lost " + reputationLoss.ToString() + " reputation with " + characterDisplayName);
						}
				}

				public void SetPersonalReputation(string characterName, string characterDisplayName, int reputationValue)
				{
						ReputationState rep = null;
						if (!CharacterReputation.TryGetValue(characterName, out rep)) {
								rep = GetNewRepForCharacter(characterName);
								CharacterReputation.Add(characterName, rep);
						}
						rep.PersonalReputation = reputationValue;
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcReputationChange, WorldClock.AdjustedRealTime);
				}
		}

		[Serializable]
		public class ReputationState
		{
				//this will return a value from -1 to 1
				//useful for penalties
				public float NormalizedOffsetReputation {
						get {
								return (NormalizedReputation * 2f) - 1f;
						}
				}

				public float NormalizedReputation {
						get {
								return ((float)FinalReputation / 100f);
						}
				}

				public int FinalReputation { 
						get {
								int personalReputation = PersonalReputation;
								RefreshRepModifiers();
								for (int i = 0; i < Modifiers.Count; i++) {
										personalReputation = Modifiers[i].ModifyRep(personalReputation);
								}
								return personalReputation;
						}
				}

				public int PersonalReputation = 50;

				public float NormalizedReputationDifference(int otherRep)
				{
						int difference = Mathf.Abs(FinalReputation - otherRep);
						return ((float)difference / (float)Globals.MaxReputation);
				}

				public List <ReputationModifier> Modifiers = new List <ReputationModifier>();

				public void AddModifier(ReputationModifier repMod)
				{
						RefreshRepModifiers();
						//remove all existing modifiers that match this one
						//Debug.Log ("Adding rep modifier " + repMod.Name);
						for (int i = Modifiers.LastIndex(); i >= 0; i--) {
								if (Modifiers[i].Name == repMod.Name || Modifiers[i] == repMod) {
										Modifiers.RemoveAt(i);
								}
						}
						//insert the new mod at the head of the list
						Modifiers.Insert(0, repMod);
						//this will reset its effects
						repMod.TimeAdded = WorldClock.AdjustedRealTime;
				}

				public bool HasModifier(string modifierName, out ReputationModifier repMod)
				{
						repMod = null;
						for (int i = 0; i < Modifiers.Count; i++) {
								if (Modifiers[i].Name == modifierName) {
										repMod = Modifiers[i];
										break;
								}
						}
						return repMod != null;
				}

				protected void RefreshRepModifiers()
				{
						for (int i = Modifiers.LastIndex(); i >= 0; i--) {
								if (Modifiers[i].IsTemporary) {
										if (WorldClock.AdjustedRealTime > Modifiers[i].EndTime) {
												//time to remove it and apply final penalty
												//Debug.Log ("Reputation modifier " + Modifiers [i].Name + " has expired - applying final penalty of " + Modifiers [i].ReputationChangeOnFinished.ToString ());
												PersonalReputation += Modifiers[i].ReputationChangeOnFinished;
												Modifiers.RemoveAt(i);
										}
								}
						}
				}

				public static float NormalizeRep(int rep)
				{
						return ((float)rep) / 100f;
				}

				public static string ReputationToDescription(int rep)
				{
						//TODO these suck, fix them
						string description = "Decent";
						if (rep > 95) {
								description = "Excellent";
						}
						if (rep > 80) {
								description = "Good";
						}
						if (rep > 60) {
								description = "Neutral";
						}
						if (rep > 40) {
								description = "Neutral";
						}
						if (rep > 20) {
								description = "Decent";
						}
						if (rep <= 5) {
								description = "Poor";
						}

						return description;
				}
		}
		//is this class really necessary?
		//whatever i'm keeping it
		[Serializable]
		public class ReputationModifier
		{
				public ReputationModifier(string name, int reputationChange, float reputationWeight, bool isTemporary, float rtDuration, float timeAdded, int reputationChangeOnFinished)
				{
						Name = name;
						ReputationChange = reputationChange;
						ReputationWeight = reputationWeight;
						IsTemporary = isTemporary;
						RTDuration = rtDuration;
						TimeAdded = timeAdded;
						ReputationChangeOnFinished = reputationChangeOnFinished;
				}

				public ReputationModifier(string name)
				{
						Name = name;
				}

				public ReputationModifier()
				{

				}

				public int ModifyRep(int rep)
				{
						return Mathf.CeilToInt(rep * (1.0f - ReputationWeight)) + Mathf.FloorToInt(ReputationChange * ReputationWeight);
				}

				public double EndTime {
						get {
								return TimeAdded + RTDuration;
						}
				}

				public string Name = "Modifier";
				public int ReputationChange = 0;
				public float ReputationWeight = 0.5f;
				public bool IsTemporary = true;
				public double RTDuration = 0f;
				public double TimeAdded = 0f;
				public int ReputationChangeOnFinished = 0;
		}
}