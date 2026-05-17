window.addEventListener("DOMContentLoaded", () => {
    const config = {
        showExtensions: true,
        sanitize: true,
        downloadUrls: [
            { title: "OpenAPI YAML", url: "/openapi/v1.yaml" },
            { title: "OpenAPI JSON", url: "/openapi/v1.json" },
        ],
        theme: {
            typography: {
                fontFamily:
                    "system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif",
                headings: {
                    fontFamily:
                        "system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif",
                    fontWeight: 400,
                },
                code: {
                    fontFamily:
                        "ui-monospace, SFMono-Regular, Menlo, Consolas, 'Liberation Mono', monospace",
                },
            },
        },
    };
    Redoc.init(
        "/openapi/v1.yaml",
        config,
        document.getElementById("redoc-container"),
    );
});
