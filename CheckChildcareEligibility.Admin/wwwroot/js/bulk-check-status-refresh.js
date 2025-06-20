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


document.querySelectorAll('.delete-link').forEach(function (el) {
    el.addEventListener('click', function (e) {
        e.preventDefault();
        const groupId = this.getAttribute('data-group-id');
        deleteItem(groupId);
    });
});

window.deleteItem = function (groupId) {
    fetch(`/BulkCheck/Bulk_check_file_delete?groupId=${groupId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        }
    })
        .then(response => {
            if (response.ok) {
//                alert("OK");
            } else {
//                alert("Failed to delete.");
            }
        });
};