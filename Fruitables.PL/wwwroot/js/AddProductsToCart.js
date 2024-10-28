function addToCart(button, event) {
    event.preventDefault();
    event.stopPropagation(); // يمنع تفعيل الرابط <a>
    const url = button.getAttribute("formaction");
    console.log(url)
    $.ajax({
        url: url,
        type: 'POST',
        success: function (response) {
            console.log(response);
            Swal.fire({
                icon: 'success',
                title: 'تمت العملية',
                text: 'تم إضافة المنتج إلى السلة بنجاح!',
                confirmButtonText: 'حسنًا'
            });
            if (response.cartItemCount !== undefined) {
                document.getElementById("cartItemCount").innerText = response.cartItemCount;
            }
        },
        error: function (xhr) {
            Swal.fire({
                icon: 'error',
                title: 'خطأ',
                text: 'حدث خطأ أثناء إضافة المنتج. حاول مرة أخرى.',
                confirmButtonText: 'حسنًا'
            });
        }
    });
}