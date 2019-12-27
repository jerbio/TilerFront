class Preview {
    constructor(domContainer) {
        this.UIContainer = domContainer
    }

    setAsNow(subEventId) {

    }

    procrastinateAll(timeSpan) {

    }

    bestPossibleDay(subEventId) {

    }
}

class SetAsNowPreview {
    constructor(eventId, domContainer) {
        this.DayEvaluation = {}
    }

    sendRequest(eventId) {
        
    }

    processSleep(day) {
        let dayId = day.Start
        let sleepDomId = "day-preview-sleep-container-" + dayId;
        let sleepDom = getDomOrCreateNew(sleepDomId);
        let retValue = {day: day, dom: sleepDom};

        return retValue;
    }

    processTardy(day) {
        let dayId = day.Start
        let lateDomId = "day-preview-late-container-" + dayId;
        let lateDom = getDomOrCreateNew(lateDomId);
        let retValue = {day: day, dom: lateDom};
        let lateCountDomId= "day-preview-late-container-" + dayId;
        let lateCountDom = getDomOrCreateNew(lateCountDomId);
        lateCountDom.Dom.innerHTML = day.late.length;
        lateDom.appendChild(lateCountDom)
        let retValue = {day: day, dom: lateDom};
        return retValue;
    }

    processDayDom (day) {
        let dayId = day.Start
        let dayDomId = "day-preview-container-" + dayId;
        let dayDomContainer = getDomOrCreateNew(dayDomId)
        let sleepInfo = this.procesSleep(day);
        let daySleepWrapperId = "day-preview-sleep-wrapper-" + dayId;
        let daySleepWrapperDom = getDomOrCreateNew(daySleepWrapperId);
        daySleepWrapperDom.Dom.appendChild(sleepInfo.dom);

        let tardyInfo = this.processTardy(day);
        let dayTardyWrapperId = "day-preview-tardy-wrapper-" + dayId;
        let dayTardyWrapperDom = getDomOrCreateNew(dayTardyWrapperId);
        dayTardyWrapperDom.Dom.appendChild(tardyInfo.dom);


        dayDomContainer.Dom.appendChild(daySleepWrapperDom.Dom);
        dayDomContainer.Dom.appendChild(dayTardyWrapperDom.Dom);

        let retValue = {
            dom: dayDomContainer,
            sleep: sleepInfo
        }

        return retValue;
    }



    processWebRequest(data) {
        let days = []
        days.sort((day) => {return day.Start - day.Start})
        days.forEach((day) => {
            this.processDayDom(day)
        })
    }

    update(eventId) {

    }
}
class ProcrastinateAllPreview {
    constructor() {
        
    }

    update(timeSpan) {

    }
}