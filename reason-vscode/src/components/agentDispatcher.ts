import * as vscode from 'vscode';
import { TextDecoder } from 'util';
import { ExternalContextState } from '../extensions/sharedState';

export async function dispatchToAgentAsync(
    request: vscode.ChatRequest,
    context: vscode.ChatContext,
    sessionId: string,
    agentId: string,
    apiUrl: string
) : Promise<string> {
    //  1.  Parse Chat History
    const historyPayload: any[] = [];
    for (const turn of context.history) {
        if (turn instanceof vscode.ChatRequestTurn) {
            historyPayload.push({
                role: 'user',
                content: turn.prompt
            });
        } else if (turn instanceof vscode.ChatResponseTurn) {
            const responseText = turn.response.map(part => {
                if (part instanceof vscode.ChatResponseMarkdownPart) {
                    return part.value.value;
                }
                return '';
            }).join('');
            historyPayload.push({
                role: 'assistant',
                content: responseText
            });
        }
    }

    //  2.  Process Standard VS Code File References
    const attachedFiles: {
        fileName: string;
        content: string;
    }[] = [];
    for (const reference of request.references) {
        let fileUri: vscode.Uri | undefined;

        if (reference.value instanceof vscode.Uri) {
            fileUri = reference.value;
        } else if (reference.value instanceof vscode.Location) {
            fileUri = reference.value.uri;
        }

        if (fileUri) {
            try {
                const fileData = await vscode.workspace.fs.readFile(fileUri);
                const fileContent = new TextDecoder('utf-8').decode(fileData);
                const fileName = fileUri.path.split('/').pop() || "UnknownFile.txt";

                attachedFiles.push({
                    fileName,
                    content: fileContent
                });
            } catch (err) {
                console.error(`Failed to read attached file ${fileUri.path}`, err);
            }
        }
    }

    //  3.  Consume External Paths (the custom extension)
    const externallyAttachedFiles = ExternalContextState.consumePaths();

    //  4.  Build the final flat payload
    const payload = {
        sessionId: sessionId,
        agentId: agentId,
        role: 'user',
        prompt: request.prompt,
        history: historyPayload,
        attachedPaths: externallyAttachedFiles,
        attachments: attachedFiles
    };

    console.log(`[TS PAYLOAD OUT to ${agentId}]: ` + JSON.stringify(payload, null, 2));

    //  5.  Execute the fetch to C#
    const res = await fetch(apiUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });

    if (!res.ok) {
        throw new Error(`C# Backend returned Http ${res.status}`);
    }

    const data = await res.json() as any;
    return data.response || "No response received from backend.";
}
