using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

[CustomEditor(typeof(WorldClock))]
public class WorldClockEditor : Editor
{
		protected WorldClock worldclock;

		public void Awake()
		{
				worldclock = (WorldClock)target;
		}

		public override void OnInspectorGUI()
		{
				if (WorldClock.Get == null) {
						GUILayout.Label("Not initialized");
						return;
				}

				GUILayout.Label("CURRENT TIME:");

				if (Profile.Get != null && Profile.Get.HasCurrentGame) {
						GUILayout.Button("GAME TIME OFFSET:\n " + Profile.Get.CurrentGame.GameTimeOffset.ToString());
						GUILayout.Button("WORLD TIME OFFSET:\n " + Profile.Get.CurrentGame.WorldTimeOffset.ToString());
				}

				GUILayout.Button("HOUR OF DAY:\n " + WorldClock.Get.HourOfDay.ToString());
				GUILayout.Button("DAY OF YEAR:\n " + WorldClock.Get.DayOfYear.ToString());
				GUILayout.Button("NORMALIZED TIME OF DAY:\n" + WorldClock.DayCycleCurrentNormalized.ToString());
				GUILayout.Button("WORLD TIME:\n" + WorldClock.Time.ToString());
				GUILayout.Button("REAL TIME:\n" + WorldClock.RealTime.ToString());
				GUILayout.Button("ADJUSTED REAL TIME:\n" + WorldClock.AdjustedRealTime.ToString());
				GUILayout.Button("ADJUSTED REAL TIME OFFSET:\n" + WorldClock.AdjustedRealTime.ToString());
				if (!Mathf.Approximately(1.0f, (float)WorldClock.Get.TimeScale)) {
						GUI.color = Color.yellow;
				} else {
						GUI.color = Color.cyan;
				}
				GUILayout.Button("TIME SCALE:\n" + WorldClock.Get.TimeScale.ToString());
				GUI.color = Color.gray;
				GUILayout.Button("Time scale target: " + WorldClock.TimescaleTarget.ToString());
				UnityEditor.EditorUtility.SetDirty(worldclock);
		}
}