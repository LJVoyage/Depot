// ================================================================
//  1. Unity 项目虚拟文件树
// ================================================================
const RESOURCE_TYPES = ['prefabs', 'UI', 'materials', 'scripts', 'shaders', 'animators', 'models'];

const initialData = {
    'assets': {
        type: 'folder',
        children: {
            'scenes': {
                type: 'folder',
                children: {
                    'A.scene': { type: 'file', size: '12 MB', date: '2026-08-14 10:00' },
                    'sample': {
                        type: 'folder',
                        children: {
                            'b.scene': { type: 'file', size: '8 MB', date: '2026-08-13 15:30' }
                        }
                    },
                    'C.scene': { type: 'file', size: '6 MB', date: '2026-08-12 09:20' }
                }
            },
            'prefabs': {
                type: 'folder',
                children: {
                    'A': {
                        type: 'folder',
                        children: {
                            '1.prefab': { type: 'file', size: '2 MB', date: '2026-08-12 09:00' },
                            'sub': {
                                type: 'folder',
                                children: {
                                    '2.prefab': { type: 'file', size: '1 MB', date: '2026-08-11 14:20' }
                                }
                            }
                        }
                    },
                    'sample': {
                        type: 'folder',
                        children: {
                            'b': {
                                type: 'folder',
                                children: {
                                    '1.prefab': { type: 'file', size: '1.5 MB', date: '2026-08-10 11:10' }
                                }
                            }
                        }
                    }
                }
            },
            'UI': {
                type: 'folder',
                children: {
                    'A': {
                        type: 'folder',
                        children: {
                            'login.png': { type: 'file', size: '10 KB', date: '2026-08-10 12:00' },
                            'menu.png': { type: 'file', size: '8 KB', date: '2026-08-09 09:30' }
                        }
                    },
                    'sample': {
                        type: 'folder',
                        children: {
                            'b': {
                                type: 'folder',
                                children: {
                                    'login.png': { type: 'file', size: '7 KB', date: '2026-08-08 14:20' }
                                }
                            }
                        }
                    }
                }
            },
            'materials': {
                type: 'folder',
                children: {
                    'A': {
                        type: 'folder',
                        children: {
                            'red.mat': { type: 'file', size: '1 KB', date: '2026-08-09 10:00' },
                            'blue.mat': { type: 'file', size: '1 KB', date: '2026-08-08 11:00' }
                        }
                    }
                }
            },
            'scripts': {
                type: 'folder',
                children: {
                    'A': {
                        type: 'folder',
                        children: {
                            'PlayerController.cs': { type: 'file', size: '4 KB', date: '2026-08-14 08:30' },
                            'GameManager.cs': { type: 'file', size: '6 KB', date: '2026-08-13 17:00' }
                        }
                    }
                }
            },
            'shaders': {
                type: 'folder',
                children: {
                    'A': {
                        type: 'folder',
                        children: {
                            'Standard.shader': { type: 'file', size: '2 KB', date: '2026-08-11 09:00' }
                        }
                    }
                }
            },
            'animators': {
                type: 'folder',
                children: {
                    'A': {
                        type: 'folder',
                        children: {
                            'Idle.controller': { type: 'file', size: '5 KB', date: '2026-08-06 14:00' },
                            'Run.controller': { type: 'file', size: '6 KB', date: '2026-08-05 15:00' }
                        }
                    }
                }
            },
            'models': {
                type: 'folder',
                children: {
                    'A': {
                        type: 'folder',
                        children: {
                            'Player.fbx': { type: 'file', size: '15 MB', date: '2026-08-04 09:00' }
                        }
                    }
                }
            }
        }
    }
};

let fileSystem = JSON.parse(JSON.stringify(initialData));

// ================================================================
//  2. 标签管理 & 模式
// ================================================================
let tabs = [];
let activeTabId = null;
let tabIdCounter = 0;
let isMappingMode = false;

function getActiveTab() {
    return tabs.find(t => t.id === activeTabId) || null;
}

