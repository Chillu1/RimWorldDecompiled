using System;
using System.Collections.Generic;
using System.Threading;

namespace Verse;

public class FleckSystemBase<TFleck> : FleckSystem where TFleck : struct, IFleck
{
	private List<TFleck> dataRealtime = new List<TFleck>();

	private List<TFleck> dataGametime = new List<TFleck>();

	private List<GenThreading.Slice> tmpParallelizationSlices = new List<GenThreading.Slice>();

	private List<FleckParallelizationInfo> tmpParallelizationInfo = new List<FleckParallelizationInfo>();

	private List<int> tmpRemoveIndices = new List<int>();

	private WaitCallback CachedDrawParallelWaitCallback;

	protected virtual bool ParallelizedDrawing => false;

	public FleckSystemBase(FleckManager parent)
		: base(parent)
	{
	}

	public override void Update(float deltaTime)
	{
		try
		{
			for (int num = dataRealtime.Count - 1; num >= 0; num--)
			{
				TFleck value = dataRealtime[num];
				if (value.TimeInterval(deltaTime, parent.parent))
				{
					tmpRemoveIndices.Add(num);
				}
				else
				{
					dataRealtime[num] = value;
				}
			}
			dataRealtime.RemoveBatchUnordered(tmpRemoveIndices);
		}
		finally
		{
			tmpRemoveIndices.Clear();
		}
	}

	public override void Tick()
	{
		try
		{
			for (int num = dataGametime.Count - 1; num >= 0; num--)
			{
				TFleck value = dataGametime[num];
				if (value.TimeInterval(1f / 60f, parent.parent))
				{
					tmpRemoveIndices.Add(num);
				}
				else
				{
					dataGametime[num] = value;
				}
			}
			dataGametime.RemoveBatchUnordered(tmpRemoveIndices);
		}
		finally
		{
			tmpRemoveIndices.Clear();
		}
	}

	private static void DrawParallel(object state)
	{
		if (WorldComponent_GravshipController.GravshipRenderInProgess)
		{
			return;
		}
		FleckParallelizationInfo fleckParallelizationInfo = (FleckParallelizationInfo)state;
		try
		{
			List<FleckThrown> list = (List<FleckThrown>)fleckParallelizationInfo.data;
			for (int i = fleckParallelizationInfo.startIndex; i < fleckParallelizationInfo.endIndex; i++)
			{
				list[i].Draw(fleckParallelizationInfo.drawBatch);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
		finally
		{
			fleckParallelizationInfo.doneEvent.Set();
		}
	}

	public override void ForceDraw(DrawBatch drawBatch)
	{
		foreach (FleckDef handledDef in handledDefs)
		{
			if (handledDef.graphicData != null)
			{
				handledDef.graphicData.ExplicitlyInitCachedGraphic();
			}
			if (handledDef.randomGraphics == null)
			{
				continue;
			}
			foreach (GraphicData randomGraphic in handledDef.randomGraphics)
			{
				randomGraphic.ExplicitlyInitCachedGraphic();
			}
		}
		int parallelizationDegree;
		if (ParallelizedDrawing)
		{
			if (CachedDrawParallelWaitCallback == null)
			{
				CachedDrawParallelWaitCallback = DrawParallel;
			}
			parallelizationDegree = Environment.ProcessorCount;
			Process(dataRealtime);
			Process(dataGametime);
		}
		else
		{
			Process2(dataRealtime);
			Process2(dataGametime);
		}
		void Process(List<TFleck> data)
		{
			if (data.Count > 0)
			{
				try
				{
					tmpParallelizationSlices.Clear();
					GenThreading.SliceWorkNoAlloc(0, data.Count, parallelizationDegree, tmpParallelizationSlices);
					foreach (GenThreading.Slice tmpParallelizationSlice in tmpParallelizationSlices)
					{
						FleckParallelizationInfo parallelizationInfo = FleckUtility.GetParallelizationInfo();
						parallelizationInfo.startIndex = tmpParallelizationSlice.fromInclusive;
						parallelizationInfo.endIndex = tmpParallelizationSlice.toExclusive;
						parallelizationInfo.data = data;
						ThreadPool.QueueUserWorkItem(CachedDrawParallelWaitCallback, parallelizationInfo);
						tmpParallelizationInfo.Add(parallelizationInfo);
					}
					foreach (FleckParallelizationInfo item in tmpParallelizationInfo)
					{
						item.doneEvent.WaitOne();
						drawBatch.MergeWith(item.drawBatch);
					}
				}
				finally
				{
					foreach (FleckParallelizationInfo item2 in tmpParallelizationInfo)
					{
						FleckUtility.ReturnParallelizationInfo(item2);
					}
					tmpParallelizationInfo.Clear();
				}
			}
		}
		void Process2(List<TFleck> data)
		{
			for (int num = data.Count - 1; num >= 0; num--)
			{
				data[num].Draw(drawBatch);
			}
		}
	}

	public override void CreateFleck(FleckCreationData creationData)
	{
		TFleck item = new TFleck();
		item.Setup(creationData);
		if (creationData.def.realTime)
		{
			dataRealtime.Add(item);
		}
		else
		{
			dataGametime.Add(item);
		}
	}

	public override void MergeWith(FleckSystem system)
	{
		if (system is FleckSystemBase<TFleck> fleckSystemBase)
		{
			handledDefs.AddRangeUnique(fleckSystemBase.handledDefs);
			dataRealtime.AddRange(fleckSystemBase.dataRealtime);
			dataGametime.AddRange(fleckSystemBase.dataGametime);
			return;
		}
		throw new NotImplementedException("FleckSystemBase does not yet support merging with " + system.GetType().Name);
	}

	public override IEnumerable<IFleck> EnumerateFlecks()
	{
		for (int i = dataGametime.Count - 1; i >= 0; i--)
		{
			yield return dataGametime[i];
		}
		for (int i = dataRealtime.Count - 1; i >= 0; i--)
		{
			yield return dataRealtime[i];
		}
	}

	public override void RemoveAllFlecks(Predicate<IFleck> shouldRemove)
	{
		for (int num = dataGametime.Count - 1; num >= 0; num--)
		{
			if (shouldRemove(dataGametime[num]))
			{
				dataGametime.RemoveAt(num);
			}
		}
		for (int num2 = dataRealtime.Count - 1; num2 >= 0; num2--)
		{
			if (shouldRemove(dataRealtime[num2]))
			{
				dataRealtime.RemoveAt(num2);
			}
		}
	}

	public override void ExposeData()
	{
	}
}
