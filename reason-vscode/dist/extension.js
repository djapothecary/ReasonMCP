/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ([
/* 0 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.activate = activate;
exports.deactivate = deactivate;
const vscode = __importStar(__webpack_require__(1));
const child_process_1 = __webpack_require__(2);
const reason_1 = __webpack_require__(3);
const bella_1 = __webpack_require__(8);
const mozzie_1 = __webpack_require__(9);
const browseExternalContextExtension_1 = __webpack_require__(10);
const sharedState_1 = __webpack_require__(7);
const esper_1 = __webpack_require__(11);
let backendProcess = null;
async function activate(context) {
    //  1.  Silently start the C# Kestrel server in the background
    const serverPath = 'C:\\Tools\\ReasonMCPServer\\ReasonMCP.exe';
    backendProcess = (0, child_process_1.spawn)(serverPath, [], { detached: false });
    backendProcess.stdout?.on('data', (data) => console.log(`ReasonBackend: ${data}`));
    backendProcess.stderr?.on('data', (data) => console.error(`ReasonBackend Error: ${data}`));
    console.log('ReasonMCP Extension Suite is now active!');
    let pendingExternalPaths = [];
    let browseExternalContext = vscode.commands.registerCommand('browseAndAttachExternal', async () => {
        const fileUris = await vscode.window.showOpenDialog({
            canSelectFiles: true,
            canSelectFolders: true,
            canSelectMany: false,
            openLabel: 'Attach to ReasonMCP',
            defaultUri: vscode.Uri.file('C:\\Source')
        });
        if (fileUris && fileUris.length > 0) {
            const selectedPath = fileUris[0].fsPath;
            //  Save to the shared state for the C# backend payload
            sharedState_1.ExternalContextState.addPath(selectedPath);
            //  UX Hack: Paste the 'selectedPath' into the text box for the user
            const originalClipboard = await vscode.env.clipboard.readText(); // Save their current clipboard
            await vscode.env.clipboard.writeText(selectedPath); // Put the path in
            await vscode.commands.executeCommand("editor.action.clipboardPasteAction"); // Paste it
            await vscode.env.clipboard.writeText(originalClipboard); // Restore their clipboard
            // Provide visual feedback inside the editor since we can't force a badge into the text box
            vscode.window.showInformationMessage(`📎 Attached external context: ${selectedPath}`);
        }
    });
    //  1. Register your file-browsing tool class via the language model namespace
    context.subscriptions.push(vscode.lm.registerTool('browseExternalContext', new browseExternalContextExtension_1.BrowseExternalContextTool()));
    context.subscriptions.push(browseExternalContext);
    //  Bootstrap the agents
    (0, reason_1.registerReasonParticipant)(context);
    (0, bella_1.registerBellaParticipant)(context);
    (0, mozzie_1.registerMozzieParticipant)(context);
    (0, esper_1.registerEsperParticipant)(context);
}
async function deactivate() {
    //  Allow VS Code to handle cleanup automatically via context
    //  kill the C# server when VS Code closes so it doesn't leak memory
    if (backendProcess) {
        backendProcess.kill();
    }
}


/***/ }),
/* 1 */
/***/ ((module) => {

module.exports = require("vscode");

/***/ }),
/* 2 */
/***/ ((module) => {

module.exports = require("child_process");

/***/ }),
/* 3 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.registerReasonParticipant = registerReasonParticipant;
const vscode = __importStar(__webpack_require__(1));
const crypto = __importStar(__webpack_require__(4));
const agentDispatcher_1 = __webpack_require__(5);
function registerReasonParticipant(context) {
    console.log(('ReasonMCP client extension is now active!'));
    let activeSessionId = crypto.randomUUID();
    //	This output provides the VSCode "pop-up" window
    // vscode.window.showInformationMessage('ReasonMCP client extension is now active!');
    //	1.	Create the Chat Participant using the ID from package.json
    const reasonParticipant = vscode.chat.createChatParticipant('reasonmcp.chat', async (request, context, response, token) => {
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
            const answer = await (0, agentDispatcher_1.dispatchToAgentAsync)(request, context, activeSessionId, 'esper', 'http://127.0.0.1:5000/api/v1/chat');
            response.markdown(answer);
        }
        catch (error) {
            response.markdown(`**Architectural Failure:** Unable to reach Reason backend. Is Kestrel running on Port 5000? \n\nError: ${error.message}`);
        }
    });
    //	Register it to the extension context
    context.subscriptions.push(reasonParticipant);
}


/***/ }),
/* 4 */
/***/ ((module) => {

module.exports = require("crypto");

/***/ }),
/* 5 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.dispatchToAgentAsync = dispatchToAgentAsync;
const vscode = __importStar(__webpack_require__(1));
const util_1 = __webpack_require__(6);
const sharedState_1 = __webpack_require__(7);
async function dispatchToAgentAsync(request, context, sessionId, agentId, apiUrl) {
    //  1.  Parse Chat History
    const historyPayload = [];
    for (const turn of context.history) {
        if (turn instanceof vscode.ChatRequestTurn) {
            historyPayload.push({
                role: 'user',
                content: turn.prompt
            });
        }
        else if (turn instanceof vscode.ChatResponseTurn) {
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
    //  2.  Process Standard VS Code File References
    const attachedFiles = [];
    for (const reference of request.references) {
        let fileUri;
        if (reference.value instanceof vscode.Uri) {
            fileUri = reference.value;
        }
        else if (reference.value instanceof vscode.Location) {
            fileUri = reference.value.uri;
        }
        if (fileUri) {
            try {
                const fileData = await vscode.workspace.fs.readFile(fileUri);
                const fileContent = new util_1.TextDecoder('utf-8').decode(fileData);
                const fileName = fileUri.path.split('/').pop() || "UnknownFile.txt";
                attachedFiles.push({
                    fileName,
                    content: fileContent
                });
            }
            catch (err) {
                console.error(`Failed to read attached file ${fileUri.path}`, err);
            }
        }
    }
    //  3.  Consume External Paths (the custom extension)
    const externallyAttachedFiles = sharedState_1.ExternalContextState.consumePaths();
    //  4.  Build the final flat payload
    const payload = {
        sessionId: sessionId,
        agentId: agentId,
        role: 'user',
        prompt: request.prompt,
        history: historyPayload,
        attachedPaths: externallyAttachedFiles,
        attachments: attachedFiles
    };
    console.log(`[TS PAYLOAD OUT to ${agentId}]: ` + JSON.stringify(payload, null, 2));
    //  5.  Execute the fetch to C#
    const res = await fetch(apiUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });
    if (!res.ok) {
        throw new Error(`C# Backend returned Http ${res.status}`);
    }
    const data = await res.json();
    return data.response || "No response received from backend.";
}


/***/ }),
/* 6 */
/***/ ((module) => {

module.exports = require("util");

/***/ }),
/* 7 */
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.ExternalContextState = void 0;
exports.ExternalContextState = {
    pendingPaths: [],
    addPath: (path) => {
        if (!exports.ExternalContextState.pendingPaths.includes(path)) {
            exports.ExternalContextState.pendingPaths.push(path);
        }
    },
    consumePaths: () => {
        const paths = [...exports.ExternalContextState.pendingPaths];
        exports.ExternalContextState.pendingPaths = []; //  Clear the path(s) after reading
        return paths;
    }
};


