using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.GUI
{
		public class GUIOptionListDialog : GUIEditor <WIListResult>
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
				public GenericWorldItem DopplegangerProps = new GenericWorldItem();
				public WIMode DopplegangerMode = WIMode.Stacked;
				public GameObject Doppleganger;
				public Transform DopplegangerParent;
				public float TargetLightIntensity = 1.0f;
				protected float StartPosition = 25.0f;
				protected float RowSize = 55.0f;

				public override Widget FirstInterfaceObject {
						get {
								Widget w = new Widget();
								if (OptionButtons.Count > 0) {
										w.SearchCamera = NGUICamera;
										w.BoxCollider = OptionButtons[0].GetComponent<BoxCollider>();
								}
								return w;
						}
				}

				public override void WakeUp()
				{			
						base.WakeUp();

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
								CreateButton(new WIListOption("(No options available)", "Cancel"), -1, buttonPosition, "OnClickOptionButton");
						}

						RefreshDoppleganger();
				}

				protected int CreateButtons(List <WIListOption> options, ref float buttonPosition, string functionName)
				{
						int numButtonsCreated = 0;
						foreach (WIListOption option in options) {
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

				protected bool CreateButton(WIListOption option, int flavorIndex, float buttonPosition, string functionName)
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

								Transform currencyTransform = newButton.transform.FindChild("Currency");
								if (option.RequiresCurrency) {
										Transform currencyLabelTransform = currencyTransform.Find("CurrencyLabel");
										Transform currencyDopplegangerParent = currencyTransform.Find("CurrencyDopplegangerParent");
										UILabel currencyLabel = currencyLabelTransform.gameObject.GetComponent <UILabel>();
										string inventoryItemName = string.Empty;
										GenericWorldItem currencyDopplegangerProps = null;
										switch (option.RequiredCurrencyType) {
												case WICurrencyType.A_Bronze:
												default:
														currencyDopplegangerProps = Currency.BronzeGenericWorldItem;
														inventoryItemName = Currency.BronzeCurrencyNamePlural;
														break;

												case WICurrencyType.B_Silver:
														currencyDopplegangerProps = Currency.SilverGenericWorldItem;
														inventoryItemName = Currency.SilverCurrencyNamePlural;
														break;

												case WICurrencyType.C_Gold:
														currencyDopplegangerProps = Currency.GoldIGenericWorldItem;
														inventoryItemName = Currency.GoldCurrencyNamePlural;
														break;

												case WICurrencyType.D_Luminite:
														currencyDopplegangerProps = Currency.LumenGenericWorldItem;
														inventoryItemName = Currency.LumenCurrencyNamePlural;
														break;

												case WICurrencyType.E_Warlock:
														currencyDopplegangerProps = Currency.WarlockGenericWorldItem;
														inventoryItemName = Currency.WarlockCurrencyNamePlural;
														break;
										}
										currencyLabel.text = option.CurrencyValue.ToString() + " " + inventoryItemName;
										GameObject currencyDoppleganger = WorldItems.GetDoppleganger(currencyDopplegangerProps, currencyDopplegangerParent, null, WIMode.Stacked, 1f);
								} else {
										currencyTransform.gameObject.SetActive(false);
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
}