(function() {
    'use strict';

    let analyticsData = {
        overall: {
            winRate: 43.6,
            top4Rate: 71.8,
            popularComp: '星之守护者'
        },
        comps: [
            { name: '星之守护者', winRate: 56.3 },
            { name: '机甲霸王龙', winRate: 50.0 },
            { name: '地下魔盗团', winRate: 41.7 },
            { name: '淘气包', winRate: 40.9 },
            { name: '黑客', winRate: 38.9 }
        ]
    };

    // 模拟装备推荐数据
    let equipmentData = {
        recommendations: [
            {
                itemName: '无尽之刃',
                priority: 1,
                winRate: 68.5,
                top4Rate: 85.2,
                top1Rate: 42.1,
                economicValue: 1.8,
                components: ['大剑', '拳套']
            },
            {
                itemName: '饮血剑',
                priority: 2,
                winRate: 62.3,
                top4Rate: 78.9,
                top1Rate: 38.7,
                economicValue: 1.6,
                components: ['大剑', '魔抗']
            },
            {
                itemName: '鬼索的狂暴之刃',
                priority: 2,
                winRate: 60.8,
                top4Rate: 76.5,
                top1Rate: 36.2,
                economicValue: 1.5,
                components: ['反曲弓', '大棒']
            },
            {
                itemName: '守护天使',
                priority: 3,
                winRate: 58.2,
                top4Rate: 74.1,
                top1Rate: 34.5,
                economicValue: 1.4,
                components: ['大剑', '锁子甲']
            },
            {
                itemName: '蓝霸符',
                priority: 3,
                winRate: 56.7,
                top4Rate: 72.3,
                top1Rate: 32.8,
                economicValue: 1.3,
                components: ['水滴', '水滴']
            }
        ],
        synthesisPaths: [
            {
                itemName: '无尽之刃',
                priority: 1,
                feasibility: 0.8,
                availableComponents: ['大剑'],
                missingComponents: ['拳套'],
                winRate: 68.5,
                top1Rate: 42.1
            },
            {
                itemName: '饮血剑',
                priority: 2,
                feasibility: 0.6,
                availableComponents: ['魔抗'],
                missingComponents: ['大剑'],
                winRate: 62.3,
                top1Rate: 38.7
            },
            {
                itemName: '鬼索的狂暴之刃',
                priority: 2,
                feasibility: 0.5,
                availableComponents: ['反曲弓'],
                missingComponents: ['大棒'],
                winRate: 60.8,
                top1Rate: 36.2
            },
            {
                itemName: '守护天使',
                priority: 3,
                feasibility: 0.4,
                availableComponents: ['锁子甲'],
                missingComponents: ['大剑'],
                winRate: 58.2,
                top1Rate: 34.5
            }
        ]
    };

    // 游戏状态数据
    let gameState = null;

    // 防抖函数
    function debounce(func, delay) {
        let timeoutId;
        return function() {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(() => func.apply(this, arguments), delay);
        };
    }

    document.addEventListener('DOMContentLoaded', () => {
        initTabs();
        initMinimize();
        initOverwolf();
        initTheme();
        initI18n();
        loadAnalyticsData();
    });

    // 初始化Overwolf API
    function initOverwolf() {
        console.log('Initializing Overwolf integration...');
        
        // 检查Overwolf API是否可用
        if (typeof overwolf !== 'undefined') {
            console.log('Overwolf API detected');
            initOverwolfEvents();
        } else {
            console.log('Overwolf API not available, running in demo mode');
        }
        
        // 发送UI准备就绪消息
        sendMessageToBackend('ui_ready', {});
    }

    // 初始化Overwolf事件
    function initOverwolfEvents() {
        // 注册游戏信息更新事件
        overwolf.games.events.onInfoUpdates2.addListener(onInfoUpdate);
        
        // 注册游戏事件
        overwolf.games.events.onNewEvents.addListener(onGameEvent);
        
        console.log('Overwolf events initialized');
    }

    // 游戏信息更新事件
    function onInfoUpdate(info) {
        console.log('Info update received');
        if (info && info.liveClientData) {
            sendMessageToBackend('overwolf_info_update', info);
        }
    }

    // 游戏事件
    function onGameEvent(events) {
        console.log('Game events received:', events.length);
        if (events && events.length > 0) {
            sendMessageToBackend('overwolf_game_event', events);
        }
    }

    // 发送消息到后端
    function sendMessageToBackend(type, data) {
        const message = {
            type: type,
            payload: data
        };
        
        // 这里需要实现与后端的通信机制
        // 暂时使用console.log模拟
        console.log('Sending message to backend:', message);
        
        // 实际项目中，这里应该使用WebSocket或其他通信方式
    }

    // 处理来自后端的消息
    function handleBackendMessage(message) {
        console.log('Received message from backend:', message.type);
        
        switch (message.type) {
            case 'full_state':
                updateGameState(message.payload);
                break;
            case 'state_changed':
                updateGameState(message.payload);
                break;
            case 'recommendations':
                updateRecommendations(message.payload);
                break;
            default:
                console.log('Unknown message type:', message.type);
                break;
        }
    }

    // 更新游戏状态
    function updateGameState(state) {
        gameState = state;
        console.log('Game state updated:', state);
        
        // 更新连接状态
        updateConnectionStatus(true);
        
        // 更新回合信息
        if (state.gameInfo) {
            updateRoundInfo(state.gameInfo);
        }
        
        // 更新棋盘状态
        if (state.activePlayer) {
            updateBoardState(state.activePlayer.board);
        }
    }

    // 更新连接状态
    function updateConnectionStatus(connected) {
        const statusDot = document.getElementById('connection-status');
        if (statusDot) {
            statusDot.className = `status-dot ${connected ? 'connected' : 'disconnected'}`;
        }
    }

    // 更新回合信息
    function updateRoundInfo(gameInfo) {
        const roundInfo = document.getElementById('round-info');
        if (roundInfo) {
            roundInfo.textContent = `Round ${gameInfo.round} - Stage ${gameInfo.stage}`;
        }
    }

    // 更新棋盘状态
    function updateBoardState(boardUnits) {
        const boardPanel = document.getElementById('panel-board');
        if (boardPanel) {
            const content = boardPanel.querySelector('.panel-content');
            if (content) {
                // 清空现有内容
                const unitCards = content.querySelectorAll('.unit-card');
                unitCards.forEach(card => card.remove());
                
                // 添加新的单位卡片
                if (boardUnits && boardUnits.length > 0) {
                    boardUnits.forEach(unit => {
                        const unitCard = document.createElement('div');
                        unitCard.className = 'unit-card';
                        unitCard.innerHTML = `
                            <span class="unit-name">${unit.championName}</span>
                            <span class="unit-stars">${'★'.repeat(unit.starLevel)}</span>
                            <div class="unit-items">
                                ${unit.items.map(item => `<div class="item-icon" title="${item}"></div>`).join('')}
                            </div>
                        `;
                        content.appendChild(unitCard);
                    });
                } else {
                    // 显示空棋盘提示
                    const emptyMessage = document.createElement('div');
                    emptyMessage.className = 'empty-board';
                    emptyMessage.textContent = '棋盘为空';
                    content.appendChild(emptyMessage);
                }
            }
        }
    }

    // 更新推荐数据
    function updateRecommendations(recommendations) {
        console.log('Recommendations updated:', recommendations);
        // 这里需要实现推荐数据的更新逻辑
    }

    function initTabs() {
        const tabs = document.querySelectorAll('.tab');
        
        // 使用防抖处理，避免频繁触发
        const debouncedLoadAnalytics = debounce(loadAnalyticsData, 200);
        const debouncedLoadEquipment = debounce(loadEquipmentData, 200);

        tabs.forEach(tab => {
            tab.addEventListener('click', () => {
                tabs.forEach(t => t.classList.remove('active'));
                document.querySelectorAll('.tab-panel')
                    .forEach(p => p.classList.remove('active'));

                tab.classList.add('active');
                const panel = document.getElementById(`panel-${tab.dataset.tab}`);
                if (panel) {
                    panel.classList.add('active');
                    if (tab.dataset.tab === 'analytics') {
                        debouncedLoadAnalytics();
                    } else if (tab.dataset.tab === 'items') {
                        debouncedLoadEquipment();
                    }
                }
            });
        });
    }

    function initMinimize() {
        const btn = document.getElementById('btn-minimize');
        if (btn) {
            btn.addEventListener('click', () => {
                console.log('Minimize button clicked');
                // 这里需要实现最小化逻辑
            });
        }
    }

    function initTheme() {
        const savedTheme = localStorage.getItem('theme') || 'dark';
        if (savedTheme === 'light') {
            document.documentElement.classList.add('light-theme');
        } else {
            document.documentElement.classList.remove('light-theme');
        }
    }

    function loadAnalyticsData() {
        updateAnalyticsSummary();
        updateAnalyticsCompList();
    }

    function updateAnalyticsSummary() {
        const overall = analyticsData.overall;
        document.getElementById('analytics-win-rate').textContent = `${overall.winRate.toFixed(1)}%`;
        document.getElementById('analytics-top4-rate').textContent = `${overall.top4Rate.toFixed(1)}%`;
        document.getElementById('analytics-popular-comp').textContent = overall.popularComp;
    }

    function updateAnalyticsCompList() {
        const container = document.getElementById('analytics-comp-list');
        if (!container) return;

        container.innerHTML = '';

        analyticsData.comps.forEach(comp => {
            const item = document.createElement('div');
            item.className = 'analytics-comp-item';
            item.innerHTML = `
                <span class="analytics-comp-name">${comp.name}</span>
                <span class="analytics-comp-win-rate">${comp.winRate.toFixed(1)}%</span>
            `;
            container.appendChild(item);
        });
    }

    // 加载装备数据
    function loadEquipmentData() {
        updateEquipmentRecommendations();
        updateSynthesisPaths();
    }

    // 更新装备推荐
    function updateEquipmentRecommendations() {
        const container = document.getElementById('item-recommendations');
        if (!container) return;

        container.innerHTML = '';

        equipmentData.recommendations.forEach(item => {
            const suggestion = document.createElement('div');
            suggestion.className = `item-suggestion priority-${item.priority}`;
            suggestion.innerHTML = `
                <div>
                    <div class="item-name">${item.itemName}</div>
                    <div class="item-components">
                        ${item.components.map(comp => `<div class="component-icon" title="${comp}"></div>`).join('')}
                    </div>
                </div>
                <div class="item-stats">
                    <div class="item-win-rate">胜率: ${item.winRate.toFixed(1)}%</div>
                    <div class="item-top1-rate">登顶率: ${item.top1Rate.toFixed(1)}%</div>
                </div>
            `;
            container.appendChild(suggestion);
        });
    }

    // 更新合成路径
    function updateSynthesisPaths() {
        const container = document.getElementById('synthesis-paths');
        if (!container) return;

        container.innerHTML = '';

        equipmentData.synthesisPaths.forEach(path => {
            const synthesisPath = document.createElement('div');
            synthesisPath.className = `synthesis-path`;
            synthesisPath.innerHTML = `
                <div class="synthesis-path-header">
                    <div class="synthesis-item-name">${path.itemName}</div>
                    <div class="synthesis-feasibility">可行性: ${(path.feasibility * 100).toFixed(0)}%</div>
                </div>
                <div class="synthesis-components">
                    ${path.availableComponents.map(comp => `<span class="synthesis-available">${comp}</span>`).join(' ')}
                    ${path.missingComponents.map(comp => `<span class="synthesis-missing">${comp}</span>`).join(' ')}
                </div>
                <div class="item-stats" style="margin-top: 4px;">
                    <div class="item-win-rate">胜率: ${path.winRate.toFixed(1)}%</div>
                    <div class="item-top1-rate">登顶率: ${path.top1Rate.toFixed(1)}%</div>
                </div>
            `;
            container.appendChild(synthesisPath);
        });
    }
})();
