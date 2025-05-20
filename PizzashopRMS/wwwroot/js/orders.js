$(document).ready(function () {
    let today = new Date().toISOString().slice(0, 10);
    $("#orderFromDate").attr("max", today);
    $("#orderToDate").attr("max", today);

    $("#orderFromDate").on("change", function () {
        let fromdate = $("#orderFromDate").val();
        $("#orderToDate").attr("min", fromdate);
    });

});

function StatusOnChange() {
    console.log("Status Drop Down");
    let data = getDataValues();
    data.Pagenumber = 1;
    data.OrderStatusId = $(searchOrderSelect).val();
    OrderUpdatePage(data.Pagenumber, data.Pagesize, data.Searchkey, data.OrderStatusId, data.sortDr, data.sortCol, data.lastDays, data.startDate, data.endDate);
}

function TimeOnChange() {
    let data = getDataValues();
    data.Pagenumber = 1;
    data.lastDays = $(searchTimeFilter).val();
    OrderUpdatePage(data.Pagenumber, data.Pagesize, data.Searchkey, data.OrderStatusId, data.sortDr, data.sortCol, data.lastDays, data.startDate, data.endDate);
}

function OrderUpdatePage(Pagenumber = 1, Pagesize = 5, Searchkey = "", OrderStatusId = 0, sortDr = "asc", sortCol = "OrderNo", lastDays = "All Time", startDate = "", endDate = "") {
    console.log(startDate);
    $.ajax({
        url: "/Orders/OrderListTable",
        type: "GET",
        data: {
            Pagenumber: Pagenumber,
            Pagesize: Pagesize,
            Searchkey: Searchkey,
            sortDr: sortDr,
            sortCol: sortCol,
            OrderStatusId: OrderStatusId,
            lastDays: lastDays,
            startDate: startDate,
            endDate: endDate
        },
        success: function (data) {
            $("#orderTablePartialView").html(data);
        },
        error: function () {
            toastr.error("Error Order List Table");
        }
    });
}

function UpdatePageSizeOrder() {
    var data = getDataValues();
    data.Pagenumber = 1;
    data.Pagesize = $("#ordersPerPageList").val();
    OrderUpdatePage(data.Pagenumber, data.Pagesize, data.Searchkey, data.OrderStatusId, data.sortDr, data.sortCol, data.lastDays, data.startDate, data.endDate);
}

function updateListPageTable(dir) {
    var data = getDataValues();

    if (dir == 'back')
        data.Pagenumber = data.Pagenumber - 1;
    else if (dir == 'next')
        data.Pagenumber = data.Pagenumber + 1;

    OrderUpdatePage(data.Pagenumber, data.Pagesize, data.Searchkey, data.OrderStatusId, data.sortDr, data.sortCol, data.lastDays, data.startDate, data.endDate);

}

function onKeySearch() {
    var data = getDataValues();
    data.Pagenumber = 1;
    data.Searchkey = $("#searchOrderInput").val();
    OrderUpdatePage(data.Pagenumber, data.Pagesize, data.Searchkey, data.OrderStatusId, data.sortDr, data.sortCol, data.lastDays, data.startDate, data.endDate);

}

function ApplyFilters() {
    var data = getDataValues();
    data.Pagenumber = 1;
    data.OrderStatusId = $("#searchOrderSelect").val();
    data.lastDays = $("#searchTimeFilter").val();
    data.sortCol = "Orderid";
    data.sortDr = "asc";
    data.startDate = $("#orderFromDate").val();
    data.endDate = $("#orderToDate").val();
    OrderUpdatePage(data.Pagenumber, data.Pagesize, data.Searchkey, data.OrderStatusId, data.sortDr, data.sortCol, data.lastDays, data.startDate, data.endDate);

}

