using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI
{
		//very old very ugly class
		//doesn't make good use of coroutines
		//needs to be rewritten from scratch pretty much
		public class GUIIntrospectionDisplay : SecondaryInterface
		{
				public UISprite BackgroundSprite;
				public UISprite BorderSprite;
				public UIPanel ClipPanel;
				public UILabel MessageLabel;
				public GUIHudMiniAction MiniAction;

				public Vector4 ClipTarget {
						get {
								if (HasMessages) {
										if (mCurrentMessage.LongForm) {
												return LongFormClipTarget;
										}
										return ShortFormClipTarget;
								}
								return InactiveClipTarget;
						}
				}

				public Vector3 ScaleTarget {
						get {
								if (HasMessages && mCurrentMessage.LongForm) {
										return LongFormScale;
								}
								return ShortFormScale;
						}
				}

				public Vector3 PositionTarget {
						get {
								if (HasMessages && mCurrentMessage.LongForm) {
										return LongFormPosition;
								}
								return ShortFormPosition;
						}
				}

				public UIPanel GainedSomethingIconPanel;
				public UISprite IconSprite;
				public UISlicedSprite IconBackground;
				public UISlicedSprite IconShadow;
				public Vector4 GainedSomethingIconPanelVisible = new Vector4(100f, 100f, 0f, 0f);
				public Vector4 GainedSomethingIconInvisible = new Vector4(0.1f, 0.1f, 0f, 0f);
				public Vector4 GainedSomethingIconPanelTarget;
				public Vector4 ShortFormClipTarget = new Vector4(0f, 0f, 1100f, 300f);
				public Vector4 LongFormClipTarget = new Vector4(0f, 0.1f, 1100f, 1100f);
				public Vector4 InactiveClipTarget = new Vector4(0f, 0.1f, 1100f, 0.1f);
				public Vector3 ShortFormPosition = new Vector3(0f, 160f, 0f);
				public Vector3 LongFormPosition = new Vector3(0f, 0f, 0f);
				public Vector3 ShortFormScale = new Vector3(1000f, 200f, 1f);
				public Vector3 LongFormScale = new Vector3(1000f, 1000f, 1f);
				public Vector3 ShortFormLabelPositionLeft = new Vector3(0f, 15f, -10f);
				public Vector3 ShortFormLabelPositionCenter = new Vector3(500f, 15f, -10f);
				public Vector3 LongFormLabelPositionLeft = new Vector3(0f, 15f, -10f);
				public Vector3 LongFormLabelPositionCenter = new Vector3(500f, 15f, -10f);
				public Vector3 ShortFormLabelPositionWithIcon = new Vector3(600f, 15f, -10f);
				public int ShortFormLineWidth = 600;
				public int LongFormLineWidth = 800;

				public bool HasMessages {
						get {
								return mQueuedMessages.Count > 0 || (!mCurrentMessage.IsEmpty && mCurrentMessageEndTime >= Frontiers.WorldClock.RealTime);
						}
				}

				public override void Start()
				{
						base.Start();
						//UserActions.Subscribe (UserActionType.ActionSkip, new ActionListener (ActionSkip));	
						UserActions.Filter = UserActionType.NoAction;// UserActionType.ActionCancel | UserActionType.ItemUse;//intercept all clicks and esc
						UserActions.FilterExceptions = UserActionType.NoAction;//UserActionType.FlagsMovement | UserActionType.LookAxisChange;

						MessageLabel.color = Colors.Get.MenuButtonTextColorDefault;
						mQueuedMessages.Clear();
						mCurrentMessage.IsEmpty = true;

						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalDie, new ActionListener(SurvivalDie));
				}

				public bool SurvivalDie(double timeStamp)
				{
						return ActionCancel(timeStamp);
				}

				public bool ActionSkip(double timeStamp)
				{
						if (HasMessages) {
								mCurrentMessage.Skip = true;
								mDelayForFirstMessage = 0.0;
								mCurrentMessageStartTime = 0.0;
						}
						return true;
				}

				public override bool ActionCancel(double timeStamp)
				{
						if (HasMessages) {
								mQueuedMessages.Clear();
								mCurrentMessageStartTime = 0.0;
								mDelayForFirstMessage = 0.0;
						}
						return base.ActionCancel(timeStamp);
				}

				public new void Update()
				{
						base.Update();

						BackgroundSprite.transform.localScale = Vector3.Lerp(BackgroundSprite.transform.localScale, ScaleTarget, 0.25f);
						BorderSprite.transform.localScale = BackgroundSprite.transform.localScale;

						ClipPanel.transform.localPosition = Vector3.Lerp(ClipPanel.transform.localPosition, PositionTarget, 0.25f);
						ClipPanel.clipRange = Vector4.Lerp(ClipPanel.clipRange, ClipTarget, 0.25f);

						GainedSomethingIconPanel.clipRange = Vector4.Lerp(GainedSomethingIconPanel.clipRange, GainedSomethingIconPanelTarget, 0.25f);

						if (!HasMessages) {
								GainedSomethingIconPanelTarget = GainedSomethingIconInvisible;
								return;
						}

						if (mDelayForFirstMessage > WorldClock.RealTime) {
								return;
						}

						if (mCurrentMessageEndTime < WorldClock.RealTime) {
								MessageLabel.text = string.Empty;
								MessageLabel.alpha = 1.0f;
								mCurrentMessage = mQueuedMessages.Dequeue();
								if (!string.IsNullOrEmpty(mCurrentMessage.MissionToActivate)) {
										Missions.Get.ActivateMission(mCurrentMessage.MissionToActivate, MissionOriginType.Introspection, string.Empty);
								}
								mCurrentMessageStartTime = WorldClock.RealTime;
								mDelayForFirstMessage = 0.0;

								if (mCurrentMessage.GainedSomething) {
										MessageLabel.pivot = UIWidget.Pivot.Left;
										MessageLabel.lineWidth = ShortFormLineWidth;
										MessageLabel.transform.localPosition = ShortFormLabelPositionWithIcon;
										GainedSomethingIconPanelTarget = GainedSomethingIconPanelVisible;

										if (!mCurrentMessage.UpdatedIcon) {
												//if we haven't updated the icon we haven't parsed scripts either
												mCurrentMessage.Message = Data.GameData.InterpretScripts(mCurrentMessage.Message, Profile.Get.CurrentGame.Character, null);

												//first update the prompt under the icon
												if (mCurrentMessage.Control != InControl.InputControlType.None) {
														if (!Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
																//key takes priority over mouse
																if (mCurrentMessage.Key != KeyCode.None) {
																		MiniAction.SetKey(mCurrentMessage.Key, mCurrentMessage.ActionDescription);
																} else {
																		//mouse
																		MiniAction.SetMouse(mCurrentMessage.Mouse, mCurrentMessage.ActionDescription);
																}
														} else {
																MiniAction.SetControl(mCurrentMessage.Control, mCurrentMessage.ActionDescription, InterfaceActionManager.ActionSpriteSuffix);
														}
												} else {
														//if there is no prompt, reset the prompt
														MiniAction.Reset();
												}


										//UUUUGH this is fucking disgusting
										switch (mCurrentMessage.Type) {
												case GainedSomethingType.Currency:
														IconSprite.atlas = Mats.Get.IconsAtlas;
														IconSprite.color = Colors.Get.MessageInfoColor;
														IconBackground.color = Colors.Get.SuccessHighlightColor;
														IconSprite.spriteName = "SkillIconGuildSellMap";
														break;

														case GainedSomethingType.MissionItem:
																IconSprite.atlas = Mats.Get.IconsAtlas;
																IconSprite.color = Colors.Get.MessageSuccessColor;
																IconBackground.color = Colors.Get.SuccessHighlightColor;
																IconSprite.spriteName = "MiscIconMissionItem";
																break;

												case GainedSomethingType.Mission:
														IconSprite.atlas = Mats.Get.IconsAtlas;
														MissionState missionState = null;
														if (Missions.Get.MissionStateByName(mCurrentMessage.GainedSomethingName, out missionState) && missionState.ObjectivesCompleted) {
																IconSprite.color = Colors.Get.MessageSuccessColor;
																IconBackground.color = Colors.Get.SuccessHighlightColor;
														} else {
																IconSprite.color = Colors.Get.MessageInfoColor;
																IconBackground.color = Colors.Get.SuccessHighlightColor;
														}
														IconSprite.spriteName = missionState.IconName;
														break;

												case GainedSomethingType.Book:
														IconSprite.atlas = Mats.Get.IconsAtlas;
														IconSprite.color = Colors.Get.MessageInfoColor;
														IconBackground.color = Colors.Get.SuccessHighlightColor;
														IconSprite.spriteName = "SkillIconGuildLibrary";
														break;

												case GainedSomethingType.Skill:
														IconSprite.atlas = Mats.Get.IconsAtlas;
														Skill skill = null;
														if (Skills.Get.SkillByName(mCurrentMessage.GainedSomethingName, out skill)) {
																IconSprite.color = skill.SkillIconColor;
																IconBackground.color = skill.SkillBorderColor;
														}
														IconSprite.spriteName = skill.Info.IconName;
														break;

												case GainedSomethingType.Structure:
														IconSprite.atlas = Mats.Get.MapIconsAtlas;
														IconSprite.color = Colors.Get.MessageSuccessColor;
														IconBackground.color = Colors.Get.SuccessHighlightColor;
														IconSprite.spriteName = "MapIconStructure";
														break;

												case GainedSomethingType.Blueprint:
														IconSprite.atlas = Mats.Get.IconsAtlas;
														WIBlueprint bp = null;
														if (Blueprints.Get.Blueprint(mCurrentMessage.GainedSomethingName, out bp)) {
																Skill bpSkill = null;
																if (Skills.Get.SkillByName(bp.RequiredSkill, out bpSkill)) {
																		IconSprite.spriteName = bpSkill.Info.IconName;
																		IconSprite.color = bpSkill.SkillIconColor;
																		IconBackground.color = bpSkill.SkillBorderColor;
																}
														}
														IconSprite.spriteName = "SkillIconCraftCreateBlueprint";
														break;

												default:
														break;
										}

										mCurrentMessage.UpdatedIcon = true;
								}

								} else {
										//update the visibility of the mini thing

										GainedSomethingIconPanelTarget = GainedSomethingIconInvisible;
										if (mCurrentMessage.CenterText) {
												if (mCurrentMessage.LongForm) {
														MessageLabel.pivot = UIWidget.Pivot.Center;
														MessageLabel.lineWidth = LongFormLineWidth;
														MessageLabel.transform.localPosition = LongFormLabelPositionCenter;
												} else {
														MessageLabel.pivot = UIWidget.Pivot.Top;
														MessageLabel.lineWidth = ShortFormLineWidth;
														MessageLabel.transform.localPosition = ShortFormLabelPositionCenter;
												}
										} else {
												MessageLabel.pivot = UIWidget.Pivot.Left;
												if (mCurrentMessage.LongForm) {
														MessageLabel.lineWidth = LongFormLineWidth;
														MessageLabel.transform.localPosition = LongFormLabelPositionLeft;
												} else {
														MessageLabel.lineWidth = ShortFormLineWidth;
														MessageLabel.transform.localPosition = ShortFormLabelPositionLeft;
												}
										}
								}
								return;
						}

						if (mCurrentMessage.SkipOnLoseFocus) {
								if (mCurrentMessage.FocusItem == null) {
										mCurrentMessage.Skip = true;
								} else {
										if (Player.Local.Surroundings.IsWorldItemInPlayerFocus) {
												if (Player.Local.Surroundings.WorldItemFocus != mCurrentMessage.FocusItem) {
														mCurrentMessage.Skip = true;
												}
										} else {
												mCurrentMessage.Skip = true;
										}
								}
						}
			
						MessageLabel.text = mCurrentMessage.Message;

						/*if (mCurrentMessageFullDisplayTime < Frontiers.WorldClock.RealTime) {
								MessageLabel.text = mCurrentMessage.Message.Trim();
						} else {
								double normalizedDisplayAmount = 1.0 - ((Frontiers.WorldClock.RealTime - mCurrentMessageFullDisplayTime) / (mCurrentMessageStartTime - mCurrentMessageFullDisplayTime));
								int stringDisplayLength = Mathf.FloorToInt((float)(mCurrentMessage.Message.Length * normalizedDisplayAmount));
								string subMessageFront = mCurrentMessage.Message.Substring(0, stringDisplayLength).Trim();
								MessageLabel.text = subMessageFront;
								return;
						}*/
			
						if (mCurrentMessageStartFadeTime < Frontiers.WorldClock.RealTime) {
								MessageLabel.alpha = (float)(1.0 - ((Frontiers.WorldClock.RealTime - mCurrentMessageStartFadeTime) / (mCurrentMessageEndTime - mCurrentMessageStartFadeTime)));
								return;
						} else {
								MessageLabel.alpha = 1.0f;
						}
						MiniAction.SetAlpha(MessageLabel.alpha);
				}

				public void AddMessage(string newMessage, double delay, Frontiers.World.IItemOfInterest focusItem, bool longForm, bool force, bool skipOnLoseFocus)
				{
						if (force) {
								while (mQueuedMessages.Count > 0) {
										IntrospectionMessage clearedMessage = mQueuedMessages.Dequeue();
										if (!string.IsNullOrEmpty(clearedMessage.MissionToActivate)) {
												Missions.Get.ActivateMission(clearedMessage.MissionToActivate, MissionOriginType.Introspection, string.Empty);
										}
								}
								mCurrentMessageStartTime = 0.0;
								mDelayForFirstMessage = 0.0;
						}

						IntrospectionMessage newGainedSomethingMessage = new IntrospectionMessage();
						newGainedSomethingMessage.Message = newMessage;
						newGainedSomethingMessage.Delay = delay;
						newGainedSomethingMessage.SkipOnLoseFocus = skipOnLoseFocus;
						newGainedSomethingMessage.FocusItem = focusItem;
						newGainedSomethingMessage.LongForm = longForm;

						if (mQueuedMessages.Count == 0) {
								mDelayForFirstMessage = Frontiers.WorldClock.RealTime + delay;
								MessageLabel.text = string.Empty;
						}

						mQueuedMessages.Enqueue(newGainedSomethingMessage);
				}

				public void AddMessage(IntrospectionMessage newMessage, bool force)
				{
						if (force) {
								while (mQueuedMessages.Count > 0) {
										IntrospectionMessage clearedMessage = mQueuedMessages.Dequeue();
										if (!string.IsNullOrEmpty(clearedMessage.MissionToActivate)) {
												Missions.Get.ActivateMission(clearedMessage.MissionToActivate, MissionOriginType.Introspection, string.Empty);
										}
								}
								mCurrentMessageStartTime = 0.0;
								mDelayForFirstMessage = 0.0;
						}

						if (mQueuedMessages.Count == 0) {
								mDelayForFirstMessage = Frontiers.WorldClock.RealTime;
								MessageLabel.text = string.Empty;
						}

						mQueuedMessages.Enqueue(newMessage);
				}

				public void AddMessage(string newMessage, double delay, string missionToActivate, bool force)
				{
						////Debug.Log ("Adding message " + newMessage + " missionToActivate: " + missionToActivate);
						if (force) {
								while (mQueuedMessages.Count > 0) {
										IntrospectionMessage clearedMessage = mQueuedMessages.Dequeue();
										if (!string.IsNullOrEmpty(clearedMessage.MissionToActivate)) {
												Missions.Get.ActivateMission(clearedMessage.MissionToActivate, MissionOriginType.Introspection, string.Empty);
										}
								}
								mCurrentMessageStartTime = 0.0;
								mDelayForFirstMessage = 0.0;
						}
			
						if (mQueuedMessages.Count == 0) {
								mDelayForFirstMessage = Frontiers.WorldClock.RealTime + delay;
								MessageLabel.text = string.Empty;
						}

						mQueuedMessages.Enqueue(new IntrospectionMessage(false, delay, newMessage, missionToActivate, false));
				}

				public void AddLongFormMessage(string newMessage, bool centerText)
				{
						//TEMP
						if (!HasMessages) {
								MessageLabel.text = string.Empty;
						}
						mQueuedMessages.Enqueue(new IntrospectionMessage(true, 0f, newMessage, string.Empty, centerText));
				}

				public void AddLongFormMessage(string newMessage, bool centerText, bool force)
				{
						if (force) {
								while (mQueuedMessages.Count > 0) {
										IntrospectionMessage clearedMessage = mQueuedMessages.Dequeue();
										if (!string.IsNullOrEmpty(clearedMessage.MissionToActivate)) {
												Missions.Get.ActivateMission(clearedMessage.MissionToActivate, MissionOriginType.Introspection, string.Empty);
										}
								}
								mCurrentMessageStartTime = 0.0;
								mDelayForFirstMessage = 0.0;
						}

						if (mQueuedMessages.Count == 0) {
								mDelayForFirstMessage = Frontiers.WorldClock.RealTime;
								MessageLabel.text = string.Empty;
						}

						mQueuedMessages.Enqueue(new IntrospectionMessage(true, 0f, newMessage, string.Empty, centerText));
				}

				public void AddGainedSomethingMessage(string newMessage, double delay, string gainedSomethingName, GainedSomethingType gainedSomethingType, InterfaceActionType action, string actionDescription)
				{
						IntrospectionMessage newGainedSomethingMessage = new IntrospectionMessage();
						newGainedSomethingMessage.Message = newMessage;
						newGainedSomethingMessage.Delay = delay;
						newGainedSomethingMessage.GainedSomethingName = gainedSomethingName;
						newGainedSomethingMessage.UpdatedIcon = false;
						newGainedSomethingMessage.Type = gainedSomethingType;

						if (mQueuedMessages.Count == 0) {
								mDelayForFirstMessage = Frontiers.WorldClock.RealTime + delay;
								MessageLabel.text = string.Empty;
						}

						if (action != InterfaceActionType.NoAction) {
								newGainedSomethingMessage.ActionDescription = actionDescription;
								newGainedSomethingMessage.Control = InterfaceActionManager.Get.GetActionBinding((int)action);
								if (newGainedSomethingMessage.Control != InControl.InputControlType.None) {
										InterfaceActionManager.Get.GetKeyBinding(newGainedSomethingMessage.Control, ref newGainedSomethingMessage.Key);
										InterfaceActionManager.Get.GetMouseBinding(newGainedSomethingMessage.Control, ref newGainedSomethingMessage.Mouse);
								}
						}
						mQueuedMessages.Enqueue(newGainedSomethingMessage);
				}

				protected double mCurrentMessageStartTime = 0.0;

				protected double mCurrentMessageHoldTime {
						get {
								return Mathf.Clamp((float)(mCurrentMessage.Message.Length * mSecondsPerCharacter), 3.0f, 6.0f) * Profile.Get.CurrentPreferences.Accessibility.OnScreenTextSpeed;
						}
				}

				protected double mCurrentMessageStartFadeTime {
						get {
								return mCurrentMessageEndTime - mFadeDuration;
						}
				}

				protected double mCurrentMessageFullDisplayTime {
						get {
								return mCurrentMessageStartTime + mRollingDisplayDuration;
						}
				}

				protected double mCurrentMessageEndTime {
						get {
								if (mCurrentMessage.Skip) {
										return 0.0;
								} else if (mCurrentMessage.SkipOnLoseFocus) {
										return Mathf.Infinity;
								}
								return mCurrentMessageStartTime + mRollingDisplayDuration + mCurrentMessageHoldTime;
						}
				}

				protected double mFadeDuration = 0.35f;
				protected double mRollingDisplayDuration = 0.5f;
				protected double mSecondsPerCharacter = 0.25f;
				protected IntrospectionMessage mCurrentMessage;
				protected Queue <IntrospectionMessage> mQueuedMessages = new Queue <IntrospectionMessage>();
				protected double mDelayForFirstMessage = 0.0;

				public struct IntrospectionMessage
				{
						public IntrospectionMessage(bool longForm, double delay, string message, string missionToActivate, bool centerText)
						{
								Skip = false;
								LongForm = longForm;
								Delay = delay;
								Message = message;
								MissionToActivate = missionToActivate;
								CenterText = centerText;

								SkipOnLoseFocus = false;
								FocusItem = null;

								Type = GainedSomethingType.None;
								IconName = null;
								GainedSomethingName = null;

								UpdatedIcon = false;

								Key = KeyCode.None;
								Mouse = ActionSetting.MouseAction.None;
								Control = InControl.InputControlType.None;
								ActionDescription = string.Empty;
						}

						public bool	Skip;

						public bool IsEmpty {
								get {
										return string.IsNullOrEmpty(Message);
								}
								set {
										if (value) {
												Message = string.Empty;
										}
								}
						}

						public bool SkipOnLoseFocus;
						public Frontiers.World.IItemOfInterest FocusItem;
						public bool LongForm;
						public double Delay;
						public string Message;
						public string MissionToActivate;
						public bool CenterText;

						public KeyCode Key;
						public ActionSetting.MouseAction Mouse;
						public InControl.InputControlType Control;
						public string ActionDescription;

						public bool GainedSomething {
								get {
										return Type != GainedSomethingType.None;
								}
						}

						public bool UseIcon {
								get {
										return !string.IsNullOrEmpty(IconName);
								}
						}

						public string IconName;
						public GainedSomethingType Type;
						public string GainedSomethingName;
						public bool UpdatedIcon;
				}
		}
}
