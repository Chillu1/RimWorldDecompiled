using UnityEngine;

namespace Verse;

public class PawnRenderNode_Spastic : PawnRenderNode
{
	public class SpasmData
	{
		public float rotationStart;

		public float rotationTarget;

		public float scaleStart;

		public float scaleTarget;

		public Vector3 offsetStart;

		public Vector3 offsetTarget;

		public int tickStart;

		public int nextSpasm;

		public float duration;

		public SpasmData()
		{
			duration = 1f;
			scaleStart = (scaleTarget = 1f);
		}
	}

	protected SpasmData spasmData;

	public PawnRenderNode_Spastic(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override GraphicMeshSet MeshSetFor(Pawn pawn)
	{
		return new GraphicMeshSet(MeshPool.GridPlane(props.overrideMeshSize ?? props.drawSize));
	}

	public bool CheckAndDoSpasm(PawnDrawParms parms, out SpasmData dat, out float progress)
	{
		if (parms.pawn.Dead || !(props is PawnRenderNodeProperties_Spastic pawnRenderNodeProperties_Spastic) || parms.Portrait || parms.Cache || parms.Statue)
		{
			progress = 0f;
			dat = null;
			return false;
		}
		if (spasmData == null)
		{
			spasmData = new SpasmData();
		}
		if (Find.TickManager.TicksGame >= spasmData.nextSpasm)
		{
			spasmData.tickStart = Find.TickManager.TicksGame;
			spasmData.duration = GetNextSpasmDurationTicks();
			spasmData.nextSpasm = GetNextSpasmTick();
			spasmData.rotationStart = spasmData.rotationTarget;
			spasmData.rotationTarget = pawnRenderNodeProperties_Spastic.rotationRange.RandomInRange;
			spasmData.scaleStart = spasmData.scaleTarget;
			spasmData.scaleTarget = pawnRenderNodeProperties_Spastic.scaleRange.RandomInRange;
			spasmData.offsetStart = spasmData.offsetTarget;
			spasmData.offsetTarget = new Vector3(pawnRenderNodeProperties_Spastic.offsetRangeX.RandomInRange, 0f, pawnRenderNodeProperties_Spastic.offsetRangeZ.RandomInRange);
		}
		progress = (float)(Find.TickManager.TicksGame - spasmData.tickStart) / Mathf.Max(spasmData.duration, 0.0001f);
		dat = spasmData;
		return true;
	}

	protected virtual int GetNextSpasmTick()
	{
		if (props is PawnRenderNodeProperties_Spastic pawnRenderNodeProperties_Spastic)
		{
			return spasmData.tickStart + (int)spasmData.duration + pawnRenderNodeProperties_Spastic.nextSpasmTicksRange.RandomInRange;
		}
		return 0;
	}

	protected virtual int GetNextSpasmDurationTicks()
	{
		if (props is PawnRenderNodeProperties_Spastic pawnRenderNodeProperties_Spastic)
		{
			return pawnRenderNodeProperties_Spastic.durationTicksRange.RandomInRange;
		}
		return 0;
	}
}
