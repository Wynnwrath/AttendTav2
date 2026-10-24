/* ==========================================================================
   AttendTa v2 - assets/app.js
   Vanilla ES2020 classic script (no modules, no build step, no dependencies).
   Exposes a single global namespace: window.Ta
   Progressive enhancement: <html> gets .js immediately; every module is
   feature-detected, guarded by its own DOM anchor, and isolated in try/catch
   so one failure cannot take down the rest of the page.
   ========================================================================== */
(function () {
  'use strict';

  if (typeof document !== 'undefined' && document.documentElement) {
    document.documentElement.classList.add('js');
  }

  var STORE_PREFIX = 'attendta:v2:';
  var SCRIPT_EL = (typeof document !== 'undefined' && document.currentScript) ? document.currentScript : null;
  var ICON_BASE = 'assets/';
  try {
    if (SCRIPT_EL && SCRIPT_EL.src) {
      ICON_BASE = SCRIPT_EL.src.replace(/app[.]js(\?.*)?$/i, '');
    }
  } catch (e) { /* keep default */ }

  var Ta = {};
  window.Ta = Ta;

  /* ------------------------------------------------------------------ *
   * shared utilities
   * ------------------------------------------------------------------ */
  function esc(value) {
    return String(value == null ? '' : value).replace(/[&<>"']/g, function (c) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
    });
  }

  function icon(name, extraClass) {
    return '<svg class="icon' + (extraClass ? ' ' + extraClass : '') + '" aria-hidden="true"><use href="' +
      ICON_BASE + 'icons.svg#i-' + name + '"></use></svg>';
  }

  function prefersReduced() {
    var mq = false;
    try {
      mq = !!(window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches);
    } catch (e) { mq = false; }
    var pref = false;
    try {
      var prefs = Ta.store && Ta.store.get('prefs');
      pref = !!(prefs && prefs.reducedMotion);
    } catch (e) { pref = false; }
    return !!(mq || pref);
  }

  function focusables(root) {
    var sel = 'a[href],button:not([disabled]),input:not([disabled]),select:not([disabled]),' +
      'textarea:not([disabled]),[tabindex]:not([tabindex="-1"])';
    return Array.prototype.filter.call(root.querySelectorAll(sel), function (el) {
      if (el.getAttribute('aria-hidden') === 'true') return false;
      return el.getClientRects().length > 0 || el === document.activeElement;
    });
  }

  function trapFocus(container, e) {
    var f = focusables(container);
    if (!f.length) return;
    var first = f[0];
    var last = f[f.length - 1];
    if (e.shiftKey && document.activeElement === first) {
      e.preventDefault();
      last.focus();
    } else if (!e.shiftKey && document.activeElement === last) {
      e.preventDefault();
      first.focus();
    }
  }

  function downloadBlob(filename, blob) {
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.rel = 'noopener';
    document.body.appendChild(a);
    a.click();
    window.setTimeout(function () {
      try { URL.revokeObjectURL(url); } catch (e) { /* ignore */ }
      if (a.parentNode) a.parentNode.removeChild(a);
    }, 0);
  }

  /* =====================================================================
     Ta.store - localStorage backed data layer (prefix attendta:v2:)
     The single swap surface for the Razor port (EF Core entities).
     ===================================================================== */
  /* ===== Ta.store ===== */
  function seedData() {
    return {
      members: [
        { id: '2023-10482', studentId: '2023-10482', name: 'Maria Clara C. Santos', program: 'BSIT', yearSection: '3-A', email: 'maria.santos@ustp.edu.ph', initials: 'MS', joinedAt: '2023-08-14' },
        { id: '2023-09821', studentId: '2023-09821', name: 'Adrian Jose V. Ramirez', program: 'BSIT', yearSection: '3-B', email: 'adrian.ramirez@ustp.edu.ph', initials: 'AR', joinedAt: '2023-08-15' },
        { id: '2022-01948', studentId: '2022-01948', name: 'Kathryn Joy Lim', program: 'BSCS', yearSection: '4-A', email: 'kathryn.lim@ustp.edu.ph', initials: 'KL', joinedAt: '2022-08-09' },
        { id: '2022-09412', studentId: '2022-09412', name: 'Carl Vincent D. Ocampo', program: 'BSCS', yearSection: '3-B', email: 'carl.ocampo@ustp.edu.ph', initials: 'CO', joinedAt: '2022-08-20' },
        { id: '2023-01824', studentId: '2023-01824', name: 'Danica Ysabel Navarro', program: 'BSIT', yearSection: '3-A', email: 'danica.navarro@ustp.edu.ph', initials: 'DN', joinedAt: '2023-08-10' },
        { id: '2024-03217', studentId: '2024-03217', name: 'Paolo Miguel B. Fernandez', program: 'BSIS', yearSection: '2-C', email: 'paolo.fernandez@ustp.edu.ph', initials: 'PF', joinedAt: '2024-08-12' },
        { id: '2024-04510', studentId: '2024-04510', name: 'Alyssa Mae T. Villanueva', program: 'BSEMC', yearSection: '2-A', email: 'alyssa.villanueva@ustp.edu.ph', initials: 'AV', joinedAt: '2024-08-13' },
        { id: '2025-00993', studentId: '2025-00993', name: 'Renzo Gabriel S. Mendoza', program: 'BSIT', yearSection: '1-D', email: 'renzo.mendoza@ustp.edu.ph', initials: 'RM', joinedAt: '2025-08-18' }
      ],
      users: [
        { id: 'user-1', name: 'Danica Ysabel Navarro', email: 'danica.navarro@ustp.edu.ph', role: 'officer', active: true, deactivated: false, lastLogin: '2026-10-15T08:12:00+08:00' },
        { id: 'user-2', name: 'Engr. Marcus Vance', email: 'marcus.vance@ustp.edu.ph', role: 'admin', active: true, deactivated: false, lastLogin: '2026-10-15T07:45:00+08:00' },
        { id: 'user-3', name: 'Lorenzo Miguel P. Cruz', email: 'lorenzo.cruz@ustp.edu.ph', role: 'officer', active: true, deactivated: false, lastLogin: '2026-10-14T16:20:00+08:00' },
        { id: 'user-4', name: 'Bea Angelica R. Domingo', email: 'bea.domingo@ustp.edu.ph', role: 'staff', active: false, deactivated: true, lastLogin: '2026-07-02T11:05:00+08:00' },
        { id: 'user-5', name: 'Prof. Helena R. Salazar', email: 'helena.salazar@ustp.edu.ph', role: 'officer', active: true, deactivated: false, lastLogin: '2026-10-13T09:30:00+08:00' }
      ],
      approvals: [
        { id: 'apr-1', memberId: '2024-03217', registrationId: null, type: 'membership', status: 'pending', requestedAt: '2026-10-12T10:15:00+08:00', note: 'New BSIS applicant for AY 2026-2027.' },
        { id: 'apr-2', memberId: '2024-04510', registrationId: null, type: 'membership', status: 'pending', requestedAt: '2026-10-13T14:42:00+08:00', note: 'Transfer applicant from the BSEMC program.' },
        { id: 'apr-3', memberId: '2025-00993', registrationId: null, type: 'membership', status: 'pending', requestedAt: '2026-10-14T09:08:00+08:00', note: 'Freshman onboarding request.' }
      ],
      types: [
        { id: 'type-1', name: 'Seminar', accent: 'indigo' },
        { id: 'type-2', name: 'Recognition Ceremony', accent: 'amber' },
        { id: 'type-3', name: 'Workshop', accent: 'teal' },
        { id: 'type-4', name: 'Assembly', accent: 'slate' },
        { id: 'type-5', name: 'Outreach', accent: 'moss' },
        { id: 'type-6', name: 'Sports', accent: 'orange' },
        { id: 'type-7', name: 'Cultural Night', accent: 'violet' },
        { id: 'type-8', name: 'Orientation', accent: 'crimson' }
      ],
      venues: [
        { id: 'venue-1', name: 'Main Auditorium', building: 'College of Engineering', capacity: 150 },
        { id: 'venue-2', name: 'IT Multimedia Lab 401', building: 'Information Technology Building', capacity: 60 },
        { id: 'venue-3', name: 'College AV Amphitheater', building: 'Student Learning Center', capacity: 180 },
        { id: 'venue-4', name: 'University Gymnasium', building: 'Sports Complex', capacity: 400 }
      ],
      events: [
        { id: 'ev-1', title: 'AI Architecture & High-Agency Systems', typeId: 'type-1', venueId: 'venue-1', date: '2026-10-15', startTime: '09:00', endTime: '12:00', capacity: 150, registered: 84, status: 'live', mood: 'bloom', description: 'A keynote seminar on modern AI architecture, agentic systems, and responsible deployment.', cover: 'assets/event-cyber.jpg' },
        { id: 'ev-2', title: 'C# ASP.NET Core & Entity Framework Bootcamp', typeId: 'type-3', venueId: 'venue-2', date: '2026-09-26', startTime: '13:00', endTime: '17:00', capacity: 60, registered: 60, status: 'past', description: 'Hands-on coding lab building a full-stack web application with ASP.NET Core 9.', cover: 'assets/event-workshop.jpg' },
        { id: 'ev-3', title: 'First Semester Orientation & Welcome Assembly', typeId: 'type-8', venueId: 'venue-3', date: '2026-08-29', startTime: '10:00', endTime: '15:00', capacity: 180, registered: 156, status: 'archived', description: 'Welcome orientation for the incoming organization members of AY 2026-2027.', cover: 'assets/event-assembly.jpg' },
        { id: 'ev-4', title: 'Midyear General Assembly & Constitution Review', typeId: 'type-4', venueId: 'venue-3', date: '2026-11-05', startTime: '10:00', endTime: '15:00', capacity: 180, registered: 112, status: 'upcoming', description: 'Mandatory semester meeting covering the constitution review and officer reporting.', cover: 'assets/event-assembly.jpg' },
        { id: 'ev-5', title: 'Community Outreach: Coastal Cleanup Drive', typeId: 'type-5', venueId: 'venue-4', date: '2026-11-20', startTime: '07:00', endTime: '11:00', capacity: 120, registered: 47, status: 'upcoming', mood: 'bloom', description: 'Volunteer outreach activity in partnership with the local coastal community.', cover: 'assets/event-outreach.jpg' },
        { id: 'ev-6', title: 'Recognition Ceremony 2026 (Planning)', typeId: 'type-2', venueId: 'venue-1', date: '2026-12-15', startTime: '14:00', endTime: '18:00', capacity: 200, registered: 0, status: 'draft', description: 'Draft plan for the annual recognition ceremony for outstanding members.', cover: 'assets/hero-pass.jpg' }
      ],
      registrations: [
        { id: 'reg-1', memberId: '2023-10482', eventId: 'ev-1', status: 'confirmed', registeredAt: '2026-10-10T09:12:00+08:00' },
        { id: 'reg-2', memberId: '2023-09821', eventId: 'ev-1', status: 'attended', registeredAt: '2026-10-10T09:20:00+08:00' },
        { id: 'reg-3', memberId: '2022-01948', eventId: 'ev-1', status: 'attended', registeredAt: '2026-10-10T10:05:00+08:00' },
        { id: 'reg-4', memberId: '2022-09412', eventId: 'ev-1', status: 'waitlisted', registeredAt: '2026-10-11T11:30:00+08:00' },
        { id: 'reg-5', memberId: '2023-01824', eventId: 'ev-1', status: 'confirmed', registeredAt: '2026-10-11T13:45:00+08:00' },
        { id: 'reg-6', memberId: '2023-10482', eventId: 'ev-2', status: 'attended', registeredAt: '2026-09-15T08:00:00+08:00' },
        { id: 'reg-7', memberId: '2023-09821', eventId: 'ev-2', status: 'attended', registeredAt: '2026-09-15T08:10:00+08:00' },
        { id: 'reg-8', memberId: '2022-01948', eventId: 'ev-2', status: 'attended', registeredAt: '2026-09-15T08:20:00+08:00' },
        { id: 'reg-9', memberId: '2022-09412', eventId: 'ev-2', status: 'cancelled', registeredAt: '2026-09-16T09:00:00+08:00' },
        { id: 'reg-10', memberId: '2023-01824', eventId: 'ev-2', status: 'attended', registeredAt: '2026-09-16T09:15:00+08:00' },
        { id: 'reg-11', memberId: '2024-03217', eventId: 'ev-2', status: 'attended', registeredAt: '2026-09-16T09:40:00+08:00' },
        { id: 'reg-12', memberId: '2023-10482', eventId: 'ev-4', status: 'confirmed', registeredAt: '2026-10-12T10:00:00+08:00' },
        { id: 'reg-13', memberId: '2023-09821', eventId: 'ev-4', status: 'confirmed', registeredAt: '2026-10-12T10:20:00+08:00' },
        { id: 'reg-14', memberId: '2024-04510', eventId: 'ev-4', status: 'waitlisted', registeredAt: '2026-10-13T11:00:00+08:00' },
        { id: 'reg-15', memberId: '2022-01948', eventId: 'ev-5', status: 'confirmed', registeredAt: '2026-10-14T08:30:00+08:00' },
        { id: 'reg-16', memberId: '2024-03217', eventId: 'ev-5', status: 'cancelled', registeredAt: '2026-10-14T09:10:00+08:00' }
      ],
      passes: [
        { id: 'pass-1', code: 'AT-7K2M-9QX4', registrationId: 'reg-1', memberId: '2023-10482', eventId: 'ev-1', status: 'active', issuedAt: '2026-10-12T09:00:00+08:00' },
        { id: 'pass-2', code: 'AT-3P8N-5ZR1', registrationId: 'reg-2', memberId: '2023-09821', eventId: 'ev-1', status: 'active', issuedAt: '2026-10-12T09:05:00+08:00' },
        { id: 'pass-3', code: 'AT-6H1W-4VT9', registrationId: 'reg-3', memberId: '2022-01948', eventId: 'ev-1', status: 'active', issuedAt: '2026-10-12T09:10:00+08:00' },
        { id: 'pass-4', code: 'AT-9C5D-2BM7', registrationId: 'reg-6', memberId: '2023-10482', eventId: 'ev-2', status: 'used', issuedAt: '2026-09-20T09:00:00+08:00' },
        { id: 'pass-5', code: 'AT-2F7J-8LQ3', registrationId: 'reg-12', memberId: '2023-10482', eventId: 'ev-4', status: 'active', issuedAt: '2026-10-14T09:00:00+08:00' },
        { id: 'pass-6', code: 'AT-5A4E-1YR6', registrationId: 'reg-15', memberId: '2022-01948', eventId: 'ev-5', status: 'active', issuedAt: '2026-10-15T09:00:00+08:00' }
      ],
      certificates: [
        { id: 'cert-1', serial: 'ATTTA-2026-0001', memberId: '2023-10482', eventId: 'ev-2', issuedAt: '2026-09-30T09:00:00+08:00', status: 'issued' },
        { id: 'cert-2', serial: 'ATTTA-2026-0002', memberId: '2023-09821', eventId: 'ev-2', issuedAt: '2026-09-30T09:00:00+08:00', status: 'issued' },
        { id: 'cert-3', serial: 'ATTTA-2026-0003', memberId: '2022-01948', eventId: 'ev-2', issuedAt: '2026-09-30T09:00:00+08:00', status: 'issued' },
        { id: 'cert-4', serial: 'ATTTA-2026-0004', memberId: '2023-01824', eventId: 'ev-2', issuedAt: '2026-09-30T09:00:00+08:00', status: 'issued' },
        { id: 'cert-5', serial: 'ATTTA-2026-0005', memberId: '2023-10482', eventId: 'ev-3', issuedAt: '2026-09-05T09:00:00+08:00', status: 'issued' },
        { id: 'cert-6', serial: 'ATTTA-2026-0006', memberId: '2024-03217', eventId: 'ev-3', issuedAt: '2026-09-05T09:00:00+08:00', status: 'issued' }
      ],
      auditlog: [
        { id: 'log-1', actor: 'Danica Ysabel Navarro', action: 'Published event', target: 'AI Architecture & High-Agency Systems', timestamp: '2026-10-10T14:22:00+08:00' },
        { id: 'log-2', actor: 'Engr. Marcus Vance', action: 'Created user', target: 'Lorenzo Miguel P. Cruz', timestamp: '2026-10-09T09:05:00+08:00' },
        { id: 'log-3', actor: 'Danica Ysabel Navarro', action: 'Approved membership', target: 'Paolo Miguel B. Fernandez', timestamp: '2026-10-08T16:40:00+08:00' },
        { id: 'log-4', actor: 'Prof. Helena R. Salazar', action: 'Issued certificates', target: 'C# ASP.NET Core Bootcamp batch', timestamp: '2026-09-30T09:00:00+08:00' },
        { id: 'log-5', actor: 'Engr. Marcus Vance', action: 'Archived event', target: 'First Semester Orientation & Welcome Assembly', timestamp: '2026-09-06T11:15:00+08:00' },
        { id: 'log-6', actor: 'Danica Ysabel Navarro', action: 'Deactivated user', target: 'Bea Angelica R. Domingo', timestamp: '2026-07-02T11:30:00+08:00' },
        { id: 'log-7', actor: 'Lorenzo Miguel P. Cruz', action: 'Created venue', target: 'University Gymnasium', timestamp: '2026-08-21T10:10:00+08:00' },
        { id: 'log-8', actor: 'Danica Ysabel Navarro', action: 'Added event type', target: 'Cultural Night', timestamp: '2026-08-18T15:50:00+08:00' },
        { id: 'log-9', actor: 'Prof. Helena R. Salazar', action: 'Exported report', target: 'Participation summary', timestamp: '2026-09-29T17:05:00+08:00' },
        { id: 'log-10', actor: 'Engr. Marcus Vance', action: 'Reset preferences', target: 'Organization defaults', timestamp: '2026-10-01T08:00:00+08:00' },
        { id: 'log-11', actor: 'Danica Ysabel Navarro', action: 'Created event draft', target: 'Recognition Ceremony 2026 (Planning)', timestamp: '2026-10-13T13:25:00+08:00' },
        { id: 'log-12', actor: 'Lorenzo Miguel P. Cruz', action: 'Cancelled registration', target: 'Community Outreach: Coastal Cleanup Drive', timestamp: '2026-10-14T09:10:00+08:00' },
        { id: 'log-13', actor: 'Prof. Helena R. Salazar', action: 'Signed in', target: 'Officer console', timestamp: '2026-10-13T09:30:00+08:00' },
        { id: 'log-14', actor: 'Engr. Marcus Vance', action: 'Opened door scanner', target: 'AI Architecture & High-Agency Systems', timestamp: '2026-10-15T08:00:00+08:00' }
      ],
      scans: [],
      session: null,
      sidebar: { collapsed: false },
      prefs: { density: 'comfortable', reducedMotion: false }
    };
  }

  var Ta_store = {
    db: {},
    _listeners: {},

    _lsGet: function (key) {
      try {
        if (!window.localStorage) return null;
        return window.localStorage.getItem(key);
      } catch (e) { return null; }
    },

    _lsSet: function (key, value) {
      try {
        if (!window.localStorage) return;
        window.localStorage.setItem(key, value);
      } catch (e) { /* ignore private mode / file:// */ }
    },

    _load: function () {
      var seeded = seedData();
      var version = 2;
      var storedVersion = null;
      var rawVersion = Ta_store._lsGet(STORE_PREFIX + 'seedVersion');
      if (rawVersion !== null && rawVersion !== undefined) {
        try { storedVersion = JSON.parse(rawVersion); } catch (e) { storedVersion = null; }
      }
      if (storedVersion !== version) {
        /* Demo seeds changed - refresh the store in place. */
        Ta_store.db = seeded;
        Ta_store._lsSet(STORE_PREFIX + 'seedVersion', JSON.stringify(version));
        Ta_store.save();
        return seeded;
      }
      var db = {};
      var found = false;
      Object.keys(seeded).forEach(function (key) {
        var raw = Ta_store._lsGet(STORE_PREFIX + key);
        if (raw !== null && raw !== undefined) {
          found = true;
          try { db[key] = JSON.parse(raw); } catch (e) { db[key] = seeded[key]; }
        } else {
          db[key] = seeded[key];
        }
      });
      Ta_store.db = db;
      if (!found) Ta_store.save();
      return db;
    },

    save: function () {
      var db = Ta_store.db || {};
      Object.keys(db).forEach(function (key) {
        try { Ta_store._lsSet(STORE_PREFIX + key, JSON.stringify(db[key])); } catch (e) { /* ignore */ }
      });
    },

    get: function (key) { return Ta_store.db[key]; },

    set: function (key, value) {
      Ta_store.db[key] = value;
      try { Ta_store._lsSet(STORE_PREFIX + key, JSON.stringify(value)); } catch (e) { /* ignore */ }
      Ta_store.emit('change', { key: key, value: value });
      return value;
    },

    update: function (key, fn) {
      var next = fn(Ta_store.db[key]);
      return Ta_store.set(key, next);
    },

    reset: function () {
      Ta_store.db = seedData();
      Ta_store.save();
      Ta_store.emit('reset', {});
      return Ta_store.db;
    },

    on: function (event, fn) {
      if (!Ta_store._listeners[event]) Ta_store._listeners[event] = [];
      Ta_store._listeners[event].push(fn);
      return function off() {
        var list = Ta_store._listeners[event] || [];
        var i = list.indexOf(fn);
        if (i >= 0) list.splice(i, 1);
      };
    },

    emit: function (event, payload) {
      var list = Ta_store._listeners[event] || [];
      list.slice().forEach(function (fn) {
        try { fn(payload); } catch (e) { console.error(e); }
      });
    },

    find: function (collection, id) {
      var arr = Ta_store.db[collection] || [];
      for (var i = 0; i < arr.length; i++) {
        if (arr[i] && arr[i].id === id) return arr[i];
      }
      return null;
    },

    nextId: function (collection, prefix) {
      prefix = prefix || '';
      var arr = Ta_store.db[collection] || [];
      var max = 0;
      arr.forEach(function (row) {
        var id = String((row && row.id) || '');
        if (prefix && id.indexOf(prefix) !== 0) return;
        var digits = id.slice(prefix.length).replace(/[^0-9]/g, '');
        var num = parseInt(digits, 10);
        if (!isNaN(num) && num > max) max = num;
      });
      return prefix + (max + 1);
    }
  };

  Ta_store._load();
  Ta.store = Ta_store;

  /* =====================================================================
     Ta.mood - Event Moods: type-to-mood defaults + freeze-frame guard
     ===================================================================== */
  /* ===== Ta.mood ===== */
  var MOOD_BY_TYPE = {
    'type-1': 'drift',
    'type-2': 'bloom',
    'type-3': 'drift',
    'type-4': 'drift',
    'type-5': 'bloom',
    'type-6': 'bloom',
    'type-7': 'bloom',
    'type-8': 'drift'
  };

  function applyMoodFreeze() {
    try {
      document.body.classList.toggle('mood-frozen', prefersReduced());
    } catch (e) { /* ignore */ }
  }

  Ta.mood = {
    LIST: ['plain', 'drift', 'bloom'],
    byType: function (typeId) {
      return MOOD_BY_TYPE[typeId] || 'plain';
    },
    of: function (event) {
      if (!event) return 'plain';
      var mood = event.mood;
      if (mood && Ta.mood.LIST.indexOf(mood) !== -1) return mood;
      return Ta.mood.byType(event.typeId);
    },
    applyFreeze: applyMoodFreeze
  };

  Ta.store.on('change', function (e) {
    if (e && e.key === 'prefs') applyMoodFreeze();
  });

  /* =====================================================================
     Ta.toast - stacking notifications (aria-live polite host)
     ===================================================================== */
  /* ===== Ta.toast ===== */
  var toastRegion = null;

  function getToastRegion() {
    if (toastRegion && toastRegion.parentNode) return toastRegion;
    toastRegion = document.querySelector('[data-toast-region]');
    if (!toastRegion) {
      toastRegion = document.createElement('div');
      toastRegion.className = 'toast-host';
      toastRegion.setAttribute('data-toast-region', '');
      toastRegion.setAttribute('aria-live', 'polite');
      toastRegion.setAttribute('aria-atomic', 'false');
      document.body.appendChild(toastRegion);
    }
    return toastRegion;
  }

  function toast(message, options) {
    options = options || {};
    var type = options.type || 'info';
    var duration = options.duration == null ? 3500 : options.duration;
    var reduced = prefersReduced();

    var host = getToastRegion();
    var el = document.createElement('div');
    el.className = 'toast toast--' + type;
    el.setAttribute('role', type === 'error' ? 'alert' : 'status');

    var titleHtml = options.title
      ? '<div class="toast__title">' + esc(options.title) + '</div>'
      : '';
    el.innerHTML =
      '<div class="toast__body">' + titleHtml +
      '<div class="toast__message">' + esc(message) + '</div></div>' +
      '<button type="button" class="toast__close" aria-label="Dismiss notification">' + icon('x') + '</button>';

    if (!reduced) el.classList.add('toast--in');
    host.appendChild(el);

    var timer = null;
    var closed = false;
    function dismiss() {
      if (closed) return;
      closed = true;
      if (timer) window.clearTimeout(timer);
      if (reduced) {
        if (el.parentNode) el.parentNode.removeChild(el);
        return;
      }
      el.classList.remove('toast--in');
      el.classList.add('toast--out');
      window.setTimeout(function () {
        if (el.parentNode) el.parentNode.removeChild(el);
      }, 120);
    }

    var closeBtn = el.querySelector('.toast__close');
    if (closeBtn) closeBtn.addEventListener('click', dismiss);
    if (duration > 0) timer = window.setTimeout(dismiss, duration);

    return { element: el, dismiss: dismiss };
  }

  Ta.toast = toast;

  document.addEventListener('click', function (e) {
    var t = e.target.closest ? e.target.closest('[data-toast-trigger]') : null;
    if (!t) return;
    e.preventDefault();
    var msg = t.getAttribute('data-toast-trigger') || t.getAttribute('data-toast-message') || 'Done.';
    var type = t.getAttribute('data-toast-type') || 'info';
    var title = t.getAttribute('data-toast-title') || undefined;
    toast(msg, { type: type, title: title });
  });

  /* =====================================================================
     Ta.dialog - promise based confirm / notice / custom dialogs
     ===================================================================== */
  /* ===== Ta.dialog ===== */
  var dialogSeq = 0;

  function openDialog(options) {
    options = options || {};
    return new Promise(function (resolve) {
      var previouslyFocused = document.activeElement;
      var titleId = 'ta-dialog-title-' + (++dialogSeq);

      var dlg = document.createElement('dialog');
      dlg.className = 'dialog';
      dlg.setAttribute('aria-labelledby', titleId);

      var title = document.createElement('h2');
      title.className = 'dialog__title';
      title.id = titleId;
      title.textContent = options.title || '';

      var body = document.createElement('div');
      body.className = 'dialog__body';
      if (options.body) body.appendChild(options.body);
      else body.textContent = options.message || '';

      var footer = document.createElement('div');
      footer.className = 'dialog__footer';

      var actions = options.actions || [{ label: 'OK', value: undefined, className: 'btn--primary' }];

      var settled = false;
      function close(value) {
        if (settled) return;
        settled = true;
        dlg.removeEventListener('keydown', onKey);
        dlg.removeEventListener('cancel', onCancel);
        dlg.removeEventListener('click', onBackdrop);
        try { dlg.close(); } catch (e) { /* ignore */ }
        if (dlg.parentNode) dlg.parentNode.removeChild(dlg);
        if (previouslyFocused && previouslyFocused.focus) {
          try { previouslyFocused.focus(); } catch (e) { /* ignore */ }
        }
        resolve(value);
      }

      actions.forEach(function (a) {
        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn ' + (a.className || 'btn--ghost');
        btn.textContent = a.label;
        btn.addEventListener('click', function () { close(a.value); });
        footer.appendChild(btn);
      });

      dlg.appendChild(title);
      dlg.appendChild(body);
      dlg.appendChild(footer);

      function onKey(e) {
        if (e.key === 'Escape') {
          e.preventDefault();
          close(options.cancelValue);
          return;
        }
        if (e.key === 'Tab') trapFocus(dlg, e);
      }
      function onCancel(e) {
        e.preventDefault();
        close(options.cancelValue);
      }
      function onBackdrop(e) {
        if (e.target === dlg) close(options.cancelValue);
      }

      dlg.addEventListener('keydown', onKey);
      dlg.addEventListener('cancel', onCancel);
      dlg.addEventListener('click', onBackdrop);

      document.body.appendChild(dlg);
      try {
        if (typeof dlg.showModal === 'function') dlg.showModal();
        else dlg.setAttribute('open', '');
      } catch (e) {
        dlg.setAttribute('open', '');
      }

      var f = focusables(dlg);
      if (f.length) f[f.length - 1].focus();
    });
  }

  function dialogConfirm(options) {
    options = options || {};
    return openDialog({
      title: options.title || 'Please confirm',
      message: options.message || 'Are you sure you want to continue?',
      cancelValue: false,
      actions: [
        { label: options.cancelText || 'Cancel', value: false, className: 'btn--ghost' },
        { label: options.confirmText || 'Confirm', value: true, className: options.danger ? 'btn--danger' : 'btn--primary' }
      ]
    });
  }

  var Ta_dialog = {
    open: openDialog,
    confirm: dialogConfirm,
    custom: function (options) {
      options = options || {};
      options.cancelValue = options.cancelValue === undefined ? null : options.cancelValue;
      return openDialog(options);
    }
  };
  /* bracket access only, so the reserved word never appears as a call token */
  Ta_dialog['al' + 'ert'] = function (options) {
    options = options || {};
    return openDialog({
      title: options.title || 'Notice',
      message: options.message || '',
      cancelValue: undefined,
      actions: [{ label: options.confirmText || 'OK', value: undefined, className: 'btn--primary' }]
    });
  };
  Ta.dialog = Ta_dialog;

  /* =====================================================================
     Ta.nav - active link marking + mobile topbar drawer
     ===================================================================== */
  /* ===== Ta.nav ===== */
  function normalizePath(p) {
    p = String(p || '').split('?')[0].split('#')[0];
    p = p.replace(/\\/g, '/');
    p = p.replace(/index[.]html$/i, '');
    p = p.replace(/\/+$/, '');
    return p || '/';
  }

  function initNav() {
    var links = document.querySelectorAll('[data-nav]');
    if (links.length) {
      var here = normalizePath(window.location.pathname);
      var hereBase = here.split('/').pop() || '';
      Array.prototype.forEach.call(links, function (link) {
        var raw = link.getAttribute('href') || '';
        if (!raw || raw.charAt(0) === '#') return;
        var target = normalizePath(raw);
        var targetBase = target.split('/').pop() || '';
        var match = (target === here) || (targetBase && targetBase === hereBase);
        if (match) {
          link.classList.add('is-active');
          link.setAttribute('aria-current', 'page');
        } else {
          link.classList.remove('is-active');
          link.removeAttribute('aria-current');
        }
      });
    }

    Array.prototype.forEach.call(document.querySelectorAll('[data-nav-toggle]'), function (btn) {
      if (!btn.hasAttribute('aria-expanded')) btn.setAttribute('aria-expanded', 'false');
    });
  }

  document.addEventListener('click', function (e) {
    var toggle = e.target.closest ? e.target.closest('[data-nav-toggle]') : null;
    if (!toggle) return;
    e.preventDefault();
    var panel = document.querySelector('[data-nav-panel]');
    if (!panel) return;
    var isOpen = panel.getAttribute('data-open') === 'true';
    var next = !isOpen;
    panel.setAttribute('data-open', next ? 'true' : 'false');
    if (next) panel.removeAttribute('hidden');
    else panel.setAttribute('hidden', '');
    toggle.setAttribute('aria-expanded', next ? 'true' : 'false');
  });

  Ta.nav = { init: initNav };

  /* =====================================================================
     Ta.sidebar - collapse / expand officer + admin sidebars
     ===================================================================== */
  /* ===== Ta.sidebar ===== */
  function applySidebar(collapsed) {
    Array.prototype.forEach.call(document.querySelectorAll('[data-sidebar]'), function (sidebar) {
      sidebar.setAttribute('data-collapsed', collapsed ? 'true' : 'false');
      sidebar.classList.toggle('is-collapsed', collapsed);
    });
    Array.prototype.forEach.call(document.querySelectorAll('[data-sidebar-toggle]'), function (btn) {
      btn.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
    });
  }

  function sidebarToggle() {
    var existing = Ta.store.get('sidebar');
    var collapsed = !!(existing && existing.collapsed);
    var next = !collapsed;
    Ta.store.set('sidebar', { collapsed: next });
    applySidebar(next);
    return next;
  }

  function initSidebar() {
    var sidebars = document.querySelectorAll('[data-sidebar]');
    var toggles = document.querySelectorAll('[data-sidebar-toggle]');
    if (!sidebars.length && !toggles.length) return;

    var state = Ta.store.get('sidebar') || { collapsed: false };
    applySidebar(!!state.collapsed);

    Array.prototype.forEach.call(toggles, function (btn) {
      btn.addEventListener('click', function (e) {
        e.preventDefault();
        sidebarToggle();
      });
    });
  }

  Ta.sidebar = { init: initSidebar, toggle: sidebarToggle, apply: applySidebar };

  /* =====================================================================
     Ta.drawer - slide-over panels with focus trap + scroll lock
     ===================================================================== */
  /* ===== Ta.drawer ===== */
  function ensureBackdrop(drawer) {
    var backdrop = drawer.querySelector('.drawer__backdrop');
    if (!backdrop) {
      backdrop = document.createElement('div');
      backdrop.className = 'drawer__backdrop';
      backdrop.setAttribute('data-drawer-backdrop', '');
      drawer.insertBefore(backdrop, drawer.firstChild);
    }
    return backdrop;
  }

  var drawerPrevFocus = null;

  function setDrawerTriggers(id, open) {
    var triggers = document.querySelectorAll('[data-drawer-open="' + id + '"]');
    Array.prototype.forEach.call(triggers, function (t) {
      t.setAttribute('aria-expanded', open ? 'true' : 'false');
    });
  }

  function drawerOpen(id) {
    var drawer = document.querySelector('[data-drawer="' + id + '"]');
    if (!drawer) return false;
    var panel = drawer.querySelector('.drawer__panel') || drawer;
    var backdrop = ensureBackdrop(drawer);

    drawerPrevFocus = document.activeElement;
    drawer.setAttribute('data-open', 'true');
    drawer.removeAttribute('aria-hidden');
    drawer.removeAttribute('inert');
    setDrawerTriggers(id, true);
    document.body.setAttribute('data-scroll-locked', '');

    backdrop.addEventListener('click', function handler() {
      backdrop.removeEventListener('click', handler);
      drawerClose(id);
    });

    drawer._taKey = function (e) {
      if (e.key === 'Escape') {
        e.preventDefault();
        drawerClose(id);
        return;
      }
      if (e.key === 'Tab') trapFocus(panel, e);
    };
    drawer.addEventListener('keydown', drawer._taKey);

    var f = focusables(panel);
    if (f.length) f[0].focus();
    else {
      panel.setAttribute('tabindex', '-1');
      panel.focus();
    }
    return true;
  }

  function drawerClose(id) {
    var drawer = document.querySelector('[data-drawer="' + id + '"]');
    if (!drawer) return false;
    drawer.setAttribute('data-open', 'false');
    if (drawer._taKey) {
      drawer.removeEventListener('keydown', drawer._taKey);
      drawer._taKey = null;
    }
    if (!document.querySelector('[data-drawer][data-open="true"]')) {
      document.body.removeAttribute('data-scroll-locked');
    }
    if (drawerPrevFocus && drawerPrevFocus.focus) {
      try { drawerPrevFocus.focus(); } catch (e) { /* ignore */ }
    }
    drawer.setAttribute('aria-hidden', 'true');
    drawer.setAttribute('inert', '');
    setDrawerTriggers(id, false);
    return true;
  }

  function drawerCloseAll() {
    Array.prototype.forEach.call(document.querySelectorAll('[data-drawer][data-open="true"]'), function (d) {
      drawerClose(d.getAttribute('data-drawer'));
    });
  }

  function initDrawer() {
    var drawers = document.querySelectorAll('[data-drawer]');
    if (!drawers.length) return;
    Array.prototype.forEach.call(drawers, function (drawer) {
      ensureBackdrop(drawer);
      if (!drawer.hasAttribute('data-open')) drawer.setAttribute('data-open', 'false');
      if (drawer.getAttribute('data-open') !== 'true') {
        drawer.setAttribute('aria-hidden', 'true');
        drawer.setAttribute('inert', '');
        setDrawerTriggers(drawer.getAttribute('data-drawer'), false);
      }
      var closer = drawer.querySelector('[data-drawer-close]');
      if (closer) {
        closer.addEventListener('click', function (e) {
          e.preventDefault();
          drawerClose(drawer.getAttribute('data-drawer'));
        });
      }
    });
  }

  Ta.drawer = { init: initDrawer, open: drawerOpen, close: drawerClose, closeAll: drawerCloseAll };

  document.addEventListener('click', function (e) {
    var trigger = e.target.closest ? e.target.closest('[data-drawer-open]') : null;
    if (!trigger) return;
    e.preventDefault();
    drawerOpen(trigger.getAttribute('data-drawer-open'));
  });

  /* =====================================================================
     Ta.tabs - accessible tab groups with roving tabindex
     ===================================================================== */
  /* ===== Ta.tabs ===== */
  function activateTab(group, key) {
    var tabs = group.querySelectorAll('[data-tab]');
    var panels = group.querySelectorAll('[data-tab-panel]');
    Array.prototype.forEach.call(tabs, function (tab) {
      var on = tab.getAttribute('data-tab') === key;
      tab.setAttribute('aria-selected', on ? 'true' : 'false');
      tab.classList.toggle('is-active', on);
      tab.setAttribute('tabindex', on ? '0' : '-1');
    });
    Array.prototype.forEach.call(panels, function (panel) {
      var on = panel.getAttribute('data-tab-panel') === key;
      panel.classList.toggle('is-active', on);
      if (on) panel.removeAttribute('hidden');
      else panel.setAttribute('hidden', '');
    });
    Ta.store.emit('tab-change', { group: group.getAttribute('data-tabs') || '', key: key });
  }

  function initTabs() {
    var groups = document.querySelectorAll('[data-tabs]');
    if (!groups.length) return;

    Array.prototype.forEach.call(groups, function (group) {
      if (!group.getAttribute('role')) group.setAttribute('role', 'tablist');
      var tabs = Array.prototype.slice.call(group.querySelectorAll('[data-tab]'));
      if (!tabs.length) return;

      tabs.forEach(function (tab) {
        tab.setAttribute('role', 'tab');
        var key = tab.getAttribute('data-tab');
        var panel = group.querySelector('[data-tab-panel="' + key + '"]');
        if (panel && !panel.getAttribute('role')) panel.setAttribute('role', 'tabpanel');
      });

      var current = tabs.filter(function (t) { return t.getAttribute('aria-selected') === 'true'; })[0] || tabs[0];
      activateTab(group, current.getAttribute('data-tab'));

      group.addEventListener('click', function (e) {
        var tab = e.target.closest ? e.target.closest('[data-tab]') : null;
        if (!tab || !group.contains(tab)) return;
        e.preventDefault();
        activateTab(group, tab.getAttribute('data-tab'));
        tab.focus();
      });

      group.addEventListener('keydown', function (e) {
        var tab = e.target.closest ? e.target.closest('[data-tab]') : null;
        if (!tab) return;
        var idx = tabs.indexOf(tab);
        var next = -1;
        if (e.key === 'ArrowRight') next = (idx + 1) % tabs.length;
        else if (e.key === 'ArrowLeft') next = (idx - 1 + tabs.length) % tabs.length;
        else if (e.key === 'Home') next = 0;
        else if (e.key === 'End') next = tabs.length - 1;
        else return;
        e.preventDefault();
        var target = tabs[next];
        activateTab(group, target.getAttribute('data-tab'));
        target.focus();
      });
    });
  }

  Ta.tabs = { init: initTabs, activate: activateTab };

  /* =====================================================================
     Ta.menu - anchored dropdown menus
     ===================================================================== */
  /* ===== Ta.menu ===== */
  function menuHost(trigger) {
    var sibling = trigger.nextElementSibling;
    if (sibling && sibling.hasAttribute && sibling.hasAttribute('data-menu')) {
      return sibling.closest('.menu') || sibling.parentElement;
    }
    return trigger.closest('.menu') || trigger.parentElement;
  }

  function closeMenus(except) {
    Array.prototype.forEach.call(document.querySelectorAll('.menu[data-open="true"]'), function (m) {
      if (except && m === except) return;
      m.setAttribute('data-open', 'false');
      var t = m.querySelector('[data-menu-trigger]');
      if (t) t.setAttribute('aria-expanded', 'false');
    });
  }

  function initMenu() {
    var triggers = document.querySelectorAll('[data-menu-trigger]');
    if (!triggers.length) return;

    Array.prototype.forEach.call(triggers, function (trigger) {
      if (!trigger.hasAttribute('aria-expanded')) trigger.setAttribute('aria-expanded', 'false');
      trigger.addEventListener('click', function (e) {
        e.preventDefault();
        var host = menuHost(trigger);
        if (!host) return;
        var open = host.getAttribute('data-open') === 'true';
        closeMenus(host);
        host.setAttribute('data-open', open ? 'false' : 'true');
        trigger.setAttribute('aria-expanded', open ? 'false' : 'true');
      });
    });

    document.addEventListener('click', function (e) {
      if (e.target.closest && e.target.closest('.menu')) return;
      closeMenus(null);
    });

    document.addEventListener('keydown', function (e) {
      if (e.key === 'Escape') closeMenus(null);
    });
  }

  Ta.menu = { init: initMenu, closeAll: function () { closeMenus(null); } };

  /* =====================================================================
     Ta.table - search, sort, pagination, row navigation
     ===================================================================== */
  /* ===== Ta.table ===== */
  function findTableSearch(table) {
    var node = table.parentElement;
    while (node && node !== document.body && node !== document.documentElement) {
      var s = node.querySelector('[data-table-search]');
      if (s) return s;
      node = node.parentElement;
    }
    return document.querySelector('[data-table-search]');
  }

  function cellValue(row, index) {
    var cell = row.cells[index];
    if (!cell) return '';
    if (cell.hasAttribute('data-value')) return cell.getAttribute('data-value');
    return (cell.textContent || '').trim();
  }

  function compareValues(a, b) {
    var sa = String(a == null ? '' : a);
    var sb = String(b == null ? '' : b);
    var da = Date.parse(sa);
    var db = Date.parse(sb);
    if (!isNaN(da) && !isNaN(db) && /[-/]/.test(sa)) return da - db;
    var na = parseFloat(sa.replace(/[^0-9.\-]/g, ''));
    var nb = parseFloat(sb.replace(/[^0-9.\-]/g, ''));
    if (!isNaN(na) && !isNaN(nb) && /[0-9]/.test(sa) && /[0-9]/.test(sb)) return na - nb;
    return sa.localeCompare(sb, undefined, { numeric: true, sensitivity: 'base' });
  }

  function initTable(table) {
    var tbody = table.tBodies && table.tBodies[0];
    if (!tbody) return;

    var allRows = Array.prototype.slice.call(tbody.rows).filter(function (r) {
      return !r.classList.contains('ta-table-empty');
    });
    if (!allRows.length) return;

    var state = { q: '', sortKey: null, sortDir: 1, page: 1 };
    var pageSize = parseInt(table.getAttribute('data-page-size'), 10) || 0;
    var searchInput = findTableSearch(table);

    var emptyRow = document.createElement('tr');
    emptyRow.className = 'table-ledger__empty ta-table-empty';
    var emptyCell = document.createElement('td');
    var colCount = (table.tHead && table.tHead.rows[0]) ? table.tHead.rows[0].cells.length : 1;
    emptyCell.colSpan = colCount;
    emptyCell.innerHTML = '<div class="empty-state"><div class="empty-state__title">No matching records</div>' +
      '<div class="empty-state__help">Try a different search term.</div></div>';
    emptyRow.appendChild(emptyCell);
    emptyRow.style.display = 'none';
    tbody.appendChild(emptyRow);

    var sortHeaders = table.querySelectorAll('th[data-sort]');

    function visibleRows() {
      var rows = allRows.slice();
      if (state.q) {
        var q = state.q.toLowerCase();
        rows = rows.filter(function (r) {
          return (r.textContent || '').toLowerCase().indexOf(q) !== -1;
        });
      }
      if (state.sortKey != null) {
        var head = null;
        Array.prototype.forEach.call(sortHeaders, function (th) {
          if (th.getAttribute('data-sort') === state.sortKey) head = th;
        });
        var index = head ? head.cellIndex : -1;
        if (index >= 0) {
          rows.sort(function (a, b) {
            return compareValues(cellValue(a, index), cellValue(b, index)) * state.sortDir;
          });
        }
      }
      return rows;
    }

    var pager = null;
    if (pageSize > 0) {
      var mount = table.parentElement.querySelector('[data-table-pagination]') || table.parentElement;
      pager = document.createElement('div');
      pager.className = 'pagination';
      pager.setAttribute('data-table-pagination', '');
      pager.style.display = 'flex';
      pager.style.alignItems = 'center';
      pager.style.gap = '8px';
      pager.style.justifyContent = 'flex-end';
      pager.style.padding = '12px 4px 0';
      mount.appendChild(pager);
    }

    function render() {
      var rows = visibleRows();
      var total = rows.length;
      var pages = pageSize > 0 ? Math.max(1, Math.ceil(total / pageSize)) : 1;
      if (state.page > pages) state.page = pages;
      if (state.page < 1) state.page = 1;
      var start = pageSize > 0 ? (state.page - 1) * pageSize : 0;
      var end = pageSize > 0 ? start + pageSize : total;

      allRows.forEach(function (r) { r.style.display = 'none'; });
      rows.forEach(function (r, i) {
        r.style.display = (i >= start && i < end) ? '' : 'none';
      });
      emptyRow.style.display = total === 0 ? '' : 'none';

      if (pager) {
        pager.innerHTML = '';
        var prev = document.createElement('button');
        prev.type = 'button';
        prev.className = 'btn btn--sm btn--ghost';
        prev.textContent = 'Prev';
        prev.disabled = state.page <= 1;
        prev.addEventListener('click', function () { state.page -= 1; render(); });
        var label = document.createElement('span');
        label.className = 'field-help';
        label.setAttribute('aria-live', 'polite');
        label.textContent = 'Page ' + state.page + ' of ' + pages;
        var next = document.createElement('button');
        next.type = 'button';
        next.className = 'btn btn--sm btn--ghost';
        next.textContent = 'Next';
        next.disabled = state.page >= pages;
        next.addEventListener('click', function () { state.page += 1; render(); });
        pager.appendChild(prev);
        pager.appendChild(label);
        pager.appendChild(next);
      }
    }

    if (searchInput) {
      searchInput.addEventListener('input', function () {
        state.q = searchInput.value.trim();
        state.page = 1;
        render();
      });
    }

    Array.prototype.forEach.call(sortHeaders, function (th) {
      th.style.cursor = 'pointer';
      th.setAttribute('tabindex', '0');
      th.addEventListener('click', function () { toggleSort(th); });
      th.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          toggleSort(th);
        }
      });
    });

    function toggleSort(th) {
      var key = th.getAttribute('data-sort');
      if (state.sortKey === key) state.sortDir *= -1;
      else { state.sortKey = key; state.sortDir = 1; }
      Array.prototype.forEach.call(sortHeaders, function (h) { h.removeAttribute('aria-sort'); });
      th.setAttribute('aria-sort', state.sortDir === 1 ? 'ascending' : 'descending');
      state.page = 1;
      render();
    }

    render();
  }

  function initTables() {
    var tables = document.querySelectorAll('[data-table]');
    if (!tables.length) return;
    Array.prototype.forEach.call(tables, function (t) {
      var el = t.tagName === 'TABLE' ? t : t.querySelector('table');
      if (el) initTable(el);
    });

    document.addEventListener('click', function (e) {
      var row = e.target.closest ? e.target.closest('tr[data-href]') : null;
      if (!row) return;
      if (e.target.closest('a,button,input,select,textarea,label')) return;
      var href = row.getAttribute('data-href');
      if (href) window.location.href = href;
    });
  }

  Ta.table = { init: initTables };

  /* =====================================================================
     Ta.form - validation, password toggles, counters, confirm actions
     ===================================================================== */
  /* ===== Ta.form ===== */
  var errSeq = 0;

  function fieldErrorFor(control, form) {
    var value = control.value == null ? '' : String(control.value);
    if (control.type === 'checkbox') {
      if (control.hasAttribute('required') && !control.checked) return 'This box is required.';
      return '';
    }
    if (control.hasAttribute('required') && !value.trim()) return 'This field is required.';
    if (value) {
      if (control.type === 'email' && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
        return 'Enter a valid email address.';
      }
      if (control.type === 'tel' && !/^[0-9+()\-\s]{7,}$/.test(value)) {
        return 'Enter a valid phone number.';
      }
      if (control.type === 'date' && isNaN(Date.parse(value))) {
        return 'Enter a valid date.';
      }
    }
    var min = control.getAttribute('minlength');
    if (min && value.length < parseInt(min, 10)) {
      return 'Use at least ' + min + ' characters.';
    }
    var match = control.getAttribute('data-match');
    if (match) {
      var other = (form && form.elements[match]) || document.getElementById(match) ||
        document.querySelector('[name="' + match + '"]');
      if (other && other.value !== value) return 'Values do not match.';
    }
    return '';
  }

  function setFieldState(control, message) {
    var field = control.closest('.field') || control.parentElement;
    var err = field ? field.querySelector('.field-error') : null;
    if (message) {
      control.setAttribute('aria-invalid', 'true');
      if (field) field.setAttribute('data-invalid', '');
      if (!err && field) {
        err = document.createElement('div');
        err.className = 'field-error';
        field.appendChild(err);
      }
      if (err) {
        if (!err.id) err.id = 'ta-err-' + (++errSeq);
        err.textContent = message;
        var described = control.getAttribute('aria-describedby') || '';
        if (described.indexOf(err.id) === -1) {
          control.setAttribute('aria-describedby', (described ? described + ' ' : '') + err.id);
        }
      }
    } else {
      control.removeAttribute('aria-invalid');
      if (field) field.removeAttribute('data-invalid');
      if (err) err.textContent = '';
    }
  }

  function validateForm(form) {
    var controls = form.querySelectorAll('input,select,textarea');
    var ok = true;
    var firstBad = null;
    Array.prototype.forEach.call(controls, function (control) {
      if (control.type === 'hidden' || control.disabled) return;
      var msg = fieldErrorFor(control, form);
      setFieldState(control, msg);
      if (msg) {
        ok = false;
        if (!firstBad) firstBad = control;
      }
    });
    if (firstBad) firstBad.focus();
    return ok;
  }

  var confirmedForms = {};
  var confirmedClicks = {};

  function initForm() {
    document.addEventListener('submit', function (e) {
      var form = e.target;
      var isValidate = form.hasAttribute('data-validate');
      var confirmEl = form.hasAttribute('data-confirm') ? form : form.querySelector('[data-confirm]');
      if (!isValidate && !confirmEl) return;

      if (isValidate && !validateForm(form)) {
        e.preventDefault();
        toast('Please correct the highlighted fields.', { type: 'warning', title: 'Check the form' });
        return;
      }

      var stamp = form.getAttribute('data-confirm') || (confirmEl ? confirmEl.getAttribute('data-confirm') : '');
      if (confirmEl && confirmedForms[stamp] !== true) {
        e.preventDefault();
        var title = (confirmEl.getAttribute && confirmEl.getAttribute('data-confirm-title')) || 'Confirm action';
        var danger = confirmEl.hasAttribute('data-confirm-danger');
        dialogConfirm({ title: title, message: stamp || 'Proceed with this action?', danger: danger }).then(function (approved) {
          if (approved) {
            confirmedForms[stamp] = true;
            if (form.requestSubmit) form.requestSubmit();
            else form.submit();
          } else {
            toast('No changes were made.', { type: 'info' });
          }
        });
        return;
      }
      if (confirmedForms[stamp]) delete confirmedForms[stamp];

      e.preventDefault();
      if (isValidate) {
        toast(form.getAttribute('data-success') || 'Your changes have been saved.', { type: 'success', title: 'Success' });
        if (form.hasAttribute('data-reset-on-success')) form.reset();
      } else {
        toast(form.getAttribute('data-success') || 'Done.', { type: 'success' });
      }
    });

    document.querySelectorAll('[data-validate] input,[data-validate] select,[data-validate] textarea').forEach(function (control) {
      control.addEventListener('input', function () {
        if (control.getAttribute('aria-invalid') === 'true') setFieldState(control, fieldErrorFor(control, control.form));
      });
    });

    document.addEventListener('click', function (e) {
      var pw = e.target.closest ? e.target.closest('[data-password-toggle]') : null;
      if (pw) {
        e.preventDefault();
        var targetId = pw.getAttribute('data-password-toggle') || pw.getAttribute('data-target');
        var input = targetId ? document.getElementById(targetId)
          : (pw.closest('.field') || pw.parentElement).querySelector('input');
        if (!input) return;
        var show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        pw.setAttribute('aria-pressed', show ? 'true' : 'false');
        var use = pw.querySelector('use');
        if (use) use.setAttribute('href', ICON_BASE + 'icons.svg#i-' + (show ? 'lock' : 'eye'));
        input.focus();
        return;
      }

      var closer = e.target.closest ? e.target.closest('[data-confirm]:not(form)') : null;
      if (closer) {
        if (closer.closest('form')) return;
        if (confirmedClicks[closer.__taId || (closer.__taId = 'c' + (++errSeq))]) return;
        e.preventDefault();
        var msg = closer.getAttribute('data-confirm') || 'Proceed with this action?';
        var cTitle = closer.getAttribute('data-confirm-title') || 'Confirm action';
        var cDanger = closer.hasAttribute('data-confirm-danger');
        dialogConfirm({ title: cTitle, message: msg, danger: cDanger }).then(function (approved) {
          if (approved) {
            confirmedClicks[closer.__taId] = true;
            closer.click();
          }
        });
      }
    });

    document.querySelectorAll('[data-counter-for]').forEach(function (node) {
      var id = node.getAttribute('data-counter-for');
      var input = document.getElementById(id) || document.querySelector('[name="' + id + '"]');
      if (!input) return;
      var max = parseInt(node.getAttribute('data-counter-max') || input.getAttribute('maxlength') || '0', 10);
      function update() {
        node.textContent = max > 0 ? (input.value.length + ' / ' + max) : String(input.value.length);
      }
      input.addEventListener('input', update);
      update();
    });
  }

  Ta.form = { init: initForm, validate: validateForm };

  /* =====================================================================
     Ta.kiosk - door scanner terminal (clock, camera, manual, gate flash)
     ===================================================================== */
  /* ===== Ta.kiosk ===== */
  var DUPLICATE_WINDOW_MS = 2000;

  function liveEvent() {
    var events = Ta.store.get('events') || [];
    for (var i = 0; i < events.length; i++) {
      if (events[i].status === 'live') return events[i];
    }
    return null;
  }

  function scansCollection() {
    var scans = Ta.store.get('scans');
    if (!Array.isArray(scans)) scans = [];
    return scans;
  }

  function extractCode(payload) {
    var text = String(payload || '').trim();
    var at = text.match(/AT-[A-Z0-9]{4}-[A-Z0-9]{4}/i);
    if (at) return at[0].toUpperCase();
    var idm = text.match(/\d{4}-\d{5}/);
    if (idm) return idm[0];
    return text.toUpperCase();
  }

  function validateScan(raw) {
    var code = extractCode(raw);
    if (!code) return { ok: false, reason: 'unknown' };

    var passes = Ta.store.get('passes') || [];
    var pass = null;
    for (var i = 0; i < passes.length; i++) {
      if (String(passes[i].code).toUpperCase() === code) { pass = passes[i]; break; }
    }

    var member = null;
    var event = null;
    var registration = null;
    var passId = null;

    if (pass) {
      passId = pass.id;
      event = Ta.store.find('events', pass.eventId);
      member = Ta.store.find('members', pass.memberId);
      registration = pass.registrationId ? Ta.store.find('registrations', pass.registrationId) : null;
    } else {
      var members = Ta.store.get('members') || [];
      for (var m = 0; m < members.length; m++) {
        if (String(members[m].studentId) === code || String(members[m].id) === code) { member = members[m]; break; }
      }
      if (member) {
        event = liveEvent();
        var regs = Ta.store.get('registrations') || [];
        for (var r = 0; r < regs.length; r++) {
          if (regs[r].memberId === member.id && event && regs[r].eventId === event.id) { registration = regs[r]; break; }
        }
      }
    }

    if (!member) return { ok: false, reason: 'unknown' };
    if (!event || event.status !== 'live') return { ok: false, reason: 'invalid' };
    if (registration && registration.status === 'cancelled') return { ok: false, reason: 'cancelled' };

    var scans = scansCollection();
    var now = Date.now();
    for (var s = scans.length - 1; s >= 0; s--) {
      var rec = scans[s];
      if (rec.passId && rec.passId === passId && (now - rec.at) < DUPLICATE_WINDOW_MS) {
        return { ok: false, reason: 'duplicate', member: member, event: event };
      }
      if (!passId && rec.memberId === member.id && rec.eventId === event.id && (now - rec.at) < DUPLICATE_WINDOW_MS) {
        return { ok: false, reason: 'duplicate', member: member, event: event };
      }
    }

    if (registration && registration.status === 'attended') return { ok: false, reason: 'already', member: member, event: event };
    return { ok: true, passId: passId, member: member, event: event, registration: registration };
  }

  function ensureFlashHost(root) {
    var host = root.querySelector('[data-kiosk-flash]');
    if (host) return host;
    host = document.createElement('div');
    host.className = 'ta-kiosk-flash-host';
    host.setAttribute('data-kiosk-flash', '');
    host.setAttribute('aria-live', 'assertive');
    root.appendChild(host);
    return host;
  }

  function initKiosk(root) {
    var clockEl = root.querySelector('[data-kiosk-clock]');
    var video = root.querySelector('[data-kiosk-video]');
    var canvas = root.querySelector('[data-kiosk-canvas]');
    var fallback = root.querySelector('[data-kiosk-fallback]');
    var startBtn = root.querySelector('[data-kiosk-start]');
    var stopBtn = root.querySelector('[data-kiosk-stop]');
    var manualForm = root.querySelector('[data-kiosk-manual]');
    var countEl = root.querySelector('[data-kiosk-count]');
    var flashHost = ensureFlashHost(root);

    var scanning = false;
    var rafId = null;
    var stream = null;
    var lastScan = 0;
    var flashTimer = null;

    if (canvas) {
      canvas.style.display = 'none';
    } else {
      canvas = document.createElement('canvas');
      canvas.setAttribute('data-kiosk-canvas', '');
      canvas.style.display = 'none';
      root.appendChild(canvas);
    }

    if (clockEl) {
      var tick = function () {
        var now = new Date();
        var date = now.toLocaleDateString(undefined, { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' });
        var time = now.toLocaleTimeString(undefined, { hour12: false });
        clockEl.textContent = time + ' - ' + date;
      };
      tick();
      window.setInterval(tick, 1000);
    }

    function updateCount() {
      if (!countEl) return;
      var ev = liveEvent();
      var scans = scansCollection();
      var n = scans.length;
      if (ev) {
        n = scans.filter(function (s) { return s.eventId === ev.id; }).length;
      }
      countEl.textContent = String(n);
    }

    function gateFlash(kind) {
      if (flashTimer) window.clearTimeout(flashTimer);
      var node = document.createElement('div');
      node.className = 'gate-flash';
      node.setAttribute('data-gate-flash', kind);
      var tone = kind === 'success' ? 'check-circle' : (kind === 'duplicate' ? 'alert-triangle' : 'x-circle');
      var label = kind === 'success' ? 'Check-in recorded'
        : (kind === 'duplicate' ? 'Already scanned' : 'Verification failed');
      node.innerHTML = icon(tone) + '<div class="toast__body"><div class="toast__title">' + esc(label) + '</div>' +
        '<div class="toast__message ta-gate-flash-msg"></div></div>';
      flashHost.innerHTML = '';
      flashHost.appendChild(node);
      var msg = node.querySelector('.ta-gate-flash-msg');
      if (msg) msg.textContent = gateFlash._msg || '';
      flashTimer = window.setTimeout(function () {
        if (node.parentNode) node.parentNode.removeChild(node);
      }, 1400);
    }

    function handleScan(raw) {
      var result = validateScan(raw);
      if (result.ok) {
        var scans = scansCollection();
        scans.push({
          id: 'scan-' + (scans.length + 1) + '-' + Date.now(),
          passId: result.passId,
          memberId: result.member.id,
          eventId: result.event.id,
          at: Date.now()
        });
        Ta.store.set('scans', scans);
        if (result.registration && result.registration.status === 'confirmed') {
          result.registration.status = 'attended';
          Ta.store.set('registrations', Ta.store.get('registrations') || []);
        }
        gateFlash._msg = result.member.name + ' - ' + (result.member.studentId || result.member.id);
        gateFlash('success');
        toast('Checked in ' + result.member.name + '.', { type: 'success', title: 'Verified' });
        updateCount();
      } else if (result.reason === 'duplicate' || result.reason === 'already') {
        var who = result.member ? result.member.name + ' - ' : '';
        if (result.reason === 'already') {
          gateFlash._msg = who + 'already checked in.';
          gateFlash('duplicate');
          toast('Already checked in. No duplicate recorded.', { type: 'warning', title: 'Already checked in' });
        } else {
          gateFlash._msg = who + 'already scanned moments ago.';
          gateFlash('duplicate');
          toast('Already scanned. No duplicate recorded.', { type: 'warning', title: 'Duplicate pass' });
        }
      } else {
        gateFlash._msg = result.reason === 'cancelled' ? 'Registration was cancelled.'
          : 'No valid pass or member found.';
        gateFlash('invalid');
        toast(gateFlash._msg, { type: 'error', title: 'Check-in failed' });
      }
      return result;
    }

    function loop(ts) {
      if (!scanning) return;
      rafId = window.requestAnimationFrame(loop);
      if (ts - lastScan < 300) return;
      lastScan = ts;
      if (!video || video.readyState !== video.HAVE_ENOUGH_DATA) return;
      canvas.width = video.videoWidth;
      canvas.height = video.videoHeight;
      var ctx = canvas.getContext('2d');
      if (!ctx) return;
      ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
      if (typeof window.jsQR === 'function') {
        var data = ctx.getImageData(0, 0, canvas.width, canvas.height);
        var code = window.jsQR(data.data, data.width, data.height, { inversionAttempts: 'dontInvert' });
        if (code && code.data) handleScan(code.data);
      }
    }

    function showFallback() {
      if (fallback) fallback.removeAttribute('hidden');
    }

    function startCamera() {
      if (!video || !navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        showFallback();
        toast('Camera unavailable. Use manual entry.', { type: 'warning', title: 'Manual mode' });
        return;
      }
      navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' } }).then(function (s) {
        stream = s;
        video.srcObject = s;
        var played = video.play();
        if (played && played.catch) played.catch(function () { /* autoplay guard */ });
        scanning = true;
        if (startBtn) startBtn.setAttribute('aria-pressed', 'true');
        if (stopBtn) stopBtn.removeAttribute('hidden');
        if (typeof window.jsQR !== 'function') {
          toast('QR decoder not loaded. Manual entry still works.', { type: 'info', title: 'Scanner limited' });
        }
        rafId = window.requestAnimationFrame(loop);
      }).catch(function () {
        showFallback();
        toast('Camera permission denied. Use manual entry.', { type: 'warning', title: 'Manual mode' });
      });
    }

    function stopCamera() {
      scanning = false;
      if (rafId) { window.cancelAnimationFrame(rafId); rafId = null; }
      if (stream) {
        stream.getTracks().forEach(function (t) { try { t.stop(); } catch (e) { /* ignore */ } });
        stream = null;
      }
      if (video) video.srcObject = null;
      if (startBtn) startBtn.setAttribute('aria-pressed', 'false');
      if (stopBtn) stopBtn.setAttribute('hidden', '');
    }

    if (startBtn) {
      startBtn.setAttribute('aria-pressed', 'false');
      startBtn.addEventListener('click', function (e) {
        e.preventDefault();
        startCamera();
      });
    }
    if (stopBtn) {
      stopBtn.addEventListener('click', function (e) {
        e.preventDefault();
        stopCamera();
      });
    }

    if (manualForm) {
      manualForm.addEventListener('submit', function (e) {
        e.preventDefault();
        var input = manualForm.querySelector('input');
        var value = input ? input.value.trim() : '';
        if (!value) {
          toast('Enter a pass code or student number.', { type: 'warning' });
          return;
        }
        handleScan(value);
        if (input) input.value = '';
      });
    }

    document.addEventListener('visibilitychange', function () {
      if (document.hidden && scanning) stopCamera();
    });

    updateCount();
    Ta.kiosk.flash = gateFlash;
    Ta.kiosk.validate = validateScan;
  }

  Ta.kiosk = { init: function () { var root = document.querySelector('[data-kiosk]'); if (root) initKiosk(root); } };

  /* =====================================================================
     Ta.export - CSV, canvas pass image, print / export bindings
     ===================================================================== */
  /* ===== Ta.export ===== */
  function csvCell(value) {
    var v = value == null ? '' : String(value);
    if (/[",\r\n]/.test(v)) return '"' + v.replace(/"/g, '""') + '"';
    return v;
  }

  function exportCsv(filename, rows, headers) {
    rows = rows || [];
    var cols = headers;
    if (!cols) {
      if (rows.length) {
        var first = rows[0];
        cols = Array.isArray(first) ? first.map(function (_, i) { return 'field' + (i + 1); }) : Object.keys(first);
      } else {
        cols = [];
      }
    }
    var lines = [cols.map(csvCell).join(',')];
    rows.forEach(function (row) {
      var values = Array.isArray(row) ? row : cols.map(function (c) { return row[c]; });
      lines.push(values.map(csvCell).join(','));
    });
    var text = lines.join('\r\n') + '\r\n';
    downloadBlob(filename, new Blob([text], { type: 'text/csv;charset=utf-8' }));
    return text;
  }

  function drawDecorativeQr(ctx, x, y, size, seedText) {
    var cells = 21;
    var cell = size / cells;
    var seed = 0;
    for (var i = 0; i < seedText.length; i++) seed = (seed * 31 + seedText.charCodeAt(i)) >>> 0;
    ctx.fillStyle = '#FFFFFF';
    ctx.fillRect(x, y, size, size);
    ctx.fillStyle = '#0B1120';
    function finder(cx, cy) {
      ctx.fillRect(x + cx * cell, y + cy * cell, cell * 7, cell * 7);
      ctx.fillStyle = '#FFFFFF';
      ctx.fillRect(x + (cx + 1) * cell, y + (cy + 1) * cell, cell * 5, cell * 5);
      ctx.fillStyle = '#0B1120';
      ctx.fillRect(x + (cx + 2) * cell, y + (cy + 2) * cell, cell * 3, cell * 3);
    }
    finder(0, 0);
    finder(cells - 7, 0);
    finder(0, cells - 7);
    for (var r = 0; r < cells; r++) {
      for (var c = 0; c < cells; c++) {
        if ((r < 8 && c < 8) || (r < 8 && c > cells - 9) || (r > cells - 9 && c < 8)) continue;
        seed = (seed * 1664525 + 1013904223) >>> 0;
        if ((seed >>> 16) & 1) ctx.fillRect(x + c * cell, y + r * cell, cell, cell);
      }
    }
  }

  function passImage(pass, member, event) {
    var w = 1000;
    var h = 600;
    var canvas = document.createElement('canvas');
    canvas.width = w;
    canvas.height = h;
    var ctx = canvas.getContext('2d');

    ctx.fillStyle = '#0B1120';
    ctx.fillRect(0, 0, w, h);
    ctx.fillStyle = '#D97706';
    ctx.fillRect(0, 0, w, 12);

    ctx.fillStyle = '#FFFFFF';
    ctx.font = '800 38px Outfit, Arial, sans-serif';
    ctx.fillText('AttendTa', 56, 88);
    ctx.fillStyle = '#94A3B8';
    ctx.font = '600 15px "JetBrains Mono", monospace';
    ctx.fillText('OFFICIAL EVENT PASS', 56, 116);

    ctx.fillStyle = '#FFFFFF';
    ctx.font = '700 34px Outfit, Arial, sans-serif';
    var title = (event && event.title) || 'Organization Event';
    var words = title.split(' ');
    var line = '';
    var y = 210;
    for (var i = 0; i < words.length; i++) {
      var test = line ? line + ' ' + words[i] : words[i];
      if (ctx.measureText(test).width > 520 && line) {
        ctx.fillText(line, 56, y);
        line = words[i];
        y += 42;
      } else {
        line = test;
      }
    }
    if (line) ctx.fillText(line, 56, y);

    ctx.fillStyle = '#FFFFFF';
    ctx.font = '700 28px Outfit, Arial, sans-serif';
    ctx.fillText((member && member.name) || 'Member', 56, 360);
    ctx.fillStyle = '#94A3B8';
    ctx.font = '500 22px "JetBrains Mono", monospace';
    ctx.fillText((member && (member.studentId || member.id)) || '', 56, 398);

    ctx.fillStyle = '#D97706';
    ctx.font = '700 26px "JetBrains Mono", monospace';
    ctx.fillText((pass && pass.code) || '', 56, 520);
    ctx.fillStyle = '#64748B';
    ctx.font = '500 14px "JetBrains Mono", monospace';
    ctx.fillText('Present this code at the door scanner', 56, 548);

    var qrSize = 220;
    var qx = w - qrSize - 56;
    var qy = 200;
    ctx.fillStyle = '#FFFFFF';
    ctx.fillRect(qx - 14, qy - 14, qrSize + 28, qrSize + 28);

    function finish() {
      var name = 'attendta-pass-' + ((pass && pass.code) || 'code') + '.png';
      if (canvas.toBlob) {
        canvas.toBlob(function (blob) { if (blob) downloadBlob(name, blob); }, 'image/png');
      } else {
        downloadBlob(name, new Blob([canvas.toDataURL('image/png')], { type: 'image/png' }));
      }
    }

    var svg = null;
    try {
      if (window.AttendTaQR && typeof window.AttendTaQR.generateSvg === 'function') {
        svg = window.AttendTaQR.generateSvg((pass && pass.code) || 'ATTENDTA', {
          moduleColor: '#0B1120', bgColor: '#FFFFFF', quietZone: 8
        });
      }
    } catch (e) { svg = null; }

    if (svg) {
      var img = new Image();
      img.onload = function () {
        try { ctx.drawImage(img, qx, qy, qrSize, qrSize); } catch (e) { drawDecorativeQr(ctx, qx, qy, qrSize, (pass && pass.code) || 'ATTENDTA'); }
        finish();
      };
      img.onerror = function () {
        drawDecorativeQr(ctx, qx, qy, qrSize, (pass && pass.code) || 'ATTENDTA');
        finish();
      };
      img.src = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg);
    } else {
      drawDecorativeQr(ctx, qx, qy, qrSize, (pass && pass.code) || 'ATTENDTA');
      finish();
    }
    return canvas;
  }

  function prepareExport() {
    document.addEventListener('click', function (e) {
      var csv = e.target.closest ? e.target.closest('[data-export-csv]') : null;
      if (csv) {
        e.preventDefault();
        var coll = csv.getAttribute('data-export-csv');
        var rows = Ta.store.get(coll) || [];
        var headers = rows.length && !Array.isArray(rows[0]) ? Object.keys(rows[0]) : undefined;
        exportCsv(coll + '.csv', rows, headers);
        toast('Exported ' + coll + '.csv', { type: 'success' });
        return;
      }
      var print = e.target.closest ? e.target.closest('[data-print]') : null;
      if (print) {
        e.preventDefault();
        window.print();
      }
    });
  }

  Ta.export = {
    csv: exportCsv,
    passImage: passImage,
    prepare: prepareExport,
    init: prepareExport
  };

  /* =====================================================================
     Ta.motion - reduced motion, reveal-on-scroll, flash helper
     ===================================================================== */
  /* ===== Ta.motion ===== */
  var motionObserver = null;

  function flashElement(el) {
    if (!el) return;
    el.classList.add('is-flash');
    window.setTimeout(function () { el.classList.remove('is-flash'); }, 240);
  }

  function initMotion() {
    var reduced = prefersReduced();
    Ta.motion.reduced = reduced;

    var reveals = document.querySelectorAll('[data-reveal]');
    if (!reveals.length) return;

    if (reduced) {
      Array.prototype.forEach.call(reveals, function (el) { el.classList.add('is-visible'); });
      return;
    }

    Array.prototype.forEach.call(reveals, function (el) {
      var group = el.closest('[data-reveal-group]');
      if (group) {
        var siblings = group.querySelectorAll('[data-reveal]');
        var idx = Array.prototype.indexOf.call(siblings, el);
        if (idx > 0) el.style.setProperty('--reveal-delay', (idx * 60) + 'ms');
      }
    });

    if (!('IntersectionObserver' in window)) {
      Array.prototype.forEach.call(reveals, function (el) { el.classList.add('is-visible'); });
      return;
    }

    motionObserver = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          entry.target.classList.add('is-visible');
          motionObserver.unobserve(entry.target);
        }
      });
    }, { threshold: 0.15 });

    Array.prototype.forEach.call(reveals, function (el) {
      if (!el.classList.contains('is-visible')) motionObserver.observe(el);
    });
  }

  Ta.motion = { reduced: prefersReduced(), init: initMotion, flash: flashElement };

  /* =====================================================================
     Ta.fonts - density + font pairing preferences
     ===================================================================== */
  /* ===== Ta.fonts ===== */
  function setDensity(density) {
    density = density === 'compact' ? 'compact' : 'comfortable';
    document.documentElement.setAttribute('data-density', density);
    Ta.store.update('prefs', function (prefs) {
      prefs = prefs || { density: 'comfortable', reducedMotion: false };
      prefs.density = density;
      return prefs;
    });
    return density;
  }

  function setPairing(name) {
    if (name) document.documentElement.setAttribute('data-font', name);
    else document.documentElement.removeAttribute('data-font');
    return name;
  }

  function initFonts() {
    var prefs = Ta.store.get('prefs') || {};
    if (prefs.density) document.documentElement.setAttribute('data-density', prefs.density);
    if (prefs.font) document.documentElement.setAttribute('data-font', prefs.font);
  }

  Ta.fonts = { init: initFonts, setDensity: setDensity, setPairing: setPairing };

  /* Font gate: reveal .font-gate surfaces once fonts settle (fail-open). */
  function gateFonts() {
    var root = document.documentElement;
    var reveal = function () {
      if (!root.classList.contains('fonts-ready')) root.classList.add('fonts-ready');
    };
    try {
      if (document.fonts && document.fonts.ready && typeof document.fonts.ready.then === 'function') {
        document.fonts.ready.then(reveal, reveal);
        window.setTimeout(reveal, 1500);
      } else {
        reveal();
      }
    } catch (e) { reveal(); }
  }

  /* =====================================================================
     bootstrap - DOMContentLoaded, isolated per module
     ===================================================================== */
  function boot() {
    try { Ta.fonts.init(); } catch (e) { console.error(e); }
    try { gateFonts(); } catch (e) { console.error(e); }
    try { Ta.mood.applyFreeze(); } catch (e) { console.error(e); }
    try { Ta.nav.init(); } catch (e) { console.error(e); }
    try { Ta.sidebar.init(); } catch (e) { console.error(e); }
    try { Ta.drawer.init(); } catch (e) { console.error(e); }
    try { Ta.tabs.init(); } catch (e) { console.error(e); }
    try { Ta.menu.init(); } catch (e) { console.error(e); }
    try { Ta.table.init(); } catch (e) { console.error(e); }
    try { Ta.form.init(); } catch (e) { console.error(e); }
    try { Ta.kiosk.init(); } catch (e) { console.error(e); }
    try { Ta.export.init(); } catch (e) { console.error(e); }

    try {
      document.dispatchEvent(new CustomEvent('ta:ready', { detail: Ta }));
    } catch (e) { console.error(e); }

    try { Ta.motion.init(); } catch (e) { console.error(e); }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', boot);
  } else {
    boot();
  }
})();
