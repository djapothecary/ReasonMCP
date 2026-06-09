using System.Text;
using ReasonMCP.DTOs;

namespace ReasonMCP.Utilities
{
    public static class ChatPayloadUtility
    {
        /// <summary>
        /// Converts the raw prompt and any attached files into a single,
        /// XML-augmented string for the LLM
        /// </summary>
        public static string ToAugmentedPrompt(
            this VSCodeChatPayloadDto payload
        )
        {
            // fast exit if no files attached
            if (payload.Attachments == null || !payload.Attachments.Any())
                return payload.Prompt;

            var augmentedPrompt = new StringBuilder();
            augmentedPrompt.AppendLine("Here are the provided reference files:");

            foreach (var file in payload.Attachments)
            {
                augmentedPrompt.AppendLine($"\n<file name=\"{file.Filename}\">");
                augmentedPrompt.AppendLine(file.Content);
                augmentedPrompt.AppendLine("</file>");
            }

            augmentedPrompt.AppendLine("\nBased on the above files, please address the following request:\n");
            augmentedPrompt.AppendLine(payload.Prompt);

            return augmentedPrompt.ToString();
        }
    }
}