using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.World;
using Frontiers.World.WIScripts;

namespace Frontiers
{
	public class DamageManager : Manager
	{
		//handles sending and receiving of damage from one object to another
		//spawns fx, makes sound, applies modifiers and so on
		//will also send damage across the network once multiplayer is up & running
		public static DamageManager Get;
		protected IItemOfInterest damageReceiver = null;
		protected IDamageable damageHandler = null;
		protected WorldItem damageWorldItem = null;
		protected WIMaterialType receiverMaterial;
		protected WIMaterialType receiverArmorMaterials;

		public void SendDamage (DamagePackage damage)
		{
			SendDamage (damage, null);
		}

		public void SendDamage (DamagePackage damage, BodyPart bodyPart)
		{
			damageReceiver = damage.Target;
			damageHandler = null;
			damageWorldItem = null;
			damage.OnPackageSent.SafeInvoke ();

			if (damageReceiver != null) {
				//check if it's a player because it's the eaiest
				switch (damageReceiver.IOIType) {
				case ItemOfInterestType.Player:
					damageHandler = Player.Local.DamageHandler;
					break;

				case ItemOfInterestType.WorldItem:
					Damageable damageable = null;
					damageWorldItem = damageReceiver.worlditem;
					if (damageReceiver.worlditem.Is <Damageable> (out damageable)) {
						damageHandler = damageable;
					} else {
						//if it's an instant kill, destroy it now
						if (damage.InstantKill) {
							Debug.Log ("Instant kill in damage manager");
							damage.HitTarget = true;
							damage.TargetIsDead = true;
							damage.DamageDealt = damage.DamageSent;
							damageReceiver.worlditem.RemoveFromGame ();
							return;
						}
					}
					break;

				case ItemOfInterestType.Scenery:
					DamageableChild dc = null;
					if (damageReceiver.gameObject.HasComponent <DamageableChild> (out dc)) {	//get the parent from the damageable child
						damageHandler = dc.DamageableParent;
					} else {
						//if it's not a world item and it's not a body part check for a general damage handler
						//this includes the player's damage handler
						damageHandler = (IDamageable)damageReceiver.gameObject.GetComponent (typeof(IDamageable));
					}
					break;

				default:
					break;
				}
			}
			
			//apply damage using the damage handler
			if (damageHandler != null) {
				receiverMaterial = damageHandler.BaseMaterialType;
				receiverArmorMaterials = damageHandler.ArmorMaterialTypes;
				int receiverArmorLevel = 0;
				if (receiverArmorMaterials != WIMaterialType.None) {
					receiverArmorLevel = damageHandler.ArmorLevel (receiverArmorMaterials);
				}
				//get damage multiplier for material types
				//TODO re-enable these
				//damage.DamageSent = damage.DamageSent * BaseWeaponDamageMultiplier (damage.SenderMaterial, receiverMaterial);
				//damage.DamageSent = damage.DamageSent * BaseArmorReductionMultiplier (damage.SenderMaterial, receiverArmorMaterials);
				//check if there's a material bonus for this damage sender
				if (damage.MaterialBonus != WIMaterialType.None) {
					if (Flags.Check ((uint)receiverMaterial, (uint)damage.MaterialBonus, Flags.CheckType.MatchAny)) {
						damage.DamageSent *= Globals.DamageMaterialBonusMultiplier;
					}
				}
				if (damage.MaterialPenalty != WIMaterialType.None) {
					if (Flags.Check ((uint)receiverMaterial, (uint)damage.MaterialPenalty, Flags.CheckType.MatchAny)) {
						damage.DamageSent *= Globals.DamageMaterialPenaltyMultiplier;
					}
				}
				damageHandler.LastDamageSource = damage.Source;//this has a good chance of being null
				damageHandler.LastBodyPartHit = bodyPart;
				if (damage.InstantKill) {
					damageHandler.InstantKill (damage.Source);
				} else {
					damage.HitTarget = damageHandler.TakeDamage (damage.SenderMaterial, damage.Point, damage.DamageSent, damage.Force, damage.SenderName, out damage.DamageDealt, out damage.TargetIsDead);
				}

				if (damage.HitTarget) {
					SpawnDamageFX (damage.Point, receiverMaterial, damage.DamageDealt);
					PlayDamageSound (damage.Point, receiverMaterial, damage.DamageDealt);

					if (bodyPart != null && !damage.TargetIsDead) {
						switch (bodyPart.Type) {
						case BodyPartType.Eye:
							GUI.GUIManager.PostSuccess ("Inflicted eye wound");
							damage.DamageSent = Globals.DamageBodyPartEyeMultiplier;
							break;

						case BodyPartType.Head:
						case BodyPartType.Face:
						case BodyPartType.Neck:
							GUI.GUIManager.PostSuccess ("Inflicted head wound");
							damage.DamageSent *= Globals.DamageBodyPartHeadMultiplier;
							break;

						case BodyPartType.Chest:
						case BodyPartType.Hip:
						case BodyPartType.Segment:
						case BodyPartType.Shoulder:
						case BodyPartType.None:
						default:
							GUI.GUIManager.PostSuccess ("Inflicted torso wound");
							damage.DamageSent *= Globals.DamageBodyPartTorsoMultiplier;
							break;

						case BodyPartType.Finger:
						case BodyPartType.Hand:
						case BodyPartType.Wrist:
						case BodyPartType.Arm:
						case BodyPartType.Foot:
						case BodyPartType.Shin:
						case BodyPartType.Leg:
							GUI.GUIManager.PostSuccess ("Inflicted limb wound");
							damage.DamageSent *= Globals.DamageBodyPartLimbMultiplier;
							break;

						}
					}
				}	
				damage.OnPackageReceived.SafeInvoke ();
			} else {
				//if we didn't find a damage handler but we did find a worlditem
				//send force to the worlditem so it doesn't just sit there
				if (damageWorldItem != null) {
					damageWorldItem.ApplyForce (damage.Force, damage.Point);
				}
			}
			//play the sender's fx no matter what
			SpawnDamageFX (damage.Point, damage.SenderMaterial, damage.DamageDealt);
			PlayDamageSound (damage.Point, damage.SenderMaterial, damage.DamageDealt);
		}

