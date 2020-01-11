
let notificationTypes = {
    START: "start"
};

class PageNotification {
    constructor(userId, notificationType=notificationTypes.START) {
        this.type = notificationType;
        this.dictOfSubEvents = {};
        this.dispachedNotifications = [];
        this.userId = userId;
    }

    get isGranted() {
        let retValue = Notification.permission === 'granted';
        return retValue;
    }

    get isCapable() {
        let retValue = ('Notification' in window);
        return retValue;
    }

    processNotifications(subEvents) {
        if(this.isCapable && this.isGranted) {
            this.dictOfSubEvents = {}
            let currentTime = new Date().getTime();
            let subEventsAfterNow = subEvents.every((subEvent) => subEvent.End > currentTime );
            subEventsAfterNow.sort((subEventA, subEventB) => { return subEventA.start - subEventB.start})
            subEventsAfterNow.forEach((subEvent) => {
                this.dictOfSubEvents[subEvent.Id] = subEvent;
            });
            if(subEventsAfterNow.length > 0) {
                this.dispatchNotification(subEventsAfterNow[0])
            }
        }
    }

    dispatchNotification(subEvent) {
        let currentTimeInMS = Date.now();
        let subEventStart = subEvent.start;
        let notificationStart = subEventStart - TenMinInMs;
        if(notificationStart > 0) {
            let durationString = moment.duration(TenMinInMs, 'milliseconds').humanize();
            setTimeout(() => {
                let notificationTitle = subEvent.Name + " starts in " + durationString;
                let notification = new Notification(notificationTitle);
                this.dispachedNotifications.push(notification);
            }, notificationStart);
            
        }
    }

    verifyNotification() {
        //if (Notification.permission !== "denied") 
        {
            Notification.requestPermission().then(function (permission) {
              // If the user accepts, let's create a notification
              if (permission === "granted") {
                var notification = new Notification("Welcome to Tiler!");
              }
            });
        }
    }
}