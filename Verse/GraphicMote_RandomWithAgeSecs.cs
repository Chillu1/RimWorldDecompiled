using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class GraphicMote_RandomWithAgeSecs : Graphic_Random
{
	protected static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

	protected virtual bool ForcePropertyBlock => true;

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		Mote mote = (Mote)thing;
		propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
		propertyBlock.SetFloat(ShaderPropertyIDs.AgeSecs, mote.AgeSecs);
		propertyBlock.SetFloat(ShaderPropertyIDs.AgeSecsPausable, mote.AgeSecsPausable);
		Graphic_Mote.DrawMote(data, SubGraphicFor((Mote)thing).MatSingle, base.Color, loc, rot, thingDef, thing, 0, ForcePropertyBlock, propertyBlock);
	}

	public Graphic SubGraphicFor(Mote mote)
	{
		return subGraphics[mote.offsetRandom % subGraphics.Length];
	}

	public override string ToString()
	{
		string[] obj = new string[7]
		{
			"Mote(path=",
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
