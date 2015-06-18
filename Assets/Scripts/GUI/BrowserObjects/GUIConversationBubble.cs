using UnityEngine;
using System.Collections;
using Frontiers;
using System;

namespace Frontiers.GUI
{
	[ExecuteInEditMode]
	public class GUIConversationBubble : MonoBehaviour
	{
		public Color BackgroundColor;
		public Color TextColor;
		public UILabel Text;
		public UILabel CharacterName;
		public UISprite BackgroundSprite;
		public UISprite OverlaySprite;
		public UISprite OverlayHighlightSprite;
		public UISprite TextureSprite;
		public UISprite ShadowSprite;
		public UIButton OverlayButton;
		public BubbleStyle Style;
		public bool EnableAutomatically = false;
		public bool ObexFont = false;
		public float Alpha = 0f;
		public float CharacterNameAlpha = 0f;
		public float Height;
		public float FadeDelay;
		public bool FadedColor = false;
		public string TargetText;
		public string TargetCharacterName;
		public Color TargetBackgroundColor;
		public Color TargetTextColor;
		public bool ResetPosition = false;
		public UIScrollBar TargetScrollbar;
		public BoxCollider Collider;
		protected float mShadowAlpha = 0.35f;
		protected float mOverlayAlpha = 0.5f;
		protected float mTextureAlpha = 0.5f;
		protected bool mObexFont = false;
		public float MaxScrollTargetSize = 0f;
		public float ScrollTarget = 1f;

		public bool ReadyToBeClicked {
			get {
				return !mSettingProps;
			}
		}

		public bool IsDestroying {
			get {
				return mDestroying;
			}
		}

		public enum BubbleStyle
		{
			LeftBot,
			RightBot,
			LeftBotNone,
		}

		public void Awake ()
		{
			mPivotOnStartup = Text.pivot;
		}

		public void Start ()
		{ 
			ResetPosition = false;
		}

		public void OnEnable ()
		{
			if (Application.isPlaying) {
				OverlayButton.tweenTarget = OverlayHighlightSprite.gameObject;
				Alpha = 0f;
				CharacterNameAlpha = 0f;
				RefreshColor ();
				Text.depth = 25;
				OverlayHighlightSprite.depth = 30;
			}

			Collider = gameObject.GetComponent<BoxCollider> ();
			if (Collider != null) {
				Collider.enabled = false;
			}
		}

		public void RefreshColor ()
		{
			if (Colors.Get == null) {
				return;
			}

			try {
				if (FadedColor) {
					BackgroundSprite.color = Colors.Alpha (Colors.Char (Colors.Desaturate (BackgroundColor)), Alpha);
					Text.color = Colors.Alpha (Colors.Darken (TextColor), Alpha);
				} else {
					BackgroundSprite.color = Colors.Alpha (Colors.Darken (BackgroundColor), Alpha);
					Text.color = Colors.Alpha (TextColor, Alpha);
				}

				if (CharacterName.text != TargetCharacterName) {
					CharacterNameAlpha = Alpha;
				} else {
					CharacterNameAlpha = Mathf.Max (CharacterName.alpha, Alpha);
				}

				Text.effectColor = Colors.Alpha (Colors.Char (TextColor), Alpha);
				CharacterName.color = Colors.Alpha (Colors.Brighten (BackgroundColor), CharacterNameAlpha);
				OverlayHighlightSprite.color = Colors.Alpha (Colors.Get.GeneralHighlightColor, 0f);

				TextureSprite.color = Colors.Alpha (TextureSprite.color, Alpha * mTextureAlpha);
				ShadowSprite.color = Colors.Alpha (ShadowSprite.color, Alpha * mShadowAlpha);
				OverlaySprite.color = Colors.Alpha (BackgroundSprite.color, Alpha * mOverlayAlpha);

				OverlayButton.defaultColor = Colors.Alpha (BackgroundSprite.color, 0f);
				OverlayButton.hover = Colors.Alpha (Colors.Get.GeneralHighlightColor, 0.5f);
				OverlayButton.pressed = Colors.Alpha (BackgroundSprite.color, 0f);
			} catch (Exception e) {
				Debug.Log ("Exception in convo bubble, ignoring: " + e.ToString ());
			}
		}

