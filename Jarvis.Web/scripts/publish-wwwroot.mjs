import { cp, mkdir, readdir, rm } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const webRoot = path.resolve(scriptDir, "..");
const exportDir = path.join(webRoot, "out");
const targetDir = path.resolve(webRoot, "..", "Jarvis", "wwwroot");
const preservedRuntimeDirs = new Set(["generated-audio"]);

await mkdir(targetDir, { recursive: true });

for (const entry of await readdir(targetDir, { withFileTypes: true })) {
  if (entry.isDirectory() && preservedRuntimeDirs.has(entry.name)) {
    continue;
  }

  await rm(path.join(targetDir, entry.name), { recursive: true, force: true });
}

await cp(exportDir, targetDir, { recursive: true });

console.log(`Published Jarvis.Web static export to ${targetDir}`);
