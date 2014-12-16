using UnityEngine;
using System.Collections;
using Frontiers.World;

namespace Frontiers.GUI
{
		public class GUIDetailsPage : MonoBehaviour
		{
				public static GUIDetailsPage Get;
				public UIPanel Panel;
				public UIPanel ScrollingPanel;
				public UIScrollBar ScrollBar;
				public UILabel NameLabel;
				public UISprite Icon;
				public UISprite IconBackground;
				public UILabel DetailTextLabel;
				public Vector4 ClippingClosed;
				public Vector4 ClippingOpen;
				public Vector4 ClippingTarget;
				public GenericWorldItem DopplegangerProps = GenericWorldItem.Empty;
				public GameObject Doppleganger;
				public GameObject DopplegangerButton;
				public UILabel DopplegangerButtonLabel;
				public GameObject DopplegangerButtonTarget;
				public string DopplegangerButtonMessage;
				public bool Visible = false;
				public bool ShowDoppleganger = false;
				public Transform DopplegangerParent;
				public float DopplegangerScale = 1f;
				public Collider DopplegangerBoundsCollider;
				FrontiersInterface LastUser;

				public void Start()
				{
						Get = this;
						NameLabel.text = string.Empty;
						DetailTextLabel.text = string.Empty;
				}

				public void DisplayDetail(FrontiersInterface user, string name, string detailText, string iconName, UIAtlas iconAtlas, Color iconColor, Color iconBackgroundColor, GenericWorldItem dopplegangerProps)
				{
						ScrollBar.scrollValue = 0f;
						Show();
						NameLabel.text = name;
						DetailTextLabel.text = detailText;
						if (string.IsNullOrEmpty(iconName) || iconAtlas == null) {
								Icon.enabled = false;
								IconBackground.enabled	= false;
						} else {
								Icon.enabled = true;
								IconBackground.enabled	= true;
								Icon.atlas = iconAtlas;
								Icon.spriteName = iconName;
								Icon.color = iconColor;
								IconBackground.color = iconBackgroundColor;
						}
						if (dopplegangerProps != null) {
								ShowDoppleganger = true;
								DopplegangerProps.CopyFrom(dopplegangerProps);
						} else {
								ShowDoppleganger = false;
						}
						RefreshDoppleganger();
						LastUser = user;
				}

				public void DisplayDetail(FrontiersInterface user, string name, string detailText, string iconName, UIAtlas iconAtlas, Color iconColor, Color iconBackgroundColor)
				{
						ScrollBar.scrollValue = 0f;
						Show();
						NameLabel.text = name;
						DetailTextLabel.text = detailText;
						if (string.IsNullOrEmpty(iconName) || iconAtlas == null) {
								Icon.enabled = false;
								IconBackground.enabled	= false;
						} else {
								Icon.enabled = true;
								IconBackground.enabled	= true;
								Icon.atlas = iconAtlas;
								Icon.spriteName = iconName;
								Icon.color = iconColor;
								IconBackground.color = iconBackgroundColor;
						}
						ShowDoppleganger = false;
						RefreshDoppleganger();
						LastUser = user;
				}

				public void DisplayDopplegangerButton(string buttonLabel, string buttonMessage, GameObject buttonTarget)
				{
						DopplegangerButton.SetActive(true);
						DopplegangerButtonLabel.text = buttonLabel;
						DopplegangerButtonTarget = buttonTarget;
						DopplegangerButtonMessage = buttonMessage;
				}

				public void OnClickDopplegangerButton()
				{
						if (DopplegangerButtonTarget != null) {
								DopplegangerButtonTarget.SendMessage(DopplegangerButtonMessage, SendMessageOptions.DontRequireReceiver);
						}
				}

				public void RefreshDoppleganger()
				{
						if (Visible && ShowDoppleganger) {
								Doppleganger = WorldItems.GetDoppleganger(DopplegangerProps, DopplegangerParent, Doppleganger, WIMode.Stacked);
								WorldItems.FitDopplegangerToBounds(DopplegangerParent, Doppleganger, DopplegangerBoundsCollider.bounds);
						} else {
								WorldItems.ReturnDoppleganger(Doppleganger);
						}
						DopplegangerParent.transform.localEulerAngles = Vector3.zero;
				}

				public void Show()
				{
						Panel.enabled = true;
						Visible = true;
						ScrollingPanel.enabled = true;
						enabled = true;
						RefreshDoppleganger();
				}

				public void Hide()
				{
						LastUser = null;
						Visible = false;
						Panel.enabled = false;
						ScrollingPanel.enabled = false;
						enabled = false;
						DetailTextLabel.text = string.Empty;
						Icon.enabled = false;
						IconBackground.enabled = false;
						DopplegangerButton.SetActive(false);
						RefreshDoppleganger();
				}

				public void Update()
				{
						if ((LastUser == null || !LastUser.HasFocus)) {
								Hide();
						} else {
								DopplegangerParent.transform.Rotate(0f, 0.25f, 0f);
						}
				}
		}
}