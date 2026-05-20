namespace Verse;

public class PawnRenderNodeProperties_BulbfreakTentacle : PawnRenderNodeProperties_Spastic
{
	public PawnRenderNodeProperties_BulbfreakTentacle()
	{
		rotateFacing = true;
		baseLayer = 10f;
		scaleRange = new FloatRange(0.7f, 1.2f);
		rotationRange = new FloatRange(-25f, 25f);
		durationTicksRange = new IntRange(10, 35);
		nextSpasmTicksRange = new IntRange(0, 20);
	}
}
