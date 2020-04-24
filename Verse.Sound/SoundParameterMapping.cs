namespace Verse.Sound
{
	public class SoundParameterMapping
	{
		[Description("The independent parameter that the game will change to drive this relationship.\n\nOn the graph, this is the X axis.")]
		public SoundParamSource inParam;

		[Description("The dependent parameter that will respond to changes to the in-parameter.\n\nThis must match something the game can change about this sound.\n\nOn the graph, this is the y-axis.")]
		public SoundParamTarget outParam;

		[Description("Determines when sound parameters should be applies to samples.\n\nConstant means the parameters are updated every frame and can change continuously.\n\nOncePerSample means that the parameters are applied exactly once to each sample that plays.")]
		public SoundParamUpdateMode paramUpdateMode;

		[EditorHidden]
		public SimpleCurve curve;

		public SoundParameterMapping()
		{
			curve = new SimpleCurve();
			curve.Add(new CurvePoint(0f, 0f));
			curve.Add(new CurvePoint(1f, 1f));
		}

		public void DoEditWidgets(WidgetRow widgetRow)
		{
			string title = ((inParam != null) ? inParam.Label : "null") + " -> " + ((outParam != null) ? outParam.Label : "null");
			if (widgetRow.ButtonText("Edit curve", "Edit the curve mapping the in parameter to the out parameter."))
			{
				Find.WindowStack.Add(new EditWindow_CurveEditor(curve, title));
			}
		}

		public void Apply(Sample samp)
		{
			if (inParam != null && outParam != null)
			{
				float x = inParam.ValueFor(samp);
				float value = curve.Evaluate(x);
				outParam.SetOn(samp, value);
			}
		}
	}
}
