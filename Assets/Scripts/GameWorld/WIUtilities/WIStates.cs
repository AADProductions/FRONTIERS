using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using Frontiers.GUI;

namespace Frontiers.World
{
		[ExecuteInEditMode]
		public class WIStates : MonoBehaviour
		{		//used to create worlditems that can change from one state to another
				//originally created for edibles to make the transition from raw to cooked easier
				//then started using it for tents, lights, etc.
				//now i use it everywhere
				//it should really be a wiscript, but it's too late for that now
				public WorldItem worlditem;
				public string DefaultState = "Default";

				public bool CanEnterInventory {
						get {
								if (mCurrentState != null) {
										return mCurrentState.CanEnterInventory;
								}
								return true;
						}
				}

				public bool CanBeCarried {
						get {
								if (mCurrentState != null) {
										return mCurrentState.CanBeCarried;
								}
								return true;
						}
				}

				public bool CanBePlaced {
						get {
								if (mCurrentState != null) {
										return mCurrentState.CanBePlaced;
								}
								return true;
						}
				}

				public bool CanBeDropped {
						get {
								if (mCurrentState != null) {
										return mCurrentState.CanBeDropped;
								}
								return true;
						}
				}

				public bool UnloadWhenStacked {
						get {
								if (mCurrentState != null) {
										return mCurrentState.UnloadWhenStacked;
								}
								return true;
						}
				}

				public string DisplayName(string displayName)
				{
						if (mCurrentState != null) {
								if (!string.IsNullOrEmpty(mCurrentState.Suffix)) {
										displayName = displayName + " (" + mCurrentState.Suffix + ")";
								}
						}
						return displayName;
				}

				public string StackName(string stackName)
				{
						if (mCurrentState != null) {
								if (!string.IsNullOrEmpty(mCurrentState.StackName)) {
										return mCurrentState.StackName;
								}
						}
						return stackName;
				}

				public static string StackName(string stackName, string lastStateName)
				{
						return stackName + " (" + lastStateName + ")";
				}

				public List <WIState> States = new List <WIState>();

				public void InitializeTemplate()
				{
						worlditem = gameObject.GetComponent <WorldItem>();
						worlditem.States = this;

						worlditem.Renderers.Clear();
						worlditem.Colliders.Clear();

						mCurrentState = null;
						mDefaultState = null;
						//clear the current state object
						if (DefaultState == "Default") {
								DefaultState = string.Empty;
						}

						for (int i = 0; i < States.Count; i++) {
								WIState state = States[i];
								//set color and intensity settings
								if (state.UseRendererColor && !string.IsNullOrEmpty(state.RendererColorName)) {
										state.RendererColor = Colors.Get.ByName(state.RendererColorName);
								} else {
										state.RendererColor = Color.white;
								}

								Transform stateObjectTransform = transform.FindChild(state.Name);
								if (stateObjectTransform != null) {
										state.StateObject = stateObjectTransform.gameObject;
										state.StateRenderer = state.StateObject.renderer;
										state.StateCollider = state.StateObject.collider;

										if (mDefaultState == null) {
												if (string.IsNullOrEmpty(DefaultState)) {
														if (state.StateObject.activeSelf) {
																DefaultState = state.Name;
																mCurrentState = state;
																mDefaultState = mCurrentState;
														}
												} else if (state.Name == DefaultState) {
														mCurrentState = state;
														mDefaultState = mCurrentState;
												}
										}

								}
						}

						if (mDefaultState == null) {
								if (States.Count > 0) {
										mDefaultState = States[0];
										DefaultState = mDefaultState.Name;
										mCurrentState = mDefaultState;
								} else {
										return;
								}
						}

						//add the default state renderer and collider so the worlditem can calculate its base object bounds
						if (mDefaultState.StateRenderer != null) {
								worlditem.Renderers.Add(mDefaultState.StateRenderer);
						}
						if (mDefaultState.StateCollider != null) {
								worlditem.Colliders.Add(mDefaultState.StateCollider);
						}
				}

				public string State {
						get {
								if (mCurrentState != null) {
										return mCurrentState.Name;
								}
								return DefaultState;
						}
						set {
								SetState(value);
						}
				}

