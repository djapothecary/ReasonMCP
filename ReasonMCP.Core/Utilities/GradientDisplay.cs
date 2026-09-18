using System.Text.RegularExpressions;
using ReasonMCP.Core.Utilities;
using Spectre.Console;

namespace ReasonMCP.Core.Utilities
{
    public static class GradientDisplay
    {
        // --- Core Math & Parsing ---

        public static Color GetGradientColor(
            Color start,
            Color end,
            float factor
        )
        {
            // Clamp factor between 0 and 1 just to be mathematically safe
            factor = Math.Max(0, Math.Min(1, factor));

            int r = (int)(start.R + (end.R - start.R) * factor);
            int g = (int)(start.G + (end.G - start.G) * factor);
            int b = (int)(start.B + (end.B - start.B) * factor);

            return new Color((byte)r, (byte)g, (byte)b);
        }

        /// <summary>
        /// Safely extracts the hex value from NeuralStyle constants (e.g., "[#ff00ff]" -> Color)
        /// </summary>
        private static Color ParseNeuralColor(
            string neuralStyleColor
        )
        {   // Strip out the brackets and the hash to get the raw hex
            var hex = neuralStyleColor.Replace("[", "").Replace("]", "").Replace("#", "");
            return Color.FromHex(hex);
        }


        // --- Gradient Execution Methods ---

        /// <summary>
        /// Standard 2-Color Horizontal Gradient using Spectre Colors
        /// </summary>
        public static async Task MarkupHorizontalGradientAsync(
            string text,
            Color start,
            Color end,
            int delayMs = 2
        )
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            for (int i = 0; i < text.Length; i++)
            {
                float factor = text.Length == 1 ? 0 : i / (float)(text.Length - 1);
                Color color = GetGradientColor(start, end, factor);

                string escapedChar = Markup.Escape(text[i].ToString());
                await Task.Delay(delayMs);
                AnsiConsole.Markup($"[#{color.ToHex()}]{escapedChar}[/]");
            }
            AnsiConsole.WriteLine();
        }

        /// <summary>
        /// 2-Color Gradient specifically taking NeuralStyle string constants
        /// Example: MarkupNeuralGradientAsync("System Online", NeuralStyle.Info, NeuralStyle.Neural)
        /// </summary>
        public static async Task MarkupNeuralGradientAsync(
            string text,
            string neuralStart,
            string neuralEnd,
            int delayMs = 2
        )
        {
            Color start = ParseNeuralColor(neuralStart);
            Color end = ParseNeuralColor(neuralEnd);

            await MarkupHorizontalGradientAsync(text, start, end, delayMs);
        }

        /// <summary>
        /// The "Toxikk" 3-Color Gradient for premium Cyberpunk aesthetics.
        /// Interpolates from Start -> Mid -> End.
        /// </summary>
        public static async Task MarkupToxikkGradientAsync(
            string text,
            string neuralStart,
            string neuralMid,
            string neuralEnd,
            int delayMs = 2
        )
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            Color start = ParseNeuralColor(neuralStart);
            Color mid = ParseNeuralColor(neuralMid);
            Color end = ParseNeuralColor(neuralEnd);

