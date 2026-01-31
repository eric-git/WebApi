import fs from "fs";
import path from "path";
import fg from "fast-glob";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

console.log("🚀 Starting deploy-docs");
console.log(`📌 Script directory: ${__dirname}`);

const redocSourceFolders = fg.sync(path.join(__dirname, "WebApi.*"), {
  onlyDirectories: true,
  deep: 1,
});

console.log(`📂 Found ${redocSourceFolders.length} source folders in /src/redoc:`);
redocSourceFolders.forEach((f) => console.log(`   • ${f}`));

const projectNames = redocSourceFolders.map((p) => path.basename(p));

projectNames.forEach((projectName) => {
  const projectPath = path.join(__dirname, "..", projectName);
  const docsFolder = path.join(projectPath, "docs");

  if (!fs.existsSync(projectPath)) {
    console.log(`⚠️ Skipping ${projectName} — no matching target project found`);
    return;
  }

  console.log(`\n==============================`);
  console.log(`📦 Processing project: ${projectName}`);
  console.log(`==============================`);

  fs.mkdirSync(docsFolder, { recursive: true });
  console.log(`📁 Ensured docs folder exists: ${docsFolder}`);

  const existingFiles = fg.sync(["*.*"], { cwd: docsFolder });
  console.log(`🧹 Clearing ${existingFiles.length} file(s) from ${projectName}/docs`);
  existingFiles.forEach((file) => {
    fs.rmSync(path.join(docsFolder, file));
    console.log(`   ❌ Removed: ${file}`);
  });

  const sourceFolder = path.join(__dirname, projectName);
  const htmlFiles = fg.sync(["*.html"], { cwd: sourceFolder });
  const docsSourceFolder = path.join(sourceFolder, "docs");
  const assetFiles = fg.sync(["*.*"], { cwd: docsSourceFolder });

  console.log(`📄 Found ${htmlFiles.length} HTML file(s) in ${projectName}/`);
  console.log(`🎨 Found ${assetFiles.length} asset file(s) in ${projectName}/docs`);

  htmlFiles.forEach((filename) => {
    const src = path.join(sourceFolder, filename);
    const dest = path.join(docsFolder, filename);
    fs.copyFileSync(src, dest);
    console.log(`   ➕ Copied HTML: ${filename}`);
  });

  assetFiles.forEach((filename) => {
    const src = path.join(docsSourceFolder, filename);
    const dest = path.join(docsFolder, filename);
    fs.copyFileSync(src, dest);
    console.log(`   🎨 Copied asset: ${filename}`);
  });

  console.log(`✅ Finished project: ${projectName}`);
});

console.log("\n🎉 All matching projects processed.");
