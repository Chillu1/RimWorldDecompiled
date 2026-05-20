using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class PawnRenderNodeProperties_Eye : PawnRenderNodeProperties
{
	public PawnRenderNodeProperties_Eye()
	{
		visibleFacing = new List<Rot4>
		{
			Rot4.East,
			Rot4.South,
			Rot4.West
		};
		workerClass = typeof(PawnRenderNodeWorker_Eye);
		nodeClass = typeof(PawnRenderNode_AttachmentHead);
	}

	public override void ResolveReferences()
	{
		skipFlag = RenderSkipFlagDefOf.Eyes;
	}
}
