// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

window.setTimeout(function () {
    $(".alert").fadeTo(500, 0).slideUp(500, function () {
        $(this).remove();
    });
},5555);

function confirmDelete(isDeleteClicked) {
    var deleteSpan = 'deleteSpan';
    var confirmDeleteSpan = 'confirmDeleteSpan';
    if (isDeleteClicked) {
        $('#' + deleteSpan).hide();
        $('#' + confirmDeleteSpan).show();
    } else {
        $('#' + deleteSpan).show();
        $('#' + confirmDeleteSpan).hide();
    }
}

var audio = document.getElementById('layout-audio');
var playPauseBtn = document.getElementById('playPauseBtn');
var count = 0;
function playPause() {
    if (count == 0) {
        count = 1;
        audio.play();
        /*playPauseBtn.innerHTML = "Pause &#9208";*/
    } else {
        count = 0;
        audio.pause();
        /*playPauseBtn.innerHTML = "Play &#9658";*/
    }
}

function addProfileImage(isClicked) {
    var addPhoto = 'addPhoto';
    var confirm = 'confirm';
    if (isClicked) {
        $('#' + addPhoto).hide();
        $('#' + confirm).show();
    } else {
        $('#' + addPhoto).show();
        $('#' + confirm).hide();
    }
}