(function () {
    var summary = document.getElementById("error-summary");
    var links = summary ? summary.querySelectorAll("a") : [];

    function linkAndStyleErrors() {
        for (let i = 0; i < links.length; i++) {
            let href = links[i].getAttribute("href");
            let targetId = href.replace("#", "");
            let element = document.getElementById(targetId);

            // if it's a DOB subfield - style whole DOB block
            if (targetId.startsWith("Child.ChildDateOfBirth")) {
                element = document.getElementById("Child.ChildDateOfBirth");
            } else if (targetId.startsWith("DateOfBirth")) {
                element = document.getElementById("DateOfBirth");
            }

            if (element) {
                let parent = element.closest('.govuk-form-group');
                if (parent) {
                    parent.classList.add("govuk-form-group--error");
                }
            }
        }
    }

    function setFocusOnSummary() {
        let summary = document.querySelector('.govuk-error-summary');
        if (summary) {
            window.onload = function () {
                summary.focus();
            };
        }
    }

    if (links.length > 0) {
        linkAndStyleErrors();
    }
    setFocusOnSummary();
})();