function getActivePath() {
    const tab = getActiveTab();
    return tab ? tab.path : [];
}

function createTab(path, switchTo = true) {
    const id = ++tabIdCounter;
    const tab = {
        id,
        path: [...path],
        history: [[...path]],
        historyIndex: 0
    };
    tabs.push(tab);
    if (switchTo) {
        activeTabId = id;
    }
    renderTabs();
    if (switchTo) {
        renderAllForActiveTab();
        updateNavButtonsForActiveTab();
    }
    return id;
}

function closeTab(tabId) {
    if (tabs.length <= 1) return;
    const index = tabs.findIndex(t => t.id === tabId);
    if (index === -1) return;
    tabs.splice(index, 1);
    if (activeTabId === tabId) {
        const newIndex = Math.min(index, tabs.length - 1);
        activeTabId = tabs[newIndex].id;
    }
    renderTabs();
    renderAllForActiveTab();
    updateNavButtonsForActiveTab();
}

function switchTab(tabId) {
    if (activeTabId === tabId) return;
    const tab = tabs.find(t => t.id === tabId);
    if (!tab) return;
    activeTabId = tabId;
    renderTabs();
    renderAllForActiveTab();
    updateNavButtonsForActiveTab();
}

function updateActiveTabPath(path) {
    const tab = getActiveTab();
    if (!tab) return;
    tab.path = [...path];
    tab.history = tab.history.slice(0, tab.historyIndex + 1);
    tab.history.push([...path]);
    tab.historyIndex = tab.history.length - 1;
    renderTabs();
    renderAllForActiveTab();
    updateNavButtonsForActiveTab();
}

function goBackActiveTab() {
    const tab = getActiveTab();
    if (!tab || tab.historyIndex <= 0) return;
    tab.historyIndex--;
    tab.path = [...tab.history[tab.historyIndex]];
    renderTabs();
    renderAllForActiveTab();
    updateNavButtonsForActiveTab();
}

function goUpActiveTab() {
    const tab = getActiveTab();
    if (!tab) return;
    if (tab.path.length <= 1) return;
    const newPath = tab.path.slice(0, -1);
    tab.path = [...newPath];
    tab.history = tab.history.slice(0, tab.historyIndex + 1);
    tab.history.push([...newPath]);
    tab.historyIndex = tab.history.length - 1;
    renderTabs();
    renderAllForActiveTab();
    updateNavButtonsForActiveTab();
}

// ================================================================
//  3. 工具函数 (操作文件树)
// ================================================================

// --- 普通模式 ---
function getNodeByPathReal(path) {
    if (!path || path.length === 0) return null;
    let node = fileSystem;
    for (let i = 0; i < path.length; i++) {
        const seg = path[i];
        if (i === 0) {
            if (node && node[seg]) node = node[seg];
            else return null;
        } else {
            if (node && node.children && node.children[seg]) node = node.children[seg];
            else return null;
        }
    }
    return node;
}

function getCurrentChildrenReal() {
    const node = getNodeByPathReal(getActivePath());
    if (!node || node.type !== 'folder') return [];
    const children = node.children || {};
    return Object.keys(children).map(name => ({ name, ...children[name] }));
}

// --- 映射模式辅助 ---

// 获取所有场景标识 (相对于 scenes 的路径，去掉 .unity)
function getSceneIdentifiers() {
    const scenesNode = fileSystem.assets.children.scenes;
    if (!scenesNode) {
        console.warn('[getSceneIdentifiers] scenesNode not found');
        return [];
    }
    console.log('[getSceneIdentifiers] scenesNode:', scenesNode);
    console.log('[getSceneIdentifiers] scenesNode.children:', scenesNode.children);

    const result = [];
    // 递归遍历 scenes 节点，收集所有 .unity 文件路径（相对于 scenes）
    function collect(node, currentPath) {
        if (!node || node.type !== 'folder') {
            console.warn('[collect] Skipping non-folder node:', node);
            return;
        }
        const children = node.children || {};
        console.log(`[collect] currentPath: "${currentPath}", children keys:`, Object.keys(children));
        for (const [name, child] of Object.entries(children)) {
            const newPath = currentPath.length ? currentPath + '/' + name : name;
            if (child.type === 'file' && name.endsWith('.scene')) {
                const id = newPath.slice(0, -6); // 去掉 .unity
                result.push(id);
                console.log(`[getSceneIdentifiers] Found scene: ${id} (from ${newPath})`);
            } else if (child.type === 'folder') {
                collect(child, newPath);
            }
        }
    }
    collect(scenesNode, '');
    console.log(`[getSceneIdentifiers] Total scenes: ${result.length}`, result);
    return result;
}

