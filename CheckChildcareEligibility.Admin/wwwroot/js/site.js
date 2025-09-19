//BEGIN-- Summon print dialogue from a link
document.addEventListener("DOMContentLoaded", function () {
    const printLink = document.getElementById("print-link");
    if (printLink) {
        printLink.addEventListener("click", function (e) {
            e.preventDefault();
            printPage();
        });
    }
});

function printPage() {
    window.print();
}
//END-- Summon print dialogue from a link

//BEGIN-- Can show elements only when JavaScript is enabled by using this class on the element
document.querySelectorAll('.js-only').forEach(x => x.classList.add("show"))
//END-- Can show elements only when JavaScript is enabled by using this class on the element

