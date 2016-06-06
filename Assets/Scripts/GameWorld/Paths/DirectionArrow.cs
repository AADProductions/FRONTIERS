using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.Data;
using System.Collections.Generic;
using System;

public class DirectionArrow : MonoBehaviour, IItemOfInterest
{
		public ItemOfInterestType IOIType { get { return ItemOfInterestType.Scenery; } }

		public Vector3 Position { get { return tr.position; } }

		public Vector3 FocusPosition { get { return tr.position; } }

		public bool Has(string scriptName)
		{
				return false;
		}

		public bool HasAtLeastOne(List <string> scriptNames)
		{
				if (scriptNames.Count == 0) {
						return true;
				}
				return false;
		}

		public WorldItem worlditem { get { return null; } }

		public PlayerBase player { get { return null; } }

		public ActionNode node { get { return null; } }

		public WorldLight worldlight { get { return null; } }

		public Fire fire { get { return null; } }

		public bool Destroyed { get { return mDestroyed; } }

		public bool HasPlayerFocus {
				get {
						return mHasPlayerFocus;
				}
				set {
						if (!mHasPlayerFocus) {
								if (value) {
										mHasPlayerFocus = value;
										OnGainPlayerFocus();
								}
						}
						mHasPlayerFocus = value;
				}
		}

		protected bool mHasPlayerFocus = false;
		//for directing characters to paths
		public Action OnGainFocus;
		public Vector3 TargetScale = Vector3.one;
		public bool HasBeenSelected = false;
		public bool Destroying = false;
		public float ChangeSpeed = 1f;
		public PlayerProjections Projections = null;
		public Material ArrowMaterial;
		public Transform ArrowTransform;
		public Color TargetColor;
		public BoxCollider Collider;
		public TravelManager.FastTravelChoice Choice;

		public void Awake()
		{
				tr = transform;
				rb = gameObject.GetOrAdd <Rigidbody>();

				Collider = gameObject.GetComponent <BoxCollider>();
				Collider.isTrigger = true;
				Collider.enabled = false;
				gameObject.layer = Globals.LayerNumScenery;
				//copy the material beause we'll need to change its color
				ArrowMaterial = new Material(Mats.Get.DirectionArrowMaterial);
				ArrowMaterial.color = Color.black;
				ArrowTransform.renderer.sharedMaterial = ArrowMaterial;
				ArrowTransform.localScale = new Vector3(1f, 0.001f, 1f);
				ArrowTransform.gameObject.layer = Globals.LayerNumScenery;

				TargetColor = Colors.Get.MessageInfoColor;
				ChangeSpeed = 0.1f;
		}

		public void Start()
		{
				//point at the direction
				PointAt(Choice.FirstInDirection.Position);
				//then put the newly-positioned arrow at the height of the surrounding terrain
				gArrowHit.feetPosition = ArrowTransform.position;
				gArrowHit.groundedHeight = 5f;
				gArrowHit.overhangHeight = 5f;
				gArrowHit.ignoreWorldItems = true;
				gArrowHit.feetPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition(ref gArrowHit);
				ArrowTransform.position = gArrowHit.feetPosition;
		}

		protected static GameWorld.TerrainHeightSearch gArrowHit;

		public void PointAt(Vector3 position)
		{
				//Debug.Log("Rotating to look at: " + position.ToString());
				//only rotate on Y axis
				tr.LookAt(position);
				Vector3 eulerAngles = tr.rotation.eulerAngles;
				eulerAngles.x = 0f;
				eulerAngles.z = 0f;
				tr.rotation = Quaternion.Euler(eulerAngles);
		}

		public void OnGainPlayerFocus()
		{
				//Debug.Log("Gained player focus in arrow");
		string directionName = WorldMap.GetMapDirectionNameFromRotation(ArrowTransform);
		string pathName = WorldItems.CleanWorldItemName (Choice.ConnectingPath) + "(" + directionName + ")";
		Frontiers.GUI.GUIHud.Get.ShowAction(this, UserActionType.ItemUse, pathName, ArrowTransform, GameManager.Get.GameCamera);
				OnGainFocus.SafeInvoke();
		}

		public void Update()
		{
				ArrowTransform.localScale = Vector3.Lerp(ArrowTransform.localScale, TargetScale, ChangeSpeed);
				ArrowMaterial.color = Color.Lerp(ArrowMaterial.color, TargetColor, ChangeSpeed);

				if (Destroying) {
						if (ArrowTransform.localScale.y < 0.01f) {
								enabled = false;
								GameObject.Destroy(gameObject, 0.01f);
						} else {
								ChangeSpeed = 0.1f;
								TargetScale.y = 0f;
								TargetColor = Color.black;
								Collider.enabled = false;
						}

				} else {
						Collider.enabled = true;
						if (HasPlayerFocus) {
								ChangeSpeed = 0.1f;
								TargetScale.y = 1.5f;
								TargetColor = Colors.Get.GeneralHighlightColor;
						} else {
								ChangeSpeed = 0.25f;
								TargetScale.y = 1f;
								TargetColor = Colors.Get.MessageInfoColor;
						}
				}
		}

		public void OnChooseDirection()
		{
				ShrinkAndDestroy();
				HasBeenSelected = true;
		}

		public void ShrinkAndDestroy()
		{
				Collider.enabled = false;
				transform.parent = null;
				Destroying = true;
		}

		public void OnDestroy()
		{
				mDestroyed = true;
		}

	#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
				Gizmos.color = Color.yellow;
				if (UnityEditor.Selection.activeGameObject == gameObject) {
						Gizmos.color = Color.red;
				}
				Gizmos.color = Colors.Alpha(Gizmos.color, 0.5f);
				Gizmos.DrawWireCube(Choice.FirstInDirection.Position, Vector3.one);
				Gizmos.DrawCube(Choice.EndMarker.Position, Vector3.one * 1.5f);
		}
	#endif

		protected bool mDestroyed = false;
		protected Rigidbody rb;
		protected Transform tr;
}
