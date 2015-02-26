using UnityEngine;
using System.Collections;
using Frontiers;
using System;
using System.Collections.Generic;
using Frontiers.GUI;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		//used for permanent stuff like rocks and bridges
		//anything that can never be destroyed but should still spawn appropriate damage
		public class DamageableScenery : SceneryScript, IDamageable, IItemOfInterest
		{
				public DamageableSceneryState State = new DamageableSceneryState();

				public BodyPart LastBodyPartHit { get; set; }

				public IItemOfInterest LastDamageSource { get; set; }

				public bool IsDead { get { return false; } }

				public float NormalizedDamage { get { return State.TotalDamage / State.SpawnDamageMinimum; } }

				public WIMaterialType BaseMaterialType { get { return State.MaterialType; } }

				public WIMaterialType ArmorMaterialTypes { get { return WIMaterialType.None; } }

				public int ArmorLevel(WIMaterialType materialType)
				{
						return 0;
				}

				public override void Awake()
				{
						base.Awake();
						gameObject.layer = Globals.LayerNumSolidTerrain;
				}

				public virtual void OnGainPlayerFocus()
				{
						if (State.SpawnOnDamaage) {
								if (string.IsNullOrEmpty(State.SpawnDescription)) {
										WICategory cat = null;
										if (WorldItems.Get.Category(State.SpawnCategory, out cat)) {
												cat.RefreshDisplayNames();
												List <string> stuffInCategory = new List<string>();
												for (int i = 0; i < cat.GenericWorldItems.Count; i++) {
														stuffInCategory.SafeAdd(cat.GenericWorldItems[i].DisplayName);
												}
												State.SpawnDescription = Data.GameData.CommaJoinWithLast(stuffInCategory, "&");
										}
								}
								mHasWeaponEquipped = false;
								mHadWeaponEquippedLastFrame = false;
								enabled = true;
						}
				}

				public virtual void OnLosePlayerFocus()
				{

				}

				public virtual void Update()
				{
						if (HasPlayerFocus) {
								mHasWeaponEquipped = Player.Local.Tool.HasWorldItem && Player.Local.Tool.worlditem.Is <Weapon>();
								if (mHasWeaponEquipped && !mHadWeaponEquippedLastFrame) {
										GUIHud.Get.ShowAction(this, UserActionType.ToolUse, "Mine (" + State.SpawnDescription + ")", Player.Local.Focus.FocusTransform, GameManager.Get.GameCamera);
								}
								mHadWeaponEquippedLastFrame = mHasWeaponEquipped;
						} else {
								enabled = false;
						}
				}

				public virtual void InstantKill (IItemOfInterest causeOfDeath) {
						return;
				}

				public virtual bool TakeDamage(WIMaterialType materialType, Vector3 damagePoint, float attemptedDamage, Vector3 attemptedForce, string damageSource, out float actualDamage, out bool isDead)
				{
						actualDamage = attemptedDamage;
						isDead = false;
						State.TotalDamage += actualDamage;

						if (HasPlayerFocus) {
								GUIHud.Get.ShowProgressBar(Colors.Get.GeneralHighlightColor, Colors.Darken(Colors.Get.GenericNeutralValue), NormalizedDamage);
						}

						if (State.SpawnOnDamaage && State.TotalDamage >= State.SpawnDamageMinimum) {	//reset
								State.TotalDamage = 0f;
								//spawn a thing
								WorldItem worlditem = null;
								if (WorldItems.CloneRandomFromCategory(State.SpawnCategory, ParentChunk.AboveGroundGroup, out worlditem)) {
										//spit it out at the player
										Vector3 direction = (Player.Local.Position - damagePoint).normalized * 5f;
										worlditem.tr.parent = ParentChunk.AboveGroundGroup.transform;
										worlditem.tr.position = damagePoint;
										worlditem.Props.Local.FreezeOnStartup = false;
										worlditem.Props.Local.Transform.CopyFrom(worlditem.tr);
										worlditem.Initialize();
										worlditem.SetMode(WIMode.World);
										worlditem.ApplyForce(direction, damagePoint);
										FXManager.Get.SpawnFX(worlditem.tr, "DrawAttentionToItem");
										worlditem.GetOrAdd <DespawnOnPlayerLeave>();
								}
						}
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
								if (!mHasPlayerFocus) {
										if (value) {
												OnGainPlayerFocus();
										}
								}
								mHasPlayerFocus = value;
						}
				}

				protected bool mHasPlayerFocus = false;
				protected bool mHadWeaponEquippedLastFrame = false;
				protected bool mHasWeaponEquipped = false;

				#endregion

				protected Transform mTr;
		}

		[Serializable]
		public class DamageableSceneryState : SceneryScriptState
		{
				public WIMaterialType MaterialType = WIMaterialType.Stone;
				public bool SpawnOnDamaage = false;
				public float TotalDamage = 0f;
				[FrontiersAvailableMods("Category")]
				public string SpawnCategory = string.Empty;
				public string SpawnDescription = string.Empty;
				public float SpawnDamageMinimum	= 50.0f;
				public float SpawnDamageMaximum = 0f;
				public bool DepletedOnMaxDamage = false;
				public string IntrospectionOnDepleted;
		}
}