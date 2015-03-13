using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using System;

namespace Frontiers
{
		public class Museums : Manager
		{
				public static Museums Get;
				public MuseumCurator ActiveCurator;
				public Museum ActiveMuseum;

				public bool HasActiveMuseum { 
						get {
								return ActiveMuseum != null;
						}
				}

				public void SetActiveMuseum(string museumName)
				{
						if (!HasActiveMuseum || ActiveMuseum.Name != museumName) {
								Mods.Get.Runtime.LoadMod <Museum>(ref ActiveMuseum, "Museum", museumName);
						}
				}

				public List <Artifact> ActiveCuratedArtifacts = new List <Artifact>();
				//we don't display shards so we don't keep active curated shards
				public override void WakeUp()
				{
						base.WakeUp();

						Get = this;
						ActiveMuseum = null;
				}

				protected List <IWIBase> mAvailableArtifacts = new List <IWIBase>();
				protected System.Object mArtifactStateObject = null;
				protected ArtifactState mArtifactState = null;
				protected ArtifactShardState mArtifactShardState = null;
				protected IWIBase mCurrentArtifact = null;

				public int UndatedShardAvailable { get { return UndatedShard.Count; } }

				public int UndatedSmallAvailable { get { return UndatedSmall.Count; } }

				public int DatedShardAvailable { get { return DatedShard.Count; } }

				public int DatedSmallAvailable { get { return DatedSmall.Count; } }

				public int DatedLargeAvailable { get { return DatedLarge.Count; } }

				public List <IWIBase> UndatedShard = new List<IWIBase>();
				public List <IWIBase> UndatedSmall = new List<IWIBase>();
				public List <IWIBase> DatedShard = new List<IWIBase>();
				public List <IWIBase> DatedSmall = new List<IWIBase>();
				public List <IWIBase> DatedLarge = new List<IWIBase>();
				int LastOfferMade = 0;
				public string LastOfferType = string.Empty;
				List <IWIBase> LastOfferItems = null;

				public void RefreshCuratedItems()
				{
						//looks through each worlditem in the current museum's list
						//and sets it to visible / proper quality
						for (int i = 0; i < ActiveCuratedArtifacts.Count; i++) {
								Artifact artifact = ActiveCuratedArtifacts[i];
								if (artifact != null && artifact.State.MuseumName == ActiveMuseum.Name) {
										//see if the artifact list contains its age
										//if it does, make it visible with the highest quality
										//otherwise make it invisible
										bool isVisible = false;
										ArtifactQuality aquiredQuality;
										SDictionary <ArtifactAge, ArtifactQuality> lookup = null;
										if (artifact.StackName.Contains("Large")) {
												lookup = ActiveMuseum.LargeArtifactsAquired;
										} else {
												lookup = ActiveMuseum.SmallArtifactsAquired;
										}
										if (lookup.TryGetValue(artifact.State.Age, out aquiredQuality)) {
												//we've aquired this age artifact
												//so make it visible and set the quality
												artifact.SetCuratedProperties(true, aquiredQuality);
										} else {
												//we havnen't aquired it, make it invisible
												artifact.SetCuratedProperties(false, ArtifactQuality.VeryPoor);
										}
								}
						}
				}