		public void Refresh ()
		{
			RefreshColor ();
			RefreshSize ();
		}

		public void Update ()
		{
			if (!Application.isPlaying) {
				return;
			}
			if (ResetPosition) {
				if (BackgroundSprite.transform.localScale.y > MaxScrollTargetSize) {
					//Debug.Log("Background sprite was larger than max scroll target size, so setting scroll to zero");
					//it's going to exceed the size of the panel
					TargetScrollbar.scrollValue = 0f;
				} else {
					//Debug.Log("Background sprite was smaller, setting scroll to target scroll");
					//move the object to 1/2 its height
					TargetScrollbar.scrollValue = ScrollTarget;
				}
				TargetScrollbar.ForceUpdate ();
				ResetPosition = false;
			}
			enabled = false;
		}

		public void SetProps (string text, Color bgColor, Color textColor)
		{
			SetProps (text, string.Empty, bgColor, textColor, false, null, false);
		}

		public void SetProps (string text, string characterName, Color bgColor, Color textColor)
		{
			SetProps (text, characterName, bgColor, textColor, false, null, false);
		}

		public void SetProps (string text, string characterName, Color bgColor, Color textColor, bool obex)
		{
			SetProps (text, characterName, bgColor, textColor, false, null, obex);
		}

		public void SetProps (string text, Color bgColor, Color textColor, bool resetPosition, UIScrollBar targetScrollbar)
		{
			SetProps (text, string.Empty, bgColor, textColor, resetPosition, targetScrollbar, false);
		}

		public void SetProps (string text, string characterName, Color bgColor, Color textColor, bool resetPosition, UIScrollBar targetScrollbar) {
			SetProps (text, characterName, bgColor, textColor, resetPosition, targetScrollbar, false);
		}

		public void SetProps (string text, string characterName, Color bgColor, Color textColor, bool resetPosition, UIScrollBar targetScrollbar, bool obexFont)
		{
			if (mDestroying) {
				//Debug.Log("Setting props on a destroyed convo bubble, returning");
				return;
			}

			ObexFont = obexFont;

			mSetPropsOnce = true;

			if (Collider != null) {
				Collider.enabled = false;
			}
			if (OverlayButton != null) {
				OverlayButton.enabled = false;
			}
			TargetText = text;
			TargetCharacterName = characterName;
			TargetBackgroundColor = bgColor;
			TargetTextColor = textColor;
			ResetPosition = resetPosition;
			TargetScrollbar = targetScrollbar;
			if (obexFont) {
				Text.font = Mats.Get.ObexFont;
				CharacterName.font = Mats.Get.ObexFont;
			}

			if (!mSettingProps) {
				mSettingProps = true;
				StartCoroutine (SetPropsOverTime ());
			}
		}

		public void Clear ()
		{
			TargetText = string.Empty;
			TargetCharacterName = string.Empty;
			StartCoroutine (SetPropsOverTime ());
		}

		public void DestroyBubble ()
		{
			if (!mDestroying) {
				mDestroying = true;
				StartCoroutine (DestroyBubbleOverTime ());
			}
		}

		protected IEnumerator DestroyBubbleOverTime ()
		{
			OverlayButton.enabled = false;
			TargetCharacterName = string.Empty;//do this so it'll fade

			while (Alpha > 0f) {
				Alpha = Mathf.Lerp (Alpha, 0f, 0.5f);
				if (Alpha < 0.01f) {
					Alpha = 0f;
				}
				RefreshColor ();
				yield return null;
			}
			RefreshColor ();
			GameObject.Destroy (gameObject);
			gameObject.SetActive (false);
			yield break;
		}