		public void PlayDamageSound (Vector3 point, WIMaterialType type, float intensity)
		{
			mDamageAudioSource.position = point;
			MasterAudio.PlaySound (MasterAudio.SoundType.Damage, mDamageAudioSource, MaterialTypeToSoundType (type));
		}

		public void SpawnDamageFX (Vector3 point, WIMaterialType type, float intensity)
		{
			FXManager.Get.SpawnFX (point, MaterialTypeToFXType (type));
		}

		public float BaseFallDamageMultiplier (WIMaterialType groundMaterial, WIMaterialType fallingObjectMaterial)
		{
			float multiplier = 1.0f;
			KeyValuePair <WIMaterialType, WIMaterialType> pair = new KeyValuePair <WIMaterialType, WIMaterialType> (groundMaterial, fallingObjectMaterial);
			MaterialDamageMatrix.TryGetValue (pair, out multiplier);
			return multiplier;
		}

		public float BaseWeaponDamageMultiplier (WIMaterialType weaponMaterial, WIMaterialType objectMaterial)
		{
			float multiplier = 1.0f;
			KeyValuePair <WIMaterialType, WIMaterialType> pair = new KeyValuePair <WIMaterialType, WIMaterialType> (weaponMaterial, objectMaterial);
			MaterialDamageMatrix.TryGetValue (pair, out multiplier);
			return multiplier;
		}

		public float BaseArmorReductionMultiplier (WIMaterialType damageSourceMaterial, WIMaterialType armorMaterial)
		{
			float multiplier = 1.0f;
			KeyValuePair <WIMaterialType, WIMaterialType> pair = new KeyValuePair <WIMaterialType, WIMaterialType> (damageSourceMaterial, armorMaterial);	
			MaterialArmorMatrix.TryGetValue (pair, out multiplier);
			return multiplier;
		}

		protected void AddMaterialEntry (WIMaterialType dealer, WIMaterialType recipient, float multiplier)
		{
			//KeyValuePair <WIMaterialType, WIMaterialType> entry = new KeyValuePair <WIMaterialType, WIMaterialType> (dealer, recipient);
			//MaterialDamageMatrix.Add (entry, multiplier);
		}

