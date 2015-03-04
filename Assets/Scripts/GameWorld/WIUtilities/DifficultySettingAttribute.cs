using UnityEngine;
using System.Collections;

//flags a color as interface related
public class InterfaceColorAttribute : System.Attribute {

}

//flags a global variable as editiable by difficulty settings
public class EditableDifficultySettingAttribute : System.Attribute {

		public EditableDifficultySettingAttribute () {
				//we don't show this option to the player
				Type = SettingType.Hidden;
				Editable = false;
		}

		public EditableDifficultySettingAttribute (SettingType t, bool editable, string desc) {
				//we don't show this option to the player
				Type = t;
				Editable = editable;
				desc = desc;
		}

		public EditableDifficultySettingAttribute (float min, float max, string desc) {
				Description = desc;
				MinRangeFloat = min;
				MaxRangeFloat = max;
				Type = SettingType.FloatRange;

		}

		public EditableDifficultySettingAttribute (int min, int max, string desc) {
				Description = desc;
				MinRangeInt = min;
				MaxRangeInt = max;
				Type = SettingType.IntRange;
		}

		public EditableDifficultySettingAttribute (string desc) {
				Description = desc;
				Type = SettingType.String;
		}

		public bool Editable;
		public string Description;
		public int MinRangeInt;
		public int MaxRangeInt;
		public float MinRangeFloat;
		public float MaxRangeFloat;
		public SettingType Type = SettingType.IntRange;

		public enum SettingType {
				FloatRange,
				IntRange,
				String,
				Bool,
				Hidden,
		}
}

//flags a global variable as editiable by profile settings
public class ProfileSettingAttribute : System.Attribute {

}

//flags a global variable as a purely visual setting
public class VisualSettingAttribute : System.Attribute {

}

//flags a global variable as editable by the world
public class WorldSettingAttribute : System.Attribute {

}
