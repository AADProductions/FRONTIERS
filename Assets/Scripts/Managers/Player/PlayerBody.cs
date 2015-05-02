using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using TNet;
using Frontiers.World;

public class PlayerBody : WorldBody
{
		public PlayerBase PlayerObject;

		public override void Awake()
		{
				base.Awake();

				foreach (Renderer renderer in Renderers) {
						renderer.gameObject.layer = Globals.LayerNumHidden;
						renderer.castShadows = false;
						renderer.receiveShadows = false;
				}

				SetVisible(false);
				IgnoreCollisions(true);

//		_worldBodyNetworkUpdateTime = NetworkManager.WorldBodyUpdateRate;
//		_bodyAnimatorNetworkUpdateTime = NetworkManager.BodyAnimatorUpdateRate;
		}

		public override void OnSpawn(IBodyOwner owner)
		{
				base.OnSpawn(owner);
				SetVisible(true);
		}

		public override void Update()
		{
				if (!GameManager.Is(FGameState.InGame) || !PlayerObject.HasSpawned)
						return;

				//if we're the brain then we're the one setting the position
				//update the position based on the owner's position
				if (NObject.isMine) {
						SmoothPosition = PlayerObject.transform.position;
						SmoothRotation = PlayerObject.transform.rotation;

						// Decrease Timer
//			_worldBodyNetworkUpdateTime -= Time.deltaTime;
//			if (_worldBodyNetworkUpdateTime <= 0) {
//				tno.Send ("OnNetworkWorldBodyUpdate", Target.Others, new WorldBodyUpdate (
//					SmoothPosition, SmoothRotation));
//
//				// Reset to send again
//				_worldBodyNetworkUpdateTime = NetworkManager.WorldBodyUpdateRate;
//			}
//
//			_bodyAnimatorNetworkUpdateTime -= Time.deltaTime;
//			if (_bodyAnimatorNetworkUpdateTime <= 0) {
//				tno.Send ("OnBodyAnimatorUpdate", Target.Others, new BodyAnimatorUpdate (Animator));
//
//				_bodyAnimatorNetworkUpdateTime = NetworkManager.BodyAnimatorUpdateRate;
//			}
				}
				//do this regardelss of network state
				//this will ensure a smooth transition even if the updates don't happen very often
				rb.MovePosition(Vector3.Lerp(transform.position, mSmoothPosition, 0.5f));
				rb.MoveRotation(Quaternion.Lerp(transform.rotation, mSmoothRotation, 0.5f));
		}
		//public AppearanceFlags TBD
}
