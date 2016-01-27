//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Editable text input field.
/// </summary>

[AddComponentMenu("NGUI/UI/Input (Basic)")]
public class UIInputRaw : MonoBehaviour
{
		static public UIInputRaw current;
		public UILabel label;
		public int maxChars = 0;
		public string caratChar = "|";
		public bool isPassword = false;
		public Color activeColor = Color.white;
		public GameObject eventReceiver;
		public string functionName = "OnSubmit";
		public string functionNameEnter = "OnSubmitWithEnter";
		string mText = "";
		string mDefaultText = "";
		Color mDefaultColor = Color.white;
		#if UNITY_IPHONE || UNITY_ANDROID
		#if UNITY_3_4
	iPhoneKeyboard mKeyboard;

#else
	TouchScreenKeyboard mKeyboard;
#endif
		#else
		string mLastIME = "";
		#endif
		/// <summary>
		/// Input field's current text value.
		/// </summary>
		/// 

		public string text {
				get {
						return mText;
				}
				set {
						mText = value;

						if (label != null) {
								if (string.IsNullOrEmpty(value))
										value = mDefaultText;

								label.supportEncoding = true;
								label.text = selected ? value + ("[FF00FF]" + caratChar + "[-]") : value;
								label.showLastPasswordChar = selected;
								label.color = (selected || value != mDefaultText) ? activeColor : mDefaultColor;
						}
				}
		}

		/// <summary>
		/// Whether the input is currently selected.
		/// </summary>

		public bool selected {
				get {
						return UICamera.selectedObject == gameObject;
				}
				set {
						if (!value && UICamera.selectedObject == gameObject)
								UICamera.selectedObject = null;
						else if (value)
								UICamera.selectedObject = gameObject;
				}
		}

		/// <summary>
		/// Labels used for input shouldn't support color encoding.
		/// </summary>

		protected void Init()
		{
				if (label == null)
						label = GetComponentInChildren<UILabel>();
				if (label != null) {
						mDefaultText = label.text;
						mDefaultColor = label.color;
						label.supportEncoding = true;
				}
		}

		/// <summary>
		/// Initialize everything on awake.
		/// </summary>

		void Awake()
		{
				gameObject.tag = Globals.TagGuiInputObject;
				Init();
		}

		/// <summary>
		/// If the object is currently highlighted, it should also be selected.
		/// </summary>

		void OnEnable()
		{
				if (UICamera.IsHighlighted(gameObject))
						OnSelect(true);
		}

		/// <summary>
		/// Remove the selection.
		/// </summary>

		void OnDisable()
		{
				if (UICamera.IsHighlighted(gameObject))
						OnSelect(false);
		}

		/// <summary>
		/// Selection event, sent by UICamera.
		/// </summary>

		void OnSelect(bool isSelected)
		{
				if (label != null && enabled && gameObject.activeSelf) {
						if (isSelected) {
								mText = (label.text == mDefaultText) ? "" : label.text;
								label.color = activeColor;
								if (isPassword)
										label.password = true;

#if UNITY_IPHONE || UNITY_ANDROID
				if (Application.platform == RuntimePlatform.IPhonePlayer ||
					Application.platform == RuntimePlatform.Android)
				{
#if UNITY_3_4
					mKeyboard = iPhoneKeyboard.Open(mText);
#else
					mKeyboard = TouchScreenKeyboard.Open(mText);
#endif
				}
				else
#endif
								{
										Input.imeCompositionMode = IMECompositionMode.On;
										Transform t = label.cachedTransform;
										Vector3 offset = label.pivotOffset;
										offset.y += label.relativeSize.y;
										offset = t.TransformPoint(offset);
										Input.compositionCursorPos = UICamera.currentCamera.WorldToScreenPoint(offset);
										UpdateLabel();
								}
						}
#if UNITY_IPHONE || UNITY_ANDROID
			else if (mKeyboard != null)
			{
				mKeyboard.active = false;
			}
#endif
			else {
								if (string.IsNullOrEmpty(mText)) {
										label.text = mDefaultText;
										label.color = mDefaultColor;
										if (isPassword)
												label.password = false;
								} else
										label.text = mText;

								label.showLastPasswordChar = false;
								Input.imeCompositionMode = IMECompositionMode.Off;
						}
				}
		}
		#if UNITY_IPHONE || UNITY_ANDROID
	/// <summary>
	/// Update the text and the label by grabbing it from the iOS/Android keyboard.
	/// </summary>

	void Update()
	{
		if (mKeyboard != null)
		{
			string text = mKeyboard.text;

			if (mText != text)
			{
				mText = text;
				UpdateLabel();
			}

			if (mKeyboard.done)
			{
				mKeyboard = null;
				current = this;
				if (eventReceiver == null) eventReceiver = gameObject;
				eventReceiver.SendMessage(functionName, SendMessageOptions.DontRequireReceiver);
				current = null;
				selected = false;
			}
		}
	}

#else
		void Update()
		{
				if (Frontiers.InterfaceActionManager.AvailableKeyDown) {
					if (selected && enabled && gameObject.activeSelf) {
						if (eventReceiver == null)
							eventReceiver = gameObject;

						mText = Frontiers.InterfaceActionManager.LastKey.ToString();
						SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
						eventReceiver.SendMessage(functionName, SendMessageOptions.DontRequireReceiver);

						// Ensure that we don't exceed the maximum length
						UpdateLabel();
					}
				}

				if (mLastIME != Input.compositionString) {
						mLastIME = Input.compositionString;
						UpdateLabel();
				}
		}
		#endif
		/// <summary>
		/// Input event, sent by UICamera.
		/// </summary>		
		void OnInput(string input) {
				return;
		}

		/// <summary>
		/// Update the visual text label, capping it at maxChars correctly.
		/// </summary>

		void UpdateLabel()
		{
				if (maxChars > 0 && mText.Length > maxChars)
						mText = mText.Substring(0, maxChars);

				if (label.font != null) {
						// Start with the text and append the IME composition and carat chars
						string processed = selected ? (mText + Input.compositionString) : mText;

						// Now wrap this text using the specified line width
						processed = label.font.WrapText(processed, label.lineWidth / label.cachedTransform.localScale.x, true, false);

						if (!label.multiLine) {
								// Split it up into lines
								string[] lines = processed.Split(new char[] { '\n' });

								// Only the last line should be visible
								processed = (lines.Length > 0) ? lines[lines.Length - 1] : "";
						}
						// Update the label's visible text
						label.text = processed + ("[FF00FF]" + caratChar + "[-]");
						label.showLastPasswordChar = selected;
				}
		}
}