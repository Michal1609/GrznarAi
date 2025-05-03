// Google Analytics configuration
window.dataLayer = window.dataLayer || [];
function gtag() {
  dataLayer.push(arguments);
  console.log('GA call:', arguments);
}
gtag('js', new Date());

// Základní konfigurace Google Analytics
gtag('config', 'G-GNKF9TVGNV', {
  'debug_mode': true,
  'transport_type': 'beacon', // Změna na beacon, protože xhr může být blokováno
  'cookie_domain': 'localhost',
  'cookie_flags': 'samesite=none;secure',
  'cookie_expires': 63072000, // 2 roky v sekundách
  'page_location': window.location.href,
  'page_path': window.location.pathname,
  'page_title': document.title,
  'send_page_view': false // Disable automatic pageview to manually control it
});

// Globální proměnné pro sledování stavu
let lastTrackedUrl = '';
let lastTrackedTitle = '';
let lastTrackedTime = 0;
const TRACKING_DEBOUNCE = 1000; // Minimální interval mezi sledováním v milisekundách

// Explicitní přímé sledování stránek
window.trackPage = function(path, title) {
  // Pro Firefox ošetříme případ, kdy path neobsahuje úvodní /
  if (path && !path.startsWith('/')) {
    path = '/' + path;
  }
  
  // Kompletní URL pro sledování
  const fullUrl = window.location.origin + path;
  
  // Kontrola proti duplicitnímu sledování stejné stránky v krátkém čase
  const now = new Date().getTime();
  if (fullUrl === lastTrackedUrl && 
      title === lastTrackedTitle && 
      (now - lastTrackedTime) < TRACKING_DEBOUNCE) {
    console.log('*** Skipping duplicate page view tracking:', path, title);
    return; // Přeskočíme duplicitní sledování
  }
  
  // Aktualizujeme informace o posledním sledování
  lastTrackedUrl = fullUrl;
  lastTrackedTitle = title;
  lastTrackedTime = now;
  
  console.log('*** Tracking page view:', path, title);
  
  // Získáme client_id
  let clientId = getClientId();
  
  // Explicitně spustíme odesílání pomocí beacon API
  if ('sendBeacon' in navigator) {
    try {
      // Vytvoříme URL s parametry
      const beaconUrl = new URL('https://www.google-analytics.com/g/collect');
      beaconUrl.searchParams.append('v', '2');
      beaconUrl.searchParams.append('tid', 'G-GNKF9TVGNV');
      beaconUrl.searchParams.append('cid', clientId);
      beaconUrl.searchParams.append('sid', generateSessionId());
      beaconUrl.searchParams.append('dl', fullUrl);
      beaconUrl.searchParams.append('dt', title || document.title);
      beaconUrl.searchParams.append('ul', navigator.language);
      
      // Data ve formátu FormData
      const beaconData = new FormData();
      beaconData.append('en', 'page_view');
      beaconData.append('ep.page_location', fullUrl);
      beaconData.append('ep.page_path', path);
      beaconData.append('ep.page_title', title || document.title);
      
      navigator.sendBeacon(beaconUrl.toString(), beaconData);
      console.log('*** Beacon sent to:', beaconUrl.toString());
    } catch (e) {
      console.error('*** Failed to send beacon:', e);
      
      // Jako fallback použijeme standardní gtag
      gtag('event', 'page_view', {
        'page_location': fullUrl,
        'page_path': path,
        'page_title': title || document.title,
        'send_to': 'G-GNKF9TVGNV'
      });
    }
  } else {
    // Fallback pro prohlížeče bez podpory Beacon API
    gtag('event', 'page_view', {
      'page_location': fullUrl,
      'page_path': path,
      'page_title': title || document.title,
      'send_to': 'G-GNKF9TVGNV'
    });
  }
};

// Helper pro získání nebo vytvoření client_id
function getClientId() {
  // Zkusíme načíst _ga cookie
  const gaCookieMatch = document.cookie.match(/_ga=GA\d\.\d+\.(\d+\.\d+)/);
  if (gaCookieMatch && gaCookieMatch[1]) {
    return gaCookieMatch[1];
  }
  
  // Zkusíme najít client_id v localStorage
  const storedClientId = localStorage.getItem('ga_client_id');
  if (storedClientId) {
    return storedClientId;
  }
  
  // Vytvoříme nový client_id
  const newClientId = Math.round(2147483647 * Math.random()) + '.' + Math.round(new Date().getTime() / 1000);
  try {
    localStorage.setItem('ga_client_id', newClientId);
  } catch (e) {
    console.error('Failed to store client_id in localStorage:', e);
  }
  
  return newClientId;
}

