using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExtensionMethods;
using Pathfinding;
using Pathfinding.RVO;
using Frontiers;

public class RVOTargetHolder : MonoBehaviour
{
	//this is used by motile creatures to stalk / follow other objects
	//it basically keeps a set of transforms moving around the target in predictable ways
	//you can see it used in Motile all the time
	//this is a very old script probably riddles with inefficiencies
	//how close the ground targets are from this object
	public float InnerCircleDistance = 2.0f;
	//how far away you have to be from your target to lose your spot
	public float LostSpotThreshold = 2.0f;
	//min/max of how close to keep targets in the outer circle
	public float OuterCircleMinDistance = 6.5f * 0.6f;
	public float OuterCircleMaxDistance = 8.0f * 0.6f;
	public float OuterCircleMinRotate = 2.5f;
	public float OuterCircleMaxRotate = 4.0f;
	public float OuterCircleOffset = 0f;
	public float FollowSeparation = 2.5f;
	public Transform SpotHelper;

	public void OnEnabled ()
	{
		if (SpotHelper == null) {
			SpotHelper = new GameObject ("SpotHelper-" + name).transform;
		}
	}

	public void OnDisabled ()
	{
		if (SpotHelper != null) {
			GameObject.Destroy (SpotHelper.gameObject);
		}
	}

	public bool HasNorthAttacker {
		get {
			if (NorthAttacker != null) {
				if (NorthAttacker.parent != tr) {
					mGroundAttackers.Remove (NorthAttacker);
					NorthAttacker = null;
				}
			}
			return NorthAttacker != null;
		}
	}

	public bool HasEastAttacker {
		get {
			if (EastAttacker != null) {
				if (EastAttacker.parent != tr) {
					mGroundAttackers.Remove (EastAttacker);
					EastAttacker = null;
				}
			}
			return EastAttacker != null;
		}
	}

	public bool HasSouthAttacker {
		get {
			if (SouthAttacker != null) {
				if (SouthAttacker.parent != tr) {
					mGroundAttackers.Remove (SouthAttacker);
					SouthAttacker = null;
				}
			}
			return SouthAttacker != null;
		}
	}

	public bool HasWestAttacker {
		get {
			if (WestAttacker != null) {
				if (WestAttacker.parent != tr) {
					mGroundAttackers.Remove (WestAttacker);
					WestAttacker = null;
				}
			}
			return WestAttacker != null;
		}
	}

	public Transform NorthAttacker;
	public Transform EastAttacker;
	public Transform SouthAttacker;
	public Transform WestAttacker;
	public Transform tr;

	public void Awake ()
	{
		tr = transform;
	}

	public void Start ()
	{
		enabled = false;
	}

	public void AddCompanion (Transform target)
	{
		if (mCompanions.SafeAdd (target)) {
			target.parent = tr;
			Debug.Log ("Added companion " + target.name + " to " + name);
			//make sure they're not in any of our other lists
			mGroundAttackers.Remove (target);
			mFollowers.Remove (target);
			mGroundStalkers.Remove (target);
			//mCompanions.Remove (target);
		}
	}
	//check to see if the target is in the inner circle of followers
	//if it is, it sets the map direction of the target
	public bool IsGroundAttacker (Transform target, ref MapDirection direction)
	{
		bool result = false;
		if (HasNorthAttacker && NorthAttacker == target) {
			direction = MapDirection.A_North;
			result = true;
		} else if (HasWestAttacker && WestAttacker == target) {
			direction = MapDirection.G_West;
			result = true;
		} else if (HasSouthAttacker && SouthAttacker == target) {
			direction = MapDirection.E_South;
			result = true;
		} else if (HasEastAttacker && EastAttacker == target) {
			direction = MapDirection.C_East;
			result = true;
		}
		return result;
	}
	//check to see if the target is in the outer circle of followers
	public bool IsGroundStalker (Transform target)
	{
		return mGroundStalkers.Contains (target);
	}

	public bool	HasTargets {
		get {
			return mGroundStalkers.Count > 0 || mGroundAttackers.Count > 0 || mFollowers.Count > 0 || mCompanions.Count > 0;
		}
	}

	public bool HasGroundAttackers {
		get { 
			return mGroundAttackers.Count > 0;
		}
	}

	public bool	HasFollower (Transform target)
	{
		return mFollowers.Contains (target);
	}

