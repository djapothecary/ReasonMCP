using System.Xml.Serialization;

namespace ReasonMCP.Core.Models
{
    [XmlRoot("reason_playbook", Namespace = "")]
    public class AgenticPlaybook
    {
        [XmlElement("step")]
        public List<OrchestrationStep> Steps { get; set; } = [];

        [XmlIgnore]
        public int LastStepCompleted { get; set; }

        [XmlIgnore]
        public string PlaybookState { get; set; } = string.Empty;

        [XmlIgnore]
        public bool PlaybookCompleted { get; set; } = false;
    }
}