				public void RefreshAvailableArtifacts()
				{
						UndatedShard.Clear();
						UndatedSmall.Clear();
						DatedShard.Clear();
						DatedSmall.Clear();
						DatedLarge.Clear();

						mAvailableArtifacts.Clear();
						//this goes through the player's inventory and looks for
						//anything they can sell to the museum
						//this is used by converstions to search the player's inventory once
						//instead of for every exchange
						Player.Local.Inventory.FindItemsOfType("Artifact", mAvailableArtifacts);
						for (int i = 0; i < mAvailableArtifacts.Count; i++) {
								mCurrentArtifact = mAvailableArtifacts[i];
								if (mCurrentArtifact.GetStateOf <Artifact>(out mArtifactStateObject)) {
										mArtifactState = (ArtifactState)mArtifactStateObject;
										//check if it's dated or not
										if (mArtifactState.HasBeenDated) {
												if (mCurrentArtifact.StackName.Contains("Large")) {
														DatedLarge.SafeAdd(mCurrentArtifact);
												} else {
														DatedSmall.SafeAdd(mCurrentArtifact);
												}
										} else {
												UndatedSmall.SafeAdd(mCurrentArtifact);
										}
								}
						}

						mAvailableArtifacts.Clear();
						Player.Local.Inventory.FindItemsOfType("ArtifactShard", mAvailableArtifacts);
						for (int i = 0; i < mAvailableArtifacts.Count; i++) {
								mCurrentArtifact = mAvailableArtifacts[i];
								if (mCurrentArtifact.GetStateOf <ArtifactShard>(out mArtifactStateObject)) {
										mArtifactShardState = (ArtifactShardState)mArtifactStateObject;
										//check if it's dated or not
										if (mArtifactState.HasBeenDated) {
												DatedShard.SafeAdd(mCurrentArtifact);
										} else {
												UndatedShard.SafeAdd(mCurrentArtifact);
										}
								}
						}

						/*
						Debug.Log("Found " + UndatedShardAvailable.ToString() + " Undated Shard Available");
						Debug.Log("Found " + UndatedSmallAvailable.ToString() + " Undated Small Available");
						Debug.Log("Found " + DatedShardAvailable.ToString() + " Dated Shard Available");
						Debug.Log("Found " + DatedSmallAvailable.ToString() + " Dated Small Available");
						Debug.Log("Found " + DatedLargeAvailable.ToString() + " Dated Large Available");
						*/
				}

				public int CalculateOffer(string itemType)
				{
						LastOfferType = itemType;
						LastOfferMade = 0;
						LastOfferItems = null;
						switch (LastOfferType) {
								case "UndatedShard":
										LastOfferItems = UndatedShard;
										break;

								case "UndatedSmall":
										LastOfferItems = UndatedSmall;
										break;

								case "DatedShard":
										LastOfferItems = DatedShard;
										break;

								case "DatedSmall":
										LastOfferItems = DatedSmall;
										break;

								case "DatedLarge":
										LastOfferItems = DatedLarge;
										break;

								default:
										Debug.Log("Couldn't find artifact type " + itemType);
										return 0;
						}

						for (int i = 0; i < LastOfferItems.Count; i++) {
								LastOfferMade += CalculateValueOfArtifact(LastOfferItems[i]);
						}
						return LastOfferMade;
				}

				public int CalculateValueOfArtifact(IWIBase artifact)
				{
						return Mathf.FloorToInt(artifact.BaseCurrencyValue);
				}

				protected IWIBase mLastOfferItem;

				static bool IsOfHigherQuality(ArtifactQuality thisQuality, ArtifactQuality otherQuality)
				{
						return ((int)thisQuality) > ((int)otherQuality);
				}

				public void AquireLastOffer()
				{
						if (LastOfferItems == null || string.IsNullOrEmpty(LastOfferType)) {
								return;
						}

						Player.Local.Inventory.InventoryBank.AddBaseCurrencyOfType(LastOfferMade, WICurrencyType.A_Bronze);

						for (int i = 0; i < LastOfferItems.Count; i++) {
								mLastOfferItem = LastOfferItems[i];
								//update which items we've aquired
								switch (LastOfferType) {
										case "UndatedShard":
												ActiveMuseum.UndatedShardAquired++;
												break;

										case "UndatedSmall":
												ActiveMuseum.UndatedSmallAquired++;
												break;

										case "DatedShard":
												if (mLastOfferItem.GetStateOf <ArtifactShard>(out mArtifactStateObject)) {
														mArtifactShardState = (ArtifactShardState)mArtifactStateObject;
														ActiveMuseum.DatedShardsAquired.Add(mArtifactShardState.Age);
												}
												break;

										case "DatedSmall":
												if (mLastOfferItem.GetStateOf <Artifact>(out mArtifactStateObject)) {
														mArtifactState = (ArtifactState)mArtifactStateObject;
														ArtifactQuality currentQuality;
														if (!ActiveMuseum.SmallArtifactsAquired.TryGetValue(mArtifactState.Age, out currentQuality)) {
																//if we haven't alread aquired this age add it now
																ActiveMuseum.SmallArtifactsAquired.Add(mArtifactState.Age, mArtifactState.Quality);
														} else if (IsOfHigherQuality(mArtifactState.Quality, currentQuality)) {
																//set the existing entry to the newer level of quality
																ActiveMuseum.SmallArtifactsAquired[mArtifactState.Age] = mArtifactState.Quality;
														}
												}
												break;

										case "DatedLarge":
												if (mLastOfferItem.GetStateOf <Artifact>(out mArtifactStateObject)) {
														mArtifactState = (ArtifactState)mArtifactStateObject;
														ArtifactQuality currentQuality;
														if (!ActiveMuseum.LargeArtifactsAquired.TryGetValue(mArtifactState.Age, out currentQuality)) {
																//if we haven't alread aquired this age add it now
																ActiveMuseum.LargeArtifactsAquired.Add(mArtifactState.Age, mArtifactState.Quality);
														} else if (IsOfHigherQuality(mArtifactState.Quality, currentQuality)) {
																//set the existing entry to the newer level of quality
																ActiveMuseum.LargeArtifactsAquired[mArtifactState.Age] = mArtifactState.Quality;
														}
												}
												break;

										default:
												Debug.Log("Couldn't find artifact type " + LastOfferType);
												break;
								}
								//destroy it - the curator will disply a doppleganger
								mLastOfferItem.RemoveFromGame();
						}

						Mods.Get.Runtime.SaveMod(ActiveMuseum, "Museum", ActiveMuseum.Name);


						LastOfferItems.Clear();
						UndatedShard.Clear();
						UndatedSmall.Clear();
						DatedShard.Clear();
						DatedSmall.Clear();
						DatedLarge.Clear();
						//this will force the containers to update their newly missing items
						GUI.GUIInventoryInterface.Get.RefreshContainers();

						RefreshCuratedItems();
				}

