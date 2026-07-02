const i18n = {
    zh: {
        appTitle: 'TFT Assistant',
        tabs: {
            history: '历史记录',
            stats: '统计分析',
            analytics: '大数据分析',
            settings: '设置'
        },
        history: {
            title: '对局历史'
        },
        stats: {
            title: '总体胜率统计',
            totalMatches: '总对局数',
            winRate: '胜率',
            top4Rate: '前四率',
            top1Rate: '吃鸡率',
            compStats: '阵容胜率'
        },
        analytics: {
            title: '大数据分析',
            filter: '数据筛选',
            sort: '排序方式',
            filterOptions: {
                all: '全部数据',
                recent: '最近30天',
                season: '当前赛季'
            },
            sortOptions: {
                winRate: '胜率',
                matches: '对局数',
                top4Rate: '前四率'
            },
            overview: {
                totalMatches: '总对局数',
                avgWinRate: '平均胜率',
                avgTop4Rate: '平均前四率',
                avgPlacement: '平均排名'
            },
            table: {
                compName: '阵容名称',
                matches: '对局数',
                winRate: '胜率',
                top4Rate: '前四率',
                avgPlacement: '平均排名'
            },
            equipment: '装备分析'
        },
        settings: {
            title: '设置',
            dataManagement: '数据管理',
            gameVersion: '游戏版本',
            cacheManagement: '缓存管理',
            cacheItems: {
                stats: '统计数据缓存',
                static: '静态数据缓存'
            },
            cacheActions: {
                clearStats: '清除统计缓存',
                clearStatic: '清除静态数据缓存',
                clearAll: '清除所有缓存'
            },
            compliance: {
                title: '合规声明',
                text: '本工具遵守 Riot 第三方工具政策，不提供以下功能：',
                points: [
                    '对手棋盘侦察',
                    '实时动态操作指令',
                    '实时胜率预测',
                    '英雄池追踪'
                ],
                note: '所有统计数据为历史记录，仅供参考。'
            }
        },
        footer: 'TFT Assistant 历史数据分析工具',
        disclaimer: '以下数据为历史统计，仅供参考，不代表实时预测',
        version: '版本'
    },
    en: {
        appTitle: 'TFT Assistant',
        tabs: {
            history: 'History',
            stats: 'Statistics',
            analytics: 'Big Data',
            settings: 'Settings'
        },
        history: {
            title: 'Match History'
        },
        stats: {
            title: 'Overall Win Rate',
            totalMatches: 'Total Matches',
            winRate: 'Win Rate',
            top4Rate: 'Top 4 Rate',
            top1Rate: '1st Place Rate',
            compStats: 'Composition Win Rates'
        },
        analytics: {
            title: 'Big Data Analysis',
            filter: 'Data Filter',
            sort: 'Sort By',
            filterOptions: {
                all: 'All Data',
                recent: 'Last 30 Days',
                season: 'Current Season'
            },
            sortOptions: {
                winRate: 'Win Rate',
                matches: 'Matches',
                top4Rate: 'Top 4 Rate'
            },
            overview: {
                totalMatches: 'Total Matches',
                avgWinRate: 'Average Win Rate',
                avgTop4Rate: 'Average Top 4 Rate',
                avgPlacement: 'Average Placement'
            },
            table: {
                compName: 'Composition',
                matches: 'Matches',
                winRate: 'Win Rate',
                top4Rate: 'Top 4 Rate',
                avgPlacement: 'Avg Placement'
            },
            equipment: 'Equipment Analysis'
        },
        settings: {
            title: 'Settings',
            dataManagement: 'Data Management',
            gameVersion: 'Game Version',
            cacheManagement: 'Cache Management',
            cacheItems: {
                stats: 'Statistics Cache',
                static: 'Static Data Cache'
            },
            cacheActions: {
                clearStats: 'Clear Stats Cache',
                clearStatic: 'Clear Static Cache',
                clearAll: 'Clear All Cache'
            },
            compliance: {
                title: 'Compliance',
                text: 'This tool complies with Riots third-party tool policy and does not provide the following features:',
                points: [
                    'Opponent board scouting',
                    'Real-time dynamic operation instructions',
                    'Real-time win rate prediction',
                    'Hero pool tracking'
                ],
                note: 'All statistics are historical records for reference only.'
            }
        },
        footer: 'TFT Assistant History Analysis Tool',
        disclaimer: 'The following data are historical statistics for reference only, not real-time predictions',
        version: 'Version'
    }
};

let currentLang = 'zh';

function setLanguage(lang) {
    currentLang = lang;
    document.documentElement.lang = lang;
    updateUI();
    localStorage.setItem('language', lang);
}

function getTranslation(key) {
    const keys = key.split('.');
    let result = i18n[currentLang];
    for (const k of keys) {
        result = result?.[k];
    }
    return result || key;
}

