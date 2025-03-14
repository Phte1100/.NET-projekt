document.addEventListener("DOMContentLoaded", function() {
    let searchInput = document.querySelector("input[name='searchString']");
    let searchForm = document.querySelector("form");

    searchInput.addEventListener("keyup", function() {
        clearTimeout(window.searchTimeout);
        window.searchTimeout = setTimeout(() => {
            searchForm.submit();
        }, 500); // Väntar 0.5 sekunder innan sökning sker
    });
});
