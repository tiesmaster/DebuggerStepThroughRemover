namespace DebuggerStepThroughRemover.Test
{
    public class TestData
    {
        public string Description { private get; set; }
        public string BrokenSource { get; set; }
        public string ExpectedFixedSource { get; set; }
        // TODO: maybe replace these two props with some higher level construct
        // from Rosly itself (maybe Microsoft.CodeAnalysis.Text.LinePosition?)
        public int Line { get; set; }
        public int Column { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}