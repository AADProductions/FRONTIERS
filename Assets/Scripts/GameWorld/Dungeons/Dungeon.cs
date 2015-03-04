using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
	[ExecuteInEditMode]
	public class Dungeon : MonoBehaviour
	{
		public WIFlags ResidentFlags = new WIFlags ();
		public WorldChunk ParentChunk;
		public WIGroup DungeonGroup;
		public bool HasDungeonGroup
		{
			get {
				return DungeonGroup != null; 
			}
		}
		public List <DungeonModule> Modules = new List <DungeonModule> ();
		public List <DungeonTransition> Transitions = new List <DungeonTransition> ();
		public List <CullingArea_Manual> ModuleOcclusionAreas = new List<CullingArea_Manual> ( );
		public List <CullingArea_Manual> TransitionOcclusionAreas = new List<CullingArea_Manual> ( );
		public DungeonTemplate Template = null;
		public Transform ModuleParent;
		public Transform OcclusionAreaParent;
		public Transform TransitionParent;

		public void OnEnterDungeon ()
		{
			////Debug.Log ("DUNGEONBUILDER: Entering dungeon");
			DungeonGroup.Load ();
			CreateDungeonModules ();
			LinkOcclusionGroups ();
		}

		public void OnExitDungeon ()
		{
			////Debug.Log ("DUNGEONBUILDER: Exiting dungeon");
			for (int i = 0; i < Modules.Count; i++) {
				if (Modules [i].ModuleGroup != null) { 
					Modules [i].ModuleGroup.Unload ();
				}
			}
			DungeonGroup.Unload ();
		}

		public void OnEnterTransition ()
		{
			////Debug.Log ("DUNGEONBUILDER: Entering transition - leaving deeper dungeon");
			//this means we're on our way outside
			GameWorld.Get.ShowAboveGround (true);
			Ocean.Get.SetMode (OceanMode.Default);
			Player.Local.Surroundings.ExitUnderground ();
		}

		public void OnExitTransition ()
		{
			////Debug.Log ("DUNGEONBUILDER: Exiting transition - entering deeper dungeon");
			//this means we're on our way into the dungeon
			GameWorld.Get.ShowAboveGround (false);
			Ocean.Get.SetMode (OceanMode.Disabled);
			Player.Local.Surroundings.EnterUnderground ();
		}

		public void OnEntranceVisible ( )
		{
			////Debug.Log ("On entrance visible");
			if (Template == null) {
				//load template
				Mods.Get.Runtime.LoadMod <DungeonTemplate> (ref Template, "Dungeon", GetTemplateName (gameObject.name));
			}

			if (Transitions.Count == 0) {
				BuildDungeonExterior ();
			}
			DungeonGroup.Load ();
			for (int i = 0; i < Modules.Count; i++) {
				if (Modules [i].ModuleGroup != null) { 
					Modules [i].ModuleGroup.Load ();
				}
			}
		}

		public void BuildDungeonExterior ()
		{
			////Debug.Log ("DUNGEONBUILDER: Building dungeon");
			CreateDungeonGroup ();
			CreateDungeonTransforms ();
			CreateDungeonTransitions ();
			//save the rest for when we actually enter
		}

		public void CreateDungeonModules ()
		{
			for (int i = 0; i < Template.Modules.InteriorVariants.Count; i++) {
				//first create the object
				StructureTemplateGroup dungeonModuleGroup = Template.Modules.InteriorVariants [i];
				DungeonModule dungeonModule = CreateDungeonModule (i);
				//move the group into position
				dungeonModule.transform.localPosition = dungeonModuleGroup.GroupOffset;
				dungeonModule.transform.localRotation = Quaternion.Euler (dungeonModuleGroup.GroupRotation);
				dungeonModule.Refresh ();//move the group into the final position
				dungeonModule.BuildModule (dungeonModuleGroup, Template.HasLoadedModulesOnce);
			}
			Template.HasLoadedModulesOnce = true;
		}

		public void CreateDungeonTransitions ()
		{
			if (Template == null) {
				Debug.Log ("TEMPLATE WAS NULL");
				return;
			}

			for (int i = 0; i < Template.Transitions.InteriorVariants.Count; i++) {
				//first create the object
				StructureTemplateGroup dungeonTransitionGroup = Template.Transitions.InteriorVariants [i];
				DungeonTransition dungeonTransition = CreateDungeonTransition (i);
				dungeonTransition.BuildTransition (dungeonTransitionGroup, DungeonGroup);
				dungeonTransition.FindTriggers ();
				//move the group into position
				dungeonTransition.OuterTrigger.OnEnter += OnEnterDungeon;
				dungeonTransition.OuterTrigger.OnExit += OnExitDungeon;
				dungeonTransition.InnerTrigger.OnExit += OnExitTransition;
				dungeonTransition.InnerTrigger.OnEnter += OnEnterTransition;
				dungeonTransition.transform.localPosition = dungeonTransitionGroup.GroupOffset;
				dungeonTransition.transform.localRotation = Quaternion.Euler (dungeonTransitionGroup.GroupRotation);
			}
		}

		public void CreateDungeonGroup ()
		{
			if (!HasDungeonGroup) {
				//create a dungeon group in the chunk's WI/BG group
				WIGroup belowGroundWorldItems = ParentChunk.Transforms.BelowGroundWorldItems.GetComponent <WIGroup> ();
				DungeonGroup = WIGroups.GetOrAdd (name, belowGroundWorldItems, null);
				DungeonGroup.Load ();
			}
		}

		public void CreateDungeonTransforms ( )
		{
			ModuleParent = gameObject.FindOrCreateChild ("Modules");
			TransitionParent = gameObject.FindOrCreateChild ("Transitions");
			OcclusionAreaParent = gameObject.FindOrCreateChild ("OcclusionAreas");
		}

		public void LinkOcclusionGroups ( )
		{	//first make sure all the occlusion areas are created
			ModuleOcclusionAreas.Clear ();
			TransitionOcclusionAreas.Clear ();
			for (int i = 0; i < Modules.Count; i++) {
				ModuleOcclusionAreas.Add (CreateOcclusionArea (Modules [i].transform));
			}
			for (int i = 0; i < Transitions.Count; i++) {
				TransitionOcclusionAreas.Add (CreateOcclusionArea (Transitions [i].transform));
			}
			//now create links for every module
			for (int i = 0; i < Modules.Count; i++) {
				CullingArea_Manual occlusionArea = ModuleOcclusionAreas [i];
				for (int j = 0; j < Modules.Count; j++) {
					CullingAreaGroupSettings groupSettings = new CullingAreaGroupSettings ();
					groupSettings.script = Modules [j].OcclusionGroup;
					if (i == j) {
						//for now, only show our own group
						groupSettings.cullingOptions = CullingOptions.Show;
					} else {
						groupSettings.cullingOptions = CullingOptions.AlwaysHide;
					}
					occlusionArea.groupsList.Add (groupSettings);
				}
				//now add the transitions = these are always on
				for (int k = 0; k < Transitions.Count; k++) {
					CullingAreaGroupSettings groupSettings = new CullingAreaGroupSettings ();
					groupSettings.script = Transitions [k].OcclusionGroup;
					groupSettings.cullingOptions = CullingOptions.Show;
					occlusionArea.groupsList.Add (groupSettings);
				}
			}
			//finally create links for every transition
			//transitions are always visible from all groups
			for (int i = 0; i < TransitionOcclusionAreas.Count; i++) {
				CullingArea_Manual occlusionArea = TransitionOcclusionAreas [i];
				for (int j = 0; j < Modules.Count; j++) {
					CullingAreaGroupSettings groupSettings = new CullingAreaGroupSettings ();
					groupSettings.script = Modules [j].OcclusionGroup;
					groupSettings.cullingOptions = CullingOptions.AlwaysHide;
					occlusionArea.groupsList.Add (groupSettings);
				}
				for (int k = 0; k < Transitions.Count; k++) {
					CullingAreaGroupSettings groupSettings = new CullingAreaGroupSettings ();
					groupSettings.script = Transitions [k].OcclusionGroup;
					groupSettings.cullingOptions = CullingOptions.Show;
					occlusionArea.groupsList.Add (groupSettings);
				}
			}
		}

		protected DungeonTransition CreateDungeonTransition (int transitionNum)
		{
			//first create the object
			Transform transitionTransform = TransitionParent.gameObject.FindOrCreateChild (TransitionName (name, transitionNum));
			DungeonTransition dungeonTransition = transitionTransform.gameObject.AddComponent <DungeonTransition> ();
			dungeonTransition.ParentDungeon = this;
			dungeonTransition.TransitionNumber = transitionNum;
			dungeonTransition.OcclusionGroup = dungeonTransition.gameObject.AddComponent <CullingGroup_Manual> ();
			dungeonTransition.Initialize ();
			Transitions.Add (dungeonTransition);
			return dungeonTransition;
		}

		protected DungeonModule CreateDungeonModule (int moduleNum)
		{
			//first create the object
			Transform moduleTransform = ModuleParent.gameObject.FindOrCreateChild (ModuleName (name, moduleNum));
			DungeonModule dungeonModule = moduleTransform.gameObject.AddComponent <DungeonModule> ();
			dungeonModule.ParentDungeon = this;
			dungeonModule.ModuleNumber = moduleNum;
			dungeonModule.OcclusionGroup = dungeonModule.gameObject.AddComponent <CullingGroup_Manual> ();
			dungeonModule.Initialize ();
			Modules.Add (dungeonModule);
			return dungeonModule;
		}

		protected CullingArea_Manual CreateOcclusionArea (Transform rendererChild)
		{
			Transform occlusionChild = OcclusionAreaParent.gameObject.FindOrCreateChild (rendererChild.name);
			occlusionChild.transform.rotation = Quaternion.identity;//occlusion bounds don't respect rotation
			CullingArea_Manual occlusionArea = occlusionChild.gameObject.GetOrAdd <CullingArea_Manual> ();
			//resize the occlusion area to fit
			Renderer[] occlusionAreaRenderers = rendererChild.GetComponentsInChildren <Renderer> (true);
			Bounds combinedBounds = new Bounds (rendererChild.position, Vector3.one);
			foreach (Renderer occlusionAreaRenderer in occlusionAreaRenderers) {
				combinedBounds.Encapsulate (occlusionAreaRenderer.bounds);
			}
			occlusionArea.transform.position = combinedBounds.center;
			occlusionArea.transform.localScale = combinedBounds.size;
			occlusionArea.gizmoColor = Colors.RandomColor (0.25f);
			return occlusionArea;
		}

		#region Editor-only helper functions
		public void EditorFindDungeonPieces ()
		{
			Modules.Clear ();
			Transitions.Clear ();

			int moduleNumber = 0;
			foreach (Transform module in ModuleParent) {
				DungeonModule dungeonModule = module.gameObject.GetOrAdd <DungeonModule> ();
				dungeonModule.ModuleNumber = moduleNumber;
				dungeonModule.ParentDungeon = this;
				dungeonModule.OcclusionGroup = dungeonModule.gameObject.GetOrAdd <CullingGroup_Manual> ();
				dungeonModule.Initialize ();
				Modules.Add (dungeonModule);
				moduleNumber++;
			}

			int transitionNumber = 0;
			foreach (Transform transition in TransitionParent) {
				DungeonTransition dungeonTransition = transition.gameObject.GetOrAdd <DungeonTransition> ();
				dungeonTransition.ParentDungeon = this;
				dungeonTransition.TransitionNumber = transitionNumber;
				dungeonTransition.OcclusionGroup = dungeonTransition.gameObject.GetOrAdd <CullingGroup_Manual> ();
				dungeonTransition.Initialize ();
				dungeonTransition.FindTriggers ();
				Transitions.Add (dungeonTransition);
				transitionNumber++;
			}
		}

		public void EditorSaveDungeonToTemplate ( )
		{
			if (!Manager.IsAwake <WorldItems> ()) {
				Manager.WakeUp <WorldItems> ("Frontiers_WorldItems");
			}
			if (!Manager.IsAwake <Structures> ()) {
				Manager.WakeUp <Structures> ("Frontiers_Structures");
			}
			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
			}
			Mods.Get.Editor.InitializeEditor (true);

			//make sure the dungeon is all set up
			CreateDungeonTransforms ();
			EditorFindDungeonPieces ();
			LinkOcclusionGroups ();

			DungeonTemplate newTemplate = new DungeonTemplate ();
			for (int i = 0; i < Modules.Count; i++) {
				DungeonModule module = Modules [i];
				StructureTemplateGroup moduleInteriorGroup = new StructureTemplateGroup ();
				moduleInteriorGroup.GroupOffset = module.transform.localPosition;
				moduleInteriorGroup.GroupRotation = module.transform.localRotation.eulerAngles;
				module.EditorSaveModuleToTemplate (moduleInteriorGroup);
				newTemplate.Modules.InteriorVariants.Add (moduleInteriorGroup);
			}

			for (int i = 0; i < Transitions.Count; i++) {
				DungeonTransition transition = Transitions [i];
				StructureTemplateGroup transitionInteriorGroup = new StructureTemplateGroup ();
				transitionInteriorGroup.GroupOffset = transition.transform.localPosition;
				transitionInteriorGroup.GroupRotation = transition.transform.localRotation.eulerAngles;
				StructureTemplate.AddStaticChildrenToStructureTemplate (transition.transform, transitionInteriorGroup.StaticStructureLayers);
				StructureTemplate.AddGenericDynamicToStructureTemplate (transition.TriggerParent, ref transitionInteriorGroup.GenericDynamic);
				newTemplate.Transitions.InteriorVariants.Add (transitionInteriorGroup);
			}

			Mods.Get.Editor.SaveMod <DungeonTemplate> (newTemplate, "Dungeon", GetTemplateName (gameObject.name));
		}
		#endregion

		public static string TransitionName (string dungeonName, int transitionNumber)
		{
			return dungeonName + "-Transition" + transitionNumber.ToString ();
		}

		public static string ModuleName (string dungeonName, int moduleNumber)
		{
			return dungeonName + "-Module" + moduleNumber.ToString ();
		}

		public static string GetTemplateName (string rootName)
		{
			string[] templateName = rootName.Split ('-');
			return templateName [0];
		}
	}

	[Serializable]
	public class DungeonTemplate : Mod
	{
		public bool HasLoadedModulesOnce = false;
		public StructureTemplate Modules = new StructureTemplate ();
		public StructureTemplate Transitions = new StructureTemplate ();
	}
}