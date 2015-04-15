using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.World.WIScripts
{
		public class Wearable : WIScript
		{
				public Character Wearer;

				public bool HasWearer {
						get {
								return Wearer != null;
						}
				}

				public override void OnInitialized()
				{
						worlditem.Props.Local.Subcategory = BodyPart.ToString();
				}

				public WearableState State = new WearableState();
				//Transient features
				[BitMask(typeof(BodyOrientation))]
				public BodyOrientation Orientation = BodyOrientation.None;
				[BitMask(typeof(WearableType))]
				public WearableType Type = WearableType.Armor;
				public WearableMethod Method = WearableMethod.Rigid;
				public BodyPartType BodyPart = BodyPartType.Head;
				public bool Gendered = false;
				public int FingerIndex = 0;
				public int ColdProtection = 0;
				public int HeatProtection = 0;
				public int EnergyProtection = 0;
				public int VisibilityChange = 0;
				public int StrengthChange = 0;

				public override void PopulateOptionsList(List<WIListOption> options, List<string> message)
				{
						if (worlditem.Group == WIGroups.Get.Player && worlditem.Is(WIMode.Equipped | WIMode.Frozen | WIMode.World)) {
								if (CanWear(Type, BodyPart, Orientation, worlditem)) {
										WIListOption listOption = new WIListOption("Wear", "Wear");
										if (Player.Local.Wearables.IsWearing(Type, BodyPart, Orientation)) {
												listOption.Disabled = true;
										}
										options.Add(listOption);
								}
						}
				}

				public void OnPlayerUseWorldItemSecondary(object dialogResult)
				{
						WIListResult result = (WIListResult)dialogResult;

						switch (result.SecondaryResult) {
								case "Wear":
										Player.Local.Wearables.Wear(this);
										break;

								default:
										break;
						}
				}

				public static bool CanWear(WearableType type, BodyPartType bodyPart, BodyOrientation orientation, IWIBase itemBase)
				{
						WorldItem item = null;

						if (itemBase.IsWorldItem) {
								item = itemBase.worlditem;
						} else {
								//use the item to get the template
								WorldItem prefab = null;
								WorldItems.Get.PackPrefab(itemBase.PackName, itemBase.PrefabName, out item);
						}

						if (item == null) {
								//whoops, not sure what went wrong here
								return false;
						}

						bool result = false;
						Wearable wearable = null;
						if (item.worlditem.Is <Wearable>(out wearable)) {
								bool matchesType = Flags.Check((uint)wearable.Type, (uint)type, Flags.CheckType.MatchAny);
								bool matchesOrientation = (wearable.Orientation == BodyOrientation.None && orientation == BodyOrientation.None)
								                      || Flags.Check((uint)wearable.Orientation, (uint)orientation, Flags.CheckType.MatchAny);
								bool matchesBodyPart = wearable.BodyPart == bodyPart;
								//Debug.Log("Matches type? " + matchesType.ToString() + " matches orientation? " + matchesOrientation.ToString() + " matchesBodyPart? " + matchesBodyPart.ToString());
								result = matchesType & matchesOrientation & matchesBodyPart;

						} else {
								Debug.Log("World item " + itemBase.FileName + " is not wearable");
						}
						return result;
				}

				public bool Wear(WearableType type, BodyPartType bodyPart, BodyOrientation orientation, CharacterGender gender)
				{
						State.CurrentOrientation = orientation;
						State.CurrentGender = gender;
						//update the current state
						worlditem.State = StateName(this);
			
						return false;
				}

				[Serializable]
				public class WearableState
				{
						public CharacterGender CurrentGender = CharacterGender.Male;
						public BodyOrientation CurrentOrientation = BodyOrientation.None;
						public WearableStyle CurrentStyle = WearableStyle.A;
						[FrontiersColorAttribute]
						public string BaseColor;
						[FrontiersColorAttribute]
						public string TrimColor;
						[FrontiersColorAttribute]
						public string HighlightColor;
						[FrontiersColorAttribute]
						public string CrystalColor;
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						try {
						worlditem.Props.Local.Subcategory = BodyPart.ToString();
						}
						catch (Exception e) {
								Debug.LogError(e.ToString ());
						}
				}
				#endif
				public static string StateName(Wearable wearable)
				{
						return wearable.State.CurrentOrientation.ToString() + "_" + wearable.State.CurrentStyle.ToString();
				}
		}
}
