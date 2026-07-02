const i18n = {
    zh: {
        appTitle: 'TFT Assistant',
        tabs: {
            board: '棋盘',
            items: '装备',
            comps: '阵容',
            econ: '经济',
            analytics: '大数据'
        },
        board: {
            title: '棋盘追踪'
        },
        items: {
            title: '装备分析',
            recommendations: '装备推荐',
            synthesis: '合成路径'
        },
        comps: {
            title: '阵容建议',
            top: '推荐阵容'
        },
        econ: {
            title: '经济提示',
            hints: {
                early: '前期运营',
                earlyDesc: '尽量保持 50 金吃利息'
            }
        },
        analytics: {
            title: '大数据分析',
            overview: {
                winRate: '平均胜率',
                top4Rate: '前四率',
                popularComp: '热门阵容'
            },
            compRanking: '阵容胜率排行'
        },
        disclaimer: '以下数据为历史统计，仅供参考',
        status: {
            connected: '已连接',
            disconnected: '未连接'
        }
    },
    en: {
        appTitle: 'TFT Assistant',
        tabs: {
            board: 'Board',
            items: 'Items',
            comps: 'Comps',
            econ: 'Economy',
            analytics: 'Analytics'
        },
        board: {
            title: 'Board Tracking'
        },
        items: {
            title: 'Equipment Analysis',
            recommendations: 'Item Recommendations',
            synthesis: 'Synthesis Paths'
        },
        comps: {
            title: 'Composition Suggestions',
            top: 'Recommended Composition'
        },
        econ: {
            title: 'Economy Tips',
            hints: {
                early: 'Early Game',
                earlyDesc: 'Try to maintain 50 gold for interest'
            }
        },
        analytics: {
            title: 'Big Data Analysis',
            overview: {
                winRate: 'Average Win Rate',
                top4Rate: 'Top 4 Rate',
                popularComp: 'Popular Composition'
            },
            compRanking: 'Composition Win Rate Ranking'
        },
        disclaimer: 'The following data are historical statistics for reference only',
        status: {
            connected: 'Connected',
            disconnected: 'Disconnected'
        }
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
    document.querySelector('.overlay-title').textContent = getTranslation('appTitle');
    
    // 更新标签页
    document.querySelector('[data-tab="board"]').textContent = getTranslation('tabs.board');
    document.querySelector('[data-tab="items"]').textContent = getTranslation('tabs.items');
    document.querySelector('[data-tab="comps"]').textContent = getTranslation('tabs.comps');
    document.querySelector('[data-tab="econ"]').textContent = getTranslation('tabs.econ');
    document.querySelector('[data-tab="analytics"]').textContent = getTranslation('tabs.analytics');
    
    // 更新棋盘
    document.querySelector('#panel-board h3').textContent = getTranslation('board.title');
    
    // 更新装备
    document.querySelector('#panel-items h3').textContent = getTranslation('items.title');
    document.querySelector('#panel-items .disclaimer-text').textContent = getTranslation('disclaimer');
    document.querySelector('#panel-items h4:nth-child(3)').textContent = getTranslation('items.recommendations');
    document.querySelector('#panel-items h4:nth-child(5)').textContent = getTranslation('items.synthesis');
    
    // 更新阵容
    document.querySelector('#panel-comps h3').textContent = getTranslation('comps.title');
    document.querySelector('.comp-name').textContent = getTranslation('comps.top');
    
    // 更新经济
    document.querySelector('#panel-econ h3').textContent = getTranslation('econ.title');
    document.querySelector('.econ-title').textContent = getTranslation('econ.hints.early');
    document.querySelector('.econ-description').textContent = getTranslation('econ.hints.earlyDesc');
    
    // 更新大数据
    document.querySelector('#panel-analytics h3').textContent = getTranslation('analytics.title');
    document.querySelector('#panel-analytics .disclaimer-text').textContent = getTranslation('disclaimer');
    document.querySelector('#panel-analytics .analytics-label:nth-child(2)').textContent = getTranslation('analytics.overview.winRate');
    document.querySelector('#panel-analytics .analytics-label:nth-child(4)').textContent = getTranslation('analytics.overview.top4Rate');
    document.querySelector('#panel-analytics .analytics-label:nth-child(6)').textContent = getTranslation('analytics.overview.popularComp');
    document.querySelector('#panel-analytics h4:nth-child(4)').textContent = getTranslation('analytics.compRanking');
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
