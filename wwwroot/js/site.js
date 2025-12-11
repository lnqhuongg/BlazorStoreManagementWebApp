window.downloadFileFromBase64 = (fileName, base64) => {
    const link = document.createElement("a");
    link.href = "data:application/pdf;base64," + base64;
    link.download = fileName;
    link.click();
}

window.showPdfPreview = (base64) => {
    if (!base64) {
        console.error("Base64 is null or undefined!");
        return;
    }

    const iframe = document.getElementById("pdfPreviewFrame");
    if (!iframe) {
        console.error("iframe not found");
        return;
    }

    iframe.src = "data:application/pdf;base64," + base64 + "#toolbar=0";
};


window.printPdfPreview = (base64) => {
    if (!base64) {
        console.error("Base64 is null or undefined!");
        return;
    }

    const pdfWindow = window.open("", "_blank");

    pdfWindow.document.write(`
        <html>
            <head><title>Print PDF</title></head>
            <body style="margin:0; overflow:hidden;">
                <embed id="pdfEmbed" type="application/pdf"
                       width="100%" height="100%"
                       src="data:application/pdf;base64,${base64}#toolbar=0">
            </body>
        </html>
    `);

    pdfWindow.document.close();

    const embed = pdfWindow.document.getElementById("pdfEmbed");

    // Quan trọng: đợi chính <embed> load xong
    embed.addEventListener("load", () => {
        pdfWindow.focus();
        pdfWindow.print();
    });
};

