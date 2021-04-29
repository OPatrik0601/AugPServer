// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function confirmNewArticle() {
    var conf = confirm("Are you sure? The current article will be no longer available!");
    if (conf) {
        window.location.href = '/FinishNewProject/StartNewProject';
    }
}