// 在节点下查找路径
function findNodeByPath(node, pathArray) {
    if (!node) return null;
    let current = node;
    for (const seg of pathArray) {
        if (current && current.children && current.children[seg]) {
            current = current.children[seg];
        } else {
            return null;
        }
    }
    return current;
}

// 获取场景虚拟文件夹下的内容
function getSceneVirtualItems(sceneId) {
    console.log(`[getSceneVirtualItems] sceneId: ${sceneId}`);
    const items = [];
    // 1. 场景文件
    const scenePath = sceneId + '.scene';
    const scenesNode = fileSystem.assets.children.scenes;
    const pathParts = sceneId.split('/');
    let parent = scenesNode;
    for (const part of pathParts) {
        if (parent && parent.children && parent.children[part]) {
            parent = parent.children[part];
        } else {
            parent = null;
            break;
        }
    }
    if (parent && parent.type === 'folder' && parent.children && parent.children[scenePath]) {
        const fileNode = parent.children[scenePath];
        items.push({ name: scenePath, type: 'file', size: fileNode.size, date: fileNode.date });
        console.log(`[getSceneVirtualItems] Added scene file: ${scenePath}`);
    }

    // 2. 资源子文件夹
    for (const type of RESOURCE_TYPES) {
        const typeNode = fileSystem.assets.children[type];
        if (!typeNode) continue;
        const subNode = findNodeByPath(typeNode, sceneId.split('/'));
        if (subNode && subNode.type === 'folder') {
            items.push({ name: type, type: 'folder', _realPath: ['assets', type, sceneId] });
            console.log(`[getSceneVirtualItems] Added resource folder: ${type}`);
        }
    }
    console.log(`[getSceneVirtualItems] Total items: ${items.length}`, items);
    return items;
}

// 获取虚拟路径下的子项
function getVirtualChildren(virtualPath) {
    console.log(`[getVirtualChildren] Called with path:`, virtualPath, `mode: ${isMappingMode ? 'MAPPING' : 'NORMAL'}`);
    if (virtualPath.length === 1 && virtualPath[0] === 'assets') {
        const sceneIds = getSceneIdentifiers();
        const result = sceneIds.map(id => ({ name: id, type: 'folder', _sceneId: id }));
        console.log(`[getVirtualChildren] Returning scene folders:`, result);
        return result;
    } else if (virtualPath.length === 2) {
        const sceneId = virtualPath[1];
        const items = getSceneVirtualItems(sceneId);
        console.log(`[getVirtualChildren] Returning items for scene ${sceneId}:`, items);
        return items;
    } else if (virtualPath.length >= 3) {
        const sceneId = virtualPath[1];
        const resourceType = virtualPath[2];
        const rest = virtualPath.slice(3);
        const realPath = ['assets', resourceType, sceneId, ...rest];
        const realNode = getNodeByPathReal(realPath);
        if (realNode && realNode.type === 'folder') {
            const children = realNode.children || {};
            const result = Object.keys(children).map(name => ({ name, ...children[name] }));
            console.log(`[getVirtualChildren] Returning real children for ${realPath.join('/')}:`, result);
            return result;
        }
        console.warn(`[getVirtualChildren] No real node found for path:`, realPath);
        return [];
    }
    console.warn(`[getVirtualChildren] Unhandled path:`, virtualPath);
    return [];
}

