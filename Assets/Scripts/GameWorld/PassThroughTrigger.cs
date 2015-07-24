﻿using UnityEngine;
using System.Collections;

//can you believe it's so fucking difficult
//just to reliably detect when an object has passed through a goddamn door
//this is the third method i've used and it's still not 100%
namespace Frontiers.World
{
		public class PassThroughTrigger : MonoBehaviour
		{
				public PassThroughTriggerPair InnerTrigger;
				public PassThroughTriggerPair OuterTrigger;
				public GameObject TargetObject;
				public string PassThroughFunctionName = "OnPassThrough";
				public bool ReadyToPassThrough = false;
				public PassThroughTriggerPair LastSingleIntersection;

				public void Start()
				{
						enabled = false;
						InnerTrigger.ParentTrigger = this;
						OuterTrigger.ParentTrigger = this;
				}

				public void TriggerPairEnter(PassThroughTriggerPair trigger)
				{
						Debug.Log ("Trigger pair enter: " + trigger.name);
						if (LastSingleIntersection == null) {
								//if we have no last single intersection
								//then whatever just triggered this is the last single
								LastSingleIntersection = trigger;
						}
						enabled = true;
				}

				public void Update()
				{
						//if we're ready to pass through
						if (ReadyToPassThrough) {
								//if both are still intersecting
								if (OuterTrigger.IsIntersecting && InnerTrigger.IsIntersecting) {
										//nothing has changed so get out of there
										return;
								}
								//we're no longer ready to pass through
								//we've either passed through or else moved back
								//find out which next
								ReadyToPassThrough = false;
								if (LastSingleIntersection == InnerTrigger) {
										//if the last single intersection was inner
										//then the new single intersection has to be outer
										//for us to have passed through
										if (OuterTrigger.IsIntersecting) {
												//we've passed through
												//set the last single intersection to outer
												LastSingleIntersection = OuterTrigger;
												OnPassThrough();
												return;
										}
								} else {//if LastSingleIntersection == OuterTrigger) {
										//if the last single intersection was outer
										//then the new single intersection has to be inner
										//for us to have passed through
										if (InnerTrigger.IsIntersecting) {
												//we've passed through
												//set the last single intersection to outer
												LastSingleIntersection = InnerTrigger;
												OnPassThrough();
												return;
										}
								}
						}

						//if we're not ready to pass through, see if we've become ready
						//if both are intersecting then the next time we only have one intersecting
						//we will have passed through the trigger
						if (InnerTrigger.IsIntersecting && OuterTrigger.IsIntersecting) {
								ReadyToPassThrough = true;
								//do a sanity check
								//if we don't have a LastSingleIntersection then something has gone wrong
								//take the closest collider to the player and use that instead
								if (LastSingleIntersection == null) {
										if (Vector3.Distance(InnerTrigger.transform.position, Player.Local.Position) <
										Vector3.Distance(OuterTrigger.transform.position, Player.Local.Position)) {
												Debug.Log("Last single intersection was null in door, setting to inner trigger");
												LastSingleIntersection = InnerTrigger;
										} else {
												Debug.Log("Last single intersection was null in door, setting to outer trigger");
												LastSingleIntersection = OuterTrigger;
										}
								}
								return;
						}

						//if neither trigger is intersecting then we're either done
						//or else something has gone wrong
						if (!InnerTrigger.IsIntersecting && !OuterTrigger.IsIntersecting) {
								//this may have happened because the player passed through the triggers
								//too fast for Unity to send the correct OnEnter / OnExit messages
								//SO we're going to try a sanity check before saying that we're done
								if (LastSingleIntersection != null) {
									LastSingleIntersection = null;
								}
						}
				}

				protected void OnPassThrough()
				{
						Debug.Log ("On pass through in trigger pair " + name);
						TargetObject.SendMessage(PassThroughFunctionName, SendMessageOptions.DontRequireReceiver);
				}
		}
}