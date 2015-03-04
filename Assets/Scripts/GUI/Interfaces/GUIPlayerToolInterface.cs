using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers;

namespace Frontiers.GUI {
	public class GUIPlayerToolInterface : MonoBehaviour
	{
		public UISlicedSprite 	SwingCurrent;
		public UISlicedSprite 	SwingMaximum;
		public UILabel			ToolName;
		
		public void								Awake ( )
		{
			DontDestroyOnLoad (this);
		}
			
		public void Update ( )
		{
	//		bool showTension 	= false;
	//		bool showName		= false;
	//		if (Player.Local.Tool.IsEquipped)
	//		{
	//			showName = true;
	//			if (Player.Local.Tool.worlditem.Is <Weapon> ( ) && Player.Local.Tool.worlditem.Get<Weapon> ( ).Style == PlayerToolStyle.Swing)
	//			{
	//				showTension = true;
	//			}
	//		}
	//		
	//		if (showName)
	//		{
	//			string toolNameText = string.Empty;
	//			ToolName.enabled 	= true;
	//			if (Player.Local.Tool.Type == PlayerToolType.PathEditor)
	//			{
	//				if (Player.Local.ToolPathEditor.Mode == PlayerToolPathEditor.EditMode.CreateNewPath)
	//				{
	//					toolNameText = "Place first path marker for " + Player.Local.ToolPathEditor.NewPathName;
	//				}
	//			}
	//			else if (Player.Local.ItemPlacement.PlacementPermitted)
	//			{
	//				if (Player.Local.ItemPlacement.PlacementOnTerrainPossible)
	//				{
	//					if (Player.Local.ItemPlacement.PlacementResultsInDrop)
	//					{
	//						toolNameText = "'E' to Drop";
	//					}
	//					else
	//					{
	//						toolNameText = "'E' to Place on Ground";
	//					}
	//				}
	//				else if (Player.Local.ItemPlacement.PlacementInReceptaclePossible)
	//				{
	//					toolNameText = "'E' to Place on " + Player.Local.ItemPlacement.PlacementPreferredReceptacle.worlditem.StackName;
	//				}
	//			}
	//			else
	//			{
	//				toolNameText = "'F' to Throw";
	//			}
	//			ToolName.text = toolNameText;
	//		}
	//		else
	//		{
	//			ToolName.enabled 	= false;
	//		}
	//		
	//		if (showTension)
	//		{
	//			SwingCurrent.enabled 	= true;
	//			SwingMaximum.enabled 	= true;
	//		}
	//		else
	//		{
	//			SwingCurrent.enabled 	= false;
	//			SwingMaximum.enabled 	= false;			
	//		}
		}
	}
}