// Vytvoření session ID pro GA
function generateSessionId() {
  // Zkusíme najít session ID v sessionStorage
  const storedSessionId = sessionStorage.getItem('ga_session_id');
  if (storedSessionId) {
    return storedSessionId;
  }
  
  // Vytvoříme nové session ID založené na čase
  const sessionId = Math.round(new Date().getTime() / 1000);
  try {
    sessionStorage.setItem('ga_session_id', sessionId);
  } catch (e) {
    console.error('Failed to store session_id in sessionStorage:', e);
  }
  
  return sessionId;
}

// Make sure cookies are enabled
function checkCookiesEnabled() {
  if (navigator.cookieEnabled === false) {
    console.warn('*** Cookies are disabled in this browser. Google Analytics may not work properly.');
    return false;
  }
  return true;
}

// Test zda můžeme nastavit cookies
function testCookies() {
  try {
    // Zkusíme nastavit a přečíst testovací cookie
    document.cookie = "ga_test=1; path=/; max-age=60";
    const hasCookie = document.cookie.indexOf("ga_test=") !== -1;
    console.log('*** Cookie test result:', hasCookie ? 'Cookies are enabled' : 'Cookies blocked');
    return hasCookie;
  } catch (e) {
    console.error('*** Cookie test error:', e);
    return false;
  }
}

// Automatické sledování změn URL
(function() {
  let lastUrl = window.location.href;
  
  // Check cookies on startup
  const cookiesEnabled = checkCookiesEnabled();
  const cookiesWritable = testCookies();
  
  if (!cookiesEnabled || !cookiesWritable) {
    console.warn('*** Sledování Google Analytics nemusí fungovat správně: cookies jsou zakázány nebo blokované');
  }
  
  // Sledování počáteční stránky - s malým zpožděním, aby se načetl správný titulek
  setTimeout(function() {
    window.trackPage(window.location.pathname, document.title);
  }, 100);
  
  // Kontrola změn URL každých 500ms - optimalizace pro SPA navigaci
  let checkUrlInterval = setInterval(function() {
    const currentUrl = window.location.href;
    if (currentUrl !== lastUrl) {
      console.log('*** URL changed:', lastUrl, '->', currentUrl);
      lastUrl = currentUrl;
      
      // Počkáme na aktualizaci titulku
      setTimeout(function() {
        window.trackPage(window.location.pathname, document.title);
      }, 50);
    }
  }, 500);
  
  // Sledování navigace zpět/vpřed
  window.addEventListener('popstate', function() {
    console.log('*** History navigation (popstate)');
    lastUrl = window.location.href; // Aktualizujeme lastUrl aby nedošlo k duplicitnímu sledování
    
    // Počkáme na aktualizaci stavu
    setTimeout(function() {
      window.trackPage(window.location.pathname, document.title);
    }, 50);
  });
  
  // Explicitně zachytit Firefox navigační události
  if (navigator.userAgent.indexOf('Firefox') > -1) {
    console.log('*** Firefox detected, adding extra event listeners');
    
    // Přidat listener na kliknutí na odkazy
    document.addEventListener('click', function(e) {
      // Kontrola, zda bylo kliknuto na odkaz
      let target = e.target;
      while (target && target.tagName !== 'A') {
        target = target.parentNode;
        if (!target) break;
      }
      
      if (target && target.tagName === 'A' && 
          target.href && 
          target.href.startsWith(window.location.origin) && 
          !target.hasAttribute('download') && 
          target.target !== '_blank') {
        console.log('*** Internal link clicked in Firefox:', target.href);
        
        // Nesledujeme přímo tady - necháme to na detekci změny URL v intervalu nebo
        // na popstate události, abychom neměli duplicitní sledování
      }
    });
  }
  
  // Přidání odkazu na GA v konzoli pro snadnější ladění
  console.log('Google Analytics tracking initialized with ID:', 'G-GNKF9TVGNV');
})();
