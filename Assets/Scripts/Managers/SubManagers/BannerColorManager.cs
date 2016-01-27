using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers {
//[ExecuteInEditMode]
	public class BannerColorManager : MonoBehaviour
	{
		public List <Color> BannerColors = new List <Color> ();
		public List <Color> ObjectColors = new List <Color> ();
		public List <string> ObjectIconNames = new List <string> ();
		public List <Texture2D> ObjectIcons = new List <Texture2D> ();

		public void Start ( )
		{
			//convert legacy banners
		}

		public Color RandomObjectColor ()
		{
			return ObjectColors [UnityEngine.Random.Range (0, ObjectColors.Count)];
		}

		public Color RandomBannerColor ()
		{
			return BannerColors [UnityEngine.Random.Range (0, BannerColors.Count)];
		}

		public void Refresh ()
		{
			ObjectIconNames.Clear ();
			for (int i = 0; i < ObjectIcons.Count; i++) {
				ObjectIconNames.Add (ObjectIcons [i].name);
			}
			ObjectIconNames.Sort ();

			List <Texture2D> newBannerIcons = new List<Texture2D> (ObjectIcons);
			for (int i = 0; i < ObjectIconNames.Count; i++)
			{
				string bannerIconName = ObjectIconNames [i];
				foreach (Texture2D objectIcon in newBannerIcons) {
					if (objectIcon.name == bannerIconName) {
						ObjectIcons [i] = objectIcon;
					}
				}
			}
		}

		public int GetIconIndex (string iconName)
		{
			int index = 0;
			for (int i = 0; i < ObjectIcons.Count; i++) {
				if (string.Equals (iconName.ToLower ().Trim (), ObjectIcons [i].name.ToLower ().Trim ())) {
					index = i;
					break;
				}
			}
			return index;
		}
	}
}