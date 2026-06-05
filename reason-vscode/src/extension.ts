import * as vscode from 'vscode';
import { registerReasonParticipant } from './participants/reason';
import { registerBellaParticipant } from './participants/bella';

export function activate(context: vscode.ExtensionContext) {
    console.log('ReasonMCP Extension Suite is now active!');

    //  Bootstrap the agents
    registerReasonParticipant(context);
    registerBellaParticipant(context);
}

export function deactivate() {
    //  Allow VS Code to handle cleanup automatically via conte
}
