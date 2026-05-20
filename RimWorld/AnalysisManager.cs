using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class AnalysisManager : IExposable
{
	private Dictionary<int, AnalysisDetails> analysisDetails = new Dictionary<int, AnalysisDetails>();

	public IEnumerable<AnalysisDetails> AnalysisDetailsForReading => analysisDetails.Values;

	public bool TryGetAnalysisProgress(int id, out AnalysisDetails details)
	{
		return analysisDetails.TryGetValue(id, out details);
	}

	public bool TryIncrementAnalysisProgress(int id, out AnalysisDetails details)
	{
		if (!analysisDetails.TryGetValue(id, out details))
		{
			return false;
		}
		if (details.timesDone >= details.required)
		{
			return false;
		}
		details.timesDone++;
		return true;
	}

	public bool IsAnalysisComplete(int id)
	{
		if (!analysisDetails.TryGetValue(id, out var value))
		{
			return true;
		}
		return value.Satisfied;
	}

	public void ForceCompleteAnalysisProgress(int id)
	{
		if (!HasAnalysisWithID(id))
		{
			AddAnalysisTask(id, 1);
		}
		if (analysisDetails.TryGetValue(id, out var value))
		{
			value.timesDone = value.required;
		}
	}

	public void AddAnalysisTask(int id, int requiredTimes)
	{
		if (!analysisDetails.ContainsKey(id))
		{
			analysisDetails[id] = new AnalysisDetails
			{
				id = id,
				required = requiredTimes
			};
		}
	}

	public void RemoveAnalysisDetails(int id)
	{
		if (analysisDetails.ContainsKey(id))
		{
			analysisDetails.Remove(id);
		}
	}

	public bool HasAnalysisWithID(int id)
	{
		return analysisDetails.ContainsKey(id);
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref analysisDetails, "analysisDetails", LookMode.Value, LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && analysisDetails == null)
		{
			analysisDetails = new Dictionary<int, AnalysisDetails>();
		}
	}
}
