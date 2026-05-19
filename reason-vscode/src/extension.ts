import * as vscode from 'vscode';

export function activate(context: vscode.ExtensionContext) {
	console.log('ReasonMCP client extension is now active!');

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
				//	3.	Send the HTTP Post to the C# backend
				//	Using native fetch commands
				const res = await fetch('http://127.0.0.1:5000/api/v1/chat', {
					method: 'POSt',
					headers: {
						'Content-Type': 'application/json'
					},
					body: JSON.stringify({
						prompt: request.prompt
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
