using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Frontiers.World.BaseWIScripts
{
	public class Photosensitive : WIScript, IPhotosensitive
	{
		public WorldLight NearestLight {
			get {
				WorldLight nearestLight = null;
				float nearestDistance = Mathf.Infinity;
				for (int i = LightSources.LastIndex (); i >= 0; i--) {
					if (LightSources [i] == null) {
						LightSources.RemoveAt (i);
					}else {
						float distance = Vector3.Distance (transform.position, LightSources [i].transform.position);
						if (distance < nearestDistance) {
							nearestDistance = distance;
							nearestLight = LightSources [i];
						}
					}
				}
				return nearestLight;
			}
		}

		public Fire NearestFire {
			get {
				Fire fire = null;
				float nearestDistance = Mathf.Infinity;
				for (int i = FireSources.LastIndex (); i >= 0; i--) {
					if (FireSources [i] == null) {
						FireSources.RemoveAt (i);
					}else {
						float distance = Vector3.Distance (transform.position, FireSources [i].transform.position);
						if (distance < nearestDistance) {
							nearestDistance = distance;
							fire = FireSources [i];
						}
					}
				}
				return fire;
			}
		}

		public bool HasNearbyLights {
			get {
				return LightSources.Count > 0;
			}
		}

		public bool HasNearbyFires {
			get {
				return FireSources.Count > 0;
			}
		}

		#region IPhotosensitive implementation
		public float Radius { get; set; }

		public Vector3 Position { get { return transform.position; } }

		public float LightExposure { get; set; }

		public float HeatExposure { get; set; }

		public List <WorldLight> LightSources
		{ 
			get {
				if (mLightSources == null) {
					mLightSources = new List <WorldLight> ();
				}
				return mLightSources;
			}
			set {
				mLightSources = value;
			}
		}
		protected List <WorldLight> mLightSources = null;

		public List <Fire> FireSources
		{ 
			get {
				if (mFireSources == null) {
					mFireSources = new List <Fire> ();
				}
				return mFireSources;
			}
			set {
				mFireSources = value;
			}
		}
		protected List <Fire> mFireSources = null;

		public Action OnExposureIncrease { get; set; }

		public Action OnExposureDecrease { get; set; }

		public Action OnHeatIncrease { get; set; }

		public Action OnHeatDecrease { get; set; }
		#endregion
	}
}
