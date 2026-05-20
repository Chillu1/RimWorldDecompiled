namespace Verse;

public class PawnRenderNodeProperties_Carried : PawnRenderNodeProperties
{
	public PawnRenderNodeProperties_Carried()
	{
		useGraphic = false;
		baseLayer = 90f;
		drawData = DrawData.NewWithData(new DrawData.RotationalData(Rot4.North, -10f));
	}
}
