using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompCableConnection : ThingComp
{
	private class Cable
	{
		public Mesh mesh;

		public Vector3 pos;

		public Quaternion quat;

		public Material mat;

		public List<(Vector2 offset, float rot)> points;

		public Map map;

		public ThingDef moteDef;

		private Mote motePower;

		public void Draw()
		{
			Graphics.DrawMesh(mesh, pos, quat, mat, 0);
		}

		public void Tick()
		{
			if (motePower == null || motePower.Destroyed)
			{
				(Vector2 offset, float rot) tuple = points.RandomElement();
				Vector3 vector = tuple.offset.ToVector3();
				float exactRot = tuple.rot + 180f;
				motePower = MoteMaker.MakeStaticMote(pos + vector, map, moteDef, 1f, makeOffscreen: false, exactRot);
			}
		}
	}

	private readonly List<Cable> cables = new List<Cable>();

	private const string CableTexturePath = "Things/Building/Cable";

	private const float CableYOffset = -5f;

	private const float CableLineMeshPointsSpacing = 0.2f;

	private const float CableLineMeshWidth = 0.15f;

	private static readonly float CableRotRange1 = 0f - Rand.Range(35f, 40f);

	private static readonly float CableRotRange2 = Rand.Range(20f, 25f);

	private readonly AttachPointType[] CableAttachPoints = new AttachPointType[4]
	{
		AttachPointType.CableConnection0,
		AttachPointType.CableConnection1,
		AttachPointType.CableConnection2,
		AttachPointType.CableConnection3
	};

	public CompProperties_CableConnection Props => (CompProperties_CableConnection)props;

	public Building ParentBuilding => parent as Building;

	public bool CanMote => ParentBuilding.IsWorking();

	public override void CompTick()
	{
		base.CompTick();
		if (!Props.drawMote || !CanMote)
		{
			return;
		}
		foreach (Cable cable in cables)
		{
			cable.Tick();
		}
	}

	public override void PostDraw()
	{
		base.PostDraw();
		foreach (Cable cable in cables)
		{
			cable.Draw();
		}
	}

	private Vector3 GetTargetConnectionPt(Thing target, ref int idx)
	{
		if (target is ThingWithComps thingWithComps)
		{
			CompAttachPoints comp = thingWithComps.GetComp<CompAttachPoints>();
			if (comp != null)
			{
				return comp.points.GetWorldPos(CableAttachPoints[idx++ % CableAttachPoints.Length]);
			}
		}
		return target.DrawPos + Props.offsets[parent.Rotation.AsInt][idx++];
	}

	public void RebuildCables(List<Thing> connections, Func<Thing, bool> connectionValidator = null)
	{
		cables.Clear();
		int idx = 0;
		foreach (Thing connection in connections)
		{
			if (connectionValidator == null || connectionValidator(connection))
			{
				Vector3 worldPos = parent.GetComp<CompAttachPoints>().points.GetWorldPos(AttachPointType.CableConnection0);
				Vector3 targetConnectionPt = GetTargetConnectionPt(connection, ref idx);
				Vector3 vector = worldPos - targetConnectionPt;
				Vector2 vector2 = new Vector2(vector.x, vector.z);
				Vector2 vector3 = vector2 * 0.5f;
				Vector3 pos = targetConnectionPt;
				pos.y += -4.9268293f;
				Vector2 vector4 = Vector2.Perpendicular(vector2 * 0.4f);
				bool num = Vector2.Dot(vector4.normalized, Vector2.up) < 0f;
				if (num)
				{
					vector4 = -vector4;
				}
				float degrees = (num ? CableRotRange1 : CableRotRange2);
				float degrees2 = (num ? CableRotRange2 : CableRotRange1);
				Vector2[] array = LineMeshGenerator.CalculateEvenlySpacedPoints(new List<Vector2>
				{
					new Vector2(0f, 0f),
					vector3 + vector4.RotatedBy(degrees),
					vector3 + vector4.RotatedBy(degrees2),
					vector2
				}, 0.2f);
				Mesh mesh = LineMeshGenerator.Generate(array, 0.15f);
				Material mat = MaterialPool.MatFrom(new MaterialRequest
				{
					mainTex = ContentFinder<Texture2D>.Get("Things/Building/Cable"),
					shader = ShaderDatabase.Transparent,
					color = Props.color
				});
				List<(Vector2, float)> list = new List<(Vector2, float)>();
				int i = 1;
				for (int num2 = array.Length; i < num2; i++)
				{
					Vector2 vector5 = array[i];
					Vector2 b = array[i - 1];
					float item = vector5.AngleTo(b) + 90f;
					list.Add((vector5, item));
				}
				Cable item2 = new Cable
				{
					mesh = mesh,
					mat = mat,
					pos = pos,
					map = parent.Map,
					points = list,
					quat = Quaternion.identity,
					moteDef = Props.moteDef
				};
				cables.Add(item2);
			}
		}
	}
}
