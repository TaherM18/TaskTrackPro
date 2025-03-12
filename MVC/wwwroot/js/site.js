// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function checkIdentityAndAccess() {
    let user = JSON.parse(sessionStorage.getItem("user"));
    if (user == null) {
        // not authenticated user
        window.location.assign("/Auth/Login");
    }
    else {
        // authenticated user
        // check role
        let controller = window.location.href.split("/")[3].toLowerCase();
        switch(controller) {
            case "admin":
                if (user.role != "A") {
                    alert("You are not authorized to view this page");
                    window.location.assign("/Auth/Login");
                }
                break;
            case "employee":
                if (user.role != "E") {
                    alert("You are not authorized to view this page");
                    window.location.assign("/Auth/Login");
                }
                break;
            default:
                // code block
                console.log("Authentication not required");
        }
    }
}

// Important
checkIdentityAndAccess();