using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.GUI
{
		public class GUITabButton : GUIObject, IComparable <GUITabButton>
		{
				public string Name;
				public string DisplayName;
				public bool Horizontal = true;
				public GUITabs TabParent;
				public GUITabPage Page;
				public UIButtonScale ButtonScale;
				public UIButton Button;
				public BoxCollider Collider;

				public bool Disabled {
						get {
								return mDisabled;
						}
						set {
								if (mDisabled != value) {
										mDisabled = value;
										Refresh();
								}
						}
				}

				public bool Initialized {
						get {
								return mInitialized;
						}
				}

				public bool Selected {
						get {
								return mSelected;
						}
						set {
								if (!mInitialized) {
										return;
								}
								mSelected = value;

								if (Page != null) {
										if (mSelected) {
												Page.OnSelected.SafeInvoke();
												/*
												if (!mSelected) {
													mSelected = value;
													Debug.Log ("Selecting tab button " + name);
													Page.OnSelected.SafeInvoke ();
												} else {
													Debug.Log ("Refreshing tab button " + name);
													Page.OnRefreshed.SafeInvoke ();
												}
												*/
										} else {
												Page.OnDeselected.SafeInvoke();
												/*
												if (mSelected) {
													mSelected = value;
													Debug.Log ("De-selecting tab button " + name);
													Page.OnDeselected.SafeInvoke ();
												} else {
													Debug.Log ("Refreshing tab button " + name);
													Page.OnRefreshed.SafeInvoke ();
												}
												*/
										}
								}
								RefreshRequest();
						}
				}

				public UILabel TabNameLabel;
				public UISprite TabSprite;
				public UISprite TabSpriteSelected;

				public override void Awake()
				{
						base.Awake();
						ButtonScale = gameObject.GetComponent <UIButtonScale>();
						Button = gameObject.GetComponent <UIButton>();
						ButtonScale.enabled = false;
						Button.enabled = false;
						Collider = gameObject.GetComponent<BoxCollider>();
				}

				public void Initialize(GUITabs tabParent, GUITabPage page)
				{
						mInitialized = true;
						Page = page;
						TabParent = tabParent;
						mSelected = true;
						Selected = false;
				}

				public void OnClickButton()
				{
						if (Disabled) {
								return;
						}

						TabParent.OnClickButton(this);
				}

				protected override void OnRefresh()
				{
						if (mDisabled) {
								TabSprite.enabled = false;
								TabNameLabel.enabled = false;
								if (Button != null) {
										Button.enabled = false;
								}
								return;
						} else {
								TabSprite.enabled = true;
								TabNameLabel.enabled = true;
								if (Button != null) {
										Button.enabled = true;
								}
						}

						TabSprite.transform.localRotation = Quaternion.identity;
						if (mSelected) {
								//TODO make it possible to swap out sprites
								//instead of this flipping business
								TabSprite.transform.Rotate(180f, 0f, 0f);
								if (ButtonScale != null) {
										ButtonScale.enabled = false;
								}
								if (Button != null) {
										Button.enabled = false;
								}
						} else {
								if (ButtonScale != null) {
										ButtonScale.enabled = true;
								}
								if (Button != null) {
										Button.enabled = true;
								}
						}
						if (!string.IsNullOrEmpty(DisplayName)) {
								TabNameLabel.text = DisplayName;
						} else {
								TabNameLabel.text = Data.GameData.AddSpacesToSentence(Name);
						}
				}

				void OnHover(bool isOver)
				{
						if (!mSelected && !mDisabled && !mIsPressed && isOver && Time.time > mNextHover) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonMouseOver");
						}
				}

				void OnPress(bool isPressed)
				{
						mIsPressed = isPressed;
						mNextHover = Time.time + 2.0f;
				}

				void OnClick()
				{
						if (!mSelected) {
								if (!mDisabled) {
										MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonClickEnabled");
								} else {
										MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonClickDisabled");
								}
						}
				}

				public override bool Equals(object obj)
				{
						if (obj == null) {
								return false;
						}

						GUITabButton other = obj as GUITabButton;

						if (other == null) {
								return false;
						}

						return (this == other);
				}

				public bool Equals(GUITabButton p)
				{
						if (p == null) {
								return false;
						}

						return (this == p);
				}

				public int CompareTo(GUITabButton other)
				{
						if (other == null) {
								return 0;
						}
						gComparePosThis = this.transform.localPosition;
						gComparePosOther = other.transform.localPosition;
						int thisPos = Mathf.FloorToInt(gComparePosThis.x);
						int otherPos = Mathf.FloorToInt(gComparePosOther.x);
						//check to see if we're horizontal
						if (thisPos == otherPos) {
								//we're vertical
								Horizontal = true;
								thisPos = Mathf.FloorToInt(gComparePosThis.y);
								otherPos = Mathf.FloorToInt(gComparePosOther.y);
								return otherPos.CompareTo(thisPos);
						} else {
								Horizontal = false;
						}
						return thisPos.CompareTo(otherPos);
				}

				public static Vector3 gComparePosThis;
				public static Vector3 gComparePosOther;

				public override int GetHashCode()
				{
						return Name.GetHashCode();
				}

				protected bool mStartupSet = false;
				protected bool mDisabled = false;
				protected bool mIsPressed = false;
				protected float mNextHover = 0.0f;
				protected bool mSelected = false;
		}
}
