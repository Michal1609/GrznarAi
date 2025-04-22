// MobileMenu.js - Handles mobile navigation for GrznarAI

document.addEventListener('DOMContentLoaded', function() {
    // Initialize mobile menu handlers
    initMobileMenu();
});

function initMobileMenu() {
    console.log('Mobile menu initialization started');
    
    // Find navbar elements
    const navbarToggler = document.querySelector('.navbar-toggler');
    const navbarCollapse = document.querySelector('.navbar-collapse');
    
    if (!navbarToggler || !navbarCollapse) {
        console.error('Navbar elements not found, aborting menu initialization');
        return;
    }
    
    console.log('Navbar elements found, setting up event handlers');
    
    // Override click handler for toggler to ensure it works
    navbarToggler.addEventListener('click', function(event) {
        event.preventDefault();
        event.stopPropagation();
        
        // Toggle the collapsed state
        const isCurrentlyVisible = navbarCollapse.classList.contains('show');
        console.log('Navbar toggler clicked, current state:', isCurrentlyVisible);
        
        if (isCurrentlyVisible) {
            navbarCollapse.classList.remove('show');
            navbarToggler.setAttribute('aria-expanded', 'false');
        } else {
            navbarCollapse.classList.add('show');
            navbarToggler.setAttribute('aria-expanded', 'true');
        }
        
        return false;
    });
    
    // Close menu when clicking outside
    document.addEventListener('click', function(event) {
        if (navbarCollapse.classList.contains('show') &&
            !navbarCollapse.contains(event.target) &&
            !navbarToggler.contains(event.target)) {
            
            console.log('Clicked outside menu, closing');
            navbarCollapse.classList.remove('show');
            navbarToggler.setAttribute('aria-expanded', 'false');
        }
    });
    
    // Close menu on window resize if viewport becomes desktop size
    window.addEventListener('resize', function() {
        if (window.innerWidth >= 992 && navbarCollapse.classList.contains('show')) {
            console.log('Window resized to desktop size, closing mobile menu');
            navbarCollapse.classList.remove('show');
            navbarToggler.setAttribute('aria-expanded', 'false');
        }
    });
    
    // Create Blazor interop function
    window.toggleMobileMenu = function(show) {
        console.log('toggleMobileMenu called from Blazor with show:', show);
        
        if (typeof show === 'boolean') {
            if (show) {
                navbarCollapse.classList.add('show');
                navbarToggler.setAttribute('aria-expanded', 'true');
            } else {
                navbarCollapse.classList.remove('show');
                navbarToggler.setAttribute('aria-expanded', 'false');
            }
        } else {
            // Toggle current state
            const isCurrentlyVisible = navbarCollapse.classList.contains('show');
            if (isCurrentlyVisible) {
                navbarCollapse.classList.remove('show');
                navbarToggler.setAttribute('aria-expanded', 'false');
            } else {
                navbarCollapse.classList.add('show');
                navbarToggler.setAttribute('aria-expanded', 'true');
            }
        }
        
        return navbarCollapse.classList.contains('show');
    };
    
    console.log('Mobile menu initialization complete');
} 