using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World.Gameplay;
using Frontiers.World;
using System.Collections.Generic;
using System;
using Frontiers.World.WIScripts;

namespace Frontiers.GUI
{
		public class GUIPlayerClothingInterface : MonoBehaviour, IGUITabPageChild
		{
				public Camera NGUICamera;
				public GameObject UpperBodySquaresParent;
				public GameObject LowerBodySquaresParent;
				public List <InventorySquareWearable> Squares = new List <InventorySquareWearable>();
				public List <BoxCollider> StatButtons = new List<BoxCollider>();
				public List <UISprite> ShadowSprites = new List<UISprite>();
				public GameObject MaleBodyDoppleganger;
				public GameObject FemaleBodyDoppleganger;
				public UISprite HeatProtection;
				public UISprite ColdProtection;
				public UISprite DamageProtection;
				public UISprite EnergyProtection;
				public UISprite VisibilityChange;
				public UISprite StrengthChange;
				public Action RefreshClothing;
				//info display
				public bool DisplayInfo;
				public GameObject CurrentInfoTarget;
				public float CurrentInfoTargetXOffset;
				public float CurrentInfoTargetYOffset;
				public string CurrentInfo;
				public UILabel InfoLabel;
				public UISprite InfoSpriteShadow;
				public UISprite InfoSpriteBackground;
				public Transform InfoOffset;

				public void PostInfo(GameObject target, string info)
				{
						CurrentInfoTarget = target;
						CurrentInfoTargetXOffset = target.transform.localPosition.x;
						CurrentInfoTargetYOffset = target.transform.localPosition.y;
						CurrentInfo = info;
						InfoLabel.text = CurrentInfo;
						DisplayInfo = true;
						//update the box around the text to reflect its size
						Transform textTrans = InfoLabel.transform;
						Vector3 offset = textTrans.localPosition;
						Vector3 textScale = textTrans.localScale;

						// Calculate the dimensions of the printed text
						Vector3 size = InfoLabel.relativeSize;

						// Scale by the transform and adjust by the padding offset
						size.x *= textScale.x;
						size.y *= textScale.y;
						size.x += 50f;
						size.y += 50f;
						size.x += (InfoSpriteBackground.border.x + InfoSpriteBackground.border.z + (offset.x - InfoSpriteBackground.border.x) * 2f);
						size.y += (InfoSpriteBackground.border.y + InfoSpriteBackground.border.w + (-offset.y - InfoSpriteBackground.border.y) * 2f);
						size.z = 1f;

						InfoSpriteBackground.transform.localScale = size;
						InfoSpriteShadow.transform.localScale = size;
				}

				public void GetActiveInterfaceObjects(List<FrontiersInterface.Widget> currentObjects, int flag)
				{
						FrontiersInterface.Widget w = new FrontiersInterface.Widget(flag);
						for (int i = 0; i < Squares.Count; i++) {
								w.BoxCollider = Squares[i].Collider;
								w.SearchCamera = NGUICamera;
								currentObjects.Add(w);
						}
						for (int i = 0; i < StatButtons.Count; i++) {
								w.BoxCollider = StatButtons[i];
								w.SearchCamera = NGUICamera;
								currentObjects.Add(w);
						}
				}

