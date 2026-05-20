using System;
using System.Collections.Generic;

namespace Verse;

public abstract class DynamicPawnRenderNodeSetup
{
	public virtual List<Type> SetupAfter { get; }

	public abstract bool HumanlikeOnly { get; }

	public abstract IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> GetDynamicNodes(Pawn pawn, PawnRenderTree tree);
}
