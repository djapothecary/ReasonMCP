using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using ReasonMCP.Core.Utilities;
using Spectre.Console;

namespace ReasonMCP.Enrichment.Dispatchers
{
    public class EnrichmentDispatcher
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EnrichmentDispatcher> _logger;

        public EnrichmentDispatcher(
            IServiceScopeFactory scopeFactory,
            ILogger<EnrichmentDispatcher> logger
        )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task RunAsync(
            CancellationToken cancellationToken = default
        )
        {
            AnsiConsole.MarkupLine("[lime]Initializing ReasonMCP Enrichment processes ...[/]");

            await GradientDisplay.DisplayCyberpunkStartupAsync();

            while (!cancellationToken.IsCancellationRequested)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[bold Yellow]ReasonMCP Enrichment Flight Deck[/]")
                        .AddChoices(
                            "1. Run Codebase Scan",
                            "2. Run Documents Scan",
                            "3. Run Reference Scan",
                            "4. Exit Flight Deck"
                        )
                );

                if (choice == "4. Exit Flight Deck")
                {
                    AnsiConsole.MarkupLine("[OrangeRed1]Exiting Enrichment ...[/]");
                    break;
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    try
                    {
                        if (choice == "1. Run Codebase Scan")
                        {

                        }
                        else if (choice == "2. Run Documents Scan")
                        {

                        }
                        else if (choice == "3. Run Reference Scan")
                        {

                        }
                        else if (choice == "4. Exit Flight Deck")
                        {

                        }
                        else
                        {
                            //  safety bailout
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "A fatal error occured during the workflow execution.");
                        AnsiConsole.MarkupLine($"[Red3_1]Workflow Crashed: {ex.Message}[/]");
                        // Console.WriteLine(ex.ToString(), ex.Message);
                    }
                }
            }
        }
    }
}