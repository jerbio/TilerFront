
let notificationTypes = {
    START: "start"
};

class PageNotification {
    constructor(userId, notificationType=notificationTypes.START) {
        this.type = notificationType;
        this.dictOfSubEvents = {};
        this.subEventList = [];
        this.dispachedNotifications = [];
        this.userId = userId;
        this.activeTimers = [];
        this.nextSubEventNotification = {
            subEvent: null,
            time: null
        };
        this.isEnabled = true;
    }

    get isGranted() {
        let retValue = false;
        try {
            retValue = Notification.permission === 'granted';
            return retValue;
        } catch (e) {
            retValue = false;
        } finally {
            retValue = false;
        }
        
    }

    get isCapable() {
        let retValue = ('Notification' in window);
        return retValue;
    }

    resetAllNotifications() {
        this.dictOfSubEvents = {};
        this.subEventList = [];
        this.activeTimers.forEach((timeOutHandler) => {
            clearTimeout(timeOutHandler);
        });
        this.nextSubEventNotification = {
            subEvent: null,
            time: null
        };
    }

    processNotifications(subEvents) {
        this.resetAllNotifications();
        if(this.isCapable && this.isGranted) {
            this.subEventList = subEvents;
            this.processListOfSubEvents();
        }
    }

    processListOfSubEvents() {
        let currentTime = Date.now();
        let subEventsAfterNow = this.subEventList.filter((subEvent) => subEvent.Start > currentTime );
        subEventsAfterNow.sort((subEventA, subEventB) => { return subEventA.Start - subEventB.Start;});
        subEventsAfterNow.forEach((subEvent) => {
            this.dictOfSubEvents[subEvent.ID] = subEvent;
        });
        if(subEventsAfterNow.length > 0) {
            for(let i =0; i < subEventsAfterNow.length; i++) {
                let successDispatch = this.dispatchNotification(subEventsAfterNow[i]);
                if(successDispatch) {
                    break;
                }
            }
        }
    }

    dispatchNotification(subEvent) {
        let checkIfIsStillNext = (subEvent) => {
            this.dispatchNotification.isWaitingOnTimeVerification = true;
            let callBackAfterRefresh = () => {
                let postRefreshSubEvent = this.dictOfSubEvents[subEvent.ID];
                if (postRefreshSubEvent) {
                    let subEventStart = subEvent.SubCalStartDate.getTime();
                    let postRefreshStart = postRefreshSubEvent.SubCalStartDate.getTime();
                    if (postRefreshStart == subEventStart) {
                        if (this.isGranted) {
                            let durationString = moment.duration(TenMinInMs, 'milliseconds').humanize();
                            let notificationTitle = subEvent.Name + " starts in " + durationString;
                            let notification = new Notification(notificationTitle);
                            this.dispachedNotifications.push(notification);
                        }
                    }
                }
                this.dispatchNotification.isWaitingOnTimeVerification = false;
                this.processListOfSubEvents();
            };
            getRefreshedData(callBackAfterRefresh);


        };

        let currentTimeInMS = Date.now();
        let subEventStart = subEvent.SubCalStartDate.getTime();
        let notificationStart = (subEventStart - TenMinInMs) - currentTimeInMS;
        if(notificationStart > 0 && notificationStart < OneWeekInMs) {
            this.nextSubEventNotification.subEvent = subEvent;
            this.nextSubEventNotification.time = currentTimeInMS + notificationStart;
            if(!this.dispatchNotification.isWaitingOnTimeVerification) {
                let currentTimeOut = setTimeout(() => {
                    checkIfIsStillNext(subEvent);
                }, notificationStart);
                this.activeTimers.push(currentTimeOut);
            }
            return true;
        }
        return false;
    }

    

    authenticateNotification() {
        let permissionType = {
            DENIED: "denied",
            GRANTED: "granted"
        };
        try {
            if (Notification && Notification.permission !== permissionType.DENIED) {
                let initialStatus = Notification.permission;
                Notification.requestPermission().then(function (permission) {
                    // If the user accepts, let's create a notification
                    if (permission === permissionType.GRANTED && permissionType.GRANTED !== initialStatus) {
                        var notification = new Notification("Thanks for enabling Tiler notifications! Now let's optimize the future");
                    }
                });
            }
        } catch (e){
            console.log(e);
        }
        
    }
}