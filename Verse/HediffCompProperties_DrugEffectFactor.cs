using RimWorld;

namespace Verse
{
	public class HediffCompProperties_DrugEffectFactor : HediffCompProperties
	{
		public ChemicalDef chemical;

		public HediffCompProperties_DrugEffectFactor()
		{
			compClass = typeof(HediffComp_DrugEffectFactor);
		}
	}
}