		protected IEnumerator SetPropsOverTime ()
		{
			while (Alpha > 0f) {
				Alpha = Mathf.Lerp (Alpha, 0f, 0.75f);
				if (Alpha < 0.01f) {
					Alpha = 0f;
				}
				RefreshColor ();
				yield return null;
			}
			RefreshColor ();

			if (string.IsNullOrEmpty (TargetText)) {
				//we're done
				mSettingProps = false;
				yield break;
			} else {
				Text.text = TargetText;
				CharacterName.text = TargetCharacterName;
				BackgroundColor = TargetBackgroundColor;
				TextColor = TargetTextColor;
				Refresh ();
				double waitUntil = WorldClock.RealTime + FadeDelay;
				while (WorldClock.RealTime < waitUntil) {
					yield return null;
				}
			}

			if (Collider != null && EnableAutomatically) {
				Collider.enabled = true;
			}
			if (OverlayButton != null) {
				OverlayButton.enabled = true;
			}

			while (Alpha < 1f) {
				Alpha = Mathf.Lerp (Alpha, 1f, 0.5f);
				if (Alpha > 0.98f) {
					Alpha = 1f;
				}
				RefreshColor ();
				yield return null;
			}
			RefreshColor ();
			mSettingProps = false;
			yield break;
		}

		protected void RefreshSize ()
		{
			//update the box around the text to reflect its size
			Transform textTrans = Text.transform;
			Vector3 offset = textTrans.localPosition;
			Vector3 textScale = textTrans.localScale;

			// Calculate the dimensions of the printed text
			Vector3 size = Text.relativeSize;

			// Scale by the transform and adjust by the padding offset
			size.x *= textScale.x;
			size.y *= textScale.y;
			size.x += (BackgroundSprite.border.x + BackgroundSprite.border.z + (offset.x - BackgroundSprite.border.x) * 2f);
			size.y += (BackgroundSprite.border.y + BackgroundSprite.border.w + (-offset.y - BackgroundSprite.border.y) * 2f);
			size.z = 1f;
			size.x = BackgroundSprite.transform.localScale.x;

			if (Collider != null) {
				Collider.size = size;
				Collider.center = new Vector3 (size.x / 2f, -size.y / 2f, -10f);
			}

			Height = size.y;

			BackgroundSprite.transform.localScale = size;
			OverlayHighlightSprite.transform.localScale = size;
			OverlaySprite.transform.localScale = size;
			TextureSprite.transform.localScale = size;

			ShadowSprite.transform.localScale = new Vector3 (size.x + 25f, size.y + 25f, 1f);
			ShadowSprite.transform.localPosition = new Vector3 ((size.x / 2f) + 25f, (-size.y / 2f) - 25f, 0f);

			//now that we've set the sizes of things
			//set the anchor and position
			if (VRManager.VRMode) {
				Vector3 positionWithOffset = textTrans.localPosition;
				if (mPivotOnStartup == UIWidget.Pivot.TopLeft) {
					positionWithOffset.x = (size.x / 2);
				} else if (mPivotOnStartup == UIWidget.Pivot.TopRight) {
					positionWithOffset.x = (size.x / 2);
				}
				Text.pivot = UIWidget.Pivot.Top;
				Debug.Log ("Setting position with offset: " + positionWithOffset.ToString ());
				textTrans.localPosition = positionWithOffset;
			} else if (Text.pivot != mPivotOnStartup) {
				Text.pivot = mPivotOnStartup;
				//don't bother with the position it's already fine
			}

			if (ResetPosition) {
				//Debug.Log("We've been asked to reset our position");
				transform.localPosition = new Vector3 (0f, Height / 2f, 0f);
				TargetScrollbar.scrollValue = ScrollTarget;
				TargetScrollbar.ForceUpdate ();
				enabled = true;
			}
		}

		protected bool mSetPropsOnce = false;
		protected bool mSettingProps = false;
		protected bool mDestroying = false;
		protected UILabel.Pivot mPivotOnStartup = UILabel.Pivot.Left;
	}
}