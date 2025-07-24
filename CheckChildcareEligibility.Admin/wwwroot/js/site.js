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
