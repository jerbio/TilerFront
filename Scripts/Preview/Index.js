class Preview {

    constructor(eventId, domContainer) {
        this.UIContainer = domContainer;
        this.processedDay = {};
        this.previewDays = [];
        this.isLoading = false;
        this.previewEventId = eventId;
        this.currentRequests = [];
    }

    _beforePreviewRequest () {
        if(Array.isArray(this.currentRequests) && this.currentRequests.length > 0) {
            let previousRequest = this.currentRequests.shift();
            previousRequest.abort();
        }
        
    }

    _afterPreveiwRequestCompletes () {
        this.currentRequests.pop();
    }

    setAsNow(subEventId) {

    }

    showError(response) {

    }

    editSubEvent() {
        this._beforePreviewRequest();
        let Url = global_refTIlerUrl + "WhatIf/SubEventEdit";
        let currentSubevent = Dictionary_OfSubEvents[this.previewEventId];
        let postData = getSubeventUpdateData(currentSubevent);
        postData.TimeZone = moment.tz.guess();
        preSendRequestWithLocation(postData);


        this._beforePreviewRequest();
        this.show();
        this.startLoading();

        let request = $.ajax({
            type: "POST",
            url: Url,
            data: postData,
            // DO NOT SET CONTENT TYPE to json
            // contentType: "application/json; charset=utf-8", 
            // DataType needs to stay, otherwise the response object
            // will be treated as a single string
            //dataType: "json",
            success: (response) => {
                let previewDays = PreviewDay.convertPreviewResponseToPreviewDays(response.Content);
                this.processPreviewDays(previewDays);    
                this.show();
                this.endLoading();
            },
            error: (err) => {
                this.showError();
                this.endLoading();
            }

        }).done(
            () => {
                this._afterPreveiwRequestCompletes();
            }
        );
    }

    procrastinateEvent() {
        this._beforePreviewRequest();
        let Url = global_refTIlerUrl + "WhatIf/PushedEvent";
        let currentSubevent = Dictionary_OfSubEvents[this.previewEventId];
        let postData = getProcrastinateSingleEventData(currentSubevent);
        postData.TimeZone = moment.tz.guess();
        preSendRequestWithLocation(postData);


        this._beforePreviewRequest();
        this.show();
        this.startLoading();
        let endLoading = this.endLoading;

        let request = $.ajax({
            type: "POST",
            url: Url,
            data: postData,
            // DO NOT SET CONTENT TYPE to json
            // contentType: "application/json; charset=utf-8", 
            // DataType needs to stay, otherwise the response object
            // will be treated as a single string
            //dataType: "json",
            success: (response) => {
                //PreviewDay.processPreviewRequest(response.Content);
                let previewDays = PreviewDay.convertPreviewResponseToPreviewDays(response.Content);
                this.processPreviewDays(previewDays);    
                this.show();
                var myContainer = (response);
                if (myContainer.Error.code == 0) {
                    //exitSelectedEventScreen();
                }
                else {
                    var NewMessage = myContainer.Error && myContainer.Error.code && myContainer.Error.Message ? myContainer.Error.Message : "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                    var ExitAfter = {
                        ExitNow: true, Delay: 5000
                    };
                }
                this.endLoading();
            },
            error: (err) => {
                var myError = err;
                var step = "err";
                var NewMessage = err.Error && err.Error.code && err.Error.Message ? err.Error.Message : "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                var ExitAfter = {
                    ExitNow: true, Delay: 1000
                };
                this.endLoading();
            }

        }).done(
            () => {
                this._afterPreveiwRequestCompletes();
            }
        );
    }

    procrastinateAll() {
        this._beforePreviewRequest();
        let Url = global_refTIlerUrl + "WhatIf/PushedAll";
        let currentSubevent = Dictionary_OfSubEvents[this.previewEventId];
        let postData = getProcrastinateAllData(currentSubevent);
        postData.TimeZone = moment.tz.guess();
        preSendRequestWithLocation(postData);


        this._beforePreviewRequest();
        this.show();
        this.startLoading();
        let endLoading = this.endLoading;

        let request = $.ajax({
            type: "POST",
            url: Url,
            data: postData,
            // DO NOT SET CONTENT TYPE to json
            // contentType: "application/json; charset=utf-8", 
            // DataType needs to stay, otherwise the response object
            // will be treated as a single string
            //dataType: "json",
            success: (response) => {
                //PreviewDay.processPreviewRequest(response.Content);
                let previewDays = PreviewDay.convertPreviewResponseToPreviewDays(response.Content);
                this.processPreviewDays(previewDays);    
                this.show();
                var myContainer = (response);
                if (myContainer.Error.code == 0) {
                    //exitSelectedEventScreen();
                }
                else {
                    var NewMessage = myContainer.Error && myContainer.Error.code && myContainer.Error.Message ? myContainer.Error.Message : "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                    var ExitAfter = {
                        ExitNow: true, Delay: 5000
                    };
                }
                this.endLoading();
            },
            error: (err) => {
                var myError = err;
                var step = "err";
                var NewMessage = err.Error && err.Error.code && err.Error.Message ? err.Error.Message : "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                var ExitAfter = {
                    ExitNow: true, Delay: 1000
                };
                this.endLoading();
            }

        }).done(
            () => {
                this._afterPreveiwRequestCompletes();
            }
        );
    }

    bestPossibleDay(subEventId) {

    }

    generateRandomPreviewDays() {
        let today = new PreviewDay();
        today.start = (new Date().setHours(0,0,0,0));
        let tomorrow  = new PreviewDay();
        tomorrow.start = today.start + OneDayInMs
        let dafAfterTomorrow  = new PreviewDay();
        dafAfterTomorrow.start = tomorrow.start + OneDayInMs
        let devDays = [
            today, tomorrow, dafAfterTomorrow
        ];
        devDays.forEach((devDay) => {
            devDay.randomizeDev();
        });

        return devDays;
    }

    startLoading() {
        this.isLoading = true;
        let loadingBarId = "PreviewLoading";
        let loadingBar = getDomOrCreateNew(loadingBarId);
        $(loadingBar).addClass("active")
    }

    endLoading() {
        this.isLoading = false;
        let loadingBarId = "PreviewLoading";
        let loadingBar = getDomOrCreateNew(loadingBarId);
        $(loadingBar).removeClass("active")
    }

    show() {
        $(this.UIContainer.parentNode).addClass('active');// this will be unnecessary. The goal is what ever container is provided as UIContainer, will append our generated preview node as the child node. If there is no container then this defaults to sliding out of the subevent infomration.
    }

    sleepRendering(previewDay) {
        if(previewDay.sleep) {
            
            let sleepData = this.sleepData;
            if(!sleepData) {
                this.sleepData = {};
                sleepData = this.sleepData;
            }
            let dayStart = previewDay.start;
            let daySleepInfo = sleepData[dayStart];
            if (!daySleepInfo) {
                daySleepInfo = {
                    id: globalCounter()
                };
                sleepData[dayStart] = daySleepInfo;
                let sleepContainerDomId = "day-preview-sleep-container-" + daySleepInfo.id;
                let sleepContainer = getDomOrCreateNew(sleepContainerDomId);
                daySleepInfo.container = sleepContainer;
                $(daySleepInfo.container).addClass("day-preview-sleep-container");

                let sleepColorContainerDomId = "day-preview-sleep-color-container-" + daySleepInfo.id;
                let sleepColorContainerDom = getDomOrCreateNew(sleepColorContainerDomId);
                daySleepInfo.colorContainer = sleepColorContainerDom;
                $(daySleepInfo.colorContainer).addClass("day-preview-sleep-color-container");

                let sleepTextContainerDomId = "day-preview-sleep-text-container-" + daySleepInfo.id;
                let sleepTextContainerDom = getDomOrCreateNew(sleepTextContainerDomId);
                daySleepInfo.TextContainer = sleepTextContainerDom;
                $(daySleepInfo.TextContainer).addClass("day-preview-sleep-text-container");

                let sleepTextDomId = "day-preview-sleep-text-" + daySleepInfo.id;
                let sleepTextDom = getDomOrCreateNew(sleepTextDomId, "span");
                daySleepInfo.TextDom = sleepTextDom;
                $(daySleepInfo.TextDom).addClass("day-preview-sleep-text");
                
                daySleepInfo.TextContainer.Dom.appendChild(daySleepInfo.TextDom);
                daySleepInfo.container.Dom.appendChild(daySleepInfo.colorContainer.Dom);
                daySleepInfo.container.Dom.appendChild(daySleepInfo.TextContainer.Dom);
            }

            daySleepInfo.TextDom.innerHTML = moment.duration(previewDay.sleep.duration, 'milliseconds').humanize();
            
            let retValue = {
                dom: daySleepInfo.container.Dom,
                info: daySleepInfo
            }

            return retValue;
        } else {
            return {
                dom: null,
                info: null
            }
        }
    }

    conflictRendering(previewDay) {
        let dayStart = previewDay.start;
        let conflictData = this.conflictData;
        if(!conflictData) {
            conflictData = {}
            this.conflictData = conflictData;
        }

        let dayConflictInfo = conflictData[dayStart];

        if(!dayConflictInfo) {
            dayConflictInfo = {
                id: globalCounter()
            };

            let dayConflictInfoContainerId = "day-preview-conflict-container-"+dayConflictInfo.id;
            let dayConflictInfoContainer = getDomOrCreateNew(dayConflictInfoContainerId);
            dayConflictInfo.conflictContainer = dayConflictInfoContainer;
            $(dayConflictInfo.conflictContainer).addClass("day-preview-conflict-container");

            let dayConflictInfoIconContainerId = "day-preview-conflict-icon-container-"+dayConflictInfo.id;
            let dayConflictInfoIconContainer = getDomOrCreateNew(dayConflictInfoIconContainerId);
            dayConflictInfo.iconContainer = dayConflictInfoIconContainer;
            $(dayConflictInfo.iconContainer).addClass("day-preview-conflict-icon-container");

            let dayConflictInfoTextContainerId = "day-preview-conflict-text-container-"+dayConflictInfo.id;
            let dayConflictInfoTextContainer = getDomOrCreateNew(dayConflictInfoTextContainerId);
            dayConflictInfo.textContainer = dayConflictInfoTextContainer;
            $(dayConflictInfo.textContainer).addClass("day-preview-conflict-text-container");

            let dayConflictInfoTextId = "day-preview-conflict-text-"+dayConflictInfo.id;
            let dayConflictInfoText = getDomOrCreateNew(dayConflictInfoTextId, "span");
            dayConflictInfo.textDom = dayConflictInfoText;
            $(dayConflictInfo.textDom).addClass("day-preview-conflict-text");

            dayConflictInfoTextContainer.Dom.appendChild(dayConflictInfoText);
            dayConflictInfoContainer.Dom.appendChild(dayConflictInfoIconContainer);
            dayConflictInfoContainer.Dom.appendChild(dayConflictInfoTextContainer);

            conflictData[dayStart] = dayConflictInfo;
        }

        if(previewDay.conflict && previewDay.conflict.length > 0) {
            dayConflictInfo.textDom.innerHTML = previewDay.conflict.length + " conflict" + (previewDay.conflict.length>1 ? "s" :"");
        } else {
            dayConflictInfo.textDom.innerHTML = "";
        }
        let retValue = {
            dom: dayConflictInfo.conflictContainer
        }

        return retValue;
        
    }

    tardyRendering(previewDay) {
        let dayStart = previewDay.start;
        let tardyData = this.tardyData;
        if(!tardyData) {
            tardyData = {}
            this.tardyData = tardyData;
        }

        let dayTardyInfo = tardyData[dayStart];

        if(!dayTardyInfo) {
            dayTardyInfo = {
                id: globalCounter()
            };

            let dayTardyInfoContainerId = "day-preview-tardy-container-"+dayTardyInfo.id;
            let dayTardyInfoContainer = getDomOrCreateNew(dayTardyInfoContainerId);
            dayTardyInfo.tardyContainer = dayTardyInfoContainer;
            $(dayTardyInfo.tardyContainer).addClass("day-preview-tardy-container");

            let dayTardyInfoIconContainerId = "day-preview-tardy-icon-container-"+dayTardyInfo.id;
            let dayTardyInfoIconContainer = getDomOrCreateNew(dayTardyInfoIconContainerId);
            dayTardyInfo.iconContainer = dayTardyInfoIconContainer;
            $(dayTardyInfo.iconContainer).addClass("day-preview-tardy-icon-container");

            let dayTardyInfoTextContainerId = "day-preview-tardy-text-container-"+dayTardyInfo.id;
            let dayTardyInfoTextContainer = getDomOrCreateNew(dayTardyInfoTextContainerId);
            dayTardyInfo.textContainer = dayTardyInfoTextContainer;
            $(dayTardyInfo.textContainer).addClass("day-preview-tardy-text-container");

            let dayTardyInfoTextId = "day-preview-tardy-text-"+dayTardyInfo.id;
            let dayTardyInfoText = getDomOrCreateNew(dayTardyInfoTextId, "span");
            dayTardyInfo.textDom = dayTardyInfoText;
            $(dayTardyInfo.textDom).addClass("day-preview-tardy-text");

            dayTardyInfoTextContainer.Dom.appendChild(dayTardyInfoText);
            dayTardyInfoContainer.Dom.appendChild(dayTardyInfoIconContainer);
            dayTardyInfoContainer.Dom.appendChild(dayTardyInfoTextContainer);

            tardyData[dayStart] = dayTardyInfo;
        }

        if(previewDay.tardy && previewDay.tardy.length > 0) {
            dayTardyInfo.textDom.innerHTML = "Late to " + previewDay.tardy.length + " event" + (previewDay.tardy.length>1 ? "s" :"");
        } else {
            dayTardyInfo.textDom.innerHTML = "On Time";
        }
        let retValue = {
            dom: dayTardyInfo.tardyContainer
        }

        return retValue;
    }

    generateDayDom(previewDay) {
        let dayStart = Number(previewDay.start);
        let dayInfo = this.processedDay[dayStart];
        if(!dayInfo) {
            dayInfo = {
                id: globalCounter(),
            };

            let dayContainerId = "preview-day-whole-container-"+dayInfo.id;
            let dayContainer = getDomOrCreateNew(dayContainerId);
            dayInfo.dayContainer = dayContainer;
            $(dayInfo.dayContainer).addClass("preview-day-whole-container");

            let dayOfWeekContainerId = "preview-day-of-week-"+dayInfo.id;
            let dayOfWeekContainer = getDomOrCreateNew(dayOfWeekContainerId);
            dayInfo.dayOfWeekContainer = dayOfWeekContainer;
            $(dayInfo.dayOfWeekContainer).addClass("preview-day-of-week");

            let dayOfWeekTextContainerId = "preview-day-of-week-text-container-"+dayInfo.id;
            let dayOfWeekTextContainer = getDomOrCreateNew(dayOfWeekTextContainerId);
            dayInfo.dayOfWeekTextContainer = dayOfWeekTextContainer;
            $(dayInfo.dayOfWeekTextContainer).addClass("preview-day-of-week-text-container");

            let dayOfWeekTextId = "preview-day-of-week-text-"+dayInfo.id;
            let dayOfWeekText = getDomOrCreateNew(dayOfWeekTextId, 'span');
            $(dayInfo.dayOfWeekTextContainer).addClass("preview-day-of-week-text");
            dayOfWeekText.innerHTML = WeekDays[new Date(dayStart).getDay()]
            


            dayInfo.dayOfWeekText = dayOfWeekText;
            dayInfo.dayOfWeekTextContainer.Dom.appendChild(dayInfo.dayOfWeekText);
            dayInfo.dayOfWeekContainer.Dom.appendChild(dayInfo.dayOfWeekTextContainer);
            dayInfo.dayContainer.Dom.appendChild(dayInfo.dayOfWeekContainer);
            

            let dayDomSleepWrapperId = "day-preview-whole-sleep-wrapper-"+dayInfo.id;
            let dayDomSleepWrapper = getDomOrCreateNew(dayDomSleepWrapperId);
            dayInfo.sleepDayDom = dayDomSleepWrapper;

            dayInfo.dayContainer.Dom.appendChild(dayInfo.sleepDayDom.Dom);

            this.processedDay[dayStart] = dayInfo;
        }

        let sleepInfo = this.sleepRendering(previewDay);
        let conflictInfo = this.conflictRendering(previewDay);
        let tardyInfo = this.tardyRendering(previewDay);
        if(sleepInfo.dom) {
            dayInfo.sleepDayDom.Dom.appendChild(sleepInfo.dom);
        }
        dayInfo.sleepDayDom.Dom.appendChild(conflictInfo.dom);
        dayInfo.sleepDayDom.Dom.appendChild(tardyInfo.dom);
        return dayInfo.dayContainer.Dom;
    }


    processPreviewDays(previewDays) {
        if(previewDays && Array.isArray(previewDays) && previewDays.length > 0) {
            let dayDoms = [];
            for (let i=0; i< previewDays.length; i++) {
                let previewDay = previewDays[i];
                let dayDom = this.generateDayDom(previewDay);
                dayDoms.push(dayDom);
                this.UIContainer.appendChild(dayDom);
            }
        }
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
        $(sleepDom).addClass("day-preview-sleep-container");
        let retValue = {day: day, dom: sleepDom};

        return retValue;
    }

    processTardy(day) {
        let dayId = day.Start
        let lateDomId = "day-preview-late-container-" + dayId;
        let lateDom = getDomOrCreateNew(lateDomId);
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