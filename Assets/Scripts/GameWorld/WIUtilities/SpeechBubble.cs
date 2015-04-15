using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Story;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
		public class SpeechBubble : MonoBehaviour
		{		//used by characters to say things out loud
				//other characters within range will hear it
				//those characters can be given commands by the speech

				//it is assumed that these will be set before Start
				public Speech ParentSpeech;
				public Talkative Speaker;
				public ActionNode Dispatcher = null;
				public Transform ListenerTarget = null;
				public List <Listener> Listeners = new List <Listener>();

				public void Start()
				{
						gameObject.layer	= Globals.LayerNumTrigger;
						SphereCollider sc = gameObject.AddComponent <SphereCollider>();
						sc.isTrigger = true;
						sc.radius = ParentSpeech.AudibleRange;
						StartCoroutine(NudgeBubbleCollider());
				}

				public void NextPage(string listenerTargetName)
				{
						if (listenerTargetName == "[Random]" && Listeners.Count > 0) {	//get a random listener to look at
								Listener randomListener = Listeners[UnityEngine.Random.Range(0, Listeners.Count)];
								if (randomListener != null && ListenerTarget != null) {	//look at the listener
										ListenerTarget.transform.parent = Speaker.worlditem.Group.transform;
										ListenerTarget.transform.position = randomListener.transform.position;
								}
						}
				}

				public void FinishSpeech()
				{
						if (!string.IsNullOrEmpty(ParentSpeech.OnFinishCommand)) {
								for (int i = Listeners.Count - 1; i >= 0; i--) {
										if (Listeners[i] != null) {
												Listeners[i].HearCommand(this, ParentSpeech.OnFinishCommand);
										}
								}
						}
						DestroyInAMoment();
				}

				public void InterruptSpeech()
				{
						if (!string.IsNullOrEmpty(ParentSpeech.OnInterruptCommand)) {
								for (int i = Listeners.Count - 1; i >= 0; i--) {
										if (Listeners[i] != null) {
												Listeners[i].HearCommand(this, ParentSpeech.OnInterruptCommand);
										}
								}			
						}
						DestroyInAMoment();
				}

				public void StartSpeech()
				{

				}

				protected void DestroyInAMoment()
				{
						if (!mStartedDestroying) {
								StartCoroutine(DestroyOverTime());
						}
				}

				public void OnTriggerEnter(Collider other)
				{
						if (mStartedDestroying) {
								return;
						}

						//TODO use items of interest
						switch (other.gameObject.layer) {
								case Globals.LayerNumWorldItemActive:
										//see if it's a character
										BodyPart bodyPart = null;
										if (other.gameObject.HasComponent <BodyPart>(out bodyPart)) {
												Listener listener = null;
												if (bodyPart.Owner.IOIType == ItemOfInterestType.WorldItem && bodyPart.Owner.worlditem.Is <Listener>(out listener)
												&&	bodyPart.Owner.worlditem != Speaker.worlditem
												&&	!Listeners.Contains(listener)) {	//if it's a listener AND it isn't our speaker
														//and we don't already have it
														Listeners.Add(listener);
														if (!string.IsNullOrEmpty(ParentSpeech.OnAudibleCommand)) {
																listener.HearCommand(this, ParentSpeech.OnAudibleCommand);
														}
												}
										}
										break;
								
								default:
										break;
						}
				}
				//this ensures that OnTriggerEnter will fire
				protected IEnumerator NudgeBubbleCollider()
				{	//weird I know, seems to be a physics limitation
						double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 0.05f;
						while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
								yield return null;
						}
						transform.Translate(Vector3.zero);
						yield break;
				}

				protected IEnumerator DestroyOverTime()
				{
						double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 0.15f;
						while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
								yield return null;
						}
						GameObject.Destroy(gameObject);
						yield break;
				}

				protected bool mStartedDestroying = false;
		}
}