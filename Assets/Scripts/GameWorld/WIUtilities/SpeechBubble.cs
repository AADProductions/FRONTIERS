using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Story;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	public class SpeechBubble : MonoBehaviour
	{
		//used by characters to say things out loud
		//other characters within range will hear it
		//those characters can be given commands by the speech
		//it is assumed that these will be set before Start
		public Speech ParentSpeech;
		public Talkative Speaker;
		public ActionNode Dispatcher = null;
		public Transform ListenerTarget = null;
		public HashSet <Listener> Listeners = new HashSet <Listener> ();

		public void Start ()
		{
			Debug.Log ("Creating speech bubble...");
			gameObject.layer = Globals.LayerNumAwarenessBroadcaster;
			SphereCollider sc = gameObject.AddComponent <SphereCollider> ();
			sc.isTrigger = true;
			sc.radius = ParentSpeech.AudibleRange;
			StartCoroutine (NudgeBubbleCollider ());
		}

		public void NextPage (string listenerTargetName)
		{
			if (listenerTargetName == "[Random]" && Listeners.Count > 0) {	//get a random listener to look at
				int randomIndex = UnityEngine.Random.Range (0, Listeners.Count);
				var listenersEnum = Listeners.GetEnumerator ();
				for (int i = 0; i < randomIndex; i++) {
					listenersEnum.MoveNext ();
				}
				Listener randomListener = listenersEnum.Current;
				listenersEnum.Dispose ();
				if (randomListener != null && ListenerTarget != null) {	//look at the listener
					ListenerTarget.transform.parent = Speaker.worlditem.Group.transform;
					ListenerTarget.transform.position = randomListener.transform.position;
				}
			}
		}

		public void FinishSpeech ()
		{
			if (!string.IsNullOrEmpty (ParentSpeech.OnFinishCommand)) {
				var listenersEnum = Listeners.GetEnumerator ();
				while (listenersEnum.MoveNext ()) {
					if (listenersEnum.Current != null) {
						listenersEnum.Current.HearCommand (this, ParentSpeech.OnFinishCommand);
					}
				}
			}
			DestroyInAMoment ();
		}

		public void InterruptSpeech ()
		{
			if (!string.IsNullOrEmpty (ParentSpeech.OnInterruptCommand)) {
				var listenersEnum = Listeners.GetEnumerator ();
				while (listenersEnum.MoveNext ()) {
					if (listenersEnum.Current != null) {
						listenersEnum.Current.HearCommand (this, ParentSpeech.OnInterruptCommand);
					}
				}
			}
			DestroyInAMoment ();
		}

		public void StartSpeech ()
		{

		}

		protected void DestroyInAMoment ()
		{
			if (!mStartedDestroying) {
				StartCoroutine (DestroyOverTime ());
				Listeners.Clear ();
				Listeners = null;
			}
		}

		public void OnTriggerEnter (Collider other)
		{
			if (mStartedDestroying) {
				return;
			}

			IItemOfInterest ioi = null;
			if (WorldItems.GetIOIFromCollider (other, out ioi)) {
				Listener listener = null;
				if (ioi.IOIType == ItemOfInterestType.WorldItem && ioi.worlditem.Is <Listener> (out listener) && !Listeners.Contains (listener)) {
					Listeners.Add (listener);
					if (!string.IsNullOrEmpty (ParentSpeech.OnAudibleCommand)) {
						listener.HearCommand (this, ParentSpeech.OnAudibleCommand);
					}
				}
			}
		}
		//this ensures that OnTriggerEnter will fire
		protected IEnumerator NudgeBubbleCollider ()
		{	//weird I know, seems to be a physics limitation
			double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 0.05f;
			while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
				yield return null;
			}
			transform.Translate (Vector3.zero);
			yield break;
		}

		protected IEnumerator DestroyOverTime ()
		{
			double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 0.15f;
			while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
				yield return null;
			}
			GameObject.Destroy (gameObject);
			yield break;
		}

		protected bool mStartedDestroying = false;
	}
}