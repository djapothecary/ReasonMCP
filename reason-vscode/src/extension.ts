import * as vscode from 'vscode';

export function activate(context: vscode.ExtensionContext) {
	console.log(('ReasonMCP client extension is now active!'));

	//	This output provides the VSCode "pop-up" window
	// vscode.window.showInformationMessage('ReasonMCP client extension is now active!');

	//	1.	Create the Chat Participant using the ID from package.json
	const reasonParticipant = vscode.chat.createChatParticipant(
		'reasonmcp.chat',
		async (
			request: vscode.ChatRequest,
			context: vscode.ChatContext,
			response: vscode.ChatResponseStream,
			token: vscode.CancellationToken
		) => {
			//	2.	UI Feedback: Shows a progress indicator
			response.progress('Reason is thinking ...');

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
				response.markdown(`**Architectural Failure:** Unable to reach Reason backend. Is Kestrel running on Port 5000? \n\nError: ${error.message}`);
			}
		}
	);

	//	Register it to the extension context
	context.subscriptions.push(reasonParticipant);
}

export function deactivate() {}
