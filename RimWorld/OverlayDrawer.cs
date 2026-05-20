using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class OverlayDrawer
{
	private Dictionary<Thing, OverlayTypes> overlaysToDraw = new Dictionary<Thing, OverlayTypes>();

	private Dictionary<Thing, ThingOverlaysHandle> overlayHandles = new Dictionary<Thing, ThingOverlaysHandle>();

	private Vector3 curOffset;

	private DrawBatch drawBatch = new DrawBatch();

	private static readonly Material ForbiddenMat;

	private static readonly Material NeedsPowerMat;

	private static readonly Material PowerOffMat;

	private static readonly Material QuestionMarkMat;

	private static readonly Material BrokenDownMat;

	private static readonly Material OutOfFuelMat;

	private static readonly Material WickMaterialA;

	private static readonly Material WickMaterialB;

	private static readonly Material SelfShutdownMat;

	private const int AltitudeIndex_Forbidden = 4;

	private const int AltitudeIndex_BurningWick = 5;

	private const int AltitudeIndex_QuestionMark = 6;

	private static float SingleCellForbiddenOffset;

	public const float PulseFrequency = 4f;

	public const float PulseAmplitude = 0.7f;

	private static readonly float BaseAlt;

	private const float StackOffsetMultipiler = 0.25f;

	static OverlayDrawer()
	{
		ForbiddenMat = MaterialPool.MatFrom("Things/Special/ForbiddenOverlay", ShaderDatabase.MetaOverlay);
		NeedsPowerMat = MaterialPool.MatFrom("UI/Overlays/NeedsPower", ShaderDatabase.MetaOverlay);
		PowerOffMat = MaterialPool.MatFrom("UI/Overlays/PowerOff", ShaderDatabase.MetaOverlay);
		QuestionMarkMat = MaterialPool.MatFrom("UI/Overlays/QuestionMark", ShaderDatabase.MetaOverlay);
		BrokenDownMat = MaterialPool.MatFrom("UI/Overlays/BrokenDown", ShaderDatabase.MetaOverlay);
		OutOfFuelMat = MaterialPool.MatFrom("UI/Overlays/OutOfFuel", ShaderDatabase.MetaOverlay);
		WickMaterialA = MaterialPool.MatFrom("Things/Special/BurningWickA", ShaderDatabase.MetaOverlay);
		WickMaterialB = MaterialPool.MatFrom("Things/Special/BurningWickB", ShaderDatabase.MetaOverlay);
		SelfShutdownMat = MaterialPool.MatFrom("UI/Overlays/SelfShutdown", ShaderDatabase.MetaOverlay);
		SingleCellForbiddenOffset = 0.3f;
		BaseAlt = AltitudeLayer.MetaOverlays.AltitudeFor();
	}

	public ThingOverlaysHandle GetOverlaysHandle(Thing thing)
	{
		if (!thing.Spawned)
		{
			return null;
		}
		if (!overlayHandles.TryGetValue(thing, out var value))
		{
			value = new ThingOverlaysHandle(this, thing);
			overlayHandles.Add(thing, value);
		}
		return value;
	}

	public void DisposeHandle(Thing thing)
	{
		if (overlayHandles.TryGetValue(thing, out var value))
		{
			value.Dispose();
		}
		overlayHandles.Remove(thing);
	}

	public OverlayHandle Enable(Thing thing, OverlayTypes types)
	{
		return GetOverlaysHandle(thing).Enable(types);
	}

	public void Disable(Thing thing, ref OverlayHandle? handle)
	{
		GetOverlaysHandle(thing).Disable(ref handle);
	}

	public void DrawOverlay(Thing t, OverlayTypes overlayType)
	{
		if (overlayType != OverlayTypes.None && !WorldComponent_GravshipController.CutsceneInProgress)
		{
			if (overlaysToDraw.TryGetValue(t, out var value))
			{
				overlaysToDraw[t] = value | overlayType;
			}
			else
			{
				overlaysToDraw.Add(t, overlayType);
			}
		}
	}

	public void DrawAllOverlays()
	{
		if (WorldComponent_GravshipController.CutsceneInProgress)
		{
			return;
		}
		try
		{
			foreach (KeyValuePair<Thing, ThingOverlaysHandle> overlayHandle in overlayHandles)
			{
				if (!overlayHandle.Key.Fogged())
				{
					DrawOverlay(overlayHandle.Key, overlayHandle.Value.OverlayTypes);
				}
			}
			foreach (KeyValuePair<Thing, OverlayTypes> item in overlaysToDraw)
			{
				curOffset = Vector3.zero;
				Thing key = item.Key;
				OverlayTypes value = item.Value;
				if ((value & OverlayTypes.BurningWick) != OverlayTypes.None)
				{
					RenderBurningWick(key);
				}
				else
				{
					OverlayTypes overlayTypes = OverlayTypes.NeedsPower | OverlayTypes.PowerOff;
					int bitCountOf = Gen.GetBitCountOf((long)(value & overlayTypes));
					float num = StackOffsetFor(key);
					switch (bitCountOf)
					{
					case 1:
						curOffset = Vector3.zero;
						break;
					case 2:
						curOffset = new Vector3(-0.5f * num, 0f, 0f);
						break;
					case 3:
						curOffset = new Vector3(-1.5f * num, 0f, 0f);
						break;
					}
					if ((value & OverlayTypes.NeedsPower) != OverlayTypes.None)
					{
						RenderNeedsPowerOverlay(key);
					}
					if ((value & OverlayTypes.PowerOff) != OverlayTypes.None)
					{
						RenderPowerOffOverlay(key);
					}
					if ((value & OverlayTypes.BrokenDown) != OverlayTypes.None)
					{
						RenderBrokenDownOverlay(key);
					}
					if ((value & OverlayTypes.OutOfFuel) != OverlayTypes.None)
					{
						RenderOutOfFuelOverlay(key);
					}
				}
				if ((value & OverlayTypes.ForbiddenBig) != OverlayTypes.None)
				{
					RenderForbiddenBigOverlay(key);
				}
				if ((value & OverlayTypes.Forbidden) != OverlayTypes.None)
				{
					RenderForbiddenOverlay(key);
				}
				if ((value & OverlayTypes.ForbiddenRefuel) != OverlayTypes.None)
				{
					RenderForbiddenRefuelOverlay(key);
				}
				if ((value & OverlayTypes.QuestionMark) != OverlayTypes.None)
				{
					RenderQuestionMarkOverlay(key);
				}
				if ((value & OverlayTypes.SelfShutdown) != OverlayTypes.None && ModsConfig.BiotechActive)
				{
					RenderRechargineOverlay(key);
				}
				if ((value & OverlayTypes.ForbiddenAtomizer) != OverlayTypes.None && ModsConfig.BiotechActive)
				{
					RenderForbiddenAtomizerOverlay(key);
				}
			}
		}
		finally
		{
			overlaysToDraw.Clear();
		}
		drawBatch.Flush();
	}

	private float StackOffsetFor(Thing t)
	{
		return (float)t.RotatedSize.x * 0.25f;
	}

	private void RenderNeedsPowerOverlay(Thing t)
	{
		RenderPulsingOverlay(t, NeedsPowerMat, 2);
	}

	private void RenderPowerOffOverlay(Thing t)
	{
		RenderPulsingOverlay(t, PowerOffMat, 3);
	}

	private void RenderBrokenDownOverlay(Thing t)
	{
		RenderPulsingOverlay(t, BrokenDownMat, 4);
	}

	private void RenderRechargineOverlay(Thing t)
	{
		Vector3 drawPos = t.DrawPos;
		drawPos.y = BaseAlt + 0.21951221f;
		RenderPulsingOverlayInternal(t, SelfShutdownMat, drawPos, MeshPool.plane05);
	}

	private void RenderOutOfFuelOverlay(Thing t)
	{
		CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();
		Material mat = MaterialPool.MatFrom((compRefuelable != null) ? compRefuelable.Props.FuelIcon : ThingDefOf.Chemfuel.uiIcon, ShaderDatabase.MetaOverlay, Color.white);
		RenderPulsingOverlay(t, mat, 5, incrementOffset: false);
		RenderPulsingOverlay(t, OutOfFuelMat, 6);
	}

	private void RenderPulsingOverlay(Thing thing, Material mat, int altInd, bool incrementOffset = true)
	{
		Mesh plane = MeshPool.plane08;
		RenderPulsingOverlay(thing, mat, altInd, plane, incrementOffset);
	}

	private void RenderPulsingOverlay(Thing thing, Material mat, int altInd, Mesh mesh, bool incrementOffset = true)
	{
		Vector3 drawPos = thing.TrueCenter();
		drawPos.y = BaseAlt + 0.03658537f * (float)altInd;
		drawPos += curOffset;
		if (thing.def.building != null && thing.def.building.isAttachment)
		{
			drawPos += (thing.Rotation.AsVector2 * 0.5f).ToVector3();
		}
		drawPos.y = Mathf.Min(drawPos.y, Find.Camera.transform.position.y - 0.1f);
		if (incrementOffset)
		{
			curOffset.x += StackOffsetFor(thing);
		}
		RenderPulsingOverlayInternal(thing, mat, drawPos, mesh);
	}

	private void RenderPulsingOverlayInternal(Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
	{
		float num = ((float)Math.Sin((Time.realtimeSinceStartup + 397f * (float)(thing.thingIDNumber % 571)) * 4f) + 1f) * 0.5f;
		num = 0.3f + num * 0.7f;
		Material material = FadedMaterialPool.FadedVersionOf(mat, num);
		drawBatch.DrawMesh(mesh, Matrix4x4.TRS(drawPos, Quaternion.identity, Vector3.one), material, 0, renderInstanced: true);
	}

	private void RenderForbiddenRefuelOverlay(Thing t)
	{
		CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();
		Material material = MaterialPool.MatFrom((compRefuelable != null) ? compRefuelable.Props.FuelIcon : ThingDefOf.Chemfuel.uiIcon, ShaderDatabase.MetaOverlayDesaturated, Color.white);
		Vector3 pos = t.TrueCenter();
		pos.y = BaseAlt + 15f / 82f;
		new Vector3(pos.x, pos.y + 0.03658537f, pos.z);
		drawBatch.DrawMesh(MeshPool.plane08, Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one), material, 0, renderInstanced: true);
		drawBatch.DrawMesh(MeshPool.plane08, Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one), ForbiddenMat, 0, renderInstanced: true);
	}

	private void RenderForbiddenAtomizerOverlay(Thing t)
	{
		if (ModsConfig.BiotechActive)
		{
			t.TryGetComp<CompAtomizer>();
			Material material = MaterialPool.MatFrom(ThingDefOf.Wastepack.uiIcon, ShaderDatabase.MetaOverlayDesaturated, Color.white);
			Vector3 pos = t.TrueCenter();
			pos.y = BaseAlt + 15f / 82f;
			drawBatch.DrawMesh(MeshPool.plane08, Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one), material, 0, renderInstanced: true);
			drawBatch.DrawMesh(MeshPool.plane08, Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one), ForbiddenMat, 0, renderInstanced: true);
		}
	}

	private void RenderForbiddenOverlay(Thing t)
	{
		Vector3 drawPos = t.DrawPos;
		bool? flag = (t.def.entityDefToBuild as ThingDef)?.building?.isAttachment;
		bool num;
		if (!flag.HasValue)
		{
			BuildingProperties building = t.def.building;
			if (building == null)
			{
				goto IL_0094;
			}
			num = building.isAttachment;
		}
		else
		{
			num = flag == true;
		}
		if (num)
		{
			drawPos += (t.Rotation.AsVector2 * 0.5f).ToVector3();
		}
		goto IL_0094;
		IL_0094:
		if (t.RotatedSize.z == 1)
		{
			drawPos.z -= SingleCellForbiddenOffset;
		}
		else
		{
			drawPos.z -= (float)t.RotatedSize.z * 0.3f;
		}
		drawPos.y = BaseAlt + 0.14634147f;
		drawBatch.DrawMesh(MeshPool.plane05, Matrix4x4.TRS(drawPos, Quaternion.identity, Vector3.one), ForbiddenMat, 0, renderInstanced: true);
	}

	private void RenderForbiddenBigOverlay(Thing t)
	{
		Vector3 drawPos = t.DrawPos;
		bool? flag = (t.def.entityDefToBuild as ThingDef)?.building?.isAttachment;
		bool num;
		if (!flag.HasValue)
		{
			BuildingProperties building = t.def.building;
			if (building == null)
			{
				goto IL_0094;
			}
			num = building.isAttachment;
		}
		else
		{
			num = flag == true;
		}
		if (num)
		{
			drawPos += (t.Rotation.AsVector2 * 0.5f).ToVector3();
		}
		goto IL_0094;
		IL_0094:
		drawPos.y = BaseAlt + 0.14634147f;
		drawBatch.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawPos, Quaternion.identity, Vector3.one), ForbiddenMat, 0, renderInstanced: true);
	}

	private void RenderBurningWick(Thing parent)
	{
		Material material = (((parent.thingIDNumber + Find.TickManager.TicksGame) % 6 >= 3) ? WickMaterialB : WickMaterialA);
		Vector3 drawPos = parent.DrawPos;
		drawPos.y = BaseAlt + 15f / 82f;
		drawBatch.DrawMesh(MeshPool.plane20, Matrix4x4.TRS(drawPos, Quaternion.identity, Vector3.one), material, 0, renderInstanced: true);
	}

	private void RenderQuestionMarkOverlay(Thing t)
	{
		Vector3 drawPos = t.DrawPos;
		drawPos.y = BaseAlt + 0.21951221f;
		if (t is Pawn)
		{
			drawPos.x += (float)t.def.size.x - 0.52f;
			drawPos.z += (float)t.def.size.z - 0.45f;
		}
		RenderPulsingOverlayInternal(t, QuestionMarkMat, drawPos, MeshPool.plane05);
	}
}