		protected void AddArmorEntry (WIMaterialType dealer, WIMaterialType armor, float multiplier)
		{
			//KeyValuePair <WIMaterialType, WIMaterialType> entry = new KeyValuePair <WIMaterialType, WIMaterialType> (dealer, armor);
			//MaterialDamageMatrix.Add (entry, multiplier);
		}

		public static WIMaterialType GroundTypeToMaterialType (GroundType type)
		{
			WIMaterialType material = WIMaterialType.Dirt;
			switch (type) {
			case GroundType.Stone:
				material = WIMaterialType.Stone;
				break;

			case GroundType.Wood:
				material = WIMaterialType.Wood;
				break;

			case GroundType.Leaves:
				material = WIMaterialType.Plant;
				break;

			case GroundType.Metal:
				material = WIMaterialType.Metal;
				break;

			case GroundType.Water:
				material = WIMaterialType.Liquid;
				break;

			case GroundType.Mud:
				material = WIMaterialType.Dirt;
				break;

			case GroundType.Snow:
				material = WIMaterialType.Dirt;
				break;

			default:
				break;
			}
			return material;
		}

		public string MaterialTypeToSoundType (WIMaterialType type)
		{
			switch (type) {
			case WIMaterialType.Bone:
				return BoneDamageSound;

			case WIMaterialType.Crystal:
				return CrystalDamageSound;

			case WIMaterialType.Dirt:
			default:
				return DirtDamageSound;

			case WIMaterialType.Fabric:
				return FabricDamageSound;

			case WIMaterialType.Fire:
				return FireDamageSound;

			case WIMaterialType.Flesh:
				return FleshDamageSound;

			case WIMaterialType.Food:
				return FoodDamageSound;

			case WIMaterialType.Glass:
				return GlassDamageSound;

			case WIMaterialType.Ice:
				return IceDamageSound;

			case WIMaterialType.Liquid:
				return LiquidDamageSound;

			case WIMaterialType.Metal:
				return MetalDamageSound;

			case WIMaterialType.Plant:
				return PlantDamageSound;

			case WIMaterialType.Stone:
				return StoneDamageSound;

			case WIMaterialType.Wood:
				return WoodDamageSound;
			}
		}

		public string MaterialTypeToFXType (WIMaterialType type)
		{
			switch (type) {
			case WIMaterialType.Bone:
				return BoneDamageFX;

			case WIMaterialType.Crystal:
				return CrystalDamageFX;

			case WIMaterialType.Dirt:
			default:
				return DirtDamageFX;

			case WIMaterialType.Fabric:
				return FabricDamageFX;

			case WIMaterialType.Fire:
				return FireDamageFX;

			case WIMaterialType.Flesh:
				return FleshDamageFX;

			case WIMaterialType.Food:
				return FoodDamageFX;

			case WIMaterialType.Glass:
				return GlassDamageFX;

			case WIMaterialType.Ice:
				return IceDamageFX;

			case WIMaterialType.Liquid:
				return LiquidDamageFX;

			case WIMaterialType.Metal:
				return MetalDamageFX;

			case WIMaterialType.Plant:
				return PlantDamageFX;

			case WIMaterialType.Stone:
				return StoneDamageFX;

			case WIMaterialType.Wood:
				return WoodDamageFX;
			}
		}

