namespace ReasonMCP.Configuration
{
    public class DocumentsProcessing
    {
        public bool ProcessADRs { get; set; } = false;
        public bool ProcessNewsLetters { get; set; } = false;

        public bool ProcessPDFs { get; set; } = false;
    }
}