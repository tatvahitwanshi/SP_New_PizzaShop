function togglePasswordOld() {
    var x = document.getElementById("OldPassword");
    var eye=document.getElementById("eye1");
    if (x.type === "password") {
      x.type = "text";
        eye.classList.remove("bi-eye");
        eye.classList.add("bi-eye-slash");
    }
    else {
      x.type = "password";
      eye.classList.remove("bi-eye-slash");
    eye.classList.add("bi-eye");
    }
  };
  function togglePasswordNew() {
    var x = document.getElementById("NewPassword");
    var eye=document.getElementById("eye2");
    if (x.type === "password") {
      x.type = "text";
        eye.classList.remove("bi-eye");
        eye.classList.add("bi-eye-slash");
    }
    else {
      x.type = "password";
      eye.classList.remove("bi-eye-slash");
    eye.classList.add("bi-eye");
    }
  };

  function togglePasswordConfirm() {
    var x = document.getElementById("ConfirmPassword");
    var eye=document.getElementById("eye3");
    if (x.type === "password") {
      x.type = "text";
        eye.classList.remove("bi-eye");
        eye.classList.add("bi-eye-slash");
    }
    else {
      x.type = "password";
      eye.classList.remove("bi-eye-slash");
    eye.classList.add("bi-eye");
    }
  };
