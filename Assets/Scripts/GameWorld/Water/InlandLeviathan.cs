using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System;
using ExtensionMethods;

public class InlandLeviathan : WaterLeviathan
{
	public Transform TargetPosition;
	public Vector3 DormantOffset = Vector3.up * 5f;
	public GameObject LastWakeBubbles;

	public override float SeekSpeed {
		get {
			return Globals.LeviathanMoveSpeed * 2f;
		}
	}

	public override void LateUpdate ()
	{
		Wake.enableEmission = true;
		if ((mMode == HostileMode.Dormant || mMode == HostileMode.CoolingOff) && TargetPosition != null) {
			transform.position = Vector3.MoveTowards (transform.position, TargetPosition.position, SeekSpeed);
		}
		base.LateUpdate ();
	}

	protected override IEnumerator CustomDormantAction ()
	{
		for (int i = 0; i < Renderers.Count; i++) {
			Renderers [i].enabled = true;
		}
		Wake.enableEmission = true;
		GetComponent<Animation>().Play ("BehemothLeviathanDormant");
		yield break;
	}

}