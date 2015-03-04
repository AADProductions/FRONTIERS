using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.GUI;

public class NGUIHUD : MonoBehaviour
{
	public UIFollowTarget FollowTarget = null;
	public static float gDistanceToScaleMultiplier	= 0.1f;
	public static float gStartShrinkingDistance = 2.0f;
	public static float gMinSize = 0.15f;
	public bool ForceActive = false;

	public void 					Update ()
	{
//		while (mElementsToRemove.Count > 0)
//		{
//			string removeElementOwner = mElementsToRemove.Dequeue ( );
//			GUIHudElement element = null;
//			if (mElements.TryGetValue (removeElementOwner, out element))
//			{
//				element.Deactivate ( );
//				mElements.Remove (removeElementOwner);
//			}
//		}

//		float distance = Vector3.Distance (Player.Local.HeadPosition, FollowTarget.target.position);
//		if (distance >= HUDManager.gMaxHUDDistance)
//		{
//			if (!ForceActive)
//			{
//				HUDManager.Get.RetireWorldItemHUD (this);
//			}
//			else
//			{
//				//something something, make it small
//			}
//			return;
//		}
//		float scale 						= Mathf.Clamp (distance - gStartShrinkingDistance, 0f, Mathf.Infinity) * gDistanceToScaleMultiplier;
//		scale								= 1.0f - Mathf.Max (scale, gMinSize);
//		gameObject.transform.localScale 	= Vector3.one * scale;

		float currentOffset = 0.0f;
		foreach (KeyValuePair <string, GameObject> widget in mWidgets) {
			GUIHudElement hudElement = null;
			if (mElements.TryGetValue (widget.Key, out hudElement)) {
				if (hudElement.Deactivated) {
					mElements.Remove (widget.Key);
					GameObject.Destroy (widget.Value);
				} else if (hudElement.Initialized) {
					GameObject widgetGo = widget.Value;
					switch (hudElement.Type) {
					case HudElementType.Label:
					default:
						if (hudElement.IsDirty) {
							UILabel label = widgetGo.GetComponent <UILabel> ();
							label.text = hudElement.LabelText;
							label.color = hudElement.LabelColor;
						}
						widgetGo.transform.localScale = Vector3.Lerp (widgetGo.transform.localScale, Vector3.one * 50.0f, 0.35f);
						break;

					case HudElementType.ProgressBar:
						if (hudElement.IsDirty) {
							UISlider slider = widgetGo.GetComponent <UISlider> ();
							slider.sliderValue	= hudElement.ProgressValue;
							UISlicedSprite fg	= slider.foreground.GetComponent <UISlicedSprite> ();
							UISlicedSprite bg	= slider.gameObject.FindOrCreateChild ("Background").GetComponent <UISlicedSprite> ();
							fg.color = hudElement.FGColor;
							bg.color = hudElement.BGColor;
//							UISlicedSprite ping	= widgetGo.FindOrCreateChild ("Ping").gameObject.GetOrAdd <UISlicedSprite> ( );
//							float pingValue		= hudElement.PingIntensity;
//							ping.color			= hudElement.PingColor;
						}
						widgetGo.transform.localScale = Vector3.Lerp (widgetGo.transform.localScale, Vector3.one, 0.15f);
						break;

					case HudElementType.Icon:
						if (hudElement.IsDirty) {
							GameObject iconSpriteGo = widgetGo.transform.FindChild ("IconSprite").gameObject;
							GameObject iconBGSpriteGo = widgetGo.transform.FindChild ("IconBackground").gameObject;
							UISprite iconSprite = iconSpriteGo.GetComponent <UISprite> ();
							UISprite iconBGSprite = iconBGSpriteGo.GetComponent <UISprite> ();
							iconSprite.atlas = Mats.Get.IconsAtlas;//TEMP
							iconSprite.spriteName = hudElement.HudIcon.IconName;
							iconSprite.color = hudElement.HudIcon.IconColor;
							iconBGSprite.color = hudElement.HudIcon.BGColor;
						}
						widgetGo.transform.localScale = Vector3.Lerp (widgetGo.transform.localScale, Vector3.one, 0.35f);
						break;
					}
					hudElement.TargetPosition = new Vector3 (0f, currentOffset, 0f);
					widgetGo.transform.localPosition	= Vector3.Lerp (widgetGo.transform.localPosition, hudElement.TargetPosition, 0.5f);
					currentOffset += hudElement.Dimensions.y;
				}
			} else {	//if it has no hud element, it's dead
				mWidgets.Remove (widget.Key);
				GameObject.Destroy (widget.Value);
			}
		}
	}

	public void Initialize (Transform target, Vector3 offset)
	{
		FollowTarget = gameObject.GetOrAdd <UIFollowTarget> ();
		FollowTarget.target = target;
		FollowTarget.offset = offset;
	}

	public GUIHudElement GetOrAddElement (HudElementType type, string owner)
	{
		GUIHudElement element = null;
		if (!mElements.TryGetValue (owner, out element)) {	//create element
			element = new GUIHudElement ();
			element.Name = owner;
			element.Type	= type;
			mElements.Add (owner, element);

			GameObject newObject = null;
			switch (type) {
			case HudElementType.Label:
			default:
				newObject = NGUITools.AddChild (gameObject, HUDManager.Get.NGUIHudLabelPrefab);
				////Debug.Log ("Added label " + owner);
				break;

			case HudElementType.ProgressBar:
				newObject = NGUITools.AddChild (gameObject, HUDManager.Get.NGUIHudProgressBarPrefab);
				////Debug.Log ("Added progress " + owner);
				break;

			case HudElementType.Icon:
				newObject = NGUITools.AddChild (gameObject, HUDManager.Get.NGUIHudIconPrefab);
				break;
			}
			newObject.name = owner;
			mWidgets.Add (owner, newObject);
		}
		return element;
	}

	public void						RemoveElement (string owner)
	{
		mElementsToRemove.Enqueue (owner);
	}

	protected Queue <string> mElementsToRemove = new Queue <string> ();
	protected Dictionary <string, GUIHudElement> mElements = new Dictionary <string, GUIHudElement> ();
	protected Dictionary <string, GameObject> mWidgets = new Dictionary <string, GameObject> ();
	protected bool mIsDirty = false;
}
