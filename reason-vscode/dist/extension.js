/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ([
/* 0 */,
/* 1 */
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
const vscode = __importStar(__webpack_require__(2));
const crypto = __importStar(__webpack_require__(3));
const util_1 = __webpack_require__(4);
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
        //	2.	UI Feedback: Shows a progress indicator
        response.progress('Reason is thinking ...');
        try {
            //	prepare the chat history with proper role mapping
            const historyPayload = [];
            for (const turn of context.history) {
                if (turn instanceof vscode.ChatRequestTurn) {
                    //	it's a message from the user
                    historyPayload.push({
                        role: 'user',
                        content: turn.prompt
                    });
                }
                else if (turn instanceof vscode.ChatResponseTurn) {
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
            //	4.	Array to hold our extracted files
            const attachedFiles = [];
            //	5.	Loop through VS Code's attached references
            for (const reference of request.references) {
                let fileUri;
                // References can be raw URIs or Location objects depending
                // on how they were attached
                if (reference.value instanceof vscode.Uri) {
                    fileUri = reference.value;
                }
                else if (reference.value instanceof vscode.Location) {
                    fileUri = reference.value.uri;
                }
                if (fileUri) {
                    try {
                        // Read the file directly from the VS Code workspace filesystem
                        const fileData = await vscode.workspace.fs.readFile(fileUri);
                        const fileContent = new util_1.TextDecoder('utf-8').decode(fileData);
                        // Extract just the filename from the path
                        const fileName = fileUri.path.split('/').pop() || "UnknownFile.txt";
                        attachedFiles.push({
                            fileName: fileName,
                            content: fileContent
                        });
                    }
                    catch (err) {
                        console.error(`FAiled to read attached file ${fileUri.path}`, err);
                    }
                }
            }
            console.log("[TS PAYLOAD OUT]: " + JSON.stringify(payload, null, 2));
            //	This output provides the VSCode "pop-up" window
            // vscode.window.showInformationMessage("[TS PAYLOAD OUT]: " + JSON.stringify(payload, null, 2));
            const res = await fetch('http://127.0.0.1:5000/api/v1/chat', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    sessionId: activeSessionId,
                    agentId: 'reason',
                    role: 'user', // this will alswys be the user sending a prompt to the API
                    prompt: request.prompt,
                    history: historyPayload,
                    attachments: attachedFiles
                })
            });
            if (!res.ok) {
                throw new Error(`C# Backend returned HTTP ${res.status}`);
            }
            const data = await res.json();
            //	4.	Stream the response directly into the VS Code chat window
            response.markdown(data.response || "No response received from Reason backend.");
        }
        catch (error) {
            response.markdown(`**Architectural Failure:** Unable to reach Reason backend. Is Kestrel running on Port 5000? \n\nError: ${error.message}`);
        }
    });
    //	Register it to the extension context
    context.subscriptions.push(reasonParticipant);
}


/***/ }),
/* 2 */
/***/ ((module) => {

module.exports = require("vscode");

/***/ }),
/* 3 */
/***/ ((module) => {

module.exports = require("crypto");

/***/ }),
/* 4 */
/***/ ((module) => {

module.exports = require("util");

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
exports.registerBellaParticipant = registerBellaParticipant;
const vscode = __importStar(__webpack_require__(2));
const crypto = __importStar(__webpack_require__(3));
const util_1 = __webpack_require__(4);
function registerBellaParticipant(context) {
    //	1.	Hold the active session ID in memory on the client
    let activeSessionId = crypto.randomUUID();
    const bellaParticipant = vscode.chat.createChatParticipant('bella.chat', async (request, context, response, token) => {
        response.progress('Bella is sniffing for answers...');
        //	2.	If history is empty reset the session
        if (context.history.length === 0) {
            activeSessionId = crypto.randomUUID();
        }
        try {
            //	prepare the chat history with proper role mapping
            const historyPayload = [];
            for (const turn of context.history) {
                if (turn instanceof vscode.ChatRequestTurn) {
                    //	it's a message from the user
                    historyPayload.push({
                        role: 'user',
                        content: turn.prompt
                    });
                }
                else if (turn instanceof vscode.ChatResponseTurn) {
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
            //	4.	Array to hold our extracted files
            const attachedFiles = [];
            //	5.	Loop through VS Code's attached references
            for (const reference of request.references) {
                let fileUri;
                //	References can be raw URIs or Location objects depending
                // on how they were attached
                if (reference.value instanceof vscode.Uri) {
                    fileUri = reference.value;
                }
                else if (reference.value instanceof vscode.Location) {
                    fileUri = reference.value.uri;
                }
                if (fileUri) {
                    try {
                        // Read the file directly from the VS Code workspace filesystem
                        const fileData = await vscode.workspace.fs.readFile(fileUri);
                        const fileContent = new util_1.TextDecoder('utf-8').decode(fileData);
                        // Extract just the filename from the path
                        const fileName = fileUri.path.split('/').pop() || "UnknownFile.txt";
                        attachedFiles.push({
                            fileName: fileName,
                            content: fileContent
                        });
                    }
                    catch (err) {
                        console.error(`Failed to read attached file ${fileUri.path}`, err);
                    }
                }
            }
            console.log("[TS PAYLOAD OUT]: " + JSON.stringify(payload, null, 2));
            //	This output provides the VSCode "pop-up" window
            // vscode.window.showInformationMessage("[TS PAYLOAD OUT]: " + JSON.stringify(payload, null, 2));
            const res = await fetch('http://127.0.0.1:5000/api/v1/chat', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    sessionId: activeSessionId,
                    agentId: 'bella',
                    role: 'user', // this will alswys be the user sending a prompt to the API
                    prompt: request.prompt,
                    history: historyPayload,
                    attachments: attachedFiles
                })
            });
            if (!res.ok) {
                throw new Error(`C# Backend returned HTTP ${res.status}`);
            }
            const data = await res.json();
            //	4.	Stream the response directly into the VS Code chat window
            response.markdown(data.response || "No response received from Reason backend.");
        }
        catch (error) {
            response.markdown(`*
                    whimpers* Woof! I couldn't find the backend ... \n\nError: ${error.message}`);
        }
    });
    // Give Bella a custom icon if you want!
    // bellaParticipant.iconPath = vscode.Uri.joinPath(context.extensionUri, 'images', 'dog.png');
    context.subscriptions.push(bellaParticipant);
}


/***/ })
/******/ 	]);
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		var cachedModule = __webpack_module_cache__[moduleId];
/******/ 		if (cachedModule !== undefined) {
/******/ 			return cachedModule.exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
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
var __webpack_exports__ = {};
// This entry needs to be wrapped in an IIFE because it needs to be isolated against other modules in the chunk.
(() => {
var exports = __webpack_exports__;

Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.activate = activate;
exports.deactivate = deactivate;
const reason_1 = __webpack_require__(1);
const bella_1 = __webpack_require__(5);
function activate(context) {
    console.log('ReasonMCP Extension Suite is now active!');
    //  Bootstrap the agents
    (0, reason_1.registerReasonParticipant)(context);
    (0, bella_1.registerBellaParticipant)(context);
}
function deactivate() {
    //  Allow VS Code to handle cleanup automatically via conte
}

})();

module.exports = __webpack_exports__;
/******/ })()
;
//# sourceMappingURL=extension.js.map