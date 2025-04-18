// Funkce pro získání cookie podle jména
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}

// Funkce pro nastavení cookie
function setCookie(name, value, days) {
    let expires = '';
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = `; expires=${date.toUTCString()}`;
    }
    document.cookie = `${name}=${value}${expires}; path=/; SameSite=Lax`;
}

// Funkce pro generování náhodného ID uživatele pro cookies
function generateUserId() {
    const array = new Uint32Array(4);
    window.crypto.getRandomValues(array);
    return Array.from(array, dec => ('0'.repeat(8) + dec.toString(16)).substr(-8)).join('');
}

// Funkce pro získání nebo vytvoření ID uživatele pro cookies
function getUserCookieId() {
    let userId = getCookie('blog_user_id');
    if (!userId) {
        userId = generateUserId();
        setCookie('blog_user_id', userId, 365); // Platnost 1 rok
    }
    return userId;
}

// Funkce pro inicializaci systému komentářů
function initCommentSystem() {
    // Kontrola, zda existuje ID uživatele
    getUserCookieId();
}

// Funkce pro sdílení blogu
function shareBlog(data) {
    if (navigator.share) {
        navigator.share({
            title: data.title,
            text: data.text,
            url: data.url
        })
        .catch(error => console.error('Error sharing:', error));
    } else {
        // Fallback pro prohlížeče bez podpory Web Share API
        prompt('Zkopírujte a sdílejte tento odkaz:', data.url);
    }
}

// Funkce pro získání základní URL stránky
function getBaseUrl() {
    return window.location.origin;
}

// Funkce pro sdílení na sociálních sítích
function shareToFacebook() {
    const url = encodeURIComponent(window.location.href);
    window.open(`https://www.facebook.com/sharer/sharer.php?u=${url}`, '_blank');
}

function shareToTwitter() {
    const url = encodeURIComponent(window.location.href);
    const title = encodeURIComponent(document.title);
    window.open(`https://twitter.com/intent/tweet?url=${url}&text=${title}`, '_blank');
}

function shareToLinkedIn() {
    const url = encodeURIComponent(window.location.href);
    window.open(`https://www.linkedin.com/sharing/share-offsite/?url=${url}`, '_blank');
}

function shareByEmail() {
    const url = window.location.href;
    const title = document.title;
    const body = `Podívej se na tento zajímavý článek: ${url}`;
    window.open(`mailto:?subject=${encodeURIComponent(title)}&body=${encodeURIComponent(body)}`, '_blank');
}

// Přidat funkce do globálního objektu window
window.getUserCookieId = getUserCookieId;
window.initCommentSystem = initCommentSystem;
window.shareBlog = shareBlog;
window.getBaseUrl = getBaseUrl;
window.shareToFacebook = shareToFacebook;
window.shareToTwitter = shareToTwitter;
window.shareToLinkedIn = shareToLinkedIn;
window.shareByEmail = shareByEmail; 