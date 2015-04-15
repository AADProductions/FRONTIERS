using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using ExtensionMethods;
using Frontiers;
using Frontiers.GUI;
using Frontiers.World.WIScripts;

namespace Frontiers.World.Gameplay
{
	public class Examine : Skill
	{
				public static string GetExamineInfo (StackItem targetObject) {
						string examineString = string.Empty;
						List <WIExamineInfo> info = new List <WIExamineInfo>();
						Examinable examinable = null;
						if (targetObject.Is <Examinable>()){
								examineString = "Examine info for stack item";//examinable.State.StaticExamineMessage;
						} else {
								WIGlobalProps props = WorldItems.Get.GlobalPropsFromName(targetObject.PackName, targetObject.PrefabName);
								examineString = targetObject.DisplayName + "\n" + props.ExamineInfo.StaticExamineMessage;
						}
						return examineString;
				}

				public static string GetExamineInfo (IItemOfInterest targetObject) {
						string examineString = string.Empty;
						WorldItem worlditem = null;
						List <WIExamineInfo> info = new List <WIExamineInfo>();
						if (targetObject.IOIType == ItemOfInterestType.WorldItem) {
								worlditem = targetObject.worlditem;
								Examinable examinable = null;
								if (worlditem.Is <Examinable>(out examinable)) {
										examineString = examinable.State.StaticExamineMessage;
								} else {
										worlditem.Examine(info);
										//get all the examine info from each script
										//if we have enough skill, add it to the introspection
										if (info.Count == 0) {
												//TODO this sucks come up with a better line
												if (worlditem.Props.Global.MaterialType != WIMaterialType.None && worlditem.Props.Global.Flags.Size != WISize.NoLimit) {
														examineString = "It's " + Colors.ColorWrap(worlditem.Props.Global.Flags.Size.ToString(), Colors.Get.MessageSuccessColor)
														+ " and is made mostly of " + Colors.ColorWrap(worlditem.Props.Global.MaterialType.ToString(), Colors.Get.MessageSuccessColor);
												}
										}

										examineString = worlditem.DisplayName;

										for (int i = 0; i < info.Count; i++) {
												WIExamineInfo examineInfo = info[i];
												bool displaySuccess = true;
												if (!string.IsNullOrEmpty(examineInfo.RequiredSkill)) {
														displaySuccess = false;
														float skillUsageLevel = 0f;
														if (Skills.Get.HasLearnedSkill(examineInfo.RequiredSkill, out skillUsageLevel) && skillUsageLevel >= examineInfo.RequiredSkillUsageLevel) {
																displaySuccess = true;
														}
												}
												if (displaySuccess) {
														examineString = examineInfo.StaticExamineMessage;
												} else if (!string.IsNullOrEmpty(examineInfo.ExamineMessageOnFail)) {
														examineString = examineInfo.ExamineMessageOnFail;
												}
										}
								}
						}
						return examineString;
				}

		public override bool Use(IItemOfInterest targetObject, int flavorIndex)
		{
			WorldItem worlditem = null;
			List <string> introspectionStrings = new List <string>();
			List <WIExamineInfo> info = new List <WIExamineInfo>();
			int numLocationsRevealed = 0;
			if (targetObject.IOIType == ItemOfInterestType.WorldItem) {
				worlditem = targetObject.worlditem;
				Examinable examinable = null;
				if (worlditem.Is <Examinable>(out examinable)) {
					Frontiers.GUI.GUIIntrospectionDisplay.IntrospectionMessage message = new Frontiers.GUI.GUIIntrospectionDisplay.IntrospectionMessage();
					message.CenterText = false;
					message.Delay = 0f;
					message.FocusItem = worlditem;
					message.SkipOnLoseFocus = worlditem.Is(WIMode.World | WIMode.Frozen);
					message.LongForm = examinable.State.LongFormDisplay;
					message.Message = examinable.State.StaticExamineMessage;
					message.IconName = string.Empty;

					GUIManager.Get.NGUIIntrospectionDisplay.AddMessage(message, true);

					for (int i = 0; i < examinable.State.LocationsToReveal.Count; i++) {
						if (Player.Local.Surroundings.Reveal(examinable.State.LocationsToReveal[i])) {
							Profile.Get.CurrentGame.MarkedLocations.SafeAdd(examinable.State.LocationsToReveal[i]);
							numLocationsRevealed++;
						}
					}
				} else {
					worlditem.Examine(info);
					//get all the examine info from each script
					//if we have enough skill, add it to the introspection
					if (info.Count == 0) {
						//TODO this sucks come up with a better line
						if (worlditem.Props.Global.MaterialType != WIMaterialType.None && worlditem.Props.Global.Flags.Size != WISize.NoLimit) {
							introspectionStrings.Insert(0, "It's " + Colors.ColorWrap(worlditem.Props.Global.Flags.Size.ToString(), Colors.Get.MessageSuccessColor)
							+ " and is made mostly of " + Colors.ColorWrap(worlditem.Props.Global.MaterialType.ToString(), Colors.Get.MessageSuccessColor));
						}
					}

					introspectionStrings.Insert(0, worlditem.DisplayName);

					for (int i = 0; i < info.Count; i++) {
						WIExamineInfo examineInfo = info[i];
						bool displaySuccess = true;
						if (!string.IsNullOrEmpty(examineInfo.RequiredSkill)) {
							displaySuccess = false;
							float skillUsageLevel = 0f;
							if (Skills.Get.HasLearnedSkill(examineInfo.RequiredSkill, out skillUsageLevel) && skillUsageLevel >= examineInfo.RequiredSkillUsageLevel) {
								displaySuccess = true;
							}
						}
						if (displaySuccess) {
							if (examineInfo.LocationsToReveal.Count > 0) {
								for (int j = 0; j < examineInfo.LocationsToReveal.Count; j++) {
									if (Player.Local.Surroundings.Reveal(examineInfo.LocationsToReveal[j])) {
										Profile.Get.CurrentGame.MarkedLocations.SafeAdd(examineInfo.LocationsToReveal[j]);
										numLocationsRevealed++;
									}
								}
							}
							introspectionStrings.Add(examineInfo.StaticExamineMessage);
						} else if (!string.IsNullOrEmpty(examineInfo.ExamineMessageOnFail)) {
							introspectionStrings.Add(examineInfo.ExamineMessageOnFail);
						}
					}

					Frontiers.GUI.GUIIntrospectionDisplay.IntrospectionMessage message = new Frontiers.GUI.GUIIntrospectionDisplay.IntrospectionMessage();
					message.CenterText = false;
					message.Delay = 0f;
					message.FocusItem = worlditem;
					message.SkipOnLoseFocus = worlditem.Is(WIMode.World | WIMode.Frozen);
					message.Message = introspectionStrings.JoinToString("\n");
					message.LongForm = introspectionStrings.Count > 4 || message.Message.Length > 220;
					message.IconName = string.Empty;

					GUIManager.Get.NGUIIntrospectionDisplay.AddMessage(message, true);
				}
				if (numLocationsRevealed > 0) {
					GUIManager.PostSuccess(numLocationsRevealed.ToString() + " locations revealed on World Map");
					Player.Get.AvatarActions.ReceiveAction(AvatarAction.LocationReveal, WorldClock.AdjustedRealTime);
				}
				worlditem.OnExamine.SafeInvoke();
				return true;
			}
			return false;
		}
	}
}