// 获取当前激活路径下的子项 (根据模式)
function getCurrentChildren() {
    const path = getActivePath();
    console.log(`[getCurrentChildren] Path:`, path, `Mode: ${isMappingMode ? 'MAPPING' : 'NORMAL'}`);
    if (isMappingMode) {
        const result = getVirtualChildren(path);
        console.log(`[getCurrentChildren] Mapping result: ${result.length} items`, result);
        return result;
    } else {
        const result = getCurrentChildrenReal();
        console.log(`[getCurrentChildren] Real result: ${result.length} items`, result);
        return result;
    }
}

// 获取节点 (根据模式)
function getNodeByPath(path) {
    console.log(`[getNodeByPath] Checking path:`, path, `Mode: ${isMappingMode ? 'MAPPING' : 'NORMAL'}`);
    if (!path || path.length === 0) return null;
    if (isMappingMode) {
        if (path.length === 1 && path[0] === 'assets') {
            console.log('[getNodeByPath] Returning virtual assets folder');
            return { type: 'folder' };
        } else if (path.length === 2) {
            const sceneId = path[1];
            const sceneIds = getSceneIdentifiers();
            if (sceneIds.includes(sceneId)) {
                console.log(`[getNodeByPath] Returning virtual scene folder: ${sceneId}`);
                return { type: 'folder' };
            }
            console.warn(`[getNodeByPath] Scene not found: ${sceneId}`);
            return null;
        } else if (path.length >= 3) {
            const sceneId = path[1];
            const resourceType = path[2];
            const rest = path.slice(3);
            const realPath = ['assets', resourceType, sceneId, ...rest];
            const realNode = getNodeByPathReal(realPath);
            if (realNode) {
                console.log(`[getNodeByPath] Returning real node for ${realPath.join('/')}`);
                return realNode;
            }
            console.warn(`[getNodeByPath] No real node for ${realPath.join('/')}`);
            return null;
        }
        return null;
    } else {
        const node = getNodeByPathReal(path);
        console.log(`[getNodeByPath] Real node:`, node);
        return node;
    }
}

// ================================================================
//  4. 渲染函数 (针对激活标签)
// ================================================================

function renderTabs() {
    const container = document.getElementById('tabBar');
    let html = '';
    for (const tab of tabs) {
        const active = (tab.id === activeTabId) ? 'active' : '';
        const label = tab.path.length > 0 ? tab.path[tab.path.length - 1] : 'assets';
        const icon = '📁';
        html += `
            <div class="tab-item ${active}" data-tab-id="${tab.id}">
                <span class="tab-icon">${icon}</span>
                <span>${label}</span>
                <span class="tab-close" data-tab-id="${tab.id}">✕</span>
            </div>
        `;
    }
    container.innerHTML = html;

    container.querySelectorAll('.tab-item').forEach(el => {
        const tabId = parseInt(el.dataset.tabId, 10);
        el.addEventListener('click', function(e) {
            if (e.target.closest('.tab-close')) return;
            switchTab(tabId);
        });
        const closeBtn = el.querySelector('.tab-close');
        if (closeBtn) {
            closeBtn.addEventListener('click', function(e) {
                e.stopPropagation();
                closeTab(tabId);
            });
        }
    });
}

