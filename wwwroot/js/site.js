$(document).ready(function () {
    // --- Navbar Scroll Effect ---
    // Check if we are on the homepage (where the navbar is initially transparent)
    var mainNavbar = $('#main-navbar');
    if (mainNavbar.hasClass('navbar-transparent')) {
        $(window).on('scroll', function () {
            // Add 'scrolled' class to navbar if user scrolls down more than 50px
            if ($(window).scrollTop() > 50) {
                mainNavbar.addClass('scrolled');
            } else {
                mainNavbar.removeClass('scrolled');
            }
        });
    }

    // --- "Add to Cart" AJAX Functionality ---
    // Update the cart badge count as soon as the page loads
    updateCartBadge();

    // Attach click handler to any button with the 'add-to-cart-btn' class
    $(document).on('click', '.add-to-cart-btn', function (e) {
        e.preventDefault(); // Prevent the link from navigating away
        var button = $(this);
        var url = button.attr('href');

        $.ajax({
            type: 'POST',
            url: url,
            success: function (response) {
                if (response.success) {
                    updateCartBadge(); // Re-fetch count to update the badge
                    showToast('Property added to cart!');
                    button.prop('disabled', true).text('Added!');
                    setTimeout(function () {
                        button.prop('disabled', false).text('Add to Cart');
                    }, 2000);
                } else {
                    showToast(response.error || 'Failed to add property.', true);
                }
            },
            error: function () {
                showToast('An error occurred. Please try again.', true);
            }
        });
    });
});

// --- Helper Functions ---

// Function to get the current cart count from the server
function updateCartBadge() {
    $.get('/Buyer/GetCartCount', function (response) {
        updateBadgeDisplay(response.count);
    }).fail(function () {
        // Silently fail if the user is not a buyer or not logged in
        updateBadgeDisplay(0);
    });
}

// Function to physically update the badge number in the navbar
function updateBadgeDisplay(count) {
    var badge = $('#cart-badge');
    if (badge.length > 0) {
        if (count > 0) {
            badge.text(count).show();
        } else {
            badge.hide();
        }
    }
}

// Function to show a pop-up message (a "toast")
function showToast(message, isError = false) {
    $('.toast-message').remove(); // Remove any existing toast

    var toast = $('<div class="toast-message"></div>').text(message);
    if (isError) {
        toast.addClass('error');
    }
    $('body').append(toast);

    setTimeout(function () {
        toast.fadeOut(500, function () {
            $(this).remove();
        });
    }, 3000);
}