				public int NumItemsAvailable(string itemType, bool duplicatesOK)
				{
						int numAquired = 0;
						int numAvailable = 0;

						switch (itemType) {
								case "Any":
										numAvailable = UndatedShardAvailable + UndatedSmallAvailable + DatedShardAvailable + DatedSmallAvailable + DatedLargeAvailable;
										break;

								case "Undated":
										numAvailable = UndatedShardAvailable + UndatedSmallAvailable;
										break;

								case "Dated":
										numAvailable = DatedShardAvailable + DatedSmallAvailable + DatedLargeAvailable;
										break;

								case "UndatedShard":
										numAvailable = UndatedShardAvailable;
										numAquired = ActiveMuseum.UndatedShardAquired;
										break;

								case "UndatedSmall":
										numAvailable = UndatedSmallAvailable;
										numAquired = ActiveMuseum.UndatedSmallAquired;
										break;

								case "DatedShard":
										numAvailable = DatedShardAvailable;
										numAquired = ActiveMuseum.DatedShardsAquired.Count;
										break;

								case "DatedSmall":
										numAvailable = DatedSmallAvailable;
										numAquired = ActiveMuseum.SmallArtifactsAquired.Count;
										break;

								case "DatedLarge":
										numAvailable = DatedLargeAvailable;
										numAquired = ActiveMuseum.LargeArtifactsAquired.Count;
										break;

								default:
										Debug.Log("Couldn't find artifact type " + itemType);
										return 0;
						}

						if (!duplicatesOK && numAquired > 0) {
								return 0;
						}
						return numAvailable;
				}
				#if UNITY_EDITOR
				public void DrawEditor()
				{
						UnityEngine.GUI.color = Color.cyan;
						if (GUILayout.Button("\nSave Museum\n")) {
								if (!Manager.IsAwake <Mods>()) {
										Manager.WakeUp <Mods>("__MODS");
								}
								Mods.Get.Editor.InitializeEditor(true);
								Mods.Get.Editor.SaveMod(ActiveMuseum, "Museum", ActiveMuseum.Name);
						}
						if (GUILayout.Button("\nRe-Load Museum\n")) {
								if (!Manager.IsAwake <Mods>()) {
										Manager.WakeUp <Mods>("__MODS");
								}
								Mods.Get.Editor.InitializeEditor(true);
								Mods.Get.Editor.LoadMod(ref ActiveMuseum, "Museum", ActiveMuseum.Name);
						}
				}
				#endif
		}

		[Serializable]
		public class Museum : Mod
		{
				public SDictionary <ArtifactAge, ArtifactQuality> LargeArtifactsAquired = new SDictionary <ArtifactAge, ArtifactQuality>();
				public SDictionary <ArtifactAge, ArtifactQuality> SmallArtifactsAquired = new SDictionary <ArtifactAge, ArtifactQuality>();
				public List <ArtifactAge> DatedShardsAquired = new List<ArtifactAge>();
				public int UndatedSmallAquired = 0;
				public int UndatedShardAquired = 0;
		}
}