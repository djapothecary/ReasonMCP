import * as vscode from 'vscode';

export function registerBellaParticipant(context: vscode.ExtensionContext) {

    const bellaParticipant = vscode.chat.createChatParticipant(
        'bella.chat',
        async (
            request: vscode.ChatRequest,
            context: vscode.ChatContext,
            response: vscode.ChatResponseStream,
            token: vscode.CancellationToken
        ) => {
            response.progress('Bella is sniffing for answers...');

            try {
				//	prepare the chat history with proper role mapping
				const historyPayload: any[] = [];

				for (const turn of context.history) {
					if (turn instanceof vscode.ChatRequestTurn) {
						//	it's a message from the user
						historyPayload.push({
							role: 'user',
							content: turn.prompt
						});
					} else if (turn instanceof vscode.ChatResponseTurn) {
						//	it's a message from Reason.  The response is an array of "parts".
						//	we map them and extract the Markdown text.
						const responseText = turn.response.map(part => {
							if (part instanceof vscode.ChatResponseMarkdownPart) {
								return part.value.value; //	the actual string content
							}
							return '';
						}).join('');

						historyPayload.push({
							role: 'assistant',
							content: responseText
						});
					}
				}

				//	3.	Send the HTTP Post to the C# backend
				//	Using native fetch commands
				const payload = {
					prompt: request.prompt,
					history: historyPayload
				};

				console.log("[TS PAYLOAD OUT]: " + JSON.stringify(payload, null, 2));
				//	This output provides the VSCode "pop-up" window
				// vscode.window.showInformationMessage("[TS PAYLOAD OUT]: " + JSON.stringify(payload, null, 2));

				const res = await fetch('http://127.0.0.1:5000/api/v1/chat', {
					method: 'POST',
					headers: {
						'Content-Type': 'application/json'
					},
					body: JSON.stringify({
                        agentId: 'bella',
						role: 'user', // this will alswys be the user sending a prompt to the API
						prompt: request.prompt,
						history: historyPayload
					})
				});

				if (!res.ok) {
					throw new Error(`C# Backend returned HTTP ${res.status}`);
				}

				const data = await res.json() as any;

				//	4.	Stream the response directly into the VS Code chat window
				response.markdown(data.response || "No response received from Reason backend.");
			} catch (error: any) {
				response.markdown(`*
                    whimpers* Woof! I couldn't find the backend ... \n\nError: ${error.message}`);
			}
        }
    );

	// Give Bella a custom icon if you want!
    // bellaParticipant.iconPath = vscode.Uri.joinPath(context.extensionUri, 'images', 'dog.png');

    context.subscriptions.push(bellaParticipant);

}