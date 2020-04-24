using Verse;

namespace RimWorld
{
	public class FiringIncident : IExposable
	{
		public IncidentDef def;

		public IncidentParms parms = new IncidentParms();

		public StorytellerComp source;

		public QuestPart sourceQuestPart;

		public FiringIncident()
		{
		}

		public FiringIncident(IncidentDef def, StorytellerComp source, IncidentParms parms = null)
		{
			this.def = def;
			if (parms != null)
			{
				this.parms = parms;
			}
			this.source = source;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Deep.Look(ref parms, "parms");
		}

		public override string ToString()
		{
			string text = def.defName;
			if (parms != null)
			{
				text = text + ", parms=(" + parms.ToString() + ")";
			}
			if (source != null)
			{
				text = text + ", source=" + source;
			}
			if (sourceQuestPart != null)
			{
				text = text + ", sourceQuestPart=" + sourceQuestPart;
			}
			return text;
		}
	}
}
