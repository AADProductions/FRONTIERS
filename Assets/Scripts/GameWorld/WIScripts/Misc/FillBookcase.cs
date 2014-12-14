using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Frontiers.World
{
		public class FillBookcase : WIScript
		{
				public FillBookcaseState State = new FillBookcaseState();

				public override void OnInitialized()
				{
						StartCoroutine(TryToFillBookcase());
				}

				protected IEnumerator TryToFillBookcase()
				{
						if (State.HasBeenFilled)
								yield break;

						State.HasBeenFilled = true;

						while (worlditem.Group == null) {
								//wait for on initialized to finish working
								yield return null;
						}

						WIStackContainer stackContainer = worlditem.StackContainer;
						List <StackItem> avatarStackItems = new List<StackItem>();

						switch (State.FillMethod) {
								case ContainerFillMethod.AllRandomItemsFromCategory:
								default:
										yield return StartCoroutine(Books.Get.BookStackItemsByFlagsAndOwner(stackContainer.NumStacks, State.Flags, State.ManualSkillLevel, worlditem, true, avatarStackItems));
										break;

								case ContainerFillMethod.SpecificItems:
										Books.Get.BookStackItemsByName(State.SpecificBooks, avatarStackItems);
										break;
						}

						WIStackError error = WIStackError.None;
						for (int i = 0; i < avatarStackItems.Count; i++) {
								if (!Stacks.Push.Item(stackContainer, avatarStackItems[i], ref error)) {
										break;
								}
								yield return null;
						}
						yield return null;
						//wait for one last bit, then refresh our recepticle
						worlditem.Get <Receptacle>().Refresh();
						yield break;
				}
		}

		[Serializable]
		public class FillBookcaseState
		{
				public bool HasBeenFilled = false;
				public int ManualSkillLevel = -1;
				public BookFlags Flags = new BookFlags();
				public ContainerFillMethod FillMethod = ContainerFillMethod.AllRandomItemsFromCategory;
				public List <string> SpecificBooks = new List <string>();
		}
}