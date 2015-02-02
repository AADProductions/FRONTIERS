using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World.BaseWIScripts
{
		public class WorldPlant : WIScript, IHasPosition
		{
				//used by plant instance assigner / quad tree
				public Vector3 Position {
						get {
								if (mDestroyed) {
										return Vector3.zero;
								}
								return transform.position;
						}
						set {
								if (mDestroyed) {
										return;
								}
								transform.position = value;
						}
				}

				public override string FileNamer(int increment)
				{
						if (HasPlantProps) {
								return Props.Name + "-" + increment.ToString();
						} else {
								return "WorldPlant-" + increment.ToString();
						}
				}

				public override void PopulateExamineList(List<WIExamineInfo> examine)
				{
						if (!HasPlantProps) {
								if (!Plants.Get.PlantProps(State.PlantName, ref Props)) {
										Debug.LogError("Couldn't get plant props " + State.PlantName);
										return;
								}
						}
						Props.Revealed = true;
						Props.NumTimesEncountered++;
						Props.EncounteredTimesOfYear |= WorldClock.SeasonCurrent;
						Plants.Examine(Props, examine);
				}

				public override string StackNamer(int increment)
				{
						if (Props != null) {
								return Props.Name;
						} else {
								return "WorldPlant";
						}
				}

				public override string DisplayNamer(int increment)
				{
						if (Props != null) {
								return Props.CommonName;
						}
						return State.PlantName;
				}

				public override bool CanBeCarried {
						get {
								return false;
						}
				}

				public override bool CanBePlacedOn(IItemOfInterest targetObject, Vector3 point, Vector3 normal, ref string errorMessage)
				{
						return false;
				}

				public override bool CanEnterInventory {
						get {
								return HasBeenPicked;
						}
				}

				public bool HasBeenPicked {
						get {
								return State.TimePicked > 0f;
						}
				}

				public bool HasBeenPlanted {
						get {
								return State.TimePlanted > 0f && !HasBeenPicked;
						}
				}

				public override void Awake()
				{
						base.Awake();
						//kill accidentally set editor props
						Props = null;
				}

				public Plant Props = null;
				public WorldPlantState State = new WorldPlantState();

				public bool HasPlantProps {
						get {
								return Props != null;
						}
				}

				public void RefreshPlantProps()
				{
						if (string.IsNullOrEmpty(State.PlantName)) {
								State.PlantName = worlditem.Props.Local.Subcategory;
						} else {
								worlditem.Props.Local.Subcategory = State.PlantName;
						}
						if (State.Season == TimeOfYear.None || !HasBeenPicked) {
								State.Season = WorldClock.SeasonCurrent;
						}
						//use the world item's subcategory to get our plant properties
						if (Plants.Get.PlantProps(State.PlantName, ref Props)) {
								if (Plants.Get.InitializeWorldPlantGameObject(gameObject, State.PlantName, State.Season)) {
										Plants.Get.InitializeWorldPlantFoodStuff(this, Props);
										worlditem.Props.Name.StackName = Props.Name;
										//make sure the current season is set
								}
						}
//			}
				}

				public override void InitializeTemplate()
				{
						WorldItem worlditem = gameObject.GetComponent <WorldItem>();
						worlditem.Props.Local.DisplayNamerScript = "WorldPlant";
				}

				public override void OnInitialized()
				{
						worlditem.ActiveStateLocked = false;
						worlditem.ActiveState = WIActiveState.Active;
						worlditem.ActiveStateLocked = true;

						Detectable detectable = worlditem.Get <Detectable>();
						detectable.OnPlayerDetect += OnPlayerDetect;

						FoodStuff foodStuff = worlditem.Get <FoodStuff>();
						foodStuff.OnEat += OnEat;
						foodStuff.State.ConsumeOnEat = false;//we'll handle consumption

						Damageable damageable = worlditem.Get <Damageable>();
						damageable.OnDie += OnDie;

						if (HasBeenPicked) {
								RefreshPlantProps();
						}
				}

				public void OnDie()
				{
						Damageable damageable = worlditem.Get <Damageable>();
						damageable.ResetDamage();
						Plants.Dispose(this);
				}

				public void OnEat()
				{
						Props.Revealed = true;
						Props.EncounteredTimesOfYear |= WorldClock.SeasonCurrent;
						Props.NumTimesEncountered++;
						if (worlditem.Get <FoodStuff>().Props.Name == "Raw") {
								Props.RawPropsRevealed = true;
						} else {
								Props.CookedPropsRevealed = true;
						}
						Plants.SaveProps(Props);
						worlditem.RemoveFromGame();
				}

				public void OnPlayerDetect()
				{
						Props.Revealed = true;
						Props.CurrentSeason.Revealed = true;
						Props.EncounteredTimesOfYear |= WorldClock.SeasonCurrent;
						Plants.SaveProps(Props);
				}

				public void OnPlaced()
				{
						//don't bother to lock our state any more
						RefreshPlantProps();
				}

				public override void PopulateOptionsList(List<WIListOption> options, List <string> message)
				{
						if (!HasBeenPicked && worlditem.Is(WIMode.Frozen)) {
								options.Add(new WIListOption("Pick " + worlditem.DisplayName, "PickPlant"));
						}
				}

				public virtual void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{	//this is where we handle skills
						WIListResult dialogResult = secondaryResult as WIListResult;
						switch (dialogResult.SecondaryResult) {
								case "PickPlant":
										PickPlant(true);
										break;

								default:
										break;
						}
				}

				public void PickPlant(bool addToInventory)
				{
						if (!HasBeenPicked && worlditem.Is(WIMode.Frozen)) {
								//this will handle everything
								Props.Revealed = true;
								Props.EncounteredTimesOfYear |= WorldClock.SeasonCurrent;
								Plants.SaveProps(Props);
								Plants.Pick(this, addToInventory);
						}
				}

				public void OnCollisionEnter(Collision other)
				{
						if (other.collider == null || other.collider.isTrigger)
								return;

						if (HasPlantProps && Props.HasThorns) {
								if (WorldItems.GetIOIFromCollider(other.collider, out Plants.Get.ThornDamage.Target)) {
										DamageManager.Get.SendDamage(Plants.Get.ThornDamage);
								}
						}
				}

				protected bool mIsPicking = false;
		}

		[Serializable]
		public class WorldPlantState
		{
				[FrontiersAvailableModsAttribute("Plant")]
				public string PlantName;
				public double TimePicked = -1f;
				public double TimePlanted = -1f;
				[BitMaskAttribute(typeof(TimeOfYear))]
				public TimeOfYear Season;
				public float Elevation;
		}
}