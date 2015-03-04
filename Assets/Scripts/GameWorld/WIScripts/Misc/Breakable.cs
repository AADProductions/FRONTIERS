using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.World
{
	public class Breakable : WIScript
	{
		public BreakableState State = new BreakableState ( );
		public float NormalizedBreakableThreshold = 0.1f;
		public bool IsBroken
		{
			get
			{
				return State.NumTimesBroken > State.NumTimesRepaired;
			}
		}

		public void Strengthen ( )
		{
			if (IsBroken) {
				return;
			}
			State.NumTimesStrengthened++;
		}

		public void Repair ( )
		{
			State.NumTimesRepaired++;
		}

		public void Break ( )
		{
			if (State.NumTimesStrengthened > 0) {
				State.NumTimesStrengthened--;
				return;
			} else {
				State.NumTimesBroken++;
			}
		}
	}

	public class BreakableState
	{
		public int NumTimesBroken = 0;
		public int NumTimesRepaired = 0;
		public int NumTimesStrengthened = 0;
	}
}