import * as vscode from 'vscode';
import * as crypto from 'crypto';
import { dispatchToAgentAsync } from '../components/agentDispatcher';

export function registerEsperParticipant(context: vscode.ExtensionContext) {
    console.log('Esper is now available');

    let activeSessionId = crypto.randomUUID();

    const esperParticipant = vscode.chat.createChatParticipant(
        'esper.chat',
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
                response.markdown(`What do you need me to enhance, Apoth?`);
                return;
            }

            response.progress('Esper is extracting context...');

            try {
                //  one 'line' to handle all the payload assembly
                //  and API communication
                const answer = await dispatchToAgentAsync(
                    request,
                    context,
                    activeSessionId,
                    'esper',
                    'http://127.0.0.1:5000/api/v1/external/scan'
                );

                response.markdown(answer);
            } catch (error: any) {
                //  Agent-specific error message
                response.markdown(`[System Error]: Esper failed to extract the payload.  Details: ${error.message}`);
            }
        }
    );

    context.subscriptions.push(esperParticipant);
}