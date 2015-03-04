using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;

namespace Frontiers.World.BaseWIScripts
{
		public class Visitable : WIScript
		{		//creates a visit trigger that watches for items of interest
				//which is a rigidbody trigger that exists outside of the main transform heirarchy
				//this is just to keep things clean and to abide by the 'rules' of rigidbodies
				public VisitTrigger Trigger = null;
				public VisitableState State = new VisitableState();
				public Action OnPlayerVisitFirstTime;
				public Action OnPlayerLeaveFirstTime;
				public Action OnPlayerVisit;
				public Action OnPlayerLeave;
				public Action OnItemOfInterestVisit;
				public Action OnItemOfInterestLeave;
				public WorldItem LastItemOfInterestToVisit = null;
				public WorldItem LastItemOfInterestToLeave = null;
				public List <string> ItemsOfInterest = new List <string>();
				public bool PlayerOnly = true;

				public override void OnInitialized()
				{
						worlditem.OnAddedToGroup += OnAddedToGroup;
				}

				public void OnAddedToGroup()
				{
						GameObject triggerObject = new GameObject("Visit Trigger - " + name);
						Trigger = triggerObject.AddComponent <VisitTrigger>();
						Trigger.Initialize(this);
				}

				public bool IsVisiting {
						get {
								return State.IsVisiting;
						}
				}

				public void PlayerVisit(LocationVisitMethod method)
				{
						State.NumTimesVisited++;
						State.IsVisiting = true;
						State.TimeLastVisited = WorldClock.AdjustedRealTime;
						if (State.FirstVisitMethod == LocationVisitMethod.None) {
								State.FirstVisitMethod = method;
								State.TimeFirstVisit = WorldClock.AdjustedRealTime;
								OnPlayerVisitFirstTime.SafeInvoke();
						}
						OnPlayerVisit.SafeInvoke();
						Player.Local.Surroundings.Visit(this);
				}

				public void PlayerLeave()
				{
						State.IsVisiting = false;
						State.TimeLastLeft = WorldClock.AdjustedRealTime;
						State.TotalTimeVisited += (State.TimeLastLeft - State.TimeLastVisited);
						if (State.NumTimesVisited <= 1) {
								State.TimeFirstLeft = WorldClock.AdjustedRealTime;
								OnPlayerLeaveFirstTime.SafeInvoke();
						}
						OnPlayerLeave.SafeInvoke();
						Player.Local.Surroundings.Leave(this);
				}

				public void ItemOfInterestVisit(WorldItem itemOfInterest)
				{
						LastItemOfInterestToVisit = itemOfInterest;
						OnItemOfInterestVisit.SafeInvoke();
				}

				public void ItemOfInterestLeave(WorldItem itemOfInterest)
				{
						LastItemOfInterestToLeave = itemOfInterest;
						OnItemOfInterestLeave.SafeInvoke();
				}
		}

		[Serializable]
		public class VisitableState
		{
				public bool HasBeenVisited {
						get {
								return FirstVisitMethod != LocationVisitMethod.None;
						}
				}

				public LocationVisitMethod FirstVisitMethod = LocationVisitMethod.None;
				public double TimeFirstVisit = 0.0f;
				public double TimeLastVisited = 0.0f;
				public double TimeFirstLeft = 0.0f;
				public double TimeLastLeft = 0.0f;
				public double TotalTimeVisited = 0.0f;
				public int NumTimesVisited = 0;
				public bool IsVisiting = false;
		}
}