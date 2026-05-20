using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class CompBook : CompReadable
{
	public new CompProperties_Book Props => (CompProperties_Book)props;

	public new IEnumerable<BookOutcomeDoer> Doers => doers.OfType<BookOutcomeDoer>();

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (BookOutcomeDoer doer in Doers)
		{
			foreach (Gizmo gizmo in doer.GetGizmos())
			{
				yield return gizmo;
			}
		}
	}
}
