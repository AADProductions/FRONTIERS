using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
	public class DungeonTransition : MonoBehaviour {

		public TriggerDungeonTransition OuterTrigger;
		public TriggerDungeonTransition InnerTrigger;
		public Transform TriggerParent;
		public Dungeon ParentDungeon;
		public CullingGroup_Manual OcclusionGroup;
		public int TransitionNumber = 0;

		public void Initialize ( )
		{
			name = Dungeon.TransitionName (ParentDungeon.name, TransitionNumber);
			OcclusionGroup.cullingGroupMasterName = name;
			TriggerParent = gameObject.FindOrCreateChild ("Triggers");
		}

		public void Refresh ( )
		{

		}

		public void BuildTransition (StructureTemplateGroup dungeonTransitionGroup, WIGroup dungeonGroup)
		{
			//then add the children
			for (int j = 0; j < dungeonTransitionGroup.StaticStructureLayers.Count; j++) {
				StructureTemplate.InstantiateStructureLayer (dungeonTransitionGroup.StaticStructureLayers [j], transform);
			}
			//create and find triggers
			StructureTemplate.InstantiateGenericDynamic (dungeonTransitionGroup.GenericDynamic, TriggerParent, dungeonGroup);
		}

		public void FindTriggers () 
		{
			OuterTrigger = TriggerParent.gameObject.FindOrCreateChild ("DungeonTriggerOuter").gameObject.GetOrAdd <TriggerDungeonTransition> ();
			InnerTrigger = TriggerParent.gameObject.FindOrCreateChild ("DungeonTriggerInner").gameObject.GetOrAdd <TriggerDungeonTransition> ();
		}
	}
}
