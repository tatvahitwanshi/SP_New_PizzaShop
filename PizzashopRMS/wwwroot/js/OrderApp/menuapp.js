
$(document).ready(function () {
    updateMenu(0);
    calculateTotal(0);
    createMainOrderJSON();
});
// Update Menu -------------------------------------------------------------------
function updateMenu(id = null, searchKey = null) {

    if (id == null) {
        id = $("#current-category-id").val();
    }

    if (searchKey === null) {
        searchKey = $("#search-input-for-items").val();
    }

    setCurrentCategoryColor(id);
    loadMenuData(id, searchKey);
}
function loadMenuData(id, searchKey) {
    $.ajax({
        url: '/MenuApp/ChangeCategory',
        type: 'GET',
        data: { CategoryId: id, SearchKey: searchKey },
        success: function (response) {
            $("#itemValuesContainer").html(response);
        },
        error: function (xhr) {
            console.log("Status: " + xhr.status);
            console.log("Response Text: " + xhr.responseText);
            toastr.error('Error loading content');
        }
    });
}

function setCurrentCategoryColor(id) {
    $(".category-card").each(function () {
        if ($(this).data("categoryid") == id) $(this).addClass("active-card");
        else $(this).removeClass("active-card");
    });
}

function markFavourite(element, itemId) {
    var isFavourite = element.classList.contains("bi-heart");

    if (isFavourite) {
        element.classList.remove("bi-heart");
        element.classList.add("bi-heart-fill");
    } else {
        element.classList.remove("bi-heart-fill");
        element.classList.add("bi-heart");

        var id = $("#current-category-id").val();
        if (id == -1) {
            // remove closest item-card
            $(element).closest(".item-card").remove();
        }
    }

    $.ajax({
        url: '/MenuApp/MarkFavouriteItem',
        type: 'POST',
        data: { itemId: itemId, isFavourite: isFavourite },
        success: function (response) {
            if(response.success)
            {
                toastr.success("Favourite status updated.");
            }
            else
            {
                toastr.error("Error while marking item favourtite.");
            }
        },
        error: function (xhr) {
            console.log("Status: " + xhr.status);
            console.log("Response Text: " + xhr.responseText);
            toastr.error('Error while marking Favourite Item');
        }
    });
}

// Modifier List Modal ------------------------------------------------
// select modifiers for order in app
function showModifierListModal(id) {
    // console.log(status);
    // if(status == "True")
    // {
        getModifiersListModal(id);
        var modal = new bootstrap.Modal(document.getElementById("ModifierListModal"));
        modal.show();
    // }
}
function getModifiersListModal(id) {
    $.ajax({
        url: '/MenuApp/GetModifierList',
        type: 'GET',
        data: { ItemId: id },
        success: function (response) {
            if(response)
            {
                $("#ModifierListModalContainer").html(response);
    
                const json = $("#modifierlist-model-json").html();
                const data = JSON.parse(json);
                mapCurrentData(data);
            }
            else
            {
                toastr.error('Error loading content Get Modifier List');
            }
        },
        error: function () {
            toastr.error('Error loading content Get Modifier List');
        }
    });
}
var currItem;
// var currOrder = [];
var mainOrder;
function createItemJSON(itemId, itemName, itemType, price, quantity = 1, instruction = "", isEdit=true, isServed=false) {
    const uniqueId = generateUniqueIdForItem();
    return {
        id: uniqueId,
        itemId: itemId,
        itemName: itemName,
        itemType: itemType,
        price: price,
        quantity: quantity,
        instruction: instruction,
        modifierList: [],
        isEdit: isEdit,
        isServed : isServed,
        readyQuantity : 0,
        servedQuantity : 0
        
    }
}
function generateUniqueIdForItem()
{
    let id = 0;
    mainOrder.currOrder.forEach(item => {
        if (item.id) {
            if(item.id > id)
            {
                id = item.id;
            }
        }
    });

    return id+1;
}
function createModifierJSON(modifierId, modifierName, price, groupId) {
    return {
        modifierId: modifierId,
        modifierName: modifierName,
        price: price,
        groupId: groupId
    }
}
function mapCurrentData(data) {

    currItem = createItemJSON(data.itemId, data.itemName, data.itemType, data.price);
    console.log(currItem);
}
function toggleModifier(element)
{
    if($("#tableIdForCheck").val() == -1)
    {
        return;
    }
    const groupId =element.dataset.groupid;
    const max = +element.dataset.max;
    const modifierId =element.dataset.modifierid;
    const modifierName = element.dataset.modifiername;
    const price = element.dataset.price;
    const selectedCards = document.querySelectorAll(`.modifier-card[data-groupid='${groupId}'].selected`);
    const isSelected = element.classList.contains('selected');

    if (isSelected) {
        element.classList.remove('selected');
        currItem.modifierList = currItem.modifierList.filter(mod => mod.modifierId !== modifierId || mod.groupId !== groupId);
    } else if (selectedCards.length < max) {
        element.classList.add('selected');
        currItem.modifierList.push(createModifierJSON(modifierId, modifierName, price, groupId));
    } else {
        toastr.warning(`You can only select up to ${max} item(s).`);
    }
}

