using Verse;

namespace RimWorld;

public interface IThingStudied
{
	void OnStudied(Pawn studier, float amount, KnowledgeCategoryDef category = null);
}
