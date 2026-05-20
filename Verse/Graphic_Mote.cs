using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class Graphic_Mote : Graphic_Single
{
	protected static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

	protected virtual bool ForcePropertyBlock => false;

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		DrawMoteInternal(loc, rot, thingDef, thing, 0);
	}

	public void DrawMoteInternal(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, int layer)
	{
		DrawMote(data, MatSingle, base.Color, loc, rot, thingDef, thing, 0, ForcePropertyBlock);
	}

	public static void DrawMote(GraphicData data, Material material, Color color, Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, int layer, bool forcePropertyBlock = false, MaterialPropertyBlock overridePropertyBlock = null)
	{
		Mote mote = (Mote)thing;
		float alpha = mote.Alpha;
		if (!(alpha <= 0f))
		{
			Color color2 = color * mote.instanceColor;
			color2.a *= alpha;
			Vector3 exactScale = mote.ExactScale;
			exactScale.x *= data.drawSize.x;
			exactScale.z *= data.drawSize.y;
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(mote.DrawPos, Quaternion.AngleAxis(mote.exactRotation, Vector3.up), exactScale);
			if (!forcePropertyBlock && color2.IndistinguishableFrom(material.color))
			{
				Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer, null, 0);
				return;
			}
			propertyBlock.SetColor(ShaderPropertyIDs.Color, color2);
			Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer, null, 0, overridePropertyBlock ?? propertyBlock);
		}
	}

	public static void DrawMote(GraphicData data, Material material, Vector3 loc, float rotation, int layer, bool forcePropertyBlock = false, MaterialPropertyBlock overridePropertyBlock = null)
	{
		Color color = data.color;
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetTRS(loc, Quaternion.AngleAxis(rotation, Vector3.up), new Vector3(data.drawSize.x, 1f, data.drawSize.y));
		if (!forcePropertyBlock && color.IndistinguishableFrom(material.color))
		{
			Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer, null, 0);
			return;
		}
		propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
		Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer, null, 0, overridePropertyBlock ?? propertyBlock);
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
