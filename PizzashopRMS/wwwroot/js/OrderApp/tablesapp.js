$(document).ready(function () {

    updateTimeDifference(); // Run immediately on page lo
    setInterval(updateTimeDifference, 1000);

});
// show time on the cards -----------------------------------------------------------
function updateTimeDifference() {
    document.querySelectorAll(".table-view-card").forEach(element => {

        if(element.getAttribute("data-time") == '') {
            return;
        }

        const orderTime = new Date(element.getAttribute("data-time"));
        const now = new Date();
        const diffInSeconds = Math.floor((now - orderTime) / 1000);

        if (diffInSeconds < 0) {
            element.querySelector(".time-difference").textContent = "Just now";
            return;
        }

        let days = Math.floor(diffInSeconds / (24 * 3600));
        let hours = Math.floor((diffInSeconds % (24 * 3600)) / 3600);
        let minutes = Math.floor((diffInSeconds % 3600) / 60);
        let seconds = diffInSeconds % 60;

        element.querySelector(".time-diff").textContent =
            `${days} days ${hours} hours ${minutes} min ${seconds} sec `;
    });
}
//waiting token ---------------------------------------------------------------
function GenerateTokenModal(id = 0) {
    console.log(id);
    $.ajax({
        url: '/WaitingList/GenerateToken',
        type: 'GET',
        data: { id: id },
        success: function (response) {
            $("#GenerateTokenModalContainer").html(response);
            $("#sectionTypeDropdown").val(id);
            // $("#waitingTokenModal").modal("show");
            var modal = new bootstrap.Modal(document.getElementById("waitingTokenModal"));
            modal.show();
            $("#sectionTypeDropdown").attr("tabindex","-1");
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
            if (response.success) {
                $("#waitingTokenModal").modal("hide");
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
function completeAllDetailAssgin(email)
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
                $("#tokenNameAssign").val(response.customer.customername);
                $("#tokenMobileNoAssign").val(response.customer.phoneNumber);
                $("#tokenEmailAssign").val(response.customer.customeremail);
            }
        },
        error: function () {
            alert("Error fetching details");
        }
    });
}
function loadPartialView(sectionid)
{
    $.ajax({
        url: '/TablesApp/LoadParialView',
        type: 'GET',
        success: function (response) {
            $("#sectionListContainer").html(response);
            selectedCards = [];
            selectedSectionId = null;
            selectedSectionName = null;
            console.log("Section id from load =",sectionid);
            openAccordion(sectionid);
        },
        error: function () {
            toastr.error('Error loading content Load Parial View');
        }
    });
}

let selectedCards = [];
let selectedSectionId = null;
let selectedSectionName = null;


function selectTable(clickedCard) {
    const cardSectionId = clickedCard.getAttribute('data-secid');
    const cardStatus = clickedCard.getAttribute('data-status');
    const cardId = clickedCard.getAttribute('data-id');

    if (cardStatus !== "available")
    {
        createOrder(clickedCard);
        return;
    } 

    // If different section is clicked, clear previous selection
    if (selectedSectionId && cardSectionId !== selectedSectionId) {
        // Deselect all previously selected cards
        selectedCards.forEach(card => card.classList.remove('card-color-selected'));
        selectedCards = [];
        selectedSectionId = null;
        selectedSectionName = null;
    }

    // Toggle clicked card
    const index = selectedCards.indexOf(clickedCard);
    if (index !== -1) {
        // Already selected, so deselect it
        clickedCard.classList.remove('card-color-selected');
        selectedCards.splice(index, 1);

        if (selectedCards.length === 0) {
            selectedSectionId = null;
            selectedSectionName = null;
        }
    } else {
        // New selection
        selectedCards.push(clickedCard);
        clickedCard.classList.add('card-color-selected');

        selectedSectionId = cardSectionId;
        selectedSectionName = clickedCard.getAttribute('data-secname');
    }
}

function getCustomerList(id) {
    $.ajax({
        url: '/TablesApp/GetCustomerList',
        type: 'GET',
        data: {sectionId: id},
        success: function (response) {
            $("#waitingTokenTableContainer").html(response);
        },
        error: function () {
            toastr.error('Error loading content of customer list.');
        }
    });
}

function getAssignTable(id) {
    $.ajax({
        url: '/TablesApp/GetAssignTable',
        type: 'GET',
        data: {sectionId: id},
        success: function (response) {
            $("#assignTableContainer").html(response);
            const selectedTableIds = selectedCards.map(card => parseInt(card.getAttribute('data-id')));
            $("#tableIdAssign").val(JSON.stringify(selectedTableIds)); 
        },
        error: function (xhr, status, error) {
            toastr.error('Error loading content of assign table.');
        }
    });
}

