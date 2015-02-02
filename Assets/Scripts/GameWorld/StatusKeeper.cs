using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Frontiers.World.Gameplay;
using System.Text;

namespace Frontiers
{
		[Serializable]
		public class StatusKeeper : Mod, IComparable <StatusKeeper>
		{
				public StatusKeeper()
				{
						mInitialized = false;
				}

				public int CompareTo(StatusKeeper o)
				{
						if (o.DisplayOrder < DisplayOrder) {
								return 1;
						} else if (o.DisplayOrder > DisplayOrder) {
								return -1;
						}
						return 0;
				}

				public string ValueDescriptionGreaterThanFull;
				public string ValueDescriptionFull;
				public string ValueDescriptionFourFifths;
				public string ValueDescriptionThreeFifths;
				public string ValueDescriptionTwoFifths;
				public string ValueDescriptionOneFifth;
				public string ValueDescriptionEmpty;
				public string ValueDescriptionLessThanEmpty;
				public bool ManualChangeOnly = false;
				public bool ShowInStatusInterface = false;
				public bool ShowInConversationInterface = false;
				public bool ShowInBarterInterface = false;
				public bool ShowOnlyWhenAffectedByCondition = false;
				public string PlayerVariable = string.Empty;
				public int PlayerVariableMaxValue = 100;
				public string IconName = "StatusKeeperIcon";
				public string AtlasName = "Default";
				//used by GUI, stored in state Color vars
				[XmlIgnore]
				[HideInInspector]
				public Color HighColorValue = Color.white;
				[XmlIgnore]
				[HideInInspector]
				public Color LowColorValue = Color.white;
				[XmlIgnore]
				[HideInInspector]
				public Color MidColorValue = Color.white;
				[XmlIgnore]
				[HideInInspector]
				public Color KeeperColor = Color.white;
				//Value is the raw unclamped value of the seeker
				//condition modifiers are applied directly to Value
				public float Value = 0f;
				public StatusKeeperState DefaultState = new StatusKeeperState();
				public List <StatusKeeperState> AlternateStates = new List <StatusKeeperState>();
				[XmlIgnore]//ignore this because the player state will keep a set of active conditions without duplicates 
				[HideInInspector]
				public List <Condition> Conditions = new List <Condition>();
				[XmlIgnore]//ignore this because they'll be sent again anyway and we want to preserve references
				[HideInInspector]
				public List <StatusFlow> StatusFlows = new List <StatusFlow>();
				//AdjustedValue value is the raw value with last update's overflow / underflow applied, then clamped to the -1 - 2 range
				//Normalized value clamps AdjustedValue to make it usable with GUI / display
				public bool Initialized { get { return mInitialized && mActiveState != null; } }

				public float AdjustedValue { get { return Mathf.Clamp(mValueWithFlow, -1f, 2f); } }

				public float NormalizedValue { get { return Mathf.Clamp01(AdjustedValue); } }

				public float NormalizedUrgency { get { return Mathf.Clamp01(mNormalizedUrgency); } }

				public StatusKeeperState ActiveState { get { return mActiveState; } }

				public float ChangeLastUpdate { get { return mValueWithFlow - mValueWithFlowLastUpdate; } }

				[XmlIgnore]//purely aesthetic
				[HideInInspector]
				public bool Ping = false;

				public string CurrentDescription {
						get {
								BuildDescription();
								return mCurrentDescription;
						}
				}

