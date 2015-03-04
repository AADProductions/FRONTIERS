using UnityEngine;
using System.Collections;
using Frontiers;
using System;
using System.Text;

namespace Frontiers.World
{
	public class HeadstoneAvatar : WIScript
	{
		public HeadstoneAvatarState State = new HeadstoneAvatarState ( );
		public Examinable examinable = null;

		public Headstone Props = null;
		public bool HasProps {
			get {
				return Props != null;
			}
		}

		protected static WIExamineInfo gNoPropsInfo;// = new WIExamineInfo ("The engraving has worn away");
		protected static WIExamineInfo gHeadstoneExamineInfo;// = new WIExamineInfo ();

		public override void OnInitialized ()
		{
			if (gNoPropsInfo == null) {
				gNoPropsInfo = new WIExamineInfo ("The engraving has worn away");
				gHeadstoneExamineInfo = new WIExamineInfo ();
			}

			examinable = worlditem.GetOrAdd <Examinable> ();
			RefreshProps ();
		}

		public void RefreshProps ( )
		{
			examinable.State.OverrideIdentification = "A Grave";
			examinable.State.CenterText = true;

			if (string.IsNullOrEmpty (State.HeadstoneName)) {
				return;
			}
			else if (State.HeadstoneName == "[Random]") {
				examinable.State.StaticExamineMessage = "The engraving has worn away";
				worlditem.State = HeadstoneStyle.Headstone.ToString ( );//TODO randomize type
			} else {
				Mods.Get.Runtime.LoadMod <Headstone> (ref Props, "Headstone", State.HeadstoneName);
				if (HasProps) {
					//TEMP to test styles
					float randomVal = UnityEngine.Random.value;
					if (randomVal < 0.25f) {
						Props.Style = HeadstoneStyle.Marker;
					} else if (randomVal < 0.5f) {
						Props.Style = HeadstoneStyle.Headstone;
					} else if (randomVal < 0.5f) {
						Props.Style = HeadstoneStyle.StoneCross;
					} else {
						Props.Style = HeadstoneStyle.WoodCross;
					}

					worlditem.State = Props.Style.ToString ();
					examinable.State.LongFormDisplay = true;
					StringBuilder sb = new StringBuilder ();
					sb.Append (Props.Line1);
					sb.Append ("\n");
					sb.Append (Props.Line2);
					sb.Append ("\n");
					sb.Append (Props.Line3);
					sb.Append ("\n");
					sb.Append (Props.Line4);
					sb.Append ("\n_\n");
					sb.Append (Props.Epitaph);
					examinable.State.StaticExamineMessage = sb.ToString ();
				} else {
					examinable.State.StaticExamineMessage = "The engraving has worn away";
				}
			}
		}

		#if UNITY_EDITOR
		public override void OnEditorRefresh ()
		{
			
		}
		#endif

		public override bool CanBeCarried
		{
			get
			{
				return false;
			}
		}
		
		public override bool CanEnterInventory
		{
			get
			{
				return false;
			}
		}	
	}

	[Serializable]
	public class HeadstoneAvatarState {
		public string HeadstoneName = string.Empty;
	}

	[Serializable]
	public class Headstone : Mod {
		public HeadstoneStyle Style = HeadstoneStyle.Headstone;
		public string Line1;
		public string Line2;
		public string Line3;
		public string Line4;
		public string Epitaph;
	}
}