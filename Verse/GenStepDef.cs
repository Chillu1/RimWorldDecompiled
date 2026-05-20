using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class GenStepDef : Def
{
	public SitePartDef linkWithSite;

	public float order;

	public GenStep genStep;

	public List<GenStepDef> preventsGenSteps;

	public override void PostLoad()
	{
		base.PostLoad();
		genStep.def = this;
	}
}
