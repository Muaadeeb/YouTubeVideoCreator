// IndexedDB for large media; localStorage only holds lightweight project JSON.
(function () {
    const DB_NAME = 'asvc-assets';
    const DB_VERSION = 1;
    const STORE = 'assets';

    function openDb() {
        return new Promise((resolve, reject) => {
            const req = indexedDB.open(DB_NAME, DB_VERSION);
            req.onupgradeneeded = () => {
                const db = req.result;
                if (!db.objectStoreNames.contains(STORE)) {
                    db.createObjectStore(STORE, { keyPath: 'id' });
                }
            };
            req.onsuccess = () => resolve(req.result);
            req.onerror = () => reject(req.error || new Error('IndexedDB open failed'));
        });
    }

    function stripDataUrl(b64) {
        if (!b64) return '';
        const idx = b64.indexOf('base64,');
        return idx >= 0 ? b64.substring(idx + 7) : b64;
    }

    window.asvcAssetDb = {
        async put(id, mimeType, dataBase64) {
            if (!id || !dataBase64) return false;
            const db = await openDb();
            return new Promise((resolve, reject) => {
                const tx = db.transaction(STORE, 'readwrite');
                tx.objectStore(STORE).put({
                    id: String(id),
                    mimeType: mimeType || 'application/octet-stream',
                    dataBase64: stripDataUrl(dataBase64),
                    updatedAt: Date.now()
                });
                tx.oncomplete = () => { db.close(); resolve(true); };
                tx.onerror = () => { db.close(); reject(tx.error || new Error('IndexedDB put failed')); };
            });
        },

        async get(id) {
            if (!id) return null;
            const db = await openDb();
            return new Promise((resolve, reject) => {
                const tx = db.transaction(STORE, 'readonly');
                const req = tx.objectStore(STORE).get(String(id));
                req.onsuccess = () => {
                    db.close();
                    const row = req.result;
                    if (!row || !row.dataBase64) {
                        resolve(null);
                        return;
                    }
                    resolve({
                        id: row.id,
                        mimeType: row.mimeType || 'application/octet-stream',
                        dataBase64: row.dataBase64
                    });
                };
                req.onerror = () => { db.close(); reject(req.error || new Error('IndexedDB get failed')); };
            });
        },

        async delete(id) {
            if (!id) return;
            const db = await openDb();
            return new Promise((resolve, reject) => {
                const tx = db.transaction(STORE, 'readwrite');
                tx.objectStore(STORE).delete(String(id));
                tx.oncomplete = () => { db.close(); resolve(); };
                tx.onerror = () => { db.close(); reject(tx.error || new Error('IndexedDB delete failed')); };
            });
        },

        async deleteMany(ids) {
            if (!ids || !ids.length) return;
            const db = await openDb();
            return new Promise((resolve, reject) => {
                const tx = db.transaction(STORE, 'readwrite');
                const store = tx.objectStore(STORE);
                for (const id of ids) {
                    store.delete(String(id));
                }
                tx.oncomplete = () => { db.close(); resolve(); };
                tx.onerror = () => { db.close(); reject(tx.error || new Error('IndexedDB deleteMany failed')); };
            });
        },

        async clear() {
            const db = await openDb();
            return new Promise((resolve, reject) => {
                const tx = db.transaction(STORE, 'readwrite');
                tx.objectStore(STORE).clear();
                tx.oncomplete = () => { db.close(); resolve(); };
                tx.onerror = () => { db.close(); reject(tx.error || new Error('IndexedDB clear failed')); };
            });
        }
    };

    window.asvcLocalStorage = {
        setItem(key, value) {
            try {
                localStorage.setItem(key, value);
                return { ok: true };
            } catch (e) {
                const name = e && e.name ? e.name : 'Error';
                const message = e && e.message ? e.message : String(e);
                return { ok: false, name, message };
            }
        },
        getItem(key) {
            return localStorage.getItem(key);
        },
        removeItem(key) {
            localStorage.removeItem(key);
        },
        keys(prefix) {
            const out = [];
            const p = prefix || '';
            for (let i = 0; i < localStorage.length; i++) {
                const key = localStorage.key(i);
                if (key && key.startsWith(p)) out.push(key);
            }
            return out;
        }
    };
})();
