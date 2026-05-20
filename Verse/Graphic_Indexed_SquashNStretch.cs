using System;
using UnityEngine;

namespace Verse;

public class Graphic_Indexed_SquashNStretch : Graphic_Indexed
{
	private Vector4 snsProps;

	protected override Type SingleGraphicType => typeof(Graphic_Single_SquashNStretch);

	public float AgeSecs(Thing thing)
	{
		return (float)(Find.TickManager.TicksGame - thing.TickSpawned) / 60f;
	}

	public override void Init(GraphicRequest req)
	{
		base.Init(req);
		snsProps = new Vector4(data.maxSnS.x, data.maxSnS.y, data.offsetSnS.x, data.offsetSnS.y);
	}

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		Graphic_Single_SquashNStretch graphic_Single_SquashNStretch = ((thing == null) ? (subGraphics[0] as Graphic_Single_SquashNStretch) : (SubGraphicFor(thing) as Graphic_Single_SquashNStretch));
		graphic_Single_SquashNStretch.propertyBlock.SetVector(ShaderPropertyIDs.SquashNStretch, snsProps);
		graphic_Single_SquashNStretch.propertyBlock.SetFloat(ShaderPropertyIDs.RandomPerObject, thing.thingIDNumber.HashOffset());
		graphic_Single_SquashNStretch.MatSingle.SetFloat(ShaderPropertyIDs.AgeSecs, AgeSecs(thing));
		graphic_Single_SquashNStretch.DrawWorker(loc, rot, thingDef, thing, extraRotation);
		if (base.ShadowGraphic != null)
		{
			base.ShadowGraphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
		}
	}
}
