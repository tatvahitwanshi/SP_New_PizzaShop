// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function updateFileNameAndValidFile(myFile, fileNameSpan, imageFileValidation, showName = true, checksize = true) {
    var input = document.getElementById(myFile);
    var fileNameSpan = document.getElementById(fileNameSpan);
    var validationSpan = document.getElementById(imageFileValidation);
    validationSpan.textContent = "";
   
    if (input.files && input.files.length > 0) {
      var file = input.files[0];
      var fileSizeMB = file.size / (1024 * 1024);
      var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.jfif)$/i;
   
      //  Check extension
      if (!allowedExtensions.exec(file.name)) {
        validationSpan.textContent =
          "Only JPG, JPEG, PNG, and JFIF files are allowed.";
 
        fileNameSpan.textContent = "Drag and Drop or browse file";
 
        if(showName == false) fileNameSpan.textContent = "";
        input.value = "";
        return;
      }
   
      //  Check file size
      if (fileSizeMB > 2 && checksize == true) {
        validationSpan.textContent = "File size should not exceed 2 MB.";
        fileNameSpan.textContent = "Drag and Drop or browse file";
        if(showName == false) fileNameSpan.textContent = "";
       
        input.value = "";
        return;
      }
   
      //  If valid, show file name
      fileNameSpan.textContent = file.name;
    } else {
      fileNameSpan.textContent = "Drag and Drop or browse file";
    }
  }

  $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
    if (jqxhr.status === 401 || jqxhr.status === 403) {
        var response = JSON.parse(jqxhr.responseText);
        if (response.redirectUrl) {
            window.location.href = response.redirectUrl;
        }
    }
});
 
$(document).ready(function () {
 
  toastr.options = {
      "closeButton": true,
      "progressBar": true,
      "positionClass": "toast-top-right",
      "timeOut": "5000" // Auto dismiss in 5 seconds
  };

});

document.addEventListener("DOMContentLoaded", function () {
  window.addEventListener("load", function () {
      const loader = document.getElementById("pageLoader");
      if (loader) {
          loader.classList.add("hidden");
      }
  });
});

