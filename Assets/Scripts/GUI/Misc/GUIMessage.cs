using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUIMessage : MonoBehaviour
		{
				//public UISprite Icon;
				public UILabel Message;
				public UILabel StackLabel;
				public UISlicedSprite Shadow;
				public GUIMessageDisplay.Type Type;
				public Color MessageColor = Color.black;
				public double TimeSent;
				public double HoldInterval = 4.0f;
				public double FadeInterval = 1.0f;
				public double FadeVelocity = 1.0f;
				public float FadeCurrent = 1.0f;
				public float ShadowFadeMultiplier = 0.5f;
				public int StackNumber = 0;
				public string OriginalMessage = string.Empty;
				public float Height = 0f;
				protected static float mShadowAlpha = 0.1175f;
				protected static float mShadowOffset = 12.5f;

				public void Initialize(GUIMessageDisplay.Type type, string message)
				{
						//gameObject.transform.localScale = Vector3.one * 0.01f;
						FadeCurrent = 1.0f;
						TimeSent = WorldClock.RealTime;

						TargetOffset = 0f;
						OriginalMessage = message;
						Type = type;
						Vector3 messageSize = Message.relativeSize * Message.transform.localScale.x;
						Height = messageSize.y;

						StackLabel.text = string.Empty;
						switch (Type) {
								case GUIMessageDisplay.Type.Danger:
										MessageColor = Colors.Get.MessageDangerColor;
										break;

								case GUIMessageDisplay.Type.Info:
										MessageColor = Colors.Get.MessageInfoColor;
										break;

								case GUIMessageDisplay.Type.Warning:
										MessageColor = Colors.Get.MessageWarningColor;
										break;

								case GUIMessageDisplay.Type.Success:
										MessageColor = Colors.Get.MessageSuccessColor;
										break;

								default:
										MessageColor = Message.color;
										break;
						}

						//Icon.enabled = true;
						//Shadow.transform.localScale = new Vector3 (Shadow.transform.localScale.x, YScale, Shadow.transform.localScale.z);
						Vector3 shadowScale = messageSize;

						Transform shadowTrans = Shadow.transform;
						Transform textTrans = Message.transform;
						Vector3 offset = textTrans.localPosition;
						Vector3 textScale = textTrans.localScale;

						// Calculate the dimensions of the printed text
						Vector3 size = Message.relativeSize;

						// Scale by the transform and adjust by the padding offset
						size.x *= textScale.x;
						size.y *= textScale.y;
						size.x += Shadow.border.x + Shadow.border.z + (offset.x - Shadow.border.x) * 2f;
						size.y += Shadow.border.y + Shadow.border.w + (-offset.y - Shadow.border.y) * 2f;
						size.z = 1f;

						shadowTrans.localScale = size;

						Message.text = '[' + Colors.ColorToHex(MessageColor) + ']' + OriginalMessage;
				}

				public bool StacksWith(GUIMessageDisplay.Type type, string message)
				{
						if (ReadyToRemove) {
								return false;	
						}
			
						if (type == Type && message == OriginalMessage) {
								StackNumber++;
								TimeSent = WorldClock.RealTime;
								FadeCurrent = 1.0f;
								UpdateStackLabel();
								return true;
						}
			
						return false;
				}

				protected void UpdateStackLabel()
				{
						if (StackNumber > 0) {
								StackLabel.alpha = 1.0f;
								StackLabel.text = '[' + Colors.ColorToHex(MessageColor) + "](" + (StackNumber + 1).ToString() + ')';
						} else {
								StackLabel.alpha = 0.0f;
						}
				}

				public void UpdateFadeAndPosition()
				{
						if (WorldClock.RealTime > (TimeSent + HoldInterval)) {
								FadeCurrent = Mathf.Lerp(FadeCurrent, 0f, 0.25f);
						}
						Message.alpha = FadeCurrent;
						StackLabel.alpha = FadeCurrent;

						Vector3 currentPosition = transform.localPosition;
						Vector3 targetPosition = currentPosition;
						targetPosition.y = TargetOffset;
						transform.localPosition = Vector3.Lerp(currentPosition, targetPosition, 0.5f);

						Shadow.alpha = FadeCurrent * mShadowAlpha;
				}

				public bool ReadyToRemove {
						get {
								return (WorldClock.RealTime > (TimeSent + HoldInterval) && FadeCurrent < 0.0025f);
						}
				}

				public float TargetOffset;
		}
}