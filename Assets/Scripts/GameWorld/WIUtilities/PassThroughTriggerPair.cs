using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
	public class PassThroughTriggerPair : MonoBehaviour
	{
		//used to detect when a player has passed through an entrance
		//always used in pairs (inner / outer) hence the name
		public PassThroughTrigger ParentTrigger;
		public PassThroughTriggerType TriggerType = PassThroughTriggerType.Inner;
		public bool IsIntersecting = false;
		//public string EnterFunctionName = "OnEnter";
		//public string ExitFunctionName = "OnExit";
		//public PassThroughTriggerPair Sibling;
		//public PassThroughState CurrentState = PassThroughState.InnerOff_OuterOff;
		//public PassThroughState PreviousState = PassThroughState.InnerOff_OuterOff;
		public void OnTriggerEnter(Collider other)
		{
			//Debug.Log ("On trigger enter in pass through trigger pair: " + other.name);
			if (other.isTrigger || other.gameObject.layer != Globals.LayerNumPlayer || !other.CompareTag ("Player")) {
				return;
			}

			IsIntersecting = true;

			if (ParentTrigger != null) {
				ParentTrigger.TriggerPairEnter(this);
			} else {
				//Debug.LogWarning("Pass through trigger pair parent trigger is null, proceeding normally");
			}
		}

		public void OnTriggerExit(Collider other)
		{
			//Debug.Log ("On trigger exit in pass through trigger pair: " + other.name);
			if (other.isTrigger || other.gameObject.layer != Globals.LayerNumPlayer || !other.CompareTag ("Player")) {
				return;
			}

			IsIntersecting = false;
		}

		public void OnDrawGizmos()
		{
			switch (TriggerType) {
				case PassThroughTriggerType.Inner:
					break;
				default:
					Gizmos.color = Color.green;
					DrawArrow.ForGizmo(transform.position + transform.up, transform.forward, 0.25f, 20);
					break;
			}
		}
	}
}