// json structures -------------------------------------------------------------------
function createMainOrderJSON()
{
    const model = JSON.parse($("#main-model-json").html());
    console.log(model.tokenOrderDetails);

    var Id = 0;
    var tokenOrOrder = "";
    var customerId = 0;
    var currOrder = [];
    var currTaxes = [];
    var instruction = "";
    var customerDetails = null;
    var maxTableCapacity=0;
    
    if($("#tableIdForCheck").val() != -1)
    {
        Id = model.tokenOrderDetails.id;
        tokenOrOrder = model.tokenOrderDetails.tokenORorder;
        customerId = model.tokenOrderDetails.customerId;
        customerDetails = model.tokenOrderDetails.customerDetail;
        numberOfPerson= model.tokenOrderDetails.numberOfPerson;
        maxTableCapacity= model.tokenOrderDetails.maxTableCapacity;

        if(tokenOrOrder == "order")
        {
            currOrder = model.tokenOrderDetails.currOrder;
            currOrder.forEach(e=>{
                addItemInOrder(e);
            });
            instruction = model.tokenOrderDetails.instruction;
        }
    }

    mainOrder = {
        id : Id,
        tokenOrOrder: tokenOrOrder,
        customerId : customerId,
        instruction : instruction,
        currOrder : currOrder,
        currTaxes : currTaxes,
        customerDetails : customerDetails,
        numberOfPerson : numberOfPerson,
        maxTableCapacity: maxTableCapacity
    }
    
}
function addItemInOrder(item, isEdit = false)
{
    const id = item.id;
    item = JSON.stringify(item);
    $.ajax({
        url: '/MenuApp/AddOrderedItem',
        type: 'GET',
        data: { itemValues: item },
        success: function (response) {
                if($("#item-rows-container").hasClass("nothing-added"))
                {
                    $("#item-rows-container").removeClass("nothing-added");
                    $("#item-rows-container").html(response);
                }else
                {
                    $("#item-rows-container").append(response);
                }
                calculateSubTotal();
        },
        error: function () {
            toastr.error('Error loading content add order item');
        }
    });
}
function validateMinModifierSelection() {
    const modifierGroups = document.querySelectorAll('.modifier-card[data-groupid]');
    const groupMap = {};

    modifierGroups.forEach(card => {
        const groupId = card.dataset.groupid;
        const min = +card.dataset.min;

        if (!groupMap[groupId]) {
            groupMap[groupId] = { min: min, selectedCount: 0 };
        }

        if (card.classList.contains('selected')) {
            groupMap[groupId].selectedCount++;
        }
    });

    for (const groupId in groupMap) {
        const { min, selectedCount } = groupMap[groupId];
        if (selectedCount < min) {
            toastr.error(`Please select at least ${min} item(s) in each required group.`);
            return false;
        }
    }

    return true;
}

