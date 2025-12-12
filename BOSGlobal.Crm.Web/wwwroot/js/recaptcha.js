window.recaptcha = (function () {
    let grecaptchaLoaded = false;
    let grecaptchaLoading = false;

    function loadGrecaptcha() {
        return new Promise((resolve, reject) => {
            if (grecaptchaLoaded && window.grecaptcha) return resolve(window.grecaptcha);
            if (grecaptchaLoading) {
                const t = setInterval(() => { if (window.grecaptcha) { clearInterval(t); resolve(window.grecaptcha); } }, 100);
                return;
            }
            grecaptchaLoading = true;
            const s = document.createElement('script');
            s.src = 'https://www.google.com/recaptcha/api.js?render=explicit';
            s.onload = () => {
                grecaptchaLoaded = true;
                resolve(window.grecaptcha);
            };
            s.onerror = reject;
            document.head.appendChild(s);
        });
    }

    async function executeRecaptcha(siteKey) {
        if (!siteKey) return null;
        const gre = await loadGrecaptcha();
        // use v3 execute if available
        if (gre && gre.execute) {
            return await gre.execute(siteKey, { action: 'submit' });
        }
        return null;
    }

    return {
        executeRecaptcha: executeRecaptcha
    };
})();
