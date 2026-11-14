const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

function load({ height = 720, top = 250, rowHeight = 50, total = 50, url = 'http://localhost/Member?pageSize=5', editing = false, search = null, paging = true, grid = false, mode = 'auto' } = {}) {
    const events = {}, changes = [], forms = [];
    const table = { getBoundingClientRect: () => ({ top }), querySelector: () => ({ getBoundingClientRect: () => ({ height: 40 }) }), querySelectorAll: () => Array.from({ length: 5 }, () => ({ querySelector: () => null, getBoundingClientRect: () => ({ top: top + 40, height: rowHeight }) })) };
    const footer = { dataset: { pagingSize: '5', pagingPage: new URL(url).searchParams.get('page') || '1', pagingMode: mode, pagingTotal: String(total) }, getBoundingClientRect: () => ({ height: 64 }), parentElement: { querySelector: () => grid ? null : table } };
    const cards = { children: Array.from({ length: 5 }, (_, i) => ({ querySelector: () => null, getBoundingClientRect: () => ({ top: top + Math.floor(i / 2) * 220, height: 200 }) })), getBoundingClientRect: () => ({ top }) };
    if (search) forms.push(search);
    const document = {
        activeElement: { matches: () => editing },
        querySelectorAll: () => forms,
        querySelector: selector => selector === '[data-paging-size]' ? paging ? footer : null : selector === 'dialog[open]' ? null : selector === '[data-paging-grid]' ? cards : grid ? null : table,
        addEventListener: (event, callback) => { events[event] = callback; },
        createElement: () => ({})
    };
    const window = { getComputedStyle: () => ({ rowGap: '20px' }), innerWidth: 1366, innerHeight: height, scrollY: 0, location: { href: url, replace: value => changes.push(value) }, addEventListener: (event, callback) => { events[event] = callback; } };
    vm.runInNewContext(fs.readFileSync(path.join(__dirname, '../wwwroot/js/features/list-controls.js'), 'utf8'), { document, window, URL, setTimeout: callback => { callback(); return 1; }, clearTimeout() {} });
    return { events, changes, window };
}

test('auto fit uses the real row height and preserves filters when requesting fewer rows', () => {
    const { changes } = load({ height: 600, url: 'http://localhost/Member?search=Ana&course=BSIT&pageSize=5' });
    assert.equal(changes.length, 1);
    const url = new URL(changes[0]);
    assert.equal(url.searchParams.get('pageSize'), '4');
    assert.equal(url.searchParams.get('search'), 'Ana');
    assert.equal(url.searchParams.get('course'), 'BSIT');
    assert.equal(url.searchParams.get('fit'), '2');
});

test('fit waits for editing and uses taller screens beyond five rows', () => {
    assert.equal(load({ height: 500, editing: true }).changes.length, 0);
    assert.equal(new URL(load({ height: 1000 }).changes[0]).searchParams.get('pageSize'), '12');
    assert.equal(new URL(load({ height: 2400 }).changes[0]).searchParams.get('pageSize'), '25');
    assert.equal(load({ height: 1000, total: 4 }).changes.length, 0);
});

test('paging a fitted query is stable until a real resize', () => {
    const { changes, events, window } = load({ height: 500, url: 'http://localhost/Member?pageSize=5&fit=2&page=2' });
    assert.equal(changes.length, 0);
    window.innerWidth = 1200;
    events.resize();
    assert.equal(changes.length, 1);
    const url = new URL(changes[0]);
    assert.equal(url.searchParams.get('pageSize'), '2');
    assert.equal(url.searchParams.get('page'), '3');
});

test('dossier has a five-row minimum but grows on taller screens', () => {
    assert.equal(load({ height: 600, top: 650, mode: 'roster' }).changes.length, 0);
    assert.equal(new URL(load({ height: 1400, top: 650, mode: 'roster' }).changes[0]).searchParams.get('pageSize'), '12');
});

test('old fitted URLs are recalculated once for the current viewport', () => {
    const { changes } = load({ height: 1000, url: 'http://localhost/Member?pageSize=5&fit=1' });
    assert.equal(new URL(changes[0]).searchParams.get('pageSize'), '12');
    assert.equal(new URL(changes[0]).searchParams.get('fit'), '2');
});

test('scrollbar-sized viewport changes do not restart a fitted page', () => {
    const { events, changes, window } = load({ height: 500, url: 'http://localhost/Member?fit=2' });
    window.innerWidth -= 15;
    events.resize();
    assert.equal(changes.length, 0);
});

test('a pending search is not lost to an automatic viewport fit', () => {
    const search = { dataset: { appliedSearch: 'Ana' }, querySelector: () => ({ value: 'Ben' }) };
    const { changes } = load({ search, height: 1000 });
    assert.equal(changes.length, 0);
});

test('list POSTs preserve their original view and do not replace the action route', () => {
    const { events } = load({ paging: false, url: 'http://localhost/Member?search=Ana&page=2&pageSize=3' });
    let origin;
    const form = { method: 'post', action: 'http://localhost/Member/Deactivate/7', closest: () => ({}), querySelector: () => origin, append: input => { origin = input; } };
    events.submit({ target: form });
    assert.equal(new URL(form.action).pathname, '/Member/Deactivate/7');
    assert.equal(new URL(form.action).searchParams.get('search'), 'Ana');
    assert.equal(origin.value, '/Member?search=Ana&page=2&pageSize=3');
});

test('event cards fit complete grid rows within the supported capacity', () => {
    const { changes } = load({ height: 600, grid: true });
    assert.equal(new URL(changes[0]).searchParams.get('pageSize'), '2');
});
