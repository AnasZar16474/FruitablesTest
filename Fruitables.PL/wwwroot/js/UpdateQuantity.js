function updateQuantity(amount, event) {
    event.preventDefault();
    var quantityInput = document.getElementById("quantityInput");
    var currentQuantity = parseInt(quantityInput.value);

    if (!isNaN(currentQuantity) && currentQuantity + amount >= 1) {
        quantityInput.value = currentQuantity + amount;
    }
}