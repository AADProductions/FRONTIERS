using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World.BaseWIScripts
{
		public class LiquidContainer : WIScript
		{
				public LiquidContainerState	State = new LiquidContainerState();

				public void Start()
				{
						State.Contents.Clear();
				}

				public override void PopulateExamineList(List<WIExamineInfo> examine)
				{
						if (State.IsEmpty) {
								examine.Add(new WIExamineInfo("It's empty"));
						} else {
								examine.Add(new WIExamineInfo("It's filled with " + State.Contents.InstanceWeight.ToString() + "/" + State.Capacity.ToString() + " " + State.Contents.DisplayName));
						}
				}

				public override void PopulateOptionsList(List<WIListOption> options, List <string> message)
				{
						if (State.IsEmpty) {
								WIListOption option = new WIListOption("(Empty)", "Drink");
								option.Disabled = true;
								options.Add(option);
						} else {
								if (Player.Local.Status.HasCondition("BurnedByFire")) {
										options.Add(new WIListOption("Pour on Self", "PourOnSelf"));
								}
								options.Add(new WIListOption("Pour Out"));
								options.Add(new WIListOption("Drink " + State.Contents.DisplayName, "Drink"));
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						WIListResult dialogResult = secondaryResult as WIListResult;

						switch (dialogResult.SecondaryResult) {
								case "PourOnSelf":
										MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "FillLiquidContainer");
										Player.Local.Status.RemoveCondition("BurnedByFire");
										Player.Local.Status.AddCondition("Wet");
										State.Contents.Clear();
										break;

								case "Drink":
										WorldItem liquid = null;
										if (WorldItems.Get.PackPrefab(State.Contents.PackName, State.Contents.PrefabName, out liquid)) {//this is tricky - we want to drink it without destroying the prefab
												FoodStuff foodstuff = null;
												if (liquid.Is <FoodStuff>(out foodstuff)) {	//DON'T consume the foodstuff!
														FoodStuff.Drink(foodstuff);
														State.Contents.InstanceWeight--;
														GUIManager.PostInfo("Drank 1 " + State.Contents.DisplayName + ", " + State.Contents.InstanceWeight.ToString() + "/" + State.Capacity.ToString() + " left.");						                      
												}
										}
										break;

								case "Pour Out":
										//two options here
										//if we're in the inventory then we want to add our contents to the selected stack
										//if we're in the world we want to dump it into the world
										bool playSound = false;
										if (PrimaryInterface.IsMaximized("Inventory")) {
												WIStack selectedStack = Player.Local.Inventory.SelectedStack;
												if (Stacks.Can.Stack(selectedStack, State.Contents)) {
														WIStackError error = WIStackError.None;
														for (int i = 0; i < State.Contents.InstanceWeight; i++) {
																StackItem contents = State.Contents.ToStackItem();
																if (!Stacks.Push.Item(selectedStack, contents, ref error)) {
																		break;
																} else {
																		playSound = true;
																}
														}
												}
										} else {
												State.Contents.Clear();
												GUIManager.PostInfo("Discarded contents");
												if (Player.Local.Surroundings.IsWorldItemInRange) {
														Flammable flammable = null;
														if (Player.Local.Surroundings.WorldItemFocus.worlditem.Is <Flammable>(out flammable) && flammable.IsOnFire) {
																flammable.Extinguish();
														}
												}
												playSound = true;
										}
										if (playSound) {
												MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "FillLiquidContainer");
										}
										break;

								default:
										break;
						}
				}
		}

		[Serializable]
		public class LiquidContainerState
		{
				public bool TryToFillWith(GenericWorldItem item, int numItems, out int numFilled, out string errorMessage)
				{
						int availableCapacity = 0;
						numFilled = 0;
						if (CanFillWith(item, out availableCapacity, out errorMessage)) {
								Contents.CopyFrom(item);
								numFilled = availableCapacity;
								if (numFilled > numItems) {
										numFilled = numItems;
								}
								Contents.InstanceWeight = numFilled;
								return true;
						}
						return false;
				}

				public bool CanFillWith(GenericWorldItem item, out int availableCapacity, out string errorMessage)
				{
						availableCapacity = 0;
						errorMessage = string.Empty;
						if (IsFilled) {
								errorMessage = "Already full";
								return false;
						}

						if (!IsEmpty) {	//if we have some filled, see if the items are compatible
								availableCapacity = Capacity - Contents.InstanceWeight;
								return Stacks.Can.Stack(item, Contents.StackName);
						}

						if (CanContain.Count > 0) {	//if we have restrictions, check them
								bool canContain = false;
								foreach (string canContainType in CanContain) {
										if (Stacks.Can.Stack(item, canContainType)) {//if any item is compatible, yay we can fill
												canContain = true;
												break;
										}
								}
								if (!canContain) {
										errorMessage = "This container can't hold " + item.DisplayName;
										return false;
								}
						}
						availableCapacity = Capacity;
						return true;
				}

				public float NormalizedFillAmount {
						get {
								return ((float)this.Contents.InstanceWeight) / ((float)Capacity);
						}
				}

				public bool IsEmpty {
						get {
								return Contents.IsEmpty || Contents.InstanceWeight <= 0;
						}
				}

				public bool IsFilled {
						get {
								return Contents.InstanceWeight >= Capacity;
						}
				}

				public int Capacity = 1;
				public List <string> CanContain = new List <string>();
				public GenericWorldItem Contents = new GenericWorldItem();
		}
}