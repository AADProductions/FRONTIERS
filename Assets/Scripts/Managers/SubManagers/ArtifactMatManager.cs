using UnityEngine;
using System.Collections;
using Frontiers.World;

namespace Frontiers
{
	public class ArtifactMatManager : MonoBehaviour
	{
		public Material 	ArtifactMatRecent;
		public Material 	ArtifactMatModern;
		public Material 	ArtifactMatOld;
		public Material 	ArtifactMatAntiquated;
		public Material 	ArtifactMatAncient;
		public Material 	ArtifactMatPrehistoric;
		
		public Material		ArtifactMaterial (ArtifactAge age)
		{
			Material artifactMat = ArtifactMatRecent;
			switch (age)
			{
			case ArtifactAge.Recent:
				artifactMat = ArtifactMatRecent;
				break;
			case ArtifactAge.Modern:
				artifactMat = ArtifactMatModern;
				break;
			case ArtifactAge.Old:
				artifactMat = ArtifactMatOld;
				break;
				
			case ArtifactAge.Antiquated:
				artifactMat = ArtifactMatAntiquated;
				break;
				
			case ArtifactAge.Ancient:
				artifactMat = ArtifactMatAncient;
				break;
				
			case ArtifactAge.Prehistoric:
				artifactMat = ArtifactMatPrehistoric;
				break;
				
			default:
				break;
			}
			return artifactMat;
		}
	}
}