import * as vscode  from 'vscode';
import * as node    from 'vscode-languageclient/node';
import * as jsonrpc from 'vscode-jsonrpc';

export function activate(context: vscode.ExtensionContext) {
	const serverOptions: node.ServerOptions = {
		run  : { command: `${context.extensionPath}/../teuchiudon-lsp/target/debug/teuchiudon-lsp.exe`, args: [] },
		debug: { command: `${context.extensionPath}/../teuchiudon-lsp/target/debug/teuchiudon-lsp.exe`, args: [] }
	};

	const clientOptions: node.LanguageClientOptions = {
		documentSelector: [
			{
				pattern: '**/*.teuchi'
			}
		],
		synchronize: {
			configurationSection: 'teuchiudonLanguageServer',
			fileEvents: vscode.workspace.createFileSystemWatcher('**/*.teuchi')
		}
	};

	const client = new node.LanguageClient('teuchiudonLanguageServer', 'TeuchiUdon Language Server', serverOptions, clientOptions);
	client.trace = jsonrpc.Trace.Verbose;

	const disposable = client.start();
	context.subscriptions.push(disposable);
}

export function deactivate() {}
