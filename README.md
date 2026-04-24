
# Reason MCP 🧠

**A high-performance, locally-hosted Model Context Protocol (MCP) server built in C# and .NET Core, designed for seamless integration with [Continue.dev](https://continue.dev).**

## 📖 The Story of "Reason"

In Neal Stephenson’s foundational cyberpunk novel *Snow Crash*, the protagonists are given an experimental, nuclear-powered, water-cooled gatling gun. Printed on the side of the weapon is the Latin phrase *Ultima Ratio Regum*—"The Last Argument of Kings." When diplomacy, hacking, and standard negotiations fail, the characters simply say: *"Let's listen to Reason."*

In the modern software engineering landscape, we are currently living through an AI hype cycle dominated by "vibe-coding." Developers are rushing brittle, unarchitected prototypes to production, relying on non-deterministic LLMs to write logic they don't fully understand.

I operate my career as the **"Adult in the Room"**. I believe that AI is an incredible productivity multiplier, but it must be governed by strict, deterministic architectural boundaries, type safety, and enterprise rigor.

When your AI coding assistant starts hallucinating bad architecture or forgetting your codebase's design patterns, it needs to be constrained by hard data, local RAG pipelines, and strict rules. It is time to make the AI listen to *Reason*.

## 🛠️ What is Reason MCP?

While much of the current Model Context Protocol (MCP) ecosystem is heavily dominated by Python and Node.js/TypeScript reference implementations, **Reason MCP** brings the protocol to the robust, strongly-typed world of the modern Microsoft ecosystem.

This project is a standalone C#/.NET Core executable that operates over standard input/output (`stdio`). It acts as a local RAG (Retrieval-Augmented Generation) backend and vector database engine for the Continue.dev VS Code extension.

By running Reason MCP, your local AI coding agents gain secure, air-gapped, and lightning-fast access to your proprietary codebase context, Architecture Decision Records (ADRs), and coding standards.

### Key Features
* **Built on .NET Core:** Utilizing modern C# features, primary constructors, and high-performance asynchronous streams.
* **Local Vector RAG:** Replaces basic text-search context with a robust vector embeddings pipeline (powered by Microsoft Semantic Kernel and SQLite Vector).
* **Zero-Trust & Air-Gapped:** Runs entirely on local hardware. No cloud egress, no vendor lock-in, and no API costs.
* **Continue.dev Native:** Plugs directly into the `config.yaml` of the Continue VS Code extension via the `stdio` transport layer.

## ⚙️ Tech Stack
* **Language:** C# 12 / .NET 10 (Preview)
* **AI Orchestration:** Microsoft Semantic Kernel
* **Protocol:** Model Context Protocol (MCP) JSON-RPC over `stdio`
* **Data Layer:** SQLite / Vector Store

## 🚀 Getting Started

### 1. Build the Project
Clone the repository and build the C# console application.
```bash
git clone https://github.com/djapothecary/ReasonMCP.git
cd ReasonMCP
dotnet build -c Release
```

### 2. Configure Continue.dev
Open your Continue.dev `config.yaml` or `config.json` (located in your `%APPDATA%\.continue\` or `~/.continue/` folder) and add Reason to your `mcpServers` array:

```yaml
mcpServers:
  - name: Reason
    command: dotnet
    args:
      - "run"
      - "--project"
      - "C:/Absolute/Path/To/Your/ReasonMCP.csproj"
      - "-c"
      - "Release"
```
*(Alternatively, you can point the `command` directly to the compiled `.exe` or `.dll` file).*

### 3. Let it Reason
Once configured, reload your VS Code window. The Continue.dev extension will automatically spin up the Reason MCP server in the background. You can now use tools provided by Reason directly in your chat context!

## 🤝 Contributing
As a champion of Clean Architecture and "Adult in the Room" engineering, contributions, pull requests, and discussions regarding C# design patterns are highly encouraged.
```

