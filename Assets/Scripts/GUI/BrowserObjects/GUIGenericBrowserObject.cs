using UnityEngine;
using System.Collections;

namespace Frontiers.GUI
{
		public class GUIGenericBrowserObject : GUIBrowserObjectBase
		{
				public override void Awake()
				{
						base.Awake();

						if (MiniIcon != null) {
								MiniIcon.enabled = false;
						}
						if (AttentionIcon != null) {
								AttentionIcon.enabled = false;
						}
				}

				public UILabel Name;
				public UISprite BackgroundHighlight;
				public UISprite Icon;
				public UISprite MiniIcon;
				public UISprite MiniIconBackground;
				public UISprite AttentionIcon;
				public UISprite IconBackround;
				public UISprite Background;
				public Color BackgroundColor;
				public Color GeneralColor;
				public UIButtonMessage EditButton;
				public bool UseAsDivider;
				public bool DeleteRequest = false;

				public void OnClickDeleteButton ( ) {
						Debug.Log("On click delete button was called on " + name);
						DeleteRequest = true;
						EditButton.SendMessage("OnClick");
				}

				public override void Initialize(string argument)
				{
						base.Initialize(argument);

						if (UseAsDivider) {
								if (BackgroundHighlight != null) {
										BackgroundHighlight.enabled = false;
								}
								if (IconBackround != null) {
										IconBackround.enabled = false;
								}
								if (MiniIcon != null) {
										MiniIcon.enabled = false;
										MiniIconBackground.enabled = false;
								}
								if (AttentionIcon != null) {
										AttentionIcon.enabled = false;
								}
								EditButton.enabled = false;
								Icon.enabled = false;
								Background.enabled = false;
								if (GetComponent<Collider>() != null) {
										GetComponent<Collider>().enabled = false;
								}
						}
				}
		}
}