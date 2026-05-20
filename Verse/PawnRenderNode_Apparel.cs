using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class PawnRenderNode_Apparel : PawnRenderNode
{
	public bool useHeadMesh;

	public PawnRenderNode_Apparel(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
		useHeadMesh = props.parentTagDef == PawnRenderNodeTagDefOf.ApparelHead;
		meshSet = MeshSetFor(pawn);
	}

	public PawnRenderNode_Apparel(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel)
		: base(pawn, props, tree)
	{
		base.apparel = apparel;
		useHeadMesh = props.parentTagDef == PawnRenderNodeTagDefOf.ApparelHead;
		meshSet = MeshSetFor(pawn);
	}

	public PawnRenderNode_Apparel(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel, bool useHeadMesh)
		: base(pawn, props, tree)
	{
		base.apparel = apparel;
		this.useHeadMesh = useHeadMesh;
		meshSet = MeshSetFor(pawn);
	}

	public override GraphicMeshSet MeshSetFor(Pawn pawn)
	{
		if (apparel == null)
		{
			return base.MeshSetFor(pawn);
		}
		if (base.Props.overrideMeshSize.HasValue)
		{
			return MeshPool.GetMeshSetForSize(base.Props.overrideMeshSize.Value.x, base.Props.overrideMeshSize.Value.y);
		}
		if (useHeadMesh)
		{
			return HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
		}
		return HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn);
	}

	protected override IEnumerable<Graphic> GraphicsFor(Pawn pawn)
	{
		if (ApparelGraphicRecordGetter.TryGetGraphicApparel(apparel, tree.pawn.story.bodyType, pawn.Drawer.renderer.StatueColor.HasValue, out var rec))
		{
			yield return rec.graphic;
		}
	}

	protected override int TexSeedFor(Pawn pawn)
	{
		return base.TexSeedFor(pawn) + apparel.thingIDNumber;
	}
}
