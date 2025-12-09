// InteractionSurface Initialize
window.surfaceManagers = new Map();

window.initializeSurfaceManager = (surfaceElement) => {
    if (!surfaceElement) return;

    window.surfaceManagers.set(surfaceElement, new InteractionSurfaceManager(surfaceElement)); // ←必要なら保持
};

// SetScrollArea　※子をSet
window.setScrollArea = (surfaceElement, scrollAreaElement) => {
    const manager = window.surfaceManagers.get(surfaceElement);
    if (manager) {
        manager.setScrollArea(scrollAreaElement);
    }
};

window.getScrollAreaBounds = (surfaceElement) => {
    const surfaceManager = window.surfaceManagers.get(surfaceElement);
    if (!surfaceManager || !surfaceManager.scrollArea) return null;

    const scrollManager = window.scrollManagers.get(surfaceManager.scrollArea);
    if (!scrollManager) return null;

    return scrollManager.getBounds();
};

class InteractionSurfaceManager {
    constructor(surfaceElement) {
        this.id = surfaceElement.id;
        this.surface = surfaceElement;
        this.surfaceRect = surfaceElement.getBoundingClientRect();
        this.scrollArea = null;
        this.pageOffset = {
            scrollLeft: Math.round(window.scrollX),
            scrollTop: Math.round(window.scrollY)
        };
    }

    setScrollArea(scrollAreaElement) {
        this.scrollArea = scrollAreaElement;
    }

    updateSurfaceRect() {
        this.surfaceRect = this.surface.getBoundingClientRect();
    }

    getRelativeBounds(childElement) {
        if (!childElement) return { xmin: 0, xmax: 0, ymin: 0, ymax: 0 };
        const childRect = childElement.getBoundingClientRect();
        return {
            xmin: Math.round(childRect.left - this.surfaceRect.left + window.scrollX),
            xmax: Math.round(childRect.right - this.surfaceRect.left + window.scrollX),
            ymin: Math.round(childRect.top - this.surfaceRect.top + window.scrollY),
            ymax: Math.round(childRect.bottom - this.surfaceRect.top + window.scrollY)
        };
    }

    getRelativePosition(childElement) {
        if (!childElement) return { x: 0, y: 0 };
        const childRect = childElement.getBoundingClientRect();
        return {
            x: Math.round(childRect.left - this.surfaceRect.left),
            y: Math.round(childRect.top - this.surfaceRect.top)
        };
    }

    getCorrectedBounds(childElement, scrollAreaElement) {
        const bounds = this.getRelativeBounds(childElement);
        const manager = window.scrollManagers.get(scrollAreaElement);
        const scrollOffset = manager?.getState() ?? { scrollTop: 0, scrollLeft: 0 };

        return {
            XMin: bounds.xmin + scrollOffset.scrollLeft,
            XMax: bounds.xmax + scrollOffset.scrollLeft,
            YMin: bounds.ymin + scrollOffset.scrollTop,
            YMax: bounds.ymax + scrollOffset.scrollTop
        };
    }
}

window.getPageScrollOffset = () => {
    return window.windowScrollManager.getState(); // ← インスタンスから呼び出す
};



window.getRelativePositionFromManager = (surfaceElement) => {
    const manager = window.surfaceManagers.get(surfaceElement);
    if (!manager) return { x: 0, y: 0 };
    return manager.getRelativePosition(manager.scrollArea);
};

window.getScrollOffsetFromManager = (surfaceElement) => {
    const surfaceManager = window.surfaceManagers.get(surfaceElement);
    if (!surfaceManager || !surfaceManager.scrollArea) {
        return { scrollTop: 0, scrollLeft: 0 };
    }

    const scrollAreaElement = surfaceManager.scrollArea;
    const scrollManager = window.scrollManagers.get(scrollAreaElement);

    if (!scrollManager) {
        return { scrollTop: 0, scrollLeft: 0 };
    }

    return scrollManager.getState();
};

window.getCorrectedBoundsByIdAndRef = (surfaceId, buttonElement) => {
    if (!surfaceId || !buttonElement) return null;

    // Manager を SurfaceId から直接探す
    const manager = [...window.surfaceManagers.values()].find(m => m.id === surfaceId);
    if (!manager) return null;

    return manager.getCorrectedBounds(buttonElement, manager.scrollArea);
};


// ScrollAreaの状態の取得(), AutoScroll機能付き
class ScrollManager {
    constructor(scrollAreaElement) {
        this.scrollArea = scrollAreaElement;
        this.scrollTop = 0;
        this.scrollLeft = 0;
        this.bounds = this.scrollArea.getBoundingClientRect();
        this.scrollWidth = this.scrollArea.scrollWidth;
        this.scrollHeight = this.scrollArea.scrollHeight;
        this.isDragging = false;
        this.ticking = false;
        this.directionX = DirectionX.None;
        this.directionY = DirectionY.None;
        this.mode = 'manual'; // 'manual', 'locked', 'auto' など拡張可能
        this.lastClientX = 0;
        this.lastClientY = 0;
        this.edgeSize = 100;
        this.CurrentSpeed = 20;
        this.MaxSpeed = 50;

        this.init();
    }

