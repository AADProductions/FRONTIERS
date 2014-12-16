using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.GUI;

namespace Frontiers
{
		public class PlayerWearables : PlayerScript
		{
				public PlayerWearablesState State = new PlayerWearablesState();

				public override void AdjustPlayerMotor(ref float MotorAccelerationMultiplier, ref float MotorJumpForceMultiplier, ref float MotorSlopeAngleMultiplier)
				{
						MotorJumpForceMultiplier += NormalizedRangeStrengthChange;
				}

				public TemperatureRange AdjustTemperatureExposure(TemperatureRange temp)
				{
						int coldProtectValue = Mathf.FloorToInt(NormalizedRangeColdProtection * 5);//temps range from 0 to 4
						int heatProtectValue = Mathf.FloorToInt(NormalizedRangeHeatProtection * 5);
						int adjustedTempValue = (int)temp;

						switch (temp) {
								case TemperatureRange.A_DeadlyCold:
								case TemperatureRange.B_Cold:
										adjustedTempValue += coldProtectValue;
										break;

								case TemperatureRange.C_Warm:
								default:
										//don't do anything when it's comfortably warm
										break;

								case TemperatureRange.D_Hot:
								case TemperatureRange.E_DeadlyHot:
										adjustedTempValue -= heatProtectValue;
										break;
						}
						return (TemperatureRange)adjustedTempValue;
				}

				public int ColdProtection;
				public int HeatProtection;
				public int DamageProtection;
				public int EnergyProtection;
				public int VisibilityChange;
				public int StrengthChange;
				//the cumulative material types for armor - used to check whether an ArmorLevel check is necessary
				public WIMaterialType ArmorMaterialTypes;

				public int ArmorLevel(WIMaterialType type)
				{		//use the lookup we generated earlier
						return mArmorLevelLookup[type];
				}

				#region convenience properties

				//values from 0 to 1
				public float NormalizedDamageProtection {
						get {
								//damage can never go below 0, there are never penalties
								//so this value will always be from 0 to 1
								return 1.0f - ((float)(MaxDamageRange - DamageProtection)) / MaxDamageRange;
						}
				}

				public float NormalizedColdProtection {
						get {
								return 1.0f - ((float)(MaxCold - (ColdProtection + MaxColdRange))) / MaxCold;
						}
				}

				public float NormalizedHeatProtection {
						get {
								return 1.0f - ((float)(MaxHeat - (HeatProtection + MaxHeatRange))) / MaxHeat;
						}
				}

				public float NormalizedEnergyProtection {
						get {
								return 1.0f - ((float)(MaxEnergy - (EnergyProtection + MaxEnergyRange))) / MaxEnergy;
						}
				}

				public float NormalizedVisibilityChange {
						get {
								return 1.0f - ((float)(MaxVisibility - (VisibilityChange + MaxVisibilityRange))) / MaxVisibility;
						}
				}

				public float NormalizedStrengthChange {
						get {
								return 1.0f - ((float)(MaxStrength - (StrengthChange + MaxStrengthRange))) / MaxStrength;
						}
				}
				//values from -1 to 1 - useful for applying directly to stats
				public float NormalizedRangeColdProtection {
						get {
								return (NormalizedColdProtection - 0.5f) * 2;
						}
				}

				public float NormalizedRangeHeatProtection {
						get {
								return (NormalizedHeatProtection - 0.5f) * 2;
						}
				}

				public float NormalizedRangeEnergyProtection {
						get {
								return (NormalizedEnergyProtection - 0.5f) * 2;
						}
				}

				public float NormalizedRangeVisibilityChange {
						get {
								return (NormalizedStrengthChange - 0.5f) * 2;
						}
				}

				public float NormalizedRangeStrengthChange {
						get {
								return (NormalizedStrengthChange - 0.5f) * 2;
						}
				}
				//range * 2
				//you might be wondering why i bother to create props like this
				//it's because i'm really bad at basic arethmatic and bugs show up if i don't
				public int MaxCold {
						get {
								return MaxColdRange * 2;
						}
				}

				public int MaxHeat {
						get {
								return MaxHeatRange * 2;
						}
				}

				public int MaxEnergy {
						get {
								return MaxEnergyRange * 2;
						}
				}

				public int MaxVisibility {
						get {
								return MaxVisibilityRange * 2;
						}
				}

				public int MaxStrength {
						get {
								return MaxStrengthRange * 2;
						}
				}

				#endregion

				//TODO move these into globals
				public int MaxColdRange = 25;
				public int MaxHeatRange = 25;
				public int MaxDamageRange = 25;
				public int MaxEnergyRange = 25;
				public int MaxVisibilityRange = 25;
				public int MaxStrengthRange = 25;

				public bool IsWearing(WearableType typeOfWearable, BodyPartType onBodyPart, BodyOrientation orientation)
				{
						return IsWearing(typeOfWearable, onBodyPart, orientation, string.Empty);
				}

				public bool Wear(Wearable wearable)
				{
						foreach (InventorySquareWearable square in GUIInventoryInterface.Get.ClothingInterface.Squares) {
								if (square.PushWearable(wearable)) {
										return true;
								}
						}
						return false;
				}

