import * as vscode from 'vscode';
import * as crypto from 'crypto';
import { dispatchToAgentAsync } from '../components/agentDispatcher';

export function registerReasonParticipant(context: vscode.ExtensionContext) {
    console.log(('ReasonMCP client extension is now active!'));

	let activeSessionId = crypto.randomUUID();

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

			if (context.history.length === 0) {
				activeSessionId = crypto.randomUUID();
			}

            //  Handle empty prompt instructions
            if (request.prompt === "") {
                response.markdown(`What are we building today, Apoth?`);
                return;
            }

			//	2.	UI Feedback: Shows a progress indicator
			response.progress('Reason is thinking ...');

			try {
				//  one 'line' to handle all the payload assembly
				//  and API communication
				const answer = await dispatchToAgentAsync(
					request,
					context,
					activeSessionId,
					'esper',
					'http://127.0.0.1:5000/api/v1/chat'
				);

				response.markdown(answer);
			} catch (error: any) {
				response.markdown(`**Architectural Failure:** Unable to reach Reason backend. Is Kestrel running on Port 5000? \n\nError: ${error.message}`);
			}
		}
	);

	//	Register it to the extension context
	context.subscriptions.push(reasonParticipant);
}
