namespace ReasonMCP.DTOs
{
    // A temporary DTO to catch whatever VS Code is throwing at us
    public class VSCodeChatPayload
    {
        public string Model { get; set; } = string.Empty;
        public object[] Messages { get; set; } = []; // We will strongly type this once we see the payload
    }

}