				public void OnClickStatButton(GameObject button)
				{
						if (!mInitialized)
								return;

						float stat = 0f;
						float rangeState = 0f;
						string introspection = string.Empty;
						switch (button.name) {
								case "Heat":
										stat = Player.Local.Wearables.NormalizedHeatProtection;
										rangeState = Player.Local.Wearables.NormalizedRangeHeatProtection;
										if (stat >= .9f) {
												introspection = "This clothing makes me almost impervious to heat";
										} else if (stat > 0.65) {
												introspection = "This clothing protects me well from heat";
										} else if (stat > 0.55f) {
												introspection = "This clothing protects a little bit from heat";
										} else if (stat > 0.45f) {
												introspection = "This clothing gives me no special protection from heat";
										} else if (stat > 0.35f) {
												introspection = "This clothing makes me feel slightly overheated";
										} else {
												introspection = "This clothing makes me feel uncomfortably warm";
										}
										break;

								case "Damage":
										stat = Player.Local.Wearables.NormalizedDamageProtection;
										rangeState = stat;
										if (stat >= .9f) {
												introspection = "This clothing will protect me almost entirely from damage, while it lasts";
										} else if (stat > 0.65) {
												introspection = "This clothing will protect me from a large amount of damage, while it lasts";
										} else if (stat > 0.55f) {
												introspection = "This clothing will protect me from a moderate amount of damage, while it lasts";
										} else if (stat > 0.45f) {
												introspection = "This clothing will protect me from some damage, while it lasts";
										} else if (stat > 0.35f) {
												introspection = "This clothing will protect me from a small amount of damage, while it lasts";
										} else {
												introspection = "This clothing leaves me vulnerable to damage";
										}
										break;

								case "Energy":
										stat = Player.Local.Wearables.NormalizedEnergyProtection;
										rangeState = Player.Local.Wearables.NormalizedRangeEnergyProtection;
										if (stat >= .9f) {
												introspection = "This clothing makes me almost impervious to energy";
										} else if (stat > 0.65) {
												introspection = "This clothing protects me well from energy";
										} else if (stat > 0.55f) {
												introspection = "This clothing protects a little bit from energy";
										} else if (stat > 0.45f) {
												introspection = "This clothing gives me no special protection from energy";
										} else if (stat > 0.35f) {
												introspection = "This clothing leaves me vulnerable to energy";
										} else {
												introspection = "This clothing leaves me very vulnerable to energy";
										}
										break;

								case "Visibility":
										stat = Player.Local.Wearables.NormalizedVisibilityChange;//invert this because higher is more conspicuous
										rangeState = Player.Local.Wearables.NormalizedRangeVisibilityChange;
										if (stat >= .9f) {
												introspection = "This clothing makes me highly conspicuous";
										} else if (stat > 0.55f) {
												introspection = "This clothing makes me stand out a bit";
										} else if (stat > 0.45f) {
												introspection = "This clothing doesn't affect my visibility";
										} else if (stat > 0.35f) {
												introspection = "This clothing makes me less visible than usual";
										} else {
												introspection = "This clothing makes me practically invisible";
										}
										break;

								case "Strength":
										stat = Player.Local.Wearables.NormalizedStrengthChange;
										rangeState = Player.Local.Wearables.NormalizedRangeStrengthChange;
										if (stat >= .9f) {
												introspection = "This clothing makes me feel stronger and jump much farther";
										} else if (stat > 0.55f) {
												introspection = "This clothing gives me a small strength boost, and I can jump a bit farther";
										} else if (stat > 0.45f) {
												introspection = "This clothing doesn't affect my strength";
										} else if (stat > 0.35f) {
												introspection = "This clothing makes me tire more easily, and I can't jump as far";
										} else {
												introspection = "This clothing is exhausting to wear, I feel glued to the ground";
										}
										break;

								case "Cold":
										stat = Player.Local.Wearables.NormalizedColdProtection;
										rangeState = Player.Local.Wearables.NormalizedRangeColdProtection;
										if (stat >= .9f) {
												introspection = "This clothing makes me almost impervious to cold";
										} else if (stat > 0.65) {
												introspection = "This clothing protects me well from cold";
										} else if (stat > 0.55f) {
												introspection = "This clothing protects a little bit from cold";
										} else if (stat > 0.45f) {
												introspection = "This clothing gives me no special protection from cold";
										} else if (stat > 0.35f) {
												introspection = "This clothing leaves me vulnerable to cold";
										} else {
												introspection = "This clothing leaves me very vulnerable to cold";
										}
										break;

								default:
										break;
						}
						PostInfo(button, introspection);
				}