function SaveNewItem() {
    if (!validateMinModifierSelection()) return;
    const modal = bootstrap.Modal.getInstance(document.getElementById('ModifierListModal'));
    modal.hide();
    if(isItemAlreadyAdded())
        return;
    mainOrder.currOrder.push(currItem);
    addItemInOrder(currItem);
}


function arraysEqualAsSets(a, b) {
    if (a.length !== b.length) return false;

    const count = (arr) => {
        const map = new Map();
        for (const val of arr) {
            map.set(val, (map.get(val) || 0) + 1);
        }
        return map;
    };

    const countA = count(a);
    const countB = count(b);

    if (countA.size !== countB.size) return false;

    for (const [key, val] of countA.entries()) {
        if (countB.get(key) !== val) return false;
    }

    return true;
}

function isItemAlreadyAdded() {
    const matchedItems = mainOrder.currOrder.filter(i => i.itemId == currItem.itemId);
    if (matchedItems.length == 0) return false;

    for (let i = 0; i < matchedItems.length; i++) {
        let item = matchedItems[i];

        let itemModifierIDs = item.modifierList.map(mod => parseInt(mod.modifierId));
        let currModifierIDs = currItem.modifierList.map(mod => parseInt(mod.modifierId));

        if (arraysEqualAsSets(itemModifierIDs, currModifierIDs)) {
            changeQuantityItem(document.querySelector(`#new-add-quantity-${item.id}`), "inc", item.id);
            return true;
        }
    }

    return false;
}

// }

// change quantity of item in order--------------------------------------------------------
function changeQuantityItem(btn, action, id) {
    const wrapper = btn.closest('.quantity-box');
    const display = wrapper.querySelector('.quantity-box-display');
    let quantity = parseInt(display.textContent);
    let item = mainOrder.currOrder.find(i => i.id == id);
    if (item == null) return;
    
    
    if (action == 'inc') {
        quantity++;
    } else if (action == 'dec') {
        if (quantity <= (item.readyQuantity + item.servedQuantity)) {
            toastr.warning("Cannot decrease the quantity");
        } else if (quantity > 1) {
            quantity--;
        }
    }
    const row = btn.closest('tr');
    const itemAmount = row.querySelector('.item-amount');
    const modifierAmount = row.querySelector('.modifier-amount');

    const itemTotalAmount = parseFloat(itemAmount.getAttribute("data-amount")?.replace(/[^\d.-]/g, "") || 0);
    const modifierTotalAmount = parseFloat(modifierAmount.getAttribute("data-amount")?.replace(/[^\d.-]/g, "") || 0);

    itemAmount.textContent = "₹" + (itemTotalAmount * quantity).toFixed(2);
    modifierAmount.textContent = "₹" + (modifierTotalAmount * quantity).toFixed(2);
    display.textContent = quantity;

    updateCurrentOrder(id, quantity);
    calculateSubTotal();
}

function updateCurrentOrder(id, quantity)
{
    let item = mainOrder.currOrder.find(i=> i.id == id);
    if (item) {
        item.quantity = quantity;
    } else {
        console.log("Item not found in current order.");
    }
}

