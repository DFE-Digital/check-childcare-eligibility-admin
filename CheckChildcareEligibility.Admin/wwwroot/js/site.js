//BEGIN-- Summon print dialogue from a link
document.addEventListener("DOMContentLoaded", function () {
    // Use event delegation to handle clicks on dynamically added #print-link
    document.addEventListener("click", function (e) {
        const target = e.target;

        // Check if the clicked element is the print link or inside it
        if (target && target.id === "print-link") {
            e.preventDefault();
            window.print();
        }
    });
});
//END-- Summon print dialogue from a link


//BEGIN-- Can show elements only when JavaScript is enabled by using this class on the element
document.querySelectorAll('.js-only').forEach(x => x.classList.add("show"))
//END-- Can show elements only when JavaScript is enabled by using this class on the element


//BEGIN-- Back link in views
document.addEventListener("DOMContentLoaded", function () {
    const backs = document.getElementsByClassName("backLinkJS");
    for (var i = 0; i < backs.length; i++) {
        backs[i].onclick = function () { history.back(); return false; };
    }
});
//END-- Back link in views
