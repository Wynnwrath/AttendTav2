const test = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
const path = require('node:path');

function setup() {
    const handlers = {};
    const context = { window: {}, document: {
        addEventListener: (name, callback) => { handlers[name] = callback; }
    }};
    vm.runInNewContext(fs.readFileSync(path.join(__dirname, '../wwwroot/js/dialog-guard.js'), 'utf8'), context);
    const fields = [{ name: 'Title', type: 'text', value: 'Draft', disabled: false }];
    const dialog = {
        querySelectorAll: () => fields,
        querySelector: () => null,
        getAttribute: () => null,
        close() { this.closed = true; }
    };
    return { api: context.window.AttendTaDialogs, fields, dialog };
}

test('dialog guard recognizes edits and undoing edits', () => {
    const { api, dialog, fields } = setup();
    api.track(dialog);
    assert.equal(api.hasChanges(dialog), false);
    fields[0].value = 'Changed';
    assert.equal(api.hasChanges(dialog), true);
    fields[0].value = 'Draft';
    assert.equal(api.hasChanges(dialog), false);
});

test('server-loaded editor establishes a new baseline only when explicitly reset', () => {
    const { api, dialog, fields } = setup();
    api.track(dialog);
    fields[0].value = 'Loaded event';
    api.track(dialog);
    assert.equal(api.hasChanges(dialog), true);
    api.track(dialog, true);
    assert.equal(api.hasChanges(dialog), false);
});

test('unchanged dialog dismisses immediately but a busy save cannot close', () => {
    const { api, dialog } = setup();
    api.track(dialog);
    assert.equal(api.requestClose(dialog), true);
    dialog.closed = false;
    dialog.getAttribute = () => 'true';
    assert.equal(api.requestClose(dialog), false);
    assert.equal(dialog.closed, false);
});

test('checkboxes and selected files participate in change detection', () => {
    const { api, dialog, fields } = setup();
    fields.push({ name: 'Consent', type: 'checkbox', checked: false });
    fields.push({ name: 'Image', type: 'file', files: [] });
    api.track(dialog);
    fields[1].checked = true;
    assert.equal(api.hasChanges(dialog), true);
    fields[1].checked = false;
    fields[2].files = [{ name: 'cover.png', size: 42, lastModified: 1 }];
    assert.equal(api.hasChanges(dialog), true);
});