function renderSidebar() {
    const container = document.getElementById('sidebarItems');
    const path = getActivePath();
    let html = '';
    const titleEl = document.getElementById('sidebarTitle');
    if (isMappingMode) {
        console.log('[renderSidebar] MAPPING mode');
        titleEl.textContent = '📌 场景';
        const sceneIds = getSceneIdentifiers();
        for (const id of sceneIds) {
            const active = (path.length >= 2 && path[1] === id) ? 'active' : '';
            html += `
                <div class="nav-item ${active}" data-scene="${id}">
                    <span class="icon">📁</span>
                    ${id}
                </div>
            `;
        }
        container.innerHTML = html;
        container.querySelectorAll('.nav-item').forEach(el => {
            el.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                const scene = this.dataset.scene;
                if (scene) {
                    console.log(`[Sidebar] Clicked scene: ${scene}`);
                    navigateActiveTabToPath(['assets', scene]);
                }
            });
        });
    } else {
        console.log('[renderSidebar] NORMAL mode');
        titleEl.textContent = '📌 快速访问';
        const assetsNode = fileSystem.assets;
        if (assetsNode && assetsNode.children) {
            const folders = Object.keys(assetsNode.children);
            for (const name of folders) {
                const active = (path.length >= 2 && path[1] === name) ? 'active' : '';
                const node = assetsNode.children[name];
                const count = node && node.children ? Object.keys(node.children).length : 0;
                html += `
                    <div class="nav-item ${active}" data-folder="${name}">
                        <span class="icon">📁</span>
                        ${name}
                        <span class="badge">${count}</span>
                    </div>
                `;
            }
            container.innerHTML = html;
            container.querySelectorAll('.nav-item').forEach(el => {
                el.addEventListener('click', function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    const folder = this.dataset.folder;
                    if (folder) {
                        console.log(`[Sidebar] Clicked folder: ${folder}`);
                        navigateActiveTabToPath(['assets', folder]);
                    }
                });
            });
        }
    }
}

function renderPath() {
    const container = document.getElementById('pathSegments');
    const path = getActivePath();
    let html = '';
    if (path.length === 0 || (path.length === 1 && path[0] === 'assets')) {
        html = `<span class="seg root">./assets</span>`;
    } else {
        const parts = [];
        if (isMappingMode) {
            parts.push('./assets');
            for (let i = 1; i < path.length; i++) {
                parts.push(path[i]);
            }
        } else {
            parts.push('./assets');
            for (let i = 1; i < path.length; i++) {
                parts.push(path[i]);
            }
        }
        for (let i = 0; i < parts.length; i++) {
            if (i > 0) html += `<span class="sep"> › </span>`;
            const isLast = (i === parts.length - 1);
            const cls = isLast ? 'seg last' : 'seg';
            const dataIndex = i - 1;
            html += `<span class="${cls}" data-index="${dataIndex}">${parts[i]}</span>`;
        }
    }
    container.innerHTML = html;

    container.querySelectorAll('.seg:not(.last)').forEach(el => {
        el.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            const idx = parseInt(this.dataset.index, 10);
            if (!isNaN(idx) && idx < path.length) {
                const newPath = path.slice(0, idx + 1);
                navigateActiveTabToPath(newPath);
            } else if (this.classList.contains('root') && path.length > 0) {
                navigateActiveTabToPath(['assets']);
            }
        });
    });
}

