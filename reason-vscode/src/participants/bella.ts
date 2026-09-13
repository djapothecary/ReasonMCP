import * as vscode from 'vscode';
import * as crypto from 'crypto';
import { dispatchToAgentAsync } from '../components/agentDispatcher';

export function registerBellaParticipant(context: vscode.ExtensionContext) {
	console.log('Bella is back from chasing squirrels');

    //	1.	Hold the active session ID in memory on the client
	let activeSessionId = crypto.randomUUID();

	const bellaParticipant = vscode.chat.createChatParticipant(
        'bella.chat',
        async (
            request: vscode.ChatRequest,
            context: vscode.ChatContext,
            response: vscode.ChatResponseStream,
            token: vscode.CancellationToken
        ) => {

			//  Handle sessions resets
			if (context.history.length === 0) {
				activeSessionId = crypto.randomUUID();
			}

			//  Handle empty prompt instructions
            if (request.prompt === "") {
                response.markdown(`*barks* Woof! I'm ready to fetch anything!`);
                return;
            }

            response.progress('Bella is sniffing for answers...');

            try {

				//  one 'line' to handle all the payload assembly
                //  and API communication
                const answer = await dispatchToAgentAsync(
                    request,
                    context,
                    activeSessionId,
                    'bella',
                    'http://127.0.0.1:5000/api/v1/chat'
                );

                response.markdown(answer);
			} catch (error: any) {
				response.markdown(`*whimpers* Woof! I couldn't find the backend ... \n\nError: ${error.message}`);
			}
        }
    );

	// Give Bella a custom icon if you want!
    // bellaParticipant.iconPath = vscode.Uri.joinPath(context.extensionUri, 'images', 'dog.png');

    context.subscriptions.push(bellaParticipant);
}
