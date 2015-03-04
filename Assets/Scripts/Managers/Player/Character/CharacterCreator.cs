using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers
{
		//used to gather information about a character
		//when the information is all present and a confirmation is sent
		//a new character is created in the player's profile
		public class CharacterCreator
		{
				public PlayerCharacter Character = null;
				public bool Confirmed = false;
				public bool Cancelled = false;

				public void StartEditing(PlayerCharacter characterToEdit)
				{
						Character = characterToEdit;
				}

				public bool Confirm(out string errorMessage)
				{
						Confirmed = true;
						errorMessage = string.Empty;
						if (string.IsNullOrEmpty(Character.FirstName)
						 || Character.Age < 0
						 || Character.Gender == CharacterGender.None
						 || Character.Ethnicity == CharacterEthnicity.None
						 || Character.HairColor == CharacterHairColor.None
						 || Character.EyeColor == CharacterEyeColor.None
						 || Character.HairLength == CharacterHairLength.None) {
								errorMessage = "You haven't finished your character yet.";
								Debug.Log(Character.FirstName);
								Debug.Log(Character.Age);
								Debug.Log(Character.Ethnicity);
								Debug.Log(Character.HairColor);
								Debug.Log(Character.EyeColor);
								Debug.Log(Character.HairLength);
								Confirmed = false;
						}

						if (Confirmed) {
								Character.OnCreated();
						}

						return Confirmed;
				}

				public bool SetGender(CharacterGender newGender)
				{
						Character.Gender = newGender;
						return true;
				}

				public bool SetName(string newName)
				{
						Character.FirstName = newName;
						return true;
				}

				public bool SetAge(int newAge)
				{
						Character.Age = newAge;
						return true;			
				}

				public bool SetHairColor(CharacterHairColor newHairColor)
				{
						Character.HairColor = newHairColor;
						return true;
				}

				public bool SetHairLength(CharacterHairLength newHairLength)
				{
						Character.HairLength = newHairLength;
						return true;
				}

				public bool SetEthnicity(CharacterEthnicity newEthnicity)
				{
						Character.Ethnicity = newEthnicity;
						return true;
				}

				public bool SetEyeColor(CharacterEyeColor newEyeColor)
				{
						Character.EyeColor = newEyeColor;
						return true;
				}
		}
}