using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers.GUI;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
		public class TreeCollider : TreeColliderTemplate, IAudible, IItemOfInterest, IDamageable
		{
				public void Awake()
				{
						Physics.IgnoreCollision(MainCollider, SecondaryCollider);
						MainCollider.tag = "ColliderTreeMain";
						SecondaryCollider.tag = "ColliderTreeSecondary";
						MainCollider.isTrigger = false;
						SecondaryCollider.isTrigger = true;
						LastSoundType = MasterAudio.SoundType.Foliage;
						mIgnoreColliders.Add(MainCollider);
						mIgnoreColliders.Add(SecondaryCollider);
				}

				public void Refresh()
				{
						if (MainColliderFlags == TreeColliderFlags.None || Flags.Check((uint)MainColliderFlags, (uint)TreeColliderFlags.Ignore, Flags.CheckType.MatchAny)) {
								MainCollider.enabled = false;
						} else {
								MainCollider.enabled = true;
								MainCollider.radius = MainRadius;
								MainCollider.height = MainHeight;
								MainCollider.transform.localPosition = MainOffset;
								if (Flags.Check((uint)MainColliderFlags, (uint)TreeColliderFlags.Solid, Flags.CheckType.MatchAny)) {
										MainCollider.isTrigger = false;
										MainCollider.gameObject.layer = Globals.LayerNumObstacleTerrain;
								} else {
										MainCollider.isTrigger = true;
										MainCollider.gameObject.layer = Globals.LayerNumTrigger;
								}
						}

						if (SecondaryColliderFlags == TreeColliderFlags.None || Flags.Check((uint)SecondaryColliderFlags, (uint)TreeColliderFlags.Ignore, Flags.CheckType.MatchAny)) {
								SecondaryCollider.enabled = false;
						} else {
								SecondaryCollider.enabled = true;
								SecondaryCollider.radius = SecondaryRadius;
								SecondaryCollider.height = SecondaryHeight;
								SecondaryCollider.transform.localPosition = SecondaryOffset;
								if (Flags.Check((uint)SecondaryColliderFlags, (uint)TreeColliderFlags.Solid, Flags.CheckType.MatchAny)) {
										SecondaryCollider.isTrigger = false;
										SecondaryCollider.gameObject.layer = Globals.LayerNumObstacleTerrain;
								} else {
										SecondaryCollider.isTrigger = true;
										SecondaryCollider.gameObject.layer = Globals.LayerNumTrigger;
								}
						}
				}

				public void OnTriggerEnter(Collider other)
				{
						if (other.isTrigger) {
								return;
						}

						//triggers can impede
						switch (other.gameObject.layer) {
								case Globals.LayerNumPlayer:
										CheckAgainst(SecondaryCollider, other, SecondaryColliderFlags);
										break;

								case Globals.LayerNumWorldItemActive:
										CheckAgainst(SecondaryCollider, other, SecondaryColliderFlags);
										break;

								default:
										break;
						}
				}

				public void OnCollisionEnter(Collision collision)
				{
						if (collision.collider.isTrigger)
								return;

						//collisions can't impede so both will just rustle
						switch (collision.collider.gameObject.layer) {
								case Globals.LayerNumPlayer:
								case Globals.LayerNumWorldItemActive:
										CheckAgainst(MainCollider, collision.collider, MainColliderFlags);
										break;

								default:
										break;
						}
				}

				public void CopyFrom(TreeColliderTemplate template)
				{
						if (template == null) {
								Debug.Log("TEMPLATE WAS NULL IN TREE COLLIDER");
								return;
						}

						MainRadius = template.MainRadius;
						MainHeight = template.MainHeight;
						MainOffset = template.MainOffset;
						MainColliderFlags = template.MainColliderFlags;

						SecondaryRadius = template.SecondaryRadius;
						SecondaryHeight = template.SecondaryHeight;
						SecondaryOffset = template.SecondaryOffset;
						SecondaryColliderFlags = template.SecondaryColliderFlags;

						SpawnCategory = template.SpawnCategory;
						SpawnDamageMaximum = template.SpawnDamageMinimum;
						SpawnDamageMaximum = template.SpawnDamageMaximum;
						DepletedOnMaxDamage = template.DepletedOnMaxDamage;
						IntrospectionOnDepleted = template.IntrospectionOnDepleted;

						//Debug.Log ("OnGainPlayerFocus");
						if (string.IsNullOrEmpty(template.SpawnDescription)) {
								WICategory cat = null;
								if (WorldItems.Get.Category(SpawnCategory, out cat)) {
										cat.RefreshDisplayNames();
										List <string> stuffInCategory = new List<string>();
										for (int i = 0; i < cat.GenericWorldItems.Count; i++) {
												stuffInCategory.SafeAdd(cat.GenericWorldItems[i].DisplayName);
										}
										SpawnDescription = Data.GameData.CommaJoinWithLast(stuffInCategory, "&");
								}
						}


						TotalDamage = 0f;

						Refresh();
				}

				protected void CheckAgainst(Collider treeCollider, Collider otherCollider, TreeColliderFlags flags)
				{
						if (Flags.Check((uint)flags, (uint)TreeColliderFlags.Rustle, Flags.CheckType.MatchAny)) {
								if (!mRustling) {
										mRustling = true;
										StartCoroutine(RustleOverTime(otherCollider));
								}
						}
				}

				protected IAudible audibleThing;

				protected IEnumerator RustleOverTime(Collider otherCollider)
				{
						if (otherCollider != null) {
								//if the thing that's rustling us is an IAudible, we want it to make the noise, not us
								audibleThing = (IAudible)otherCollider.GetComponent(typeof(IAudible));
								if (audibleThing == null) {
										audibleThing = this;
								}
						} else {
								audibleThing = this;
						}

						AudioManager.MakeWorldSound(audibleThing, mIgnoreColliders, LastSoundType);
						windZone.transform.localScale = Vector3.one * SecondaryCollider.radius * 2;
						windZone.transform.localPosition = SecondaryCollider.transform.localPosition;

						windZone.GetComponent<Animation>().enabled = true;
						windZone.GetComponent<Animation>()["WindZoneRustle"].wrapMode = WrapMode.ClampForever;
						windZone.GetComponent<Animation>().Rewind("WindZoneRustle");
						windZone.GetComponent<Animation>().Play("WindZoneRustle");

						while (windZone.GetComponent<Animation>().IsPlaying("WindZoneRustle")) {
								yield return null;
								if (windZone.GetComponent<Animation>()["WindZoneRustle"].normalizedTime > 1f) {
										windZone.GetComponent<Animation>().enabled = false;
										yield break;
								}
						}
						mRustling = false;
						yield break;
				}

				public WorldChunk ParentChunk;

				#region damageable

				public IItemOfInterest LastDamageSource { get; set; }

				public BodyPart LastBodyPartHit { get; set; }

				public bool IsDead { get { return false; } }

				public float NormalizedDamage { get { return TotalDamage / SpawnDamageMinimum; } }

				public WIMaterialType BaseMaterialType { get { return WIMaterialType.Wood; } }

				public WIMaterialType ArmorMaterialTypes { get { return WIMaterialType.None; } }

				public int ArmorLevel(WIMaterialType materialType)
				{
						return 0;
				}

				public bool SpawnOnDamaage { 
						get {
								return !string.IsNullOrEmpty(SpawnCategory);
						}
				}

				public bool HasPlayerFocus {
						get {
								return mHasPlayerFocus;
						}
						set {
								if (!mHasPlayerFocus) {
										if (value) {
												OnGainPlayerFocus();
										}
								}
								mHasPlayerFocus = value;
						}
				}

				public virtual void InstantKill (IItemOfInterest causeOfDeath) {
						return;
				}

				public virtual bool TakeDamage(WIMaterialType materialType, Vector3 damagePoint, float attemptedDamage, Vector3 attemptedForce, string damageSource, out float actualDamage, out bool isDead)
				{
						actualDamage = attemptedDamage;
						isDead = false;
						TotalDamage += actualDamage;

						if (!mRustling) {
								StartCoroutine(RustleOverTime(null));
						}

						if (HasPlayerFocus) {
								GUIHud.Get.ShowProgressBar(Colors.Get.GeneralHighlightColor, Colors.Darken(Colors.Get.GenericNeutralValue), NormalizedDamage);
						}

						if (SpawnOnDamaage && TotalDamage >= SpawnDamageMinimum) {	//reset
								TotalDamage = 0f;
								//spawn a thing
								WorldItem worlditem = null;
								if (WorldItems.CloneRandomFromCategory(SpawnCategory, ParentChunk.AboveGroundGroup, out worlditem)) {
										//spit it out at the player
										Vector3 direction = (Player.Local.Position - damagePoint).normalized * 5f;
										worlditem.tr.parent = ParentChunk.AboveGroundGroup.transform;
										worlditem.tr.position = damagePoint;
										worlditem.Props.Local.FreezeOnStartup = false;
										worlditem.Props.Local.Transform.CopyFrom(worlditem.tr);
										worlditem.Initialize();
										worlditem.SetMode(WIMode.World);
										worlditem.ApplyForce(direction, damagePoint);
								}
						}
						return true;
				}

				public virtual void OnGainPlayerFocus()
				{
						if (!mUpdatingHudTarget) {
								mUpdatingHudTarget = true;
								if (mHudTarget == null) {
										mHudTarget = new GameObject("DamageableSceneryHudTarget").transform;
								}
								mHudTarget.position = Player.Local.Surroundings.ClosestObjectFocusHitInfo.point;
								mHasWeaponEquipped = false;
								mHadWeaponEquippedLastFrame = false;
								StartCoroutine(UpdateHudTarget());
						}
				}

				#endregion

				protected IEnumerator UpdateHudTarget()
				{
						yield return null;
						while (HasPlayerFocus) {
								mHasWeaponEquipped = Player.Local.Tool.HasWorldItem && Player.Local.Tool.worlditem.Is <Weapon>();
								if (mHasWeaponEquipped && !mHadWeaponEquippedLastFrame) {
										mHudTarget.position = Player.Local.Surroundings.ClosestObjectFocusHitInfo.point;
										GUIHud.Get.ShowAction(this, UserActionType.ToolUse, "Shake", mHudTarget, GameManager.Get.GameCamera);
								}
								mHudTarget.position = Vector3.Lerp(mHudTarget.position, Player.Local.Surroundings.ClosestObjectFocusHitInfo.point, 0.35f);
								mHadWeaponEquippedLastFrame = mHasWeaponEquipped;
								yield return null;
						}
						mUpdatingHudTarget = false;
						yield break;
				}

				protected bool mHasPlayerFocus = false;
				protected bool mHasWeaponEquipped = false;
				protected bool mHadWeaponEquippedLastFrame = false;
				protected bool mUpdatingHudTarget = false;
				protected bool mRustling = false;
				protected static Transform mHudTarget = null;
				protected static string gSpawnDescription;
				public GameObject windZone;
				public CapsuleCollider MainCollider;
				public CapsuleCollider SecondaryCollider;
				//convenience property for quad tree
				public Vector3 Position {
						get { return transform.position; }
						set { transform.position = value; }
				}

				public Vector3 FocusPosition {
					get { return transform.position; }
					set { transform.position = value; }
				}

				#region IAudible implementation

				public ItemOfInterestType IOIType { get { return ItemOfInterestType.Scenery; } }

				public bool Has(string scriptName)
				{
						return false;
				}

				public bool HasAtLeastOne(List <string> scriptNames)
				{
						return scriptNames == null || scriptNames.Count == 0;
				}

				public WorldItem worlditem { get { return null; } }

				public PlayerBase player { get { return null; } }

				public ActionNode node { get { return null; } }

				public WorldLight worldlight { get { return null; } }

				public Fire fire { get { return null; } }

				public bool IsAudible { get { return true; } }

				public float AudibleRange { get { return Globals.MaxAudibleRange; } }

				public float AudibleVolume { get { return 1.0f; } }

				public MasterAudio.SoundType LastSoundType { get; set; }

				public string LastSoundName { get; set; }

				public bool Destroyed { get { return false; } }

				public void ListenerFailToHear()
				{

				}

				#endregion

				protected List <Collider> mIgnoreColliders = new List<Collider>();
		}
}