function renderFileList() {
    const container = document.getElementById('fileListContent');
    const children = getCurrentChildren();
    console.log(`[renderFileList] ${children.length} children to render`, children);

    if (children.length === 0) {
        container.innerHTML = `
            <div class="empty-folder">
                <div class="big-icon">📭</div>
                <div>此文件夹为空</div>
            </div>
        `;
        updateStatus(0);
        return;
    }

    children.sort((a, b) => {
        if (a.type === 'folder' && b.type !== 'folder') return -1;
        if (a.type !== 'folder' && b.type === 'folder') return 1;
        return a.name.localeCompare(b.name);
    });

    let gridHtml = '<div class="file-grid">';
    for (const item of children) {
        const icon = getFileIcon(item);
        const isFolder = item.type === 'folder';
        const nameClass = isFolder ? 'file-name folder-name' : 'file-name';
        const itemClass = isFolder ? 'file-item folder-item' : 'file-item';
        gridHtml += `
            <div class="${itemClass}" data-name="${item.name}" data-type="${item.type}">
                <div class="file-icon">${icon}</div>
                <div class="${nameClass}">${item.name}</div>
            </div>
        `;
    }
    gridHtml += '</div>';

    let listHtml = '<div class="file-list-view">';
    for (const item of children) {
        const icon = getFileIcon(item);
        const isFolder = item.type === 'folder';
        const rowClass = isFolder ? 'list-row folder-row' : 'list-row';
        const size = isFolder ? '—' : (item.size || '—');
        const date = item.date || '—';
        listHtml += `
            <div class="${rowClass}" data-name="${item.name}" data-type="${item.type}">
                <span class="col-name"><span class="icon">${icon}</span> ${item.name}</span>
                <span class="col-date">${date}</span>
                <span class="col-size">${size}</span>
            </div>
        `;
    }
    listHtml += '</div>';

    container.innerHTML = gridHtml + listHtml;

    // 双击文件夹
    container.querySelectorAll('.folder-item, .folder-row').forEach(el => {
        el.addEventListener('dblclick', function(e) {
            e.preventDefault();
            e.stopPropagation();
            const name = this.dataset.name;
            if (name) {
                console.log(`[dblclick] Entering: ${name}`);
                enterFolderInActiveTab(name);
            }
        });
    });

    // 右键菜单
    container.querySelectorAll('.folder-item, .folder-row').forEach(el => {
        el.addEventListener('contextmenu', function(e) {
            e.preventDefault();
            e.stopPropagation();
            const name = this.dataset.name;
            if (!name) return;
            showContextMenu(e.clientX, e.clientY, name);
        });
    });

    container.addEventListener('contextmenu', function(e) {
        const target = e.target;
        if (target === container || target.closest('.empty-folder') ||
            target.closest('.file-grid') || target.closest('.file-list-view')) {
            e.preventDefault();
            showContextMenu(e.clientX, e.clientY, null);
        }
    });

    container.addEventListener('selectstart', function(e) {
        e.preventDefault();
    });

    updateStatus(children.length);
}

function getFileIcon(item) {
    if (item.type === 'folder') return '📁';
    const name = item.name.toLowerCase();
    if (name.endsWith('.unity')) return '🎮';
    if (name.endsWith('.prefab')) return '🧩';
    if (name.endsWith('.cs')) return '📄';
    if (name.endsWith('.shader')) return '🟣';
    if (name.endsWith('.mat')) return '🔵';
    if (name.endsWith('.controller')) return '🎬';
    if (name.endsWith('.fbx')) return '🗿';
    if (name.endsWith('.meta')) return '⚙';
    if (name.endsWith('.png') || name.endsWith('.jpg')) return '🖼';
    return '📄';
}

function formatPathForDisplay(path) {
    if (!path || path.length === 0) return './assets';
    if (path[0] === 'assets') {
        return './' + path.join('/');
    }
    return path.join('/');
}

function updateStatus(count) {
    const path = getActivePath();
    document.getElementById('statusCount').textContent = `📂 ${count} 个项目`;
    document.getElementById('statusSelected').textContent = `📊 已选中 0 个`;
    document.getElementById('statusPath').textContent = `📁 ${formatPathForDisplay(path)}`;
}

function updateNavButtonsForActiveTab() {
    const tab = getActiveTab();
    if (!tab) {
        document.getElementById('btnBack').disabled = true;
        document.getElementById('btnUp').disabled = true;
        return;
    }
    document.getElementById('btnBack').disabled = (tab.historyIndex <= 0);
    document.getElementById('btnUp').disabled = (tab.path.length <= 1);
}

// ================================================================
//  5. 导航函数 (针对激活标签)
// ================================================================

