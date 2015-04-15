using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;

namespace Frontiers.World.WIScripts
{
		public class Wearables : WIScript
		{
				public WearablesState State = new WearablesState();
				public WorldBody Body;

				public override void OnInitialized()
				{
						State.UpperBodyContainer = Stacks.Create.StackContainer(worlditem, worlditem.Group);
						State.LowerBodyContainer = Stacks.Create.StackContainer(worlditem, worlditem.Group);
				}

				public static int GetWearableIndex(BodyPartType onBodyPart, BodyOrientation orientation, ref bool upperBody)
				{
						int index = -1;
						switch (onBodyPart) {
								default:
										break;
						//upper body
								case BodyPartType.Head:
										index = Wearables.UpperBodyHeadIndex;
										break;

								case BodyPartType.Face:
										index = Wearables.UpperBodyFaceIndex;
										break;

								case BodyPartType.Neck:
										index = Wearables.UpperBodyNeckIndex;
										break;

								case BodyPartType.Chest:
										index = Wearables.UpperBodyChestIndex;
										break;

								case BodyPartType.Shoulder:
										if (orientation == BodyOrientation.Left) {
												index = Wearables.UpperBodyLeftShoulderIndex;
										} else {
												index = Wearables.UpperBodyRightShoulderIndex;
										}
										break;

								case BodyPartType.Arm:
										if (orientation == BodyOrientation.Left) {
												index = Wearables.UpperBodyLeftArmIndex;
										} else {
												index = Wearables.UpperBodyRightArmIndex;
										}
										break;

								case BodyPartType.Hand:
										if (orientation == BodyOrientation.Left) {
												index = Wearables.UpperBodyLeftHandIndex;
										} else {
												index = Wearables.UpperBodyRightHandIndex;
										}
										break;
										//lower body

								case BodyPartType.Hip:
										index = Wearables.LowerBodyHipIndex;
										upperBody = false;
										break;

								case BodyPartType.Leg:
										if (orientation == BodyOrientation.Left) {
												index = Wearables.LowerBodyLeftKneeIndex;
										} else {
												index = Wearables.LowerBodyRightKneeIndex;
										}
										upperBody = false;
										break;

								case BodyPartType.Shin:
										if (orientation == BodyOrientation.Left) {
												index = Wearables.LowerBodyLeftShinIndex;
										} else {
												index = Wearables.LowerBodyRightShinIndex;
										}
										upperBody = false;
										break;

								case BodyPartType.Foot:
										if (orientation == BodyOrientation.Left) {
												index = Wearables.LowerBodyLeftShinIndex;
										} else {
												index = Wearables.LowerBodyRightShinIndex;
										}
										upperBody = false;
										break;

								case BodyPartType.Finger:
										if (orientation == BodyOrientation.Left) {
												index = Wearables.LowerBodyLeftFingerIndex;
										} else {
												index = Wearables.LowerBodyRightFingerIndex;
										}
										upperBody = false;
										break;
						}
						return index;
				}

				public static int UpperBodyHeadIndex = 0;
				public static int UpperBodyFaceIndex = 1;
				public static int UpperBodyLeftShoulderIndex = 2;
				public static int UpperBodyRightShoulderIndex = 3;
				public static int UpperBodyLeftArmIndex = 4;
				public static int UpperBodyRightArmIndex = 5;
				public static int UpperBodyNeckIndex = 6;
				public static int UpperBodyChestIndex = 7;
				public static int UpperBodyLeftHandIndex = 8;
				public static int UpperBodyRightHandIndex = 9;
				public static int LowerBodyHipIndex = 0;
				public static int LowerBodyLeftKneeIndex = 2;
				public static int LowerBodyRightKneeIndex = 3;
				public static int LowerBodyLeftFootIndex = 4;
				public static int LowerBodyRightFoodIndex = 5;
				public static int LowerBodyLeftShinIndex = 6;
				public static int LowerBodyRightShinIndex = 7;
				public static int LowerBodyLeftFingerIndex = 8;
				public static int LowerBodyRightFingerIndex = 9;
		}

		[Serializable]
		public class WearablesState
		{
				public WIStackContainer UpperBodyContainer;
				public WIStackContainer LowerBodyContainer;
		}
}
