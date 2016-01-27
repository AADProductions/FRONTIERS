using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Data;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
		public class PathHoleFiller : MonoBehaviour
		{
				public PathAvatar BrokenPath = null;
				public PathTaskCallback CallBack = null;
				public int LocationsLeft = 0;
				public Dictionary <string, KeyValuePair <int, MobileReference>> LocationsToLookup = new Dictionary <string, KeyValuePair <int, MobileReference>>();

				public void Start()
				{
						BrokenPath.IsLoadingOrFilling = true;
						GUI.GUIManager.PostInfo("Loading " + BrokenPath.name + "...");

						//first figure out what we're looking up specifically
//			foreach (KeyValuePair <int, MobileReference> reference in BrokenPath.State.LocationReferences)
//			{
//				if (BrokenPath.spline.splineNodesArray [reference.Key] == null)
//				{
//					LocationsToLookup.Add (reference.Value.FileName, reference);
//					LocationsLeft++;
//				}
//			}			
			
//			StartCoroutine (LoadLocationsOverTime ( ));
				}

				public void OnLocationLoaded(WorldItem locationWorldItem)
				{
						LocationsLeft--;
						//int positionInSpline = -1;
						KeyValuePair <int, MobileReference> reference;
						if (LocationsToLookup.TryGetValue(locationWorldItem.FileName, out reference)) {
								Location location = null;
								if (locationWorldItem.Is <Location>(out location)) {
										////Debug.Log ("LOCATION " + locationWorldItem.FileName + " HAD A PLACE IN THE PATH " + BrokenPath.SafeName);
										BrokenPath.spline.splineNodesArray[reference.Key] = location.Node;
								}
						} else {
								////Debug.Log ("LOCATION " + locationWorldItem.FileName + " HAS NO PLACE IN SPLINE FOR PATH " + BrokenPath.SafeName);
						}
				}

				public void OnFinish()
				{
						GUI.GUIManager.PostInfo("Finished loading " + BrokenPath.name);
						BrokenPath.IsLoadingOrFilling = false;
						//destroy after a bit, we don't need this any more
						if (CallBack != null) {
								CallBack(BrokenPath);
						}
						GameObject.Destroy(gameObject, 0.5f);
				}
		}
}