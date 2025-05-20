function togglePassword() {
    var x = document.getElementById("exampleInputPassword1");
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
  function togglePasswordConfirm() {
    var x = document.getElementById("exampleInputPassword2");
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