				public void PopulateOptionsList(List <GUIListOption> options, List <string> message)
				{
						if (mCurrentState == null || !mCurrentState.IsInteractive) {
								return;
						}

						for (int i = 0; i < States.Count; i++) {
								WIState state = States[i];
								if (!string.IsNullOrEmpty(state.OptionListDisplay)) {
										GUIListOption option = new GUIListOption(state.OptionListDisplay, "SetWIState" + state.Name);
										if (mCurrentState.Name == state.Name) {
												option.Disabled = true;
										}
										options.Add(option);
								}
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{	//this is where we handle skills
						OptionsListDialogResult dialogResult = secondaryResult as OptionsListDialogResult;
						if (dialogResult.SecondaryResult.Contains("SetWIState")) {
								string stateName = dialogResult.SecondaryResult.Replace("SetWIState", "");
								SetState(stateName);
						}
				}

				public void OnInitialized()
				{
						string startupState = DefaultState;
						if (worlditem.HasSaveState && !string.IsNullOrEmpty(worlditem.SaveState.LastState)) {
								startupState = worlditem.SaveState.LastState;
						}
						SetState(startupState);
						worlditem.OnModeChange += OnModeChange;
				}

				public void OnModeChange()
				{
						RefreshState();
				}

				public void RefreshState()
				{
						if (mCurrentState == null) {
								SetState(DefaultState);
						}
				}

				public WIState CurrentState {
						get {
								if (mCurrentState == null) {
										SetState(DefaultState);
								}
								return mCurrentState;
						}
				}

				public bool CanBe(string stateName)
				{
						if (States.Count == 0) {
								return false;
						}

						if (CurrentState.IsPermanent && CurrentState.Name != stateName) {
								//if the state is permanent then we can't be that thing
								return false;
						}
						//otherwise check each state and see if the state's available
						for (int i = 0; i < States.Count; i++) {
								if (States[i].Name == stateName) {
										return true;
										break;
								}
						}
						return false;
				}

				public WIState GetState(string stateName)
				{
						for (int i = 0; i < States.Count; i++) {
								if (States[i].Name == stateName) {
										return States[i];
								}
						}
						return null;
				}

				protected bool SetState(string stateName)
				{
						if (mInTransition) {
								return false;
						}

						if (stateName == "Default") {
								stateName = DefaultState;
						}

						if (mCurrentState != null && (mCurrentState.Name == stateName || mCurrentState.IsPermanent)) {
								//no need to do anything
								return false;
						}

						WIState newState = null;
						WIState oldState = mCurrentState;
						for (int i = 0; i < States.Count; i++) {
								if (!States[i].Enabled) {
										//can't set to a state that isn't enabled!
										continue;
								}

								if (States[i].Name == stateName) {
										newState = States[i];
								} else {
										if (States[i].StateObject != null) {
												//disable all but the new state object
												States[i].StateObject.SetActive(false);
										}
								}
						}

						if (newState == null) {
								if (mHasSetInitialState) {
										return false;
								} else {
										//continue, because we have to set initial state at least once
										newState = mCurrentState;
										mHasSetInitialState = true;
								}
						}

						if (newState != null) {
								//make sure we have a state object and renderer
								//if we do set it active
								if (newState.StateObject != null) {
										newState.StateObject.SetActive(true);
								} else {
										foreach (Transform child in transform) {
												if ((child.CompareTag("StateChild") || child.CompareTag("WorldItem")) && child.name == newState.Name) {
														newState.StateObject = child.gameObject;
														newState.StateRenderer = child.renderer;
														newState.StateCollider = child.collider;
														newState.StateObject.SetActive(true);
														break;
												}
												//the rest have already been set to false
										}
								}

								//clone the serialized properties
								mInTransition = true;
								mCurrentState = newState;
								OnTransitionStart(oldState, newState, mCurrentState);
								StartCoroutine(SetStateOverTime(oldState, newState, WorldClock.Time, WorldClock.RTSecondsToGameSeconds(newState.TransitionRTDuration)));
						} else {
								if (mCurrentState == null) {
										return false;
								}
						}

						if (mCurrentState.IsInteractive && mCurrentState.StateObject != null) {
								mCurrentState.StateObject.layer = Globals.LayerNumWorldItemActive;
								mCurrentState.StateObject.tag = "StateChild";
						} else if (mCurrentState.StateObject != null) {
								//we'll still see it and collider with it
								//but we won't show up as a world item
								mCurrentState.StateObject.layer = Globals.LayerNumSolidTerrain;
								mCurrentState.StateObject.tag = "WorldItem";
						}
						worlditem.OnStateChange.SafeInvoke();
						return true;
				}

				protected WIState mCurrentState = null;
				protected WIState mDefaultState = null;
				protected float mTransitionStartTime = 0f;
				protected float mTransitionEndTime = 0f;
				protected bool mInTransition = false;
				protected bool mHasSetInitialState = false;

				protected IEnumerator SetStateOverTime(WIState oldState, WIState newState, double transitionStartTime, double transitionDuration)
				{
						if (!Application.isPlaying) {
								yield break;
						}

						double transitionEndTime = transitionStartTime + transitionDuration;
						float normalizedTransition = 0f;

						//get the world light if we're using one
						if (!newState.UseLight) {
								if (worlditem.Light != null) {
										//whoops, better get rid of this one
										LightManager.DeactivateWorldLight(worlditem.Light);
								}
						} else {
								//if we are using one
								Transform lightParent = worlditem.tr;
								if (worlditem.Is(WIMode.Equipped | WIMode.Stacked)) {
										lightParent = Player.Local.Tool.ToolDoppleganger.transform;
								}

								WorldLightType wlType = WorldLightType.Exterior;
								if (worlditem.Group != null) {
										if (worlditem.Group.Props.Interior || worlditem.Group.Props.TerrainType == LocationTerrainType.BelowGround) {
												wlType = WorldLightType.InteriorOrUnderground;
										} else if (worlditem.Is(WIMode.Equipped)) {
												wlType = WorldLightType.Equipped;
										}
								}
								worlditem.Light = LightManager.GetWorldLight(worlditem.Light, newState.LightTemplateName, lightParent, newState.LightOffset, newState.LightRotation, true, wlType);
						}

						while (WorldClock.Time < transitionEndTime) {
								normalizedTransition = (float)((WorldClock.Time - transitionStartTime) / (transitionEndTime - transitionStartTime));
								BlendState(oldState, newState, mCurrentState, normalizedTransition);
								yield return null;
						}
						//finish transition
						BlendState(oldState, newState, mCurrentState, 1.0f);
						OnTransitionFinish(oldState, newState, mCurrentState);
						yield break;
				}

				protected void OnTransitionStart(WIState oldState, WIState newState, WIState currentState)
				{
						if (oldState != null) {
								if (oldState.FXObject != null) {
										GameObject.Destroy(oldState.FXObject, 1.0f);
								}
						}

						if (!string.IsNullOrEmpty(newState.FXOnChange)) {
								newState.FXObject = FXManager.Get.SpawnFX(worlditem, newState.FXOnChange);
								newState.FXObject.transform.localPosition = newState.FXOffset;
						}
						if (newState.SoundType != MasterAudio.SoundType.None) {
								MasterAudio.PlaySound(newState.SoundType, newState.StateObject.transform, newState.SoundOnChange);
						}
						if (!string.IsNullOrEmpty(newState.AnimationOnChange)) {
								if (newState.StateObject.animation != null) {
										newState.StateObject.animation.Play(newState.AnimationOnChange);
								}
						}
						if (newState.EarthQuakeOnChange > 0f) {
								Player.Local.DoEarthquake(newState.EarthQuakeOnChange);
						}
				}

				protected void BlendState(WIState oldState, WIState newState, WIState currentState, float normalizedBlend)
				{

				}

				protected void OnTransitionFinish(WIState oldState, WIState newState, WIState currentState)
				{
						mInTransition = false;
				}

				public void EditorCreateStates()
				{
						WorldItem worlditem = gameObject.GetComponent <WorldItem>();
						worlditem.Colliders.Clear();
						worlditem.Renderers.Clear();
						worlditem.Props.Global.ParentColliderType = WIColliderType.None;

						States.Clear();
						foreach (Transform child in transform) {
								child.gameObject.tag = "StateChild";
								WIState newState = new WIState();
								newState.Name = child.name;
								newState.Suffix = child.name;
								BoxCollider bc = child.gameObject.GetOrAdd <BoxCollider>();
								bc.enabled = false;

								switch (newState.Name) {
										case "Rotten":
										case "Burned":
										case "Preserved":
												newState.IsPermanent = true;
												child.gameObject.SetActive(false);
												break;

										case "Raw":
												child.gameObject.SetActive(true);
												break;

										default:
												child.gameObject.SetActive(false);
												break;
								}

								States.Add(newState);
						}
				}
		}

		[Serializable]
		public class WIState
		{
				//basic props
				public string Name = "Default";
				public bool Enabled = true;
				public string Suffix = string.Empty;
				public string StackName	= string.Empty;
				public string OptionListDisplay = string.Empty;
				public bool IsPermanent = false;
				public bool CanEnterInventory = true;
				public bool CanBeCarried = true;
				public bool CanBePlaced = true;
				public bool CanBeDropped = true;
				public bool UnloadWhenStacked = true;
				public string AnimationOnChange = string.Empty;
				public float EarthQuakeOnChange = 0f;
				//if this is false, the layer is set to scenery
				public bool IsInteractive = true;
				//state aesthetics
				public float TransitionRTDuration = 0f;

				public bool UseLight {
						get {
								return !string.IsNullOrEmpty(LightTemplateName);
						}
				}

				[FrontiersAvailableModsAttribute("Light")]
				public string LightTemplateName = string.Empty;
				public SVector3 LightOffset = SVector3.zero;
				public SVector3 LightRotation = SVector3.zero;
				public bool UseRendererColor = false;
				public string RendererColorName = string.Empty;
				public string FXOnChange = string.Empty;
				public string SoundOnChange = string.Empty;
				public SVector3 FXOffset = SVector3.zero;
				public MasterAudio.SoundType SoundType = MasterAudio.SoundType.None;
				//these are loaded at runtime
				[XmlIgnore]
				public GameObject StateObject = null;
				[XmlIgnore]
				public Renderer StateRenderer = null;
				[XmlIgnore]
				public Collider StateCollider = null;
				[XmlIgnore]
				public Color LightColor = Color.white;
				[XmlIgnore]
				public Color RendererColor = Color.white;
				[XmlIgnore]
				public GameObject FXObject = null;
		}
}