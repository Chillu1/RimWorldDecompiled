using Verse;

namespace RimWorld;

public class CompDrawAdditionalGraphics : ThingComp
{
	private CompProperties_DrawAdditionalGraphics Props => (CompProperties_DrawAdditionalGraphics)props;

	public override void PostDraw()
	{
		foreach (GraphicData graphic in Props.graphics)
		{
			graphic.Graphic.Draw(parent.DrawPos, parent.Rotation, parent);
		}
	}
}