function updateUI() {
    // 更新标题
    document.querySelector('.desktop-title').textContent = getTranslation('appTitle');
    
    // 更新标签页
    document.querySelector('[data-tab="history"]').textContent = getTranslation('tabs.history');
    document.querySelector('[data-tab="stats"]').textContent = getTranslation('tabs.stats');
    document.querySelector('[data-tab="analytics"]').textContent = getTranslation('tabs.analytics');
    document.querySelector('[data-tab="settings"]').textContent = getTranslation('tabs.settings');
    
    // 更新历史记录
    document.querySelector('#panel-history h3').textContent = getTranslation('history.title');
    
    // 更新统计分析
    document.querySelector('#panel-stats h3').textContent = getTranslation('stats.title');
    document.querySelector('#panel-stats .stat-label:nth-child(2)').textContent = getTranslation('stats.totalMatches');
    document.querySelector('#panel-stats .stat-label:nth-child(4)').textContent = getTranslation('stats.winRate');
    document.querySelector('#panel-stats .stat-label:nth-child(6)').textContent = getTranslation('stats.top4Rate');
    document.querySelector('#panel-stats .stat-label:nth-child(8)').textContent = getTranslation('stats.top1Rate');
    document.querySelector('#panel-stats h3:nth-child(3)').textContent = getTranslation('stats.compStats');
    
    // 更新大数据分析
    document.querySelector('#panel-analytics h3').textContent = getTranslation('analytics.title');
    document.querySelector('#panel-analytics .disclaimer-text').textContent = getTranslation('disclaimer');
    document.querySelector('#panel-analytics .control-group:nth-child(1) label').textContent = getTranslation('analytics.filter');
    document.querySelector('#panel-analytics .control-group:nth-child(2) label').textContent = getTranslation('analytics.sort');
    
    // 更新筛选选项
    const filterSelect = document.querySelector('#analytics-filter');
    filterSelect.options[0].text = getTranslation('analytics.filterOptions.all');
    filterSelect.options[1].text = getTranslation('analytics.filterOptions.recent');
    filterSelect.options[2].text = getTranslation('analytics.filterOptions.season');
    
    // 更新排序选项
    const sortSelect = document.querySelector('#analytics-sort');
    sortSelect.options[0].text = getTranslation('analytics.sortOptions.winRate');
    sortSelect.options[1].text = getTranslation('analytics.sortOptions.matches');
    sortSelect.options[2].text = getTranslation('analytics.sortOptions.top4Rate');
    
    // 更新概览
    document.querySelector('#panel-analytics .overview-label:nth-child(2)').textContent = getTranslation('analytics.overview.totalMatches');
    document.querySelector('#panel-analytics .overview-label:nth-child(4)').textContent = getTranslation('analytics.overview.avgWinRate');
    document.querySelector('#panel-analytics .overview-label:nth-child(6)').textContent = getTranslation('analytics.overview.avgTop4Rate');
    document.querySelector('#panel-analytics .overview-label:nth-child(8)').textContent = getTranslation('analytics.overview.avgPlacement');
    
    // 更新表格
    document.querySelector('#panel-analytics th:nth-child(1)').textContent = getTranslation('analytics.table.compName');
    document.querySelector('#panel-analytics th:nth-child(2)').textContent = getTranslation('analytics.table.matches');
    document.querySelector('#panel-analytics th:nth-child(3)').textContent = getTranslation('analytics.table.winRate');
    document.querySelector('#panel-analytics th:nth-child(4)').textContent = getTranslation('analytics.table.top4Rate');
    document.querySelector('#panel-analytics th:nth-child(5)').textContent = getTranslation('analytics.table.avgPlacement');
    
    // 更新装备分析
    document.querySelector('#panel-analytics h4:nth-child(6)').textContent = getTranslation('analytics.equipment');
    
    // 更新设置
    document.querySelector('#panel-settings h3').textContent = getTranslation('settings.title');
    document.querySelector('#panel-settings h4:nth-child(2)').textContent = getTranslation('settings.dataManagement');
    document.querySelector('#panel-settings .setting-item label').textContent = getTranslation('settings.gameVersion');
    document.querySelector('#panel-settings h4:nth-child(5)').textContent = getTranslation('settings.cacheManagement');
    document.querySelector('#panel-settings .cache-label:nth-child(1)').textContent = getTranslation('settings.cacheItems.stats');
    document.querySelector('#panel-settings .cache-label:nth-child(3)').textContent = getTranslation('settings.cacheItems.static');
    document.querySelector('#btn-clear-stats-cache').textContent = getTranslation('settings.cacheActions.clearStats');
    document.querySelector('#btn-clear-static-cache').textContent = getTranslation('settings.cacheActions.clearStatic');
    document.querySelector('#btn-clear-all-cache').textContent = getTranslation('settings.cacheActions.clearAll');
    document.querySelector('#panel-settings h4:nth-child(8)').textContent = getTranslation('settings.compliance.title');
    document.querySelector('#panel-settings .compliance-text p:nth-child(1)').textContent = getTranslation('settings.compliance.text');
    document.querySelector('#panel-settings .compliance-text li:nth-child(1)').textContent = getTranslation('settings.compliance.points.0');
    document.querySelector('#panel-settings .compliance-text li:nth-child(2)').textContent = getTranslation('settings.compliance.points.1');
    document.querySelector('#panel-settings .compliance-text li:nth-child(3)').textContent = getTranslation('settings.compliance.points.2');
    document.querySelector('#panel-settings .compliance-text li:nth-child(4)').textContent = getTranslation('settings.compliance.points.3');
    document.querySelector('#panel-settings .compliance-text p:nth-child(3)').textContent = getTranslation('settings.compliance.note');
    
    // 更新页脚
    document.querySelector('.footer-text').textContent = getTranslation('footer');
}

// 初始化语言
function initI18n() {
    const savedLang = localStorage.getItem('language') || 'zh';
    setLanguage(savedLang);
}

// 导出函数
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { i18n, setLanguage, getTranslation, initI18n };
}
