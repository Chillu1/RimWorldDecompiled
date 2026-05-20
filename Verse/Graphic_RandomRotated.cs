using UnityEngine;

namespace Verse;

public class Graphic_RandomRotated : Graphic
{
	private Graphic subGraphic;

	private float maxAngle;

	public override Material MatSingle => subGraphic.MatSingle;

	public Graphic SubGraphic => subGraphic;

	public Graphic_RandomRotated(Graphic subGraphic, float maxAngle)
	{
		this.subGraphic = subGraphic;
		this.maxAngle = maxAngle;
		drawSize = subGraphic.drawSize;
	}

	public override Material MatAt(Rot4 rot, Thing thing = null)
	{
		return subGraphic.MatAt(rot, thing);
	}

	public override Material MatSingleFor(Thing thing)
	{
		return subGraphic.MatSingleFor(thing);
	}

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		Mesh mesh = MeshAt(rot);
		float num = 0f;
		float? rotInRack = GetRotInRack(thing, thingDef, loc.ToIntVec3());
		if (rotInRack.HasValue)
		{
			num = rotInRack.Value;
		}
		else if (thing != null)
		{
			num = 0f - maxAngle + (float)(thing.thingIDNumber * 542) % (maxAngle * 2f);
		}
		num += extraRotation;
		Graphics.DrawMesh(mesh, loc, Quaternion.AngleAxis(num, Vector3.up), MatSingleFor(thing), 0, null, 0);
	}

	public override string ToString()
	{
		return "RandomRotated(subGraphic=" + subGraphic?.ToString() + ")";
	}

	public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
	{
		return new Graphic_RandomRotated(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo), maxAngle)
		{
			data = data,
			drawSize = drawSize
		};
	}

	public override void Print(SectionLayer layer, Thing thing, float extraRotation)
	{
		float num = 0f;
		float? rotInRack = GetRotInRack(thing, thing.def, thing.Position);
		if (rotInRack.HasValue)
		{
			num = rotInRack.Value;
		}
		else if (thing != null)
		{
			num = 0f - maxAngle + (float)(thing.thingIDNumber * 542) % (maxAngle * 2f);
		}
		num += extraRotation;
		subGraphic.Print(layer, thing, num);
	}

	public override void TryInsertIntoAtlas(TextureAtlasGroup group)
	{
		subGraphic.TryInsertIntoAtlas(group);
	}

	private float? GetRotInRack(Thing thing, ThingDef thingDef, IntVec3 loc)
	{
		if (thing != null && thingDef.IsWeapon && thing.Spawned && loc.InBounds(thing.Map) && loc.GetEdifice(thing.Map) != null && loc.GetItemCount(thing.Map) >= 2)
		{
			if (thingDef.rotateInShelves)
			{
				return -90f;
			}
			return 0f;
		}
		return null;
	}
}
