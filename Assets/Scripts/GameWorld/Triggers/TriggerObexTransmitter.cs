using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers.Data;
using Frontiers.Story;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	public class TriggerObexTransmitter : WorldTrigger
	{
		public ObexTransmitter Transmitter;

		public TriggerObexTransmitterState State = new TriggerObexTransmitterState ();

		public override bool OnPlayerEnter ()
		{
			if (Transmitter == null || Transmitter.IsDestroyed) {
				ObexTransmitter [] transmitters = GameObject.FindObjectsOfType <ObexTransmitter> ();
				for (int i = 0; i < transmitters.Length; i++) {
					if (transmitters [i].worlditem.FileName == State.TransmitterFileName) {
						Transmitter = transmitters [i];
						break;
					}
				}
			}

			if (Transmitter != null) {
				Transmitter.ActivateTransmitter ();
				return true;
			}

			return false;
		}
	}

	[Serializable]
	public class TriggerObexTransmitterState : WorldTriggerState
	{
		public string TransmitterFileName;
	}
}