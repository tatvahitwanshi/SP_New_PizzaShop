let CurrentSlide = 0;
let CardToShow = 1;
const CardWidth =314; 

$(document).ready(function () {

    //--------------------- Load All Tab in View first---------------------
    GetTabContent(0, "In Progress");
    GetPendingOrder();

    //---------------- Load selected Tab in View when Clicked --------------
    $(".tab-btn").click(function () {
        var tabId = $(this).data("tab-id");
        var orderStatus = $("#orderStatusField").val();
        
        $(".tab-btn").removeClass("active");
        $(this).addClass("active");

        GetTabContent(tabId, orderStatus);
    });
 
    setInterval(updateTimeDifference, 1000);
    updateTimeDifference(); // Run immediately on page load
  
    $(document).on("submit", "#updateItemQuantityForm", function (event) {
        event.preventDefault();
        // Check if at least one checkbox is selected
        var isChecked = $(this).find('input[type="checkbox"]:checked').length > 0;

        if (!isChecked) {
            toastr.warning("Please select at least one item.");
            return;
        }

        var formData = $(this).serialize();
        $.ajax({
            url: '/Kot/UpdateChangeQuantity', // Ensure correct URL path
            type: "POST", // POST should be a string
            data: formData,
            success: function (response) {
                     $("#ChangeStatusModal").modal("hide");
                    GetTabContent(response.id, response.orderStatus);
    
                    $(".tab-btn").each(function () {
                        if ($(this).data("tab-id") == response.id) {
                            $(this).addClass("active");
                        } else {
                            $(this).removeClass("active");
                        }
                    });
                
            },
            error: function (ex) {
                toastr.error(`Error updating quantity ${ex}`);
            }
        });

    });
});

// ------------------------------show time -----------------------------
function updateTimeDifference() {
    document.querySelectorAll(".order-time").forEach(element => {

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

        element.querySelector(".time-difference").textContent =
            `${days}d ${hours}h ${minutes}m ${seconds}s `;
    });
}

function GetTabContent(id, orderStatus) {
    $.ajax({
        url: '/Kot/GetTabContent',
        type: 'GET',
        data: {id: id, orderStatus: orderStatus},
        success: function (response) {
            $("#tabContent").html('<div class="tab-pane fade show active">' + response + '</div>');
            StartScroll();
        },
        error: function () {
            $("#tabContent").html('<div class="tab-pane fade show active">Error loading content</div>');
            toastr.error(`Error Loading the tab content ${ex}`);
        }
    });

}

function StartScroll()
{
    CurrentSlide = 0;
}

function changePage(direction) {
    console.log(direction);
    const cardWrapper = document.getElementById("cardWrapper");
    if(direction == 'next')
        CurrentSlide += CardToShow;
    else
        CurrentSlide -= CardToShow;

    if (!cardWrapper) 
    {
        return;
    }

    const maxSlide = cardWrapper.children.length - CardToShow;
    if (CurrentSlide > maxSlide || CurrentSlide < 0) {
        CurrentSlide = 0;
    }
    const offset = -CurrentSlide * CardWidth;
    cardWrapper.style.transform = `translateX(${offset}px)`;
}

function OpenchangeQuantityModal(orderValues, orderStatus, CurrentCategory)
{
    $.ajax({
        url: '/Kot/ChangeQuantityModal',
        type: 'GET',
        data: { orderValues: JSON.stringify(orderValues), orderStatus: orderStatus, CurrentCategory: CurrentCategory },
        success: function (response) {
            $("#changeQuanityContainer").html(response);
            $("#ChangeStatusModal").modal("show");
        },
        error: function (ex) {
            toastr.error(`Error while open changeQuantityModal ${ex}`);
        }
    });
}

function ChangeQuantity(id, direction)
{
    var input = $("#quantity-input-" + id);
    var value = parseInt(input.val());
    let maxVal = parseInt(input.attr("max"));

    if (direction == "increment") {
        if (value < maxVal) {
            value++;
        }
    }
    else if (direction == "decrement") {
        if (value > 0) {
            value--;
        }
    }

    input.val(value);
}

function GetPendingOrder()
{
    $.ajax({
        url: '/Kot/GetPendingOrder',
        type: 'GET',
        success: function (response) {
            $("#pendingOrdersContainer").html(response);
        },
        error: function () {
            toastr.error('Error loading content');
        }
    });
}

function sendToInProgress() {

    const selectedCheckboxes = document.querySelectorAll('input[name="selectedOrders"]:checked');
    const selectedOrderIds = Array.from(selectedCheckboxes).map(cb => cb.value);

    if (selectedOrderIds.length === 0) {
        alert("Please select at least one order to move to In Progress.");
        return;
    }

    console.log("Selected Order IDs:", selectedOrderIds);

    $.ajax({
        url: '/Kot/ChangePendingToInProgress',
        type: 'GET',
        data: {selectedOrderIds : JSON.stringify(selectedOrderIds)},
        success: function (response) {
            if(response.success)
            {
                GetPendingOrder();
    
                var orderStatus = $("#orderStatusField").val();
                var id = $("#currentCategoryInputId").val();
    
                GetTabContent(id, orderStatus);
            }
            else
            {
                toastr.error("Error while Change Pending To InProgress");
            }

        },
        error: function () {
            
        }
    });
}

function markServedOrder(orderId)
{

    $.ajax({
        url: '/Kot/MarkOrderServed',
        type: 'GET',
        data: {orderId : orderId},
        success: function (response) {

            if(response.success)
            {
                var modalEl = document.getElementById('ChangeStatusModal');
                var modalInstance = bootstrap.Modal.getInstance(modalEl);
                modalInstance.hide();

                var orderStatus = $("#orderStatusField").val();
                var id = $("#currentCategoryInputId").val();
    
                GetTabContent(id, orderStatus);
                $(".tab-btn").each(function () {
                    if ($(this).data("tab-id") == id) {
                        $(this).addClass("active");
                    } else {
                        $(this).removeClass("active");
                    }
                });

                toastr.success(response.messsage);
            }else
            {
                toastr.error(response.messsage);
            }

        },
        error: function () {
            toastr.error("Error ocuured while marking order served.");

        }
    });
}
