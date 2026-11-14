// One dismissal policy for record editors and decision dialogs. The prompt stays
// inside the current dialog so the user never has to navigate stacked modals.
(() => {
    'use strict';
    const baselines = new WeakMap();
    function snapshot(dialog) {
        return JSON.stringify(Array.from(dialog.querySelectorAll('input, select, textarea'))
            .filter(field => !field.disabled && field.type !== 'hidden')
            .map(field => [field.name, field.type === 'checkbox' || field.type === 'radio'
                ? field.checked : field.type === 'file'
                    ? Array.from(field.files || []).map(file => [file.name, file.size, file.lastModified])
                    : field.value]));
    }
    function track(dialog, reset = false) {
        if (!baselines.has(dialog) || reset) baselines.set(dialog, snapshot(dialog));
    }
    function hasChanges(dialog) {
        return baselines.has(dialog) && baselines.get(dialog) !== snapshot(dialog);
    }
    function requestClose(dialog) {
        if (!dialog || dialog.getAttribute('aria-busy') === 'true' || dialog.querySelector('form[aria-busy="true"]')) return false;
        if (!hasChanges(dialog)) { dialog.close(); return true; }
        const existing = dialog.querySelector('[data-discard-prompt]');
        if (existing) { existing.querySelector('button').focus(); return false; }
        const returnFocus = document.activeElement;
        const prompt = document.createElement('section');
        prompt.className = 'dialog-discard-notice';
        prompt.dataset.discardPrompt = '';
        prompt.setAttribute('role', 'group');
        prompt.setAttribute('aria-label', 'Unsaved changes');
        const text = document.createElement('p');
        text.textContent = 'You have unsaved changes. Keep editing or discard them?';
        const actions = document.createElement('div');
        actions.className = 'dialog-discard-actions';
        const keep = document.createElement('button');
        keep.type = 'button'; keep.className = 'button button-secondary'; keep.textContent = 'Keep editing';
        keep.addEventListener('click', () => { prompt.remove(); returnFocus?.focus(); });
        const discard = document.createElement('button');
        discard.type = 'button'; discard.className = 'button button-danger'; discard.textContent = 'Discard changes';
        discard.addEventListener('click', () => { prompt.remove(); dialog.close(); });
        actions.append(keep, discard); prompt.append(text, actions); dialog.append(prompt); keep.focus();
        return false;
    }
    document.addEventListener('focusin', event => {
        const dialog = event.target.closest('dialog');
        if (dialog) track(dialog);
    });
    document.addEventListener('cancel', event => {
        const dialog = event.target;
        if (dialog instanceof HTMLDialogElement && (hasChanges(dialog) || dialog.getAttribute('aria-busy') === 'true' || dialog.querySelector('form[aria-busy="true"]'))) {
            event.preventDefault(); requestClose(dialog);
        }
    }, true);
    document.addEventListener('close', event => {
        if (!(event.target instanceof HTMLDialogElement)) return;
        baselines.delete(event.target);
        event.target.querySelector('[data-discard-prompt]')?.remove();
    }, true);
    window.AttendTaDialogs = { track, hasChanges, requestClose };
})();
