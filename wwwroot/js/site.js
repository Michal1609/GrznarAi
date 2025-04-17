// Toggle dark/light theme
function toggleTheme(isDark) {
    if (isDark) {
        document.documentElement.setAttribute('data-bs-theme', 'dark');
        localStorage.setItem('theme', 'dark');
    } else {
        document.documentElement.setAttribute('data-bs-theme', 'light');
        localStorage.setItem('theme', 'light');
    }
}

// Check if user prefers dark mode
function isDarkModePreferred() {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
        return savedTheme === 'dark';
    }
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
}

// Initialize carousel
function initCarousel(carouselId) {
    try {
        var myCarousel = document.getElementById(carouselId);
        if (myCarousel) {
            var carousel = new bootstrap.Carousel(myCarousel, {
                interval: 5000,
                wrap: true,
                touch: true,
                ride: 'carousel'
            });

            // Force start the carousel
            carousel.cycle();

            console.log('Carousel initialized successfully');
            return true;
        } else {
            console.error('Carousel element not found: ' + carouselId);
            return false;
        }
    } catch (error) {
        console.error('Error initializing carousel:', error);
        return false;
    }
}

// Initialize dropdowns
document.addEventListener('DOMContentLoaded', function() {
    // Initialize all dropdowns
    var dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
    var dropdownList = dropdownElementList.map(function(dropdownToggleEl) {
        return new bootstrap.Dropdown(dropdownToggleEl);
    });

    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function(tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize all carousels on the page
    var carouselElementList = [].slice.call(document.querySelectorAll('.carousel'));
    carouselElementList.forEach(function(carouselEl) {
        try {
            var carousel = new bootstrap.Carousel(carouselEl, {
                interval: 5000,
                wrap: true,
                touch: true,
                ride: 'carousel'
            });
            carousel.cycle();
        } catch (error) {
            console.error('Error initializing carousel:', error);
        }
    });
});
