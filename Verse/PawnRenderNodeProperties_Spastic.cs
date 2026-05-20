namespace Verse;

public class PawnRenderNodeProperties_Spastic : PawnRenderNodeProperties
{
	public bool rotateFacing = true;

	public FloatRange scaleRange = FloatRange.One;

	public FloatRange rotationRange = FloatRange.Zero;

	public FloatRange offsetRangeX = FloatRange.Zero;

	public FloatRange offsetRangeZ = FloatRange.Zero;

	public IntRange durationTicksRange = new IntRange(60, 60);

	public IntRange nextSpasmTicksRange = new IntRange(60, 60);

	public PawnRenderNodeProperties_Spastic()
	{
		nodeClass = typeof(PawnRenderNode_Spastic);
		workerClass = typeof(PawnRenderNodeWorker_Spastic);
	}
}
