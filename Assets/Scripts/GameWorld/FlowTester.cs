using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.World.Gameplay {
	[ExecuteInEditMode]
	public class FlowTester : MonoBehaviour {

		public List <StatusKeeper>	StatusKeepers 		= new List <StatusKeeper> ( );
		public List <Condition>		Conditions			= new List <Condition> ( );
		public List <Condition>		ActiveConditions	= new List <Condition> ( );
		public float				TimeMultiplier		= 1.0f;
		public string				ApplyConditionName	= string.Empty;

		// Use this for initialization
		void Awake ( )
		{
	//		foreach (StatusKeeper statusKeeper in StatusKeepers)
	//		{
	//			statusKeeper.Initialize ( );
	//			statusKeeper.SetState ("Default");
	//		}
	//		CheckForFlows ( );
		}

		// Update is called once per frame
		void Update ( )
		{
	//		float deltaTime = Time.deltaTime * TimeMultiplier;
	//		foreach (StatusKeeper statusKeeper in StatusKeepers)
	//		{
	//			statusKeeper.UpdateState (deltaTime);
	//			statusKeeper.ApplyConditions (deltaTime, new List <AvatarAction> ( ), new List <Condition> ( ));
	//			statusKeeper.ApplyStatusFlows (deltaTime);
	//		}
		}
		


		public void OnGUI ( )
		{
	//		float offset 		= 0f;
	//		float flowOffset	= 180f;
	//		float size 			= 50f;
	//		float flowSize		= 70f;
	//		Color positiveColor	= Color.green;
	//		Color negativeColor	= Color.red;
	//		Color neutralColor	= Color.white;
	//
	//		UnityEngine.GUI.color = Color.white;
	//		foreach (StatusKeeper statusKeeper in StatusKeepers)
	//		{
	//			Color svc = Color.white;
	//			switch (statusKeeper.ActiveState.SeekType)
	//			{
	//			case StatusSeekType.Neutral:
	//				break;
	//
	//			case StatusSeekType.Positive:
	//			default:
	//				break;
	//
	//			case StatusSeekType.Negative:
	//				break;
	//			}
	//			if (statusKeeper.ActiveState.PositiveChange == StatusSeekType.Positive)
	//			{
	//				svc = Color.Lerp (Color.red, Color.green, statusKeeper.NormalizedValue);
	//			}
	//			else
	//			{
	//				svc = Color.Lerp (Color.green, Color.red, statusKeeper.NormalizedValue);
	//			}
	//			UnityEngine.GUI.color = svc;
	//			if (GUI.Button (new Rect (10f, 10f + offset, 150f, size), (statusKeeper.Name + " : " + statusKeeper.AdjustedValue.ToString ("0.##") + "\n(" + statusKeeper.ActiveState.StateName + ")")))
	//		    {
	//				statusKeeper.Value = statusKeeper.ActiveState.DefaultValue;
	//			}
	//
	//			UnityEngine.GUI.color = Color.white;
	//			foreach (StatusFlow flow in statusKeeper.StatusFlows)
	//			{
	//				bool display 	= true;
	//				Color fc 		= Color.green;
	//				if (flow.FlowType == StatusSeekType.Negative)
	//				{
	//					fc = Color.red;
	//				}
	//				if (!flow.HasEffect)
	//				{
	//					fc = Color.grey;
	//					display = false;
	//				}			
	//				UnityEngine.GUI.color = fc;
	//				if (display)
	//				{
	//					GUI.Button (new Rect (flowOffset, 10f, flowSize, size), flow.SenderName + "\n(" + flow.FlowLastUpdate.ToString ("0.#") + ")");
	//				}
	//				flowOffset += flowSize;
	//			}
	//
	//			UnityEngine.GUI.color = Color.white;
	//			foreach (Condition condition in statusKeeper.Conditions)
	//			{
	//				Symptom symptom = null;
	//				if (condition.HasSymptomFor (statusKeeper.Name, out symptom))
	//				{
	//					Color cc = Color.green;
	//					if (symptom.SeekType == StatusSeekType.Negative)
	//					{
	//						cc = Color.red;
	//					}
	//					UnityEngine.GUI.color = cc;
	//					GUI.Button (new Rect (flowOffset, 10f, flowSize, size), condition.Name + "\n(" + condition.NormalizedTimeLeft.ToString ("0.#") + ")");
	//					flowOffset += flowSize;
	//				}
	//			}
	//
	//			UnityEngine.GUI.color = Color.red;
	//			if (GUI.Button (new Rect (800f, 10f + offset, 100f, size), "Neg 0.25"))
	//			{
	//				statusKeeper.Value -= 0.25f;
	//			}
	//			UnityEngine.GUI.color = Color.green;
	//			if (GUI.Button (new Rect (900f, 10f + offset, 100f, size), "Pos by 0.25"))
	//			{
	//				statusKeeper.Value += 0.25f;
	//			}
	//			offset += size;
	//		}
	//
	//		UnityEngine.GUI.color = Color.gray;
	//		ApplyConditionName = GUI.TextField (new Rect (1010f, 10f + offset, 200f, 35f), ApplyConditionName);
	//		if (GUI.Button (new Rect (1300f, 10f + offset, 100f, size), "Apply"))
	//		{
	//			//Debug.Log ("Clicking apply button...");
	////			ApplyCondition (ApplyConditionName);
	//		}
	//
	//		UnityEngine.GUI.color = Color.gray;
	//		if (GUI.Button (new Rect (10f, 600f, 150f, 60f), "Set Default State"))
	//		{
	//			foreach (StatusKeeper statusKeeper in StatusKeepers)
	//			{
	//				statusKeeper.SetState ("Default");
	//			}
	////			CheckForFlows ( );
	//		}
	//
	//		if (GUI.Button (new Rect (200f, 600f, 250f, 60f), "Set Civ State"))
	//		{
	//			foreach (StatusKeeper statusKeeper in StatusKeepers)
	//			{
	//				statusKeeper.SetState ("Civilization");
	//			}
	////			CheckForFlows ( );
	//		}
		}

	}
}