function enterFolderInActiveTab(name) {
    const currentPath = getActivePath();
    console.log(`[enterFolderInActiveTab] name: ${name}, currentPath:`, currentPath);
    if (isMappingMode) {
        if (currentPath.length === 1 && currentPath[0] === 'assets') {
            const sceneIds = getSceneIdentifiers();
            if (sceneIds.includes(name)) {
                navigateActiveTabToPath(['assets', name]);
            }
            return;
        }
        if (currentPath.length === 2) {
            const sceneId = currentPath[1];
            if (RESOURCE_TYPES.includes(name)) {
                navigateActiveTabToPath(['assets', sceneId, name]);
                return;
            }
            return;
        }
        if (currentPath.length >= 3) {
            const newPath = [...currentPath, name];
            const node = getNodeByPath(newPath);
            if (node && node.type === 'folder') {
                navigateActiveTabToPath(newPath);
            }
            return;
        }
    } else {
        const newPath = [...currentPath, name];
        const node = getNodeByPath(newPath);
        if (node && node.type === 'folder') {
            navigateActiveTabToPath(newPath);
        }
    }
}

function navigateActiveTabToPath(path) {
    console.log(`[navigateActiveTabToPath] Trying to navigate to:`, path);
    const node = getNodeByPath(path);
    if (!node || node.type !== 'folder') {
        console.warn('[navigateActiveTabToPath] Invalid path:', path);
        return;
    }
    const tab = getActiveTab();
    if (!tab) return;
    tab.path = [...path];
    tab.history = tab.history.slice(0, tab.historyIndex + 1);
    tab.history.push([...path]);
    tab.historyIndex = tab.history.length - 1;
    console.log(`[navigateActiveTabToPath] Navigated to:`, tab.path);
    renderTabs();
    renderAllForActiveTab();
    updateNavButtonsForActiveTab();
}

// ================================================================
//  6. 右键菜单
// ================================================================

let contextMenuTarget = null;

function showContextMenu(x, y, targetFolder) {
    const menu = document.getElementById('contextMenu');
    contextMenuTarget = targetFolder;

    const openTabItem = menu.querySelector('[data-action="open-tab"]');
    if (targetFolder) {
        openTabItem.style.display = 'flex';
    } else {
        openTabItem.style.display = 'none';
    }

    const newFileItem = menu.querySelector('[data-action="new-file"]');
    const newFolderItem = menu.querySelector('[data-action="new-folder"]');
    if (isMappingMode) {
        newFileItem.style.opacity = '0.5';
        newFileItem.style.cursor = 'not-allowed';
        newFolderItem.style.opacity = '0.5';
        newFolderItem.style.cursor = 'not-allowed';
    } else {
        newFileItem.style.opacity = '1';
        newFileItem.style.cursor = 'pointer';
        newFolderItem.style.opacity = '1';
        newFolderItem.style.cursor = 'pointer';
    }

    const winW = window.innerWidth;
    const winH = window.innerHeight;
    const menuW = 220;
    const menuH = 160;
    let left = Math.min(x, winW - menuW - 10);
    let top = Math.min(y, winH - menuH - 10);
    left = Math.max(10, left);
    top = Math.max(10, top);

    menu.style.left = left + 'px';
    menu.style.top = top + 'px';
    menu.style.display = 'block';

    setTimeout(() => {
        document.addEventListener('click', closeContextMenu, { once: true });
        document.addEventListener('contextmenu', closeContextMenu, { once: true });
    }, 10);
}

function closeContextMenu() {
    document.getElementById('contextMenu').style.display = 'none';
    contextMenuTarget = null;
}

document.getElementById('contextMenu').addEventListener('click', function(e) {
    const item = e.target.closest('.menu-item');
    if (!item) return;
    const action = item.dataset.action;
    if (!action) return;
    e.stopPropagation();

    if (isMappingMode && (action === 'new-file' || action === 'new-folder')) {
        alert('映射模式下不支持创建文件/文件夹');
        closeContextMenu();
        return;
    }

    switch (action) {
        case 'new-file': handleNewFile(); break;
        case 'new-folder': handleNewFolder(); break;
        case 'open-tab': handleOpenTab(); break;
    }
    closeContextMenu();
});

// ================================================================
//  7. 操作函数 (仅在普通模式可用)
// ================================================================

