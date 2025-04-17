// Carousel initialization
document.addEventListener('DOMContentLoaded', function() {
    // Find all carousels on the page
    var carousels = document.querySelectorAll('.carousel');
    
    // Initialize each carousel
    carousels.forEach(function(carouselElement) {
        try {
            // Create new carousel instance
            var carousel = new bootstrap.Carousel(carouselElement, {
                interval: 5000,
                wrap: true,
                ride: 'carousel'
            });
            
            console.log('Carousel initialized:', carouselElement.id);
            
            // Add manual event listeners for navigation
            var prevButton = carouselElement.querySelector('.carousel-control-prev');
            var nextButton = carouselElement.querySelector('.carousel-control-next');
            
            if (prevButton) {
                prevButton.addEventListener('click', function(e) {
                    e.preventDefault();
                    carousel.prev();
                    console.log('Manual prev clicked');
                });
            }
            
            if (nextButton) {
                nextButton.addEventListener('click', function(e) {
                    e.preventDefault();
                    carousel.next();
                    console.log('Manual next clicked');
                });
            }
            
            // Add event listeners for indicators
            var indicators = carouselElement.querySelectorAll('.carousel-indicators button');
            indicators.forEach(function(indicator) {
                indicator.addEventListener('click', function(e) {
                    e.preventDefault();
                    var slideIndex = this.getAttribute('data-bs-slide-to');
                    carousel.to(parseInt(slideIndex));
                    console.log('Indicator clicked:', slideIndex);
                });
            });
            
        } catch (error) {
            console.error('Error initializing carousel:', error);
        }
    });
});
