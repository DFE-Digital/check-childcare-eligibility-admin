function checkStatus() {
    const currentContent = document.getElementById("content");
    const url = currentContent?.getAttribute("data-url");

    if (!currentContent || !url) {
        clearInterval(loaderTimer);
        return;
    }

    fetch(url)
        .then(response => response.text())
        .then(html => {
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, "text/html");
            const newContent = doc.getElementById("content");
            const currentContainer = currentContent.parentElement;
            const newContainer = newContent?.parentElement;

            // Ignore an unexpected response or a poll superseded by another response.
            if (!newContent || !currentContainer || !newContainer) {
                return;
            }

            if (newContent.getAttribute("data-type") !== currentContent.getAttribute("data-type")) {
                currentContainer.innerHTML = newContainer.innerHTML;
                document.title = doc.title;

                if (!newContent.getAttribute("data-url")) {
                    clearInterval(loaderTimer);
                }
            }
        })
        .catch(error => {
            console.error("Error fetching status:", error);
        });
}

// Poll the server for status if JavaScript is enabled
var loaderTimer = setInterval(function () {
    checkStatus();
}, 5000);