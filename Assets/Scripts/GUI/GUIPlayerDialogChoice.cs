using UnityEngine;
using System.Collections;
using Frontiers.Story;
using Frontiers.Story.Conversations;

namespace Frontiers.GUI
{
		public class GUIPlayerDialogChoice : MonoBehaviour
		{
				public Exchange Choice;
				public float Offset;
				public UIButton ChoiceButton;
				public UIButtonMessage ChoiceButtonMessage;

				public void Start(){
						Transform tr = transform;
						tr.localPosition = new Vector3(tr.localPosition.x, Offset, tr.localPosition.z);
				}
		}
}