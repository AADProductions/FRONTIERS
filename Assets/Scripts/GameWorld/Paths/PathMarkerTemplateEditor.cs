using UnityEngine;
using System.Collections;
using Frontiers.World;
using Frontiers;
using System.Collections.Generic;

public class PathMarkerTemplateEditor : MonoBehaviour
{
		//used in the editor
		public List <PathEditor> PathsUsingNode = new List <PathEditor>();
		public PathMarkerInstanceTemplate Template = new PathMarkerInstanceTemplate();
}