function ClearFilters() {
    var data = getDataValues();
    data.Pagenumber = 1;

    data.OrderStatusId = 0;
    $("#searchOrderSelect").val(0);

    data.lastDays = "All Time";
    $("#searchTimeFilter").val("All Time");

    data.sortCol = "OrderNo";
    data.sortDr = "asc";

    data.startDate = "";
    $("#orderFromDate").val("");

    data.endDate = "";
    $("#orderToDate").val("");

    data.Searchkey = "";
    $("#searchOrderInput").val("");

    OrderUpdatePage(data.Pagenumber, data.Pagesize, data.Searchkey, data.OrderStatusId, data.sortDr, data.sortCol, data.lastDays, data.startDate, data.endDate);
}

function sortCol(sortDr, sortCol) {
    var data = getDataValues();
    data.sortDr = sortDr;
    data.sortCol = sortCol;
    OrderUpdatePage(data.Pagenumber, data.Pagesize, data.Searchkey, data.OrderStatusId, data.sortDr, data.sortCol, data.lastDays, data.startDate, data.endDate);

}

function exportToExcelFile() {
    var search = $("#searchOrderInput").val();
    var orderStatus = $("#searchOrderSelect").val();
    var lastDays = $("#searchTimeFilter").val();
    const loader = document.getElementById("pageLoader");
    if (loader) {
        loader.classList.remove("hidden");
    }

    $.ajax({
        url: "/Orders/ExportToExcelFile",
        type: "GET",
        data: {
            Searchkey: search,
            OrderStatusId: orderStatus,
            lastDays: lastDays
        },
        xhrFields: {
            responseType: 'blob' // This ensures we receive binary data
        },
        success: function (data, status, xhr) {
            var blob = new Blob([data], { type: xhr.getResponseHeader("Content-Type") });
            var link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = "Orders.xlsx";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr) {
            if (xhr.status === 400) {
                toastr.warning("No records found.");
            } else {
                toastr.error("An error occurred while exporting.");
            }
        },
        complete: function () {
            if (loader) {
                loader.classList.add("hidden");
            }
        }
    });
}

function GenerateInvoiceOrder(orderId) {
    if (orderId == null || orderId <= 0) {
        toastr.error("OrderId is null");
        return;
    }

    // Show loader
    const loader = document.getElementById("pageLoader");
    if (loader) {
        loader.classList.remove("hidden");
    }

    $.ajax({
        url: "/Orders/ExportToPdf",
        type: "GET",
        data: {
            orderId: orderId
        },
        xhrFields: {
            responseType: 'blob' // This ensures we receive binary data
        },
        success: function (data, status, xhr) {
            var blob = new Blob([data], { type: xhr.getResponseHeader("Content-Type") });
            var link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = "Invoice.pdf";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.location.reload();
        },
        error: function (xhr) {
            if (xhr.status === 400) {
                toastr.warning("No records found.");
            } else {
                toastr.error("An error occurred while exporting.");
            }
        },
        complete: function () {
            // Always hide the loader once request completes
            if (loader) {
                loader.classList.add("hidden");
            }
        }
    });
}

function exportToExcelFileOrderDetails(orderId) {
    if (orderId == null || orderId <= 0) {
        toastr.error("OrderId is null");
        return;
    }

    // Show loader
    const loader = document.getElementById("pageLoader");
    if (loader) {
        loader.classList.remove("hidden");
    }

    $.ajax({
        url: "/Orders/OrderDetails",
        type: "GET",
        data: {
            orderId: orderId
        },
        xhrFields: {
            responseType: 'blob' // This ensures we receive binary data
        },
        success: function (data, status, xhr) {
            var blob = new Blob([data], { type: xhr.getResponseHeader("Content-Type") });
            var link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = "OrdersDetails.xlsx";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.location.reload();
        },
        error: function (xhr) {
            if (xhr.status === 400) {
                toastr.warning("No records found.");
            } else {
                toastr.error("An error occurred while exporting.");
            }
        },
        complete: function () {
            // Always hide the loader once request completes
            if (loader) {
                loader.classList.add("hidden");
            }
        }
    });
}