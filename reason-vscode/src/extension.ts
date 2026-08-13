import * as vscode from 'vscode';
import { spawn, ChildProcess } from 'child_process';
import { registerReasonParticipant } from './participants/reason';
import { registerBellaParticipant } from './participants/bella';
import { registerMozzieParticipant } from './participants/mozzie';

let backendProcess: ChildProcess | null = null;

export function activate(context: vscode.ExtensionContext) {
    //  1.  Silently start the C# Kestrel server in the background
    const serverPath = 'C:\\Tools\\ReasonMCPServer\\ReasonMCP.exe';
    backendProcess = spawn(serverPath, [], { detached: false });

    backendProcess.stdout?.on('data', (data: any) => console.log(`ReasonBackend: ${data}`));
    backendProcess.stderr?.on('data', (data: any) => console.error(`ReasonBackend Error: ${data}`));

    console.log('ReasonMCP Extension Suite is now active!');

    //  Bootstrap the agents
    registerReasonParticipant(context);
    registerBellaParticipant(context);
}

export function deactivate() {
    //  Allow VS Code to handle cleanup automatically via context

    //  kill the C# server when VS Code closes so it doesn't leak memory
    if (backendProcess) {
        backendProcess.kill();
    }
}
