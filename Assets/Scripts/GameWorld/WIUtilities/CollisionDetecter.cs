using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System;

public class CollisionDetecter : MonoBehaviour
{
	public void OnCollisionEnter (Collision collision)
	{
		foreach (ContactPoint c in collision.contacts) {
			Debug.Log (name + " collided with " + c.otherCollider.name);
		}
	}
}

