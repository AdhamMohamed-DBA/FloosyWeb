// Simple local biometric helper using WebAuthn (platform authenticator).
// This is used ONLY as an extra lock on this device, on top of Firebase email/password.
// It does NOT replace real backend WebAuthn verification.

window.floosyBiometric = {
    isSupported: function () {
        return !!(window.PublicKeyCredential && navigator.credentials && window.crypto && window.localStorage);
    },

    // Returns true if a credential has been created on this device for Floosy.
    isEnabled: function () {
        if (!this.isSupported()) return false;
        return !!localStorage.getItem("floosy_bio_cred");
    },

    // Create a platform credential and store its ID locally.
    // Returns true on success, false otherwise.
    register: async function () {
        if (!this.isSupported()) return false;

        try {
            const challenge = new Uint8Array(32);
            crypto.getRandomValues(challenge);

            const userId = new Uint8Array(16);
            crypto.getRandomValues(userId);

            const publicKey = {
                challenge,
                rp: {
                    name: "Floosy",
                    id: window.location.hostname
                },
                user: {
                    id: userId,
                    name: "floosy-user",
                    displayName: "Floosy User"
                },
                pubKeyCredParams: [{ type: "public-key", alg: -7 }],
                authenticatorSelection: {
                    authenticatorAttachment: "platform",
                    userVerification: "required"
                },
                timeout: 60000,
                attestation: "none"
            };

            const cred = await navigator.credentials.create({ publicKey });
            if (!cred) return false;

            const rawId = btoa(String.fromCharCode(...new Uint8Array(cred.rawId)));
            localStorage.setItem("floosy_bio_cred", rawId);
            return true;
        } catch (e) {
            console.error("Biometric register failed", e);
            return false;
        }
    },

    // Ask the authenticator to verify the stored credential.
    // Returns true if the user successfully completes biometric / PIN.
    verify: async function () {
        if (!this.isSupported()) return false;

        const stored = localStorage.getItem("floosy_bio_cred");
        if (!stored) return false;

        try {
            const rawId = Uint8Array.from(atob(stored), c => c.charCodeAt(0));
            const challenge = new Uint8Array(32);
            crypto.getRandomValues(challenge);

            const publicKey = {
                challenge,
                timeout: 60000,
                rpId: window.location.hostname,
                allowCredentials: [{
                    id: rawId,
                    type: "public-key",
                    transports: ["internal"]
                }],
                userVerification: "required"
            };

            const assertion = await navigator.credentials.get({ publicKey });
            return !!assertion;
        } catch (e) {
            console.error("Biometric verify failed", e);
            return false;
        }
    },

    disable: function () {
        if (!this.isSupported()) return;
        localStorage.removeItem("floosy_bio_cred");
    }
};

