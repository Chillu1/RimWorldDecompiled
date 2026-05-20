using System;
using System.Collections.Generic;
using Gilzoide.ManagedJobs;
using LudeonTK;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Verse;

public sealed class DynamicDrawManager
{
	private struct ThingCullDetails
	{
		public IntVec3 cell;

		public CellRect coarseBounds;

		public bool seeThroughFog;

		public float hideAtSnowOrSandDepth;

		public bool drawOffscreen;

		public Vector3 pos;

		public Vector2 drawSize;

		public bool drawSilhouette;

		public bool hasSunShadows;

		public Matrix4x4 trs;

		public bool shouldDraw;

		public bool shouldDrawShadow;
	}

	[BurstCompile]
	private struct CullJob : IJobParallelFor
	{
		[ReadOnly]
		public CellRect viewRect;

		[ReadOnly]
		public CellRect shadowViewRect;

		[ReadOnly]
		public CellIndices indices;

		[ReadOnly]
		public bool checkShadows;

		[ReadOnly]
		public NativeBitArray fogGrid;

		[ReadOnly]
		public NativeArray<float> depthGrid;

		public NativeArray<ThingCullDetails> details;

		[BurstCompile]
		public void Execute(int index)
		{
			ThingCullDetails value = details[index];
			int num = indices[value.cell];
			if (!indices.Contains(num) || (!value.seeThroughFog && fogGrid.IsSet(num)) || (value.hideAtSnowOrSandDepth < 1f && depthGrid[num] > value.hideAtSnowOrSandDepth))
			{
				return;
			}
			if (!value.drawOffscreen && !viewRect.Overlaps(value.coarseBounds))
			{
				if (checkShadows && value.hasSunShadows)
				{
					value.shouldDrawShadow = shadowViewRect.Contains(value.cell);
				}
			}
			else
			{
				value.shouldDraw = true;
				details[index] = value;
			}
		}
	}

	[BurstCompile]
	private struct ComputeSilhouetteMatricesJob : IJobParallelFor
	{
		public Vector3 inverseFovScale;

		public float altitude;

		public NativeArray<ThingCullDetails> details;

		[BurstCompile]
		public void Execute(int index)
		{
			ThingCullDetails value = details[index];
			if (value.drawSilhouette)
			{
				Vector3 vector = new Vector3(value.drawSize.x, 0f, value.drawSize.y);
				Vector3 s = inverseFovScale;
				if (vector.x < 2.5f)
				{
					s.x *= vector.x + SilhouetteUtility.AdjustScale(vector.x);
				}
				else
				{
					s.x *= vector.x;
				}
				if (vector.z < 2.5f)
				{
					s.z *= vector.z + SilhouetteUtility.AdjustScale(vector.z);
				}
				else
				{
					s.z *= vector.z;
				}
				Vector3 pos = value.pos;
				pos.y = altitude;
				value.trs = Matrix4x4.TRS(pos, Quaternion.AngleAxis(0f, Vector3.up), s);
				details[index] = value;
			}
		}
	}

	private class PreDrawThings : IJobParallelFor
	{
		public NativeArray<ThingCullDetails> details;

		public List<Thing> things;

		public void Execute(int index)
		{
			Thing thing = things[index];
			if (details[index].shouldDraw)
			{
				thing.DynamicDrawPhase(DrawPhase.ParallelPreDraw);
			}
		}
	}

	private Map map;

	private readonly List<Thing> drawThings = new List<Thing>();

	private bool drawingNow;

	public IReadOnlyList<Thing> DrawThings => drawThings;

	public DynamicDrawManager(Map map)
	{
		this.map = map;
	}

	public void RegisterDrawable(Thing t)
	{
		if (t.def.drawerType != DrawerType.None)
		{
			if (drawingNow)
			{
				Log.Warning($"Cannot register drawable {t} while drawing is in progress. Things shouldn't be spawned in Draw methods.");
			}
			drawThings.Add(t);
		}
	}

	public void DeRegisterDrawable(Thing t)
	{
		if (t.def.drawerType != DrawerType.None)
		{
			if (drawingNow)
			{
				Log.Warning($"Cannot deregister drawable {t} while drawing is in progress. Things shouldn't be despawned in Draw methods.");
			}
			drawThings.Remove(t);
		}
	}