//taxes-----------------------------------------------------------
function calculateSubTotal() {
    let subtotal = 0;
    $(".item-sums").each(function () {
        let itemAmount = parseFloat($(this).find(".item-amount").html().replace(/[^0-9.]/g, "") || "0");
        let modifierAmount = parseFloat($(this).find(".modifier-amount").html().replace(/[^0-9.]/g, "") || "0");

        subtotal += itemAmount + modifierAmount;
    });

    console.log("Subtotal:", subtotal);
    calculateTotal(subtotal);
}
function calculateTotal(subtotal) {
    subtotal = parseFloat(subtotal);
    $("#order-subtotal").text(subtotal.toFixed(2)); // ensure 2 decimals

    let currTotal = subtotal;
    var taxCards = $(".tax-card");
    
    taxCards.each(function () {
        const taxValue = parseFloat($(this).data("taxvalue"));
        const taxType = $(this).data("taxvaluetype");
        const isDefault = $(this).data("defaulttax");

        let currTax = 0;

        if(isDefault == "False")
        {
            currTax += dealWithNotDefault(this, taxValue, taxType, isDefault);
        }else
        {
            if (taxType == "Amount") {
                currTax = subtotal != 0 ? taxValue : 0;
            } else if (taxType == "Percentage") {
                currTax = (subtotal * taxValue) / 100;
            }
            currTax = parseFloat(currTax.toFixed(2)); // round to 2 decimals
            $(this).find('.tax-value').html(currTax.toFixed(2)); // also show 2 decimals
        }

        currTotal += currTax;
    });

    currTotal = parseFloat(currTotal.toFixed(2)); // round total as well
    $("#order-total").text(currTotal.toFixed(2)); // display 2 decimals
}
function dealWithNotDefault(element, taxValue, taxType) {
    const checkbox = $(element).find('.tax-checkbox');

    if (checkbox.is(':checked')) {
        const subtotal = parseFloat($("#order-subtotal").text());
        let currTax = 0;

        if (taxType === "Amount") {
            currTax = subtotal !== 0 ? taxValue : 0;
        } else if (taxType === "Percentage") {
            currTax = (subtotal * taxValue) / 100;
        }

        currTax = parseFloat(currTax.toFixed(2));
        $(element).find('.rupee-sign').html("₹");
        $(element).find('.tax-value').html(currTax.toFixed(2)); 
        return currTax;
    } else {
        // Unchecked — show nothing
        $(element).find('.tax-value').html("");
        $(element).find('.rupee-sign').html("");
        return 0;
    }
}


// Give Insturction ----------------------------------------------------------------------------------
function giveInstruction(id = 0)
{
    $("#instructionModal").modal('show');
    $("#itemIdForInstruction").val(id);
    $("#InstructionsInput").prop("disabled", false);
    $("#saveInstructionBtn").removeClass("d-none");


    if(id == 0)
    {
        $("#instructionModalName").html("Order Wise Comment");
        $("#InstructionsInput").val(mainOrder.instruction);
    }else
    {
        $("#instructionModalName").html("Special Instruction");
        $("#InstructionsInput").val(mainOrder.currOrder.find(i=> i.id == id).instruction);
        
        if(mainOrder.currOrder.find(i=> i.id == id).quantity - mainOrder.currOrder.find(i=> i.id == id).servedQuantity - mainOrder.currOrder.find(i=> i.id == id).readyQuantity <= 0)
        {
            $("#InstructionsInput").prop("disabled", true);
            $("#saveInstructionBtn").addClass("d-none");
        }
    }
}
function saveInstructionForItem()
{
    var id = parseInt($("#itemIdForInstruction").val());
    var instruction = $("#InstructionsInput").val();
    console.log(id, instruction);

    if(id==0)
    {
        mainOrder.instruction = instruction;
        var instructionElement = document.querySelector("#main-instruction-container");
        
        if(!instruction || instruction.trim() === "")
        {
            if(instructionElement)
                instructionElement.remove();
            return;
        }
        if(!instructionElement)
        {
            var htmlToPrepend = 
                `<div id="main-instruction-container">
                    <span class="special-color fw-bold me-1">Instruction: </span>
                    <span id="main-instruction">${instruction}</span>
                </div>`;
                
            $(".tax-container").prepend(htmlToPrepend);

        }else
        {
            console.log("inside isntruction");
            $("#main-instruction").html(instruction);
        }
    }else
    {
        mainOrder.currOrder.find(i=>i.id == id).instruction = instruction;

        var instructionElement = document.querySelector(`#order-item-${id} .item-instruction`);
        if(!instruction || instruction.trim() === "")
        {
            if(instructionElement)
                instructionElement.remove();

            return;
        }
        if(!instructionElement)
        {
            var htmlToaAppend =
                `<li class="mt-2 item-instruction">
                    <span class="special-color fw-bold me-1">Instruction: </span>
                    <span class="instruction-detail text-break" style="word-break: break-word; white-space: pre-wrap;"> ${instruction}</span>
                </li>`;

            $(`#modifier-list-for-order-${id}`).append(htmlToaAppend);
        }else
        {
            var descElement = instructionElement.querySelector(".instruction-detail");
            if (descElement) {
                descElement.textContent = instruction;
            }
        }
    }

    console.log(mainOrder);

}

