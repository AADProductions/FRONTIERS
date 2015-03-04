using UnityEngine;

namespace Frontiers
{
		//TODO move more of these into appropriate classes
		public static class Status
		{
				public static PlayerStatusRestore FloatToRestore(float restore)
				{
						if (restore >= 1.0f) {
								return PlayerStatusRestore.F_Full;
						} else if (restore >= 0.8f) {
								return PlayerStatusRestore.E_FourFifths;
						} else if (restore >= 0.6f) {
								return PlayerStatusRestore.D_ThreeFifths;
						} else if (restore >= 0.4f) {
								return PlayerStatusRestore.C_TwoFifths;
						} else if (restore >= 0.2f) {
								return PlayerStatusRestore.B_OneFifth;
						} else {
								return PlayerStatusRestore.A_None;
						}
			
				}

				public static string RestoreToString(PlayerStatusRestore restore)
				{
						string result = string.Empty;
						switch (restore) {
								case PlayerStatusRestore.A_None:
										result = "none";
										break;
				
								case PlayerStatusRestore.B_OneFifth:
										result = "one fifth";
										break;
				
								case PlayerStatusRestore.C_TwoFifths:
										result = "two fifths";
										break;
				
								case PlayerStatusRestore.D_ThreeFifths:
										result = "three fifths";
										break;
				
								case PlayerStatusRestore.E_FourFifths:
										result = "four fifths";
										break;
				
								case PlayerStatusRestore.F_Full:
										result = "all";
										break;
				
								default:
										break;
						}
						return result;
				}

				public static float RestoreToFloat(PlayerStatusRestore restore)
				{
						float result = 0.0f;
						switch (restore) {
								case PlayerStatusRestore.A_None:
										result = 0.0f;
										break;
				
								case PlayerStatusRestore.B_OneFifth:
										result = 0.2f;
										break;
				
								case PlayerStatusRestore.C_TwoFifths:
										result = 0.4f;
										break;
				
								case PlayerStatusRestore.D_ThreeFifths:
										result = 0.6f;
										break;
				
								case PlayerStatusRestore.E_FourFifths:
										result = 0.8f;
										break;
				
								case PlayerStatusRestore.F_Full:
										result = 1.0f;
										break;
				
								default:
										break;
						}
						return result;
				}

				public static float StatusIntervalToFloat(PlayerStatusInterval interval)
				{
						double time = Mathf.Infinity;
						switch (interval) {
								case PlayerStatusInterval.OnePing:
										time = WorldClock.HoursToSeconds(0.001f);
										break;
				
								case PlayerStatusInterval.HalfHour:
										time = WorldClock.HoursToSeconds(0.01f);
										break;
				
								case PlayerStatusInterval.OneHour:
										time = WorldClock.HoursToSeconds(0.05f);
										break;
				
								case PlayerStatusInterval.TwoHours:
										time = WorldClock.HoursToSeconds(0.10f);
										break;
				
								case PlayerStatusInterval.HalfDay:
										time = WorldClock.HoursToSeconds(0.5f);
										break;
				
								case PlayerStatusInterval.OneDay:
										time = WorldClock.HoursToSeconds(1.0f);
										break;
				
								default:
										break;
						}
						return (float)time;
				}

				public static PlayerStatusRestore DoubleRestore(PlayerStatusRestore restore)
				{
						PlayerStatusRestore doubled = PlayerStatusRestore.A_None;
						switch (restore) {
								case PlayerStatusRestore.A_None:
										doubled = PlayerStatusRestore.A_None;
										break;
				
								case PlayerStatusRestore.B_OneFifth:
										doubled = PlayerStatusRestore.C_TwoFifths;
										break;
				
								case PlayerStatusRestore.C_TwoFifths:
										doubled = PlayerStatusRestore.D_ThreeFifths;
										break;
				
								case PlayerStatusRestore.D_ThreeFifths:
										doubled = PlayerStatusRestore.E_FourFifths;
										break;
				
								case PlayerStatusRestore.E_FourFifths:
										doubled = PlayerStatusRestore.F_Full;
										break;
				
								default:
										doubled = PlayerStatusRestore.F_Full;
										break;
						}
						return doubled;
				}

				public static float TimeMethodToPulseInterval(PlayerStatusOverTimeMethod method)
				{
						float pulseInterval = 0.1f;
						switch (method) {
								case PlayerStatusOverTimeMethod.SlowPulse:
										pulseInterval = 10.0f;
										break;
				
								case PlayerStatusOverTimeMethod.RapidPulse:
										pulseInterval = 1.0f;
										break;
				
								default:
										break;
						}
						return pulseInterval;
				}
		}
}