	public bool	AttackOrStalk (Transform target, bool outerCircleOK, ref MapDirection direction)
	{
		bool result = false;
		//sees if any ground targets are open
		//adds target to dictionary if available, sets direction to open spot, returns true
		//adds target to outer circle if not available, returns false
		if (!HasNorthAttacker) {
			NorthAttacker = target;
			direction = MapDirection.A_North;
			result = true;
		} else if (!HasEastAttacker) {
			EastAttacker = target;
			direction = MapDirection.C_East;
			result = true;
		} else if (!HasSouthAttacker) {
			SouthAttacker = target;
			direction = MapDirection.E_South;
			result = true;
		} else if (!HasWestAttacker) {
			WestAttacker = target;
			direction = MapDirection.G_West;
			result = true;
		}

		if (result) {
			target.parent = tr;
			if (!mGroundAttackers.Contains (target)) {
				mGroundAttackers.Add (target);
			}
			//mGroundAttackers.Remove (target);
			mFollowers.Remove (target);
			mGroundStalkers.Remove (target);
			mCompanions.Remove (target);
		} else if (outerCircleOK) {
			direction = MapDirection.I_None;
			AddGroundStalker (target);
			result = true;
		}

		CheckTargetUpdater ();
		return result;
	}

	public void	AddGroundFollower (Transform target)
	{
		//if we're not already being followed
		if (!mFollowers.Contains (target)) {	//parent under this object
			target.parent = tr;
			target.localPosition = Vector3.zero;
			mFollowers.Add (target);

			mGroundAttackers.Remove (target);
			//mFollowers.Remove (target);
			mGroundStalkers.Remove (target);
			mCompanions.Remove (target);
		}
		CheckTargetUpdater ();
	}

	public void	AddGroundStalker (Transform target)
	{
		//if we're not already managing it
		if (target != null && target.parent != tr && !mGroundStalkers.Contains (target)) {	//parent under this object
			target.parent = tr;
			mGroundStalkers.Add (target);
	
			mGroundAttackers.Remove (target);
			mFollowers.Remove (target);
			//mGroundStalkers.Remove (target);
			mCompanions.Remove (target);
		}
		CheckTargetUpdater ();
	}

	public void	RemoveGroundStalker (Transform target)
	{
		//if we're managing it
		if (target != null && target.parent == tr) {	//release it
			target.parent = null;
			mGroundStalkers.Remove (target);
		}
		CheckTargetUpdater ();
	}
	//add a target that will automatically try to occupy the inner circle
	//as spots open up they will jockey for position
	public bool	AddGroundAttacker (Transform target, ref MapDirection direction)
	{
		return AttackOrStalk (target, false, ref direction);
	}

	public void	RemoveAttacker (Transform target)
	{
		//set the parent to null in any case
		if (target.parent == tr) {
			target.parent = null;
		}
		CheckTargetUpdater ();
	}

	protected float targetDistanceStep;
	protected float targetDistance;
	protected float targetRotateStep;
	protected float targetRotate;
	protected float currentFollowDistance;
	protected Vector3 newTargetPosition;

	public void Update ()
	{
		if (mGroundStalkers.Count > 0) {
			if (SpotHelper == null) {
				SpotHelper = new GameObject ("SpotHelper-" + name).transform;
			}

			targetDistanceStep = (OuterCircleMaxDistance - OuterCircleMinDistance) / mGroundStalkers.Count;
			targetDistance = OuterCircleMinDistance;
			targetRotateStep = (OuterCircleMaxRotate - OuterCircleMinRotate) / mGroundStalkers.Count;
			targetRotate = OuterCircleMinRotate;

			for (int i = mGroundStalkers.Count - 1; i >= 0; i--) {
				//if it's been destroyed or moved
				if (mGroundStalkers [i] == null || mGroundStalkers [i].parent != tr) {	//get rid of it
					mGroundStalkers.RemoveAt (i);
				} else {//otherwise update it normally
					Transform currentTarget = mGroundStalkers [i];
					//temporarily unparent the object and reset the rotation
					float wave = Mathf.Sin ((float)WorldClock.AdjustedRealTime + (1.234f * i));
					SpotHelper.position = tr.position + (Vector3.forward * (targetDistance + wave));
					SpotHelper.rotation = Quaternion.identity;
					if (i % 2 != 0) {//if it's an odd number, rotate normally
						SpotHelper.RotateAround (tr.position, Vector3.up, (float)(targetRotate * (WorldClock.AdjustedRealTime + wave)));//then rotate it
					} else {//if it's an even number reverse rotation
						SpotHelper.RotateAround (tr.position, Vector3.up, (float)-(targetRotate * (WorldClock.AdjustedRealTime + wave)));
					}
					currentTarget.position = SpotHelper.position;
					currentTarget.rotation = SpotHelper.rotation;
					//rotate it around the target based on time
				}
				//up the target distance and reverse direction
				targetDistance += targetDistanceStep;
				targetRotate += targetRotateStep;
			}
		}

		if (mGroundAttackers.Count > 0) {
			if (HasNorthAttacker) {
				ApplyAttackerDirection (NorthAttacker, Vector3.forward);
			}
			if (HasEastAttacker) {
				ApplyAttackerDirection (EastAttacker, Vector3.right);
			}
			if (HasSouthAttacker) {
				ApplyAttackerDirection (SouthAttacker, Vector3.back);
			}
			if (HasWestAttacker) {
				ApplyAttackerDirection (WestAttacker, Vector3.left);
			}

			for (int i = mGroundAttackers.LastIndex (); i >= 0; i--) {
				if (mGroundAttackers [i] == null || mGroundAttackers [i].parent != tr) {
					mGroundAttackers.RemoveAt (i);
				}
			}
		}
	}

