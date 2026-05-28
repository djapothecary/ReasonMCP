using System;

namespace ReasonMCP.Enums
{
    public enum IngestionStatus
    {
        Pending = 0,               //   Waiting to be picked up
        Converting = 1,            //   Currently reading/parsing/chunking
        ConversionFailed = 2,      //   Conversion failed
        PendingIngestion = 3,      //   Converted successfully, waiting to hit the LLM/Vector DB
        Ingesting = 4,             //   Currently generating embeddings and writing to SQLite
        IngestionFailed = 5,       //   Ingestion failed
        Complete = 6,              //   100% Done
        Failed = 7                 //   Crashed (Check ErrorMessage column)

    }
}