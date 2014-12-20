#pragma warning disable 0219
using UnityEngine;
using System;
using Frontiers.Data;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Locations;
using Frontiers.World.Gameplay;
using Frontiers.GUI;

namespace Frontiers.World
{
		public partial class WorldItem
		{
				#region IUnloadableChild implementation

				public bool PrepareToUnload()
				{
						if (!Is(WILoadState.Initialized | WILoadState.PreparingToUnload)) {
								//we shouldn't be prepared to unload unless we're initialized or already preparing
								return false;
						}

						LoadState = WILoadState.PreparingToUnload;
						var enumerator = mScripts.Values.GetEnumerator();
						while (enumerator.MoveNext ()) {
								//foreach (KeyValuePair <Type, WIScript> script in mScripts) {
								if (!enumerator.Current.PrepareToUnload()) {
										return false;
								}
						}
						return true;
				}

				public bool ReadyToUnload {
						get {
								if (!Is(WILoadState.PreparingToUnload)) {
										return false;
								}

								var enumerator = mScripts.Values.GetEnumerator();
								//foreach (WIScript script in mScripts.Values) {
								while (enumerator.MoveNext ()) {
										//we don't need to check them all
										if (!enumerator.Current.ReadyToUnload) {
												return false;
										}
								}
								return true;
						}
				}

				public void BeginUnload()
				{
						if (!Is(WILoadState.PreparingToUnload)) {
								//we don't want to being unload more than once
								//and it has to happen while preparing to unload
								return;
						}
						LoadState = WILoadState.Unloading;
						IEnumerator <WIScript> enumerator = mScripts.Values.GetEnumerator();
						while (enumerator.MoveNext ()) {
						//foreach (WIScript script in mScripts.Values) {
								//we don't need to check them all
								//script.BeginUnload();
								enumerator.Current.BeginUnload();
						}
				}

				public void CancelUnload()
				{
						if (Is(WILoadState.PreparingToUnload)) {
								//we can only cancel unload if we're preparing
								//if we've already started uloading then it's too late
						}

						LoadState = WILoadState.Initialized;//???this isn't guaranteed to be true! TODO
						IEnumerator <WIScript> enumerator = mScripts.Values.GetEnumerator();
						while (enumerator.MoveNext ()) {
								//foreach (WIScript script in mScripts.Values) {
								//we don't need to check them all
								enumerator.Current.CancelUnload();
						}
				}

				public bool FinishedUnloading {
						get {
								bool result = false;
								if (Is(WILoadState.Unloading)) {
										//if we're still unloading, check if we're done
										result = true;
										IEnumerator <WIScript> enumerator = mScripts.Values.GetEnumerator();
										while (enumerator.MoveNext ()) {
												//foreach (WIScript script in mScripts.Values) {
												//we don't need to check them all
												//just until we hit one that isn't done
												if (!enumerator.Current.FinishedUnloading) {
														result = false;
														break;
												}
										}
										if (result) {
												//are all scripts done? good then we're permanently unloaded
												LoadState = WILoadState.Unloaded;
												OnFinishedUnloading();
										}
								} else if (Is(WILoadState.Unloaded)) {
										//if we've already unloaded there's no way back
										//so we don't have to check a second time
										result = true;
								}
								return result;
						}
				}

				public bool SaveItemOnUnloaded {
						get {
								IEnumerator <WIScript> enumerator = mScripts.Values.GetEnumerator();
								while (enumerator.MoveNext ()) {
										//foreach (WIScript script in mScripts.Values) {
										if (!enumerator.Current.SaveItemOnUnloaded) {
												return false;
										}
								}
								return true;
						}
				}

				public int Depth { get { return Group.Depth + 1; } }

				public bool Terminal { get { return true; } }

				public bool HasUnloadingParent { get { return true; } }

				public IUnloadableChild ShallowestUnloadingParent { get { return Group.ShallowestUnloadingParent; } }

				#endregion

				#region IWIBase implementation

				public WorldItem worlditem { get { return this; } }

				public bool IsWorldItem	{ get { return true; } }

				public string PrefabName { get { return Props.Name.PrefabName; } }

				public string PackName { get { return Props.Name.PackName; } }

				public bool HasStates { get { return States != null; } }

				public string State {
						get {
								if (States != null) {
										return States.State;
								}
								return "Default";
						}
						set {
								if (States != null && SaveState != null) {
										if (Is(WILoadState.Initialized | WILoadState.Initializing)) {
												States.State = value;
										} else {
												SaveState.LastState = value;
										}
								}
						}
				}

				public bool CanBe(string stateName)
				{
						if (HasStates) {
								return States.CanBe(stateName);
						}
						return false;
				}

				public virtual string StackName {
						get {
								if (IsTemplate || string.IsNullOrEmpty(Props.Name.StackName)) {
										Props.Name.StackName = WorldItems.CleanWorldItemName(Props.Name.PrefabName);
								}
								if (HasStates) {
										return States.StackName(Props.Name.StackName);
								}
								return Props.Name.StackName;
						}
				}

				public virtual string DisplayName {
						get {
								if (HasStates) {
										return States.DisplayName(WorldItems.WIDisplayName(this));
								}
								return WorldItems.WIDisplayName(this);
						}
				}

				public string FileName { get { return Props.Name.FileName; } }

