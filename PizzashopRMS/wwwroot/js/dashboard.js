$(document).ready(function () {
    LoadPartialDashboard();

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

    // Close dropdown when clicking outside
    $(document).click(function (event) {
        if (!$(event.target).closest(".custom-select-box").length) {
            $("#optionsContainerDashboard").hide();
        }
    });
})

function toggleDropdown() {
    $("#optionsContainerDashboard").toggle();
}

function ApplyFilters(filterval) {
    console.log(filterval);
    $("#selectedFilterTime").text(filterval);
    $("#optionsContainerDashboard").hide();
    console.log("aaaaaa",filterval);
    if (filterval != "Custom date") {
        LoadPartialDashboard(filterval);
    } else {
        $("#customDateModal").modal('show');
        $("#dateSelectionError").text("");
    }
}

function customDate() {
    var startDate = $("#custFromDate").val();
    var endDate = $("#custToDate").val();
    if(startDate == null || endDate == null || startDate == "" || endDate == "")
    {
        $("#customDateError").show();
        return;
    }
    console.log("Start Date: " + startDate);
    console.log("End Date: " + endDate);

    $("#customDateModal").modal("hide");
    LoadPartialDashboard("Custom Date", startDate, endDate);
}

function LoadPartialDashboard(timeinterval="", startDate="", endDate=""){
    $.ajax({
        url:"/Dashboard/LoadDashboardDataPartial",
        type: "GET",
        data: {
            timeinterval: timeinterval,
            startDate: startDate,
            endDate: endDate
        },
        success: function (data) {
            $("#dashboardContainer").html(data);
            const revenueData = JSON.parse($("#revenueDataJson").val() || "[]");
            console.log("data",revenueData);
            renderRevenueChart(revenueData);
            const customerData = JSON.parse($("#customerDataJson").val() || "[]");
            renderCustomerChart(customerData);
        }
    });
}

function formatLabels(data) {
    return data.map(d => {
        const date = new Date(d.date);
        if (data.length > 1500) {
            // Only Year
            return new Intl.DateTimeFormat('en-GB', {
                year: 'numeric'
            }).format(date);
        } else if (data.length > 300) {
            // Month + Year
            return new Intl.DateTimeFormat('en-GB', {
                month: 'short',
                year: '2-digit'
            }).format(date);
        } else {
            // Day + Month + Year
            return new Intl.DateTimeFormat('en-GB', {
                day: '2-digit',
                month: 'short',
                year: '2-digit'
            }).format(date);
        }
    });
}

function renderRevenueChart(revenueData) {
    const labels = formatLabels(revenueData);
    const values = revenueData.map(x => x.totalRevenue);
    console.log(revenueData);
    const ctx = document.getElementById('revenueChart');
    if (!ctx) return;

    const chartCtx = ctx.getContext('2d');
    new Chart(chartCtx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Revenue',
                data: values,
                borderColor: 'rgba(75, 192, 192, 1)',
                fill: true,
                backgroundColor: 'rgba(75, 192, 192, 0.2)'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                x: {
                    ticks: {
                        maxTicksLimit: 12,
                        color: '#666',
                        font: {
                            size: window.innerWidth < 600 ? 8 : 12 // Adjust label font size based on screen width
                        }
                    },
                    grid: {
                        display: false
                    }
                },
                y: {
                    beginAtZero: true,
                    ticks: {
                        color: '#666',
                        font: {
                            size: window.innerWidth < 600 ? 8 : 12
                        }
                    },
                    grid: {
                        color: '#eee'
                    }
                }
            }
        }
    });
}


function renderCustomerChart(customerData) {
    const labels = formatLabels(customerData);
    const values = customerData.map(x => x.totalCustomers);

    const ctx = document.getElementById('customerChart');
    if (!ctx) return;

    const chartCtx = ctx.getContext('2d');
    new Chart(chartCtx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Customer Growth',
                data: values,
                borderColor: 'rgba(153, 102, 255, 1)',
                fill: true,
                backgroundColor: 'rgba(153, 102, 255, 0.2)'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false, // corrected typo
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                x: {
                    ticks: {
                        maxTicksLimit: 12,
                        color: '#666',
                        font: {
                            size: window.innerWidth < 600 ? 8 : 12 // Responsive font size
                        }
                    },
                    grid: {
                        display: false
                    }
                },
                y: {
                    beginAtZero: true,
                    ticks: {
                        color: '#666',
                        font: {
                            size: window.innerWidth < 600 ? 8 : 12 // Responsive font size
                        }
                    },
                    grid: {
                        color: '#eee'
                    }
                }
            }
        }
    });
}


