using UnityEngine;
using System;
using System.Collections;
using Frontiers.World.WIScripts;

namespace Frontiers.World.WIScripts {
	public class Demolishable : WIScript {

		public StructureDemolitionController DemolitionController;

		public DemolishableState State = new DemolishableState ( );

		public Action OnDemolished;

		public override void OnInitialized ()
		{
			Damageable damageable = null;
			if (!worlditem.Is <Damageable> (out damageable)) {
				//Debug.Log ("Demolishable requires damageable");
				Finish ();
				return;
			}
			damageable.OnTakeDamage += OnTakeDamage;

			if (State.UseDemolitionController) {
				//if we're supposed to use a demolition controller find that now
				Invoke ("LookForDemolitionController", 1f);
			}
		}

		public void LookForDemolitionController ( ) {
			WorldItem demolitionControllerWorldItem = null;
			if (WIGroups.FindChildItem (State.DemolitionControllerPath, out demolitionControllerWorldItem)) {
				DemolitionController = demolitionControllerWorldItem.Get <StructureDemolitionController> ();
				DemolitionController.AddDemolishable (this);
				//don't let our colliders turn off
				worlditem.ActiveState = WIActiveState.Active;
				worlditem.ActiveStateLocked = true;
			}
		}

		public void OnTakeDamage ( )
		{
			Debug.Log ("taking damage in demolishable");
			Damageable damageable = null;
			if (worlditem.Is <Damageable> (out damageable)) {
				//check to see if the force exceeded our threshold
				Vector3 lastForce = damageable.State.LastDamageForce;
				if (lastForce.magnitude > State.MaximumForce) {
					//instant kill
					damageable.State.DamageTaken = damageable.State.Durability;
					OnDemolished.SafeInvoke ();
					Debug.Log ("We are demolished");
				} else {
					damageable.ResetDamage ();
					Debug.Log ("last force " + lastForce.magnitude.ToString () + " wasn't enough to kill " + State.MaximumForce.ToString ());
				}
			}
		}

		#if UNITY_EDITOR
		public override void OnEditorRefresh ()
		{
			if (DemolitionController != null) {
				State.DemolitionControllerPath = DemolitionController.GetComponent <WorldItem> ( ).StaticReference.FullPath;
			}
		}
		#endif
	}

	[Serializable]
	public class DemolishableState {
		public float MaximumForce = 1f;
		public bool UseDemolitionController {
			get {
				return !string.IsNullOrEmpty (DemolitionControllerPath);
			}
		}
		public string DemolitionControllerPath;
	}
}