				protected void BuildDescription()
				{
						StringBuilder sb = new StringBuilder();
						Color currentColor = Colors.Alpha(Colors.BlendThree(LowColorValue, MidColorValue, HighColorValue, NormalizedValue), 1f);
						//static description
						sb.Append(Colors.ColorWrap(Name, KeeperColor));
						sb.Append(Colors.ColorWrap(": ", KeeperColor));
						sb.Append(Colors.ColorWrap(Description.Trim(), KeeperColor));
						sb.Append("\n_\n");
						//current value description
						bool overFlow = false;
						bool underFlow = false;
						if (mValueWithFlow > 1) {
								overFlow = true;
								sb.Append(Colors.ColorWrap(ValueDescriptionGreaterThanFull.Trim(), currentColor));
						} else if (mValueWithFlow > 0.95) {
								sb.Append(Colors.ColorWrap(ValueDescriptionFull.Trim(), currentColor));
						} else if (mValueWithFlow > 0.8) {
								sb.Append(Colors.ColorWrap(ValueDescriptionFourFifths.Trim(), currentColor));
						} else if (mValueWithFlow > 0.6) {
								sb.Append(Colors.ColorWrap(ValueDescriptionThreeFifths.Trim(), currentColor));
						} else if (mValueWithFlow > 0.4) {
								sb.Append(Colors.ColorWrap(ValueDescriptionTwoFifths.Trim(), currentColor));
						} else if (mValueWithFlow > 0.2) {
								sb.Append(Colors.ColorWrap(ValueDescriptionOneFifth.Trim(), currentColor));
						} else if (mValueWithFlow > 0.01) {
								sb.Append(Colors.ColorWrap(ValueDescriptionEmpty.Trim(), currentColor));
						} else {
								underFlow = true;
								sb.Append(Colors.ColorWrap(ValueDescriptionLessThanEmpty, currentColor));
						}
						//flows - how this affects other keepers
						//and how we are affected by other keepers
						if (overFlow
						 && !string.IsNullOrEmpty(mActiveState.Overflow.TargetName)
						 && mActiveState.Overflow.HasEffect
						 && !string.IsNullOrEmpty(mActiveState.Overflow.Description.Trim())) {
								sb.Append("\n");
								sb.Append(Colors.ColorWrap(mActiveState.Overflow.Description.Trim(), HighColorValue));
						} else if (underFlow
						        && !string.IsNullOrEmpty(mActiveState.Underflow.TargetName)
						        && mActiveState.Underflow.HasEffect
						        && !string.IsNullOrEmpty(mActiveState.Underflow.Description.Trim())) {
								sb.Append("\n");
								sb.Append(Colors.ColorWrap(mActiveState.Underflow.Description.Trim(), LowColorValue));
						}

						if (StatusFlows.Count > 0) {
								sb.Append("\n");
								for (int i = 0; i < StatusFlows.Count; i++) {
										StatusFlow sf = StatusFlows[i];
										if (sf.HasEffect && !string.IsNullOrEmpty(sf.Description.Trim())) {
												if (sf.FlowType == StatusSeekType.Negative) {
														sb.Append(Colors.ColorWrap(sf.Description.Trim(), LowColorValue));
												} else {
														sb.Append(Colors.ColorWrap(sf.Description.Trim(), HighColorValue));
												}
										}
								}
						}
						//current state description
						if (!string.IsNullOrEmpty(mActiveState.StateDescription)) {
								sb.Append("\n_\n");
								sb.Append(mActiveState.StateDescription.Trim());
						}

						mCurrentDescription = sb.ToString();
						sb.Clear();
						sb = null;
				}

				protected string mCurrentDescription = string.Empty;

				public StatusSeekType LastChangeType {
						get {
								StatusSeekType changeType = StatusSeekType.Neutral;
								if (ChangeLastUpdate < 0f) {
										if (ActiveState.NegativeChange == StatusSeekType.Negative) {
												changeType = StatusSeekType.Negative;
										}
								} else {
										if (ActiveState.PositiveChange == StatusSeekType.Positive) {
												changeType = StatusSeekType.Positive;
										}
								}
								return changeType;
						}
				}

				public float Overflow { //for values > 1, up to 2
						get {
								if (Value > 1f) {
										return Mathf.Clamp01(Value - 1f);
								}
								return 0f;
						}
				}

				public float Underflow { //for values < 1, up to -1
						get {
								if (Value < 0f) {
										return Mathf.Clamp01(Mathf.Abs(Value));
								}
								return 0f;
						} 
				}
				//called when ChangeLastUpdate exceeds ChangeThreshold
				[XmlIgnore]//ignore this when serializing, player status is only one that uses it
				public Action OnChangeAction = null;
				public float OnChangeThreshold = 0.01f;

