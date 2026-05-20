using System;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class Graphic_Fleck : Graphic_Single
{
	protected virtual bool AllowInstancing => true;

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		throw new NotImplementedException();
	}

	public virtual void DrawFleck(FleckDrawData drawData, DrawBatch batch)
	{
		Color value;
		if (drawData.overrideColor.HasValue)
		{
			value = drawData.overrideColor.Value;
		}
		else
		{
			float alpha = drawData.alpha;
			if (alpha <= 0f)
			{
				if (drawData.propertyBlock != null)
				{
					batch.ReturnPropertyBlock(drawData.propertyBlock);
				}
				return;
			}
			value = base.Color * drawData.color;
			value.a *= alpha;
		}
		Vector3 scale = drawData.scale;
		scale.x *= data.drawSize.x;
		scale.z *= data.drawSize.y;
		Mesh mesh = MeshPool.plane10;
		float num = drawData.rotation;
		if (scale.x < 0f && scale.y >= 0f)
		{
			scale.x = 0f - scale.x;
			mesh = MeshPool.plane10Flip;
		}
		else if (scale.x >= 0f && scale.y < 0f)
		{
			scale.y = 0f - scale.y;
			mesh = MeshPool.plane10Flip;
			num += 180f;
		}
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetTRS(drawData.pos, Quaternion.AngleAxis(num, Vector3.up), scale);
		Material matSingle = MatSingle;
		batch.DrawMesh(mesh, matrix, matSingle, drawData.drawLayer, value, data.renderInstanced && AllowInstancing, drawData.propertyBlock);
	}

	public override string ToString()
	{
		string[] obj = new string[7]
		{
			"Fleck(path=",
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
