// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function checkIdentityAndAccess() {
    let user = JSON.parse(sessionStorage.getItem("user"));
    let controller = window.location.href.split("/")[3].toLowerCase();
    switch (controller) {
        case "admin":
            if (user == null || user.role != "A") {
                Swal.fire({
                    title: "Unauthorized access",
                    text: "Going back...",
                    icon: "error",
                    timer: 2000,
                    showConfirmButton: false
                }).then(() => {
                    window.location.assign("/Auth/Login");
                });
            }
            break;
        case "employee":
            if (user == null || user.role != "E") {
                Swal.fire({
                    title: "Unauthorized access",
                    text: "Going back...",
                    icon: "error",
                    timer: 2000,
                    showConfirmButton: false
                }).then(() => {
                    window.location.assign("/Auth/Login");
                });
            }
            break;
        default:
            // code block
            console.log("Authentication not required");
    }
}

//

function setProfileDiv() {
    let user = JSON.parse(sessionStorage.getItem("user"));
    if (user != null) {
        // user is logged in
        htmlContent = `<div class="btn-group">
        <button type="button" class="btn btn-light dropdown-toggle" data-bs-toggle="dropdown" aria-expanded="false">
            <span>${user.firstName} ${user.lastName}</span>
            <img class="rounded-circle img-thumbnail" style="height:40px; width:fit-content" src="/profile_images/${user.image}" onerror="this.src='/profile_images/placeholder.jpg'">
        </button>
        <ul class="dropdown-menu dropdown-menu-end">
            <li><a class="dropdown-item" href="/${user.role == "A" ? "Admin" : "Employee"}/Profile">Profile</a></li>
            <li><a class="dropdown-item" href="/Auth/Logout">Logout</a></li>
        </ul>
    </div>`;
    } else {
        // visitor
        htmlContent = `<a class="btn btn-light" href="/Auth/Login">Sign In</a>`;
    }
    $("#profileMenu").html(htmlContent);
}

// Important
checkIdentityAndAccess();
//
setProfileDiv();