				public void SetValue(float newValue, bool broadcast)
				{
						if (!mInitialized) {
								Initialize();
						}

						Value = newValue;
						if (broadcast) {
								mValueWithFlowLastUpdate = mValueWithFlow;
								mValueWithFlow = newValue;
								mNormalizedValue = newValue;
						} else {
								mValueWithFlowLastUpdate = newValue;
								mNormalizedValue = newValue;
								mNormalizedValueLastBroadcast = newValue;
						}
				}

				public void Reset()
				{
						StatusFlows.Clear();
						Conditions.Clear();
						mActiveState = DefaultState;
						Value = mActiveState.DefaultValue;
						mValueWithFlow = Value;
						mValueWithFlowLastUpdate = Value;
						mNormalizedValue = NormalizedValue;
						mNormalizedValueLastBroadcast = mNormalizedValue;
						mNormalizedUrgency = 0f;
				}

				public void Initialize()
				{
						if (mInitialized)
								return;
						//clear status flows but keep conditions
						//status flows are sent every update; conditions are only sent once
						StatusFlows.Clear();
						mActiveState = DefaultState;
						//Value = mActiveState.DefaultValue;
						mValueWithFlow = Value;
						mValueWithFlowLastUpdate = Value;
						mNormalizedValue = NormalizedValue;
						mNormalizedValueLastBroadcast = mNormalizedValue;
						mNormalizedUrgency = 0f;

						mStateLookup = new Dictionary <string, StatusKeeperState>();
						mStateLookup.Add("Default", DefaultState);
						foreach (StatusKeeperState state in AlternateStates) {	//set icons & stuff on overflow/underflow
								mStateLookup.Add(state.StateName, state);
						}
						mInitialized = true;
						//refresh default state
						RefreshFlows();
						GetColors();

				}
				//when a status keeper's target is above or below zero
				//and its value rises or drops above or below zero
				//the overflow and underflow can be sent to another status keeper
				//eg hunger overflow will be sent to strength so eating more will boost strength
				//hunger underflow will be sent to health so starving will reduce health
				public void UpdateState(double deltaTime)
				{
						if (!mInitialized)
								return;

						if (!string.IsNullOrEmpty(PlayerVariable)) {
								Value = Player.NormalizePlayerVariable(Player.GetPlayerVariable(PlayerVariable), PlayerVariableMaxValue);
						} else {
								switch (mActiveState.SeekType) {
										case StatusSeekType.Positive:
										default:
												Value = Mathf.Lerp(Value, mActiveState.SeekValue, (float)(mActiveState.SeekSpeed * deltaTime * Globals.StatusKeeperPositiveChangeMultiplier));
												break;

										case StatusSeekType.Negative:
												Value = Mathf.Lerp(Value, mActiveState.SeekValue, (float)(mActiveState.SeekSpeed * deltaTime * Globals.StatusKeeperNegativeChangeMultiplier));
												break;

										case StatusSeekType.Neutral:
												Value = Mathf.Lerp(Value, mActiveState.SeekValue, Mathf.Clamp01((float)(mActiveState.SeekSpeed * deltaTime * Globals.StatusKeeperNeutralChangeMultiplier)));
												break;
								}
						}

						if (!ManualChangeOnly) {
								//TODO this may not be necessary, keeping it for now
								mActiveState.Overflow.Disabled = (Value <= 1f) ? true : false;//not over one, disable
								mActiveState.Underflow.Disabled = (Value >= 0f) ? true : false;//not under zero, disable
								mActiveState.Overflow.FlowLastUpdate = Overflow;
								mActiveState.Underflow.FlowLastUpdate = Underflow;
						}

						//if we're over or under the status change threshold send a message
						mNormalizedValue = NormalizedValue;
						//now update how urgent we are
						//how urgent we are depends on how far away we are from our ideal value
						if (mActiveState.UseNeutralUrgency) {
								//urgency is determined by how far away we are positive or negative from 0.5
								if (mNormalizedValue < 0.5f) {
										float checkValue = mNormalizedValue * 2;
										mNormalizedUrgency = 1f - mNormalizedValue;
								} else {
										float checkValue = (mNormalizedValue - 0.5f) * 2;
										mNormalizedUrgency = mNormalizedValue - 1f;
								}
						} else {
								switch (mActiveState.PositiveChange) {
										case StatusSeekType.Positive:
										default:
												//urgency is determined by how far away we are from 1
												mNormalizedUrgency = 1f - mNormalizedValue;
												break;

										case StatusSeekType.Neutral:
												mNormalizedUrgency = 0f;
												break;

										case StatusSeekType.Negative:
												mNormalizedUrgency = mNormalizedValue;
												break;
								}
						}

						float normalizedChange = mNormalizedValue - mNormalizedValueLastBroadcast;
						if (Mathf.Abs(normalizedChange) >= mMinimumNormalizeChange) {
								if (!string.IsNullOrEmpty(PlayerVariable)) {
										//auto ping player variable changes
										Ping = true;
								}
								//reset the change tracker
								mNormalizedValueLastBroadcast = NormalizedValue;
								if (normalizedChange > 0) {
										//broadcast a status gained message
										Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalRestoreStatus, WorldClock.AdjustedRealTime);
								} else {
										//broadcast a status reduced message
										Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalLoseStatus, WorldClock.AdjustedRealTime);
								}
						}
				}
				//this is applying status flows to this keeper, not to other keepers
				//also where the keeper determines if a status flow is still targeting this keeper
				public void ApplyStatusFlows(double deltaTime)
				{
						if (!mInitialized)
								return;
						//store last update's value so we know how much we've changed
						mValueWithFlowLastUpdate = mValueWithFlow;
						//then reset mValueWithFlow to the current raw value and apply all flows to mValueWithFlow
						mValueWithFlow = Value;
						for (int i = StatusFlows.Count - 1; i >= 0; i--) {
								if (StatusFlows[i] == null || StatusFlows[i].TargetName != Name || StatusFlows[i].StateName != mActiveState.StateName) {
										if (StatusFlows[i] != null)
												StatusFlows[i].Disabled = true;
										StatusFlows.RemoveAt(i);
								} else if (StatusFlows[i].HasEffect) {	//if it has no effect it's disabled or shows no change
										mValueWithFlow = StatusFlows[i].ApplyTo(this, mValueWithFlow);
								}
						}
				}
				//this is where we apply the effects of conditions
				//also where we see if conditions have expired
				public void ApplyConditions(double deltaTime, List <AvatarAction> subscribedActions, List <Condition> activeConditions, List <string> activeStates)
				{
						if (!mInitialized)
								return;

						for (int i = Conditions.Count - 1; i >= 0; i--) {
								if (Conditions[i] == null || Conditions[i].CheckExpired(deltaTime, subscribedActions, activeConditions, activeStates)) {
										Conditions.RemoveAt(i);
										//remove any symptoms
								} else {
										Value = Conditions[i].ApplyTo(this, deltaTime);
								}
						}
				}
				//these are called once by PlayerStatus after SetState
				//the overflow and underflow are sent to the targeted StatusKeepers
				//StatusKeepers check to see if they're still the target of over/underflow
				//so there's no need to keep track of when it expires
				public bool HasUnderflowToSend(out StatusFlow underflow)
				{
						if (!mInitialized) {
								underflow = null;
								return false;
						}

						underflow = mActiveState.Underflow;
						return !string.IsNullOrEmpty(underflow.TargetName);
				}

				public bool HasOverflowToSend(out StatusFlow overflow)
				{
						if (!mInitialized) {
								overflow = null;
								return false;
						}

						overflow = mActiveState.Overflow;
						return !string.IsNullOrEmpty(overflow.TargetName);
				}
				//conditions will automatically be stacked with any existing conditions by the status manager
				//so we're safe to just add to our conditions list upon receiving
				public void ReceiveCondition(Condition newCondition)
				{	//TODO make sure this is all we actually need
						Conditions.Add(newCondition);
				}
				//these will automatically filter out duplicates
				//will usually be called after HasUnder/OverflowToSend by the status manager
				//StatusFlows aren't categorized by under/over once they're in the status keeper
				//all that matters to the keeper & the GUI is the effect that it has on the value
				public void ReceiveFlows(List <StatusFlow> newFlows)
				{
						if (!mInitialized)
								return;

						foreach (StatusFlow newFlow in newFlows) {
								if (newFlow != null) {
										bool replacedExisting = false;
										for (int i = StatusFlows.Count - 1; i >= 0; i--) {
												if (StatusFlows[i] != null && StatusFlows[i] == newFlow) {//replace it outright
														StatusFlows[i].Disabled = true;//disable the other flow //TODO make sure this doesn't break anything
														StatusFlows[i] = newFlow;
														replacedExisting = true;
												}
										}
										if (!replacedExisting) {	//otherwise just add it normally
												StatusFlows.Add(newFlow);
										}
								}
						}
				}

				public void SetState(List <string> states)
				{
						if (!mInitialized)
								return;
						//go through the states in order and activate them in order
						//skip any that we don't have
						StatusKeeperState newActiveState = null;
						StatusKeeperState nextAttemptedState = null;
						bool activatedState = false;
						for (int i = 0; i < states.Count; i++) {	//set true if we found any state
								//go through all of them - later states will override earlier states
								if (mStateLookup.TryGetValue(states[i], out nextAttemptedState)) {
										if (nextAttemptedState != mActiveState) {
												activatedState = true;
												newActiveState = nextAttemptedState;
										}
								}
						}
						//if we found one
						if (activatedState) {//set it to the current active state
								mActiveState.Deactivate();
								mActiveState = newActiveState;
								//update colors
								GetColors();
								RefreshFlows();
						}
				}

				public void ChangeValue(float amount, StatusSeekType changeType)
				{
						if (!mInitialized)
								return;
						//automatically applies multipliers
						SetValue(Value + GetSeekValue(mActiveState.SeekType, changeType, amount), true);
				}

				protected void RefreshFlows()
				{
						if (!mInitialized)
								return;

						mActiveState.Overflow.SenderName = Name;
						mActiveState.Underflow.SenderName = Name;
						mActiveState.Overflow.StateName = mActiveState.StateName;
						mActiveState.Underflow.StateName = mActiveState.StateName;

						//if (!string.IsNullOrEmpty (mActiveState.Underflow.IconName))
						mActiveState.Underflow.IconName = IconName;
						//if (!string.IsNullOrEmpty (mActiveState.Underflow.AtlasName))
						mActiveState.Underflow.AtlasName = AtlasName;

						//if (!string.IsNullOrEmpty (mActiveState.Overflow.IconName))
						mActiveState.Overflow.IconName = IconName;
						//if (!string.IsNullOrEmpty (mActiveState.Overflow.AtlasName))
						mActiveState.Overflow.AtlasName = AtlasName;
				}

				protected void GetColors()
				{
						if (!mInitialized)
								return;

						KeeperColor = Colors.Get.ByName("StatusKeeper" + Name);

						if (!string.IsNullOrEmpty(mActiveState.HighColorName))
								HighColorValue = Colors.Get.ByName(mActiveState.HighColorName);
						else
								HighColorValue = Colors.Get.ByName(DefaultState.HighColorName);

						if (!string.IsNullOrEmpty(mActiveState.MidColorName))
								MidColorValue = Colors.Get.ByName(mActiveState.MidColorName);
						else
								MidColorValue = Colors.Get.ByName(DefaultState.MidColorName);

						if (!string.IsNullOrEmpty(mActiveState.LowColorName))
								LowColorValue = Colors.Get.ByName(mActiveState.LowColorName);
						else
								LowColorValue = Colors.Get.ByName(DefaultState.LowColorName);
				}

				public static float GetSeekValue(StatusSeekType originType, StatusSeekType appliedType, float seekValue)
				{
						//TODO implement inverting for mismatched seek types
						seekValue = Mathf.Abs(seekValue);
						switch (appliedType) {
								case StatusSeekType.Positive:
								default:
										seekValue = (seekValue * Globals.StatusKeeperPositiveChangeMultiplier);
										break;

								case StatusSeekType.Neutral:
										break;

								case StatusSeekType.Negative:
										seekValue = (-seekValue * Globals.StatusKeeperNegativeChangeMultiplier);
										break;
						}
						return seekValue;
				}

				protected bool mInitialized = false;
				protected float mValueWithFlow = 0f;
				protected float mNormalizedUrgency = 0f;
				protected float mValueWithFlowLastUpdate = 0f;
				protected float mNormalizedValue = 0f;
				protected float mNormalizedValueLastBroadcast = 0f;
				protected static float mMinimumNormalizeChange = 0.05f;
				protected StatusKeeperState mActiveState = null;
				protected Dictionary <string, StatusKeeperState> mStateLookup = null;
		}

		[Serializable]
		public class StatusKeeperState
		{
				public StatusKeeperState()
				{
				}

				public string StateDescription;
				public string StateName = "Default";
				//an additional icon that appears above like a condition icon, usually empty
				//color is white by default
				public string StateIconName = string.Empty;
				public string StateIconAtlas = string.Empty;
				public string StateIconColor = string.Empty;
				//used in conjunction with Colors
				public string HighColorName = "GenericHighValue";
				public string MidColorName = "GenericMidValue";
				public string LowColorName = "GenericLowValue";
				//public bool MutedColor = false;
				//default value is kept here so 'restore to default' can have different effects
				//eg 'Badly Wounded' might only restore you to half-health
				public float DefaultValue = 1.0f;
				public float SeekValue = 1.0f;
				public float SeekSpeed = 1.0f;
				public bool UseNeutralUrgency = false;
				//these define how conditions affect the state
				//they also define what colors are used when seeking
				public StatusSeekType SeekType = StatusSeekType.Positive;
				public StatusSeekType PositiveChange = StatusSeekType.Positive;
				public StatusSeekType NegativeChange = StatusSeekType.Negative;
				public StatusFlow Overflow = new StatusFlow();
				public StatusFlow Underflow = new StatusFlow();

				public void Deactivate()
				{
						Overflow.Disabled = true;
						Underflow.Disabled = true;
				}
		}

		[Serializable]
		public class StatusFlow
		{
				public StatusFlow()
				{
				}

				public string TargetName = "StatusKeeper";
				public string SenderName = "StatusKeeper";
				public string IconName = "StatusKeeperIcon";
				public string StateName = "Default";
				public string AtlasName = "Default";
				public string ColorName = string.Empty;
				public string Description;
				//the value of flow is determined by the type + the target type
				//if a status keeper views negative flow as positive type
				//and overflow is intended to be negative
				//setting the flow to negative will invert the flow to be positive
				public float FlowLastUpdate = 0f;
				//Disabled is set by the sender when flow is no longer outside of normal value range
				//this keeps the flow in the status keeper's list until the state is changed
				//but will make it invisible during GUI and ignored on apply flows
				public bool Disabled = false;

				public bool HasEffect {
						get {
								return !Disabled;//TODO tie this to value changes
								//return !(Mathf.Approximately (FlowLastUpdate, 0f) || Disabled);
						}
				}

				public StatusSeekType FlowType = StatusSeekType.Positive;
				//flow can be blunted by a flow multiplier
				public float FlowMultiplier = 1.0f;
				//no need for time variables because we're applying the value from the last update
				public float ApplyTo(StatusKeeper keeper, float valueWithFlow)
				{
						//get the actual flow last update
						//first align it with the target keeper's type
						//then apply multipliers
						float flowLastUpdate = StatusKeeper.GetSeekValue(keeper.ActiveState.SeekType, FlowType, FlowLastUpdate);
						flowLastUpdate = flowLastUpdate *= FlowMultiplier;
						switch (FlowType) {
								case StatusSeekType.Positive:
								default:
										flowLastUpdate *= Globals.StatusKeeperPositiveFlowMultiplier;
										break;

								case StatusSeekType.Neutral:
								//don't adjust flow
										break;

								case StatusSeekType.Negative:
										flowLastUpdate *= Globals.StatusKeeperNegativeFlowMultiplier;
										break;
						}
						//now apply it to the keeper's value
						//apply to the actual value not the normalized value
						//yes this can result in crazy flow stuff but that's OK
						return valueWithFlow + flowLastUpdate;
				}
		}

		[Serializable]
		public class Condition : Mod
		{
				public Condition()
				{
				}

				public string DisplayName = string.Empty;
				public string IconName = "ConditionIcon";
				public string AtlasName = "Default";
				public string GainedSomethingMessage = string.Empty;
				public List <Symptom> Symptoms = new List <Symptom>();
				public List <AvatarAction> CureActions = new List <AvatarAction>();
				public List <string> CureConditions = new List <string>();
				public List <string> CureStates = new List <string>();
				public List <string> ForcedCures = new List <string>();
				public List <string> SkillUseCures = new List <string>();
				//camera FX
				public FXType FXOnStart = FXType.None;
				public float FXIntensityOnStart = 0f;
				public FXType FXOnPing = FXType.None;
				public float FXIntensityOnPing = 0f;
				public FXType FXOnExpire = FXType.None;
				public float FXIntensityOnExpire = 0f;
				public string SoundOnInitialized = string.Empty;

				public bool HasExpired { get { return mExpired; } }

				public bool DurationStacks = false;
				public bool ExpiresAutomatically = true;
				public double Duration = 1.0f;
				public double TimeSoFar = 0f;

				public float NormalizedTimeLeft {
						get {
								if (ExpiresAutomatically) {
										return Mathf.Clamp01((float)((Duration - TimeSoFar) / Duration));
								}
								//if it doesn't expire automatically
								//1 means it'll be around forever
								return 1.0f;
						}
				}
				//this is only called when a condition is cloned
				//this should never be called when using a loaded state
				public void Initialize()
				{
						if (!string.IsNullOrEmpty(SoundOnInitialized)) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, Player.Local.tr, SoundOnInitialized);
						}

						if (SkillUseCures.Count > 0) {
								Player.Get.AvatarActions.Subscribe(AvatarAction.SkillUse, new ActionListener(SkillUse));
						}
						//make sure there are no duplicate targets in the symptoms
						//if there is make a loud noise and remove it
						mSymptomLookup.Clear();
						for (int i = Symptoms.Count - 1; i >= 0; i--) {
								if (string.IsNullOrEmpty(Symptoms[i].Target)
								|| mSymptomLookup.ContainsKey(Symptoms[i].Target)) {
										Symptoms.RemoveAt(i);
								} else {
										mSymptomLookup.Add(Symptoms[i].Target, Symptoms[i]);
								}
						}
						//TimeSoFar = 0f;

						if (string.IsNullOrEmpty(DisplayName)) {	//TODO add spaces
								DisplayName = Name;
						}
				}
				//used when the player dies
				public void Cancel()
				{	
						mExpired = true;
				}

				public void IncreaseDuration(double amountToIncrease)
				{
						//TODO add min/max variables
						Duration += amountToIncrease;
				}

				public bool HasSymptomFor(string statusKeeperName)
				{
						return mSymptomLookup.ContainsKey(statusKeeperName);
				}

				public bool HasSymptomFor(string statusKeeperName, out Symptom symptom)
				{
						return mSymptomLookup.TryGetValue(statusKeeperName, out symptom);
				}
				//conditions can expire for three reasons
				//- Duration 				- it just goes away after a while
				//- AvatarAction 			- because the player did something, eg drinking cures 'Dehydrated'
				//- Cured by condition 		- because another condition wiped it out, eg 'Wet' cures 'On Fire'
				//- Forced cures 			- because something outside the condition forced a cure, eg 'Spotted Mushroom' curing specific diseases
				public bool CheckExpired(double deltaTime,
				                       List <AvatarAction> recentActions,
				                       List <Condition> activeConditions,
				                       List <string> activeStates)
				{
						TimeSoFar += deltaTime;
						//check to see if any of the active states have cured us
						if (CureStates.Count > 0) {
								for (int i = 0; i < CureStates.Count; i++) {
										if (activeStates.Contains(CureStates[i])) {
												mExpired = true;
										}
								}
						}
						if (!mExpired && ExpiresAutomatically && (TimeSoFar >= Duration)) {	//first see if it has expired automatically
								//or if all symptoms are expired
								mExpired = true;
						} else {//if that didn't trigger, check the symptoms
								bool isActive = false;
								for (int i = 0; i < Symptoms.Count; i++) {	//this will include 'inactive' symptoms that have not been activated yet
										isActive |= Symptoms[i].IsActive(TimeSoFar);
								}
								if (!isActive) {//if NO symptom is active then we're expired
										//from this poinf forward HasExpired will always return true
										mExpired = true;
								}
						}
						return mExpired;
				}

				public float ApplyTo(StatusKeeper keeper, double deltaTime)
				{
						Symptom symptom = null;
						if (mSymptomLookup.TryGetValue(keeper.Name, out symptom)) {	//if it's active...
								if (symptom.IsActive(TimeSoFar)) {
										//apply to the actual value not the normalized value
										float currentValue = keeper.Value;
										//get the ACTUAL seek value as it applies to this keeper's seek value type
										float seekValue = StatusKeeper.GetSeekValue(keeper.ActiveState.SeekType, symptom.SeekType, symptom.SeekValue);
										//apply the value to the current value
										return Mathf.Lerp(currentValue, symptom.SeekValue, (float)(symptom.SeekSpeed * deltaTime));
								} else {	//if it's not active just return the untouched value
										return keeper.Value;
								}
						} else {
								return keeper.Value;
						}
				}

				public bool SkillUse(double timeStamp)
				{
						foreach (string skillCure in SkillUseCures) {
								if (Skills.Get.IsSkillInUse(skillCure)) {
										Cancel();
										break;
								}
						}
						return true;
				}

				protected bool mExpired = false;
				protected float mValueLastUpdate = 0f;
				protected Dictionary <string, Symptom> mSymptomLookup = new Dictionary <string, Symptom>();
		}

		[Serializable]
		public class Symptom
		{
				public string Target = "StatusKeeper";
				public StatusSeekType SeekType = StatusSeekType.Negative;
				public float SeekValue = 1.0f;
				public float SeekSpeed = 1.0f;
				//TODO timed symptoms
				//these can be used for a progression of effects
				//for instance drunkenness followed by a hangover
				//if IsExpired is true for all symptoms then the condition expires automatically
				public float ActiveAfter = 0f;
				public bool ExpiresAutomatically = false;
				public float Duration = 0f;

				public bool IsActive(double timeSoFar)
				{	//true by default for most symptoms
						if ((timeSoFar > ActiveAfter) && !IsExpired(timeSoFar)) {	//yay, this symptom is active
								//this only gets flagged once
								mHasBecomeActive = true;
								return true;
						}
						return false;
				}

				public bool IsExpired(double timeSoFar)
				{	//the duration isn't from the start of the condition
						//it's from the start of the symptom
						//if the symptom has never become active yet then it can't expire
						//this is useful for symptoms that don't kick in for a long time
						return (mHasBecomeActive && ExpiresAutomatically && (timeSoFar - ActiveAfter) > Duration);
				}

				protected bool mHasBecomeActive;
		}
}