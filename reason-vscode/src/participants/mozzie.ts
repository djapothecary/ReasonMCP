import * as vscode from 'vscode';
import * as crypto from 'crypto';
import { TextDecoder } from 'util';

export function registerMozzieParticipant(context: vscode.ExtensionContext) {
    console.log(('Mozzie is now avaialble'));

    let activeSessionId = crypto.randomUUID();

    const mozzieParticipant = vscode.chat.createChatParticipant(
        'mozzie.chat',
        async (
            request: vscode.ChatRequest,
            context: vscode.ChatContext,
            response: vscode.ChatResponseStream,
            token: vscode.CancellationToken
        ) => {
            response.progress('Mozzie is sorting papers...');

            if (context.history.length === 0) {
                activeSessionId = crypto.randomUUID();
            }

            try {
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

                const payload = {
                    prompt: request.prompt,
                    history: historyPayload
                };

                const attachedFiles: {
                    fileName: string,
                    content: string
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
                                fileName: fileName,
                                content: fileContent
                            });
                        } catch (err) {
                            console.error(`Failed to read attached file ${fileUri.path}`, err);
                        }
                    }
                }

                console.log("[TS PAYLOAD OUT]: " + JSON.stringify(payload, null, 2));
				//	This output provides the VSCode "pop-up" window
				// vscode.window.showInformationMessage("[TS PAYLOAD OUT]: " + JSON.stringify(payload, null, 2));

                const res = await fetch('http://127.0.0.1:5000/api/v1/mozzie', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        sessionId: activeSessionId,
                        agentId: 'mozzie',
                        role: 'user',
                        prompt: request.prompt,
                        history: historyPayload,
                        attachments: attachedFiles
                    })
                });

                if (!res.ok) {
                    throw new Error(`C# Backend returned Http ${res.status}`);
                }

                const data = await res.json() as any;

                response.markdown(data.response || "No response received from Reason backend.");
            } catch (error: any) {
                response.markdown(`*
                    Mozzie couldn't find and enrichment files to fence ... Error ${error.message}`);
            }
        }
    );

    context.subscriptions.push(mozzieParticipant);
}
