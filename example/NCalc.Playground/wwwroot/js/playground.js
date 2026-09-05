(() => {
    const themeKey = "theme";

    const parentTheme = () => {
        if (window.parent === window) {
            return null;
        }

        try {
            return window.frameElement?.getAttribute("data-bs-theme")
                ?? window.parent.document.documentElement.getAttribute("data-bs-theme")
                ?? window.parent.document.body?.getAttribute("data-bs-theme")
                ?? null;
        } catch {
            return null;
        }
    };

    const preferredTheme = () => {
        const stored = localStorage.getItem(themeKey);
        if (stored === "light" || stored === "dark") {
            return stored;
        }

        return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    };

    const setTheme = theme => {
        const resolved = theme === "dark" ? "dark" : "light";
        document.documentElement.setAttribute("data-bs-theme", resolved);

        const toggle = document.querySelector(".theme-toggle");
        if (toggle) {
            const nextTheme = resolved === "dark" ? "light" : "dark";
            toggle.setAttribute("aria-label", `Switch to ${nextTheme} theme`);
            toggle.setAttribute("title", `Switch to ${nextTheme} theme`);
        }
    };

    window.ncalcPlayground = window.ncalcPlayground || {};
    window.ncalcPlayground.toggleTheme = () => {
        const nextTheme = document.documentElement.getAttribute("data-bs-theme") === "dark" ? "light" : "dark";
        localStorage.setItem(themeKey, nextTheme);
        setTheme(nextTheme);
    };

    setTheme(parentTheme() ?? preferredTheme());

    if (window.parent !== window) {
        try {
            const observed = [window.frameElement, window.parent.document.documentElement, window.parent.document.body].filter(Boolean);
            observed.forEach(element => {
                new MutationObserver(() => setTheme(parentTheme() ?? preferredTheme()))
                    .observe(element, { attributes: true, attributeFilter: ["data-bs-theme"] });
            });
        } catch {
            // Cross-origin embedding keeps the locally resolved theme.
        }
    }
})();
