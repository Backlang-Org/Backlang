import * as vscode from 'vscode';

export function activate(context: vscode.ExtensionContext): void {
    context.subscriptions.push(vscode.commands.registerCommand('back.updateTemplates', async () => {
		const terminal = vscode.window.createTerminal(`Backlang Terminal`);
        terminal.sendText("dotnet new update");
	}));
}