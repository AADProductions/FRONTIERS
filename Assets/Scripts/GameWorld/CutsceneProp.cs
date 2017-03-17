using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
		public class CutsceneProp : MonoBehaviour
		{
				public string AnimationOnStart;
				public string AnimationOnIdleStart;
				public string AnimationOnIdleEnd;
				public bool RequireMissionVariable = false;
				public string MissionName;
				public string VariableName;
				public int VariableValue = 0;
				public bool Visible {
						get {
								if (Cutscene.CurrentCutscene.VRMode) {
										if (!VisibleInVRMode) {
												return false;
										}
								} else if (!VisibleInStandardMode) {
										return false;
								}
								return true;
						}
				}
				public bool VisibleInVRMode = true;
				public bool VisibleInStandardMode = true;
				public VariableCheckType CheckType = VariableCheckType.GreaterThanOrEqualTo;

				public void	OnCutsceneStart()
				{
						if (!Visible) {
								gameObject.SetLayerRecursively(Globals.LayerNumHidden);//turn ourselves off
						}

						if (RequireMissionVariable) {
								int currentValue = 0;
								if (!Missions.Get.MissionVariable(MissionName, VariableName, ref currentValue)
								|| !Frontiers.Data.GameData.CheckVariable(CheckType, VariableValue, currentValue)) {
										gameObject.SetLayerRecursively(Globals.LayerNumHidden);//turn ourselves off
										return;
								}
						}
						//Debug.Log ("OnCutsceneStart in " + name);
						if (!string.IsNullOrEmpty(AnimationOnStart)) {
								gameObject.GetComponent<Animation>().Play(AnimationOnStart);
						}
				}

				public void OnCutsceneIdleStart()
				{
						//Debug.Log ("OnCutsceneIdleStart in " + name);
						if (!string.IsNullOrEmpty(AnimationOnIdleStart)) {
								gameObject.GetComponent<Animation>().Play(AnimationOnIdleStart);
						}
				}

				public void	OnCutsceneIdleEnd()
				{
						//Debug.Log ("OnCutsceneIdleEnd in " + name);
						if (!string.IsNullOrEmpty(AnimationOnIdleEnd)) {
								gameObject.GetComponent<Animation>().Play(AnimationOnIdleEnd);
						}
				}
		}
}