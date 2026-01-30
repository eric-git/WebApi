const fg = require("fast-glob");
const fs = require("fs");
const path = require("path");

const redocFiles = ["node_modules/redoc/bundles/redoc.standalone.js"];
const targets = fg.sync("src/redoc/**/");
targets.forEach((dest) => {
    fs.mkdirSync(dest, { recursive: true });
    redocFiles.forEach((file) => {
        const filename = path.basename(file);
        fs.copyFileSync(file, path.join(dest, filename));
    });
});