function handleNewFile() {
    const node = getNodeByPath(getActivePath());
    if (!node || node.type !== 'folder') return;
    const name = prompt('请输入文件名:');
    if (!name || name.trim() === '') return;
    const clean = name.trim();
    if (node.children && node.children[clean]) {
        alert('已存在同名文件/文件夹');
        return;
    }
    node.children[clean] = {
        type: 'file',
        size: '0 KB',
        date: new Date().toLocaleString('zh-CN', { hour12: false }).replace(/\//g, '-')
    };
    renderFileList();
    renderSidebar();
}

function handleNewFolder() {
    const node = getNodeByPath(getActivePath());
    if (!node || node.type !== 'folder') return;
    const name = prompt('请输入文件夹名:');
    if (!name || name.trim() === '') return;
    const clean = name.trim();
    if (node.children && node.children[clean]) {
        alert('已存在同名文件/文件夹');
        return;
    }
    node.children[clean] = {
        type: 'folder',
        children: {}
    };
    renderFileList();
    renderSidebar();
}

function handleOpenTab() {
    const folderName = contextMenuTarget;
    if (!folderName) return;
    const currentPath = getActivePath();
    let targetPath;
    if (isMappingMode) {
        if (currentPath.length === 1 && currentPath[0] === 'assets') {
            targetPath = ['assets', folderName];
        } else if (currentPath.length === 2) {
            targetPath = ['assets', currentPath[1], folderName];
        } else {
            targetPath = [...currentPath, folderName];
        }
    } else {
        targetPath = [...currentPath, folderName];
    }
    const node = getNodeByPath(targetPath);
    if (node && node.type === 'folder') {
        createTab(targetPath, true);
    } else {
        alert('无法在新标签页打开该路径');
    }
}

// ================================================================
//  8. 全量渲染
// ================================================================

function renderAllForActiveTab() {
    console.log('[renderAllForActiveTab] Rendering for active tab');
    renderSidebar();
    renderPath();
    renderFileList();
    updateNavButtonsForActiveTab();
}

// ================================================================
//  9. 模式切换
// ================================================================

document.getElementById('modeSwitch').addEventListener('change', function(e) {
    isMappingMode = this.checked;
    console.log(`[ModeSwitch] Mapping mode ${isMappingMode ? 'ENABLED' : 'DISABLED'}`);
    const tab = getActiveTab();
    if (tab) {
        // 重置路径到 assets
        tab.path = ['assets'];
        tab.history = [['assets']];
        tab.historyIndex = 0;
        console.log('[ModeSwitch] Reset active tab path to assets');
    }
    renderAllForActiveTab();
    updateNavButtonsForActiveTab();
});

// ================================================================
//  10. 初始化
// ================================================================

function init() {
    console.log('[init] Creating initial tab');
    createTab(['assets'], true);

    document.getElementById('btnBack').addEventListener('click', goBackActiveTab);
    document.getElementById('btnUp').addEventListener('click', goUpActiveTab);

    document.querySelectorAll('.view-buttons label').forEach(label => {
        label.addEventListener('click', function() {
            document.querySelectorAll('.view-buttons label').forEach(l => l.classList.remove('active'));
            this.classList.add('active');
        });
    });

    document.getElementById('fileListContainer').addEventListener('selectstart', function(e) {
        e.preventDefault();
    });

    document.addEventListener('keydown', function(e) {
        if (e.key === 'Backspace' && !e.target.closest('input, textarea, [contenteditable]')) {
            e.preventDefault();
            goUpActiveTab();
        }
        if (e.key === 'ArrowLeft' && (e.ctrlKey || e.metaKey)) {
            e.preventDefault();
            goBackActiveTab();
        }
        if (e.key === 'Escape') {
            closeContextMenu();
        }
    });

    document.addEventListener('click', function(e) {
        if (!e.target.closest('.context-menu')) {
            closeContextMenu();
        }
    });

    console.log('📁 Unity 项目浏览器已启动，根目录: ./assets');
    console.log('💡 打开“映射模式”可查看场景虚拟文件夹');
}

document.addEventListener('DOMContentLoaded', init);