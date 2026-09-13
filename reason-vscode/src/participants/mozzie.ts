import * as vscode from 'vscode';
import * as crypto from 'crypto';
import { dispatchToAgentAsync } from '../components/agentDispatcher';

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

            //  Handle sessions resets
            if (context.history.length === 0) {
                activeSessionId = crypto.randomUUID();
            }

            if (request.prompt === "") {
                response.markdown(`No prompt was provided. Please provide one of the following options:
                    1. **[Codebase Scan]** This will trigger indexing of the entire codebase.
                    2. **[Documents Scan]** This will trigger indexing of the Documents directories.
                    3. **[Reference Scan]** This will trigger indexing of the References directories.
                    4. **[Attached File]** This will trigger the indexing of a specific attached file.
                    5. Provide a message to chat with Mozzie.

                    --- or ---

                    👉 **[Click Here to Browse an External File/Folder Layout](command:browseExternalContext)**`);
                return; // Exit early so it doesn't fire an empty fetch request
            }

            response.progress('Mozzie is sorting papers...');

            try {
                            //  one 'line' to handle all the payload assembly
                            //  and API communication
                            const answer = await dispatchToAgentAsync(
                                request,
                                context,
                                activeSessionId,
                                'esper',
                                'http://127.0.0.1:5000/api/v1/workspace/queue/scan'
                            );

                            response.markdown(answer);
            } catch (error: any) {
                response.markdown(`*
                    Mozzie couldn't find and enrichment files to fence ... Error ${error.message}`);
            }
        }
    );

    context.subscriptions.push(mozzieParticipant);
}