/***/ }),
/* 8 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.registerBellaParticipant = registerBellaParticipant;
const vscode = __importStar(__webpack_require__(1));
const crypto = __importStar(__webpack_require__(4));
const agentDispatcher_1 = __webpack_require__(5);
function registerBellaParticipant(context) {
    console.log('Bella is back from chasing squirrels');
    //	1.	Hold the active session ID in memory on the client
    let activeSessionId = crypto.randomUUID();
    const bellaParticipant = vscode.chat.createChatParticipant('bella.chat', async (request, context, response, token) => {
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
            const answer = await (0, agentDispatcher_1.dispatchToAgentAsync)(request, context, activeSessionId, 'bella', 'http://127.0.0.1:5000/api/v1/chat');
            response.markdown(answer);
        }
        catch (error) {
            response.markdown(`*whimpers* Woof! I couldn't find the backend ... \n\nError: ${error.message}`);
        }
    });
    // Give Bella a custom icon if you want!
    // bellaParticipant.iconPath = vscode.Uri.joinPath(context.extensionUri, 'images', 'dog.png');
    context.subscriptions.push(bellaParticipant);
}


/***/ }),
/* 9 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.registerMozzieParticipant = registerMozzieParticipant;
const vscode = __importStar(__webpack_require__(1));
const crypto = __importStar(__webpack_require__(4));
const agentDispatcher_1 = __webpack_require__(5);
function registerMozzieParticipant(context) {
    console.log(('Mozzie is now avaialble'));
    let activeSessionId = crypto.randomUUID();
    const mozzieParticipant = vscode.chat.createChatParticipant('mozzie.chat', async (request, context, response, token) => {
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
            const answer = await (0, agentDispatcher_1.dispatchToAgentAsync)(request, context, activeSessionId, 'esper', 'http://127.0.0.1:5000/api/v1/workspace/queue/scan');
            response.markdown(answer);
        }
        catch (error) {
            response.markdown(`*
                    Mozzie couldn't find and enrichment files to fence ... Error ${error.message}`);
        }
    });
    context.subscriptions.push(mozzieParticipant);
}


/***/ }),
/* 10 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.BrowseExternalContextTool = void 0;
const vscode = __importStar(__webpack_require__(1));
// Global state tracker for paths captured via the UI tool during this session turn
let pendingExternalPaths = [];
// Define the tool class matching the LanguageModelTool interface
class BrowseExternalContextTool {
    async invoke(options, _token) {
        // Trigger the native OS file picker
        const fileUris = await vscode.window.showOpenDialog({
            canSelectFiles: true,
            canSelectFolders: true,
            canSelectMany: false,
            openLabel: 'Attach to ReasonMCP',
            defaultUri: vscode.Uri.file('C:\\Source')
        });
        if (!fileUris || fileUris.length === 0) {
            // FIX: Return the LanguageModelToolResult object directly
            return new vscode.LanguageModelToolResult([
                new vscode.LanguageModelTextPart('No directory selected.')
            ]);
        }
        const selectedPath = fileUris[0].fsPath;
        // Track the path to bundle into your final C# fetch payload
        pendingExternalPaths.push(selectedPath);
        // FIX: Return the LanguageModelToolResult object directly
        // The framework processes TextParts to display them elegantly in the UI row
        return new vscode.LanguageModelToolResult([
            new vscode.LanguageModelTextPart(`Successfully attached: ${selectedPath}`)
        ]);
    }
}
exports.BrowseExternalContextTool = BrowseExternalContextTool;


/***/ }),
/* 11 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.registerEsperParticipant = registerEsperParticipant;
const vscode = __importStar(__webpack_require__(1));
const crypto = __importStar(__webpack_require__(4));
const agentDispatcher_1 = __webpack_require__(5);
function registerEsperParticipant(context) {
    console.log('Esper is now available');
    let activeSessionId = crypto.randomUUID();
    const esperParticipant = vscode.chat.createChatParticipant('esper.chat', async (request, context, response, token) => {
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
            const answer = await (0, agentDispatcher_1.dispatchToAgentAsync)(request, context, activeSessionId, 'esper', 'http://127.0.0.1:5000/api/v1/external/scan');
            response.markdown(answer);
        }
        catch (error) {
            //  Agent-specific error message
            response.markdown(`[System Error]: Esper failed to extract the payload.  Details: ${error.message}`);
        }
    });
    context.subscriptions.push(esperParticipant);
}


/***/ })
/******/ 	]);
/************************************************************************/
/******/ 	// The module cache
/******/ 	const __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		const cachedModule = __webpack_module_cache__[moduleId];
/******/ 		if (cachedModule !== undefined) {
/******/ 			return cachedModule.exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		const module = __webpack_module_cache__[moduleId] = {
/******/ 			// no module.id needed
/******/ 			// no module.loaded needed
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		__webpack_modules__[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/************************************************************************/
/******/ 	
/******/ 	// startup
/******/ 	// Load entry module and return exports
/******/ 	// This entry module is referenced by other modules so it can't be inlined
/******/ 	let __webpack_exports__ = __webpack_require__(0);
/******/ 	module.exports = __webpack_exports__;
/******/ 	
/******/ })()
;
//# sourceMappingURL=extension.js.map