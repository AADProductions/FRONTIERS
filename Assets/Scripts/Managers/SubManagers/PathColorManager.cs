using UnityEngine;
using System.Collections;

namespace Frontiers {
	public class PathColorManager : MonoBehaviour {

		public Color InactivePathOutlineColor;
		public Color ActivePathOutlineColor;
		
		public Color	CurrentPathMarkerProjection			= Color.white;
		public Color	WorldRouteMarkerDefault				= Color.white;
		public Color	WorldRouteMarkerDestination			= Color.red;
		public Color	WorldRouteMarkerStart				= Color.green;
		
		public Color	InRoutePathMarkerIcon				= Color.white;
		public Color	AvailablePathMarkerIcon				= Color.white;
		public Color	HighlightPathMarkerIcon				= Color.white;
	}
}