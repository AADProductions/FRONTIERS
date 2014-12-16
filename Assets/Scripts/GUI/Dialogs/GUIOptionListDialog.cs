using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.GUI
{
		public class GUIOptionListDialog : GUIEditor <OptionsListDialogResult>
		{
				public Transform ScreenTarget;
				public Camera ScreenTargetCamera;
				public UILabel MessageType;
				public UILabel Message;
				public Light InventoryLight;
				public List <UIButton> ButtonPrototypes = new List <UIButton>();
				public UIPanel OptionButtonsPanel;
				public List <UIButton> OptionButtons;
				public GUIOptionListDivider DividerPrototype = new GUIOptionListDivider();
				public GenericWorldItem DopplegangerProps = GenericWorldItem.Empty;
				public WIMode DopplegangerMode = WIMode.Stacked;
				public GameObject Doppleganger;
				public Transform DopplegangerParent;
				public float TargetLightIntensity = 1.0f;
				protected float StartPosition = 25.0f;
				protected float RowSize = 55.0f;

				public override void WakeUp()
				{			
						InventoryLight.intensity = 0f;
						HideCrosshair = true;
						UserActions.Filter = UserActionType.FlagsAll;
						UserActions.Behavior = PassThroughBehavior.InterceptByFocus;
						UserActions.FilterExceptions = UserActionType.NoAction;
				}

				public void RefreshDoppleganger()
				{
						if (!DopplegangerProps.IsEmpty) {
								InventoryLight.enabled = true;
								TargetLightIntensity = 1f;
								Doppleganger = WorldItems.GetDoppleganger(DopplegangerProps, DopplegangerParent, Doppleganger, DopplegangerMode);
						} else {
								InventoryLight.enabled = false;
								WorldItems.ReturnDoppleganger(Doppleganger);
						}
				}

				public override void PushEditObjectToNGUIObject()
				{		
						MessageType.text = EditObject.MessageType;
						Message.text = EditObject.Message;

						float buttonPosition = StartPosition;
						int numButtonsCreated = 0;
						numButtonsCreated += CreateButtons(EditObject.Options, ref buttonPosition, "OnClickOptionButton");
						numButtonsCreated += CreateButtons(EditObject.SecondaryOptions, ref buttonPosition, "OnClickSecondaryOptionButton");

						transform.localPosition = mEditObject.PositionTarget;

						if (numButtonsCreated == 0) {
								CreateButton(new GUIListOption("(No options available)", "Cancel"), -1, buttonPosition, "OnClickOptionButton");
						}

						RefreshDoppleganger();
				}

				protected int CreateButtons(List <GUIListOption> options, ref float buttonPosition, string functionName)
				{
						int numButtonsCreated = 0;
						foreach (GUIListOption option in options) {
								////Debug.Log ("Creating buttons");
								if (option.HasFlavors) {
										for (int flavorIndex = 0; flavorIndex < option.Flavors.Count; flavorIndex++) {
												if (CreateButton(option, flavorIndex, buttonPosition, functionName)) {
														numButtonsCreated++;
														buttonPosition -= RowSize;
												}
										}
								} else if (CreateButton(option, -1, buttonPosition, functionName)) {
										numButtonsCreated++;
										buttonPosition -= RowSize;
								}
						}
						return numButtonsCreated;
				}

				protected bool CreateButton(GUIListOption option, int flavorIndex, float buttonPosition, string functionName)
				{
						if (option.IsValid) {
								UIButton randomPrototype = ButtonPrototypes[UnityEngine.Random.Range(0, ButtonPrototypes.Count)];
								UIButton newButton = NGUITools.AddChild(OptionButtonsPanel.gameObject, randomPrototype.gameObject).GetComponent <UIButton>();

								UISprite newButtonBackground = newButton.transform.FindChild("Background").GetComponent <UISprite>();
								UISprite newButtonSelection = newButton.transform.FindChild("Selection").GetComponent <UISprite>();
								UILabel newButtonLabel = newButton.transform.FindChild("Label").GetComponent <UILabel>();
								UISprite overlaySprite = newButton.transform.FindChild("Overlay").GetComponent <UISprite>();

								newButton.hover = Colors.Get.GeneralHighlightColor;
								newButton.defaultColor = Colors.Darken(Colors.Get.GeneralHighlightColor, 0f);
								newButtonSelection.color = Colors.Darken(Colors.Get.GeneralHighlightColor, 0f);
								newButtonLabel.effectColor = Colors.Get.MenuButtonTextOutlineColor;
								newButtonLabel.effectStyle = UILabel.Effect.Shadow;//UILabel.Effect.Outline;

								newButtonLabel.text = option.OptionText;
								newButton.name = option.Result;
								newButtonLabel.color = option.TextColor;
				
								if (flavorIndex >= 0) {
										if (string.IsNullOrEmpty(option.Flavors[flavorIndex])) {
												return false;
										} else {
												newButtonLabel.text = option.Flavors[flavorIndex];
												newButton.name += "_" + flavorIndex.ToString();
										}
								}
				
								if (option.Disabled) {
										newButton.SendMessage("SetDisabled");
										newButton.enabled = false;
										newButtonBackground.color = Colors.Disabled(option.BackgroundColor);
										newButtonLabel.color = Colors.Disabled(option.TextColor);
										newButton.hover = Colors.Disabled(newButton.hover);
										overlaySprite.color = option.OverlayColor;
									
										UIButtonScale newButtonScale = newButton.GetComponent <UIButtonScale>();
										newButtonScale.enabled = false;
								} else {
										newButton.SendMessage("SetEnabled");
										newButtonBackground.color = option.BackgroundColor;
										newButtonLabel.color = option.TextColor;
										overlaySprite.color = option.OverlayColor;
					
										UIButtonMessage newButtonMessage = newButton.GetComponent <UIButtonMessage>();
										newButtonMessage.target = this.gameObject;
										newButtonMessage.functionName = functionName;
								}
				
								newButton.transform.localPosition	= new Vector3(0.0f, buttonPosition, 0.0f);

								Transform credentialsTransform = newButton.transform.FindChild("Credentials");
								if (string.IsNullOrEmpty(option.CredentialsIconName)) {
										credentialsTransform.gameObject.SetActive(false);
								} else {
										Transform spriteTransform = credentialsTransform.FindChild("CredentialsSprite");
										Transform spriteBackgroundTransform	= credentialsTransform.transform.FindChild("CredentialsBackground");
										UISprite sprite = spriteTransform.gameObject.GetComponent <UISprite>();
										UISprite backgroundSprite = spriteBackgroundTransform.gameObject.GetComponent <UISprite>();
										sprite.spriteName = option.CredentialsIconName;
										sprite.color = newButtonBackground.color;
										backgroundSprite.enabled = true;
										backgroundSprite.color = option.IconColor;
								}


								Transform iconTransform = newButton.transform.FindChild("Icon");
								if (string.IsNullOrEmpty(option.IconName)) {
										iconTransform.gameObject.SetActive(false);
								} else {
										Transform spriteTransform = iconTransform.FindChild("IconSprite");
										Transform spriteBackgroundTransform	= iconTransform.transform.FindChild("IconBackground");
										Transform negateIconTransform = iconTransform.transform.FindChild("NegateIcon");
										UISprite negateIconSprite = negateIconTransform.gameObject.GetComponent <UISprite>();
										UISprite sprite = spriteTransform.gameObject.GetComponent <UISprite>();
										UISprite backgroundSprite = spriteBackgroundTransform.gameObject.GetComponent <UISprite>();
										sprite.spriteName = option.IconName;
										sprite.color = option.IconColor;
										backgroundSprite.enabled = true;
										backgroundSprite.color = option.BackgroundColor;
										if (option.Disabled) {
												sprite.color = Colors.Blacken(sprite.color);
										}
										if (option.NegateIcon) {
												negateIconSprite.enabled = true;
										} else {
												negateIconSprite.enabled = false;
										}
										backgroundSprite.color = option.BackgroundColor;
								}

								return true;
						}
						return false;
				}

				public virtual void OnClickOptionButton(GameObject sender)
				{
						EditObject.Result = sender.name;
						Finish();
				}

				public virtual void OnClickSecondaryOptionButton(GameObject sender)
				{
						//check for flavors
						string[ ] splitSenderName = sender.name.Split('_');
						if (splitSenderName.Length > 1) {
								EditObject.SecondaryResult = splitSenderName[0];
								EditObject.SecondaryResultFlavor	= int.Parse(splitSenderName[1]);
						} else {
								EditObject.SecondaryResult = sender.name;
								EditObject.SecondaryResultFlavor	= -1;
						}
						Finish();
				}

				public override bool ActionCancel(double timeStamp)
				{
						if (!mFinished) {
								if (EditObject.ForceChoice) {
										//don't allow the dialog to quit before making a choice
										return true;
								}
								EditObject.Result = "Cancel";
						}
						return base.ActionCancel(timeStamp);
				}

				public override void Update()
				{
						base.Update();

						InventoryLight.intensity = Mathf.Lerp(InventoryLight.intensity, TargetLightIntensity, 0.25f);

						if (ScreenTarget != null) {
								Camera targetCamera = NGUICamera;
								if (ScreenTargetCamera != null) {
										targetCamera = ScreenTargetCamera;
								}
								Vector2 screenPoint = targetCamera.WorldToScreenPoint(ScreenTarget.position);
								Vector3 worldpoint = NGUICamera.ScreenToWorldPoint(screenPoint);
								worldpoint.z = transform.position.z;
								transform.position = worldpoint;
						}
				}
		}

		public class GUIListOption
		{

				#region constructors

				public static GUIListOption Empty {
						get {
								if (mEmpty == null) {
										mEmpty = new GUIListOption();
										mEmpty.IsValid = false;
								}
								return mEmpty;
						}
				}

				protected static GUIListOption mEmpty;

				public GUIListOption()
				{
						Divider = false;
						CredentialsIconName = string.Empty;
						IconName = string.Empty;
						OptionText = "Option";
						TextColor = Color.white;
						BackgroundColor = Color.white;
						Result = "Result";
				}

				public GUIListOption(string optionText)
				{
						Divider = false;
						IconName = string.Empty;
						CredentialsIconName = string.Empty;
						OptionText = optionText;
						TextColor = Colors.Get.MenuButtonTextColorDefault;
						BackgroundColor = Colors.Get.MenuButtonBackgroundColorDefault;
						OverlayColor = Colors.Get.MenuButtonOverlayColorDefault;
						Result = optionText;
				}

				public GUIListOption(bool divider, string dividerText, Color textColor)
				{
						Divider = true;
						OptionText = dividerText;
						CredentialsIconName = string.Empty;
						TextColor = textColor;
						BackgroundColor = Colors.Get.MenuButtonBackgroundColorDefault;
						OverlayColor = Colors.Get.MenuButtonOverlayColorDefault;
				}

				public GUIListOption(string iconName, string optionText, Color textColor, Color backgroundColor, string result)
				{
						Divider = false;
						IconName = iconName;
						CredentialsIconName = string.Empty;
						OptionText = optionText;
						TextColor = textColor;
						BackgroundColor = backgroundColor;
						OverlayColor = Colors.Get.MenuButtonOverlayColorDefault;
						Result = result;
				}

				public GUIListOption(string iconName, string optionText, Color textColor, string result)
				{
						Divider = false;
						IconName = iconName;
						CredentialsIconName = string.Empty;
						OptionText = optionText;
						TextColor = textColor;
						BackgroundColor = Colors.Get.MenuButtonBackgroundColorDefault;
						OverlayColor = Colors.Get.MenuButtonOverlayColorDefault;
						Result = result;
				}

				public GUIListOption(string optionText, Color textColor, Color backgroundColor, string result)
				{
						Divider = false;
						IconName = string.Empty;
						CredentialsIconName = string.Empty;
						OptionText = optionText;
						TextColor = textColor;
						BackgroundColor = backgroundColor;
						OverlayColor = Colors.Get.MenuButtonOverlayColorDefault;
						Result = result;
				}

				public GUIListOption(string optionText, Color textColor, string result)
				{
						Divider = false;
						IconName = string.Empty;
						CredentialsIconName = string.Empty;
						OptionText = optionText;
						TextColor = textColor;
						BackgroundColor = Colors.Get.MenuButtonBackgroundColorDefault;
						OverlayColor = Colors.Get.MenuButtonOverlayColorDefault;
						Result = result;
				}

				public GUIListOption(string iconName, string optionText, string result)
				{
						Divider = false;
						IconName = iconName;
						CredentialsIconName = string.Empty;
						OptionText = optionText;
						TextColor = Colors.Get.MenuButtonTextColorDefault;
						BackgroundColor = Colors.Get.MenuButtonBackgroundColorDefault;
						OverlayColor = Colors.Get.MenuButtonOverlayColorDefault;
						Result = result;
				}

				public GUIListOption(string optionText, string result)
				{
						Divider = false;
						IconName = string.Empty;
						CredentialsIconName = string.Empty;
						OptionText = optionText;
						TextColor = Colors.Get.MenuButtonTextColorDefault;
						BackgroundColor = Colors.Get.MenuButtonBackgroundColorDefault;
						OverlayColor = Colors.Get.MenuButtonOverlayColorDefault;
						Result = result;
				}

				#endregion

				public bool IsValid {
						get {
								if (Divider) {
										return true;
								}

								if (!mIsValid) {
										return false;
								}

								if (string.IsNullOrEmpty(Result) || string.IsNullOrEmpty(OptionText)) {
										return false;
								}

								return true;
						}
						set {
								mIsValid = value;
						}
				}

				public bool HasFlavors {
						get {
								return (Flavors.Count > 0);
						}
				}

				public bool Divider = false;
				public bool Disabled = false;
				public string IconName = string.Empty;
				public string CredentialsIconName	= string.Empty;
				public string OptionText = string.Empty;
				public Color TextColor = Colors.Get.MenuButtonTextColorDefault;
				public Color BackgroundColor = Colors.Get.MenuButtonBackgroundColorDefault;
				public Color OverlayColor = Colors.Get.MenuButtonOverlayColorDefault;
				public Color IconColor = Color.white;
				public bool NegateIcon = false;
				public string Result = "Result";
				public List <string> Flavors = new List <string>();
				protected bool mIsValid = true;

				public static bool IsNullOrInvalid(GUIListOption option)
				{
						return (option == null || !option.IsValid);
				}
		}

		public class OptionsListDialogResult
		{
				public string MessageType = "Question";
				public string Message = "Dialog Question";
				public bool ForceChoice = false;
				public List <GUIListOption> Options = new List <GUIListOption>();
				public List <GUIListOption> SecondaryOptions = new List <GUIListOption>();
				public string Result = string.Empty;
				public string SecondaryResult = string.Empty;
				public int SecondaryResultFlavor = -1;
				public Vector3 PositionTarget = Vector3.zero;
		}
}