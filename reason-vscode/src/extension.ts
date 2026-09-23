import * as vscode from 'vscode';
import { spawn, ChildProcess } from 'child_process';
import { registerReasonParticipant } from './participants/reason';
import { registerBellaParticipant } from './participants/bella';
import { registerMozzieParticipant } from './participants/mozzie';
import { BrowseExternalContextTool } from './extensions/browseExternalContextExtension';
import { ExternalContextState } from './extensions/sharedState';
import { registerEsperParticipant } from './participants/esper';
import { registerTankParticipant } from './participants/tank';

let backendProcess: ChildProcess | null = null;

export async function activate(context: vscode.ExtensionContext) {
    //  1.  Silently start the C# Kestrel server in the background
    const serverPath = 'C:\\Tools\\ReasonMCPServer\\ReasonMCP.exe';
    backendProcess = spawn(serverPath, [], { detached: false });

    backendProcess.stdout?.on('data', (data: any) => console.log(`ReasonBackend: ${data}`));
    backendProcess.stderr?.on('data', (data: any) => console.error(`ReasonBackend Error: ${data}`));

    console.log('ReasonMCP Extension Suite is now active!');

    let pendingExternalPaths: string[] = [];

    let browseExternalContext = vscode.commands.registerCommand('browseAndAttachExternal', async () => {
        const fileUris = await vscode.window.showOpenDialog({
            canSelectFiles: true,
            canSelectFolders: true,
            canSelectMany: true,
            openLabel: 'Attach to ReasonMCP',
            defaultUri: vscode.Uri.file('C:\\Source')
        });

        if (fileUris && fileUris.length > 0) {
            const selectedPath = fileUris[0].fsPath;

            //  Save to the shared state for the C# backend payload
            ExternalContextState.addPath(selectedPath);

            //  UX Hack: Paste the 'selectedPath' into the text box for the user
            const originalClipboard = await vscode.env.clipboard.readText(); // Save their current clipboard
            await vscode.env.clipboard.writeText(selectedPath);              // Put the path in
            await vscode.commands.executeCommand("editor.action.clipboardPasteAction"); // Paste it
            await vscode.env.clipboard.writeText(originalClipboard);         // Restore their clipboard

            // Provide visual feedback inside the editor since we can't force a badge into the text box
            vscode.window.showInformationMessage(`📎 Attached external context: ${selectedPath}`);
        }
    });

    //  1. Register your file-browsing tool class via the language model namespace
    context.subscriptions.push(
        vscode.lm.registerTool(
            'browseExternalContext',
            new BrowseExternalContextTool()
        )
    );

    context.subscriptions.push(browseExternalContext);

    //  Bootstrap the agents
    registerReasonParticipant(context);
    registerBellaParticipant(context);
    registerMozzieParticipant(context);
    registerEsperParticipant(context);
    registerTankParticipant(context);
}

export async function deactivate() {
    //  Allow VS Code to handle cleanup automatically via context

    //  kill the C# server when VS Code closes so it doesn't leak memory
    if (backendProcess) {
        backendProcess.kill();
    }
}
