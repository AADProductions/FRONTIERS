using UnityEngine;
using System.Collections;
using Frontiers;
#pragma warning disable 0414

public class AvatarAnimation : PlayerScript
{	
//	public Transform followObject;
//	
//	public LayerMask groundLayers = -1;
//	public LayerMask wallRunLayers = -1;
//	public bool canRotate = true;
//	public float rotSpeed = 90.0f;
//	public float baseDrag = 0;
//	public float animSpeed = 1.2f;
//	public float moveSpeed = 1.5f;
//	public bool canJump = true;
//	public float jumpHeight = 4.0f;
//	public float groundedDistance = 1.5f; // from capsule center
//	public float setFloatDampTime = 0.15f;
//		
//	Transform[] bones;
//	//Transform rootB = null;
//	
//	public CapsuleCollider col;
//	
//	public GameObject RightHandObject;
//	public GameObject RightHandTarget;
//	
//	//float groundTime = 0;
//	
//	// Double Tap ----------------------- //
//	public bool canEvade = true;
//	public float doubleTapSpeed = 0.2f; // Time between the taps frames*2
//	//bool isDoubleTap = false;
//	//bool is2Fwd = false;
//	//bool is2Back = false;
//	//bool is2Left = false;
//	//bool is2Right = false;
//	
//	// WallRun ----------------------- //
//	public bool canWallRun = true;	
//	
//	protected Animator a;
//	Transform hero;
//	Rigidbody rb;
//	RaycastHit groundHit;
//	//bool canNextWeapon = true;
//	//bool canDrawHolster = true;
//	//bool climbUp = false; // Up or Down?
//	//bool climbLong = false; // Short or Long?
//	//bool isClimb = false;
//	
//	// Cached Input or AI  ----------------------- //
//	[HideInInspector]
//	public float h;
//	[HideInInspector]
//	public float v;
//	[HideInInspector]
//	public float mX;
//	bool doJumpDown;
//	//bool doJump;
//	bool doAtk1Down;
//	bool doAtk1;
//	bool doAtk2Down;
//	bool doAtk2;
//	bool doFwd;
//	bool doBack;
//	bool doLeft;
//	bool doRight;
//	bool doNextWeapon;
//	bool doCombat;
//	bool doFly;
//	bool doClimb;
//	bool doWalk;
//	bool doSprint;
//	bool doSneak;
//	bool doLShift;
//	bool doDance1;	
//	bool doDance2;
//	bool doDance3;
//	bool doPullLever;
//	bool doPushButton;
//	bool doThrow;
//	
//	AnimatorStateInfo st;
//	
//	public PlayerAvatarState AvatarState = PlayerAvatarState.Base;
//	public enum WeaponState
//	{
//		None,
//		Sword,
//		Bow,
//		Rifle,
//		Pistol,
//		Unarmed,
//		Throw
//	}
//	public WeaponState weaponState = WeaponState.None;
//	
//	public void Start ( )
//	{
//		Player.Get.AvatarActions.Subscribe (AvatarAction.Move, 							new ActionListener (Move));
//		Player.Get.AvatarActions.Subscribe (AvatarAction.MoveJump, 						new ActionListener (MoveJump));
//		Player.Get.AvatarActions.Subscribe (AvatarAction.MoveLandOnGround,				new ActionListener (MoveLandOnGround));
//		Player.Get.AvatarActions.Subscribe (AvatarAction.MoveWalk,						new ActionListener (MoveWalk));
//		Player.Get.AvatarActions.Subscribe (AvatarAction.MoveSprint,					new ActionListener (MoveSprint));
//		Player.Get.AvatarActions.Subscribe (AvatarAction.MoveSprintFaster,				new ActionListener (MoveSprintFaster));
//		Player.Get.AvatarActions.Subscribe (AvatarAction.MoveCrouch,					new ActionListener (MoveCrouch));
//		Player.Get.AvatarActions.Subscribe (AvatarAction.MoveStand,						new ActionListener (MoveStand));
//		Player.Get.AvatarActions.Subscribe (AvatarAction.MoveStopMoving,				new ActionListener (MoveStopMoving));
//		
//		Player.Get.AvatarActions.Subscribe (AvatarAction.SurvivalTakeDamage, 			new ActionListener (SurvivalTakeDamage));
//		Player.Get.AvatarActions.Subscribe (AvatarAction.SurvivalTakeDamageCritical, 	new ActionListener (SurvivalTakeDamageCritical));
//		Player.Get.AvatarActions.Subscribe (AvatarAction.SurvivalTakeDamageOverkill, 	new ActionListener (SurvivalTakeDamageOverkill));
//		Player.Get.AvatarActions.Subscribe (AvatarAction.SurvivalKilledByDamage, 	new ActionListener (SurvivalKilledByDamage));
//
//		a 		= GetComponent<Animator>();
//		hero 	= GetComponent<Transform>();
////		rb 		= GetComponent<Rigidbody>();				
//		a.speed = animSpeed;
//		
//		if (a.layerCount > 1)
//		{
//			a.SetLayerWeight(1, 1.0f); // Leg layer, IK feet placement
//		}
////		col.center = new Vector3(0, 1, 0);
////		col.height = 2.0f;
//		
////		rb.mass = 3.0f;
////		rb.constraints = RigidbodyConstraints.FreezeRotation;
//		
////		Physics.IgnoreLayerCollision(Globals.LayerPlayerTool,Globals.Layer); // ignore player / climb collision
////		// Climb
////		cacheDist = climbCheckDistance;
////		// Swim
////		cam = GameObject.FindGameObjectWithTag("MainCamera"); // Your characters's camera tag
////		bones = GetComponentsInChildren<Transform> () as Transform[];
////		foreach (Transform t in bones)
////		{
////			if(t.name == "root") // Enter your root bone name here
////				rootB = t;
////			if(t.rigidbody && t.rigidbody != rb)
////				t.rigidbody.isKinematic = true;
////			if(t.collider && t.collider != hero.collider)
////				t.collider.isTrigger = true;
////		}
////		liftVector = new Vector3 (0, 2.0f, 0);
//	}
//	
//	public void FixedUpdate ( )
//	{
//		transform.localPosition = followObject.localPosition;
//		transform.localRotation = followObject.localRotation;		
//	}
//	
//	float mUnGroundedForTime = 0f;
//	bool mUnGroundedBefore = false;
//	
//	public void Update ( )
//	{		
//		bool smoothUnGrounded = true;
//		if (!Player.Local.IsGrounded)
//		{
//			if (!mUnGroundedBefore)
//			{
//				mUnGroundedForTime = Time.time + 0.5f;
//				mUnGroundedBefore = true;
//			}
//			else if (mUnGroundedForTime >= Time.time)
//			{
//				smoothUnGrounded = false;
//			}
//		}
//		else
//		{
//			mUnGroundedBefore  = false;
//		}
//		
//		
//		h = Player.Local.HorizontalMovementLastFrame * 10.0f;
//		v = Player.Local.VerticalMovementLastFrame * 10.0f;
//		
////		// Set Animator parameters
//		if (doFwd)
//		{
//			if (doSprint)
//			{
//				a.SetFloat ("AxisY", 2.0f, setFloatDampTime, Time.deltaTime);
//			}
//			else
//			{
//				a.SetFloat ("AxisY", 1.0f, setFloatDampTime, Time.deltaTime);
//			}
//		}
//		else if (doBack)
//		{
//			a.SetFloat ("AxisY", -1.0f, setFloatDampTime, Time.deltaTime);
//		}
//		else
//		{
//			a.SetFloat ("AxisY", 0.0f, setFloatDampTime, Time.deltaTime);
//		}
////		a.SetFloat("MouseX", mX, setFloatDampTime * 4, Time.deltaTime);
//		a.SetFloat("AxisX", 0f);//, setFloatDampTime, Time.deltaTime);
//		a.SetBool ("Grounded", smoothUnGrounded);
//		
//		switch (AvatarState)
//		{
//		case PlayerAvatarState.Base:
//			UpdateBase ( );
//			break;
//			
//		default:
//			break;
//		}
//	}
//	
//	public void LateUpdate ( )
//	{
//		if (RightHandTarget != null)
//		{
//			RightHandObject.transform.position = RightHandTarget.transform.position;
//		}
//	}
//	
//	#region avatar action listeners
//	public bool Move (double timeStamp)
//	{
//		// Grab Input each frame --- Handy for your custom input setting and AI
////		doLShift = Input.GetKey(KeyCode.LeftShift);
////		mX = doLShift ? 0 : Input.GetAxis("Mouse X"); // Mouse X is 0 if leftShift is held down
////		h = Input.GetAxis("Horizontal");
////        v = Input.GetAxis("Vertical");	
////		doJumpDown = Input.GetButtonDown("Jump");
////		//doJump = Input.GetButton("Jump");
////		doAtk1Down = Input.GetMouseButtonDown(0);
////		doAtk1 = Input.GetMouseButton(0);
////		doAtk2Down = Input.GetMouseButtonDown(1);
////		doAtk2 = Input.GetMouseButton(1);
////		doFwd = Input.GetKeyDown(KeyCode.W);
////		doBack = Input.GetKeyDown(KeyCode.S);
////		doLeft = Input.GetKeyDown(KeyCode.A);
////		doRight = Input.GetKeyDown(KeyCode.D);
////		doNextWeapon = Input.GetKeyDown(KeyCode.Q);
////		doCombat = Input.GetKeyDown(KeyCode.C);
////		doFly = Input.GetKeyDown(KeyCode.Z);
////		doClimb = Input.GetKeyDown(KeyCode.E);
////		doWalk = Input.GetKeyDown(KeyCode.X);
////		doSprint = Input.GetKeyDown(KeyCode.LeftShift);
////		doSneak = Input.GetKeyDown(KeyCode.V);
////		doDance1 = Input.GetKeyDown(KeyCode.H);	
////		doDance2 = Input.GetKeyDown(KeyCode.J);
////		doDance3 = Input.GetKeyDown(KeyCode.K);
////		doPullLever = Input.GetKeyDown(KeyCode.L);
////		doPushButton = Input.GetKeyDown(KeyCode.P);
////		doThrow = Input.GetKeyDown(KeyCode.G);		
//		return true;
//	}
//	public bool MoveCrouch (double timeStamp)
//	{
//		doSneak = true;
//		return true;
//	}
//	public bool MoveStand (double timeStamp)
//	{
//		doSneak = false;
//		return true;
//	}
//	public bool MoveStopMoving (double timeStamp)
//	{
//		doFwd = false;
//		return true;
//	}
//	public bool MoveJump (double timeStamp)
//	{
//		doJumpDown = true;
//		return true;
//	}
//	public bool MoveLandOnGround (double timeStamp)
//	{
//		doJumpDown = false;
//		return true;
//	}
//	public bool MoveWalk (double timeStamp)
//	{
//		doWalk	 	= true;
//		doFwd 		= true;
//		return true;
//	}
//	public bool MoveSprint (double timeStamp)
//	{
//		doFwd		= true;
//		doSprint 	= true;
//		return true;
//	}
//	public bool MoveSprintFaster (double timeStamp)
//	{
//		doFwd		= true;
//		doSprint	= true;
//		return true;
//	}
//	public bool SurvivalTakeDamage (double timeStamp)
//	{
////		a.SetFloat ("Speed", 0.0f, 0.1f, Time.deltaTime);
//		return true;
//	}
//	public bool SurvivalTakeDamageCritical (double timeStamp)
//	{
////		a.SetFloat ("Speed", 0.0f, 0.1f, Time.deltaTime);
//		return true;
//	}
//	public bool SurvivalTakeDamageOverkill (double timeStamp)
//	{
////		a.SetFloat ("Speed", 0.0f, 0.1f, Time.deltaTime);
//		return true;
//	}
//	public bool SurvivalKilledByDamage (double timeStamp)
//	{
////		a.SetFloat ("Speed", 0.0f, 0.1f, Time.deltaTime);
//		return true;
//	}
//	#endregion
//	
//	public void UpdateBase ( )
//	{
//		if (Player.Local.IsGrounded)
//		{			
//			a.SetBool ("Jump", false);
//			a.SetBool ("CanLand", true);
//			
//			if (doSneak)
//			{
//				a.SetBool("Sneaking", true);
//				a.SetBool("Walking", false); // RESET
//				a.SetBool("Sprinting", false); // RESET				
//			}
//			else if (doWalk)
//			{
//				a.SetBool("Sneaking", false); // RESET
//				a.SetBool("Walking", true);
//				a.SetBool("Sprinting", false); // RESET				
//			}
//			else if (doSprint)
//			{
//				a.SetBool("Sneaking", false); // RESET
//				a.SetBool("Walking", false); // RESET
//				a.SetBool("Sprinting", true);
//			}
//			else
//			{
//				
//			}
//		}
//		else
//		{
//			a.SetBool ("Jump", true);
//		}
//		
////		if(Player.Local.IsGrounded)
////		{
////			if(doJumpDown && canJump && !st.IsTag("Jump") && !st.IsTag("Land"))
////			{
////				a.SetBool("Jump", true);
////				//add extra force to main jump
////				if(!st.IsTag("LedgeJump"))
////					rb.velocity = hero.up * jumpHeight;
////				// Start cooldown until we can jump again
////				//StartCoroutine (JumpCoolDown(0.5f));
////			}
////			
////			// Don't slide
////			if(!rb.isKinematic)
////				rb.velocity = new Vector3(0, rb.velocity.y, 0);
////			
////			// Extra rotation
////			if(canRotate)
////			{
////				if(!doLShift)
////					hero.Rotate(0, mX * rotSpeed/2 * Time.deltaTime, 0);
////			}
////			
////			// Punch, Kick if weapon state is not = None
////			if(weaponState != WeaponState.None)
////			{
////				if(doAtk1Down && !st.IsName("PunchCombo.Punch1"))
////				{
////					a.SetBool("Attack1", true);
////					a.SetBool("Walking", false); // RESET
////					a.SetBool("Sprinting", false); // RESET
////					a.SetBool("Sneaking", false); // RESET
////				}
////				else if(doAtk2Down && !st.IsName("KickCombo.Kick1"))
////				{
////					a.SetBool("Attack2", true);
////					a.SetBool("Walking", false); // RESET
////					a.SetBool("Sprinting", false); // RESET
////					a.SetBool("Sneaking", false); // RESET
////				}
////			}
////			
////			
////			// Walk
////			if(doWalk)
////			{
////				if(!st.IsName("WalkTree.TreeW"))
////				{
////					a.SetBool("Walking", true);
////					a.SetBool("Sneaking", false); // RESET
////					a.SetBool("Sprinting", false); // RESET
////				}
////				else
////				{
////					a.SetBool("Walking", false); // RESET
////				}
////			}
////			
////			// Sprint
////			else if(doSprint)
////			{
////				if(!st.IsName("SprintTree.TreeS"))
////				{
////					a.SetBool("Sprinting", true);
////					a.SetBool("Walking", false); // RESET
////					a.SetBool("Sneaking", false); // RESET
////				}
////				else
////				{ 
////					a.SetBool("Sprinting", false); // RESET
////				}
////			}
////			
////			// Sneak
////			else if(doSneak)
////			{
////				if(!st.IsName("SneakTree.TreeSn"))
////				{
////					a.SetBool("Sneaking", true);
////					a.SetBool("Walking", false); // RESET
////					a.SetBool("Sprinting", false); // RESET
////				}
////				else
////				{
////					a.SetBool("Sneaking", false); // RESET
////				}
////			}
////			
//////			WallGround ();
////			
////			// Balanceing trigger
////			if(groundHit.transform && groundHit.transform.gameObject.layer == 9)
////			{
////				// Layer 9 should be Climb
////				a.SetBool("Balancing", true);
////			}
////			else
////				a.SetBool("Balancing", false); // RESET
////			
////			
////			// -----------AirTime--------- //
////			if(!a.GetBool("CanLand")) // Very short air time
////			{
////				groundTime += Time.deltaTime;
////				if(groundTime >= 0.4f)
////				{
////					a.SetBool("CanLand", true);
////				}
////			}
////			else
////				groundTime = 0;
////			
////			// -----------AirTime--------- //
////			
////		}
////		else // In Air
////		{
////			// -----------AirTime--------- //
////			if(groundTime <= 0.3f)
////			{
////				groundTime += Time.deltaTime;
////				if(groundTime >= 0.2f)
////				{
////					a.SetBool("CanLand", true);
////				}
////				else
////					a.SetBool("CanLand", false);
////			}
////			// -----------AirTime--------- //
////			
////			
////			if(canRotate)
////				hero.Rotate(0, mX * rotSpeed/2 * Time.deltaTime, 0);
////			
//////			WallRun();
////			
////			// After jumping off from climb state controller
//////			if(jumpOffNext)
//////			{
//////				a.SetBool("Jump", true);
//////				jumpOffNext = false;
//////			}
////		}
////		
////		
////		
////		
////		// Resetting--------------------------------------------------
////		if(!a.IsInTransition(0))
////		{
////			if(st.IsTag("Jump") || st.IsTag("LedgeJump"))
////			{
////				// Reset our parameter to avoid looping
////            	a.SetBool("Jump", false); // RESET LedgeJump
////			}
////			
////			else if(st.IsTag("Dance"))
////			{
////				a.SetBool("Dance", false);
////			}
////			
////			else if(st.IsTag("Action"))
////			{
////				a.SetBool("Pull", false);
////				a.SetBool("Push", false);
////				a.SetBool("Throw", false);
////			}
////			
////			else if(st.IsTag("StandUp"))
////			{
////				a.SetInteger("RandomM", 3); // 0 or 1 are triggers
////				a.SetBool("StandUp", false); // RESET
////			}
////			
////			if(st.IsName("PunchCombo.Punch1") && st.normalizedTime > 0.7f && !doAtk1)
////			{
////				// Reset our parameter to avoid looping
////            	a.SetBool("Attack1", false); // RESET
////			}
////			else if(st.IsName("PunchCombo.Punch2"))
////			{
////				// Reset our parameter to avoid looping
////            	a.SetBool("Attack1", false); // RESET
////			}
////			
////			
////			
////			if(st.IsName("KickCombo.Kick1") && st.normalizedTime > 0.7f && !doAtk2)
////			{
////				// Reset our parameter to avoid looping
////            	a.SetBool("Attack2", false); // RESET
////			}
////			else if(st.IsName("KickCombo.Kick2"))
////			{
////				// Reset our parameter to avoid looping
////            	a.SetBool("Attack2", false); // RESET
////			}
////			
////			
////			if(st.IsTag("Evade"))
////			{
////				// Reset our parameter to avoid looping
////            	a.SetBool("Evade_F", false); // RESET
////				a.SetBool("Evade_B", false); // RESET
////				a.SetBool("Evade_L", false); // RESET
////				a.SetBool("Evade_R", false); // RESET
////			}
////
////			
////			if(st.IsTag("WallRun") && Player.Local.IsGrounded) // No instant reset
////			{
////				a.SetBool("WallRunL", false); // RESET
////				a.SetBool("WallRunR", false); // RESET
////				a.SetBool("WallRunUp", false); // RESET
////			}
////		}
////	}
//	}
}