using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World.WIScripts
{
	public class Fuse : WIScript
	{
		public Bomb bomb;
		public bool AttachedToBomb = false;
		public bool CanBeFlammable = false;
		public FuseState State = new FuseState ();
		public GameObject FuseDoppleganger;

		public override void OnInitialized ()
		{
			AttachToBomb (worlditem.Get <Bomb> ( ));
		}

		public void CopyFrom (Fuse existingFuse)
		{
			//if it's attached to a bomb already, copy its doppleganger props
			//otherwise copy the world item
			if (existingFuse.AttachedToBomb) {
				State.DopplegangerProps.CopyFrom (existingFuse.State.DopplegangerProps);
				State.MissionName = existingFuse.State.MissionName;
				State.MissionVariableName = existingFuse.State.MissionVariableName;
				State.VariableValue = existingFuse.State.VariableValue;
				State.CheckType = existingFuse.State.CheckType;
			} else {
				State.DopplegangerProps.CopyFrom (existingFuse.worlditem);
				State.MissionName = existingFuse.State.MissionName;
				State.MissionVariableName = existingFuse.State.MissionVariableName;
				State.VariableValue = existingFuse.State.VariableValue;
				State.CheckType = existingFuse.State.CheckType;
			}
		}

		public void AttachToBomb (Bomb newBomb)
		{
			bomb = newBomb;
			if (bomb != null) {
				AttachedToBomb = true;
			}

			if (AttachedToBomb) {
				FuseDoppleganger = WorldItems.GetDoppleganger (State.DopplegangerProps, worlditem.tr, FuseDoppleganger, WIMode.World);
				FuseDoppleganger.transform.localPosition = bomb.FuseOffset;
				//don't let our colliders turn off
				worlditem.ActiveState = WIActiveState.Active;
				worlditem.ActiveStateLocked = true;
			}

			if (!CanBeFlammable) {
				Flammable flammable = null;
				if (worlditem.Is <Flammable> (out flammable)) {
					flammable.Finish ();
				}
			}

			if (!string.IsNullOrEmpty (State.MissionName)) {
				Player.Get.AvatarActions.Subscribe (AvatarAction.MissionVariableChange, new ActionListener (MissionVariableChange));
			}
		}

		protected bool MissionVariableChange (double timeStamp) {
			if (!AttachedToBomb)
				return true;

			int variableValue = Missions.Get.GetVariableValue (State.MissionName, State.MissionVariableName);
			if (Frontiers.Data.GameData.CheckVariable (State.CheckType, State.VariableValue, variableValue)) {
				bomb.Explode ();
			}
			return true;
		}
	}

	[Serializable]
	public class FuseState {
		public GenericWorldItem DopplegangerProps = new GenericWorldItem ( );
		public string MissionName;
		public string MissionVariableName;
		public int VariableValue;
		public VariableCheckType CheckType;
	}
}
