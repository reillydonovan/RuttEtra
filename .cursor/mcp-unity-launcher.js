#!/usr/bin/env node
/**
 * Launcher for MCP Unity server. Finds the server in Unity's PackageCache
 * (handles different package versions) and runs it.
 */
const path = require('path');
const fs = require('fs');
const { spawn } = require('child_process');

// Project root: prefer folder containing .cursor (where this script lives), then cwd
const scriptProjectRoot = path.resolve(__dirname, '..');
const cwdRoot = process.cwd();
const candidates = [scriptProjectRoot, cwdRoot];

let projectRoot = null;
let cachePath = null;
for (const root of candidates) {
  const cache = path.join(root, 'Library', 'PackageCache');
  if (fs.existsSync(cache)) {
    projectRoot = root;
    cachePath = cache;
    break;
  }
}
if (!projectRoot || !cachePath) {
  const checked = candidates.filter((r) => r && r !== '.').join(' or ');
  console.error(
    '[MCP Unity] Library/PackageCache not found at:\n  ' +
      path.join(scriptProjectRoot, 'Library', 'PackageCache') +
      '\n\n' +
      '1. Open this exact project in Unity (File > Open Project > select the RuttEtra folder).\n' +
      '2. In Unity: Window > Package Manager > + > Add package from git URL…\n' +
      '    Enter: https://github.com/CoderGamester/mcp-unity.git\n' +
      '3. Then: Tools > MCP Unity > Server Window > Force Install Server.'
  );
  process.exit(1);
}

const entries = fs.readdirSync(cachePath, { withFileTypes: true });
const mcpDir = entries.find(
  (d) => d.isDirectory() && d.name.startsWith('com.gamelovers.mcp-unity')
);

if (!mcpDir) {
  console.error(
    '[MCP Unity] MCP Unity package not found in PackageCache. In Unity: Window > Package Manager > + > Add package from git URL… and enter: https://github.com/CoderGamester/mcp-unity.git'
  );
  process.exit(1);
}

const serverDir = path.join(cachePath, mcpDir.name, 'Server~');
const serverPath = path.join(serverDir, 'build', 'index.js');

if (!fs.existsSync(serverPath)) {
  console.error(
    '[MCP Unity] Server not built. In Unity: Tools > MCP Unity > Server Window > click "Force Install Server", then "Start Server".'
  );
  process.exit(1);
}

const child = spawn('node', [serverPath], {
  stdio: 'inherit',
  cwd: serverDir,
  env: { ...process.env, UNITY_PROJECT_PATH: projectRoot },
});

child.on('error', (err) => {
  console.error('[MCP Unity] Failed to start server:', err.message);
  process.exit(1);
});

child.on('exit', (code, signal) => {
  process.exit(code != null ? code : signal ? 1 : 0);
});
