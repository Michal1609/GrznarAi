// This file can be used for general purpose JavaScript
// Currently empty after removing theme logic.

(() => {
    'use strict'

    // Any future site-wide JS can go here.

    // reCAPTCHA v3 integration
    window.initializeRecaptcha = function(siteKey) {
        if (typeof grecaptcha === 'undefined') {
            // Load reCAPTCHA script if not already loaded
            const script = document.createElement('script');
            script.src = `https://www.google.com/recaptcha/api.js?render=${siteKey}`;
            script.async = true;
            script.defer = true;
            document.head.appendChild(script);
            
            // Ensure it's only loaded once
            window.recaptchaSiteKey = siteKey;
        }
    };

    window.executeRecaptcha = function(action) {
        return new Promise((resolve, reject) => {
            if (typeof grecaptcha === 'undefined') {
                console.error('reCAPTCHA has not been loaded');
                reject('reCAPTCHA not loaded');
                return;
            }
            
            grecaptcha.ready(() => {
                grecaptcha.execute(window.recaptchaSiteKey, { action: action })
                    .then(token => resolve(token))
                    .catch(error => reject(error));
            });
        });
    };

    // Cookie utility for anonymous users
    window.getUserCookieId = function() {
        const cookieName = 'AnonymousUserId';
        let cookieId = getCookie(cookieName);
        
        if (!cookieId) {
            cookieId = generateUUID();
            setCookie(cookieName, cookieId, 365); // 1 year expiration
        }
        
        return cookieId;
    };

    function generateUUID() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    function setCookie(name, value, days) {
        let expires = '';
        if (days) {
            const date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = '; expires=' + date.toUTCString();
        }
        document.cookie = name + '=' + value + expires + '; path=/; SameSite=Strict';
    }

    function getCookie(name) {
        const nameEQ = name + '=';
        const ca = document.cookie.split(';');
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i];
            while (c.charAt(0) === ' ') c = c.substring(1);
            if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    }

    // Bootstrap Navbar toggler initialization
    window.toggleMobileMenu = function(show) {
        console.log("Toggle menu called, show:", show);
        const navbarCollapse = document.querySelector('.navbar-collapse');
        if (navbarCollapse) {
            if (show === undefined) {
                // Toggle current state if no parameter is provided
                navbarCollapse.classList.toggle('show');
                console.log("Toggled navbar state:", navbarCollapse.classList.contains('show'));
            } else {
                // Otherwise, set the state based on the parameter
                if (show) {
                    navbarCollapse.classList.add('show');
                    console.log("Added 'show' class to navbar");
                } else {
                    navbarCollapse.classList.remove('show');
                    console.log("Removed 'show' class from navbar");
                }
            }
        } else {
            console.error("Navbar collapse element not found");
        }
        
        // Return a value for debugging
        return show;
    };

    // Close mobile menu when clicking outside
    document.addEventListener('click', function(event) {
        const navbarCollapse = document.querySelector('.navbar-collapse.show');
        if (navbarCollapse) {
            // If user clicks outside the expanded navbar
            if (!navbarCollapse.contains(event.target) && 
                !event.target.classList.contains('navbar-toggler') &&
                !event.target.closest('.navbar-toggler')) {
                window.toggleMobileMenu(false);
            }
        }
    });

    // Close mobile menu when window resizes to desktop size
    window.addEventListener('resize', function() {
        if (window.innerWidth >= 992) { // lg breakpoint in Bootstrap
            const navbarCollapse = document.querySelector('.navbar-collapse.show');
            if (navbarCollapse) {
                window.toggleMobileMenu(false);
            }
        }
    });

})() 