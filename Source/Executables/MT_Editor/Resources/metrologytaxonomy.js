// wait for the DOM content to load
window.addEventListener('DOMContentLoaded', () => {
    // get all the taxons and add our event
    let taxons = document.getElementsByClassName("taxon");
    for (let i = 0; i < taxons.length; i++) {
        taxons[i].addEventListener("click", (e) => {
            e.preventDefault(); // prevent href action

            // get our details and show or hide them
            let details = taxons[i].parentElement.children[1];
            if (details.classList.contains("show-details")) {
                details.classList.replace("show-details", "hide-details");
            } else {
                details.classList.replace("hide-details", "show-details");
            }
        });
    }
});