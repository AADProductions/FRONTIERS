using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class RockslideRock : MonoBehaviour, IDamageable, IItemOfInterest
		{
				public static List <string> RockClips = new List<string> { "RockFall1", "RockFall2", "RockFall3", "RockFall4" };

				public IItemOfInterest LastDamageSource { get; set; }

				public bool IsDead { get { return false; } }

				public float NormalizedDamage { get { return 0f; } }

				public float TotalDamage = 0f;
				public float FollowStrength = 0.15f;

				public WIMaterialType BaseMaterialType { get { return WIMaterialType.Stone; } }

				public WIMaterialType ArmorMaterialTypes { get { return WIMaterialType.None; } }

				public int ArmorLevel(WIMaterialType materialType)
				{
						return 0;
				}

				public void OnCollisionEnter(Collision col)
				{
						if (col.gameObject.CompareTag(Globals.TagGroundTerrain)) {
								return;
						}

						if (WorldItems.GetIOIFromGameObject(col.gameObject, out mIoi)) {
								if (gDamagePackage == null) {
										gDamagePackage = new DamagePackage();
										gDamagePackage.DamageSent = Globals.DamageOnRockslideHit;
										gDamagePackage.SenderMaterial = WIMaterialType.Stone;
										gDamagePackage.Source = this;
								}
								gDamagePackage.ForceSent = col.relativeVelocity.magnitude * 0.1f;
								gDamagePackage.Point = tr.position;
								gDamagePackage.Target = mIoi;
								DamageManager.Get.SendDamage(gDamagePackage);
						}
				}

				public void Awake()
				{
						tr = transform;
						rb = rigidbody;
						tr.localScale = tr.localScale * UnityEngine.Random.Range(0.75f, 1.25f);
				}

				protected DamagePackage gDamagePackage;
				protected IItemOfInterest mIoi;
				protected Transform tr;
				protected Rigidbody rb;
				protected double mTimeForNextClip;

				public void FixedUpdate()
				{
						rb.AddForce(Vector3.Normalize(Player.Local.ChestPosition - tr.position) * FollowStrength);

						if (WorldClock.AdjustedRealTime > mTimeForNextClip) {
								mTimeForNextClip = WorldClock.AdjustedRealTime + UnityEngine.Random.value;
								MasterAudio.PlaySound(MasterAudio.SoundType.Damage, tr, RockClips[UnityEngine.Random.Range(0, RockClips.Count)]);
						}
				}

				public virtual bool TakeDamage(WIMaterialType materialType, Vector3 damagePoint, float attemptedDamage, Vector3 attemptedForce, string damageSource, out float actualDamage, out bool isDead)
				{
						actualDamage = attemptedDamage;
						isDead = false;
						TotalDamage += actualDamage;
						return true;
				}

				#region IItemOfInterest implementation

				public ItemOfInterestType IOIType { get { return ItemOfInterestType.Scenery; } }

				public Vector3 Position { get { return transform.position; } }

				public bool Has(string scriptName)
				{
						return false;
				}

				public bool HasAtLeastOne(List <string> scriptNames)
				{
						return scriptNames.Count == 0;
				}

				public WorldItem worlditem { get { return null; } }

				public PlayerBase player { get { return null; } }

				public ActionNode node { get { return null; } }

				public WorldLight worldlight { get { return null; } }

				public Fire fire { get { return null; } }

				public bool Destroyed { get { return false; } }

				public bool HasPlayerFocus {
						get {
								return mHasPlayerFocus;
						}
						set {
								mHasPlayerFocus = value;
						}
				}

				protected bool mHasPlayerFocus = false;
				protected bool mHadWeaponEquippedLastFrame = false;
				protected bool mHasWeaponEquipped = false;

				#endregion

		}
}