using UnityEngine;

namespace Verse;

public class PawnRenderNodeProperties_NociosphereSegment : PawnRenderNodeProperties
{
	[NoTranslate]
	public string maskPath;

	public Vector2 offset;

	public PawnRenderNodeProperties_NociosphereSegment()
	{
		nodeClass = typeof(PawnRenderNode_NociosphereSegment);
		workerClass = typeof(PawnRenderNodeWorker_NociosphereSegment);
		drawSize = new Vector2(1.35f, 1.35f);
	}
}
