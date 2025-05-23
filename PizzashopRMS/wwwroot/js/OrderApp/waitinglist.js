$(document).ready(function () {

    updateTimeDifference();
    setInterval(updateTimeDifference, 60000);

});

// ------------show time on the cards ------------
function updateTimeDifference() {
    document.querySelectorAll(".created-date").forEach(element => {

        const orderTime = new Date(element.getAttribute("data-time"));
        const now = new Date();
        const diffInSeconds = Math.floor((now - orderTime) / 1000);

        if (diffInSeconds < 0) {
            element.querySelector(".time-difference").textContent = "Just now";
            return;
        }

        let totalMinutes = Math.floor(diffInSeconds / 60);
        let hours = Math.floor(totalMinutes / 60);
        let minutes = totalMinutes % 60;

        element.querySelector(".time-difference").textContent = `${hours}hrs ${minutes}min`;

    });
}

function GetTabContent(id) {
    $.ajax({
        url: '/WaitingList/GetTabContent',
        type: 'GET',
        data: { id: id },
        success: function (response) {
            $("#tab-bar-content").html(response);
            updateTimeDifference();
        },
        error: function () {
            $("#tab-bar-content").html('Error loading content');
            toastr.error("Error loading the Tab Content");
        }
    });

}
function GenerateTokenModal(id = 0) {
    const currentSectionId = $("#currentTabId").val();
    $.ajax({
        url: '/WaitingList/GenerateToken',
        type: 'GET',
        data: { id: id },
        success: function (response) {
            $("#GenerateTokenModalContainer").html(response);
            $("#waitingTokenModal").modal("show");
            if(currentSectionId != null && currentSectionId != 0)
            {
                $("#sectionTypeDropdown").val(currentSectionId); // Set selected value
                $("#sectionTypeDropdown").addClass("read-only-section"); // Add custom class
                $("#sectionTypeDropdown").attr("tabindex","-1");
            }
           
        },
        error: function () {
            toastr.error("Error generating token");
        }
    });
}
$(document).on("submit", "#waitingTokenForm", function (e) {
    e.preventDefault();

    var form = $(this);
    var formData = form.serialize();

    $.ajax({
        url: '/WaitingList/GenerateToken',
        type: 'POST',
        data: formData,
        success: function (response) {
            $("#waitingTokenModal").modal("hide");
            GetTabContent($("#currentTabId").val());
            if (response.success) {
                toastr.success(response.message);
            }
            else{
                toastr.error(response.message);
            }
        },
        error: function () {
            toastr.error(response.message);
        }
    });

});
$(document).on("submit", "#AssignTokenForm", function (e) {
    e.preventDefault();

    var form = $(this);
    var formData = form.serialize();

    $.ajax({
        url: '/WaitingList/AssignTable',
        type: 'POST',
        data: formData,
        success: function (response) {
            $("#assignTokenModal").modal("hide");
            GetTabContent($("#currentTabId").val());
            if (response.success) {
                toastr.success(response.message);
            }
            else
            {
                toastr.error(response.message);
            }
        },
        error: function () {
            toastr.error(response.message);
        }
    });

});
function setTableDropDown(sectionId) {
    $("sectionSelectHide").val(sectionId);
    $.ajax({
        url: '/WaitingList/GetTableList',
        type: 'GET',
        data: { sectionId: sectionId, capacity: $("#capacity").val() },
        success: function (tables) {
            var tableSelect = $('#tableSelect');
            tableSelect.empty();

            // // Add default placeholder
            tableSelect.append('<option selected disabled>Table* (Required)</option>');

            // Populate table dropdown
            $.each(tables.tableList, function (index, table) {
                tableSelect.append('<option value="' + table.tableId + '">' + table.tableName + '</option>');
            });
        },
        error: function () {
            toastr.error("Error loading Get Table List");
        }
    });
}

function assignToken(id, currentTabId, capacity) {
    $("#tokenIdForAssign").val(id);
    $("#sectionSelectHide").val(currentTabId);
    $("#capacity").val(capacity);


    if (currentTabId != 0) {
        $("#sectionSelect").val(currentTabId).prop("disabled", true);
        setTableDropDown(currentTabId);
    } else {
        $("#sectionSelect").val("").prop("disabled", false);
        $("#tableSelect").empty();
        $("#tableSelect").append('<option selected disabled>Table* (Required)</option>');
    }
}

function deleteToken(id) {
    $("#tokenIdForDelete").val(id);
}

$("#deleteFormToken").submit(function (e) {
    e.preventDefault();
    formData = new FormData(this);
    console.log("inside submit");
    $.ajax({
        url: '/WaitingList/DeleteToken',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            GetTabContent($("#currentTabId").val());
            $("#deleteTokenModal").modal("hide");
            if (response.success) {
                toastr.success(response.message);
            }
            else {
                toastr.error(response.message);
            }

        },
        error: function () {
            toastr.error("Error deleting token");
        }
    });
});


// suggestions for auto complete 
function completeAllDetails(email)
{
    if(email.length < 3 || email.indexOf('@') == -1 || email.indexOf('.') == -1 || email.indexOf('@') >= email.indexOf('.') || email.indexOf('@') == 0 || email.indexOf('.') == email.length - 1 )
    {
        return;
    };
    $.ajax({
        url: '/WaitingList/GetCustomerFromEmail',
        type: 'GET',
        data: { Email: email },
        success: function (response) {
            if(response.success)
            {
                $("#tokenName").val(response.customer.customername);
                $("#tokenMobileNo").val(response.customer.phoneNumber);
                $("#tokenEmail").val(response.customer.customeremail);
            }
        },
        error: function () {
            alert("Error fetching details");
        }
    });
}