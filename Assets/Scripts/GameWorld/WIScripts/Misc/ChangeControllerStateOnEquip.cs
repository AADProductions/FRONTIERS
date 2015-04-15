using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
	public class ChangeControllerStateOnEquip : WIScript
	{
		public string 			EnabledStateName;
		public List <string> 	DisabledStateNames = new List <string> ( );
		
		public void OnEquipAsTool ( )
		{
//			//Debug.Log ("Equipping " + name + " as tool");
//			foreach (string disabledStateName in DisabledStateNames)
//			{
//				Player.Local.FPSController.AllowStateRecursively (disabledStateName, false);
//				Player.Local.FPSCamera.AllowStateRecursively (disabledStateName, false);
//			}
//			Player.Local.FPSController.AllowState (EnabledStateName, true);
//			Player.Local.FPSController.SetState (EnabledStateName, true);
//			Player.Local.FPSCamera.AllowState (EnabledStateName, true);
//			Player.Local.FPSCamera.SetState (EnabledStateName, true);
		}
		
		public void OnUnequipAsTool ( )
		{
//			//Debug.Log ("UNEquipping " + name + " as tool");
//			foreach (string disabledStateName in DisabledStateNames)
//			{
//				Player.Local.FPSController.AllowStateRecursively (disabledStateName, true);
//				Player.Local.FPSCamera.AllowStateRecursively (disabledStateName, true);
//			}
//			Player.Local.FPSController.SetState (EnabledStateName, false);
//			Player.Local.FPSCamera.SetState (EnabledStateName, false);
		}
	}
}