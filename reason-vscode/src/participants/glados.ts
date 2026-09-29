import * as vscode from 'vscode';
import * as crypto from 'crypto';
import { dispatchToAgentAsync } from '../components/agentDispatcher';

export function registerGladosParticipant(context: vscode.ExtensionContext) {
    console.log('GLaDOS is now available');

    let activeSessionId = crypto.randomUUID();

    const gladosParticipant = vscode.chat.createChatParticipant(
        'glados.chat',
        async (
            request: vscode.ChatRequest,
            context: vscode.ChatContext,
            response: vscode.ChatResponseStream,
            token: vscode.CancellationToken
        ) => {
            //  Handle session resets
            if (context.history.length === 0) {
                activeSessionId = crypto.randomUUID();
            }

            if (request.prompt === "") {
                response.markdown(`GLaDOS is for requirement...`);

                return;
            }

            response.progress('GLaDOS is running the prompt through Playbook Portals...');

            try {
                const answer = await dispatchToAgentAsync(
                    request,
                    context,
                    activeSessionId,
                    'glados',
                    'http://127.0.0.1:5000/api/v1/playbook/create'
                );

                response.markdown(answer);
            } catch(error: any) {
                response.markdown(`*
                    Are you even trying?
                    The Cake is a lie.
                    Error ${error.message}`);
            }
        }
    );

    context.subscriptions.push(gladosParticipant);
}