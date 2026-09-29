namespace ReasonMCP.Core.Utilities
{
    public class NeuralStyle
    {
        // --- Core User & Interface ---
        public const string Apoth = "[#c6ff00]";       // Electric Lime (The Human / Operator)

        // --- The ReasonMCP Agent Roster ---
        public const string Reason = "[#007fff]";      // Azure Blue (The Lead Architect)
        public const string Seraph = "[#ffd700]";      // Cyber Gold (The Judge / LLM Evaluator)
        public const string Mnemosyne = "[#00ffcc]";   // Teal/Aqua (The Archivist / Context Compression)
        public const string Mozzie = "[#39ff14]";      // Toxikk Green (Ingestion / AST Parsing / Heavy Machinery)
        public const string Esper = "[#ff1493]";       // Deep Neon Pink (Surgical Indexer / File Extraction)
        public const string Tank = "[#00ff00]";        // Phosphor Green (Operator / Payload Delivery)
        public const string Dozer = "[#ff4500]";       // Orange Red (Mechanic / Testing / Integrity)
        public const string Bella = "[#ffb347]";       // Warm Amber (Chaos Testing / Roleplay)
        public const string Trinity = "[#ffffff]";     // Ghost White (The Wakeup Call / VRAM Pre-Loader)

        // --- Functional Signals & States ---
        public const string Info = "[#00f3ff]";        // Cyan (Active Data / Network Link)
        public const string Success = "[#00ff41]";     // Matrix Green (Data Integrity / Commits)
        public const string Warning = "[#ff8c00]";     // Holo-Amber (System Warning / Latency)
        public const string Error = "[#ff003c]";       // Critical Red (Security Breach / Exception)

        // --- MLOps & Enrichment Domains ---
        public const string Neural = "[#8a2be2]";      // Plasma Purple (RAG / Vector Embedding Math)
        public const string Construct = "[#ff00a0]";   // Synth-Pink (Tool Execution / Plugins)
        public const string DataLake = "[#1e90ff]";    // Dodger Blue (SQLite / Dapper / Storage)
        //  Easter Egg name for Enrichment activities
        public const string Crucible = "[#ff00ff]";    // Neon Magenta (Synthetic Data Generation / JSONL)

        // --- Environmental/Dimmed ---
        public const string Timestamp = "[#9fd3e2]";   // Ice-Blue (Time/Telemetry)
        public const string Dim = "[#444444]";         // Dark Slate (Background/Verbose)
        public const string End = "[/]";               // Close Tag

        // --- Dynamic Logging Prefixes (Time Injected) ---
        public static string Time => $"[#444444]{DateTime.Now:HH:mm:ss.fff}[/] ";

        // --- Identity Prefixes ---
        public static string ApothLabel => $"{Time}{Apoth}[[[bold] APOTH [/]]]{End} ";
        public static string ReasonLabel => $"{Time}{Reason}[[[bold] REASON [/]]]{End} ";
        public static string SeraphLabel => $"{Time}{Seraph}[[[bold] SERAPH [/]]]{End} ";
        public static string EsperLabel => $"{Time}{Esper}[[[bold] ESPER [/]]]{End} ";
        public static string MozzieLabel => $"{Time}{Mozzie}[[[bold] MOZZIE [/]]]{End} ";
        public static string TrinityLabel => $"{Time}{Trinity}[[[bold] TRINITY [/]]]{End} "; // "Wake up..."

        // --- Functional Operation Prefixes ---
        public static string LogSys => $"{Time}{Info}[[[bold] SYS [/]]]{End} ";       // Startups / I/O
        public static string LogLink => $"{Time}{Info}[[[bold] LINK [/]]]{End} ";      // Minimal APIs / Endpoints
        public static string LogTool => $"{Time}{Construct}[[[bold] PLUGIN [/]]]{End} ";// Semantic Kernel Tools
        public static string LogStore => $"{Time}{DataLake}[[[bold] SQLITE [/]]]{End} "; // DB Reads/Writes

        // --- Enrichment & MLOps Prefixes ---
        public static string LogBrain => $"{Time}{Neural}[[[bold] VECTOR [/]]]{End} "; // Embedding generation
        public static string LogParse => $"{Time}{Mozzie}[[[bold] CHUNKING [/]]]{End} "; // AST Parsing / Semantics
        //  Easter Egg name for Enrichment activities
        public static string LogCrucible => $"{Time}{Crucible}[[[bold] SYNTH [/]]]{End} "; // JSONL writing / Turn creation
        public static string LogEval => $"{Time}{Seraph}[[[bold] EVAL [/]]]{End} ";      // LLM-as-a-Judge outputs

        // --- Security & Alerts ---
        public static string SecAlert => $"{Time}{Error}[[[bold] ! SECURITY ! [/]]]{End} "; // Blocked paths
        public static string WarnDrift => $"{Time}{Warning}[[[bold] ! DRIFT ! [/]]]{End} "; // Output Hallucination
    }
}