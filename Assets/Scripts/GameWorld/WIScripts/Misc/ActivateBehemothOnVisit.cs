using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers.Gameplay;

namespace Frontiers.World.WIScripts
{
	public class ActivateBehemothOnVisit : WIScript
	{
		public override void OnInitialized ()
		{
			worlditem.Get <Visitable> ().OnPlayerVisit += OnPlayerVisit;
		}

		public void OnPlayerVisit ()
		{
			Debug.Log ("ActivateBehemothOnVisit");
			if (!mActivatingBehemoth) {
				mActivatingBehemoth = true;
				StartCoroutine (ActivatBehemothOverTime ());
			}
		}

		protected bool mActivatingBehemoth = false;

		protected IEnumerator ActivatBehemothOverTime ()
		{
			Transform BehemothLocation = gameObject.FindOrCreateChild ("BehemothLocation");
			BehemothLocation.position = Vector3.MoveTowards (Player.Local.Position, worlditem.tr.position, 25f);
			MasterAudio.PlaySound (MasterAudio.SoundType.Leviathan, BehemothLocation, "BabyLeviathanAwaken");
			GameObject.Destroy (BehemothLocation.gameObject, 0.5f);

			double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 2f;
			while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
				yield return null;
			}

			GUI.GUIManager.PostIntrospection ("What the hell was that?");

			waitUntil = Frontiers.WorldClock.AdjustedRealTime + 2f;
			while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
				yield return null;
			}

			Missions.Get.ActivateMission ("Behemoth", Frontiers.MissionOriginType.Introspection, string.Empty);

			Finish ();

			yield break;
		}
	}
}