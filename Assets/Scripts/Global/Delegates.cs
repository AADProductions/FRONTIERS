using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Data;

namespace Frontiers
{
		//TODO see about pruning most of these now that we're using actions everywhere
		public delegate bool ActionReceiver <R>(R action,double timeStamp);
		public delegate bool ActionListener(double timeStamp);
		public delegate bool SubscriptionCheck <T>(T subscription,T action);
		public delegate void SubscriptionAdd <T>(ref T subscription,T action);
		public delegate void GUITransitionCallBack(Frontiers.GUI.GUITransition transition);
		public delegate void ChildEditorCallback <R>(R editObject,IGUIChildEditor<R> childEditor);
		public delegate R CreateEditObject <R>();
		public delegate void DeleteEditObject <R>(R editObject);
		public delegate void WIStackListner(WIStack stack);
		public delegate float StatusKeeperGetValue();
		public delegate void ScreenEffectCallBack();
		public delegate float LayerMaskValue(Vector3 samplePoint,Vector3 sampleNormal);
		public delegate void WorldClockCallBack();
		public delegate string WorldNameCleaner(int increment);
		public delegate Transform HudTargetSupplier();
		public delegate bool CheckRequirement(out string failureMessage);
		public delegate void RecoilCallBack();
		namespace World
		{
				public delegate void FireStarterDelegate(Fire fireObject);
				public delegate void PathTaskCallback(PathAvatar path);
				public delegate void WorldItemCallback(WorldItem worlditem);
				public delegate void WIGroupCallback(WIGroup group);
				public delegate void IWIBaseCallback(IWIBase wiBase);
				public delegate void PilgrimCallback(MobileReference start,MobileReference target,PathAvatar path,PathDirection direction);
				//public delegate bool TriggerCallback(Trigger source,TriggerEvent triggerEvent,out string errorMessage);
		}
		namespace Gameplay
		{
				public delegate bool UseSkillDelegate(int flavorIndex);
				public delegate bool UseSkillWithTargetDelegate(GameObject targetObject,int flavorIndex);
		}
}