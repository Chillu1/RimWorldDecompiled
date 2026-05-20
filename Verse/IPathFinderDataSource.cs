using System;
using System.Collections.Generic;

namespace Verse;

public interface IPathFinderDataSource : IDisposable
{
	void ComputeAll(IEnumerable<PathRequest> requests);

	bool UpdateIncrementally(IEnumerable<PathRequest> requests, List<IntVec3> cellDeltas);
}
