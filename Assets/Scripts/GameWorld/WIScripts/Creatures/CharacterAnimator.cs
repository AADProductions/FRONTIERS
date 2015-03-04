using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;

public class CharacterAnimator : BodyAnimator
{
		public CharacterWeaponMode WeaponMode {
				get {
						return mWeaponMode;
				}
				set {
						mWeaponMode = value;
				}
		}

		public GenericWorldItem	Tool {
				get {
						return mTool;
				}
				set {
						mTool = value;
				}
		}

		public override void Start()
		{
				base.Start();
				BaseMovementMode = 0;
				IdleAnimation = 0;
				WeaponMode = CharacterWeaponMode.BareHands;
				AvailableMovementModes.Add("Sleeping");
				animator.SetBool("CanLand", true);
		}

		public override void FixedUpdate()
		{
				base.FixedUpdate();

				if (mWeaponModeLastFrame != WeaponMode) {
						ApplyWeaponMode(WeaponMode);
				}
		}

		protected void ApplyWeaponMode(CharacterWeaponMode weaponMode)
		{
				switch (WeaponMode) {
						case CharacterWeaponMode.BareHands:
						default:
								animator.SetBool("Bow", false);
								animator.SetBool("Pistol", false);
								animator.SetBool("Sword", false);
								break;

						case CharacterWeaponMode.BowBasedWeapon:
								animator.SetBool("Bow", true);
								animator.SetBool("Pistol", false);
								animator.SetBool("Sword", false);
								break;

						case CharacterWeaponMode.RangedWeapon:
								animator.SetBool("Bow", false);
								animator.SetBool("Pistol", true);
								animator.SetBool("Sword", false);
								break;

						case CharacterWeaponMode.SlashingWeapon:
								animator.SetBool("Bow", false);
								animator.SetBool("Pistol", false);
								animator.SetBool("Sword", true);
								break;
				}
		}

		protected CharacterWeaponMode mWeaponMode = CharacterWeaponMode.BareHands;
		protected GenericWorldItem mTool = GenericWorldItem.Empty;
		protected CharacterWeaponMode mWeaponModeLastFrame = CharacterWeaponMode.BareHands;
}