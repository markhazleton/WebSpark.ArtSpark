// Password Strength Checker
(function () {
    'use strict';

    const passwordInput = document.getElementById('password-input');
    if (!passwordInput) return;

    const strengthContainer = document.getElementById('password-strength');
    const strengthLabel = document.getElementById('strength-label');
    const strengthBar = document.getElementById('strength-bar');

    const commonPasswords = [
        'password', '123456', '12345678', 'qwerty', 'abc123', 'monkey',
        '1234567', 'letmein', 'trustno1', 'dragon', 'baseball', 'iloveyou',
        'master', 'sunshine', 'ashley', 'bailey', 'passw0rd', 'shadow',
        '123123', '654321', 'superman', 'qazwsx', 'michael', 'football'
    ];

    function calculateStrength(password) {
        if (!password) return 0;

        let score = 0;

        // Length scoring
        if (password.length >= 8) score += 20;
        if (password.length >= 12) score += 10;
        if (password.length >= 16) score += 10;

        // Character variety
        if (/[a-z]/.test(password)) score += 15;
        if (/[A-Z]/.test(password)) score += 15;
        if (/[0-9]/.test(password)) score += 15;
        if (/[^a-zA-Z0-9]/.test(password)) score += 15;

        // Penalize common patterns
        if (commonPasswords.includes(password.toLowerCase())) score = Math.min(score, 30);
        if (/^[0-9]+$/.test(password)) score = Math.min(score, 40);
        if (/(.)\1{2,}/.test(password)) score -= 10; // Repeated characters
        if (/^[a-z]+$/.test(password) || /^[A-Z]+$/.test(password)) score -= 5;

        return Math.max(0, Math.min(100, score));
    }

    function getStrengthInfo(score) {
        if (score < 30) return { label: 'Weak', color: 'bg-danger', textClass: 'text-danger' };
        if (score < 60) return { label: 'Fair', color: 'bg-warning', textClass: 'text-warning' };
        if (score < 80) return { label: 'Good', color: 'bg-info', textClass: 'text-info' };
        return { label: 'Strong', color: 'bg-success', textClass: 'text-success' };
    }

    passwordInput.addEventListener('input', function () {
        const password = this.value;

        if (password.length === 0) {
            strengthContainer.style.display = 'none';
            return;
        }

        strengthContainer.style.display = 'block';

        const score = calculateStrength(password);
        const info = getStrengthInfo(score);

        strengthLabel.textContent = info.label;
        strengthLabel.className = info.textClass;

        strengthBar.style.width = score + '%';
        strengthBar.className = 'progress-bar ' + info.color;
    });
})();
