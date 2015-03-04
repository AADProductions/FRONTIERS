using UnityEngine;
using System.Collections;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.GUI {
	public class GUIAvailablePathMarkerObject : MonoBehaviour
	{
		public UISlicedSprite Background;
		public UILabel DestinationLabel;
		public UILabel PathLabel;
		public UISprite TypeIcon;
		public GameObject FunctionTarget;
		public UIButton Button;
		public Location PathMarker;
		public float YOffset;
		public float YSize;
		public int Index;

		public void Refresh ()
		{
			mTargetPosition = new Vector3 (0f, YOffset + (YSize * Index), -15f);
			
			DestinationLabel.text = PathMarker.worlditem.DisplayName;
			Background.alpha = 0.5f;
			UIButtonMessage[] messages = Button.gameObject.GetComponents <UIButtonMessage> ();
			foreach (UIButtonMessage message in messages) {
				message.target = FunctionTarget;
			}
			TypeIcon.spriteName = "MapIcon" + PathMarker.State.Type.ToString ();
		}

		public void Update ()
		{
			transform.localPosition = Vector3.Lerp (transform.localPosition, mTargetPosition, 0.25f);
		}

		protected Vector3 mTargetPosition;
	}
}