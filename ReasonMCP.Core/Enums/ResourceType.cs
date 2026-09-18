namespace ReasonMCP.Core.Enums
{
    public enum ResourceType
    {
        Plugin,         //  Native or Semantic Functions
        MemoryIndex,    //  Vector DB collections / knowledge bases
        Model,          //  Access to specific LLMS (local or frontier)
        Kernel          //  Core system operations
    }
}