				public void Start()
				{
						enabled = false;
						Rigidbody rb = gameObject.AddComponent <Rigidbody>();
						rb.isKinematic = true;
				}

				public void Show()
				{
						enabled = true;
						for (int i = 0; i < Squares.Count; i++) {
								Squares[i].enabled = true;
						}

						if (Profile.Get.CurrentGame.Character.Gender == CharacterGender.Male) {
								MaleBodyDoppleganger.SetActive(true);
						} else {
								FemaleBodyDoppleganger.SetActive(true);
						}
				}

				public void Hide()
				{
						enabled = false;
						for (int i = 0; i < Squares.Count; i++) {
								Squares[i].enabled = false;
						}

						MaleBodyDoppleganger.SetActive(false);
						FemaleBodyDoppleganger.SetActive(false);

				}

				public void Initialize()
				{
						if (mInitialized)
								return;

						//Debug.Log("Initializing clothing interface");

						InfoLabel.alpha = 0f;
						InfoSpriteBackground.alpha = 0f;
						InfoSpriteShadow.alpha = 0f;

						WIStackContainer upperBodyContainer = Player.Local.Wearables.State.UpperBodyContainer;
						WIStackContainer lowerBodyContainer = Player.Local.Wearables.State.LowerBodyContainer;

						CreateWearableSquare(UpperBodySquaresParent, new Vector3(200f, -50f, -50f), WearableType.Armor, BodyPartType.Head, BodyOrientation.None, upperBodyContainer.StackList[Wearables.UpperBodyHeadIndex]);//head
						CreateWearableSquare(UpperBodySquaresParent, new Vector3(0f, -50f, -50f), WearableType.Clothing | WearableType.Jewelry, BodyPartType.Face, BodyOrientation.None, upperBodyContainer.StackList[Wearables.UpperBodyFaceIndex]);//face
						CreateWearableSquare(UpperBodySquaresParent, new Vector3(300f, -150f, -50f), WearableType.Armor, BodyPartType.Shoulder, BodyOrientation.Left, upperBodyContainer.StackList[Wearables.UpperBodyLeftShoulderIndex]);//left shoulder
						CreateWearableSquare(UpperBodySquaresParent, new Vector3(100f, -150f, -50f), WearableType.Armor, BodyPartType.Shoulder, BodyOrientation.Right, upperBodyContainer.StackList[Wearables.UpperBodyRightShoulderIndex]);//right shoulder
						CreateWearableSquare(UpperBodySquaresParent, new Vector3(325f, -250f, -50f), WearableType.Armor, BodyPartType.Arm, BodyOrientation.Left, upperBodyContainer.StackList[Wearables.UpperBodyLeftArmIndex]);//left arm
						CreateWearableSquare(UpperBodySquaresParent, new Vector3(75f, -250f, -50f), WearableType.Armor, BodyPartType.Arm, BodyOrientation.Right, upperBodyContainer.StackList[Wearables.UpperBodyRightArmIndex]);//right arm
						CreateWearableSquare(UpperBodySquaresParent, new Vector3(400f, -50f, -50f), WearableType.Jewelry, BodyPartType.Neck, BodyOrientation.None, upperBodyContainer.StackList[Wearables.UpperBodyNeckIndex]);//neck
						CreateWearableSquare(UpperBodySquaresParent, new Vector3(200f, -150f, -50f), WearableType.Armor, BodyPartType.Chest, BodyOrientation.None, upperBodyContainer.StackList[Wearables.UpperBodyChestIndex]);//chest
						CreateWearableSquare(UpperBodySquaresParent, new Vector3(350f, -350f, -50f), WearableType.Armor | WearableType.Clothing, BodyPartType.Hand, BodyOrientation.Left, upperBodyContainer.StackList[Wearables.UpperBodyLeftHandIndex]);//left hand
						CreateWearableSquare(UpperBodySquaresParent, new Vector3(50f, -350f, -50f), WearableType.Armor | WearableType.Clothing, BodyPartType.Hand, BodyOrientation.Right, upperBodyContainer.StackList[Wearables.UpperBodyRightHandIndex]);//right hand

						CreateWearableSquare(LowerBodySquaresParent, new Vector3(200f, -325f, -50f), WearableType.Armor, BodyPartType.Hip, BodyOrientation.None, lowerBodyContainer.StackList[Wearables.LowerBodyHipIndex]);//hips
						//CreateWearableSquare (LowerBodySquaresParent, new Vector3 (250f, -350f, -50f), WearableType.Container, BodyPartType.Hip, BodyOrientation.Right, lowerBodyContainer.StackList [Wearables.LowerBodyRightHipIndex]);//right hip
						CreateWearableSquare(LowerBodySquaresParent, new Vector3(250f, -450f, -50f), WearableType.Armor, BodyPartType.Leg, BodyOrientation.Left, lowerBodyContainer.StackList[Wearables.LowerBodyLeftKneeIndex]);//left knee
						CreateWearableSquare(LowerBodySquaresParent, new Vector3(150, -450f, -50f), WearableType.Armor, BodyPartType.Leg, BodyOrientation.Right, lowerBodyContainer.StackList[Wearables.LowerBodyRightKneeIndex]);//right knee
						CreateWearableSquare(LowerBodySquaresParent, new Vector3(250f, -650f, -50f), WearableType.Clothing, BodyPartType.Foot, BodyOrientation.Left, lowerBodyContainer.StackList[Wearables.LowerBodyLeftFootIndex]);//left foot
						CreateWearableSquare(LowerBodySquaresParent, new Vector3(150f, -650f, -50f), WearableType.Clothing, BodyPartType.Foot, BodyOrientation.Right, lowerBodyContainer.StackList[Wearables.LowerBodyRightFoodIndex]);//right foot
						CreateWearableSquare(LowerBodySquaresParent, new Vector3(250f, -550f, -50f), WearableType.Armor, BodyPartType.Shin, BodyOrientation.Left, lowerBodyContainer.StackList[Wearables.LowerBodyLeftShinIndex]);//left shin
						CreateWearableSquare(LowerBodySquaresParent, new Vector3(150f, -550f, -50f), WearableType.Armor, BodyPartType.Shin, BodyOrientation.Right, lowerBodyContainer.StackList[Wearables.LowerBodyRightShinIndex]);//right shin
						CreateWearableSquare(LowerBodySquaresParent, new Vector3(400f, -475f, -50f), WearableType.Jewelry, BodyPartType.Finger, BodyOrientation.Left, lowerBodyContainer.StackList[Wearables.LowerBodyLeftFingerIndex]);//left finger
						CreateWearableSquare(LowerBodySquaresParent, new Vector3(0f, -475f, -50f), WearableType.Jewelry, BodyPartType.Finger, BodyOrientation.Right, lowerBodyContainer.StackList[Wearables.LowerBodyRightFingerIndex]);//right finger

						StatButtons.Clear();
						BoxCollider b = null;
						foreach (Transform t in HeatProtection.transform.parent) {
								if (t.gameObject.HasComponent <BoxCollider>(out b)) {
										StatButtons.Add(b);
								}
						}

						mInitialized = true;

						Refresh();

				}

