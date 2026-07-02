const BridgeClient = {
    _handlers: {},
    _isReady: false,

    init() {
        if (typeof overwolf !== 'undefined' && overwolf.web) {
            overwolf.web.onMessageReceived.addListener((message) => {
                try {
                    const data = typeof message === 'string'
                        ? JSON.parse(message) : message;

                    const handlers = this._handlers[data.type] || [];
                    handlers.forEach(handler => handler(data.payload));
                } catch (e) {
                    console.error('Bridge message parse error:', e);
                }
            });
        }

        this.send('ui_ready', {});
        this._isReady = true;
        console.log('[Bridge] Initialized');
    },

    send(type, payload) {
        const message = JSON.stringify({ type, payload });
        
        if (typeof overwolf !== 'undefined' && overwolf.web) {
            overwolf.web.sendMessage(message);
        } else {
            console.warn('[Bridge] Overwolf API not available, simulating message:', message);
        }
    },

    on(type, handler) {
        if (!this._handlers[type]) {
            this._handlers[type] = [];
        }
        this._handlers[type].push(handler);
    },

    off(type, handler) {
        if (!this._handlers[type]) return;
        this._handlers[type] = this._handlers[type].filter(h => h !== handler);
    },

    requestRecommendations() {
        this.send('request_recommendations', {});
    },

    toggleOverlay() {
        this.send('toggle_overlay', {});
    }
};

window.BridgeClient = BridgeClient;