	public void FixedUpdate ()
	{
		if (mCompanions.Count > 0) {
			for (int i = mCompanions.LastIndex (); i >= 0; i--) {
				//if it's been destroyed or moved
				if (mCompanions [i] == null || mCompanions [i].parent != tr) {	//get rid of it
					mCompanions.RemoveAt (i);
				} else {
					Transform currentTarget = mCompanions [i];
					//companions always follow the main target object
					currentTarget.position = tr.position;
				}
			}
		}

		if (mFollowers.Count > 0) {
			currentFollowDistance = FollowSeparation;
			for (int i = mFollowers.Count - 1; i >= 0; i--) {
				if (mFollowers [i] == null || mFollowers [i].parent != tr) {	//if it's gone or we're not updating it
					//remove it
					mFollowers.RemoveAt (i);
				} else {//otherwise update it//followers are always behind us, so use backwards direction
					//randomize a bit to add variety
					newTargetPosition = (tr.position + (tr.forward * currentFollowDistance)) + Vector3.up;
					mFollowers [i].position = newTargetPosition;
					currentFollowDistance += FollowSeparation;
				}
			}
		}
	}

	protected void ApplyAttackerDirection (Transform target, Vector3 direction)
	{
		target.position = tr.position + (direction * InnerCircleDistance);// + Vector3.up;
		//orient the target to us
		target.LookAt (tr.position, Vector3.up);
	}

	protected void CheckTargetUpdater ()
	{
		if (HasTargets) {
			enabled = true;
		}
	}

	protected bool mIsUpdatingTargets	= false;
	protected List <Transform> mFollowers = new List <Transform> ();
	//holds attacker transforms that drive away agents
	protected List <Transform> mGroundAttackers = new List <Transform> ();
	//holds agent transforms that can't fit in main circle
	protected List <Transform> mGroundStalkers = new List <Transform> ();
	//holds companion agents that follow the main object
	protected List <Transform> mCompanions = new List<Transform> ();

	void OnDrawGizmos ()
	{
		foreach (Transform follower in mFollowers) {
			Color gc = Color.cyan;
			gc.a = 0.35f;
			Gizmos.color = gc;
			Gizmos.DrawSphere (follower.position, 1.0f);
		}

		foreach (Transform attacker in mGroundAttackers) {
			Color gc = Color.blue;
			gc.a = 0.35f;
			Gizmos.color = gc;
			Gizmos.DrawSphere (attacker.position, 1.0f);
		}

		int targetHolder = 1;
		foreach (Transform groundTargetOuter in mGroundStalkers) {
			Color gc = Color.Lerp (Color.yellow, Color.red, ((float)targetHolder / (float)mGroundStalkers.Count));
			gc.a = 0.35f;
			Gizmos.color = gc;
			Gizmos.DrawSphere (groundTargetOuter.position, 1.0f);
			DrawArrow.ForGizmo (groundTargetOuter.position, groundTargetOuter.forward, 0.25f, 20);
			targetHolder++;
		}

		foreach (Transform attacker in mGroundAttackers) {
			Color gc = Color.red;
			gc.a = 0.55f;
			Gizmos.color = gc;
			Gizmos.DrawSphere (attacker.position, 0.8f);
			DrawArrow.ForGizmo (attacker.position, attacker.forward, 0.25f, 20);
		}
	}
}