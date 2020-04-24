namespace Verse
{
	public struct GenStepWithParams
	{
		public GenStepDef def;

		public GenStepParams parms;

		public GenStepWithParams(GenStepDef def, GenStepParams parms)
		{
			this.def = def;
			this.parms = parms;
		}
	}
}
