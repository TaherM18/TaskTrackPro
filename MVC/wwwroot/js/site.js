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
                    text: "Redirecting...",
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
            <img class="rounded-circle" style="height:40px; width:40px;" src="/profile_images/${user.image}" onerror="this.src='/profile_images/placeholder.jpg'">
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

function updateNotificationList(notifications) {
    const notificationList = $("#notificationList");
    notificationList.empty();
    
    if (notifications.length === 0) {
        notificationList.append(`
            <div class="p-3 text-center text-muted">
                No notifications
            </div>
        `);
        return;
    }

    notifications.forEach(notification => {
        const html = `
            <div class="notification-item ${notification.isRead ? '' : 'unread'}" data-id="${notification.id}">
                <div class="d-flex align-items-center">
                    <div class="notification-icon ${notification.type}">
                        <i class="fas ${getNotificationIcon(notification.type)}"></i>
                    </div>
                    <div class="flex-grow-1">
                        <div class="notification-title">${notification.title}</div>
                        <div class="notification-desc">${notification.description}</div>
                        <div class="notification-time">
                            <i class="far fa-clock"></i> ${formatTimeAgo(notification.createdAt)}
                        </div>
                    </div>
                </div>
            </div>
        `;
        notificationList.append(html);
    });

    // Update badge count
    const unreadCount = notifications.filter(n => !n.isRead).length;
    $("#notificationCount").text(unreadCount);
    if (unreadCount === 0) {
        $("#notificationCount").hide();
    } else {
        $("#notificationCount").show();
    }
}

function getNotificationIcon(type) {
    switch (type) {
        case 'task':
            return 'fa-tasks';
        case 'message':
            return 'fa-envelope';
        case 'alert':
            return 'fa-exclamation-triangle';
        default:
            return 'fa-bell';
    }
}

function formatTimeAgo(date) {
    const now = new Date();
    const diff = now - new Date(date);
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);

    if (days > 0) return `${days}d ago`;
    if (hours > 0) return `${hours}h ago`;
    if (minutes > 0) return `${minutes}m ago`;
    return 'Just now';
}

function markAllAsRead() {
    // API call to mark all as read
    // Then refresh notifications
}

// Example usage:
$(document).ready(function() {
    // Sample data - replace with your API call
    const sampleNotifications = [
        {
            id: 1,
            type: 'task',
            title: 'New Task Assigned',
            description: 'You have been assigned a new task: Project Review',
            createdAt: new Date(Date.now() - 30000),
            isRead: false
        },
        {
            id: 2,
            type: 'message',
            title: 'New Message',
            description: 'John sent you a message regarding the project',
            createdAt: new Date(Date.now() - 3600000),
            isRead: true
        }
    ];

    updateNotificationList(sampleNotifications);
});

// =========================================================================

// Important
checkIdentityAndAccess();
//
setProfileDiv();
