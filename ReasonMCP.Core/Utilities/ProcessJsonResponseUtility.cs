using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;

namespace ReasonMCP.Utilities
{
    public static class ProcessJsonResponseUtility
    {
        public static string TryParseChatMessageContent(
            ChatMessageContent chatMessageContent
        )
        {
            if (chatMessageContent == null || chatMessageContent.Content == null)
            {
                return string.Empty;
            }

            if (!chatMessageContent!.Content!.StartsWith("{\"name\":"))
            {
                //  some responses from reason where "{\"query\":"
                //  requires additional testing
                return chatMessageContent.Content;
            }

            try
            {
                //  check the Chat Message Content to ensure there is a response
                //  sometimes after tool calling, no response is returned or it is buried inside a json response
                string cleanedJson = FixMalformedJson(chatMessageContent.Content);

                using JsonDocument doc = JsonDocument.Parse(cleanedJson);

                //  2.  Check the "parameters" property
                if (doc.RootElement.TryGetProperty("parameters", out JsonElement parameters))
                {
                    //  3.  extract the 'query' value
                    if (parameters.TryGetProperty("query", out JsonElement queryElement))
                    {
                        return queryElement.GetString() ?? string.Empty;
                    }
                }
            }
            catch (JsonException ex)
            {

                throw;
            }

            return chatMessageContent.Content;
        }

        private static string FixMalformedJson(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
                return rawJson;

            //  Fix 1:  resolve escaped single quotes (\') which are invalid in JSOn
            string fixedJson = rawJson.Replace("\\'", "'");

            //  Fix 2:  resolve double backslashes that shouldn't be there
            //  LLM's often output '\\' instatead of '\'
            fixedJson = fixedJson.Replace("\\\\", "\\");

            //  Fix 3:  remove common LLm "Markdown" wrappers if they exists
            fixedJson = Regex.Replace(fixedJson, @"^```json\s*", "", RegexOptions.IgnoreCase);
            fixedJson = Regex.Replace(fixedJson, @"\s*```$", "", RegexOptions.IgnoreCase);

            //  last pass of cleaning
            fixedJson = fixedJson.Replace("\\", "");

            return fixedJson;
        }
    }
}