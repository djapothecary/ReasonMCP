import * as vscode from 'vscode';
import * as crypto from 'crypto';
import { dispatchToAgentAsync } from '../components/agentDispatcher';

export function registerTankParticipant(context: vscode.ExtensionContext) {
    console.log('Tank is now available');

    let activeSessionId = crypto.randomUUID();

    const tankParticipant = vscode.chat.createChatParticipant(
        'tank.chat',
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
                response.markdown(`Tank is ready to load a Playbook...`);

                return;
            }

            response.progress('Tank is patching in... loading playbook...');

            try {
                const answer = await dispatchToAgentAsync(
                    request,
                    context,
                    activeSessionId,
                    'tank',
                    'http://127.0.0.1:5000/api/v1/playbook/load'
                );

                response.markdown(answer);
            } catch (error: any) {
                response.markdown(`*
                    Tank couldn't patch in ... Error ${error.message}`);
            }
        }
    );

    context.subscriptions.push(tankParticipant);
}