// place order -------------------------------------------------------------------
function addTaxesInOrder()
{
    var taxes = [];

    $(".tax-card").each(function (){
        const taxId = parseInt($(this).data("taxid"));
        const defaulttax = $(this).data("defaulttax");

        if(defaulttax == "False" && !$(this).find('.tax-checkbox').is(':checked')) return true;

        taxes.push(taxId);
    });

    return taxes;
}
function PlaceOrder()
{
    mainOrder.currTaxes = addTaxesInOrder();
    if(mainOrder.currOrder.length == 0)
    {
        toastr.warning("Select at least 1 Item to place order");
        return;
    }
    const orderDetails = JSON.stringify(mainOrder);
    console.log(orderDetails);
    $.ajax({
        url: '/MenuApp/PlaceOrder',
        type: 'POST',
        data: { placeOrder: orderDetails },
        success: function (response) {
            console.log(response);
            if(response.success)
            {
                toastr.success("Order Placed Successfully");

            }else
            {
                toastr.error("Error placing order");
            }
        },
        error: function () {
            toastr.error('Error loading content');
        }
    });
}

//delete item from order------------------------------------------------------------
function deleteItemFromOrder(id) {
    let item = mainOrder.currOrder.find(i=> i.id == id && i.isEdit == true);
    if(item == null) return;
    $("#item-row-" + id).remove();
    mainOrder.currOrder = mainOrder.currOrder.filter(i => i.id !== id);
    calculateSubTotal();
}

function openCustomerDetailsModalPartial()
{
    if($("#tableIdForCheck").val() == -1)
    {
        toastr.warning("Select table to add customer details");
        return;
    }
    $.ajax({
        url: '/MenuApp/GetCustomerDetailsModal',
        type: 'GET',
        data:{
            numberOfPerson: mainOrder.numberOfPerson,
            tokenOrOrder: mainOrder.tokenOrOrder,
            id : mainOrder.id,
            customerDetails: JSON.stringify(mainOrder.customerDetails)

        },
        success: function (response) {
            $("#customerDetailsModalContainer").html(response);
            const modal = new bootstrap.Modal(document.getElementById("customerDetailsModal"));
            modal.show();  
     
        },
        error: function () {
            toastr.error('Error loading content Get Customer Details Modal');
        }
    });
    
}

function checkCapacityTableInput(value)
{
    if(value == null)
    {
        $("#maxCapacityTableError").text("");
    }
    if (value > mainOrder.maxTableCapacity) {
        $("#maxCapacityTableError").text(`Number of person should be less than or equal to ${mainOrder.maxTableCapacity}`);
        $("#maxCapacityTableErrorAsp").text("");
    } else {
        $("#maxCapacityTableError").text("");
        $("#maxCapacityTableErrorAsp").text("");
    }
}
$(document).on("submit", "#customerDetailsForm", function (e) {
    e.preventDefault();

    if($("#customerCount").val() > mainOrder.maxCapacity)
    {
        $("#maxCapacityTableError").text(`Number of person should be less than or equal to ${mainOrder.maxTableCapacity}`);
        $("#maxCapacityTableErrorAsp").text("");
        return;
    }

    var form = $(this);
    var formData = form.serialize();
    
    $.ajax({
        url: '/MenuApp/SaveCustomerDetails',
        type: 'POST',
        data: formData,
        success: function (response) {
            
            if(response.success)
            {
                $("#customerDetailsModal").modal("hide");
                toastr.success(response.message); 
                
                mainOrder.customerId = response.customerId;
                mainOrder.customerDetails.email = $("#customerAddress").val();
                mainOrder.customerDetails.name = $("#customerName").val();
                mainOrder.customerDetails.phone = $("#customerPhone").val(); 
                mainOrder.numberOfPerson = $("#customerCount").val();
            }else
            {
                toastr.error("Error while saving the customer details");
            }
        },
        error: function () {
            toastr.error("Error while saving the customer details");
        }
    });

});


