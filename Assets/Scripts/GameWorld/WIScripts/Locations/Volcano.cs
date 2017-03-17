using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;

namespace Frontiers.World.WIScripts
{
	public class Volcano : WIScript
	{
		public Transform SummitParent;
		public GameObject RiftSmokePrefab;
		public GameObject RiftSmoke;
		public List <Light> PointLights = new List<Light> ();
		public VolcanoState State = new VolcanoState ();

		public override void OnInitialized ()
		{
			worlditem.OnAddedToGroup += OnAddedToGroup;
			WorldClock.Get.TimeActions.Subscribe (TimeActionType.HourStart, new ActionListener (HourStart));
		}

		public void OnAddedToGroup ()
		{
			if (RiftSmoke == null) {
				RiftSmoke = GameObject.Instantiate (RiftSmokePrefab) as GameObject;
				RiftSmoke.transform.parent = SummitParent;
				RiftSmoke.transform.ResetLocal ();
			}
		}

		public bool HourStart (double timeStamp) {
			if (!GetComponent<Animation>().isPlaying) {
				if (UnityEngine.Random.value > State.ChanceOfErupting) {
					GetComponent<Animation>().Play ();
				}
			}
			return true;
		}

		public override bool EnableAutomatically {
			get {
				return true;
			}
		}

		public void Update () 
		{
			if (Input.GetKeyDown (KeyCode.V)) {
				HourStart (0.0f);
			}
		}
	}

	[Serializable]
	public class VolcanoState
	{
		public float ChanceOfErupting = 0.2f;
	}
}
