using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World.WIScripts
{
		public class FoodStuffContainer : WIScript
		{
				public float CapacityInKG = 1.0f;

				public override void Awake()
				{
						base.Awake();
				}

				public bool IsEmpty {
						get {
								return mContents == null || mContents.worlditem.Is(WIMode.Destroyed);
						}
				}

				public bool IsFilled {
						get {
								return mContents != null && !mContents.worlditem.Is(WIMode.Destroyed);
						}
				}

				public FoodStuff Contents {
						get {
								return mContents;
						}
				}

				public List <string> CanContain = new List <string>();

				public override void PopulateOptionsList(List<WIListOption> options, List <string> message)
				{
						options.Add(new WIListOption("Eat Contents", "Eat"));
				}

				protected FoodStuff mContents;
		}
}