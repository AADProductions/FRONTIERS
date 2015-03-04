using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.World;

namespace Frontiers.World.BaseWIScripts
{
	public class PilgrimStop : WIScript
	{
		public void OnTriggerEnter (Collider other)
		{
			////Debug.Log ("Entered collision with " + other.gameObject.name + " in pilgrim stop!");
			switch (other.gameObject.layer)
			{
			case Globals.LayerNumWorldItemActive:
				//see if it's a character
				BodyPart bodyPart = null;
				if (other.gameObject.HasComponent <BodyPart> (out bodyPart))
				{
					Character 	character 	= null;
					Pilgrim		pilgrim		= null;
					if (bodyPart.Owner.worlditem.Is <Character> (out character)
						&&	bodyPart.Owner.worlditem.Is <Pilgrim> (out pilgrim))
					{
						////Debug.Log ("It's a character! Adding motile stop");
						Location location = worlditem.Get <Location> ( );
						pilgrim.AddPilgrimStop (location);
					}
				}
				break;

			case Globals.LayerNumPlayer:
				////Debug.Log ("Adding pilgrim stop " + name + " to player");
				Player.Local.Surroundings.PilgrimStopVisit (this);
				break;

			default:
				break;
			}
		}
	}
}