
$(document).ready(function () {
    $('#EnterCoupon').submit(function (e) {
        e.preventDefault();
        $.ajax({
            url: '/cart/ApplyCoupon',
            type: 'POST',
            data: $(this).serialize(),
            success: function (response) {
                console.log(response); // عرض الاستجابة في وحدة التحكم

                if (response.success) {
                    $('#discountedPriceA').text(response.discountedPrice.value.discountedPrice.toFixed(2));
                    $('#FinalPriceA').text(response.discountedPrice.value.finalPrice.toFixed(2));
                    swal.fire("Success", response.message, "success");
                } else {
                    swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                console.log("خطأ في الاستجابة:", xhr.responseText); // يعرض الخطأ في وحدة التحكم
                swal.fire("Error", "حدث خطأ أثناء تطبيق الكوبون.", "error");
            }
        });
    });
});