//complete order ----------------------------------------------

function completeOrder(orderId)
{
    $.ajax({
        url: '/MenuApp/CompleteOrder',
        type: 'GET',
        data: { orderId: orderId },
        success: function (response) {
            $("#completeOrderAppModalContainer").html(response);
            $("#CompleteOrderAppModal").modal('show');

            customerEventListner();

        },
        error: function () {
            toastr.error("Error fetching details Complete Order");
        }
    });
}
function toggleReviewModal(orderId)
{

    $.ajax({
        url: '/MenuApp/IsOrderCompletedToServed',
        type: 'GET',
        data: { orderId: orderId },
        success: function (response) {
            if(response.success)
            {
                $("#confirmModalReview").removeClass("d-none");

                $("#confirmModalCompletedOrderApp").addClass("d-none");
            }else
            {
                toastr.error("Order can not be completed!");
                $("#CompleteOrderAppModal").modal('hide');
            }
        },
        error: function () {
            toastr.error("Error Is Order Completed To Served");
        }
    });

}

function customerEventListner()
{
    document.querySelectorAll(".star-rating").forEach(group => {
        const stars = group.querySelectorAll(".star");
        const inputId = group.getAttribute("data-rating");
    
        stars.forEach((star, index) => {
            star.addEventListener("click", () => {
                // Set rating in hidden input
                const rating = index + 1;
                document.getElementById(inputId).value = rating;
    
                // Update stars visually
                stars.forEach((s, i) => {
                    s.classList.remove("bi-star", "bi-star-fill");
                    s.classList.add(i < rating ? "bi-star-fill" : "bi-star");
                });
            });
    
           
        });
    });
    
}
$(document).on("submit", "#completeOrderAppForm", function (e) {
    e.preventDefault();

    var formData = new FormData(this);
    // Get PaymentMethod value manually since it's outside the form
    var paymentMethod = $('input[name="PaymentMethod"]:checked').val();
    if (paymentMethod) {
        formData.append("PaymentMethod", paymentMethod);
    }

    var TaxList = addTaxesInOrder();
    TaxList.forEach(function (value) {
        formData.append("TaxList[]", value);
        console.log(value);
    });
    
    $.ajax({
        url: '/MenuApp/FinalCompleteOrderApp',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if(response.success)
            {
                // GenerateInvoiceComplete($('#OrderIdForInvoice').val());
                window.location.href = "/Orders/OrdersView";
            }else
            {
                toastr.error("Order can not be completed!");
                $("#CompleteOrderAppModal").modal('hide');
            }
        },
        error: function () {
            toastr.error("Error Completing token Final Complete Order App");
        }
    });

});

function openQRModal()
{
    var modal = new bootstrap.Modal(document.getElementById("ORModal"));
    modal.show();
}

function CancelOrder(orderId)
{
    var modal = new bootstrap.Modal(document.getElementById("CancelOrderAppModal"));
    modal.show();
    $('#cancelOrderId').val(orderId);
}

$(document).on("submit", "#cancelOrderAppForm", function (e){
    e.preventDefault();
    $.ajax({
        url:'/MenuApp/CancelOrderApp',
        type:'POST',
        data:{orderId : $('#cancelOrderId').val()},
        success: function(response)
        {   
            if(response.success) 
                {
                    toastr.success("Order Cancelled Successfully"); 
                    window.location.reload();
                    var modal = bootstrap.Modal.getInstance(document.getElementById("CancelOrderAppModal"));
                    modal.hide();
                }
            else toastr.error("Order can not be cancelled!");
        },
        error: function(){
            toastr.error('Error occur while Canceling the order');
        }
    })

});

function GenerateInvoiceComplete(orderId)
{
    if(orderId == null || orderId<=0)
    {
        toastr.error("OrderId is null");
    }

    $.ajax({
        url: "/MenuApp/ExportToPdf",
        type: "GET",
        data: {
            orderId:orderId
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
            // window.location.reload();
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