				public string QuestName { get { return Props.Name.QuestName; } }

				public bool IsQuestItem { get { return Is <QuestItem>(); } }

				public string Subcategory { get { return Props.Local.Subcategory; } }

				public WIMode Mode { get { return Props.Local.Mode; } }

				public int NumItems { get { return StackContainer.NumItems; } }

				public WIStackMode StackMode { get { return StackContainer.Mode; } }

				public WISize Size { get { return Props.Global.Flags.Size; } }

				public WICurrencyType CurrencyType { get { return Props.Global.CurrencyType; } }

				public float BaseCurrencyValue { get { return Mathf.Max(1, Props.Global.BaseCurrencyValue); } }//all items must cost at least 1

				public SVector3	ChunkPosition { get { return Props.Local.ChunkPosition; } set { Props.Local.ChunkPosition = value; } }

				public bool Destroyed { get { return mDestroyed; } }

				public WIFlags Flags {
						get {
								//to prevent tampering
								if (mFlags == null) {
										mFlags = new WIFlags();
										mFlags.CopyFrom(Props.Global.Flags);
								}
								return mFlags;
						}
				}

				public static List <string> DebugNames = new List <string>() { "Bread", "Hammer 2" };

				public void Refresh()
				{
						if (!Is(WILoadState.Initialized | WILoadState.PreparingToUnload | WILoadState.Unloading) || Group == null) {
								return;
						}

						RefreshHud();

						if (mActiveState != mLastActiveState) {
								//we need to refresh our active state
								//check the last state against the current state
								//avoid calling unnecessary visible / actives
								switch (mActiveState) {
										case WIActiveState.Active:
												SetActive(true);
												SetVisible(true);
												break;

										case WIActiveState.Visible:
												SetActive(false);
												SetVisible(true);
												break;

										case WIActiveState.Invisible:
										default:
												SetActive(false);
												SetVisible(false);
												break;
								}
						}
				}

				public void Clear()
				{
						//called before a worlditem is removed from game
				}

				public bool UseRemoveItemSkill(HashSet <string> removeItemSkillNames, ref IStackOwner useTarget)
				{	//first get our local remove item skills
						//this list is populatd by WIScripts
						List <string> localRemoveItemSkills = RemoveItemSkills;
						for (int i = 0; i < localRemoveItemSkills.Count; i++) {
								removeItemSkillNames.Add(localRemoveItemSkills[i]);
						}
						//then get our owner remove item skills
						//this is populated by the owner's WIScripts
						IStackOwner nextUseTarget = null;
						IStackOwner groupOwner = null;
						if (Group.HasOwner(out groupOwner)) {
								nextUseTarget = groupOwner;
								//make sure that we're not the owner of this group to prevent a loop
								//Debug.Log ("Group has owner " + nextUseTarget.DisplayName);
								if (nextUseTarget.IsWorldItem && nextUseTarget.worlditem != this) {
										useTarget = nextUseTarget;
										useTarget.UseRemoveItemSkill(removeItemSkillNames, ref useTarget);
								}
						}
						return removeItemSkillNames.Count > 0;
				}

				public List <string> RemoveItemSkills {
						get {
								HashSet <string> removeItemSkills = new HashSet <string>();
								foreach (WIScript script in mScripts.Values) {
										script.PopulateRemoveItemSkills(removeItemSkills);
								}
								Props.Local.RemoveItemSkills.Clear();
								Props.Local.RemoveItemSkills.AddRange(removeItemSkills);
								return Props.Local.RemoveItemSkills;
						}
				}
				//group actions
				public Action OnRemoveFromStack	{ get; set; }

				public Action OnRemoveFromGroup	{ get; set; }

				public Action OnAddedToPlayerInventory;
				public Action OnAddedToGroup;
				public Action OnGroupLoaded;
				public Action OnGroupUnloaded;
				public Action OnGroupRemovedFromGame;
				public Action OnScriptAdded;
				//active state actions
				public Action OnActive;
				public Action OnInactive;
				public Action OnVisible;
				public Action OnInvisible;
				public Action OnUnloading;
				public Action OnUnloaded;
				public Action OnRemovedFromGame;
				//state and mode actions
				public Action OnStateChange;
				public Action OnModeChange;
				//interaction
				public Action OnPlayerEncounter;
				//public Action OnPlayerInteract;
				public Action OnPlayerUse;
				public Action OnPlayerPlace;
				public Action OnPlayerCarry;
				public Action OnPlayerDrop;
				public Action OnExamine;
				//focus / attention
				public Action OnGainPlayerFocus;
				public Action OnLosePlayerFocus;

				#endregion

				#region IVisible implementation

				//Has and HasAtLeastOne are covererd by script manager
				public ItemOfInterestType IOIType { get { return ItemOfInterestType.WorldItem; } }

				public bool IsVisible { get { return true; } }//should this be linked to visibile state?

				public float AwarenessDistanceMultiplier { get { return 1.0f; } }//should this be added to WIScript?

				public float FieldOfViewMultiplier { get { return 1.0f; } }

				public Vector3 Position { get { return tr.position; } }

				public PlayerBase player { get { return null; } }

				public ActionNode node { get { return null; } }

				public WorldLight worldlight { get { return null; } }

				public Fire fire { get { return null; } }

				public void LookerFailToSee()
				{
						//do nothing
				}

				#endregion

		}
}