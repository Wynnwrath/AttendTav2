(() => {
    'use strict';
    const searches = Array.from(document.querySelectorAll('[data-list-search]'));
    searches.forEach(form => {
        const search = form.querySelector('[name="search"]');
        function update() {
            const applied = form.dataset.appliedSearch;
            const unchanged = applied && search.value.trim() === applied.trim();
            form.querySelector('[data-search-clear]').hidden = !unchanged;
            form.querySelector('[data-search-submit]').hidden = !!unchanged;
        }
        search.addEventListener('input', update);
        update();
    });

    // AJAX record saves already reload the current URL; their JSON results stay intact.
    document.addEventListener('submit', event => {
        const form = event.target;
        if (form.method !== 'post' || !form.closest('main, dialog')) return;
        const current = new URL(window.location.href);
        const action = new URL(form.action, current);
        if (action.origin !== current.origin) return;
        current.searchParams.forEach((value, key) => {
            if (!action.searchParams.has(key)) action.searchParams.set(key, value);
        });
        form.action = action.toString();
        let origin = form.querySelector('[name="uiReturnUrl"]');
        if (!origin) {
            origin = document.createElement('input');
            origin.type = 'hidden'; origin.name = 'uiReturnUrl'; form.append(origin);
        }
        origin.value = current.pathname + current.search;
    }, true);

    const footer = document.querySelector('[data-paging-size]');
    if (!footer || footer.dataset.pagingMode === 'fixed') return;
    let resized = false, timer;
    let viewportWidth = window.innerWidth, viewportHeight = window.innerHeight;
    function fitRows() {
        if (document.querySelector('dialog[open]') || document.activeElement?.matches('input, select, textarea')) return;
        if (searches.some(form => form.querySelector('[name="search"]').value.trim() !== form.dataset.appliedSearch.trim())) return;
        const table = footer.parentElement.querySelector('.table-scroll table') || document.querySelector('.table-scroll table');
        const grid = table ? null : document.querySelector('[data-paging-grid]');
        if (!table && !grid) return;
        const rows = Array.from(table ? table.querySelectorAll('tbody > tr') : grid.children).filter(row => !row.querySelector('[colspan]'));
        if (!rows.length) return;
        const size = Number(footer.dataset.pagingSize);
        const rowHeight = Math.max(48, ...rows.map(row => row.getBoundingClientRect().height));
        const top = (table ? rows[0] : grid).getBoundingClientRect().top + window.scrollY;
        const columns = grid ? rows.filter(row => Math.abs(row.getBoundingClientRect().top - rows[0].getBoundingClientRect().top) < 2).length : 1;
        const gap = grid ? parseFloat(window.getComputedStyle(grid).rowGap) || 0 : 0;
        const available = window.innerHeight - top - footer.getBoundingClientRect().height - 32;
        const desired = Math.max(1, Math.min(5, Math.max(1, Math.floor((available + gap) / (rowHeight + gap))) * columns));
        const url = new URL(window.location.href);
        // Hold the size while paging the same query. Refit after a filter change
        // or a real resize, rather than repeatedly navigating for different row text.
        if (url.searchParams.has('fit') && !resized) return;
        if (desired === size) return;
        const firstIndex = (Number(footer.dataset.pagingPage) - 1) * size;
        url.searchParams.set('pageSize', desired);
        url.searchParams.set('page', Math.floor(firstIndex / desired) + 1);
        url.searchParams.set('fit', '1');
        window.location.replace(url.toString());
    }
    window.addEventListener('resize', () => {
        // Scrollbar appearance and small panel jitter must not restart the paging fit.
        if (Math.abs(window.innerWidth - viewportWidth) < 32 && Math.abs(window.innerHeight - viewportHeight) < 32) return;
        viewportWidth = window.innerWidth;
        viewportHeight = window.innerHeight;
        resized = true;
        clearTimeout(timer);
        timer = setTimeout(fitRows, 400);
    });
    if (document.fonts) document.fonts.ready.then(fitRows);
    else fitRows();
})();
