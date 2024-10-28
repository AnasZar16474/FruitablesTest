document.querySelectorAll('.star').forEach(star => {
    star.addEventListener('mouseover', function () {
        // عند تحريك الفأرة فوق النجوم
        let value = this.getAttribute('data-value');
        highlightStars(value);
    });

    star.addEventListener('mouseout', function () {
        // عند إزالة الفأرة عن النجوم
        resetStars();
        let selectedValue = document.querySelector('.star.selected');
        if (selectedValue) {
            highlightStars(selectedValue.getAttribute('data-value'));
        }
    });

    star.addEventListener('click', function () {
        // عند النقر لتحديد التقييم
        let value = this.getAttribute('data-value');
        setRating(value);
    });
});

function highlightStars(value) {
    document.querySelectorAll('.star').forEach(star => {
        if (star.getAttribute('data-value') <= value) {
            star.classList.add('hover');
        } else {
            star.classList.remove('hover');
        }
    });
}

function resetStars() {
    document.querySelectorAll('.star').forEach(star => {
        star.classList.remove('hover');
    });
}

function setRating(value) {
    document.querySelectorAll('.star').forEach(star => {
        star.classList.remove('selected');
        if (star.getAttribute('data-value') <= value) {
            star.classList.add('selected');
        }
    });

    document.getElementById('ratingValue').value = value;
}