				protected InventorySquareWearable CreateWearableSquare(GameObject parentObject, Vector3 squarePosition, WearableType type, BodyPartType bodyPart, BodyOrientation orientation, WIStack stack)
				{
						//Debug.Log("Creating wearable square: " + type.ToString() + ", " + bodyPart.ToString() + ", " + orientation.ToString());

						GameObject inventorySquareGameObject = NGUITools.AddChild(parentObject, GUIManager.Get.InventorySquareWearable);
						InventorySquareWearable square = inventorySquareGameObject.GetComponent <InventorySquareWearable>();

						square.BodyPart = bodyPart;
						square.Orientation = orientation;
						square.Type = type;
						square.transform.localPosition = squarePosition;
						square.SetStack(stack);

						Squares.Add(square);

						stack.RefreshAction += Refresh;

						return square;
				}

				public void Refresh()
				{
						if (!mInitialized) {
								return;
						}

						mClothingChanged = true;
						for (int i = 0; i < Squares.Count; i++) {
								Squares[i].RefreshRequest();
						}
						//Debug.Log("Refreshing clothing");
						RefreshClothing.SafeInvoke();
				}

				protected bool mInitialized = false;

				public void Update()
				{
						if (!mInitialized) {
								return;
						}

						if (DisplayInfo) {
								if (UICamera.hoveredObject == null || UICamera.hoveredObject != CurrentInfoTarget) {
										DisplayInfo = false;
								}
								if (InfoSpriteShadow.alpha < 1f) {
										InfoSpriteShadow.alpha = Mathf.Lerp(InfoSpriteShadow.alpha, 1f, 0.25f);
										if (InfoSpriteShadow.alpha > 0.99f) {
												InfoSpriteShadow.alpha = 1f;
										}
								}
								//make sure the info doesn't overlay an icon
								mInfoOffset.x = CurrentInfoTargetXOffset;
								mInfoOffset.y = CurrentInfoTargetYOffset;
								InfoOffset.localPosition = mInfoOffset;
						} else {
								if (InfoSpriteShadow.alpha > 0f) {
										InfoSpriteShadow.alpha = Mathf.Lerp(InfoSpriteShadow.alpha, 0f, 0.25f);
										if (InfoSpriteShadow.alpha < 0.01f) {
												InfoSpriteShadow.alpha = 0f;
										}
								}
						}
						InfoLabel.alpha = InfoSpriteShadow.alpha;
						InfoSpriteBackground.alpha = InfoSpriteShadow.alpha;

						if (mClothingChanged || DisplayInfo) {
								mClothingChanged = false;
								//these take values from 0 to 1 where .5 will be neutral, or 0
								HeatProtection.color = Colors.BlendThree(
										Colors.Get.GenericLowValue,
										Colors.Get.GenericNeutralValue,
										Colors.Get.GenericHighValue,
										Player.Local.Wearables.NormalizedHeatProtection);

								ColdProtection.color = Colors.BlendThree(
										Colors.Get.GenericLowValue,
										Colors.Get.GenericNeutralValue,
										Colors.Get.GenericHighValue,
										Player.Local.Wearables.NormalizedHeatProtection);

								DamageProtection.color = Color.Lerp(
										Colors.Get.GenericNeutralValue,
										Colors.Get.GenericHighValue,
										Player.Local.Wearables.NormalizedDamageProtection);

								EnergyProtection.color = Colors.BlendThree(
										Colors.Get.GenericLowValue,
										Colors.Get.GenericNeutralValue,
										Colors.Get.GenericHighValue,
										Player.Local.Wearables.NormalizedEnergyProtection);

								VisibilityChange.color = Colors.BlendThree(//reverse the order
										Colors.Get.GenericHighValue,
										Colors.Get.GenericNeutralValue,
										Colors.Get.GenericLowValue,
										Player.Local.Wearables.NormalizedVisibilityChange);

								StrengthChange.color = Colors.BlendThree(
										Colors.Get.GenericLowValue,
										Colors.Get.GenericNeutralValue,
										Colors.Get.GenericHighValue,
										Player.Local.Wearables.NormalizedStrengthChange);

								for (int i = 0; i < ShadowSprites.Count; i++) {
										ShadowSprites[i].enabled = !VRManager.VRMode;
								}
						}
				}

				protected Vector3 mInfoOffset;
				protected bool mClothingChanged;
		}
}