		public override void Awake ()
		{
			Get = this;
			base.Awake ();

			mDamageAudioSource = gameObject.CreateChild ("DamageAudioSource");

			#region material entries
			//create our lookup tables for damage
			//TODO good god in heaven there has to be some way to automate this
			//TODO also we need a table for 'takes damage against'
			AddMaterialEntry (WIMaterialType.Dirt, WIMaterialType.Dirt, 1.0f);
			AddMaterialEntry (WIMaterialType.Dirt, WIMaterialType.Fabric, 1.0f);
			AddMaterialEntry (WIMaterialType.Dirt, WIMaterialType.Flesh, 1.5f);
			AddMaterialEntry (WIMaterialType.Dirt, WIMaterialType.Glass, 2.0f);
			AddMaterialEntry (WIMaterialType.Dirt, WIMaterialType.Liquid, 1.0f);
			AddMaterialEntry (WIMaterialType.Dirt, WIMaterialType.Metal, 0.25f);
			AddMaterialEntry (WIMaterialType.Dirt, WIMaterialType.Stone, 0.25f);
			AddMaterialEntry (WIMaterialType.Dirt, WIMaterialType.Wood, 0.5f);

			AddMaterialEntry (WIMaterialType.Fabric, WIMaterialType.Dirt, 0.01f);
			AddMaterialEntry (WIMaterialType.Fabric, WIMaterialType.Fabric, 0.01f);
			AddMaterialEntry (WIMaterialType.Fabric, WIMaterialType.Flesh, 0.01f);
			AddMaterialEntry (WIMaterialType.Fabric, WIMaterialType.Glass, 0.1f);
			AddMaterialEntry (WIMaterialType.Fabric, WIMaterialType.Liquid, 0.01f);
			AddMaterialEntry (WIMaterialType.Fabric, WIMaterialType.Metal, 0.001f);
			AddMaterialEntry (WIMaterialType.Fabric, WIMaterialType.Stone, 0.001f);
			AddMaterialEntry (WIMaterialType.Fabric, WIMaterialType.Wood, 0.001f);

			AddMaterialEntry (WIMaterialType.Flesh, WIMaterialType.Dirt, 0.25f);
			AddMaterialEntry (WIMaterialType.Flesh, WIMaterialType.Fabric, 0.25f);
			AddMaterialEntry (WIMaterialType.Flesh, WIMaterialType.Flesh, 1.0f);
			AddMaterialEntry (WIMaterialType.Flesh, WIMaterialType.Glass, 1.0f);
			AddMaterialEntry (WIMaterialType.Flesh, WIMaterialType.Liquid, 1.0f);
			AddMaterialEntry (WIMaterialType.Flesh, WIMaterialType.Metal, 0.001f);
			AddMaterialEntry (WIMaterialType.Flesh, WIMaterialType.Stone, 0.001f);
			AddMaterialEntry (WIMaterialType.Flesh, WIMaterialType.Wood, 0.001f);

			AddMaterialEntry (WIMaterialType.Glass, WIMaterialType.Dirt, 0.1f);
			AddMaterialEntry (WIMaterialType.Glass, WIMaterialType.Fabric, 0.1f);
			AddMaterialEntry (WIMaterialType.Glass, WIMaterialType.Flesh, 1.0f);
			AddMaterialEntry (WIMaterialType.Glass, WIMaterialType.Glass, 1.0f);
			AddMaterialEntry (WIMaterialType.Glass, WIMaterialType.Liquid, 1.0f);
			AddMaterialEntry (WIMaterialType.Glass, WIMaterialType.Metal, 0.01f);
			AddMaterialEntry (WIMaterialType.Glass, WIMaterialType.Stone, 0.01f);
			AddMaterialEntry (WIMaterialType.Glass, WIMaterialType.Wood, 0.125f);

			AddMaterialEntry (WIMaterialType.Liquid, WIMaterialType.Dirt, 0.001f);
			AddMaterialEntry (WIMaterialType.Liquid, WIMaterialType.Fabric, 0.001f);
			AddMaterialEntry (WIMaterialType.Liquid, WIMaterialType.Flesh, 0.001f);
			AddMaterialEntry (WIMaterialType.Liquid, WIMaterialType.Glass, 0.001f);
			AddMaterialEntry (WIMaterialType.Liquid, WIMaterialType.Liquid, 0.001f);
			AddMaterialEntry (WIMaterialType.Liquid, WIMaterialType.Metal, 0.001f);
			AddMaterialEntry (WIMaterialType.Liquid, WIMaterialType.Stone, 0.001f);
			AddMaterialEntry (WIMaterialType.Liquid, WIMaterialType.Wood, 0.001f);

			AddMaterialEntry (WIMaterialType.Metal, WIMaterialType.Dirt, 2.0f);
			AddMaterialEntry (WIMaterialType.Metal, WIMaterialType.Fabric, 2.0f);
			AddMaterialEntry (WIMaterialType.Metal, WIMaterialType.Flesh, 3.0f);
			AddMaterialEntry (WIMaterialType.Metal, WIMaterialType.Glass, 5.0f);
			AddMaterialEntry (WIMaterialType.Metal, WIMaterialType.Liquid, 1.0f);
			AddMaterialEntry (WIMaterialType.Metal, WIMaterialType.Metal, 0.5f);
			AddMaterialEntry (WIMaterialType.Metal, WIMaterialType.Stone, 1.5f);
			AddMaterialEntry (WIMaterialType.Metal, WIMaterialType.Wood, 2.0f);

			AddMaterialEntry (WIMaterialType.Stone, WIMaterialType.Dirt, 2.0f);
			AddMaterialEntry (WIMaterialType.Stone, WIMaterialType.Fabric, 2.0f);
			AddMaterialEntry (WIMaterialType.Stone, WIMaterialType.Flesh, 5.0f);
			AddMaterialEntry (WIMaterialType.Stone, WIMaterialType.Glass, 5.0f);
			AddMaterialEntry (WIMaterialType.Stone, WIMaterialType.Liquid, 1.0f);
			AddMaterialEntry (WIMaterialType.Stone, WIMaterialType.Metal, 0.5f);
			AddMaterialEntry (WIMaterialType.Stone, WIMaterialType.Stone, 1.0f);
			AddMaterialEntry (WIMaterialType.Stone, WIMaterialType.Wood, 1.5f);

			AddMaterialEntry (WIMaterialType.Wood, WIMaterialType.Dirt, 1.0f);
			AddMaterialEntry (WIMaterialType.Wood, WIMaterialType.Fabric, 1.0f);
			AddMaterialEntry (WIMaterialType.Wood, WIMaterialType.Flesh, 2.0f);
			AddMaterialEntry (WIMaterialType.Wood, WIMaterialType.Glass, 2.0f);
			AddMaterialEntry (WIMaterialType.Wood, WIMaterialType.Liquid, 1.0f);
			AddMaterialEntry (WIMaterialType.Wood, WIMaterialType.Metal, 0.5f);
			AddMaterialEntry (WIMaterialType.Wood, WIMaterialType.Stone, 0.25f);
			AddMaterialEntry (WIMaterialType.Wood, WIMaterialType.Wood, 1.0f);	
			#endregion

			#region armor entries
			//create our lookup tables for damage
			AddArmorEntry (WIMaterialType.Dirt, WIMaterialType.Dirt, 1.5f);
			AddArmorEntry (WIMaterialType.Fabric, WIMaterialType.Fabric, 1.0f);
			AddArmorEntry (WIMaterialType.Flesh, WIMaterialType.Flesh, 1.0f);
			AddArmorEntry (WIMaterialType.Glass, WIMaterialType.Glass, 1.0f);
			AddArmorEntry (WIMaterialType.Liquid, WIMaterialType.Liquid, 1.0f);
			AddArmorEntry (WIMaterialType.Metal, WIMaterialType.Metal, 2.0f);
			AddArmorEntry (WIMaterialType.Stone, WIMaterialType.Stone, 2.0f);
			AddArmorEntry (WIMaterialType.Wood, WIMaterialType.Wood, 2.0f);	

			AddArmorEntry (WIMaterialType.Dirt, WIMaterialType.Dirt, 1.0f);
			AddArmorEntry (WIMaterialType.Dirt, WIMaterialType.Fabric, 1.0f);
			AddArmorEntry (WIMaterialType.Dirt, WIMaterialType.Flesh, 1.5f);
			AddArmorEntry (WIMaterialType.Dirt, WIMaterialType.Glass, 2.0f);
			AddArmorEntry (WIMaterialType.Dirt, WIMaterialType.Liquid, 1.0f);
			AddArmorEntry (WIMaterialType.Dirt, WIMaterialType.Metal, 0.25f);
			AddArmorEntry (WIMaterialType.Dirt, WIMaterialType.Stone, 0.25f);
			AddArmorEntry (WIMaterialType.Dirt, WIMaterialType.Wood, 0.5f);

			AddArmorEntry (WIMaterialType.Fabric, WIMaterialType.Dirt, 0.01f);
			AddArmorEntry (WIMaterialType.Fabric, WIMaterialType.Fabric, 0.01f);
			AddArmorEntry (WIMaterialType.Fabric, WIMaterialType.Flesh, 0.01f);
			AddArmorEntry (WIMaterialType.Fabric, WIMaterialType.Glass, 0.1f);
			AddArmorEntry (WIMaterialType.Fabric, WIMaterialType.Liquid, 0.01f);
			AddArmorEntry (WIMaterialType.Fabric, WIMaterialType.Metal, 0.001f);
			AddArmorEntry (WIMaterialType.Fabric, WIMaterialType.Stone, 0.001f);
			AddArmorEntry (WIMaterialType.Fabric, WIMaterialType.Wood, 0.001f);

			AddArmorEntry (WIMaterialType.Flesh, WIMaterialType.Dirt, 0.25f);
			AddArmorEntry (WIMaterialType.Flesh, WIMaterialType.Fabric, 0.25f);
			AddArmorEntry (WIMaterialType.Flesh, WIMaterialType.Flesh, 1.0f);
			AddArmorEntry (WIMaterialType.Flesh, WIMaterialType.Glass, 1.0f);
			AddArmorEntry (WIMaterialType.Flesh, WIMaterialType.Liquid, 1.0f);
			AddArmorEntry (WIMaterialType.Flesh, WIMaterialType.Metal, 0.001f);
			AddArmorEntry (WIMaterialType.Flesh, WIMaterialType.Stone, 0.001f);
			AddArmorEntry (WIMaterialType.Flesh, WIMaterialType.Wood, 0.001f);

			AddArmorEntry (WIMaterialType.Glass, WIMaterialType.Dirt, 0.1f);
			AddArmorEntry (WIMaterialType.Glass, WIMaterialType.Fabric, 0.1f);
			AddArmorEntry (WIMaterialType.Glass, WIMaterialType.Flesh, 1.0f);
			AddArmorEntry (WIMaterialType.Glass, WIMaterialType.Glass, 1.0f);
			AddArmorEntry (WIMaterialType.Glass, WIMaterialType.Liquid, 1.0f);
			AddArmorEntry (WIMaterialType.Glass, WIMaterialType.Metal, 0.01f);
			AddArmorEntry (WIMaterialType.Glass, WIMaterialType.Stone, 0.01f);
			AddArmorEntry (WIMaterialType.Glass, WIMaterialType.Wood, 0.125f);

			AddArmorEntry (WIMaterialType.Liquid, WIMaterialType.Dirt, 0.001f);
			AddArmorEntry (WIMaterialType.Liquid, WIMaterialType.Fabric, 0.001f);
			AddArmorEntry (WIMaterialType.Liquid, WIMaterialType.Flesh, 0.001f);
			AddArmorEntry (WIMaterialType.Liquid, WIMaterialType.Glass, 0.001f);
			AddArmorEntry (WIMaterialType.Liquid, WIMaterialType.Liquid, 0.001f);
			AddArmorEntry (WIMaterialType.Liquid, WIMaterialType.Metal, 0.001f);
			AddArmorEntry (WIMaterialType.Liquid, WIMaterialType.Stone, 0.001f);
			AddArmorEntry (WIMaterialType.Liquid, WIMaterialType.Wood, 0.001f);

			AddArmorEntry (WIMaterialType.Metal, WIMaterialType.Dirt, 2.0f);
			AddArmorEntry (WIMaterialType.Metal, WIMaterialType.Fabric, 2.0f);
			AddArmorEntry (WIMaterialType.Metal, WIMaterialType.Flesh, 3.0f);
			AddArmorEntry (WIMaterialType.Metal, WIMaterialType.Glass, 5.0f);
			AddArmorEntry (WIMaterialType.Metal, WIMaterialType.Liquid, 1.0f);
			AddArmorEntry (WIMaterialType.Metal, WIMaterialType.Metal, 0.5f);
			AddArmorEntry (WIMaterialType.Metal, WIMaterialType.Stone, 1.5f);
			AddArmorEntry (WIMaterialType.Metal, WIMaterialType.Wood, 2.0f);

			AddArmorEntry (WIMaterialType.Stone, WIMaterialType.Dirt, 2.0f);
			AddArmorEntry (WIMaterialType.Stone, WIMaterialType.Fabric, 2.0f);
			AddArmorEntry (WIMaterialType.Stone, WIMaterialType.Flesh, 5.0f);
			AddArmorEntry (WIMaterialType.Stone, WIMaterialType.Glass, 5.0f);
			AddArmorEntry (WIMaterialType.Stone, WIMaterialType.Liquid, 1.0f);
			AddArmorEntry (WIMaterialType.Stone, WIMaterialType.Metal, 0.5f);
			AddArmorEntry (WIMaterialType.Stone, WIMaterialType.Stone, 1.0f);
			AddArmorEntry (WIMaterialType.Stone, WIMaterialType.Wood, 1.5f);

			AddArmorEntry (WIMaterialType.Wood, WIMaterialType.Dirt, 1.0f);
			AddArmorEntry (WIMaterialType.Wood, WIMaterialType.Fabric, 1.0f);
			AddArmorEntry (WIMaterialType.Wood, WIMaterialType.Flesh, 2.0f);
			AddArmorEntry (WIMaterialType.Wood, WIMaterialType.Glass, 2.0f);
			AddArmorEntry (WIMaterialType.Wood, WIMaterialType.Liquid, 1.0f);
			AddArmorEntry (WIMaterialType.Wood, WIMaterialType.Metal, 0.5f);
			AddArmorEntry (WIMaterialType.Wood, WIMaterialType.Stone, 0.25f);
			AddArmorEntry (WIMaterialType.Wood, WIMaterialType.Wood, 1.0f);		
			#endregion
		}

