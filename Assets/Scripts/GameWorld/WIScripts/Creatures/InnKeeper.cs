using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
		public class InnKeeper : WIScript
		{
				public InnKeeperState State = new InnKeeperState();

				public int PricePerNight {
						get {
								return (int)Globals.InnBasePricePerNight;
						}
				}

				public bool HasPaid {
						get {
								return WorldClock.AdjustedRealTime < State.TimeLastPaid + WorldClock.HoursToSeconds (24);
						}
				}
		}

		public class InnKeeperState
		{
				public float BasePricePerNight = 0f;
				public int NumTimesUsed = 0;
				public double TimeLastPaid = 0f;
		}
}