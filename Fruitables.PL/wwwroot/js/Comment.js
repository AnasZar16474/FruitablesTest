
$(document).ready(function () {
    $('#addReviewForm').submit(function (e) {
        e.preventDefault();
        $.ajax({
            url: '/ShopDetails/AddReview',
            type: 'POST',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                   swal.fire("Success", response.message, "success");
                } else {
                   swal.fire("Error", response.message, "error");
                }
            }
        }); 
    })
})
