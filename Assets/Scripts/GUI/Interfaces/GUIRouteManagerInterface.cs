using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers;


public class GUIRouteManagerInterface : MonoBehaviour
{
//	public Camera 					InterfaceCamera;
//	
//	public WIStack				CurrentContainer
//	{
//		get
//		{
//			return CurrentContainerDisplay.Stack;
//		}
//	}
//	public int						CurrentContainerIndex = 0;
//	public InventorySquare	CurrentContainerDisplay;
//	public bool						AreFoodRequirementsMet
//	{
//		get
//		{
//			return true;//FoodAvailable >= FoodNeeded;
//		}
//	}
//	public bool						AreWaterRequirementsMet
//	{
//		get
//		{
//			return WaterAvailable >= WaterNeeded;
//		}
//	}
//	public bool						AreGoldRequirementsMet
//	{
//		get
//		{
//			return true;
//		}
//	}
//	
//	public UILabel					StatusLabel;
//	public Vector4					ActivePanelClipping;
//	public Vector4					InactivePanelClipping;
//	public float					ActiveBackgroundScale;
//	public float					InactiveBackgroundScale;
//	public float					ActivePlanRouteButtonPosition;
//	public float					InactivePlanRouteButtonPosition;
//	
//	public float					FoodNeeded;
//	public float					WaterNeeded;
//	public int						GoldPiecesNeeded;
//	public float					FoodAvailable;
//	public float					WaterAvailable;
//	
//	public GameObject				StopsAlongRouteDivider;
//	public GameObject				AvailablePathMarkersDivider;
//	public GameObject				StopsAlongRouteDividerNoneLabel;
//	public GameObject				AvailablePathMarkersDividerNoneLabel;
//	public float					DividerOffset					= -30.0f;
//	
//	public Vector4					TargetPanelClipping;
//	public float					TargetBackgroundScale;
//	public float					TargetPlanRouteButtonPosition;
//	
//	public UIButton					PlanRouteButton;
//	public UILabel					PlanRouteButtonLabel;
//	
//	public UIAnchor					Anchor;
//	public GameObject				RouteLegBrowserPanel;
//	public UILabel					FoodNeededLabel;
//	public UILabel					TravelTimeLabel;
//	public UILabel					TotalDistanceLabel;
//	public UISlicedSprite			BackgroundSprite;
//	public UISlider					TravelSpeed;
//	
//	public UIPanel					ClippingPanel;
//	
//	public GameObject 				RouteLegBrowserPrefab;
//	public GameObject				AvailablePathMarkerBrowserPrefab;
//	
//	public void 					Awake ( )
//	{
//		DontDestroyOnLoad (this);
//	}
//	
//	public void						Start ( )
//	{
//		//Player.Get.AvatarActions.Subscribe (AvatarAction.RouteChange, 		new ActionListener (RouteChange));
//		//Player.Get.AvatarActions.Subscribe (AvatarAction.RouteTravelStart, new ActionListener (RouteTravelStart));
//		//Player.Get.AvatarActions.Subscribe (AvatarAction.RouteTravelStop, 	new ActionListener (RouteTravelStop));
//		
//		TargetPanelClipping 			= InactivePanelClipping;
//		TargetBackgroundScale			= InactiveBackgroundScale;
//		TargetPlanRouteButtonPosition	= InactivePlanRouteButtonPosition;
//		
//		Refresh ( );
//	}
//	
//	public void						Update ( )
//	{
//		if (Input.GetKeyDown (KeyCode.KeypadPlus))
//		{
//			TravelSpeed.sliderValue = Mathf.Clamp (TravelSpeed.sliderValue + 0.1f, 0.0f, 1.0f);
//		}
//		
//		if (Input.GetKeyDown (KeyCode.KeypadMinus))	
//		{
//			TravelSpeed.sliderValue = Mathf.Clamp (TravelSpeed.sliderValue - 0.1f, 0.0f, 1.0f);
//		}
//		
//		if (!PrimaryInterface.IsMaximized ("Map"))
//		{
//			return;
//		}
//		
//		ClippingPanel.clipRange 					= Vector4.Lerp (ClippingPanel.clipRange, TargetPanelClipping, 0.35f);
//		Vector3 planRouteButtonPosition = PlanRouteButton.transform.localPosition;
//		PlanRouteButton.transform.localPosition 	= Vector3.Lerp (PlanRouteButton.transform.localPosition, new Vector3 (planRouteButtonPosition.x, TargetPlanRouteButtonPosition, planRouteButtonPosition.z), 0.35f);
//		Vector3 backgroundScale = BackgroundSprite.transform.localScale;
//		BackgroundSprite.transform.localScale		= Vector3.Lerp (BackgroundSprite.transform.localScale, new Vector3 (backgroundScale.x, TargetBackgroundScale, backgroundScale.z), 0.35f);
//		
//		if (!PrimaryInterface.IsMaximized ("Map"))
//		{
//			return;
//		}
//		
//		switch (GameManager.Get.Travel.Mode)
//		{
//		case TravelManager.TravelMode.NoRoute:
//			StopsAlongRouteDivider.SetActive (false);
//			AvailablePathMarkersDivider.SetActive (false);
//			break;
//			
//		case TravelManager.TravelMode.NoStartMarker:
//			StopsAlongRouteDivider.SetActive (false);
//			AvailablePathMarkersDivider.SetActive (false);
//			break;
//				
//		case TravelManager.TravelMode.ReadyToTravel:
//			if (!StopsAlongRouteDivider.activeSelf) {StopsAlongRouteDivider.SetActive (true); }
//			if (RouteLegObjects.Count == 0) { StopsAlongRouteDividerNoneLabel.SetActive (true); }
//			else { StopsAlongRouteDividerNoneLabel.SetActive (false); }
//			StopsAlongRouteDivider.transform.localPosition 		= Vector3.Lerp (StopsAlongRouteDivider.transform.localPosition,
//																	new Vector3 (0f, 0f, -5f), 0.35f);
//			AvailablePathMarkersDivider.SetActive (false);
//			break;
//			
//		case TravelManager.TravelMode.Traveling:
//			GameManager.Get.Travel.Navigation.WorldMapCamera.enabled = false;
//			break;
//			
//		case TravelManager.TravelMode.ReachedDestination:
//			if (!StopsAlongRouteDivider.activeSelf) {StopsAlongRouteDivider.SetActive (true); }
//			if (RouteLegObjects.Count == 0) { StopsAlongRouteDividerNoneLabel.SetActive (true); }
//			else { StopsAlongRouteDividerNoneLabel.SetActive (false); }
//			StopsAlongRouteDivider.transform.localPosition 		= Vector3.Lerp (StopsAlongRouteDivider.transform.localPosition,
//																	new Vector3 (0f, 0f, -5f), 0.35f);
//			AvailablePathMarkersDivider.SetActive (false);
//			break;
//			
//		default:
//			if (AvailablePathMarkerObjects.Count == 0) { AvailablePathMarkersDividerNoneLabel.SetActive (true); }
//			else { AvailablePathMarkersDividerNoneLabel.SetActive (false); }
//			if (RouteLegObjects.Count == 0) { StopsAlongRouteDividerNoneLabel.SetActive (true); }
//			else { StopsAlongRouteDividerNoneLabel.SetActive (false); }
//			if (!StopsAlongRouteDivider.activeSelf) {StopsAlongRouteDivider.SetActive (true); }
//			if (!AvailablePathMarkersDivider.activeSelf) {AvailablePathMarkersDivider.SetActive (true); }
//			StopsAlongRouteDivider.transform.localPosition 		= Vector3.Lerp (StopsAlongRouteDivider.transform.localPosition,
//																	new Vector3 (0f, 0f, -5f), 0.35f);
//			AvailablePathMarkersDivider.transform.localPosition = Vector3.Lerp (AvailablePathMarkersDivider.transform.localPosition,
//																	new Vector3 (0f, (RouteLegObjects.Count * mBrowserObjectOffset) + DividerOffset, -5f), 0.35f);
//			break;
//		}
//	}
//	
//	public void						Hide ( )
//	{
//		Anchor.relativeOffset = new Vector2 (-1.0f, 0f);
//	}
//	
//	public void						Show ( )	
//	{
//		Anchor.relativeOffset = new Vector2 (0f, 0f);
//		Refresh ( );
//	}
//	
//	public void						OnTravelSpeedSliderChange ( )
//	{
//		GameManager.Get.Travel.TravelSpeed = TravelSpeed.sliderValue;
//	}
//	
//	public void						OnClickPlanRoute ( )
//	{
////		//Debug.Log ("Clicking plan route");
//		
//		switch (GameManager.Get.Travel.Mode)
//		{
//		case TravelManager.TravelMode.NoStartMarker:
////			//Debug.Log ("mode NoStartMarker");
//			GUIManager.PostWarning ("Can't plan new route: You're not visiting a valid path marker.");
//			break;
//			
//		case TravelManager.TravelMode.NoRoute:
////			//Debug.Log ("mode NoRoute");
//			GameManager.Get.Travel.CreateNewRoute (GameManager.Get.Travel.PlayerLastVisitedPathMarker);
//			break;
//			
//		case TravelManager.TravelMode.AddingMarkersToRoute:
////			//Debug.Log ("mode AddingMarkersToRoute");
//			GameManager.Get.Travel.FinishRoute ( );
//			break;
//			
//		case TravelManager.TravelMode.ReadyToTravel:
////			//Debug.Log ("mode ReadyToTravel");
//			GameManager.Get.Travel.StartTraveling ( );
//			break;
//			
//		case TravelManager.TravelMode.Traveling:
////			//Debug.Log ("mode Traveling");
//			GameManager.Get.Travel.PauseTraveling ( );
//			break;
//			
//		case TravelManager.TravelMode.TravelingPaused:
////			//Debug.Log ("mode TravelingPaused");
//			GameManager.Get.Travel.ResumeTraveling ( );
//			break;
//			
//		case TravelManager.TravelMode.ReachedDestination:
////			//Debug.Log ("mode ReachedDestination");
//			GameManager.Get.Travel.CreateNewRoute (GameManager.Get.Travel.PlayerLastVisitedPathMarker);
//			break;
//			
//		default:
//			break;
//		}
//	}
//	
//	public void						OnClickStartTraveling ( )
//	{
//		//Debug.Log ("Clicking start traveling...");
//		GameManager.Get.Travel.StartTraveling ( );
//	}
//	
//	public void						OnClickClearRoute ( )
//	{
//		if (GameManager.Get.Travel.IsOnRoute)
//		{
//			GameManager.Get.Travel.StopTraveling ( );
//		}
//		else
//		{
//			GameManager.Get.Travel.ClearRoute ( ); 
//		}
//	}
//	
//	public void						OnClickNextContainerButton ( )
//	{
//		List <WIStack> availableStackEnablers = new List <WIStack> ( );
//		foreach (WIStack stack in Player.Local.Inventory.EnablerStacks)
//		{
//			if (stack.NumItems > 0)
//			{
//				availableStackEnablers.Add (stack);
//			}
//		}
//		
//		if (availableStackEnablers.Count == 0)
//		{
//			CurrentContainerDisplay.SetStack (null);
//			RefreshRequirements ( );
//			return;
//		}	
//		else
//		{		
//			CurrentContainerIndex++;
//			if (CurrentContainerIndex >= availableStackEnablers.Count)
//			{
//				CurrentContainerIndex = 0;
//			}
//			CurrentContainerDisplay.SetStack (Player.Local.Inventory.EnablerStacks [CurrentContainerIndex]);
//		}
//		RefreshRequirements ( );
//	}
//	
//	public void						OnClickPrevContainerButton ( )
//	{
//		List <WIStack> availableStackEnablers = new List <WIStack> ( );
//		foreach (WIStack stack in Player.Local.Inventory.EnablerStacks)
//		{
//			if (stack.NumItems > 0)
//			{
//				availableStackEnablers.Add (stack);
//			}
//		}
//		
//		if (availableStackEnablers.Count == 0)
//		{
//			CurrentContainerDisplay.SetStack (null);
//		}
//		else
//		{		
//			CurrentContainerIndex--;
//			if (CurrentContainerIndex < 0)
//			{
//				CurrentContainerIndex = availableStackEnablers.Count - 1;
//			}
//			CurrentContainerDisplay.SetStack (Player.Local.Inventory.EnablerStacks [CurrentContainerIndex]);
//		}
//		RefreshRequirements ( );
//	}
//	
//	public void						OnClickRouteLeg (GameObject routeLegBrowserGameObject)
//	{
////		//Debug.Log ("OnClickRouteLeg " + routeLegBrowserGameObject.name);
//	}
//	
//	public void						OnMouseOverRouteLeg (GameObject routeLegBrowserGameObject)
//	{
//		GUIRouteLegBrowserObject routeLegBrowserObject = routeLegBrowserGameObject.transform.parent.gameObject.GetComponent <GUIRouteLegBrowserObject> ( );
//		if (routeLegBrowserObject.DisplayMode == GUIRouteLegBrowserObject.Mode.StartLocation)
//		{
//			GameManager.Get.Travel.RegionSelection.HighlightPathMarker (routeLegBrowserObject.StartLocation);
//		}
//		else if (routeLegBrowserObject.DisplayMode == GUIRouteLegBrowserObject.Mode.Destination)
//		{
//			GameManager.Get.Travel.RegionSelection.HighlightPathMarker (routeLegBrowserObject.DestinationMarker);	
//		}
//		else
//		{
//			GameManager.Get.Travel.RegionSelection.HighlightRouteLeg (routeLegBrowserObject.Leg);
//		}
//	}
//	
//	public void						OnMouseOutRouteLeg (GameObject routeLegBrowserGameObject)
//	{
//		GUIRouteLegBrowserObject routeLegBrowserObject = routeLegBrowserGameObject.transform.parent.gameObject.GetComponent <GUIRouteLegBrowserObject> ( );
//		if (routeLegBrowserObject.DisplayMode == GUIRouteLegBrowserObject.Mode.StartLocation)
//		{
//			GameManager.Get.Travel.RegionSelection.UnHighlightPathMarker ( );
//		}
//		else if (routeLegBrowserObject.DisplayMode == GUIRouteLegBrowserObject.Mode.Destination)
//		{
//			GameManager.Get.Travel.RegionSelection.UnHighlightPathMarker ( );		
//		}
//		else
//		{
//			GameManager.Get.Travel.RegionSelection.UnHighlightRouteLeg ( );
//		}
//	}
//	
//	public void						OnClickAvailablePathMarker (GameObject availablePathMarkerBrowserGameObject)
//	{
////		//Debug.Log ("OnClickAvailablePathMarker " + availablePathMarkerBrowserGameObject.name);
//		GUIAvailablePathMarkerObject availablePathMarkerObject = availablePathMarkerBrowserGameObject.transform.parent.gameObject.GetComponent <GUIAvailablePathMarkerObject> ( );
//		mAddRouteLegOnly = true;
//		GameManager.Get.Travel.AddMarkerToRoute (availablePathMarkerObject.PathMarker);
//	}
//	
//	public void						OnMouseOverAvailablePathMarker (GameObject availablePathMarkerBrowserGameObject)
//	{
////		//Debug.Log ("Mousing over available path marker");
//		GUIAvailablePathMarkerObject availablePathMarkerObject = availablePathMarkerBrowserGameObject.transform.parent.gameObject.GetComponent <GUIAvailablePathMarkerObject> ( );
//		GameManager.Get.Travel.RegionSelection.HighlightPathMarker (availablePathMarkerObject.PathMarker);
//	}
//	
//	public void						OnMouseOutAvailablePathMarker (GameObject availablePathMarkerBrowserGameObject)
//	{
////		//Debug.Log ("Mousing out available path marker");
//		GameManager.Get.Travel.RegionSelection.UnHighlightPathMarker ( );
//	}
//	
//	public void						RouteTravelStart ( )
//	{
//		Refresh ( );
//	}
//	
//	public void						RouteTravelStop ( )
//	{
//		Refresh ( );
//	}
//	
//	public void						RouteChange ( )
//	{
//		Refresh (true);
//	}
//	
//	public void						Refresh ( )
//	{
//		Refresh (false);
//	}
//	
//	public void						RefreshRequirements ( )
//	{
////		//Debug.Log ("Refreshing requirements");
//		if (GameManager.Get.Travel.HasRoute)
//		{
////			//Debug.Log ("We have a route");
//			//total distance in meters / average meters per hour = total hours to travel time required food per hour
//			float distanceInMeters = GameManager.Get.Travel.CurrentRoute.TotalDistanceInMeters;
//			FoodNeeded 			= (distanceInMeters / Globals.PlayerAverageMetersPerHour)
//									* Globals.RequiredFoodPerGameHour;
//			WaterNeeded 		= (distanceInMeters / Globals.PlayerAverageMetersPerHour)
//									* Globals.RequiredWaterPerGameHour;
//			
//			FoodAvailable		= 0.0f;
//			WaterAvailable		= 0.0f;
//			
//			if (CurrentContainer != null)
//			{
////				//Debug.Log ("We have a current container");
//				if (CurrentContainer.NumItems > 0)
//				{
//					FoodAvailable 	= CurrentContainer.TopItem.StackContainer.TotalFood;
////					//Debug.Log ("Food available is " + FoodAvailable);
//				}
//				WaterAvailable 		= 100.0f;
//			}
//			
//			TotalDistanceLabel.text = (GameManager.Get.Travel.CurrentRoute.TotalDistanceInMeters / 1000.0f).ToString ("F") + " km";	
//			TravelTimeLabel.text 	= (GameManager.Get.Travel.CurrentRoute.RequiredGameHours).ToString ("F") + " Hours";
//			FoodNeededLabel.text 	= FoodAvailable.ToString ("F") + "/" + FoodNeeded.ToString ("F") + " kg";
//			if (AreFoodRequirementsMet)
//			{
//				FoodNeededLabel.color = Colors.Get.MessageSuccessColor;
//				PlanRouteButton.SendMessage ("SetEnabled", SendMessageOptions.DontRequireReceiver);
//			}
//			else
//			{
//				FoodNeededLabel.color = Colors.Get.MessageDangerColor;
//				if (	GameManager.Get.Travel.Mode == TravelManager.TravelMode.ReadyToTravel
//					||	GameManager.Get.Travel.Mode == TravelManager.TravelMode.AddingMarkersToRoute)
//				{
//					PlanRouteButton.SendMessage ("SetDisabled");
//				}
//			}
//		}
//	}
//	
//	public void 					Refresh (bool routeChange)
//	{
//////		//Debug.Log ("Refreshing with mode " + GameManager.Get.Travel.Mode.ToString ( ) + " and route change is " + routeChange.ToString ( ));
////		
////		switch (GameManager.Get.Travel.Mode)
////		{
////		case TravelManager.TravelMode.NoStartMarker:
////			TargetBackgroundScale 				= InactiveBackgroundScale;
////			TargetPanelClipping					= InactivePanelClipping;
////			TargetPlanRouteButtonPosition 		= InactivePlanRouteButtonPosition;
////			PlanRouteButton.gameObject.SetActive (false);
////			ClearRouteLegs ( );
////			ClearAvailablePathMarkers ( );
////			StatusLabel.text = "(Not visiting a usable path marker)";
////			break;
////			
////		case TravelManager.TravelMode.NoRoute:
////			TargetBackgroundScale 				= InactiveBackgroundScale;
////			TargetPanelClipping					= InactivePanelClipping;
////			TargetPlanRouteButtonPosition 		= InactivePlanRouteButtonPosition;
////			PlanRouteButton.gameObject.SetActive (true);
////			PlanRouteButtonLabel.text			= "Plan WorldRoute";
////			ClearRouteLegs ( );
////			ClearAvailablePathMarkers ( );
////			StatusLabel.text = "(Click 'Plan WorldRoute' to begin)";
////			break;
////			
////		case TravelManager.TravelMode.AddingMarkersToRoute:
////			TargetBackgroundScale 				= ActiveBackgroundScale;
////			TargetPanelClipping					= ActivePanelClipping;
////			TargetPlanRouteButtonPosition 		= ActivePlanRouteButtonPosition;
////			PlanRouteButton.gameObject.SetActive (true);
////			PlanRouteButtonLabel.text			= "Finished Adding Markers";
////			PlanRouteButton.SendMessage ("SetEnabled", SendMessageOptions.RequireReceiver);
////			StatusLabel.text = "Add path markers to route";
////			break;
////			
////		case TravelManager.TravelMode.ReadyToTravel:
////			TargetBackgroundScale 				= ActiveBackgroundScale;
////			TargetPanelClipping					= ActivePanelClipping;
////			TargetPlanRouteButtonPosition 		= ActivePlanRouteButtonPosition;
////			PlanRouteButton.gameObject.SetActive (true);
////			PlanRouteButtonLabel.text			= "Start Traveling";
////			StatusLabel.text = "Click 'Start Traveling' to use route";
////			break;
////			
////		case TravelManager.TravelMode.Traveling:
////			TargetBackgroundScale 				= ActiveBackgroundScale;
////			TargetPanelClipping					= ActivePanelClipping;
////			TargetPlanRouteButtonPosition 		= ActivePlanRouteButtonPosition;
////			PlanRouteButtonLabel.text			= "Pause Traveling";
////			StatusLabel.text = "Traveling...";
////			break;
////			
////		case TravelManager.TravelMode.TravelingPaused:
////			TargetBackgroundScale 				= ActiveBackgroundScale;
////			TargetPanelClipping					= ActivePanelClipping;
////			TargetPlanRouteButtonPosition 		= ActivePlanRouteButtonPosition;
////			PlanRouteButtonLabel.text			= "Resume Traveling";
////			StatusLabel.text = "(Traveling paused)";
////			break;
////			
////		case TravelManager.TravelMode.ReachedDestination:
////			TargetBackgroundScale 				= InactiveBackgroundScale;
////			TargetPanelClipping					= InactivePanelClipping;
////			TargetPlanRouteButtonPosition 		= InactivePlanRouteButtonPosition;
////			PlanRouteButton.gameObject.SetActive (true);
////			PlanRouteButtonLabel.text			= "Start new route";
////			StatusLabel.text = "You have reached your destination.";
////			FrontiersInterface.MinimizeInterface ("Map");
////			break;
////			
////		default:
////			break;
////		}
////		
////		CurrentContainerDisplay.UpdateDisplay ( );
////		
////		if (routeChange)
////		{
////			if (mAddRouteLegOnly)
////			{				
////				mCurrentObjectIndex = RouteLegObjects.Count;
////				AddRouteLeg (GameManager.Get.Travel.CurrentRoute.EndLeg);
////				RefreshRouteLegs ( );
////				ClearAvailablePathMarkers ( );
////				CreateAvailablePathMarkers ( );
////				mAddRouteLegOnly = false;
////			}
////			else
////			{
//////				//Debug.Log ("Clearing all markers");
////				mCurrentObjectIndex = 0;
////				mLastLegObjectCount = 1000;
////				ClearRouteLegs ( );
////				ClearAvailablePathMarkers ( );
////				CreateRouteLegs ( );
////				CreateAvailablePathMarkers ( );
////			}
////			
////			RefreshRequirements ( );
////		}
//	}
//	
//	public void						CreateRouteLegs ( )
//	{
//		if (GameManager.Get.Travel.HasRoute)
//		{
//			if (GameManager.Get.Travel.CurrentRoute.NumLegs < 2)
//			{
//				AddStartMarkerLeg (GameManager.Get.Travel.CurrentRoute.StartLocation);				
//			}
//			else if (GameManager.Get.Travel.CurrentRoute.NumLegs < 3)
//			{
//				AddStartLeg (GameManager.Get.Travel.CurrentRoute.StartLeg);
//				if (GameManager.Get.Travel.CurrentRoute.IsClosed)
//				{
//					AddDestinationMarkerLeg (GameManager.Get.Travel.CurrentRoute.EndLocation);
//				}
//				else
//				{
//					AddRouteLeg (GameManager.Get.Travel.CurrentRoute.EndLeg);
//				}
//			}
//			else
//			{	
//				foreach (RouteLeg leg in GameManager.Get.Travel.CurrentRoute.Legs)
//				{
//					if (leg == GameManager.Get.Travel.CurrentRoute.StartLeg)
//					{
//						AddStartLeg (GameManager.Get.Travel.CurrentRoute.StartLeg);
//					}
//					else if (leg == GameManager.Get.Travel.CurrentRoute.EndLeg && GameManager.Get.Travel.CurrentRoute.IsClosed)
//					{
//						AddDestinationMarkerLeg (GameManager.Get.Travel.CurrentRoute.EndLocation);
//					}
//					else
//					{
//						AddRouteLeg (leg);
//					}
//				}
//			}
//		}
//		mLastLegObjectCount = RouteLegObjects.Count;
//	}
//	
//	public void						AddStartMarkerLeg (Location startMarker)
//	{
//		GameObject newLegBrowserGameObject						= NGUITools.AddChild (RouteLegBrowserPanel, RouteLegBrowserPrefab);
//		newLegBrowserGameObject.name							= "StartMarkerLeg";
//		GUIRouteLegBrowserObject newLegBrowserObject			= newLegBrowserGameObject.GetComponent <GUIRouteLegBrowserObject> ( );
//		if (mCurrentObjectIndex <= mLastLegObjectCount)
//		{
//			newLegBrowserObject.transform.localPosition			= new Vector3 (0f, (mBrowserObjectOffset * mCurrentObjectIndex) + DividerOffset, -15f);
//		}
//		else
//		{
//			newLegBrowserObject.transform.localPosition			= Vector3.zero;
//		}
//		newLegBrowserObject.Leg									= null;
//		newLegBrowserObject.StartLocation							= startMarker;
//		newLegBrowserObject.DisplayMode							= GUIRouteLegBrowserObject.Mode.StartLocation;
//		newLegBrowserObject.FunctionTarget						= this.gameObject;
//		newLegBrowserObject.Index								= mCurrentObjectIndex;
//		newLegBrowserObject.YOffset								= DividerOffset;
//		newLegBrowserObject.YSize								= mBrowserObjectOffset;
//		
//		newLegBrowserObject.Refresh ( );
//		
//		RouteLegObjects.Add (newLegBrowserObject);
//		mCurrentObjectIndex++;		
//	}
//	
//	public void 					AddStartLeg (RouteLeg newLeg)
//	{
//		GameObject newLegBrowserGameObject						= NGUITools.AddChild (RouteLegBrowserPanel, RouteLegBrowserPrefab);
//		newLegBrowserGameObject.name							= "Leg";
//		GUIRouteLegBrowserObject newLegBrowserObject			= newLegBrowserGameObject.GetComponent <GUIRouteLegBrowserObject> ( );
//		if (mCurrentObjectIndex <= mLastLegObjectCount)
//		{
//			newLegBrowserObject.transform.localPosition			= new Vector3 (0f, (mBrowserObjectOffset * mCurrentObjectIndex) + DividerOffset, -15f);
//		}
//		else
//		{
//			newLegBrowserObject.transform.localPosition			= Vector3.zero;
//		}
//		newLegBrowserObject.Leg									= newLeg;
//		newLegBrowserObject.FunctionTarget						= this.gameObject;
//		newLegBrowserObject.Index								= mCurrentObjectIndex;
//		newLegBrowserObject.DisplayMode							= GUIRouteLegBrowserObject.Mode.StartLeg;
//		newLegBrowserObject.YOffset								= DividerOffset;
//		newLegBrowserObject.YSize								= mBrowserObjectOffset;
//		
//		newLegBrowserObject.Refresh ( );
//		
//		RouteLegObjects.Add (newLegBrowserObject);
//		mCurrentObjectIndex++;
//	}
//	
//	public void 					AddRouteLeg (RouteLeg newLeg)
//	{
//		GameObject newLegBrowserGameObject						= NGUITools.AddChild (RouteLegBrowserPanel, RouteLegBrowserPrefab);
//		newLegBrowserGameObject.name							= "Leg";
//		GUIRouteLegBrowserObject newLegBrowserObject			= newLegBrowserGameObject.GetComponent <GUIRouteLegBrowserObject> ( );
//		if (mCurrentObjectIndex <= mLastLegObjectCount)
//		{
//			newLegBrowserObject.transform.localPosition			= new Vector3 (0f, (mBrowserObjectOffset * mCurrentObjectIndex) + DividerOffset, -15f);
//		}
//		else
//		{
//			newLegBrowserObject.transform.localPosition			= Vector3.zero;
//		}
//		newLegBrowserObject.Leg									= newLeg;
//		newLegBrowserObject.FunctionTarget						= this.gameObject;
//		newLegBrowserObject.Index								= mCurrentObjectIndex;
//		newLegBrowserObject.DisplayMode							= GUIRouteLegBrowserObject.Mode.Default;
//		newLegBrowserObject.YOffset								= DividerOffset;
//		newLegBrowserObject.YSize								= mBrowserObjectOffset;
//		
//		newLegBrowserObject.Refresh ( );
//		
//		RouteLegObjects.Add (newLegBrowserObject);
//		mCurrentObjectIndex++;
//	}
//	
//	public void 					AddDestinationMarkerLeg (Location destinationMarker)
//	{
//		GameObject newLegBrowserGameObject						= NGUITools.AddChild (RouteLegBrowserPanel, RouteLegBrowserPrefab);
//		newLegBrowserGameObject.name							= "Leg";
//		GUIRouteLegBrowserObject newLegBrowserObject			= newLegBrowserGameObject.GetComponent <GUIRouteLegBrowserObject> ( );
//		if (mCurrentObjectIndex <= mLastLegObjectCount)
//		{
//			newLegBrowserObject.transform.localPosition			= new Vector3 (0f, (mBrowserObjectOffset * mCurrentObjectIndex) + DividerOffset, -15f);
//		}
//		else
//		{
//			newLegBrowserObject.transform.localPosition			= Vector3.zero;
//		}
//		newLegBrowserObject.Leg									= null;
//		newLegBrowserObject.DestinationMarker					= destinationMarker;
//		newLegBrowserObject.DisplayMode							= GUIRouteLegBrowserObject.Mode.Destination;
//		newLegBrowserObject.FunctionTarget						= this.gameObject;
//		newLegBrowserObject.Index								= mCurrentObjectIndex;
//		newLegBrowserObject.YOffset								= DividerOffset;
//		newLegBrowserObject.YSize								= mBrowserObjectOffset;
//		
//		newLegBrowserObject.Refresh ( );
//		
//		RouteLegObjects.Add (newLegBrowserObject);
//		mCurrentObjectIndex++;
//	}
//	
//	public void						RefreshRouteLegs ( )
//	{
//		foreach (GUIRouteLegBrowserObject routeLegBrowserObject in RouteLegObjects)
//		{
//			routeLegBrowserObject.Refresh ( );
//		}
//		mLastLegObjectCount = RouteLegObjects.Count;
//	}
//	
//	public void						CreateAvailablePathMarkers ( )
//	{		
//		if (GameManager.Get.Travel.HasRoute)
//		{
//			foreach (Location availablePathMarker in GameManager.Get.Travel.CurrentRoute.NextAvailablePathMarkers)
//			{
//				AddAvailablePathMarker (availablePathMarker);
//			}
//		}
//	}
//	
//	public void 					AddAvailablePathMarker (Location availablePathMarker)
//	{		
//		GameObject newAvailablePathMarkerGameObject					= NGUITools.AddChild (RouteLegBrowserPanel, AvailablePathMarkerBrowserPrefab);
//		newAvailablePathMarkerGameObject.name						= availablePathMarker.name;
//		newAvailablePathMarkerGameObject.transform.localPosition	= new Vector3 (0f, RouteLegObjects.Count * mBrowserObjectOffset, -15f);
//		GUIAvailablePathMarkerObject newAvailablePathMarker			= newAvailablePathMarkerGameObject.GetComponent <GUIAvailablePathMarkerObject> ( );
//		newAvailablePathMarker.PathMarker							= availablePathMarker;
//		newAvailablePathMarker.FunctionTarget						= this.gameObject;
//		newAvailablePathMarker.Index								= mCurrentObjectIndex;
//		newAvailablePathMarker.YOffset								= DividerOffset * 2.0f;
//		newAvailablePathMarker.YSize								= mBrowserObjectOffset;
//		newAvailablePathMarker.Refresh ( );
//		
//		AvailablePathMarkerObjects.Add (newAvailablePathMarker);
//		mCurrentObjectIndex++;
//	}
//	
//	public void 					ClearAvailablePathMarkers ( )
//	{
//		foreach (GUIAvailablePathMarkerObject availableMarker in AvailablePathMarkerObjects)
//		{
//			NGUITools.Destroy (availableMarker.gameObject);
//		}
//		
//		AvailablePathMarkerObjects.Clear ( );
//	}
//		
//	public void 					ClearRouteLegs ( )
//	{
//		foreach (GUIRouteLegBrowserObject routeLegObject in RouteLegObjects)
//		{
//			NGUITools.Destroy (routeLegObject.gameObject);
//		}
//		
//		RouteLegObjects.Clear ( );
//	}
//	
//	protected int					mLastLegObjectCount							= 1000;
//	protected float					mBrowserObjectOffset						= -50.0f;
//	protected int					mCurrentObjectIndex							= 0;
//	protected bool					mAddRouteLegOnly							= false;
//	
//	protected List <GUIRouteLegBrowserObject> 		RouteLegObjects				= new List <GUIRouteLegBrowserObject> ( );
//	protected List <GUIAvailablePathMarkerObject> 	AvailablePathMarkerObjects	= new List <GUIAvailablePathMarkerObject> ( );
}