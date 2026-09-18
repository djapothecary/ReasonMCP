namespace ReasonMCP.Core.Records
{
    public record FileAttachmentRecord(
        string FileName,
        string FilePath,
        string Content,
        long Size,
        string Message,
        bool IsError
    );
}