$(document).ready(function () {
    $('#soso').on('submit', function (event) {
        event.preventDefault(); // منع السلوك الافتراضي للنموذج

        // جمع البيانات من النموذج
        var formData = $(this).serialize(); // يمكنك استخدام .serialize() لجمع كل القيم

        $.ajax({
            url: '/ShopDetails/Add', // استبدل هذا بالرابط الصحيح
            type: 'POST',
            data: formData,
            success: function (response) {
                console.log(response);
                // معالجة الاستجابة من الخادم
                swal.fire("Success", response.message, "success");
                if (response.count !== undefined) {
                    document.getElementById("cartItemCount").innerText = response.count;
                }

            },
            error: function (xhr, status, error) {
                swal.fire("Error", response.message, "error");
            }
        });
    });
});