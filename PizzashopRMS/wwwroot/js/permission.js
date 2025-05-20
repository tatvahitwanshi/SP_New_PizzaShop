document.querySelector('.main-check').addEventListener('change', function () {
    const innercheck = document.querySelectorAll('.inner-check');
    innercheck.forEach(function (check) {
        const row = check.closest('tr');
        const switches = row.querySelectorAll('.form-check-input');
        check.checked = event.target.checked;
        if (!check.checked) {
            switches.forEach(function (input) {
                if (!input.classList.contains('inner-check')) {
                    input.disabled = true;
                }
            });
        } else {
            switches.forEach(function (input) {
                if (!input.classList.contains('inner-check')) {
                    input.disabled = false;
                }
            });
        }
    });
});

document.querySelectorAll('.inner-check').forEach(function (check) {
    check.addEventListener('change', function () {
        const row = check.closest('tr');
        const switches = row.querySelectorAll('.form-check-input');
        if (!check.checked) {
            switches.forEach(function (input) {
                if (!input.classList.contains('inner-check')) {
                    input.disabled = true;
                }
            });
        } else {
            switches.forEach(function (input) {
                if (!input.classList.contains('inner-check')) {
                    input.disabled = false;
                }
            });
        }
    });
});
var mainCheck = document.querySelector('.main-check');
var innerCheck = document.querySelectorAll('.inner-check');
innerCheck.forEach(function (ic) {
    ic.addEventListener('change', function() {
        if (!this.checked) {
            mainCheck.checked = false;
        }else{
            var mc = true;
            innerCheck.forEach(function(ie) {
                if(!ie.checked){
                    mc = false;
                }
            });
            
            if(mc){
                mainCheck.checked = true;
            }
        }
    })
});

document.addEventListener('DOMContentLoaded', function () {
    var mainCheck = document.querySelector('.main-check');
    var innerCheck = document.querySelectorAll('.inner-check');
    var eo = true;

    innerCheck.forEach(function (ic) {
        if (!ic.checked) {
            eo = false;
            console.log("Hi");
        }
    });

    if (eo) {
        mainCheck.checked = true;
    }
});
