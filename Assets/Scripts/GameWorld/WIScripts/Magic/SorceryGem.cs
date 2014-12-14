using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Frontiers.World.Gameplay;

namespace Frontiers.World
{
		public class SorceryGem : WIScript, IEquippableAction
		{
				public SorceryGemState State = new SorceryGemState();

				public Renderer SorceryGemRenderer;

				public override string DisplayNamer(int increment)
				{
						if (IsEquipped && mSpellToCast != null) {
								return "Sorcery Gem (" + mSpellToCast.DisplayName + ")";
						}
						return worlditem.Props.Name.DisplayName;			
				}

				[FrontiersFXAttribute]
				public string FXOnCast;
				[FrontiersFXAttribute]
				public string FXOnCycle;

				public override bool UnloadWhenStacked {
						get {
								return false;
						}
				}

				#region IEquippableAction implementation

				public bool IsActive { get { return mCastingSpell; } }

				public bool IsCycling { get { return mCycling; } }

				public bool CanCycle { get { return !mCastingSpell; } }

				public bool IsEquipped = false;

				#endregion

				public override void OnInitialized()
				{
						Equippable equippable = null;
						if (worlditem.Is <Equippable>(out equippable)) {
								equippable.CustomAction = this;
								equippable.OnEquip += OnEquip;
								equippable.OnUnequip += OnUnequip;
								equippable.OnUseStart += OnUseStart;
								equippable.OnUseFinish += OnUseFinish;
								equippable.OnCycleNext += OnCycleNext;
								equippable.OnCyclePrev += OnCyclePrev;
								equippable.Type = PlayerToolType.CustomAction;
						}
				}

				public void OnEquip()
				{
						IsEquipped = true;
						if (mSpellToCast == null) {
								//this will grab the first spell we can find
								//or else the last spell that we used
								StartCoroutine(Cycle(0));
						}
				}

				public void OnUnequip()
				{
						IsEquipped = false;
				}

				public bool IsOnAltar {
						get {
								return true;
						}
				}

				public void OnUseStart()
				{
						if (!mCastingSpell) {
								mCastingSpell = true;
								StartCoroutine(CastSpell());
						}
				}

				public void OnUseFinish()
				{
						RefreshGem();
				}

				public void OnCyclePrev()
				{
						if (!mCycling) {
								mCycling = true;
								StartCoroutine(Cycle(-1));
						}
				}

				public void RefreshGem()
				{
						if (mHungerKeeper == null) {
								Player.Local.Status.GetStatusKeeper("Hunger", out mHungerKeeper);
						}
						SorceryGemRenderer.material.color = Color.Lerp(Color.black, mBaseColor, mHungerKeeper.NormalizedValue);
				}

				public void OnCycleNext()
				{
						if (!mCycling) {
								mCycling = true;
								StartCoroutine(Cycle(1));
						}
				}

				public IEnumerator Cycle(int direction)
				{

						//get the total number of scripts associated with this item
						List <Skill> spellsToCast = Skills.Get.SkillsAssociatedWith(worlditem);
						//remove any that we can't use
						for (int i = spellsToCast.LastIndex(); i >= 0; i--) {
								if (!spellsToCast[i].HasBeenLearned || spellsToCast[i].name == "Examine") {
										//removing from the list because it's either examine or hasn't been learned
										spellsToCast.RemoveAt(i);
								}
						}
						//if there are none to cast, we're done
						if (spellsToCast.Count == 0) {
								mCycling = false;
								yield break;
						}
						//find the one we'll be using
						//if we've never used one before it'll be the first
						mSpellToCast = spellsToCast[0];
						for (int i = 0; i < spellsToCast.Count; i++) {
								if (spellsToCast[i].name == State.LastSelectedSpell) {
										//if this is the last spell we casted we want the next or prev
										int cycleIndex = i + direction;
										if (cycleIndex > spellsToCast.LastIndex()) {
												cycleIndex = 0;
										} else if (cycleIndex < 0) {
												cycleIndex = spellsToCast.LastIndex();
										}
										mSpellToCast = spellsToCast[cycleIndex];
										break;
								}
						}

						if (mSpellToCast == null) {
								//something went wrong, didn't find spell to cast
								mCycling = false;
								yield break;
						} else {
								State.LastSelectedSpell = mSpellToCast.name;
						}

						//ok now we've got the spell that we're casting
						//we have to update the sorcery gem to look correct
						//TODO update sorcery gem look

						if (IsEquipped) {
								FXManager.Get.SpawnFX(Player.Local.Tool.ToolActionPointObject, FXOnCycle);
						}

						yield return null;
						mCycling = false;
						yield break;
				}

				public IEnumerator CastSpell()
				{
						if (mSpellToCast == null) {
								GUIManager.PostIntrospection("I need to choose which spell to use");
								mCastingSpell = false;
								yield break;
						}
						mSpellToCast.Use(0);

						FXManager.Get.SpawnFX(Player.Local.Tool.ToolActionPointObject, FXOnCast);

						yield return null;
						mCastingSpell = false;
						yield break;
				}

				protected StatusKeeper mHungerKeeper = null;
				protected Color mBaseColor = Color.white;
				protected Skill mSpellToCast = null;
				protected bool mCastingSpell = false;
				protected bool mCycling = false;
		}

		[Serializable]
		public class SorceryGemState
		{
				public string LastSelectedSpell = string.Empty;
		}
}