//open offcanvas-----------------------------------------------------------
function openOffCanvasForAssignTable(sectionId, sectionName) {
    if (!selectedSectionId || selectedSectionId != sectionId) {
        let secName = sectionName ?? "this section";
        toastr.error("Please select tables from " + secName);
        return;
    }

    const offCanvas = document.getElementById('assignTableSection');
    const offCanvasInstance = bootstrap.Offcanvas.getOrCreateInstance(offCanvas);
    offCanvasInstance.show();

    getCustomerList(selectedSectionId); 
    getAssignTable(selectedSectionId); 
  
}

// fill customer list in the offcanvas---------------------------------------
function TokenCheckboxChange(checkbox) {
    const $checkbox = $(checkbox);

    // Uncheck all others
    $('.token-checkbox').not($checkbox).prop('checked', false);

    if ($checkbox.is(':checked')) {
        $('.assign-form-input').addClass('block-input');
        fillForm($checkbox);
    } else {
        $('.assign-form-input').removeClass('block-input');
        clearForm();
    }
}

function fillForm($checkbox) {
    $('#tokenIdAssign').val($checkbox.data('id'));
    $('#tokenNameAssign').val($checkbox.data('customer-name'));
    $('#tokenEmailAssign').val($checkbox.data('email'));
    $('#tokenMobileNoAssign').val($checkbox.data('phone'));
    $('#numberOfPersonAssign').val($checkbox.data('no-person'));
}

function clearForm() {
    $('#tokenIdAssign').val('');
    $('#tokenNameAssign').val('');
    $('#tokenEmailAssign').val('');
    $('#tokenMobileNoAssign').val('');
    $('#numberOfPersonAssign').val('');
}

function closeAssignOffcanvas() {
    var $offcanvas = $('#assignTableSection');
    var offcanvasInstance = bootstrap.Offcanvas.getInstance($offcanvas[0]);
    if (offcanvasInstance) {
        offcanvasInstance.hide();
    }
}

$(document).on("submit", "#assignTokenForm", function (e) {
    e.preventDefault();

    var form = $(this);
    var formData = form.serialize();
    
    $.ajax({
        url: '/TablesApp/AssignToken',
        type: 'POST',
        data: formData,
        success: function (response) {
            if(response.status)
            {
                toastr.success("Tables Assigned Successfully");
                loadPartialView(response.sectionid);
                closeAssignOffcanvas();
                
            }else
            {
                toastr.error(response.message);
            }
        },
        error: function () {
            toastr.error("Error assigning token");
        }
    });

});

// create order 
function createOrder(element)
{
    const tableStatus = element.getAttribute('data-status');
    const tableId = element.getAttribute('data-id');
    const tableName = element.getAttribute('data-name');
    const cardSectionId = element.getAttribute('data-secid');
    
    $("#orderRemoveTokenModal").modal('show');
    $("#tableNameForModal").html(tableName);
    $("#tableIdForModal").val(tableId);
    $("#sectionIdForModal").val(cardSectionId);


    if(tableStatus == "running")
    {
        $("#orderBtn").html("Update Order");
        $("#removeTokenBtn").addClass("d-none");
    }else if(tableStatus == "assigned")
    {
        $("#orderBtn").html("Order");
        $("#removeTokenBtn").removeClass("d-none");
    }
}
function removeToken()
{
    tableId = parseInt($("#tableIdForModal").val());
    console.log($("#sectionIdForModal").val());
    $.ajax({
        url: '/TablesApp/DeleteToken',
        type: 'POST',
        data: { tableId: tableId },
        success: function (response) {
           console.lod($("#sectionIdForModal").val());

           loadPartialView($("#sectionIdForModal").val());
        },
        error: function () {
            alert("Error fetching details");
        }
    });

    loadPartialView($("#sectionIdForModal").val());
}

function openAccordion(sectionId) {
    if (!sectionId || sectionId < 1) return;
    const targetCollapse = document.querySelector(`#sec-${sectionId}`);
    if (targetCollapse) {
        // Create or get Bootstrap collapse instance
        let collapseInstance = bootstrap.Collapse.getInstance(targetCollapse);
        if (!collapseInstance) {
            collapseInstance = new bootstrap.Collapse(targetCollapse, {
                toggle: false // Don't toggle immediately
            });
        }
        // Explicitly show the collapse
        collapseInstance.show();
        // Scroll to the header
        const accordionHeader = document.querySelector(`#accordionExample-${sectionId}`);
        if (accordionHeader) {
            accordionHeader.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }
} 

function TableToOrderMenu()
{
    tableId = parseInt($("#tableIdForModal").val());
    window.location.href = `/MenuApp/MenuApp?tableId=${tableId}`;
}