            for (int i = 0; i < text.Length; i++)
            {
                float factor = text.Length == 1 ? 0 : i / (float)(text.Length - 1);
                Color color;

                // First half of the string: Start -> Mid
                if (factor <= 0.5f)
                {
                    float normalizedFactor = factor * 2f; // Scale 0.0-0.5 to 0.0-1.0
                    color = GetGradientColor(start, mid, normalizedFactor);
                }
                // Second half of the string: Mid -> End
                else
                {
                    float normalizedFactor = (factor - 0.5f) * 2f; // Scale 0.5-1.0 to 0.0-1.0
                    color = GetGradientColor(mid, end, normalizedFactor);
                }

                string escapedChar = Markup.Escape(text[i].ToString());
                await Task.Delay(delayMs);
                AnsiConsole.Markup($"[#{color.ToHex()}]{escapedChar}[/]");
            }
            AnsiConsole.WriteLine();
        }


        // --- Startup & Display Sequences ---

        public static async Task DisplayCyberpunkStartupAsync()
        {
            var figlet = new FigletText("Project Yuki").LeftJustified();
            string[] lines = figlet.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            Color startColor = Color.Cyan1;
            Color endColor = Color.HotPink;

            AnsiConsole.WriteLine();

            // Vertical Gradient for Figlet
            for (int i = 0; i < lines.Length; i++)
            {
                float factor = i / (float)lines.Length;
                Color currentColor = GetGradientColor(startColor, endColor, factor);
                string escapedLine = Markup.Escape(lines[i]);

                AnsiConsole.MarkupLine($"[#{currentColor.ToHex()}]{escapedLine}[/]");
                await Task.Delay(50);
            }

            AnsiConsole.Write(new Rule().RuleStyle($"#{endColor.ToHex()} bold"));

            AnsiConsole.MarkupLine($"{NeuralStyle.Info}[[SESSION STARTED]]:  [white]{DateTime.Now:HH:mm:ss}[/]{NeuralStyle.End}");
            AnsiConsole.MarkupLine($"{NeuralStyle.Reason}[[OLLAMA STATUS]]:   [bold green]CONNECTED[/]{NeuralStyle.End}");

            // Using the new Neural Gradient with your styles!
            await MarkupNeuralGradientAsync("Initializing Secure Connection to Santa Fe Node...", NeuralStyle.Info, NeuralStyle.Neural, 5);

            await DisplayGradiantWelcomeAsync();
            AnsiConsole.WriteLine();
        }

        private static async Task DisplayGradiantWelcomeAsync()
        {
            string[] lines = DisplayWelcomeArt().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                // The 3-Color Toxikk gradient looks amazing on ASCII art!
                await MarkupToxikkGradientAsync(line, NeuralStyle.Info, NeuralStyle.Reason, NeuralStyle.Neural, 1);
            }

            AnsiConsole.Write(new Rule().RuleStyle($"#ff00ff bold"));
            AnsiConsole.WriteLine();
        }

        private static string DisplayWelcomeArt()
        {
            return @"
    *****************************************************************************************************
    *                                                                                                   *
    *   $$$$$$$\                                                    $$\      $$\  $$$$$$\  $$$$$$$\     *
    *   $$  __$$\                                                   $$$\    $$$ |$$  __$$\ $$  __$$\    *
    *   $$ |  $$ | $$$$$$\   $$$$$$\   $$$$$$$\  $$$$$$\  $$$$$$$\  $$$$\  $$$$ |$$ /  \__|$$ |  $$ |   *
    *   $$$$$$$  |$$  __$$\  \____$$\ $$  _____|$$  __$$\ $$  __$$\ $$\$$\$$ $$ |$$ |      $$$$$$$  |   *
    *   $$  __$$< $$$$$$$$ | $$$$$$$ |\$$$$$$\  $$ /  $$ |$$ |  $$ |$$ \$$$  $$ |$$ |      $$  ____/    *
    *   $$ |  $$ |$$   ____|$$  __$$ | \____$$\ $$ |  $$ |$$ |  $$ |$$ |\$  /$$ |$$ |  $$\ $$ |         *
    *   $$ |  $$ |\$$$$$$$\ \$$$$$$$ |$$$$$$$  |\$$$$$$  |$$ |  $$ |$$ | \_/ $$ |\$$$$$$  |$$ |         *
    *   \__|  \__| \_______| \_______|\_______/  \______/ \__|  \__|\__|     \__| \______/ \__|         *
    *                                                                                                   *
    *                                           -- ReasonMCP 2026 --                                    *
    *                                                                                                   *
    *****************************************************************************************************";
        }
    }
}