	public void DrawDynamicThings()
	{
		if (!DebugViewSettings.drawThingsDynamic || map.Disposed)
		{
			return;
		}
		drawingNow = true;
		bool flag = SilhouetteUtility.CanHighlightAny();
		NativeArray<ThingCullDetails> details = new NativeArray<ThingCullDetails>(drawThings.Count, Allocator.TempJob);
		ComputeCulledThings(details);
		if (!DebugViewSettings.singleThreadedDrawing)
		{
			using (new ProfilerBlock("Ensure Graphics Initialized"))
			{
				for (int i = 0; i < details.Length; i++)
				{
					if (details[i].shouldDraw)
					{
						drawThings[i].DynamicDrawPhase(DrawPhase.EnsureInitialized);
					}
				}
			}
			PreDrawVisibleThings(details);
		}
		try
		{
			using (new ProfilerBlock("Draw Visible"))
			{
				for (int j = 0; j < details.Length; j++)
				{
					if (!details[j].shouldDraw && !details[j].shouldDrawShadow)
					{
						continue;
					}
					try
					{
						if (details[j].shouldDraw)
						{
							drawThings[j].DynamicDrawPhase(DrawPhase.Draw);
						}
						else if (drawThings[j] is Pawn pawn)
						{
							pawn.DrawShadowAt(pawn.DrawPos);
						}
					}
					catch (Exception arg)
					{
						Log.Error($"Exception drawing {drawThings[j]}: {arg}");
					}
				}
			}
			if (flag)
			{
				DrawSilhouettes(details);
			}
		}
		catch (Exception arg2)
		{
			Log.Error($"Exception drawing dynamic things: {arg2}");
		}
		finally
		{
			details.Dispose();
		}
		drawingNow = false;
	}

	private void PreDrawVisibleThings(NativeArray<ThingCullDetails> details)
	{
		using (new ProfilerBlock("Pre draw job"))
		{
			new ManagedJobParallelFor(new PreDrawThings
			{
				details = details,
				things = drawThings
			}).Schedule(details.Length, UnityData.GetIdealBatchCount(details.Length)).Complete();
		}
	}

	private void ComputeCulledThings(NativeArray<ThingCullDetails> details)
	{
		CellRect cellRect = Find.CameraDriver.CurrentViewRect.ExpandedBy(1);
		if (WorldComponent_GravshipController.GravshipRenderInProgess)
		{
			cellRect = cellRect.Encapsulate(WorldComponent_GravshipController.GravshipRenderBounds);
		}
		cellRect.ClipInsideMap(map);
		using (new ProfilerBlock("Prepare cull job"))
		{
			for (int i = 0; i < drawThings.Count; i++)
			{
				Thing thing = drawThings[i];
				ThingCullDetails value = new ThingCullDetails
				{
					cell = ((thing is Pawn) ? thing.DrawPos.ToIntVec3() : thing.Position),
					coarseBounds = thing.OccupiedDrawRect(),
					hideAtSnowOrSandDepth = thing.def.hideAtSnowOrSandDepth,
					seeThroughFog = thing.def.seeThroughFog,
					hasSunShadows = thing.def.HasSunShadows,
					drawOffscreen = thing.def.drawOffscreen
				};
				details[i] = value;
			}
		}
		using (new ProfilerBlock("Cull job"))
		{
			IJobParallelForExtensions.Schedule(new CullJob
			{
				indices = map.cellIndices,
				viewRect = cellRect,
				fogGrid = map.fogGrid.FogGrid_Unsafe,
				depthGrid = map.snowGrid.DepthGrid_Unsafe,
				details = details,
				checkShadows = MatBases.SunShadow.shader.isSupported,
				shadowViewRect = SectionLayer_SunShadows.GetSunShadowsViewRect(map, cellRect)
			}, details.Length, UnityData.GetIdealBatchCount(details.Length)).Complete();
		}
	}

	private void DrawSilhouettes(NativeArray<ThingCullDetails> details)
	{
		using (new ProfilerBlock("Prepare matrices job"))
		{
			for (int i = 0; i < details.Length; i++)
			{
				if (details[i].shouldDraw)
				{
					Thing thing = drawThings[i];
					if (SilhouetteUtility.ShouldDrawSilhouette(thing) && thing is Pawn pawn)
					{
						ThingCullDetails value = details[i];
						value.pos = pawn.Drawer.renderer.SilhouettePos;
						value.drawSize = pawn.Drawer.renderer.SilhouetteGraphic.drawSize;
						value.drawSilhouette = true;
						details[i] = value;
					}
				}
			}
		}
		using (new ProfilerBlock("Compute matrices"))
		{
			IJobParallelForExtensions.Schedule(new ComputeSilhouetteMatricesJob
			{
				inverseFovScale = Find.CameraDriver.InverseFovScale,
				altitude = AltitudeLayer.Silhouettes.AltitudeFor(),
				details = details
			}, details.Length, UnityData.GetIdealBatchCount(details.Length)).Complete();
		}
		using (new ProfilerBlock("Draw silhouettes"))
		{
			for (int j = 0; j < details.Length; j++)
			{
				if (details[j].drawSilhouette && drawThings[j] is Pawn thing2)
				{
					SilhouetteUtility.DrawSilhouetteJob(thing2, details[j].trs);
				}
			}
		}
	}

	public void LogDynamicDrawThings()
	{
		Log.Message(DebugLogsUtility.ThingListToUniqueCountString(drawThings));
	}
}