				public bool IsWearing(WearableType typeOfWearable, BodyPartType onBodyPart, BodyOrientation orientation, string articlePrefabName)
				{
						bool upperBody = true;
						int index = Wearables.GetWearableIndex(onBodyPart, orientation, ref upperBody);
						WIStackContainer container = null;
						if (upperBody) {
								container = State.UpperBodyContainer;
						} else {
								container = State.LowerBodyContainer;
						}
						WIStack stack = container.StackList[index];
						IWIBase wearableItem;
						if (stack.HasTopItem) {
								wearableItem = stack.TopItem;
								if (Wearable.CanWear(typeOfWearable, onBodyPart, orientation, wearableItem)) {
										//has top item and we can wear that item, so yes we are wearing it
										if (!string.IsNullOrEmpty(articlePrefabName)) {
												if (string.Equals(wearableItem.PrefabName, articlePrefabName)) {
														return true;
												}
										} else {
												// no need to check for article
												return true;
										}
								}
						}
						return false;
				}

				public override void OnGameStartFirstTime()
				{
						State.UpperBodyContainer = Stacks.Create.StackContainer(player, WIGroups.Get.Player);
						State.LowerBodyContainer = Stacks.Create.StackContainer(player, WIGroups.Get.Player);
				}

				public override void OnGameStart()
				{
						GUIInventoryInterface.Get.ClothingInterface.Initialize();
						GUIInventoryInterface.Get.ClothingInterface.RefreshClothing += RefreshClothing;
				}

				public void RefreshClothing()
				{

						if (mArmorLevelLookup == null) {
								//haven't initialized yet
								mArmorLevelLookup = new Dictionary<WIMaterialType, int>();
								mAllMaterialTypes = new List<WIMaterialType>();
								WIMaterialType[] enumTypes = (WIMaterialType[])Enum.GetValues(typeof(WIMaterialType));
								mAllMaterialTypes.AddRange(enumTypes);
						} else {
								mArmorLevelLookup.Clear();
						}

						//add all possible material types to the lookup so we don't have to check for keys later
						for (int i = 0; i < mAllMaterialTypes.Count; i++) {
								mArmorLevelLookup.Add(mAllMaterialTypes[i], 0);
						}

						ArmorMaterialTypes = WIMaterialType.None;
						ColdProtection = 0;
						HeatProtection = 0;
						DamageProtection = 0;
						EnergyProtection = 0;
						VisibilityChange = 0;
						StrengthChange = 0;

						for (int i = 0; i < State.UpperBodyContainer.StackList.Count; i++) {
								mCheckStack = State.UpperBodyContainer.StackList[i];
								if (mCheckStack.HasTopItem) {
										mCheckItem = mCheckStack.TopItem;
										if (WorldItems.Get.PackPrefab(mCheckItem.PackName, mCheckItem.PrefabName, out mCheckWearableWorldItem)) {
												if (mCheckWearableWorldItem.Is <Wearable>(out mCheckWearable)) {
														ColdProtection += mCheckWearable.ColdProtection;
														HeatProtection += mCheckWearable.HeatProtection;
														EnergyProtection += mCheckWearable.HeatProtection;
														VisibilityChange += mCheckWearable.VisibilityChange;
														StrengthChange += mCheckWearable.StrengthChange;
												}
												if (mCheckWearableWorldItem.Is <Armor>(out mCheckArmor)) {
														DamageProtection += mCheckArmor.BaseDamageProtection;
														ArmorMaterialTypes |= mCheckArmor.MaterialTypes;
												}
										}
								}
						}

						for (int i = 0; i < State.LowerBodyContainer.StackList.Count; i++) {
								mCheckStack = State.LowerBodyContainer.StackList[i];
								if (mCheckStack.HasTopItem) {
										mCheckItem = mCheckStack.TopItem;
										if (WorldItems.Get.PackPrefab(mCheckItem.PackName, mCheckItem.PrefabName, out mCheckWearableWorldItem)) {
												if (mCheckWearableWorldItem.Is <Wearable>(out mCheckWearable)) {
														ColdProtection += mCheckWearable.ColdProtection;
														HeatProtection += mCheckWearable.HeatProtection;
														EnergyProtection += mCheckWearable.HeatProtection;
														VisibilityChange += mCheckWearable.VisibilityChange;
														StrengthChange += mCheckWearable.StrengthChange;
												}
												if (mCheckWearableWorldItem.Is <Armor>(out mCheckArmor)) {
														//add the armor's general damage protection
														DamageProtection += mCheckArmor.BaseDamageProtection;
														//add its specific kinds of protection to the lookup
														//check each material type and see if the armor protects against it
														for (int j = 0; j < mAllMaterialTypes.Count; j++) {
																mArmorLevelLookup[mAllMaterialTypes[j]] = mArmorLevelLookup[mAllMaterialTypes[j]] + mCheckArmor.ArmorLevel(mAllMaterialTypes[j]);
														}
												}
										}
								}
						}
				}

				protected IWIBase mCheckItem;
				protected WorldItem mCheckWearableWorldItem;
				protected Wearable mCheckWearable;
				protected Armor mCheckArmor;
				protected WIStack mCheckStack;
				protected Dictionary <WIMaterialType,int> mArmorLevelLookup;
				protected List <WIMaterialType> mAllMaterialTypes;
		}

		[Serializable]
		public class PlayerWearablesState
		{
				public WIStackContainer UpperBodyContainer;
				public WIStackContainer LowerBodyContainer;
		}
}