// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import {dotnet} from './dotnet.js'

const {setModuleImports, getAssemblyExports, getConfig} = await dotnet
    .withDiagnosticTracing(false)
    .create();

setModuleImports('main.mjs', {});

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

const result = exports.Bridge.CompileAndRun("func main() { print(\"Hello Wasm\"); }");
console.log("result" + result);

await dotnet.run();
