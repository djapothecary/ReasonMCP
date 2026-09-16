using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using MimeKit;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Utilities
{
    public class MhtmlConverterUtility : IMhtmlConverterUtility
    {
        private readonly ILogger<MhtmlConverterUtility> _logger;

        public MhtmlConverterUtility(
            ILogger<MhtmlConverterUtility> logger
        )
        {
            _logger = logger;
        }

        public async Task<string> ConvertToTextAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("[CONVERSION_ERROR] SOURCE MHTML file not found: {filePath}", filePath);
                return $"[CONVERSION_ERROR] SOURCE MHTML file not found: {filePath}";
            }

            try
            {
                //  1.  Extract HTML from the MHTML MIM envelope
                // string? rawHtml = await ExtractHtmlFromMhtmlAsync(filePath, cancellationToken);
                string? rawHtml = await ExtractHtmlFromMhtmlAsync(filePath, cancellationToken);

                if (string.IsNullOrWhiteSpace(rawHtml))
                {
                    _logger.LogWarning("[CONVERSION_ERROR] No HTML body found in MHTML file: {filePath}", filePath);
                    return $"[CONVERSION_ERROR] No HTML body found in MHTML file: {filePath}";
                }

                //  2.  Strip HTML and conver to plain text
                string plainText = ConvertHtmlToPlainText(rawHtml);

                //  3.  Write deterministically to target file
                //  Using UTF8 without BOM is generally best for AI Ingestion pipelines
                return plainText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CONVERSION_ERROR] Failed to convert MHTML file: {filePath}", filePath);
                return $"[CONVERSION_ERROR] Failed to convert MHTML file: {filePath}";
            }
        }

        public async Task<string?> ExtractHtmlFromMhtmlAsync(string filePath, CancellationToken cancellationToken)
        {
            //  Using FileStream ensures we stream the file off disk rather than loading it all into a byte array
            await using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.Asynchronous
            );

            //  MimeKit is highly optimized for parsing Multipart messages
            using var message = await MimeMessage.LoadAsync(stream, cancellationToken);

            //  MimeMessage.HtmlBody automatically searches the multipart tree for the text/html payload
            return message.HtmlBody ?? message.TextBody;
        }

        private string ConvertHtmlToPlainText(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            //  1.   Remove unwanted nodes that pollute RAG Context
            doc.DocumentNode.Descendants()
                .Where(
                    n => n.Name == "script" ||
                    n.Name == "style" ||
                    n.Name == "nav" ||
                    n.Name == "footer" ||
                    n.Name == "noscript" ||
                    n.Name == "head")
                .ToList()
                .ForEach(n => n.Remove());

            //  2.  Extract inner text safely
            //  HtmlEntity.DeEntitize converts things like &amp; to & and &nbsp; to spaces
            var text = HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);

            //  3.  Cleanup Excessive whitespace.
            //  when we strip HTML, we often get massive blocks of empty lines
            return CleanupWhitespace(text);
        }

        private static string CleanupWhitespace(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var sb = new StringBuilder(text.Length);
            bool lastWasWhitespace = false;

            foreach (var c in text)
            {
                // 1. Guard clause: If it is a normal character OR a structural newline,
                // we always append it exactly as is and reset the flag.
                if (!char.IsWhiteSpace(c) || c is '\n' or '\r')
                {
                    sb.Append(c);
                    lastWasWhitespace = false;
                    continue;
                }

                // 2. We now know definitively that the character IS whitespace (spaces, tabs, etc.)
                // and IS NOT a newline. We only append a single space if we haven't just done so.
                if (!lastWasWhitespace)
                {
                    sb.Append(' ');
                    lastWasWhitespace = true;
                }
            }

            return sb.ToString().Trim();
        }

    }
}