    init() {
        if (!this.scrollArea) return;

        // スクロール位置を常に監視
        this.scrollArea.addEventListener('scroll', () => {
            this.scrollTop = this.scrollArea.scrollTop;
            this.scrollLeft = this.scrollArea.scrollLeft;
            this.scrollWidth = this.scrollArea.scrollWidth;
            this.scrollHeight = this.scrollArea.scrollHeight;
        });

/*        document.addEventListener('mousedown', this.startState.bind(this));*/
        document.addEventListener('mouseup', this.resetState.bind(this));

        // Scroll状態の更新
        document.addEventListener('mousemove', (e) => {
            if (!this.isDragging) return;

            // 直近座標
            this.lastClientX = e.clientX;
            this.lastClientY = e.clientY;

            // 方向更新
            this.directionX = e.movementX > 0 ? DirectionX.RIGHT :
                e.movementX < 0 ? DirectionX.LEFT : DirectionX.None;

            this.directionY = e.movementY > 0 ? DirectionY.DOWN :
                e.movementY < 0 ? DirectionY.UP : DirectionY.None;
        });
    }

    // AutoScroll
    runLoop() {
        const step = () => {
            if (!this.isDragging) return;

            const eX = this.lastClientX;
            const eY = this.lastClientY;

            // Y方向スクロール
            if (eY < this.bounds.top + this.edgeSize &&
                this.directionY === DirectionY.UP) {
                const dist = (this.bounds.top + this.edgeSize) - eY;
                const speed = this.calculateSpeed(dist);
                this.scrollArea.scrollTop -= speed;
            } else if (eY > this.bounds.bottom - this.edgeSize &&
                this.directionY === DirectionY.DOWN) {
                const dist = eY - (this.bounds.bottom - this.edgeSize);
                const speed = this.calculateSpeed(dist);
                this.scrollArea.scrollTop += speed;
            }

            // X方向スクロール
            if (eX < this.bounds.left + this.edgeSize &&
                this.directionX === DirectionX.LEFT) {
                const dist = (this.bounds.left + this.edgeSize) - eX;
                const speed = this.calculateSpeed(dist);
                this.scrollArea.scrollLeft -= speed;
            } else if (eX > this.bounds.right - this.edgeSize &&
                this.directionX === DirectionX.RIGHT) {
                const dist = eX - (this.bounds.right - this.edgeSize);
                const speed = this.calculateSpeed(dist);
                this.scrollArea.scrollLeft += speed;
            }

            requestAnimationFrame(step);
        }
        requestAnimationFrame(step);
    }

    // AutoScroll開始
    startState(e) {
        this.isDragging = true;
        this.lastClientX = e.clientX;
        this.lastClientY = e.clientY;
        this.bounds = this.scrollArea.getBoundingClientRect(); 
        this.runLoop(); // rAFループ開始
    }

    resetState() {
        this.isDragging = false;
        this.directionX = DirectionX.None;
        this.directionY = DirectionY.None;
    }

    // 速度計算　端に端に近づくと線形で加速
    calculateSpeed(distanceFromEdge) {
        if (distanceFromEdge <= 0) return 0;
        if (distanceFromEdge >= this.edgeSize) return this.CurrentSpeed;

        // 線形補間
        const ratio = 1 - (distanceFromEdge / this.edgeSize);
        return this.CurrentSpeed + (this.MaxSpeed - this.CurrentSpeed) * ratio;
    }

    setMode(mode) {
        this.mode = mode;
    }

    getState() {
        return {
            scrollTop: Math.round(this.scrollTop),
            scrollLeft: Math.round(this.scrollLeft)
        };
    }

    getBounds() {
        return {
            xmin: Math.round(this.bounds.left),
            ymin: Math.round(this.bounds.top),
            xmax: Math.round(this.bounds.right),
            ymax: Math.round(this.bounds.bottom)
        };
    }

    getScrollBounds() {
        return {
            xmin: Math.round(this.bounds.left),
            ymin: Math.round(this.bounds.top),
            xmax: Math.round(this.bounds.left + this.scrollWidth),
            ymax: Math.round(this.bounds.height + this.scrollHeight)
        };
    }

    resetState() {
        // ドラッグ状態解除
        this.isDragging = false;

        // スクロール方向リセット
        this.directionX = DirectionX.None;
        this.directionY = DirectionY.None;

        // requestAnimationFrame の制御フラグ
        this.ticking = false;

        // スクロール位置の同期
        this.scrollTop = this.scrollArea.scrollTop;
        this.scrollLeft = this.scrollArea.scrollLeft;
    }
}

// ScrollManager Initialize
window.scrollManagers = new Map();

window.initializeScrollManager = (scrollAreaElement) => {
    if (!scrollAreaElement) {
        return;
    }
    window.scrollManagers.set(scrollAreaElement, new ScrollManager(scrollAreaElement));
};

window.GetScrollOffsetAsync = (scrollAreaElement) => {
    if (!scrollAreaElement) {
        return null;
    }

    const manager = window.scrollManagers.get(scrollAreaElement);
    if (!manager) {
        return null;
    }
    return manager.getState();
};


window.getClientPosition = (element) => {
    if (!element) return { x: 0, y: 0 };
    const rect = element.getBoundingClientRect();
    return {
        X: Math.round(rect.left + window.scrollX),
        Y: Math.round(rect.top + window.scrollY)
    };
};

const DirectionX = {
    None: 'none',
    LEFT: 'left',
    RIGHT: 'right'
};

const DirectionY = {
    None: 'none',
    UP: 'up',
    DOWN: 'down'
};


