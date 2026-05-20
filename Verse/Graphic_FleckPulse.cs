using UnityEngine;

namespace Verse;

public class Graphic_FleckPulse : Graphic_Fleck
{
	protected override bool AllowInstancing => false;

	public override void DrawFleck(FleckDrawData drawData, DrawBatch batch)
	{
		drawData.propertyBlock = drawData.propertyBlock ?? batch.GetPropertyBlock();
		drawData.propertyBlock.SetFloat(ShaderPropertyIDs.AgeSecs, drawData.ageSecs);
		drawData.propertyBlock.SetFloat(ShaderPropertyIDs.RandomPerObject, drawData.id);
		base.DrawFleck(drawData, batch);
	}

	public override string ToString()
	{
		string[] obj = new string[7]
		{
			"Graphic_FleckPulse(path=",
			path,
			", shader=",
			base.Shader?.ToString(),
			", color=",
			null,
			null
		};
		Color color = base.color;
		obj[5] = color.ToString();
		obj[6] = ", colorTwo=unsupported)";
		return string.Concat(obj);
	}
}
