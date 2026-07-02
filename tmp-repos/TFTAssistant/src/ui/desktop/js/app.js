(function() {
    'use strict';

    let mockData = {
        overallStats: {
            totalMatches: 156,
            wins: 68,
            top4s: 112,
            top1s: 24
        },
        winRateTrend: [
            { date: '2024-03-15', winRate: 42.5, top4Rate: 68.2 },
            { date: '2024-03-20', winRate: 45.8, top4Rate: 70.5 },
            { date: '2024-03-25', winRate: 43.2, top4Rate: 69.1 },
            { date: '2024-03-30', winRate: 48.7, top4Rate: 72.8 },
            { date: '2024-04-05', winRate: 51.3, top4Rate: 74.5 },
            { date: '2024-04-10', winRate: 46.9, top4Rate: 71.2 },
            { date: '2024-04-12', winRate: 43.6, top4Rate: 71.8 }
        ],
        compStats: [
            {
                name: '星之守护者',
                matches: 32,
                wins: 18,
                top4s: 26,
                winRate: 56.3,
                top4Rate: 81.3
            },
            {
                name: '机甲霸王龙',
                matches: 28,
                wins: 14,
                top4s: 20,
                winRate: 50.0,
                top4Rate: 71.4
            },
            {
                name: '地下魔盗团',
                matches: 24,
                wins: 10,
                top4s: 16,
                winRate: 41.7,
                top4Rate: 66.7
            },
            {
                name: '淘气包',
                matches: 22,
                wins: 9,
                top4s: 15,
                winRate: 40.9,
                top4Rate: 68.2
            },
            {
                name: '黑客',
                matches: 18,
                wins: 7,
                top4s: 11,
                winRate: 38.9,
                top4Rate: 61.1
            }
        ],
        history: [
            {
                id: 1,
                date: '2024-04-12 22:45',
                comp: '星之守护者',
                placement: 1
            },
            {
                id: 2,
                date: '2024-04-12 21:30',
                comp: '机甲霸王龙',
                placement: 3
            },
            {
                id: 3,
                date: '2024-04-12 20:15',
                comp: '地下魔盗团',
                placement: 5
            },
            {
                id: 4,
                date: '2024-04-11 23:00',
                comp: '淘气包',
                placement: 2
            },
            {
                id: 5,
                date: '2024-04-11 21:45',
                comp: '黑客',
                placement: 6
            }
        ],
        cacheInfo: {
            statsCacheSize: '245 KB',
            staticCacheSize: '1.2 MB'
        }
    };

    document.addEventListener('DOMContentLoaded', () => {
        initTabs();
        initVersionSelector();
        initCacheManagement();
        initLanguageSelector();
        initThemeSelector();
        initI18n();
        loadMockData();
    });

    function initTabs() {
        const tabs = document.querySelectorAll('.tab');
        tabs.forEach(tab => {
            tab.addEventListener('click', () => {
                tabs.forEach(t => t.classList.remove('active'));
                document.querySelectorAll('.tab-panel')
                    .forEach(p => p.classList.remove('active'));

                tab.classList.add('active');
                document.getElementById(`panel-${tab.dataset.tab}`)
                    .classList.add('active');
            });
        });
    }

    function initVersionSelector() {
        const versionSelectors = document.querySelectorAll('.version-selector');
        versionSelectors.forEach(selector => {
            selector.addEventListener('change', (e) => {
                const version = e.target.value;
                console.log('Version selected:', version);
                versionSelectors.forEach(s => {
                    if (s !== selector) {
                        s.value = version;
                    }
                });
                alert(`已切换到版本 ${version}，数据将重新加载`);
            });
        });
    }

    function initCacheManagement() {
        const clearStatsBtn = document.getElementById('btn-clear-stats-cache');
        const clearStaticBtn = document.getElementById('btn-clear-static-cache');
        const clearAllBtn = document.getElementById('btn-clear-all-cache');

        clearStatsBtn?.addEventListener('click', () => {
            if (confirm('确定要清除统计数据缓存吗？')) {
                console.log('Clearing stats cache');
                alert('统计数据缓存已清除');
            }
        });

        clearStaticBtn?.addEventListener('click', () => {
            if (confirm('确定要清除静态数据缓存吗？')) {
                console.log('Clearing static cache');
                alert('静态数据缓存已清除');
            }
        });

        clearAllBtn?.addEventListener('click', () => {
            if (confirm('确定要清除所有缓存吗？这将删除所有本地缓存数据。')) {
                console.log('Clearing all cache');
                alert('所有缓存已清除');
            }
        });
    }

    function initLanguageSelector() {
        const languageSelector = document.getElementById('language-selector');
        if (languageSelector) {
            // 设置当前语言
            const savedLang = localStorage.getItem('language') || 'zh';
            languageSelector.value = savedLang;
            
            // 添加语言切换事件
            languageSelector.addEventListener('change', (e) => {
                const lang = e.target.value;
                setLanguage(lang);
            });
        }
    }

    function initThemeSelector() {
        const themeSelector = document.getElementById('theme-selector');
        if (themeSelector) {
            // 设置当前主题
            const savedTheme = localStorage.getItem('theme') || 'dark';
            themeSelector.value = savedTheme;
            setTheme(savedTheme);
            
            // 添加主题切换事件
            themeSelector.addEventListener('change', (e) => {
                const theme = e.target.value;
                setTheme(theme);
            });
        }
    }

    function setTheme(theme) {
        if (theme === 'light') {
            document.documentElement.classList.add('light-theme');
        } else {
            document.documentElement.classList.remove('light-theme');
        }
        localStorage.setItem('theme', theme);
    }

    function loadMockData() {
        updateOverallStats();
        updateCompStats();
        updateHistory();
        updateCacheInfo();
        initWinRateChart();
    }

    function updateOverallStats() {
        const stats = mockData.overallStats;
        const winRate = ((stats.wins / stats.totalMatches) * 100).toFixed(1);
        const top4Rate = ((stats.top4s / stats.totalMatches) * 100).toFixed(1);
        const top1Rate = ((stats.top1s / stats.totalMatches) * 100).toFixed(1);

        document.getElementById('total-matches').textContent = stats.totalMatches;
        document.getElementById('win-rate').textContent = `${winRate}%`;
        document.getElementById('top4-rate').textContent = `${top4Rate}%`;
        document.getElementById('top1-rate').textContent = `${top1Rate}%`;
    }

    function updateCompStats() {
        const container = document.getElementById('comp-stats-list');
        if (!container) return;

        container.innerHTML = '';

        mockData.compStats.forEach(comp => {
            const card = document.createElement('div');
            card.className = 'comp-stat-card';
            card.innerHTML = `
                <div class="comp-stat-header">
                    <span class="comp-stat-name">${comp.name}</span>
                    <div class="comp-stat-meta">
                        <span>${comp.matches} 局</span>
                        <span>胜率 ${comp.winRate.toFixed(1)}%</span>
                    </div>
                </div>
                <div class="comp-stat-progress">
                    <div class="progress-label">
                        <span>胜率</span>
                        <span>${comp.winRate.toFixed(1)}%</span>
                    </div>
                    <div class="progress-bar">
                        <div class="progress-fill" style="width: ${comp.winRate}%"></div>
                    </div>
                </div>
                <div class="comp-stat-progress" style="margin-top: 12px;">
                    <div class="progress-label">
                        <span>前四率</span>
                        <span>${comp.top4Rate.toFixed(1)}%</span>
                    </div>
                    <div class="progress-bar">
                        <div class="progress-fill" style="width: ${comp.top4Rate}%; background: var(--success);"></div>
                    </div>
                </div>
            `;
            container.appendChild(card);
        });
    }

    function updateHistory() {
        const container = document.getElementById('history-list');
        if (!container) return;

        container.innerHTML = '';

        mockData.history.forEach(match => {
            let placementClass = 'other';
            if (match.placement === 1) {
                placementClass = 'top1';
            } else if (match.placement <= 4) {
                placementClass = 'top4';
            }

            const item = document.createElement('div');
            item.className = 'history-item';
            item.innerHTML = `
                <div class="history-item-info">
                    <div class="history-date">${match.date}</div>
                    <div class="history-comp">${match.comp}</div>
                </div>
                <div class="history-result">
                    <span class="history-placement ${placementClass}">#${match.placement}</span>
                </div>
            `;
            container.appendChild(item);
        });
    }

    function updateCacheInfo() {
        const statsCacheEl = document.getElementById('stats-cache-size');
        const staticCacheEl = document.getElementById('static-cache-size');

        if (statsCacheEl) {
            statsCacheEl.textContent = mockData.cacheInfo.statsCacheSize;
        }
        if (staticCacheEl) {
            staticCacheEl.textContent = mockData.cacheInfo.staticCacheSize;
        }
    }

    // 胜率趋势图
    function initWinRateChart() {
        const ctx = document.getElementById('win-rate-chart');
        if (!ctx) return;

        const data = mockData.winRateTrend;
        const labels = data.map(item => item.date);
        const winRates = data.map(item => item.winRate);
        const top4Rates = data.map(item => item.top4Rate);

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: '胜率',
                        data: winRates,
                        borderColor: '#64b5f6',
                        backgroundColor: 'rgba(100, 181, 246, 0.1)',
                        tension: 0.3,
                        fill: true
                    },
                    {
                        label: '前四率',
                        data: top4Rates,
                        borderColor: '#4caf50',
                        backgroundColor: 'rgba(76, 175, 80, 0.1)',
                        tension: 0.3,
                        fill: true
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        labels: {
                            color: 'var(--text-primary)'
                        }
                    },
                    title: {
                        display: true,
                        text: '胜率趋势',
                        color: 'var(--text-primary)'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 100,
                        ticks: {
                            color: 'var(--text-secondary)',
                            callback: function(value) {
                                return value + '%';
                            }
                        },
                        grid: {
                            color: 'rgba(255, 255, 255, 0.1)'
                        }
                    },
                    x: {
                        ticks: {
                            color: 'var(--text-secondary)'
                        },
                        grid: {
                            color: 'rgba(255, 255, 255, 0.1)'
                        }
                    }
                }
            }
        });
    }

    // 阵容强度评估
    function initCompStrengthChart() {
        const ctx = document.getElementById('comp-strength-chart');
        if (!ctx) return;

        const comps = analyticsData.comps;
        const labels = comps.map(comp => comp.name);
        const winRates = comps.map(comp => comp.winRate);
        const top4Rates = comps.map(comp => comp.top4Rate);

        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: '胜率',
                        data: winRates,
                        backgroundColor: 'rgba(100, 181, 246, 0.7)',
                        borderColor: 'rgba(100, 181, 246, 1)',
                        borderWidth: 1
                    },
                    {
                        label: '前四率',
                        data: top4Rates,
                        backgroundColor: 'rgba(76, 175, 80, 0.7)',
                        borderColor: 'rgba(76, 175, 80, 1)',
                        borderWidth: 1
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        labels: {
                            color: 'var(--text-primary)'
                        }
                    },
                    title: {
                        display: true,
                        text: '阵容强度评估',
                        color: 'var(--text-primary)'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 100,
                        ticks: {
                            color: 'var(--text-secondary)',
                            callback: function(value) {
                                return value + '%';
                            }
                        },
                        grid: {
                            color: 'rgba(255, 255, 255, 0.1)'
                        }
                    },
                    x: {
                        ticks: {
                            color: 'var(--text-secondary)'
                        },
                        grid: {
                            color: 'rgba(255, 255, 255, 0.1)'
                        }
                    }
                }
            }
        });
    }

    // 版本Meta分析
    function initMetaAnalysisChart() {
        const ctx = document.getElementById('meta-analysis-chart');
        if (!ctx) return;

        const metaData = analyticsData.metaAnalysis;
        const labels = metaData.versions;
        const datasets = metaData.comps.map(comp => {
            return {
                label: comp.name,
                data: labels.map(version => comp[version]),
                borderColor: getRandomColor(),
                backgroundColor: 'rgba(0, 0, 0, 0)',
                tension: 0.3
            };
        });

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: datasets
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        labels: {
                            color: 'var(--text-primary)'
                        }
                    },
                    title: {
                        display: true,
                        text: '版本Meta分析',
                        color: 'var(--text-primary)'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 10,
                        ticks: {
                            color: 'var(--text-secondary)'
                        },
                        grid: {
                            color: 'rgba(255, 255, 255, 0.1)'
                        }
                    },
                    x: {
                        ticks: {
                            color: 'var(--text-secondary)'
                        },
                        grid: {
                            color: 'rgba(255, 255, 255, 0.1)'
                        }
                    }
                }
            }
        });
    }

    // 生成随机颜色
    function getRandomColor() {
        const letters = '0123456789ABCDEF';
        let color = '#';
        for (let i = 0; i < 6; i++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
        return color;
    }

    // 大数据分析相关
    let analyticsData = {
        overview: {
            totalMatches: 156,
            avgWinRate: 43.6,
            avgTop4Rate: 71.8,
            avgPlacement: 3.2
        },
        comps: [
            { name: '星之守护者', matches: 32, winRate: 56.3, top4Rate: 81.3, avgPlacement: 2.1, metaScore: 9.2 },
            { name: '机甲霸王龙', matches: 28, winRate: 50.0, top4Rate: 71.4, avgPlacement: 2.5, metaScore: 8.5 },
            { name: '地下魔盗团', matches: 24, winRate: 41.7, top4Rate: 66.7, avgPlacement: 3.1, metaScore: 7.8 },
            { name: '淘气包', matches: 22, winRate: 40.9, top4Rate: 68.2, avgPlacement: 3.2, metaScore: 7.5 },
            { name: '黑客', matches: 18, winRate: 38.9, top4Rate: 61.1, avgPlacement: 3.5, metaScore: 7.0 },
            { name: '未来战士', matches: 16, winRate: 37.5, top4Rate: 56.3, avgPlacement: 3.8, metaScore: 6.5 },
            { name: '源计划', matches: 10, winRate: 30.0, top4Rate: 50.0, avgPlacement: 4.2, metaScore: 6.0 },
            { name: '变异战士', matches: 8, winRate: 25.0, top4Rate: 37.5, avgPlacement: 4.5, metaScore: 5.5 }
        ],
        metaAnalysis: {
            versions: ['14.1', '14.2', '14.3'],
            comps: [
                { name: '星之守护者', '14.1': 7.5, '14.2': 8.2, '14.3': 9.2 },
                { name: '机甲霸王龙', '14.1': 8.0, '14.2': 8.5, '14.3': 8.5 },
                { name: '地下魔盗团', '14.1': 6.5, '14.2': 7.0, '14.3': 7.8 },
                { name: '淘气包', '14.1': 6.0, '14.2': 7.2, '14.3': 7.5 },
                { name: '黑客', '14.1': 5.5, '14.2': 6.5, '14.3': 7.0 }
            ]
        },
        equipment: {
            weapons: [
                { name: '巨人杀手', winRate: 62.5 },
                { name: '海克斯科技枪刃', winRate: 58.3 },
                { name: '无尽之刃', winRate: 56.7 }
            ],
            armor: [
                { name: '守护天使', winRate: 60.0 },
                { name: '荆棘之甲', winRate: 52.9 },
                { name: '龙爪', winRate: 48.5 }
            ],
            accessories: [
                { name: '正义之手', winRate: 57.1 },
                { name: '珠光护手', winRate: 54.2 },
                { name: '卢安娜的飓风', winRate: 51.7 }
            ]
        }
    };

    // 防抖函数
    function debounce(func, delay) {
        let timeoutId;
        return function() {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(() => func.apply(this, arguments), delay);
        };
    }

    function initAnalytics() {
        const filterSelector = document.getElementById('analytics-filter');
        const sortSelector = document.getElementById('analytics-sort');

        // 使用防抖处理，避免频繁触发
        const debouncedLoadData = debounce(loadAnalyticsData, 300);

        if (filterSelector) {
            filterSelector.addEventListener('change', debouncedLoadData);
        }

        if (sortSelector) {
            sortSelector.addEventListener('change', debouncedLoadData);
        }
    }

    function loadAnalyticsData() {
        const filter = document.getElementById('analytics-filter')?.value || 'all';
        const sortBy = document.getElementById('analytics-sort')?.value || 'winRate';
        
        const filteredData = filterAnalyticsData(filter);
        updateAnalyticsOverview(filteredData);
        updateAnalyticsTable(filteredData, sortBy);
        updateEquipmentAnalysis();
        initCompStrengthChart();
        initMetaAnalysisChart();
    }

    function filterAnalyticsData(filter) {
        // 这里可以根据不同的筛选条件返回不同的数据
        // 目前使用模拟数据，实际项目中需要根据真实数据进行筛选
        return {
            overview: analyticsData.overview,
            comps: analyticsData.comps
        };
    }

    function updateAnalyticsOverview(filteredData) {
        const overview = filteredData.overview;
        document.getElementById('analytics-total-matches').textContent = overview.totalMatches;
        document.getElementById('analytics-avg-win-rate').textContent = `${overview.avgWinRate.toFixed(1)}%`;
        document.getElementById('analytics-avg-top4-rate').textContent = `${overview.avgTop4Rate.toFixed(1)}%`;
        document.getElementById('analytics-avg-placement').textContent = overview.avgPlacement.toFixed(1);
    }

    function updateAnalyticsTable(filteredData, sortBy) {
        const tableBody = document.getElementById('analytics-table-body');
        if (!tableBody) return;

        const sortedComps = [...filteredData.comps].sort((a, b) => b[sortBy] - a[sortBy]);

        tableBody.innerHTML = '';

        sortedComps.forEach(comp => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${comp.name}</td>
                <td>${comp.matches}</td>
                <td>${comp.winRate.toFixed(1)}%</td>
                <td>${comp.top4Rate.toFixed(1)}%</td>
                <td>${comp.avgPlacement.toFixed(1)}</td>
            `;
            tableBody.appendChild(row);
        });
    }

    function updateEquipmentAnalysis() {
        const container = document.getElementById('equipment-analysis');
        if (!container) return;

        container.innerHTML = '';

        const equipmentTypes = [
            { key: 'weapons', label: '武器' },
            { key: 'armor', label: '防具' },
            { key: 'accessories', label: '配件' }
        ];

        equipmentTypes.forEach(type => {
            const card = document.createElement('div');
            card.className = 'equipment-card';
            card.innerHTML = `
                <h5>${type.label}胜率排行</h5>
                <div class="equipment-list">
                    ${analyticsData.equipment[type.key].map(item => `
                        <div class="equipment-item">
                            <span class="equipment-name">${item.name}</span>
                            <span class="equipment-win-rate">${item.winRate.toFixed(1)}%</span>
                        </div>
                    `).join('')}
                </div>
            `;
            container.appendChild(card);
        });
    }

    // 初始化所有功能
    document.addEventListener('DOMContentLoaded', () => {
        initTabs();
        initVersionSelector();
        initCacheManagement();
        initLanguageSelector();
        initThemeSelector();
        initI18n();
        loadMockData();
        initAnalytics();
    });
})();
