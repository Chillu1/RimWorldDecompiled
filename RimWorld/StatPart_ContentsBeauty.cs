namespace RimWorld;

public class StatPart_ContentsBeauty : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.Thing is IBeautyContainer beautyContainer)
		{
			val += beautyContainer.BeautyOffset;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.Thing is IBeautyContainer beautyContainer)
		{
			return beautyContainer.BeautyOffsetExplanation;
		}
		return null;
	}
}
