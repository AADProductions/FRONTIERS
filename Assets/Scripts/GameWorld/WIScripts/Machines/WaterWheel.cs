using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World.WIScripts
{
	public class WaterWheel : WIScript
	{
		public WaterWheelState State = new WaterWheelState ();
		public Transform Pivot;
		public Vector3 Axis = new Vector3 (1f, 0f, 0f);
		public ParticleSystem WaterSplashes;

		public override void OnInitialized ()
		{
			worlditem.OnVisible += OnVisible;
			worlditem.OnInvisible += OnInvisible;
			//get the direction from the river name
		}

		public void OnVisible ()
		{
			WaterSplashes.enableEmission = true;
			enabled = true;
		}

		public void OnInvisible ()
		{
			WaterSplashes.enableEmission = false;
			enabled = false;
		}

		public void Update ()
		{
			Pivot.Rotate (Axis * (State.Forward ? State.Rate : -State.Rate) * Time.deltaTime);
		}
	}

	[Serializable]
	public class WaterWheelState
	{
		public bool Forward = true;
		public float Rate = 1f;
	}
}
