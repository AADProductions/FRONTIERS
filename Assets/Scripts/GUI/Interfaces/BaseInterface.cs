using UnityEngine;
using System.Collections;

namespace Frontiers.GUI
{
		public class BaseInterface : FrontiersInterface
		{
				public override InterfaceType	Type {
						get {
								return InterfaceType.Base;
						}
				}
		}
}
