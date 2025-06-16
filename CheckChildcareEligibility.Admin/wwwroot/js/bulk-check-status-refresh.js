function loadStatuses() {
    let url = document.getElementById("content").getAttribute("data-url");
    fetch(url)
        .then(response => response.text())
        .then(html => {
            // Parse the fetched HTML and extract the #content section
            var parser = new DOMParser();
            var doc = parser.parseFromString(html, 'text/html');
            var newContent = doc.getElementById("content");

            // Only update the content if the data-type has changed
            if (newContent.innerHTML !== document.getElementById("content").innerHTML) {
                document.getElementById("content").innerHTML = newContent.innerHTML;
                if (!newContent.getAttribute("data-url")) clearInterval(loaderTimer);
            }
        })
        .catch(error => {
            console.error('Error fetching batch statuses:', error);
        });
}

// Poll the server for status if JavaScript is enabled
var refreshTimer = setInterval(function () {
    loadStatuses();
}, 5000);