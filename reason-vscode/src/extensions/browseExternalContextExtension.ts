import * as vscode from 'vscode';

// Global state tracker for paths captured via the UI tool during this session turn
let pendingExternalPaths: string[] = [];

// Define the tool class matching the LanguageModelTool interface
export class BrowseExternalContextTool implements vscode.LanguageModelTool<any> {
    async invoke(
        options: vscode.LanguageModelToolInvocationOptions<any>,
        _token: vscode.CancellationToken
    ): Promise<vscode.LanguageModelToolResult> { // Explicitly typed return signature

        // Trigger the native OS file picker
        const fileUris = await vscode.window.showOpenDialog({
            canSelectFiles: true,
            canSelectFolders: true,
            canSelectMany: false,
            openLabel: 'Attach to ReasonMCP',
            defaultUri: vscode.Uri.file('C:\\Source')
        });

        if (!fileUris || fileUris.length === 0) {
            // FIX: Return the LanguageModelToolResult object directly
            return new vscode.LanguageModelToolResult([
                new vscode.LanguageModelTextPart('No directory selected.')
            ]);
        }

        const selectedPath = fileUris[0].fsPath;

        // Track the path to bundle into your final C# fetch payload
        pendingExternalPaths.push(selectedPath);

        // FIX: Return the LanguageModelToolResult object directly
        // The framework processes TextParts to display them elegantly in the UI row
        return new vscode.LanguageModelToolResult([
            new vscode.LanguageModelTextPart(`Successfully attached: ${selectedPath}`)
        ]);
    }
}