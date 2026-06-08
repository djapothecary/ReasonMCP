namespace ReasonMCP.Configurations
{
    public class AgentPermissions
    {
        public string FileSystem { get; set; } = "ReadOnly";    //  Readonly, ReadWrite, None
        public bool AllowToolCalling { get; set; } = false;
        public List<string> AllowedTools { get; set; } = [];
    }
}