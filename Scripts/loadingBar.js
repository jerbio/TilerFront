class LoadingBar {
    constructor (groupingId) {
        this.parentNode = null;
        this.container = null;
        this.slider = null;
        this.generateLoadingBar();
        let groupId = LoadingBar.defaultGrouping();
        
        if(groupingId) {
            groupId = groupingId;
        }

        let groupings = LoadingBar.groupings();
        let loadingBars = groupings[groupId];
        if(!loadingBars) {
            groupings[groupId] = [];
            loadingBars = groupings[groupId];
        }
        loadingBars.push(this);
        this.hide();
    }

    generateLoadingBar() {
        let counter = LoadingBar.domCounter();
        let loadingContainerId = "loading-bar-container-"+counter;
        let loadingContainer = getDomOrCreateNew(loadingContainerId);
        this.container = loadingContainer;
        $(loadingContainer).addClass("loading-bar-container");

        let slidingingId = "loading-bar-slide-"+counter;
        let slider = getDomOrCreateNew(slidingingId);
        $(slider).addClass("loading-bar-slide");
        this.slider = slider;

        this.container.appendChild(this.slider);
    }

    show() {
        $(this.container).removeClass("setAsDisplayNone");
    }

    hide() {
        $(this.container).addClass("setAsDisplayNone");
    }

    embed(container) {
        if(container) {
            this.parentNode = container;
            this.parentNode.appendChild(this.container);
        }
        
    }

    static domCounter() {
        if(LoadingBar.domCounter.counter === undefined) {
            LoadingBar.domCounter.counter = 0;
        } else {
            ++LoadingBar.domCounter.counter;
        }

        return LoadingBar.domCounter.counter;
    }

    static defaultGrouping() {
        let uuid;
        if(LoadingBar.domCounter.uuid === undefined) {
            uuid = generateUUID();
            LoadingBar.defaultGrouping.uuid = uuid;
        } else {
            uuid = LoadingBar.defaultGrouping.uuid;
        }
        return uuid;
    }

    static groupings() {
        let retValue;
        if(LoadingBar.groupings.dict === undefined) {
            LoadingBar.groupings.dict = {};
            retValue = LoadingBar.groupings.dict;
        } else{
            retValue = LoadingBar.groupings.dict;
        }
        return retValue;
    }

    static showAllGroupings(groupId) {
        let groupingDict = LoadingBar.groupings();
        if(!groupId) {
            for(let groupIdKey in groupingDict) {
                let grouping = groupingDict[groupIdKey];
                for(let i=0; i< grouping.length; i++) {
                    let loadingBar = grouping[i];
                    loadingBar.show();
                }
            }
        } else {
            let grouping = groupingDict[groupId];
            if(grouping) {
                for(let i=0; i< grouping.length; i++) {
                    let loadingBar = grouping[i];
                    loadingBar.show();
                }
            }
            
        }
    }

    static hideAllGroupings(groupId) {
        let groupingDict = LoadingBar.groupings();
        if(!groupId) {
            for(let groupIdKey in groupingDict) {
                let grouping = groupingDict[groupIdKey];
                for(let i=0; i< grouping.length; i++) {
                    let loadingBar = grouping[i];
                    loadingBar.hide();
                }
            }
        } else {
            let grouping = groupingDict[groupId];
            if(grouping) {
                for(let i=0; i< grouping.length; i++) {
                    let loadingBar = grouping[i];
                    loadingBar.hide();
                }
            }
        }
    }
}