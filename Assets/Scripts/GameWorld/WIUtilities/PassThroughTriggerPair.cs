using UnityEngine;
using System.Collections;

public class PassThroughTriggerPair : MonoBehaviour
{
		//used to detect when a player has passed through an entrance
		//always used in pairs (inner / outer) hence the name
		public PassThroughTriggerType TriggerType = PassThroughTriggerType.Inner;
		public bool IsIntersecting = false;
		public GameObject TargetObject;
		public string EnterFunctionName = "OnEnter";
		public string ExitFunctionName = "OnExit";
		public PassThroughTriggerPair Sibling;
		public PassThroughState CurrentState = PassThroughState.InnerOff_OuterOff;
		public PassThroughState PreviousState = PassThroughState.InnerOff_OuterOff;

		public void OnTriggerEnter(Collider other)
		{
				if (other.gameObject.layer != Globals.LayerNumPlayer) {
						return;
				}

				IsIntersecting = true;

				CheckState();
		}

		public void OnTriggerExit(Collider other)
		{
				if (other.gameObject.layer != Globals.LayerNumPlayer) {
						return;
				}

				IsIntersecting = false;

				CheckState();
		}

		protected void CheckState()
		{
				if (TargetObject == null) {
						GameObject.Destroy(gameObject);
						return;
				}
		
				//save previous states
				PreviousState = CurrentState;
				Sibling.PreviousState = Sibling.CurrentState;

				PassThroughState state = PassThroughState.InnerOff_OuterOff;
				if (TriggerType == PassThroughTriggerType.Inner) {
						if (IsIntersecting) {
								if (Sibling.IsIntersecting) {
										state = PassThroughState.InnerOn_OuterOn;
								} else {
										state = PassThroughState.InnerOn_OuterOff;
								}
						} else {
								if (Sibling.IsIntersecting) {
										state = PassThroughState.InnerOff_OuterOn;
								} else {
										state = PassThroughState.InnerOff_OuterOff;
								}
						}
				} else { //if TriggerType == outer
						if (IsIntersecting) {
								if (Sibling.IsIntersecting) {
										state = PassThroughState.InnerOn_OuterOn;
								} else {
										state = PassThroughState.InnerOff_OuterOn;
								}
						} else {
								if (Sibling.IsIntersecting) {
										state = PassThroughState.InnerOn_OuterOff;
								} else {
										state = PassThroughState.InnerOff_OuterOff;
								}
						}
				}
				CurrentState = state;
				Sibling.CurrentState = state;
				
				if (PreviousState == PassThroughState.InnerOn_OuterOn) {
						//if we were intersecting both in our previous state
						if (CurrentState == PassThroughState.InnerOff_OuterOn) {
								//if we're now only intersecting the outer, then we've EXITED the door
								TargetObject.SendMessage(ExitFunctionName, SendMessageOptions.DontRequireReceiver);
						} else if (CurrentState == PassThroughState.InnerOn_OuterOff) {
								//if we're only intersecting the inner, then we've ENTERED the door
								TargetObject.SendMessage(EnterFunctionName, SendMessageOptions.DontRequireReceiver);
						}
				}
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

public enum PassThroughTriggerType
{
		Inner,
		Outer,
}

public enum PassThroughState
{
		InnerOn_OuterOff,
		InnerOn_OuterOn,
		InnerOff_OuterOn,
		InnerOff_OuterOff,
}
