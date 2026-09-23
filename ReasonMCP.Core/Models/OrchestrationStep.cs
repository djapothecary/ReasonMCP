using System.Xml.Serialization;

namespace ReasonMCP.Core.Models
{
    [XmlType("step")]
    public class OrchestrationStep
    {
        [XmlAttribute("number")]
        public int StepNumber { get; init; }

        // Default to Reason but can override to (Seraph, Mnemosyne, etc)
        [XmlAttribute("agent")]
        public string TargetAgentId { get; init; } = "Reason";

        //  Human in the loop toggle
        [XmlAttribute("requireApproval")]
        public bool RequiresHumanApproval { get; init; } = false;

        [XmlAttribute("agentReview")]
        public bool ProcessAgenticReview { get; init; } = false;

        [XmlElement("description")]
        public string PromptDescription { get; init; } = string.Empty;

        [XmlElement("task")]
        public string TaskToComplete { get; init; } = string.Empty;

        [XmlElement("constraints")]
        public string Constraints { get; init; } = string.Empty;

        [XmlElement("prompt")]
        public string AgentPrompt { get; init; } = string.Empty;

        [XmlElement("output")]
        public string OutputRequirements { get; init; } = string.Empty;
    }
}