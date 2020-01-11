
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
    }

    get isGranted() {
        let retValue = Notification.permission === 'granted';
        return retValue;
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
            this.dictOfSubEvents[subEvent.Id] = subEvent;
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
        let currentTimeInMS = Date.now();
        let subEventStart = subEvent.SubCalStartDate.getTime();
        let notificationStart = (subEventStart - TenMinInMs) - currentTimeInMS;
        if(notificationStart > 0) {
            let durationString = moment.duration(TenMinInMs, 'milliseconds').humanize();
            this.nextSubEventNotification.subEvent = subEvent;
            this.nextSubEventNotification.time = currentTimeInMS + notificationStart;
            let currentTimeOut = setTimeout(() => {
                let notificationTitle = subEvent.Name + " starts in " + durationString;
                let notification = new Notification(notificationTitle);
                this.dispachedNotifications.push(notification);
                this.processListOfSubEvents();

            }, notificationStart);
            this.activeTimers.push(currentTimeOut);
            return true;
        }
        return false;
    }

    authenticateNotification() {
        let permissionType = {
            DENIED: "denied",
            GRANTED: "granted"
        };
        if (Notification.permission !== permissionType.DENIED) 
        {
            let initialStatus = Notification.permission;
            Notification.requestPermission().then(function (permission) {
              // If the user accepts, let's create a notification
              if (permission === permissionType.GRANTED && permissionType.GRANTED !== initialStatus) {
                var notification = new Notification("Thanks for enabling Tiler notifications! Now let's optimize the future");
              }
            });
        }
    }
}