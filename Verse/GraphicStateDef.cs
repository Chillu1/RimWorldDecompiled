namespace Verse;

public class GraphicStateDef : Def
{
	public GraphicData defaultGraphicData;

	public bool applySkinColorTint;

	public Graphic DefaultGraphic => defaultGraphicData.Graphic;

	public GraphicData DefaultGraphicData => defaultGraphicData;

	public bool TryGetDefaultGraphic(out Graphic graphic)
	{
		graphic = defaultGraphicData?.Graphic;
		return graphic != null;
	}
}
