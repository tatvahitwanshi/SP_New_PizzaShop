$(document).ready(function () {
    let today = new Date().toISOString().slice(0, 10);
    $("#custFromDate").attr("max", today);
    $("#custToDate").attr("max", today);

    $("#custFromDate").on("change", function () {
        let fromdate = $("#custFromDate").val();
        $("#custToDate").attr("min", fromdate);
    });

    $("#custToDate").on("change", function () {
        let fromdate = $("#custToDate").val();
        $("#custFromDate").attr("max", fromdate);
    });

});



function UpdatePage(Pagenumber = 1, Pagesize = 5, Searchkey = "", SortDr = "asc", SortCol = "OrderId", LastDays = "All Time", StartDate = "", EndDate = "")
{
    console.log(Pagesize);
    $.ajax({
        url: "/Customers/CustomerListTable",
        type: "GET",
        data: {
            Pagenumber: Pagenumber,
            Pagesize: Pagesize,
            Searchkey: Searchkey,
            SortDr: SortDr,
            SortCol: SortCol,
            LastDays: LastDays,
            StartDate: StartDate,
            EndDate: EndDate
        },
        success: function (data) {
            $("#customerTablePartialView").html(data);
        },
        error:function(ex){
            toastr.error(`Error ocurred in Customer List Table ${ex}`,);
        }
    });
}

function SortCol(SortDr, SortCol)
{
    var params = getValues();
    params.SortDr = SortDr;
    params.SortCol = SortCol;
    UpdatePage(params.Pagenumber, params.Pagesize, params.Searchkey, params.SortDr, params.SortCol, params.LastDays, params.StartDate, params.EndDate);

}

function doSearch()
{
    var params = getValues();
    params.Pagenumber = 1;
    params.Searchkey = $("#searchCustomerInput").val();
    UpdatePage(params.Pagenumber, params.Pagesize, params.Searchkey, params.SortDr, params.SortCol, params.LastDays, params.StartDate, params.EndDate);
}

function UpdatePageSize()
{
    var params = getValues();
    params.Pagenumber = 1;
    params.Pagesize = $("#customerPerPageList").val();
    UpdatePage(params.Pagenumber, params.Pagesize, params.Searchkey, params.SortDr, params.SortCol, params.LastDays, params.StartDate, params.EndDate);
}

function updateListPageTable(dir)
{
    var params = getValues();
    if (dir == 'back')
        params.Pagenumber = params.Pagenumber - 1;
    else if (dir == 'next') 
        params.Pagenumber = params.Pagenumber + 1;

    UpdatePage(params.Pagenumber, params.Pagesize, params.Searchkey, params.SortDr, params.SortCol, params.LastDays, params.StartDate, params.EndDate);

}

function toggleDropdown() {
    $("#optionsContainer").toggle();
}

function ApplyFilters(filterval) {
    console.log(filterval);
    
    $("#selectedFilter").text(filterval);
    $("#optionsContainer").hide();

    if (filterval !== "Custom") {
        var params = getValues();
        params.Pagenumber = 1;
        params.lastDays = filterval;
        UpdatePage(params.Pagenumber, params.Pagesize, params.Searchkey, params.sortDr, params.sortCol, params.lastDays, "", "");
    } else {
        $("#customDateModal").modal('show');
    }
}

function customDate() {
    var params = getValues();
    params.startDate = $("#custFromDate").val();
    params.endDate = $("#custToDate").val();
    params.lastDays = "Custom";

    if (params.startDate === "" || params.endDate === "") {
        $("#customDateError").show();
    } else {
        $("#customDateError").hide(); // Hide if everything is valid
        $("#customDateModal").modal('hide');
        UpdatePage(
            params.Pagenumber,
            params.Pagesize,
            params.Searchkey,
            params.sortDr,
            params.sortCol,
            params.lastDays,
            params.startDate,
            params.endDate
        );
    }
}


function exportToExcelFile()
{
    var search = $("#searchCustomerInput").val();
    var lastDays = $("#selectedFilter").text().trim();
    var startDate = $("#custFromDate").val();
    var endDate = $("#custToDate").val();

    $.ajax({
        url: "/Customers/ExportToExcel",
        type: "GET",
        data: {
            Searchkey: search,
            LastDays: lastDays,
            StartDate: startDate,
            EndDate: endDate
        },
        xhrFields: {
            responseType: 'blob' // This ensures we receive binary data
        },
        success: function (data, status, xhr) {
            var blob = new Blob([data], { type: xhr.getResponseHeader("Content-Type") });
            var link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = "Customers.xlsx";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            toastr.success("Exported successfully")
        },
        error: function (xhr) {
            if (xhr.status === 400) {
                toastr.warning("No records found.");
            } else {
                toastr.error("An error occurred while exporting.");
            }
        }
    });
}

function CustomerHistory(id)
{
    $.ajax({
        url: "/Customers/CustomersHistory",
        type: "GET",
        data: { id: id },
        success: function (data) {
            $("#customerHistoryModal").html(data);
            $("#customerModal").modal('show');
        },
        error:function(ex){
            toastr.error(`Error ocurred in customer history ${ex}`,);
        }
    });
}