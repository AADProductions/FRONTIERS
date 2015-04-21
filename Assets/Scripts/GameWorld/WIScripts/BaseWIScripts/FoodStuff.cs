using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World.WIScripts
{
		public class FoodStuff : WIScript
		{
				public FoodStuffState State = new FoodStuffState();
				public bool NonStandardProps = false;
				public double CookTimeRTSeconds = 10.0f;
				public double RotTimeWTHours = 250f;
				public Photosensitive photosensitive = null;
				public GameObject CookingFX;
				public Action OnEat;
				public string CommonName;

				public static int CalculateGlobalPrice(int basePrice)
				{
						return basePrice + Globals.BaseValueFoodStuff;
				}

				public static int CalculateLocalPrice(int basePrice, IWIBase item)
				{
						if (item == null) {
								return basePrice;
						}

						object foodStuffStateObject = null;
						if (item.GetStateOf <FoodStuff>(out foodStuffStateObject)) {
								FoodStuffState f = (FoodStuffState)foodStuffStateObject;
								if (f != null) {
										basePrice += CalculateFoodStuffLocalPrice(f.PotentialProps, item.State);
								}
						}

						return basePrice;
				}

				public override void OnInitialized()
				{
						if (worlditem.Is <Photosensitive>(out photosensitive)) {
								photosensitive.OnHeatIncrease += OnHeatChange;
								photosensitive.OnHeatDecrease += OnHeatChange;
						}
						RefreshFoodStuffProps();
				}

				public void OnHeatChange()
				{
						if (worlditem.Is <Photosensitive>(out photosensitive)) {
								if (photosensitive.HasNearbyFires) {
										enabled = true;
								}
						}
				}

				public void Update()
				{
						if (mDestroyed || mFinished || !mInitialized)
								return;

						try {
								if (photosensitive.HasNearbyFires) {
										if (Vector3.Distance(worlditem.tr.position, photosensitive.NearestFire.FireLight.Position) < photosensitive.NearestFire.CookScale) {
												if (CookingFX == null) {
														FXManager.Get.SpawnFX(worlditem.tr, "CookingSmoke");
												}
												State.CookTimeRTSeconds += WorldClock.ARTDeltaTime;
												//are we cooked?
												if (State.CookTimeRTSeconds > CookTimeRTSeconds) {
														//if we've cooked for twice the time we should, then we're burned
														if (State.CookTimeRTSeconds > CookTimeRTSeconds * Globals.EdibleBurnTimeMultiplier) {
																worlditem.State = "Burned";
																//at this point we're no longer cooking
																FXManager.Get.DestroyFX(CookingFX);
																enabled = false;
																return;
														} else {
																worlditem.State = "Cooked";
																//cooked items can be burned
																worlditem.States.CurrentState.IsPermanent = false;
														}
												}
										}
								} else {
										FXManager.Get.DestroyFX(CookingFX);
										enabled = false;
								}
						} catch (Exception e) {

						}
				}

				public void SetProps(FoodStuffProps props)
				{
						mSettingProps = true;
						State.PotentialProps.Clear();
						State.PotentialProps.Add(props);
						mCurrentProps = props;
						worlditem.State = mCurrentProps.Name;
						mSettingProps = false;
				}

				protected bool mSettingProps = false;

				public FoodStuffProps Props {
						get {
								if (mCurrentProps == null) {
										RefreshFoodStuffProps();
								}
								return mCurrentProps;
						}
				}

				public bool HasProps {
						get {
								return mCurrentProps != null && State.PotentialProps.Count > 0;
						}
				}

				public override void OnStateChange()
				{
						if (mSettingProps) {
								return;
						}

						RefreshFoodStuffProps();
				}

				public void RefreshFoodStuffProps()
				{
						//some food stuff has states
						if (worlditem.HasStates) {
								string newStateName = worlditem.State;
								bool foundState = false;
								for (int i = 0; i < State.PotentialProps.Count; i++) {
										FoodStuffProps potentialProps = State.PotentialProps[i];
										if (potentialProps.Name.Equals(newStateName)) {
												mCurrentProps = potentialProps;
												foundState = true;
												break;
										}
								}
								if (!foundState) {
										//Debug.Log ("Didn't find state " + worlditem.State + " in foodstuff " + name);
								}
						} else {
								//others are just standalone
								//the first state is the default in this case
								mCurrentProps = State.PotentialProps[0];
						}
				}

				public override void PopulateOptionsList(List<WIListOption> options, List <string> message)
				{
						IStackOwner owner = null;
						if (worlditem.Group.HasOwner(out owner) && owner != Player.Local) {
								return;
						}

						if (!HasProps) {
								RefreshFoodStuffProps();
						}

						if (HasProps && WorldItems.IsOwnedByPlayer(worlditem)) {
								if (Props.IsLiquid) {
										options.Add(new WIListOption("Drink"));
								} else {
										options.Add(new WIListOption("Eat"));
								}
						}
				}

				public void OnPlayerUseWorldItemSecondary(object result)
				{
						WIListResult secondaryResult = result as WIListResult;
						switch (secondaryResult.SecondaryResult) {
								case "Eat":
								case "Drink":
										Eat(this);
										break;
				
								default:
										break;
						}
				}

				public static void Drink(FoodStuff foodstuff)
				{
						Eat(foodstuff);
				}

				public static void Eat(FoodStuff foodstuff)
				{
						if (foodstuff == null) {
								return;
						}

						FoodStuffProps Props = foodstuff.Props;

						if (!string.IsNullOrEmpty(Props.ConditionName)) {
								if (UnityEngine.Random.value <= Props.ConditionChance) {
										Player.Local.Status.AddCondition(Props.ConditionName);
								}
						}
			
						PlayerStatusRestore hungerRestore = PlayerStatusRestore.A_None;
						PlayerStatusRestore healthRestore = PlayerStatusRestore.A_None;
						PlayerStatusRestore healthReduce = PlayerStatusRestore.A_None;
						HallucinogenicStrength hallucinogen = HallucinogenicStrength.None;
						bool wellFed = false;
			
						if (Flags.Check((uint)Props.Type, (uint)FoodStuffEdibleType.Edible, Flags.CheckType.MatchAny)) {
								hungerRestore = Props.HungerRestore;
						}
						if (Flags.Check((uint)Props.Type, (uint)FoodStuffEdibleType.Hallucinogen, Flags.CheckType.MatchAny)) {
								hallucinogen = Props.Hallucinogen; 
						}
						if (Flags.Check((uint)Props.Type, (uint)FoodStuffEdibleType.Medicinal, Flags.CheckType.MatchAny)) {
								healthRestore = Props.HealthRestore;				
						}
						if (Flags.Check((uint)Props.Type, (uint)FoodStuffEdibleType.Poisonous, Flags.CheckType.MatchAny)) {
								healthReduce = Props.HealthReduce;
						}
						if (Flags.Check((uint)Props.Type, (uint)FoodStuffEdibleType.WellFed, Flags.CheckType.MatchAny)) {
								hungerRestore = PlayerStatusRestore.F_Full;
								wellFed = true;
						}

						Player.Local.Status.ReduceStatus(healthReduce, "Health");
						if (foodstuff.Props.IsLiquid) {
								Player.Local.Status.RestoreStatus(hungerRestore, "Thirst");
						} else {
								Player.Local.Status.RestoreStatus(hungerRestore, "Hunger");
						}
						Player.Local.Status.RestoreStatus(healthRestore, "Health");
			
						if (wellFed) {
								Player.Local.Status.AddCondition("WellFed");
						}
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoiceMale, Props.EatFoodSound);

						if (!string.IsNullOrEmpty(Props.CustomStatusKeeperRestore)) {
								Player.Local.Status.RestoreStatus(Props.CustomRestore, Props.CustomStatusKeeperRestore);
						}
						if (!string.IsNullOrEmpty(Props.CustomStatusKeeperReduce)) {
								Player.Local.Status.RestoreStatus(Props.CustomRestore, Props.CustomStatusKeeperReduce);
						}

						if (foodstuff.worlditem != null && !foodstuff.worlditem.IsTemplate) {
								foodstuff.OnEat.SafeInvoke();
								if (foodstuff.State.ConsumeOnEat) {
										//Debug.Log ("FOODSTUFF: Setting mode to RemovedFromGame");
										foodstuff.worlditem.SetMode(WIMode.RemovedFromGame);
								}
						}
				}

				public static string DescribeProperties(FoodStuffProps props, string stateName)
				{		//TODO use a f'ing string builder
						string description = "When " + stateName + ", it's";
						if (Flags.Check((uint)props.Type, (uint)FoodStuffEdibleType.Edible, Flags.CheckType.MatchAny)) {
								description += " edible,";
								if (Flags.Check((uint)props.Type, (uint)FoodStuffEdibleType.Poisonous, Flags.CheckType.MatchAny)) {
										description += " and can make for a";
										switch (props.HungerRestore) {
												case PlayerStatusRestore.F_Full:
												case PlayerStatusRestore.E_FourFifths:
														description += " full meal,";
														break;

												case PlayerStatusRestore.D_ThreeFifths:
												case PlayerStatusRestore.C_TwoFifths:
														description += " large snack,";
														break;

												default:
														description += " small snack,";
														break;
										}
										description += " but it's also";
										switch (props.HealthReduce) {
												case PlayerStatusRestore.F_Full:
												case PlayerStatusRestore.E_FourFifths:
														description += " seriously";
														break;

												case PlayerStatusRestore.D_ThreeFifths:
												case PlayerStatusRestore.C_TwoFifths:
														description += " moderately";
														break;

												default:
														description += " mildly";
														break;
										}
										description += " poisonous";
								} else {
										description += " and it's good for a";
										switch (props.HungerRestore) {
												case PlayerStatusRestore.F_Full:
												case PlayerStatusRestore.E_FourFifths:
														description += " full meal";
														break;

												case PlayerStatusRestore.D_ThreeFifths:
												case PlayerStatusRestore.C_TwoFifths:
														description += " large snack";
														break;

												default:
														description += " small snack";
														break;
										}
								}

								if (Flags.Check((uint)props.Type, (uint)FoodStuffEdibleType.Medicinal, Flags.CheckType.MatchAny)) {
										description += " It also has";
										switch (props.HealthRestore) {
												case PlayerStatusRestore.F_Full:
												case PlayerStatusRestore.E_FourFifths:
														description += " powerful.";
														break;

												case PlayerStatusRestore.D_ThreeFifths:
												case PlayerStatusRestore.C_TwoFifths:
														description += " moderate.";
														break;

												default:
														description += " mild.";
														break;
										}
										description += " medicinal properties";
								}

								if (Flags.Check((uint)props.Type, (uint)FoodStuffEdibleType.Hallucinogen, Flags.CheckType.MatchAny)) {
										description += ", but it's a ";
										switch (props.Hallucinogen) {
												case HallucinogenicStrength.Strong:
														description += " string";
														break;

												case HallucinogenicStrength.Moderate:
														description += " moderate";
														break;

												default:
														description += " mild";
														break;
										}
										description += " hallucinogen so I'd better be careful.";
								} else {
										description += ".";
								}

						} else {
								description += " inedible.";
						}
						return description;
				}

				public static int CalculateFoodStuffLocalPrice(List<FoodStuffProps> props, string stateName)
				{
						FoodStuffProps p = null;
						if (props.Count > 1) {
								p = props[0];
								for (int i = 0; i < props.Count; i++) {
										if (props[i].Name == stateName) {
												p = props[i];
										}
								}
						} else if (props.Count > 0) {
								p = props[0];
						} else {
								Debug.Log("No potential props, returning base value");
								return 1;
						}

						float price = 0f;

						price += Frontiers.Status.RestoreToFloat(p.HealthRestore) * Globals.BaseValueFoodStuff * 2;
						price -= Frontiers.Status.RestoreToFloat(p.HealthReduce) * Globals.BaseValueFoodStuff;
						if (Flags.Check((uint)p.Type, (uint)FoodStuffEdibleType.WellFed, Flags.CheckType.MatchAny)) {
								price += Globals.BaseValueFoodStuff * 10;
						} else {
								price += Frontiers.Status.RestoreToFloat(p.HungerRestore) * Globals.BaseValueFoodStuff;
						}

						return Mathf.CeilToInt(Mathf.Clamp(price, 1, Mathf.Infinity));
				}

				protected FoodStuffProps mCurrentProps;
				#if UNITY_EDITOR
				//this editor function does a bunch of sanity-check stuff
				//to make sure that the foodstuff is set up properly
				public override void InitializeTemplate()
				{
						base.InitializeTemplate();

						if (NonStandardProps)
								return;

						mCurrentProps = null;
						if (State.PotentialProps.Count > 0) {
								WIStates states = null;
								if (gameObject.HasComponent <WIStates>(out states)) {
										for (int i = 0; i < State.PotentialProps.Count; i++) {
												if (State.PotentialProps[i].Name == states.DefaultState) {
														mCurrentProps = State.PotentialProps[i];
														break;
												}
										}
								}
								if (mCurrentProps == null) {
										for (int i = 0; i < State.PotentialProps.Count; i++) {
												if (State.PotentialProps[i].Name == "Raw") {
														mCurrentProps = State.PotentialProps[i];
														break;
												}
										}
								}
								if (mCurrentProps == null) {
										//fuck it
										mCurrentProps = State.PotentialProps[0];
								}
						}

						return;
						if (Application.isPlaying) {
								return;
						}

						if (worlditem.Props.Global.MaterialType != WIMaterialType.Plant && worlditem.Props.Global.MaterialType != WIMaterialType.Flesh) {
								worlditem.Props.Global.MaterialType = WIMaterialType.Food;
						}

						if (State.PotentialProps.Count == 0) {
								Debug.Log("Didn't find any potential food props, creating 'Raw' now");
								FoodStuffProps props = new FoodStuffProps();
								props.Name = "Raw";
								props.Type = FoodStuffEdibleType.Edible;
								props.HungerRestore = PlayerStatusRestore.B_OneFifth;
								props.Perishable = false;
								State.PotentialProps.Add(props);
						} else {
								Debug.Log("Found " + State.PotentialProps.Count.ToString() + " props");
						}

						if (worlditem.States != null) {
								Debug.Log("worlditem has states");
								foreach (WIState state in worlditem.States.States) {
										bool foundAccompanyingFoodState = false;
										foreach (FoodStuffProps props in this.State.PotentialProps) {
												if (props.Name == state.Name) {
														foundAccompanyingFoodState = true;
												}
										}

										if (!foundAccompanyingFoodState) {
												Debug.Log("Didn't find accompanying food state in " + worlditem.name + " for worlditem state: " + state.Name + ", adding now");
												FoodStuffProps newProps = new FoodStuffProps();
												newProps.Name = state.Name;
												//there are some common states that we can make guesses for
												switch (state.Name) {
														case "Rotten":
																newProps.Type = FoodStuffEdibleType.Edible;
																newProps.HungerRestore = PlayerStatusRestore.B_OneFifth;
																newProps.ConditionChance = 0.95f;
																newProps.ConditionName = "FoodPoisoning"; 
																newProps.Perishable = false;
																break;

														case "Raw":
																newProps.Type = FoodStuffEdibleType.Edible;
																newProps.HungerRestore = PlayerStatusRestore.C_TwoFifths;
																newProps.Perishable = true;
																break;

														case "Cooked":
																newProps.Type = FoodStuffEdibleType.Edible;
																newProps.HungerRestore = PlayerStatusRestore.F_Full;
																newProps.Perishable = true;
																break;

														case "Burned":
																newProps.Type = FoodStuffEdibleType.Edible;
																newProps.HungerRestore = PlayerStatusRestore.B_OneFifth;
																newProps.Perishable = false;
																break;

														case "Preserved":
														case "Dried":
																newProps.Type = FoodStuffEdibleType.Edible;
																newProps.HungerRestore = PlayerStatusRestore.D_ThreeFifths;
																newProps.Perishable = false;
																break;

														default:
																newProps.Type = FoodStuffEdibleType.None;//make it inedible
																break;
												}
												State.PotentialProps.Add(newProps);
										}
								}
						}

						bool requiresRottenState = false;

						foreach (FoodStuffProps props in State.PotentialProps) {

								if (props.Name == "Raw") {
										bool foundCooked = false;
										foreach (FoodStuffProps cookedProps in State.PotentialProps) {
												if (cookedProps.Name == "Cooked") {
														foundCooked = true;
														props.ConditionName = "FoodPoisoning";
														props.ConditionChance = 0.25f;
														break;
												}
										}
										if (!foundCooked) {
												props.ConditionName = string.Empty;
												props.ConditionChance = 0f;
										} else {
												gameObject.GetOrAdd <Photosensitive>();
										}
								}


								requiresRottenState |= props.Perishable;
								if (props.Name == "Rotten") {
										requiresRottenState |= true;
								}
						}

						//check our states against our props and make sure there's parity
						if (State.PotentialProps.Count > 1) {

								if (worlditem.States == null) {
										worlditem.States = worlditem.gameObject.AddComponent <WIStates>();
										worlditem.InitializeTemplate();
										return;
								}

								//now attempt to match our props up against our states and make sure there's parity
								foreach (FoodStuffProps props in State.PotentialProps) {

										WIState existingState = null;
										bool foundAccompanyingState = false;
										foreach (WIState state in worlditem.States.States) {
												if (state.Name == props.Name) {
														if (!foundAccompanyingState) {
																existingState = state;
																foundAccompanyingState = true;
														}
														if (state.Name == "Rotten") {
																requiresRottenState = true;
														}
												}
										}

										if (!foundAccompanyingState) {
												WIState newState = null;
												Debug.Log("Didn't find accompanying worlditem state in " + worlditem.name + " for food state: " + props.Name + ", adding now");
												if (worlditem.States.States.Count == 0) {
														//create a state from the base object - this will also strip the base object of renderers etc.
														newState = WorldItems.CreateTemplateState(worlditem, props.Name, worlditem.gameObject);
												} else {
														//otherwise just make a copy from the first existing state, we'll clean it up later
														newState = WorldItems.CreateTemplateState(worlditem, props.Name, worlditem.States.States[0].StateObject);
												}
												existingState = newState;
										}

										if (existingState != null) {

												existingState.IsInteractive = true;
												existingState.Suffix = props.Name;
												existingState.UnloadWhenStacked = true;
												existingState.CanEnterInventory = true;
												existingState.CanBePlaced = true;
												existingState.CanBeDropped = true;
												existingState.CanBeCarried = true;

												switch (existingState.Name) {
														case "Raw":
																existingState.IsPermanent = false;
																break;

														case "Cooked":
																existingState.IsPermanent = true;
																existingState.FXOnChange = "FoodstuffCookedSmoke";
																break;

														case "Rotten":
																existingState.IsPermanent = true;
																break;

														case "Burned":
																existingState.IsPermanent = true;
																existingState.FXOnChange = "FoodstuffBurnedSmoke";
																break;

														case "Preserved":
														case "Dried":
																existingState.IsPermanent = true;
																break;

														default:
																break;
												}
										}
								}
						} else {
								Debug.Log("Only 1 foodstuff props so no need for states");
								if (worlditem.States != null && worlditem.States.States.Count == 0) {
										GameObject.DestroyImmediate(worlditem.States);
								}
						}

						if (requiresRottenState) {
								Debug.Log("Requires perishable, checking now");
								bool hasRottenState = false;
								bool hasRawState = false;
								foreach (FoodStuffProps props in State.PotentialProps) {
										if (props.Name == "Raw") {
												props.Perishable = true;
												hasRawState = true;
										} else if (props.Name == "Rotten") {
												hasRottenState = true;
										}
								}

								if (!hasRawState) {
										FoodStuffProps newProps = new FoodStuffProps();
										newProps.Name = "Raw";
										newProps.Perishable = true;
										newProps.Type = FoodStuffEdibleType.Edible;
										newProps.HungerRestore = PlayerStatusRestore.C_TwoFifths;
										newProps.Perishable = true;
										State.PotentialProps.Add(newProps);
								}
								if (!hasRottenState) {
										FoodStuffProps newProps = new FoodStuffProps();
										newProps.Name = "Rotten";
										newProps.Type = FoodStuffEdibleType.Edible;
										newProps.HungerRestore = PlayerStatusRestore.C_TwoFifths;
										newProps.ConditionChance = 0.5f;
										newProps.ConditionName = "FoodPoisoning";
										State.PotentialProps.Add(newProps);
								}
						} else {
								Debug.Log("Didn't require rotten state");
						}

						if (worlditem.States != null) {
								bool foundDefaultState = false;
								foreach (WIState state in worlditem.States.States) {
										if (worlditem.States.DefaultState == state.Name) {
												foundDefaultState = true;
												break;
										}
								}

								if (!foundDefaultState) {
										Debug.Log("Didin't find default state " + worlditem.States.DefaultState + ", setting to Raw");
										worlditem.States.DefaultState = "Raw";
								}
						}

						if (worlditem.HasStates) {
								//now that the worlditem has states it shouldn't have its own mesh stuff
								//destroy the me now
								MeshFilter worldItemMF = null;
								if (worlditem.gameObject.HasComponent <MeshFilter>(out worldItemMF)) {
										GameObject.DestroyImmediate(worldItemMF);
								}
								MeshRenderer worldItemMR = null;
								if (worlditem.gameObject.HasComponent <MeshRenderer>(out worldItemMR)) {
										GameObject.DestroyImmediate(worldItemMR);
								}
								if (worlditem.collider != null) {
										GameObject.DestroyImmediate(worlditem.collider);
								}
						}

						foreach (FoodStuffProps props in State.PotentialProps) {
								if (props.IsLiquid) {
										props.EatFoodSound = "DrinkLiquidGeneric";
								} else {
										props.EatFoodSound = "EatFoodGeneric";
								}
						}

						mCurrentProps = null;
						base.InitializeTemplate();
				}
				//editor function to quickly set up a new foodstuff object
				public void EditorCreateProps()
				{
						if (State.PotentialProps.Count == 0) {
								WIStates states = gameObject.GetComponent <WIStates>();

								if (states != null) {
										foreach (WIState state in states.States) {
												FoodStuffProps props = new FoodStuffProps();
												props.Name = state.Name;
												switch (props.Name) {
														case "Raw":
																props.HungerRestore = PlayerStatusRestore.D_ThreeFifths;
																props.Type = FoodStuffEdibleType.Edible;
																break;

														case "Rotten":
																props.ConditionName = "FoodPoisoning";
																props.HungerRestore = PlayerStatusRestore.C_TwoFifths;
																props.ConditionChance = 0.75f;
																props.Type = FoodStuffEdibleType.Edible;
																break;

														case "Cooked":
																props.HealthRestore = PlayerStatusRestore.F_Full;
																props.Type = FoodStuffEdibleType.Edible;
																break;

														case "Preserved":
																props.HungerRestore = PlayerStatusRestore.F_Full;
																props.Type = FoodStuffEdibleType.Edible;
																break;

														case "Burned":
																props.HungerRestore = PlayerStatusRestore.D_ThreeFifths;
																props.Type = FoodStuffEdibleType.Edible;
																break;
												}
												State.PotentialProps.Add(props);
										}
								} else {
										FoodStuffProps props = new FoodStuffProps();
										props.HungerRestore = PlayerStatusRestore.C_TwoFifths;
										props.Type = FoodStuffEdibleType.Edible;
										State.PotentialProps.Add(props);
								}
						}
				}
				#endif
		}

		[Serializable]
		public class FoodStuffState
		{
				public bool ConsumeOnEat = true;
				public double CookTimeRTSeconds = 0f;
				public double RotTimeWTSeconds = 0f;

				public bool IsLiquid(string stateName)
				{
						if (PotentialProps.Count > 1) {
								for (int i = 0; i < PotentialProps.Count; i++) {
										if (PotentialProps[i].Name == stateName) {
												return PotentialProps[i].IsLiquid;
										}
								}
						} else if (PotentialProps.Count > 0) {
								return PotentialProps[0].IsLiquid;
						}
						return false;
				}

				public List <FoodStuffProps> PotentialProps = new List <FoodStuffProps>();
		}

		[Serializable]
		public class FoodStuffProps
		{
				public string Name = "Default";
				[BitMask(typeof(FoodStuffEdibleType))]
				public FoodStuffEdibleType Type = FoodStuffEdibleType.None;
				public bool IsLiquid = false;
				public PlayerStatusRestore HungerRestore = PlayerStatusRestore.B_OneFifth;
				public PlayerStatusRestore HealthRestore = PlayerStatusRestore.A_None;
				public PlayerStatusRestore HealthReduce = PlayerStatusRestore.A_None;
				public PoisonStrength Poisonous = PoisonStrength.None;
				public HallucinogenicStrength Hallucinogen = HallucinogenicStrength.None;
				public bool Perishable = false;
				//
				[FrontiersAvailableMods("StatusKeeper")]
				public string CustomStatusKeeperReduce;
				public PlayerStatusRestore CustomReduce = PlayerStatusRestore.A_None;
				[FrontiersAvailableMods("StatusKeeper")]
				public string CustomStatusKeeperRestore;
				public PlayerStatusRestore CustomRestore = PlayerStatusRestore.A_None;
				public float ConditionChance = 0.0f;
				public string ConditionName = string.Empty;
				public string EatFoodSound = "EatFoodGeneric";
		}

		public struct DigestedFoodStuff
		{
				public float Calories;
				public float Water;
				public float Nutrients;
		}
}