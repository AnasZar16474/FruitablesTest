function updateCartItemCount() {
    $.ajax({
        url: '/Base/GetCartItemCount',
        type: 'GET',
        success: function (data) {
            $('#cartItemCount').text(data.count);
        },
        error: function () {
            console.log("حدث خطأ أثناء جلب البيانات.");
        }
    });
}
function updateTotalPrice() {
    $.ajax({
        url: '/Base/GetTotalPrice',
        type: 'GET',
        success: function (data) {
            $('#totalPrice').text(data.totalPrice.toFixed(2));
        },
        error: function () {
            console.log("حدث خطأ أثناء جلب مجموع الأسعار.");
        }
    });
}
function updateDiscountAmount(totalPrice, discount) {
    $.ajax({
        url: '/Cart/GetTotalPriceAfterDiscount',
        type: 'GET',
        data: {
            totalPrice: totalPrice,
            discount: discount
        },
        success: function (data) {
            console.log(data)
            if (data && data.discountedPrice !== undefined && data.finalPrice !== undefined) {
                $('#discountedPriceA').text(data.discountedPrice.toFixed(2));
                $('#FinalPriceA').text(data.finalPrice.toFixed(2));
            } else {
                console.log("لم يتم العثور على البيانات المطلوبة في الرد.");
            }
        },
        error: function () {
            console.log("حدث خطأ أثناء جلب مجموع الأسعار بعد الخصم.");
        }
    });
}


$(document).ready(function () {
    updateCartItemCount();
    updateTotalPrice();
    let totalPrice = configData.totalPrice;
    let discount = configData.discount;
    updateDiscountAmount(totalPrice, discount); // استدعاء الدالة مع القيم الديناميكية

});