// function openEditModal(categoryId) {
//     $.ajax({
//         url: '/Menu/EditCategory/' + categoryId,
//         type: 'GET',
//         success: function (data) {
//             if (data) {
//                 console.log("Fetched Data:", data);
//                 $('#editCategoryId').val(data.categoryid);
//                 $('#editCategoryName').val(data.categoryname);
//                 $('#editCategoryDescription').val(data.categorydescription);
//                 $('#staticBackdrop2').modal('show');
//             } else {
//                 alert("No category data found.");
//             }
//         },
//         error: function () {
//             alert("Failed to load category details.");
//         }
//     });
// }

let categoryIdToDelete = null;
var deleteCategoryId = 0;

function openDeleteModal(categoryId) {
    console.log("Delete Category ID:", categoryId);
    deleteCategoryId = categoryId;
    $('#deleteModal').modal('show');
}

$('#confirmDeleteBtn').click(function () {
    console.log("Deleting Category ID:", deleteCategoryId);
    $.ajax({
        url: '@Url.Action("DeleteCategory", "Menu")',
        type: 'POST',
        data: { categoryId: deleteCategoryId },
        success: function (response) {
            location.reload();
        },
        error: function () {
            toastr.error('An error occurred while deleting the category.');
        }
    });
});

$(document).ready(function () {
    $('button[data-bs-toggle="tab"]').on('click', function (e) {
        var target = $(e.target).attr("data-bs-target");
       
        if (target === '#nav-home') {
            $('#items-content').load('~/Views/Menu/_PartialViewTab.cshtml');
        } else if (target === '#nav-profile') {
            $('#nav-profile').html('Tab2');
        }
    });
});
let deleteItemIds = [];