using System;
using Verse;

namespace RimWorld.QuestGen;

public abstract class QuestNode
{
	[Unsaved(false)]
	[TranslationHandle]
	public string myTypeShort;

	public SlateRef<float?> selectionWeight;

	public QuestNode()
	{
		myTypeShort = GetType().Name;
		if (myTypeShort.StartsWith("QuestNode_"))
		{
			myTypeShort = myTypeShort.Substring("QuestNode_".Length);
		}
	}

	public float SelectionWeight(Slate slate)
	{
		return selectionWeight.GetValue(slate) ?? 1f;
	}

	public void Run()
	{
		if (DeepProfiler.enabled)
		{
			DeepProfiler.Start(ToString());
		}
		try
		{
			RunInt();
		}
		catch (Exception ex)
		{
			Log.Error("Exception running " + GetType().Name + ": " + ex?.ToString() + "\n\nSlate vars:\n" + QuestGen.slate.ToString());
		}
		if (DeepProfiler.enabled)
		{
			DeepProfiler.End();
		}
	}

	public bool TestRun(Slate slate)
	{
		try
		{
			if (slate.TryGet<Action<QuestNode, Slate>>("testRunCallback", out var var))
			{
				var?.Invoke(this, slate);
			}
			return TestRunInt(slate);
		}
		catch (Exception arg)
		{
			Log.Error($"Exception test running {GetType().Name}: {arg}\n\nSlate vars:\n{slate}");
			return false;
		}
		finally
		{
		}
	}

	protected abstract void RunInt();

	protected abstract bool TestRunInt(Slate slate);

	public override string ToString()
	{
		return GetType().Name;
	}
}
