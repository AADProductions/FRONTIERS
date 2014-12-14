using UnityEngine;
using System;
using System.Collections;
using Frontiers;
using Frontiers.World;
using System.Collections.Generic;
using Frontiers.Data;

public class Examinable : WIScript
{
		//used for items that need to have different examine messages than their default
		//eg the museum pieces in the opening classroom scene
		public ExaminableState State = new ExaminableState();

		[Serializable]
		public class ExaminableState
		{
				public string OverrideIdentification = string.Empty;
				public string StaticExamineMessage = "Nothing Unusual.";
				public bool LongFormDisplay = false;
				public bool CenterText = false;
				public List <MobileReference> LocationsToReveal = new List <MobileReference>();
		}
}
