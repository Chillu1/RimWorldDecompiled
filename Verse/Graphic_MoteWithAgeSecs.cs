using UnityEngine;

namespace Verse;

public class Graphic_MoteWithAgeSecs : Graphic_Mote
{
	protected override bool ForcePropertyBlock => true;

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		Graphic_Mote.propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
		if (thing == null)
		{
			bool valueOrDefault = thingDef?.mote?.realTime == true;
			float valueOrDefault2 = ((float?)Find.TickManager?.TicksGame / 60f).GetValueOrDefault();
			Graphic_Mote.propertyBlock.SetFloat(ShaderPropertyIDs.AgeSecs, valueOrDefault ? Time.realtimeSinceStartup : valueOrDefault2);
			Graphic_Mote.propertyBlock.SetFloat(ShaderPropertyIDs.AgeSecsPausable, valueOrDefault2);
			Graphic_Mote.DrawMote(data, MatSingle, loc, 0f, 0, forcePropertyBlock: true);
		}
		else
		{
			Mote mote = (Mote)thing;
			Graphic_Mote.propertyBlock.SetFloat(ShaderPropertyIDs.AgeSecs, mote.AgeSecs);
			Graphic_Mote.propertyBlock.SetFloat(ShaderPropertyIDs.AgeSecsPausable, mote.AgeSecsPausable);
			Graphic_Mote.propertyBlock.SetFloat(ShaderPropertyIDs.RandomPerObject, Gen.HashCombineInt(mote.spawnTick, mote.DrawPos.GetHashCode()));
			Graphic_Mote.propertyBlock.SetFloat(ShaderPropertyIDs.RandomPerObjectOffsetRandom, Gen.HashCombineInt(mote.spawnTick, mote.offsetRandom));
			DrawMoteInternal(loc, rot, thingDef, thing, 0);
		}
	}

	public override string ToString()
	{
		string[] obj = new string[7]
		{
			"Graphic_MoteWithAgeSecs(path=",
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