		protected Transform mDamageAudioSource = null;
		protected Dictionary <KeyValuePair <WIMaterialType, WIMaterialType>, float> MaterialDamageMatrix = new Dictionary<KeyValuePair <WIMaterialType, WIMaterialType>, float> ();
		protected Dictionary <KeyValuePair <WIMaterialType, WIMaterialType>, float> MaterialArmorMatrix	= new Dictionary<KeyValuePair<WIMaterialType, WIMaterialType>, float> ();
		/*
				Dirt		= 1,
				Stone		= 2,
				Wood		= 4,
				Metal		= 8,
				Flesh		= 16,
				Glass		= 32,
				Liquid		= 64,
				Fabric		= 128,
				Fire		= 256,
				Ice			= 512,
				Bone		= 1024,
				Plant		= 2048,
				Food		= 4096,
				Crystal		= 8192
				*/
		//TODO move these to Globals
		public string DirtDamageFX;
		public string StoneDamageFX;
		public string WoodDamageFX;
		public string MetalDamageFX;
		public string FleshDamageFX;
		public string GlassDamageFX;
		public string LiquidDamageFX;
		public string FabricDamageFX;
		public string FireDamageFX;
		public string IceDamageFX;
		public string BoneDamageFX;
		public string PlantDamageFX;
		public string FoodDamageFX;
		public string CrystalDamageFX;
		public string DirtDamageSound;
		public string StoneDamageSound;
		public string WoodDamageSound;
		public string MetalDamageSound;
		public string FleshDamageSound;
		public string GlassDamageSound;
		public string LiquidDamageSound;
		public string FabricDamageSound;
		public string FireDamageSound;
		public string IceDamageSound;
		public string BoneDamageSound;
		public string PlantDamageSound;
		public string FoodDamageSound;
		public string CrystalDamageSound;
	}

	[Serializable]
	public class DamagePackage
	{
		public string SenderName = string.Empty;
		public WIMaterialType SenderMaterial = WIMaterialType.None;
		public WIMaterialType MaterialBonus = WIMaterialType.None;
		public WIMaterialType MaterialPenalty = WIMaterialType.None;
		public string AttachScriptOnHit = string.Empty;
		public float DamageSent = 0.0f;
		public SVector3 Point = SVector3.zero;
		public SVector3 Origin = SVector3.zero;
		public float DamageDealt = 0.0f;
		public float ForceSent = 0.0f;
		public bool InstantKill = false;
		public bool TargetIsDead = false;
		public bool HitTarget = false;
		public bool HasLineOfSight = true;
		[XmlIgnore]
		[NonSerialized]
		public Action OnPackageReceived;
		[XmlIgnore]
		[NonSerialized]
		public Action OnPackageSent;
		[XmlIgnore]
		[NonSerialized]
		public IItemOfInterest Target;

		[XmlIgnore]
		public Vector3 Force {
			get {
				return Vector3.Normalize (Point - Origin) * ForceSent;
			}
		}

		[XmlIgnore]
		[NonSerialized]
		public IItemOfInterest Source;
	}
}